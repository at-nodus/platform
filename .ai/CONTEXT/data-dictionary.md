# Dicionário de dados

Inventário das tabelas/entidades do SSO Platform e a função de cada uma no domínio.

**Fontes:** `SSO.Core.Domain`, maps EF em `SSO.Infrastructures.Data`, snapshots de migration, [modules.md](modules.md), [glossary.md](glossary.md), [business.md](business.md).

## Visão geral

| Schema / DbContext | Papel |
|--------------------|-------|
| `IdentityDb` / `IdentityDbContext` | Bounded context SSO (ADR-006) — identidade, authz, sessões, OAuth |
| `DefaultDb` / `DefaultDbContext` | Scaffold Sample (Forge); não faz parte do domínio de produto |

**Composição de autorização (negócio):**

```text
User → Organization → Branch → Product → Role → Claims → Permissions
```

Resolvida no contexto ativo do token (`organization_id` / `branch_id`) + Product do AuthClient OAuth.

A maioria das entidades de domínio herda `IdentityAuditableEntity`: `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt`, `IsDeleted` (soft delete).

---

## 1. Núcleo multi-tenant e produtos

| Tabela | Entidade | Função no projeto |
|--------|----------|-------------------|
| `Organizations` | `Organization` | **Tenant.** Isolamento lógico de identidade e autorização. Agrupa branches, memberships, convites e política de herança de authz entre branches (`BranchAuthzInheritance`: Off / InheritFromAncestors). |
| `Branches` | `Branch` | **Filial / unidade** dentro de uma Organization. Hierarquia estrutural via `ParentBranchId`; herança de autorização é opt-in na org (ADR-008), não automática. |
| `Products` | `Product` | **Sistema de negócio** do ecossistema que consome o SSO (≠ AuthClient OAuth). Escopo de menus, roles, claims tipadas e bindings de clientes. |
| `Memberships` | `Membership` | **Vínculo usuário ↔ organização.** Indica que o usuário pertence ao tenant; criado tipicamente ao aceitar um convite. |
| `OrganizationInvites` | `OrganizationInvite` | **Convite para entrar na organização.** Fluxo pending → accept / decline / cancel / expire; guarda e-mail, hash do token, expiração e quem convidou. Aceite cria `Membership`. |
| `OrganizationContacts` | `OrganizationContact` | **Contato da organização** (nome, e-mail, telefone, cargo, flag principal). CRUD na aba Contatos do detalhe da empresa (00014). |

---

## 2. Identidade e conta

| Tabela | Entidade | Função no projeto |
|--------|----------|-------------------|
| `AspNetUsers` | `User` | **Conta SSO.** Extende `IdentityUser<Guid>` (login, senha, lockout, 2FA, e-mail) + `DisplayName` opcional (perfil 00014). Base para memberships, convites, roles, claims e sessões. |
| `AspNetUserClaims` | *(ASP.NET Identity)* | Claims de framework do usuário (não confundir com claims tipadas de domínio). |
| `AspNetUserLogins` | *(ASP.NET Identity)* | Vínculos de login externo (provider key) — ex.: Entra / Google. |
| `AspNetUserRoles` | *(ASP.NET Identity)* | Associação usuário ↔ role **de framework** (`AspNetRoles`). Separada das roles de domínio (`AuthRoles`). |
| `AspNetUserTokens` | *(ASP.NET Identity)* | Tokens de framework (2FA, recovery, etc.). |
| `AspNetRoles` | *(ASP.NET Identity)* | Roles de framework do ASP.NET Identity. |
| `AspNetRoleClaims` | *(ASP.NET Identity)* | Claims de framework ligadas a `AspNetRoles`. |

---

## 3. Autorização (roles, permissions, claims)

| Tabela | Entidade | Função no projeto |
|--------|----------|-------------------|
| `Permissions` | `Permission` | **Capacidade autorizável dinâmica** (código único). Compõe o conjunto efetivo embutido no JWT (`permissions` / `perm_ver` — ADR-005). |
| `AuthRoles` | `Role` | **Role de domínio** (não é `AspNetRoles`). Agrupa permissions e claims tipadas; atribuída em contexto Org / Branch / Product. |
| `RolePermissions` | `RolePermission` | **N:N Role → Permission.** Define quais permissões uma role concede. |
| `UserRoleAssignments` | `UserRoleAssignment` | **Concessão contextual de role** a um usuário. Escopo: `OrganizationId?` (null = plataforma), `BranchId?` (null = org inteira), `ProductId`, flag `Inheritable` (quando herança de branch está ligada). |
| `ClaimDefinitions` | `ClaimDefinition` | **Catálogo de claims tipadas** (`string` / `int` / `bool`), opcionalmente por Product. Emitidas no JWT como `sso_c_{code}` — distintas de Permissions de rota. |
| `UserClaimAssignments` | `UserClaimAssignment` | **Valor de claim tipada por usuário**, no mesmo modelo de escopo Org / Branch / Product das role assignments. |
| `AuthRoleClaims` | `RoleClaim` | **Claim tipada ligada a uma Role.** Herdada via `UserRoleAssignment`. Distinta de `AspNetRoleClaims`. |
| `MenuItems` | `MenuItem` | **Item de menu de UI** de um Product, desbloqueado por `PermissionCode`. Alimenta APIs de menu efetivo. |

