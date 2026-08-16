# Feature Plan — 00015 Refinamento da modelagem Identity × OpenIddict × domínio

> Arquivo: `.ai/WORK/2026-08-16-00015-refinamento-modelagem-identity-openiddict.md`  
> Template: `.ai/TEMPLATES/feature-plan.md` + `migration.md`  
> Status: **Implementado** — D-00015-1..8 aceitas (**todas A**, 2026-08-16)  
> Data: 2026-08-16  
> Depende de: 00012 (FKs Guid explícitas), 00013 (ProductEnablement), ADR-001/002/003/006  
> Relaciona: 00007 (AuthClient sidecar), 00005 (sessão/`sid`), 00008 (claims tipadas)  
> Decisões: **A / A / A / A / A / A / A / A** (D-00015-1..8)  
> Migration prevista: `Phase18OpenIddictClientFks` + `Phase18bCompositeBranchOrgFks` + `Phase18cIdentityRoleCleanup` (nomes finais na implementação)

## Objetivo

Aproximar a estrutura persistida dos padrões **ASP.NET Identity** e **OpenIddict/OIDC**, eliminar duplicidades de responsabilidade e **fortalecer relacionamentos reais de negócio** (Organization, Branch, Product, User) com FKs/índices/constraints coerentes — sem inventar FKs artificiais e sem mover regras de domínio para dentro das tabelas dos frameworks.

Entregável desta fase: modelagem alinhada (OpenIddict FKs, FK composta Branch×Org, drop Identity roles, specs/docs).

## Contexto

A 00012 fechou FKs Guid entre aggregates Identity (`DeleteBehavior.Restrict` + navigations). Isso **não** integrou Identity/OpenIddict ao modelo de negócio:

- Sidecars OAuth (`AuthClientMetadata`, `ClientProductBinding`, `ClientWebhookEndpoint`, `UserSession`) ainda referenciam OpenIddict só por `ClientId` string (D-00012-3).
- `AspNetRoles` / `AspNetUserRoles` / `AspNetRoleClaims` existem e estão **vazias de uso** — authz vive em `AuthRoles` + `UserRoleAssignments` (ADR-002).
- Não há FK composta que garanta `Branch.OrganizationId` = `*.OrganizationId` nos assignments/sessões.
- Claims de contexto (`organization_id`, `branch_id`, `permissions`, `sso_c_*`) são **projetadas na emissão** (`TokenClaimsFactory`); os vínculos estruturais já estão (ou deveriam estar) no banco.
- Glossário ainda descreve Membership como “opcionalmente branch”; a entidade **não** tem `BranchId`.

Fontes da análise: entidades em `SSO.Core.Domain/Identity`, maps em `SSO.Infrastructures.Data/Identity/EntityMappings`, `IdentityDbContextModelSnapshot`, `AddIdentityConfigurations`, `TokenClaimsFactory`, ADRs 001–008, dicionário de dados.

---

## 1. Diagnóstico da modelagem atual

### O que já está correto

| Área | Estado |
|------|--------|
| ASP.NET Identity (conta) | `User : IdentityUser<Guid>` em `AspNetUsers`; logins externos, tokens 2FA, e-mail único — uso real |
| OpenIddict store | `UseOpenIddict<Guid>()` no `AddDbContext`; Applications / Authorizations / Scopes / Tokens no schema `IdentityDb` |
| FKs Guid de domínio | 00012 + 00013 + 00016 (contacts): Restrict, navigations inversas, testes `IdentityForeignKeyModelScenarios` |
| Hierarquia de negócio | Organization (tenant) → Branch; User ↔ Org via `Membership`; Org × Product via `ProductEnablement`; client_id → Product via `ClientProductBinding` |
| Soft-delete | `IdentityAuditableEntity`; índices únicos filtrados `[IsDeleted] = 0` |
| Claims vs persistência | Contexto/permissions/claims tipadas **não** são a fonte de verdade; são emitidas a partir de Membership + assignments + binding |

### Problemas estruturais (por gravidade)

| # | Problema | Impacto |
|---|---------|---------|
| P1 | Sidecars OAuth sem FK para `OpenIddictApplications` | Órfãos de `client_id`; client removido no OpenIddict deixa metadados/bindings/webhooks/sessões vivos |
| P2 | Unicidade composta com colunas NULL (`UserRoleAssignment`, `UserClaimAssignment`) | SQL Server trata `NULL ≠ NULL` → duplicatas de assignment **platform-scoped** (Org/Branch nulos) |
| P3 | Sem FK composta Branch×Org | Assignment/sessão pode apontar `BranchId` de org A e `OrganizationId` de org B; só o `switch-context` valida isso em runtime |
| P4 | Tabelas Identity de **roles** sem uso | Dois mundos de “role”; risco de alguém gravar authz em `AspNetUserRoles` achando que vale |
| P5 | `CreateUserRoleAssignmentSpecificationsValidator` vazio | Sem spec de membership, branch∈org, product enablement; FluentValidation só impede Branch sem Org |
| P6 | `ClientId` nvarchar(128) nos sidecars vs OpenIddict nvarchar(100) | Drift de tipo; impede alternate key limpa |
| P7 | Soft-delete + FK Restrict | FK aceita pai **soft-deleted** (ainda existe a linha); integridade “lógica” só na aplicação |
| P8 | `MenuItem.PermissionCode` string sem vínculo a `Permissions` | Menu órfão de permissão; intencional na 00012, reavaliado aqui |
| P9 | `OrganizationContact` sem unicidade de `IsPrimary` | Vários contatos principais na mesma org |
| P10 | Glossário Membership “opcionalmente branch” ≠ código | Expectativa de BranchMembership que **não existe** e **não deve** ser criada sem regra nova |

