# Modules

## Context: Default

Único bounded context de scaffold Sample.

### Aggregate: Sample

| Aspecto | Detalhe |
|---------|---------|
| Entidade | `Sample` (`DomainEntity<Guid>`), propriedade `Description` (string, required, max 128) |
| Tabela | `Samples` (schema `DefaultDb`) |
| Aggregate root | Sim (Forge) |

#### API

| Controller | Rota | Verbos |
|------------|------|--------|
| `SamplesController` | `api/default/samples` | GET, GET `{id}`, POST, PUT, PATCH, DELETE |

## Context: Identity (ADR-006 — Fases 0–6)

| Aspecto | Detalhe |
|---------|---------|
| Schema / DbContext | `IdentityDb` / `IdentityDbContext` |
| Conta | Confirm e-mail, forgot/reset, 2FA TOTP (Razor) |
| Auditoria | `AuthAuditEvents` |
| Sessões | Revoke all tokens por subject (`POST .../sessions/{userId}/revoke`) |
| Mail | `IMailService` / logger; capture nos testes |
| IdPs externos | `ExternalIdentityProviders` + OIDC Entra/Google; LDAP stub |
| Hardening | CORS, rate limit, lockout, signing (`Sso:*`); P-004 AutoMigrate |

### Aggregates / entidades

| Aggregate | Tabela | Rotas | Notas |
|-----------|--------|-------|-------|
| Organization | `Organizations` | `api/identity/organizations` | Code único |
| Product | `Products` | `api/identity/products` | Code único |
| Membership | `Memberships` | `api/identity/memberships` | User×Org |
| User | AspNetUsers | `api/identity/users` | IdentityUser; GET/PATCH `me`; GET filter + GET id + POST; `DisplayName` |
| Branch | `Branches` | `api/identity/branches` | `ParentBranchId` estrutural |
| Permission | `Permissions` | `api/identity/permissions` | Code único |
| Role | `AuthRoles` | `api/identity/roles` | Domain role |
| RolePermission | `RolePermissions` | `api/identity/rolepermissions` | Role→Permission |
| UserRoleAssignment | `UserRoleAssignments` | `api/identity/userroleassignments` | Contexto Org/Branch/Product |
| ClientProductBinding | `ClientProductBindings` | `api/identity/clientproductbindings` | client_id → Product |
| ProductEnablement | `ProductEnablements` | `api/identity/productenablements` | Org × Product (comercial; 00013) |
| OrganizationContact | `OrganizationContacts` | `api/identity/organization-contacts` | Contatos da org (00014) |
| AuthAuditEvent | `AuthAuditEvents` | `api/identity/auth-audit-events` | Append-only |
| MenuItem | `MenuItems` | `api/identity/menuitems` + `api/identity/menus/effective` | PermissionCode → UI |
| ExternalIdentityProvider | `ExternalIdentityProviders` | `api/identity/external-identity-providers` | Catálogo; só `IsEnabled` |
| OrganizationInvite | `OrganizationInvites` | `api/identity/organization-invites` | POST + cancel/resend; accept via Account |

### Portal Admin (00011) + Me (00014)

Area `/Admin` — cadastros completos por papel. Area `/Me` — self-service (perfil, empresas). Ver [admin-portal.md](admin-portal.md), [ui-brand.md](ui-brand.md).

### Páginas de conta

| Página | Função |
|--------|--------|
| `/Account/Login` | Senha + lockout + providers externos + redirect 2FA (layout atNodus) |
| `/Account/ExternalLogin` | Challenge/Callback OIDC (Entra/Google) |
| `/Account/LoginWith2fa` | TOTP |
| `/Account/EnableAuthenticator` | Ativar TOTP (autenticado) |
| `/Account/ForgotPassword` / `ResetPassword` | Reset |
| `/Account/ConfirmEmail` | Confirmação |
| `/Me/Profile` | Perfil self-service (00014) |

### Pendente (pós-épico)

SMTP real; ProductEnablement com vigência/Status / billing ref (evolutiva D-00013-4 B/C).

## Serviços de infraestrutura transversais

| Serviço | Interface | Implementação | Status |
|---------|-----------|---------------|--------|
| Mail | `IMailService` | `MailService` (+ `CapturingMailService` em testes) | Ativo (logger MVP) |
| Permissions efetivas | `IEffectivePermissionsResolver` | `EffectivePermissionsResolver` | Ativo (Fase 3) |
| Claims tipadas | `IEffectiveClaimsResolver` / `IClaimPolicyVersionProvider` | `EffectiveClaimsResolver` / `ClaimPolicyVersionProvider` | Ativo (00008) |
| Product enablement | `IProductEnablementGuard` | `ProductEnablementGuard` | Ativo (00013) |
| Auditoria | `IAuthAuditService` | `AuthAuditService` | Ativo (Fase 4) |
| Sessões | `IUserSessionService` | `UserSessionService` | Ativo (Fase 4) |

## Shared

`SSO.Shared/Identity/` — claim types, grant `switch_context`, client ids, `SsoHardeningOptions`, `ExternalIdpTypes`.