---

## 4. Clientes OAuth / OIDC (OpenIddict + sidecars)

| Tabela | Entidade | Função no projeto |
|--------|----------|-------------------|
| `OpenIddictApplications` | *(OpenIddict)* | **AuthClients** registrados (client_id, secrets, redirect URIs, grants). Store oficial OAuth/OIDC. |
| `OpenIddictAuthorizations` | *(OpenIddict)* | Autorizações / registros de consentimento emitidos. |
| `OpenIddictScopes` | *(OpenIddict)* | Escopos OIDC disponíveis. |
| `OpenIddictTokens` | *(OpenIddict)* | Tokens emitidos (access, refresh, etc.). |
| `AuthClientMetadata` | `AuthClientMetadataEntity` | **Metadados admin** do AuthClient (display name, first-party, enablement, política de consent Always/First/Never). Sidecar por `ClientId`; não substitui o store OpenIddict. |
| `ClientProductBindings` | `ClientProductBinding` | **Liga `client_id` OAuth → Product.** Garante que a authz efetiva seja resolvida para o produto correto do cliente. |
| `ProductEnablements` | `ProductEnablement` | **Habilitação comercial Organization × Product.** Pré-condição fail-closed para emitir token com `organization_id` no product do client (exceto `sso-platform` / M2M). |

---

## 5. Sessões, revogação e auditoria

| Tabela | Entidade | Função no projeto |
|--------|----------|-------------------|
| `UserSessions` | `UserSession` | **Sessão SSO vista pelo produto.** `Id` = JWT `sid`; rastreia usuário, client, org/branch ativos, last seen e revogação. |
| `RevokedSessions` | `RevokedSession` | **Deny-list de revogação quente** (ADR-007). Impede uso do access token até o TTL; sem soft delete. |
| `AuthAuditEvents` | `AuthAuditEvent` | **Trilha append-only** de AuthN/AuthZ (login, 2FA, consent, revoke de sessão, etc.): outcome, IP, client, detalhe. |

---

## 6. Federação e LDAP

| Tabela | Entidade | Função no projeto |
|--------|----------|-------------------|
| `ExternalIdentityProviders` | `ExternalIdentityProvider` | **Catálogo de IdPs federados** (Entra / Google / Ldap). Escopo global (`OrganizationId` null) ou por org; controla enablement e JIT provisioning. |
| `LdapGroupRoleMaps` | `LdapGroupRoleMap` | **Mapeamento grupo LDAP → Role SSO** no login LDAP (org + product + branch opcional). |

---

## 7. Webhooks

| Tabela | Entidade | Função no projeto |
|--------|----------|-------------------|
| `ClientWebhookEndpoints` | `ClientWebhookEndpoint` | **Endpoint de webhook por AuthClient** (URL + HMAC + enablement), ex.: notificação de sessão revogada. |
| `WebhookOutbox` | `WebhookOutboxMessage` | **Outbox durável** de entrega de webhooks (`session.revoked`, etc.) com status, tentativas e erro. |

---

## 8. Scaffold Default

| Tabela | Entidade | Função no projeto |
|--------|----------|-------------------|
| `Samples` | `Sample` | Aggregate de **exemplo do Forge** (`Description`). Não participa do domínio SSO de produto; mantido até o context Default ser aposentado (ADR-006). |

---

## Nomes que diferem (classe ≠ tabela)

| Classe C# | Tabela |
|-----------|--------|
| `User` | `AspNetUsers` |
| `Role` | `AuthRoles` |
| `RoleClaim` | `AuthRoleClaims` |
| `AuthClientMetadataEntity` | `AuthClientMetadata` |
| `WebhookOutboxMessage` | `WebhookOutbox` |

---

## Valores de domínio (não são tabelas)

Constantes usadas como status/política em colunas string:

| Constante | Valores típicos | Usado em |
|-----------|-----------------|----------|
| `OrganizationInviteStatuses` | Pending, Accepted, Declined, Cancelled, Expired | `OrganizationInvite.Status` |
| `BranchAuthzInheritancePolicies` | Off, InheritFromAncestors | `Organization.BranchAuthzInheritance` |
| `AuthClientConsentPolicies` | Always, First, Never | `AuthClientMetadata.RequireConsent` |
| `ClaimValueTypes` | string, int, bool | `ClaimDefinition.ValueType` |
| `ExternalIdpTypes` | Entra, Google, Ldap | `ExternalIdentityProvider.ProviderType` |
| `WebhookOutboxStatuses` | pending, delivered, failed | `WebhookOutbox.Status` |

---

## Contagem

| Bucket | Qtd. tabelas |
|--------|--------------|
| Domínio Identity (incl. `AspNetUsers`) | 23 |
| ASP.NET Identity (exceto Users) | 6 |
| OpenIddict | 4 |
| Default (`Samples`) | 1 |
| **Total** | **34** |

Ao adicionar aggregate novo: atualizar este arquivo, [modules.md](modules.md) e, se houver termo novo, [glossary.md](glossary.md).