### O que **não** é bug (avaliado semanticamente)

| Campo / estrutura | Por que não vira FK / tabela extra |
|-------------------|-------------------------------------|
| `ExternalIdentityProvider.ClientId` | Client OAuth do **IdP externo** (Google/Entra), não AuthClient local |
| `AuthAuditEvent.UserId` / `ClientId` | Trilha append-only (D-00012-2) |
| `WebhookOutbox.ClientId` | Outbox histórico (D-00012-2) |
| `RevokedSession.SessionId` / `UserId` | Deny-list deve sobreviver à sessão/user (D-00012-4, ADR-007) |
| `OpenIddictAuthorizations.Subject` / `Tokens.Subject` | String OIDC do protocolo; tenancy está em `UserSession` + claims |
| `OpenIddictApplications.Permissions` (JSON) | Permissões **OAuth do client** (endpoints/grants/scopes), ≠ `Permissions` de domínio |
| `AspNetUserClaims` | Claims de framework Identity; distintas de `UserClaimAssignments` (contexto Org/Branch/Product) |
| `Product` sem `OrganizationId` | Catálogo global (F00001-D10, D-00013-1) |
| `Role` / `Permission` sem `ProductId` | Catálogo global; o produto entra no **assignment** |
| User ↔ Product direto | Não há “membership de produto”; habilitação é Org×Product; acesso do user é assignment |

### Divergência C# → EF → SQL

As FKs Guid da 00012 estão alinhadas nas três camadas. Divergências restantes:

| Camada | Divergência |
|--------|-------------|
| C# Domain | Sidecars têm `ClientId` string; sem `ApplicationId`; sem nav OpenIddict (correto — ADR-001: Domain não referencia OpenIddict) |
| EF maps | Sem `HasOne` para OpenIddict; `ClientId` sem principal key |
| SQL | `IX_OpenIddictApplications_ClientId` único filtrado (`ClientId IS NOT NULL`) — **não** é UNIQUE CONSTRAINT; FK SQL não pode apontá-lo até promover alternate key |
| C# validators | Branch∈Org só no `switch-context` e no `UserRoleAssignmentValidator` (Branch vazio se Org nula); **não** no banco |
| IdentityDbContext | `IdentityDbContext<User, IdentityRole<Guid>, Guid>` força tabelas de role Identity mesmo sem `RoleManager` no domínio |

---

## 2. Mapa das entidades e relacionamentos existentes

```text
                    ASP.NET Identity                         OpenIddict
                 ┌─────────────────────┐              ┌──────────────────────────┐
                 │ AspNetUsers (User)  │              │ OpenIddictApplications   │
                 │ AspNetUserLogins    │              │ OpenIddictAuthorizations │
                 │ AspNetUserTokens    │              │ OpenIddictTokens         │
                 │ AspNetUserClaims    │              │ OpenIddictScopes         │
                 │ AspNetRoles *mortas │              └────────────┬─────────────┘
                 │ AspNetUserRoles *   │                           │ ClientId string (sem FK)
                 │ AspNetRoleClaims *  │                           ▼
                 └──────────┬──────────┘              AuthClientMetadata
                            │                         ClientProductBinding ──► Products
                            │                         ClientWebhookEndpoints
                            │                         UserSessions.ClientId
                            ▼
                      Memberships ◄──────────── Organizations (tenant)
                            │                        │
                            │                        ├── Branches (ParentBranch self)
                            │                        ├── OrganizationInvites
                            │                        ├── OrganizationContacts
                            │                        ├── ProductEnablements ──► Products
                            │                        ├── ExternalIdentityProviders
                            │                        └── LdapGroupRoleMaps
                            ▼
                 UserRoleAssignments  (User × Role × Org? × Branch? × Product)
                 UserClaimAssignments (User × ClaimDef × Org? × Branch? × Product)
                            │
                     AuthRoles ── RolePermissions ── Permissions
                            └── AuthRoleClaims ── ClaimDefinitions ──► Products?
                                                      MenuItems ──► Products
                                                      (PermissionCode string)
```

### Cardinalidades atuais (domínio)

| Relação | Cardinalidade | Persistida | FK SQL |
|---------|---------------|------------|--------|
| User ↔ Organization | N:N via `Membership` | Sim | Sim (00012) |
| Organization → Branch | 1:N | Sim | Sim |
| Branch → Branch (pai) | 1:N opcional | Sim | Sim (self) |
| Organization ↔ Product | N:N via `ProductEnablement` | Sim | Sim (00013) |
| OpenIddict Application ↔ Product | 1:1 via `ClientProductBinding` (unique `ClientId`) | Sim | Só ProductId |
| User → Branch | **não é pertinência** | Só assignment/sessão | FK BranchId opcional |
| User → Product | **não é pertinência** | Só assignment | FK ProductId |
| Application → Organization | **não existe** | — | — |
| Scope → Product | convenção de nome `{product_code}.{feature}` | OpenIddictScopes | Não |

