# Bugfix — 00012 Foreign keys fracas nos aggregates Identity

> Arquivo: `.ai/WORK/2026-07-27-00012-bug-foreign-keys-fracas.md`  
> Template: `.ai/TEMPLATES/bugfix.md`  
> Status: **Implementado**  
> Data: 2026-07-27  
> Relaciona: 00001 (aggregates Identity), soft-delete F00001-D12  
> Decisões: **B / A / A / A** (D-00012-1..4)  
> Migration: `Phase14ExplicitIdentityForeignKeys` + `Phase14bInverseCollectionNavigations` (snapshot only)

## Sintoma

Colunas `*Id` (e `ClientId` string) nos aggregates do context Identity **referenciam** outras entidades semanticamente, mas **não há foreign key explícita** no EF Core nem no SQL Server.

Evidência estrutural:

- Nenhum `*Map` em `SSO.Infrastructures.Data/Identity/EntityMappings/` usa `HasOne` / `HasMany` / `HasForeignKey`.
- O `IdentityDbContextModelSnapshot` só declara FKs de **ASP.NET Identity** (`AspNet*`) e **OpenIddict** (`OpenIddict*`).
- O template de referência `SampleMap` já prevê regiões `#region Map Foreign Keys` / `Map Relations`, mas os maps Identity nunca as preencheram.

Consequência: o banco aceita órfãos (IDs inexistentes, soft-deleted ou inconsistentes) sem restrição de integridade referencial.

## Esperado

Relacionamentos de domínio com alvo estável no mesmo DbContext devem ter **FK explícita** no mapeamento EF + migration SQL, com `DeleteBehavior` alinhado ao soft-delete (tipicamente `Restrict` / `NoAction`), exceto casos documentados como referência fraca intencional (auditoria, outbox, sidecar por `client_id` string).

## Reprodução

1. Inspecionar qualquer map Identity (ex.: `MembershipMap`, `BranchMap`, `UserRoleAssignmentMap`) — só `Property` + índices; sem relação.
2. Comparar com o snapshot: `HasOne`/`HasForeignKey` apenas em Identity/OpenIddict.
3. (Opcional) Inserir via SQL um `Membership` com `OrganizationId` inexistente — commit sem erro de FK.

## Ambiente

- Branch/commit: estado atual do repo (levantamento em 2026-07-27)
- Ambiente: código / model snapshot (não depende de runtime)
- Dados relevantes: schema `IdentityDb`

## Hipótese raiz

Os aggregates Identity foram modelados com **IDs soltos** (estilo “weak reference” / integração por Guid) desde a Phase 1, sem configurar relações EF. Soft-delete (`IsDeleted`) e referências a OpenIddict por `ClientId` (string, não PK) tornaram FKs “duras” menos óbvias, e a dívida não foi fechada nas features seguintes.

## Evidências

### O que tem FK explícita (OK)

