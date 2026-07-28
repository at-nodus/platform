# Bugfix — 00012 Foreign keys fracas nos aggregates Identity

> Arquivo: `.ai/WORK/2026-07-27-00012-bug-foreign-keys-fracas.md`  
> Template: `.ai/TEMPLATES/bugfix.md`  
> Status: **Aberto** (levantamento)  
> Data: 2026-07-27  
> Relaciona: 00001 (aggregates Identity), soft-delete F00001-D12

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
| `AuthAuditEvent.UserId` | Preferível manter fraca (trilha histórica após exclusão/soft-delete) — **decisão aberta** |

Arquivos de map: `src/SSO.Infrastructures.Data/Identity/EntityMappings/*.cs`  
Snapshot: `src/SSO.Infrastructures.Data/Identity/Migrations/IdentityDbContextModelSnapshot.cs`

## Correção proposta

Abordagem mínima (sem mudar API/domínio público):

1. **Prioridade P0 — FKs Guid obrigatórias** entre aggregates Identity + `AspNetUsers` / `AuthRoles` / `Organizations` / `Products` / `Branches` / `Permissions` / `ClaimDefinitions` / `UserSessions`.
2. Em cada `*Map`: `HasOne<T>().WithMany().HasForeignKey(...).OnDelete(DeleteBehavior.Restrict)` (ou `NoAction`), **sem** navigation properties no Domain (relação só no Infrastructure), salvo decisão contrária.
3. Migration única (ou por lote) adicionando FKs; **pré-check** de órfãos antes do `AddForeignKey`.
4. Soft-delete: Restrict evita cascade físico; limpeza de filhos continua a cargo de Domain Services / specs.
5. **`ClientId` string:** documentar como referência fraca intencional **ou** (follow-up) FK via índice único em `OpenIddictApplications.ClientId` se a versão do OpenIddict permitir.
6. **Audit / Outbox:** manter fracos por padrão (retenção histórica), salvo decisão explícita.

### Ordem sugerida de implementação

| Lote | Entidades | Motivo |
|------|-----------|--------|
| A | Branch, Membership, RolePermission, OrganizationInvite | Núcleo multi-tenant |
| B | UserRoleAssignment, UserClaimAssignment, RoleClaim, MenuItem, ClaimDefinition | AuthZ / menus |
| C | LdapGroupRoleMap, ExternalIdentityProvider (`OrganizationId`), ClientProductBinding (`ProductId`) | Federação / bindings |
| D | UserSession, RevokedSession (`SessionId`/`UserId`) | Sessão (cuidado com histórico) |
| E | `ClientId` string / Audit / Outbox | Decisão + possível ADR |

## Arquivos impactados

| Camada | Caminhos previstos |
|--------|--------------------|
| Data (maps) | `src/SSO.Infrastructures.Data/Identity/EntityMappings/*Map.cs` |
| Data (migration) | `src/SSO.Infrastructures.Data/Identity/Migrations/` (nova) |
| Domain | Nenhum (preferência: sem nav props) |
| Application / API | Nenhum (comportamento de escrita já valida existência em vários fluxos) |
| Tests | Integração: insert órfão deve falhar; regressão CRUD existentes |
| Docs (.ai) | Este bugfix; opcional ADR se `ClientId`/audit permanecerem fracos |

## Testes

- [ ] Script/query de órfãos atuais (pré-migration) — zero ou limpeza documentada
- [ ] Após migration: insert com FK inválida falha no SQL/EF
- [ ] Soft-delete do pai não cascadeia delete físico dos filhos (Restrict)
- [ ] Suíte de integração Identity existente permanece verde
- [ ] (Se lote E) testes documentando ausência intencional de FK em audit/outbox

## Riscos / side effects

| Risco | Mitigação |
|-------|-----------|
| Dados órfãos em ambientes existentes bloqueiam migration | Relatório + script de limpeza/correção antes do `AddForeignKey` |
| Soft-delete + FK Restrict impede “apagar” pai com filhos | Comportamento desejado; Domain já deve impedir ou soft-deletar em cadeia |
| FK para `ClientId` string inviável / frágil com OpenIddict | Tratar lote E à parte; não bloquear lotes A–D |
| Performance em inserts (checks FK) | Negligível vs ganho de integridade; índices já existem em várias colunas |
| RevokedSession/Audit com retenção após sessão/user sumir | Manter fracos ou `SetNull` só onde nullable |

## Decisões abertas

- D-00012-1 — Navigation properties no Domain vs relação só no Infrastructure?
- D-00012-2 — `AuthAuditEvent.UserId` / `WebhookOutbox.ClientId`: fracos intencionais?
- D-00012-3 — Como (ou se) amarrar `ClientId` string a `OpenIddictApplications`?
- D-00012-4 — `RevokedSession.SessionId`: FK rígida ou snapshot histórico após purge de sessão?

## Checklist

- [ ] Causa confirmada (não só sintoma) — **sim** (maps + snapshot)
- [ ] Fix na camada correta — **Data / migration** (planejado)
- [ ] Sem mudança de comportamento colateral não intencional
- [ ] Docs atualizados se a regra ficou explícita
- [ ] Lotes A–D implementados
- [ ] Lote E decidido (ADR ou nota em Decisions.md)
- [ ] Pronto para implementação — **após** alinhamento das decisões abertas e inventário de órfãos no banco alvo