Não existe entidade Tenant/Workspace além de **Organization**.

---

## 3. Relacionamentos que devem ser fortalecidos com FKs

### 3.1 Operacionais `ClientId` → OpenIddict (P1) — **fazer**

Promover `OpenIddictApplications.ClientId` a **alternate key** (UNIQUE CONSTRAINT, não-nulo no store efetivo) e declarar FK nos sidecars **operacionais**. Domain continua com `string ClientId` (sem tipo OpenIddict) — o map na Infrastructure usa `HasOne<OpenIddictEntityFrameworkCoreApplication<Guid>>().HasPrincipalKey(a => a.ClientId)`.

| Origem | Coluna | Destino | Card. | Delete | Obrigatória |
|--------|--------|---------|-------|--------|-------------|
| `AuthClientMetadata` | `ClientId` | `OpenIddictApplications.ClientId` | 1:1 | Restrict | Sim |
| `ClientProductBindings` | `ClientId` | idem | 1:1 | Restrict | Sim |
| `ClientWebhookEndpoints` | `ClientId` | idem | 1:1 | Restrict | Sim |
| `UserSessions` | `ClientId` | idem | N:1 | Restrict | Sim |

Alinhar tamanho: sidecars `nvarchar(100)` (hoje 128) = OpenIddict.

**Não** colocar `OrganizationId`/`ProductId` nas linhas OpenIddict (preserva ADR-001 e F00007-D2). Product continua no sidecar `ClientProductBinding`.

### 3.2 FK composta Branch × Organization (P3) — **fazer**

Unique `(Id, OrganizationId)` em `Branches` (redundante com PK mas habilita FK composta) e:

| Origem | Colunas | Destino | Quando |
|--------|---------|---------|--------|
| `UserRoleAssignments` | `(BranchId, OrganizationId)` | `Branches(Id, OrganizationId)` | `BranchId` NOT NULL |
| `UserClaimAssignments` | idem | idem | idem |
| `UserSessions` | idem | idem | idem |
| `LdapGroupRoleMaps` | idem | idem | idem |
| `Branches` (self) | `(ParentBranchId, OrganizationId)` | `Branches(Id, OrganizationId)` | `ParentBranchId` NOT NULL |

DeleteBehavior: **Restrict**. EF: `IsRequired(false)` no lado BranchId; SQL precisa de FK que aceite NULL em `BranchId` (quando BranchId é null, a composta não aplica).

### 3.3 Unicidade platform-scoped (P2) — **fazer**

Substituir o único índice filtrado só por `IsDeleted` por **dois** (padrão SQL Server para NULL):

```text
UX_UserRoleAssignments_Tenant
  (UserId, RoleId, OrganizationId, BranchId, ProductId) UNIQUE
  WHERE IsDeleted = 0 AND OrganizationId IS NOT NULL

UX_UserRoleAssignments_Platform
  (UserId, RoleId, ProductId) UNIQUE
  WHERE IsDeleted = 0 AND OrganizationId IS NULL AND BranchId IS NULL
```

Espelhar em `UserClaimAssignments` (`ClaimDefinitionId` no lugar de `RoleId`).

### 3.4 Contato principal (P9) — **fazer** (baixo risco)

```text
UX_OrganizationContacts_Primary
  (OrganizationId) UNIQUE WHERE IsDeleted = 0 AND IsPrimary = 1
```

### 3.5 Specs de domínio (P5) — **fazer** (não é FK)

Em `UserRoleAssignment` / `UserClaimAssignment` / `LdapGroupRoleMap`:

- Branch, se informada, pertence à Organization (cinto e suspensório da FK composta).
- Organization, se informada, existe e não está soft-deleted.
- User existe e não está soft-deleted.
- **Não** exigir Membership no INSERT (hoje dá para pré-atribuir role antes do convite; o resolver já ignora org-roles sem membership). Documentar.
- **Não** exigir ProductEnablement no assignment (enablement é gate de **emissão de token**, não de cadastro de role).

---

## 4. Relacionamentos que devem ser removidos ou **não** criados