| Área | Tabelas | FKs |
|------|---------|-----|
| ASP.NET Identity | `AspNetUserRoles`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`, `AspNetRoleClaims` | `UserId` → `AspNetUsers`; `RoleId` → `AspNetRoles` |
| OpenIddict | `OpenIddictAuthorizations`, `OpenIddictTokens` | `ApplicationId` / `AuthorizationId` |

### Aggregates raiz (sem coluna FK — N/A)

| Entidade | Tabela |
|----------|--------|
| Organization | `Organizations` |
| Product | `Products` |
| Permission | `Permissions` |
| Role | `AuthRoles` |
| User | `AspNetUsers` (FKs Identity já cobertas) |

### Aggregates com FK fraca (bug)

| Entidade | Tabela | Coluna(s) sem FK | Alvo esperado | Obrigatória? |
|----------|--------|------------------|---------------|--------------|
| Branch | `Branches` | `OrganizationId` | `Organizations` | Sim |
| Branch | `Branches` | `ParentBranchId` | `Branches` (self) | Não |
| Membership | `Memberships` | `UserId` | `AspNetUsers` | Sim |
| Membership | `Memberships` | `OrganizationId` | `Organizations` | Sim |
| OrganizationInvite | `OrganizationInvites` | `OrganizationId` | `Organizations` | Sim |
| OrganizationInvite | `OrganizationInvites` | `InvitedByUserId` | `AspNetUsers` | Sim |
| OrganizationInvite | `OrganizationInvites` | `AcceptedUserId` | `AspNetUsers` | Não |
| RolePermission | `RolePermissions` | `RoleId` | `AuthRoles` | Sim |
| RolePermission | `RolePermissions` | `PermissionId` | `Permissions` | Sim |
| UserRoleAssignment | `UserRoleAssignments` | `UserId` | `AspNetUsers` | Sim |
| UserRoleAssignment | `UserRoleAssignments` | `RoleId` | `AuthRoles` | Sim |
| UserRoleAssignment | `UserRoleAssignments` | `OrganizationId` | `Organizations` | Não |
| UserRoleAssignment | `UserRoleAssignments` | `BranchId` | `Branches` | Não |
| UserRoleAssignment | `UserRoleAssignments` | `ProductId` | `Products` | Sim |
| MenuItem | `MenuItems` | `ProductId` | `Products` | Sim |
| ClaimDefinition | `ClaimDefinitions` | `ProductId` | `Products` | Não |
| RoleClaim | `AuthRoleClaims` | `RoleId` | `AuthRoles` | Sim |
| RoleClaim | `AuthRoleClaims` | `ClaimDefinitionId` | `ClaimDefinitions` | Sim |
| UserClaimAssignment | `UserClaimAssignments` | `UserId` | `AspNetUsers` | Sim |
| UserClaimAssignment | `UserClaimAssignments` | `ClaimDefinitionId` | `ClaimDefinitions` | Sim |
| UserClaimAssignment | `UserClaimAssignments` | `OrganizationId` | `Organizations` | Não |
| UserClaimAssignment | `UserClaimAssignments` | `BranchId` | `Branches` | Não |
| UserClaimAssignment | `UserClaimAssignments` | `ProductId` | `Products` | Sim |
| ExternalIdentityProvider | `ExternalIdentityProviders` | `OrganizationId` | `Organizations` | Não |
| LdapGroupRoleMap | `LdapGroupRoleMaps` | `OrganizationId` | `Organizations` | Sim |
| LdapGroupRoleMap | `LdapGroupRoleMaps` | `RoleId` | `AuthRoles` | Sim |
| LdapGroupRoleMap | `LdapGroupRoleMaps` | `ProductId` | `Products` | Sim |
| LdapGroupRoleMap | `LdapGroupRoleMaps` | `BranchId` | `Branches` | Não |
| ClientProductBinding | `ClientProductBindings` | `ProductId` | `Products` | Sim |
| UserSession | `UserSessions` | `UserId` | `AspNetUsers` | Sim |
| UserSession | `UserSessions` | `OrganizationId` | `Organizations` | Não |
| UserSession | `UserSessions` | `BranchId` | `Branches` | Não |
| RevokedSession | `RevokedSessions` | `SessionId` | `UserSessions` | Sim |
| RevokedSession | `RevokedSessions` | `UserId` | `AspNetUsers` | Não |

**Total:** 17 entidades / ~35 colunas Guid sem FK explícita.

### Referências por `ClientId` string (OpenIddict) — FK fraca especial

OpenIddict usa PK `Guid` em `OpenIddictApplications`; o domínio guarda o **`client_id` lógico** (`nvarchar(128)`), não o PK. Declarar FK SQL exige índice único em `ClientId` no OpenIddict (ou tabela ponte) — hoje é referência fraca por design operacional.

| Entidade | Tabela | Coluna | Alvo semântico |
|----------|--------|--------|----------------|
| ClientProductBinding | `ClientProductBindings` | `ClientId` | OpenIddict application |
| AuthClientMetadata | `AuthClientMetadata` | `ClientId` | OpenIddict application (sidecar) |
| ClientWebhookEndpoint | `ClientWebhookEndpoints` | `ClientId` | OpenIddict application |
| UserSession | `UserSessions` | `ClientId` | OpenIddict application |
| RevokedSession | `RevokedSessions` | `ClientId` | OpenIddict application |
| AuthAuditEvent | `AuthAuditEvents` | `ClientId` | OpenIddict application (audit) |
| WebhookOutboxMessage | `WebhookOutbox` | `ClientId` | OpenIddict application (outbox) |

### Não é FK de domínio (não listar como bug de Guid)

| Coluna | Motivo |
|--------|--------|
| `ExternalIdentityProvider.ClientId` | Client OAuth do IdP externo (Google/Entra), não AuthClient local |
| `MenuItem.PermissionCode` | Código de permissão (string), não `Permission.Id` |
| `AuthAuditEvent.UserId` | Fraco intencional (D-00012-2) — trilha histórica após exclusão/soft-delete |

Arquivos de map: `src/SSO.Infrastructures.Data/Identity/EntityMappings/*.cs`  
Snapshot: `src/SSO.Infrastructures.Data/Identity/Migrations/IdentityDbContextModelSnapshot.cs`

## Correção proposta

1. **FKs Guid** entre aggregates Identity + `AspNetUsers` / `AuthRoles` / `Organizations` / `Products` / `Branches` / `Permissions` / `ClaimDefinitions` (e `UserSession` Guid FKs).
2. **Domain com navigation properties** (D-00012-1 = B): FK side (`Organization`, `User`, …) **e** collections inversas nos pais (`Organization.Branches`, `User.Memberships`, …) + nos `*Map`: `HasOne(e => e.X).WithMany(p => p.Xs).HasForeignKey(...).OnDelete(DeleteBehavior.Restrict)`.
3. Migration única adicionando FKs; **pré-check** de órfãos antes do `AddForeignKey`.
4. Soft-delete: Restrict evita cascade físico; limpeza de filhos continua a cargo de Domain Services / specs.
5. **`ClientId` string, Audit, Outbox, RevokedSession:** fracos intencionais (D-00012-2..4) — fora desta correção.

### Ordem de implementação

| Lote | Entidades | Motivo |
|------|-----------|--------|
| A | Branch, Membership, RolePermission, OrganizationInvite | Núcleo multi-tenant |
| B | UserRoleAssignment, UserClaimAssignment, RoleClaim, MenuItem, ClaimDefinition | AuthZ / menus |
| C | LdapGroupRoleMap, ExternalIdentityProvider (`OrganizationId`), ClientProductBinding (`ProductId`) | Federação / bindings |
| D | UserSession (`UserId` / `OrganizationId` / `BranchId` only) | Sessão Guid; **sem** RevokedSession |
| — | `ClientId` / Audit / Outbox / RevokedSession | **Fora** (fracos intencionais) |

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Domain | Entidades com nav props (`*Id` + navegação) |
| Data (maps) | `src/SSO.Infrastructures.Data/Identity/EntityMappings/*Map.cs` |
| Data (migration) | `src/SSO.Infrastructures.Data/Identity/Migrations/` (nova) |
| Application / API | Nenhum (payloads já usam Ids) |
| Tests | Integração: insert órfão deve falhar; regressão CRUD existentes |
| Docs (.ai) | Este bugfix; Decisions.md |

## Testes

- [x] Pré-check SQL de órfãos na migration `Phase14ExplicitIdentityForeignKeys`
- [x] Modelo EF: FKs Guid com `DeleteBehavior.Restrict` (`IdentityForeignKeyModelScenarios`)
- [x] Modelo EF: collections inversas (`WithMany` nomeadas)
- [x] Modelo EF: Audit / Outbox / RevokedSession / `ClientId` string permanecem sem FK
- [x] Regressão MembershipIsolation + CreateOrganization verdes
- [ ] Suíte Identity completa (opcional / CI)

## Riscos / side effects

| Risco | Mitigação |
|-------|-----------|
| Dados órfãos em ambientes existentes bloqueiam migration | Relatório + script de limpeza/correção antes do `AddForeignKey` |
| Soft-delete + FK Restrict impede “apagar” pai com filhos | Comportamento desejado; Domain já deve impedir ou soft-deletar em cadeia |
| FK para `ClientId` string inviável / frágil com OpenIddict | Tratar lote E à parte; não bloquear lotes A–D |
| Performance em inserts (checks FK) | Negligível vs ganho de integridade; índices já existem em várias colunas |
| RevokedSession/Audit com retenção após sessão/user sumir | Manter fracos ou `SetNull` só onde nullable |

## Decisões aceitas (2026-07-27)

| ID | Escolha | Significado |
|----|---------|-------------|
| **D-00012-1** | **B** | Navigation properties no Domain + `HasOne`/`WithMany`/`HasForeignKey` nos maps |
| **D-00012-2** | **A** | `AuthAuditEvent` e `WebhookOutbox` **sem FK** (fracos intencionais) |
| **D-00012-3** | **A** | `ClientId` string **sem FK SQL** para OpenIddict; validação na app |
| **D-00012-4** | **A** | `RevokedSession.SessionId` / `UserId` **fracos** (snapshot / deny-list) |

## Checklist

- [x] Causa confirmada (não só sintoma) — maps + snapshot
- [x] Decisões D-00012-1..4 fechadas (B/A/A/A)
- [x] Fix na camada Domain + Data / migration
- [x] Sem mudança de comportamento colateral não intencional
- [x] Lotes A–C + UserSession Guid FKs implementados
- [x] Fracos intencionais documentados (Audit / Outbox / ClientId / RevokedSession)
- [x] Docs CONTEXT/Decisions atualizados
- [x] Testes de modelo FK Restrict + fracos intencionais
- [x] Pré-check SQL de órfãos na migration
