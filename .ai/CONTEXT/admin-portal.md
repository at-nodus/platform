# Admin portal

> Feature 00003 — F00003-D1/D2/D3 (shell MVP)  
> Feature **00011** — expansão completa dos cadastros (**implementado**)  
> Feature **00014** — layout atNodus + Area `/Me` + perfil (**implementado**)  
> Related: [admin-api-authz.md](admin-api-authz.md), [ui-brand.md](ui-brand.md), ADR-003

## Access

1. Login em `/Account/Login` (`admin@sso.local` / `ChangeMe!123` no Dev) — layout marca atNodus.
2. Self-service: `/Me/Profile` (qualquer usuário autenticado).
3. Admin: `/Admin` (requer `sso.admin.*`).
4. Em **Contexto**, selecionar organização (switch_context server-side na sessão).
5. Navegação Admin por `sso.admin.*` (Org vs Platform).

## Cadastros (`/Admin`)

Páginas orquestram Application (MediatR / `AdminWrap`) ou serviços equivalentes; sem Domain Service direto nas PageModels CQRS. Shell visual: brand kit atNodus (Bootstrap + `atnodus.css`).

| Papel | Páginas |
|-------|---------|
| Org (+ Platform) | Branches, Invites (+ resend/cancel), Memberships (list/remove), UserRoleAssignments, UserClaimAssignments, ProductEnablements (**GET** / read-only), Sessions (`sessions.revoke`) |
| Platform | Organizations (+ link detalhe `/Admin/Organizations/Details/{id}`), Products, Permissions, Roles, RolePermissions, ClientProductBindings, ProductEnablements (CRUD), AuthClients, ExternalIdPs, ClaimDefinitions, RoleClaims, Users, LdapMaps, MenuItems (`menus`) |
| Audit | Audit (`audit.read`) |

## Area `/Me` (00014)

| Página | Função |
|--------|--------|
| `/Me/Profile` | Dados pessoais, empresas, acessos, convites |
| `/Me/Organizations` | Listagem de empresas do usuário (ou todas se Platform) |
| `/Me/Organizations/Details/{id}` | Detalhe empresa (abas); mesma UI em `/Admin/Organizations/Details/{id}` |

## Convites (F00003-D2)

- Admin envia convite por e-mail (`/Admin/Invites`).
- Convidado abre `/Account/AcceptInvite?token=...`, aceita ou recusa.
- Também pode aceitar/recusar em `/Me/Profile` (aba Convites) quando autenticado com o e-mail do convite.
- **Membership só é criada após aceite.**
- API: `api/identity/organization-invites` (+ `PATCH …/{id}/cancel`, `PATCH …/{id}/resend`).

## Contexto (F00003-D3)

Sessão guarda o resultado do switch; claims `organization_id` / `permissions` são enriquecidas no request do portal a partir do client `sso-admin-api` (equivalente ao grant `switch_context`).