| Proposta tentadora | Decisão | Justificativa |
|--------------------|---------|---------------|
| `OpenIddictApplications.OrganizationId` | **Não criar** | AuthClients são de plataforma; o tenant entra no token via switch-context + Membership. Um SPA de produto atende **todas** as orgs com o product habilitado |
| `OpenIddictApplications.ProductId` | **Não** (já existe sidecar 1:1) | Duplicaria `ClientProductBinding`; manter um único dono da relação |
| `OpenIddictAuthorizations/Tokens.OrganizationId` | **Não criar** | Protocolo OAuth não é tenancy; `UserSession` + claims `organization_id`/`sid` já amarram o contexto |
| `OpenIddictScopes.ProductId` / tabela Scope×Product | **Não neste ciclo** | Convenção `{product_code}.{feature}` + seed; catálogo extra só se a UI admin precisar filtrar scopes por product |
| `Membership.BranchId` ou `BranchMembership` | **Não criar** | Pertinência é à **Organization**. Branch é contexto de authz (assignment + switch-context). Qualquer membro (ou PlatformAdmin) pode escolher uma branch da org — comportamento atual, alinhado a ADR-003 |
| `User` → `Product` N:N | **Não criar** | ProductEnablement (org) + UserRoleAssignment (user) cobrem comercial vs acesso |
| FK `MenuItem.PermissionCode` → `Permissions.Code` | **Não** (P8 fica fraco) | Unique de `Permission.Code` é filtrado por `IsDeleted`; FK SQL não aponta índice filtrado sem alternate key rígida; recatalog de código quebraria menus. Validar na app (spec “código existe”) |
| FK Audit / Outbox / RevokedSession | **Manter fracos** | D-00012-2/4 permanecem |
| Migrar `AuthRoles` → `AspNetRoles` | **Não** | Identity roles são globais, sem Org/Branch/Product |
| Migrar `UserClaimAssignments` → `AspNetUserClaims` | **Não** | Sem colunas de contexto; ADR-002 |
| Estender entidades OpenIddict no Domain | **Não** | ADR-001: Domain não referencia OpenIddict; extensão só no map Infrastructure |

---

## 5. Tabelas que devem utilizar o padrão ASP.NET Identity

| Tabela | Uso atual | Destino |
|--------|-----------|---------|
| `AspNetUsers` | Conta SSO (`User`) | **Manter e estender só com perfil** (`DisplayName`, auditoria soft-delete) |
| `AspNetUserLogins` | Federação Entra/Google (`FindByLoginAsync`) | **Manter** |
| `AspNetUserTokens` | 2FA / recovery (`AddDefaultTokenProviders`) | **Manter** |
| `AspNetUserClaims` | Praticamente ociosa (claims de token vêm do factory) | **Manter vazia** — Identity pode gravar claims de conta; **não** usar para tenancy/permissions |
| `AspNetRoles` | Nenhum `RoleManager` / `AddToRole` no produto | **Remover** (D-00015-2) |
| `AspNetUserRoles` | Não usada | **Remover** com AspNetRoles |
| `AspNetRoleClaims` | Não usada | **Remover** com AspNetRoles |

`AddIdentity<User, IdentityRole<Guid>>` passa a `AddIdentityCore<User>` + `AddSignInManager` + stores em `IdentityUserContext<User>` (ou equivalente sem role type). OpenIddict `Scopes.Roles` é scope OIDC, **não** depende de `AspNetRoles`.

---

## 6. Tabelas que devem utilizar o padrão OpenIddict

| Tabela | Responsabilidade | Integração proposta |
|--------|------------------|---------------------|
| `OpenIddictApplications` | client_id, secret, redirects, grants, consent type nativo, permissions OAuth | Alternate key `ClientId`; **sem** colunas de Org/Product |
| `OpenIddictAuthorizations` | Consentimento / autorização OIDC (`Subject` = user id string) | Intocado; `Subject` continua fraco (protocolo) |
| `OpenIddictTokens` | Tokens de protocolo | Intocado; sessão de produto = `UserSessions` |
| `OpenIddictScopes` | Catálogo OIDC + scopes `{product}.{feature}` | Intocado; Resources/Properties se no futuro amarrar audience |

Sidecars **não** substituem essas tabelas (F00007-D2 permanece): metadados admin, binding de product, webhooks e sessão são domínio.

---

## 7. Estruturas próprias que devem ser mantidas

| Estrutura | Responsabilidade | Por que não Identity/OpenIddict |
|-----------|------------------|----------------------------------|
| `Organizations` / `Branches` | Tenant e unidades | Não existem no Identity/OIDC |
| `Memberships` | User ∈ Org (N:N) | Identity não tem tenancy |
| `Products` / `ProductEnablements` | Catálogo + habilitação comercial | Fora de OAuth |
| `AuthRoles` / `Permissions` / `RolePermissions` | AuthZ contextual | Identity roles são globais |
| `UserRoleAssignments` / `UserClaimAssignments` / `ClaimDefinitions` / `AuthRoleClaims` | Concessão Org/Branch/Product | Claims Identity não têm esse eixo |
| `AuthClientMetadata` | IsSystem, IsFirstParty, política Always/First/Never, TTL | OpenIddict `ConsentType` é mais pobre; Properties JSON perde query/validação |
| `ClientProductBindings` | Application → Product | Dono da relação de negócio |
| `UserSessions` / `RevokedSessions` | `sid` + deny-list (ADR-007) | OpenIddict tokens não carregam org/branch |
| `OrganizationInvites` / `OrganizationContacts` | Convite e contatos | Domínio |
| `MenuItems` | Menu por product + permission code | UI |
| `ExternalIdentityProviders` / `LdapGroupRoleMaps` | Federação | IdP externo ≠ AuthClient |
| `AuthAuditEvents` / `WebhookOutbox` | Histórico / outbox | Fracos intencionais |
| `DefaultDb.Samples` | Scaffold Forge | F00001-D8 — fora deste refinamento |

---

## 8. Estruturas próprias a remover / substituir

| Estrutura | Problema | Substituta | Migração de dados | Impacto código |
|-----------|----------|------------|-------------------|----------------|
| `AspNetRoles` + `AspNetUserRoles` + `AspNetRoleClaims` | Duplicam o *conceito* de role sem a semântica do produto; estão ociosas | Nenhuma — authz já é `AuthRoles` | Confirmar vazio (`SELECT COUNT`); DROP | `IdentityDbContext` base type; `AddIdentity`; testes de foundation |
| (Nenhuma tabela de domínio SSO) | — | — | — | Não remover `AuthClientMetadata` nem `ClientProductBinding` |

Não há tabela própria que reproduza Applications/Tokens/Scopes OpenIddict. Não há tabela própria de Users paralela a `AspNetUsers`.

---

## 9. Novo modelo de relacionamento proposto

```text
Identity (AuthN)
  AspNetUsers ◄──── Memberships ────► Organizations
       │                                  │
       │                                  ├── Branches (FK Org + self pai; unique Id+Org p/ FK composta)
       │                                  ├── ProductEnablements ────► Products
       │                                  └── (invites, contacts, IdPs, ldap maps)
       │
       ├── UserRoleAssignments ──► AuthRoles, Products, Organizations?, Branches?
       └── UserClaimAssignments ─► ClaimDefinitions, Products, Organizations?, Branches?

OpenIddict (OAuth/OIDC)
  OpenIddictApplications (AK ClientId)
       ▲ FK Restrict (ClientId)
       │
       ├── AuthClientMetadata          (1:1 admin sidecar)
       ├── ClientProductBinding ──► Products   (1:1 client→product)
       ├── ClientWebhookEndpoints      (1:1)
       └── UserSessions ──► Users, Organizations?, Branches?
                │
                └── (fraco) RevokedSessions.SessionId

Claims JWT (derivados, não persistidos como vínculo)
  organization_id, branch_id  ← Membership + switch-context + Branch∈Org
  permissions, perm_ver       ← UserRoleAssignment → RolePermission
  sso_c_*, claim_ver          ← UserClaimAssignment + RoleClaim
  sid                         ← UserSession.Id
```

### Matriz Identity/OpenIddict × Organization / Branch / Product

| Relação | Regra de negócio | Dona | Card. | Física? | Associação? | FK? | Gera Claim? | Gera Scope? | Camada | Reusa? | Elimina duplicata? | Impacto auth |
|---------|------------------|------|-------|---------|-------------|-----|-------------|-------------|--------|--------|--------------------|--------------|
| User ↔ Organization | User pertence a N tenants | Membership | N:N | Sim | Membership | Sim (já) | `organization_id` no switch | Não | Domínio | Membership | — | switch-context exige membership (exceto PlatformAdmin) |
| User ↔ Branch | User **não pertence** à branch; atua nela | Assignment / sessão | N:N implícita | Só BranchId opcional | **Não** criar BranchMembership | Composta se BranchId set | `branch_id` | Não | Domínio | UserRoleAssignment / UserSession | Corrige glossário | switch-context: branch ∈ org; **não** exige assignment na branch |
| User ↔ Product | Acesso via role no product do client | UserRoleAssignment | N:N | Sim | Assignment | ProductId já | `permissions` filtradas pelo product do client | Não | Domínio | Assignment | — | Product vem de `client_id` → binding |
| Application ↔ Organization | Client de plataforma, não de tenant | — | — | **Não** | Não | Não | Não | Não | — | — | Evita tabela paralela de clients | Inalterado |
| Application ↔ Product | Todo AuthClient de produto resolve authz daquele product | ClientProductBinding | 1:1 | Sim | Sidecar | ClientId→OpenIddict **+** ProductId | Audiences/client_id | Scopes `{code}.*` no client OpenIddict | Domínio+OIDC | Binding | Não duplicar ProductId no OpenIddict | TokenClaimsFactory já usa binding |
| Scope ↔ Product | Nome `{product_code}.{feature}` | OpenIddictScopes | — | Não (ciclo atual) | Não | Não | Não | O próprio scope | OIDC | Convenção F00007-D3 | — | RegisterScopes + Permissions.Prefixes.Scope |
| Authorization ↔ Org | Consent é por user×client×scopes, não por tenant | OpenIddict | — | Não | Não | Não | Não | Não | OIDC | Authorizations | — | Re-consent por scopes, não por org |
| Token/Authorization ↔ tenancy | Contexto no access token + sid | UserSession | N:1 sessão | UserSession | Sidecar | Org/Branch já; ClientId novo | `sid`, org, branch | Não | Domínio | UserSession | Não copiar org para OpenIddictTokens | Hot revoke inalterado |

---

## 10. Alterações necessárias nas entidades / EF

### Domain

- Sem novos aggregates.
- Sem referência a tipos OpenIddict.
- Specs novas (P5) + unicidade platform-scoped (já há spec de membership duplicado; assignments precisam equivalente para o caso NULL).
- `UserRoleAssignmentValidator` já impede Branch sem Org; manter.
- Opcional: spec `MenuItem.PermissionCode` existe em `Permissions` (não-FK).

### Infrastructure maps

| Map | Mudança |
|-----|---------|
| `AuthClientMetadataMap`, `ClientProductBindingMap`, `ClientWebhookEndpointMap`, `UserSessionMap` | `HasOne<OpenIddictEntityFrameworkCoreApplication<Guid>>().HasForeignKey(e => e.ClientId).HasPrincipalKey(a => a.ClientId).OnDelete(Restrict)`; `ClientId` nvarchar(100) |
| `AddDbContext` / OpenIddict | Garantir `ClientId` required + `HasAlternateKey` (pode exigir customização do model OpenIddict após `UseOpenIddict<Guid>()`) |
| `BranchMap` | `HasAlternateKey` ou unique `(Id, OrganizationId)` |
| `UserRoleAssignmentMap`, `UserClaimAssignmentMap`, `UserSessionMap`, `LdapGroupRoleMapMap` | FK composta; índices únicos filtrados em dois pedaços |
| `OrganizationContactMap` | Unique filtrado IsPrimary |
| `IdentityDbContext` | Base `IdentityUserContext<User, Guid>` (ou `IdentityUserContext<User>`) — **sem** `IdentityRole` |
| `RoleMap` / `PermissionMap` | Sem mudança de relação; opcional `HasIndex` extra se queries pedirem |

### Convenções a preservar

- PK `Id` Guid; Restrict em FKs de domínio e nas novas FKs OpenIddict.
- Cascade **somente** nas tabelas Identity restantes (claims/logins/tokens do user) — padrão do framework.
- Sem `HasQueryFilter(IsDeleted)` neste ciclo (quebra unique filtrado + `IgnoreQueryFilters` em massa).

### DeleteBehavior resumido (alvo)

| FK | OnDelete | CASCADE? |
|----|----------|----------|
| Domínio → Users / Orgs / Branches / Products / Roles | Restrict | Não |
| Sidecar → OpenIddictApplications | Restrict | Não (apagar client exige limpar sidecars antes) |
| Identity UserClaims/Logins/Tokens → User | Cascade (framework) | Sim — só hard-delete de User, que o produto **não deve fazer** (soft-delete) |
| OpenIddict Token → Application/Authorization | Default OpenIddict (não alterar) | Interno do store |

---

## 11. Migrations necessárias

Preferir **três** migrations DDL (mais uma DML de limpeza se P2 achar duplicatas):

| # | Nome sugerido | Conteúdo |
|---|---------------|----------|
| 1 | `Phase18OpenIddictClientAlternateKeyAndFks` | Alternate key ClientId; align length; pré-check órfãos; FKs Restrict nos 4 sidecars |
| 2 | `Phase18bCompositeBranchOrganizationFks` | Unique `(Id, OrganizationId)` em Branches; FKs compostas; índices únicos platform/tenant |
| 3 | `Phase18cDropUnusedIdentityRoles` | DROP `AspNetRoleClaims`, `AspNetUserRoles`, `AspNetRoles` após count=0; snapshot IdentityUserContext |
| 4 | `Phase18dOrganizationContactPrimaryUnique` | pode ir junto da 2 se pequeno |

Pré-checks SQL (padrão 00012 `Phase14ExplicitIdentityForeignKeys`):

1. Sidecars `ClientId` inexistente em `OpenIddictApplications`.
2. `ClientId` nulo/vazio em sidecars obrigatórios.
3. Assignments com `BranchId` cujo `Branches.OrganizationId` ≠ `OrganizationId`.
4. `ParentBranchId` de outra org.
5. Duplicatas platform-scoped (mesmo User+Role+Product com Org e Branch null).
6. Mais de um `IsPrimary` ativo por org.
7. Rows em `AspNetRoles` / `AspNetUserRoles` / `AspNetRoleClaims`.

Rollback: `Down` dropa FKs novas e restaura índices antigos; DROP de AspNetRoles só com backup se count>0 (abortar migration).

---

## 12. Tratamento dos dados existentes antes das novas FKs

Ambientes atuais = seed + dev. Ainda assim o plano exige script/relatório, não `AddForeignKey` cego.

| Inconsistência | Detecção | Tratamento |
|----------------|----------|------------|
| Binding/metadata/webhook/session com `client_id` órfão | LEFT JOIN OpenIddictApplications | Corrigir para client válido **ou** soft-delete do sidecar; **não** apagar UserSession ativa sem revogar |
| `ClientId` > 100 chars | LEN() | Truncar só se for lixo; senão abortar |
| Branch/Org mismatch | JOIN Branches | Corrigir `OrganizationId` pela branch **ou** zerar `BranchId` se for org-wide; preferir corrigir Org (fonte da branch) |
| Duplicata platform assignment | GROUP BY UserId, RoleId, ProductId HAVING COUNT>1 WHERE Org IS NULL | Manter o mais recente (`UpdatedAt`/`CreatedAt`); soft-delete os outros |
| AspNetRoles com dados | COUNT | **Parar** — investigar quem gravou; não DROP |
| Contatos IsPrimary múltiplos | GROUP BY OrganizationId | Manter o mais antigo; demais `IsPrimary=0` |
| Membership órfã | já bloqueada por FK 00012 | N/A |
| User soft-deleted com filhos | Restrict + filtros app | Fora desta migration; limpeza operacional |

Não há tenant/workspace legado para converter.

---

## 13. Impactos no código da aplicação

| Área | Impacto |
|------|---------|
| `TokenClaimsFactory` / switch-context | Nenhum contrato de claim; FKs só impedem dados inválidos |
| CRUD AuthClients | Falha ao gravar metadata/binding se `client_id` OpenIddict inexistente — **desejável**; criar application **antes** do sidecar (já é a ordem do seed/admin) |
| Delete/disable AuthClient | Restrict impede apagar application com sessões/bindings; fluxo admin deve disable (`AuthClientMetadata.IsEnabled`) em vez de DELETE OpenIddict, ou limpar sidecars explicitamente |
| `AddIdentity` / `IdentityDbContext` | Troca de tipos; `AuthFoundationScenarios` |
| `IdentityForeignKeyModelScenarios` | Inverter asserts: `ClientId` **passa a ter** FK nos 4 sidecars; Audit/Outbox/Revoked/ExternalIdp.ClientId continuam sem |
| Domain specs | Novos validators; testes unitários de spec |
| APIs / Razor | Nenhum payload novo se ClientId permanece a chave pública |
| SDK / JWT | Sem breaking change |
| Docs | `data-dictionary.md`, `glossary.md` (Membership sem branch), `Decisions.md`, ADR-002 nota sobre IdentityUserContext, D-00012-3 superseded **parcialmente** |

Código que **não** muda de responsabilidade: `EffectivePermissionsResolver` (já exige membership para org-roles), `ProductEnablementGuard`, consent OpenIddict.

---

## 14. Riscos e pontos que precisam de validação **antes** da implementação

| Risco | Validação |
|-------|-----------|
| Alternate key em `ClientId` OpenIddict vs updates do store | Confirmar que o manager nunca troca `ClientId`; se trocar, Restrict + AK quebram — aceitável (client_id é identidade OAuth) |
| `UseOpenIddict<Guid>()` sobrescreve HasAlternateKey | Protótipo de model snapshot em branch; se conflitar, `ClientId` Guid `ApplicationId` nos sidecars (plano B, maior impacto no código) |
| SQL Server unique + NULL | Confirmar duplicatas reais no banco de homologação antes do índice platform |
| DROP AspNetRoles | Confirmar count=0 em **todos** os ambientes |
| Soft-delete vs FK | Aceitar que pai soft-deleted ainda passa FK; specs + queries `!IsDeleted` continuam obrigatórios |
| Pertinência User↔Branch | Product owner confirma que membro da org pode switchar **qualquer** branch (hoje sim). Se a regra mudar para “só branches autorizadas”, isso é **feature nova** (`BranchMembership`), não este refinamento |
| AuthClient por tenant (white-label) | Fora; se no futuro existir, aí sim `AuthClientMetadata.OrganizationId` opcional — não OpenIddictApplication.OrganizationId no ciclo atual |

---

## 15. Estimativa de esforço por etapa

| Etapa | Conteúdo | Esforço |
|-------|----------|---------|
| 0. Aceite decisões D-00015-1..8 | PO/arquitetura | 0,5 d |
| 1. Inventário SQL homolog (órfãos, duplicatas, AspNetRoles) | Script + relatório | 0,5 d |
| 2. Phase18 OpenIddict AK + FKs ClientId | Maps, migration, testes modelo, ajuste seed/admin order | 2 d |
| 3. Phase18b FK composta + unique NULL-safe | Maps, specs Branch∈Org, migration, testes | 2 d |
| 4. Phase18c drop Identity roles | DbContext, DI, migration, testes foundation | 1 d |
| 5. Unique IsPrimary + spec PermissionCode (opcional) | 0,5 d |
| 6. Docs CONTEXT + ADR-002 adendo + D-00012-3 | 0,5 d |
| 7. Suíte Identity (regressão CRUD, OIDC, switch-context, enablement) | 1 d |
| **Total** | | **~8 d** (1,5–2 sprints se incluir homolog dados sujos) |

Plano B (`ApplicationId` Guid em vez de FK por `ClientId`): +2 d e breaking interno nos sidecars.

---

## Escopo

### Inclui

- Fortalecer FKs OpenIddict↔sidecars operacionais.
- FK composta Branch×Org.
- Unique indexes NULL-safe em assignments.
- Remoção das tabelas Identity de role ociosas.
- Specs de consistência que o banco não cobre (soft-delete).
- Unique contato principal.
- Documentação (dicionário, glossário, decisions, ADR-002 nota).

### Fora de escopo

- Implementação **antes** do aceite das decisões.
- White-label AuthClient por Organization.
- BranchMembership / restrição de switch-context por assignment.
- Global query filter `IsDeleted`.
- Merge AuthClientMetadata → JSON Properties.
- Aposentar `DefaultDb`/Sample (F00001-D8).
- Vigência/billing de ProductEnablement.
- FK em Audit / Outbox / RevokedSession / Subject OpenIddict.
- Tabela Scope×Product.
- Estender classes OpenIddict no Domain.

---

## Abordagem (lotes de implementação)

```text
Lote A  OpenIddict ClientId AK + FKs sidecars + length 100
Lote B  FK composta Branch×Org + unique platform/tenant
Lote C  IdentityUserContext + DROP AspNetRoles*
Lote D  Specs + unique IsPrimary + docs
```

Cada lote = migration + testes de modelo + regressão mínima. Não misturar DROP Identity roles com FKs OpenIddict na mesma migration.

---

## Arquivos impactados (quando implementar)

| Camada | Caminhos previstos |
|--------|--------------------|
| Domain | Specs/validators `UserRoleAssignments`, `UserClaimAssignments`, `LdapGroupRoleMaps`, opcional `MenuItems` |
| Application | Nenhum contrato HTTP novo esperado |
| Data | `*Map.cs` listados; `IdentityDbContext.cs`; `Identity/Migrations/Phase18*` |
| Middleware/DI | `AddIdentityConfigurations.cs` (`AddIdentityCore`); `AddDbContextConfigurations.cs` se customizar OpenIddict model |
| API | Ordem create AuthClient (já OpenIddict-first); delete/disable |
| Tests | `IdentityForeignKeyModelScenarios` (inverter ClientId); novos spec tests; `AuthFoundationScenarios`; regressão OIDC/switch-context |
| Docs (.ai) | Este plano; `data-dictionary.md`; `glossary.md`; `Decisions.md`; adendo ADR-002 |

---

## Riscos (resumo operacional)

| Risco | Mitigação |
|-------|-----------|
| Homolog com órfãos bloqueia FK | Relatório lote 1; limpeza explícita |
| OpenIddict não aceita AlternateKey | Plano B `ApplicationId` Guid |
| Alguém usa `RoleManager` não encontrado no grep | Grep + teste de DI; abortar lote C se count>0 |
| Restrict impede “apagar” client | Disable via metadata; documentar no admin |

---

## Estratégia de testes

- [x] Modelo EF: FK ClientId Restrict nos 4 sidecars
- [x] Modelo EF: Audit/Outbox/Revoked/ExternalIdp.ClientId **sem** FK
- [x] Modelo EF: FK composta Branch+Org
- [ ] Insert assignment Branch de outra org **falha** no SQL
- [ ] Insert segundo assignment platform duplicado **falha**
- [ ] Insert sidecar com client_id inexistente **falha**
- [x] `AddIdentityCore` + login/2FA/external login verdes
- [ ] Switch-context + ProductEnablement + TokenClaimsFactory regressão
- [ ] Seed OpenIddict + metadata + binding na mesma transação lógica (ordem)

---

## Checklist

- [x] Análise C# / EF snapshot / uso Identity / OpenIddict / claims
- [x] Alinhado a ADR-001 (Domain ≠ OpenIddict), ADR-002 (AuthN Identity ≠ AuthZ domínio), ADR-003 (tenant=Org)
- [x] Decisões D-00015-1..8 aceitas (todas A, 2026-08-16)
- [x] Inventário SQL de homolog
- [x] Migrations
- [x] CONTEXT atualizado (na implementação)
- [x] Pronto para implementação

---

## Decisões aceitas (2026-08-16) — todas **A**

| ID | Escolha | Significado |
|----|---------|-------------|
| **D-00015-1** | **A** | Alternate key `ClientId` + FK Restrict nos 4 sidecars operacionais. Supersede parcial de D-00012-3. Audit/Outbox/Revoked fracos |
| **D-00015-2** | **A** | `IdentityUserContext<User>`; DROP AspNetRoles/UserRoles/RoleClaims se vazias. AuthZ = `AuthRoles` |
| **D-00015-3** | **A** | FK composta `(BranchId, OrganizationId)` → `Branches` + specs |
| **D-00015-4** | **A** | Dois unique filtrados (tenant vs platform) em assignments |
| **D-00015-5** | **A** | Sem `BranchMembership`; Membership = User↔Org; lista/switch = todas as filiais da org |
| **D-00015-6** | **A** | Sem Org no OpenIddict Application; Product só via `ClientProductBinding` |
| **D-00015-7** | **A** | Sem tenancy em OpenIddict Tokens/Authorizations; contexto = `UserSession` + claims |
| **D-00015-8** | **A** | `MenuItem.PermissionCode` string + spec “código existe”; sem FK |

---

## Referências

- ADR-001 OpenIddict AS; ADR-002 Identity AuthN; ADR-003 switch-context; ADR-006 IdentityDb; ADR-007 sid
- 00012 FKs Guid; 00007 sidecar AuthClient; 00013 ProductEnablement
- `CONTEXT/data-dictionary.md`, `modules.md`, `business.md`
- Teste âncora: `SSO.Tests/UnitTests/Infrastructures/Data/Identity/IdentityForeignKeyModelScenarios.cs`
