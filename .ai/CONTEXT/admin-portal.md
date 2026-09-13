# Admin portal

> Feature 00003 — F00003-D1/D2/D3 (shell MVP)  
> Feature **00011** — expansão completa dos cadastros (**implementado**)  
> Feature **00014** — layout atNodus + Area `/Me` + perfil (**implementado**)  
> Feature **00016** — navegação aninhada (hubs pai; filhos fora do sidebar)  
> Related: [admin-api-authz.md](admin-api-authz.md), [ui-brand.md](ui-brand.md), ADR-003

## Access

1. Login em `/Account/Login` (`admin@sso.local` / `ChangeMe!123` no Dev) — layout marca atNodus.
2. Self-service: `/Me/Profile` (qualquer usuário autenticado).
3. Admin: `/Admin` (requer `sso.admin.*`).
4. Em **Contexto**, selecionar organização (switch_context server-side na sessão).
5. Navegação Admin por `sso.admin.*` (Org vs Platform). Org Admin entra por **Minha organização**.

## Cadastros (`/Admin`) — hubs (00016)

O sidebar lista só **raízes** e ferramentas. Cadastros dependentes vivem no detalhe do pai (abas / rotas aninhadas). URLs antigas (`/Admin/Branches`, `/Admin/Invites`, `/Admin/MenuItems`, …) redirecionam para o hub.

| Papel | Menu | Dentro do pai |
|-------|------|----------------|
| Org (+ Platform) | Minha organização, Contexto, Sessões (se `sessions.revoke`) | No detalhe da org: Branches, Contatos, Produtos (enablements), Usuários (memberships), Convites, Acessos, Claims; Platform também LDAP e IdPs da org |
| Platform | Organizações, Produtos, Usuários, Roles, Permissões, Auth Clients, Provedores externos (catálogo **global**) | Product: menus, empresas habilitadas, clients, claims; Role: permissões e claims; User: empresas, acessos (incl. platform-scoped), claims, sessões; Auth Client: bindings; Permissão: roles que a concedem (leitura) |
| Audit | Auditoria (`audit.read`) | — |

Claims globais (`ProductId` null): `/Admin/Products/Claims` (sem item de menu). Hub Branch (2º nível) fica fora desta feature (D-00016-7 B).

Páginas orquestram Application (MediatR / `AdminWrap`) ou serviços equivalentes; sem Domain Service direto nas PageModels CQRS. Shell visual: brand kit atNodus (Bootstrap + `atnodus.css`).

## Area `/Me` (00014)

| Página | Função |
|--------|--------|
| `/Me/Profile` | Dados pessoais, empresas, acessos, convites |
| `/Me/Organizations` | Listagem de empresas do usuário (ou todas se Platform) |
| `/Me/Organizations/Details/{id}` | Detalhe empresa (abas Dados · Branches · Contatos · Produtos · Usuários). Abas de operação Admin **não** aparecem aqui (D-00016-6). |

`/Admin/Organizations/Details/{id}` usa a mesma página com abas extras (Convites, Acessos, …) e links para rotas aninhadas `/Admin/Organizations/{id}/Invites` etc.

## Convites (F00003-D2)

- Admin envia convite no hub da empresa (`/Admin/Organizations/{id}/Invites`).
- Convidado abre `/Account/AcceptInvite?token=...`, aceita ou recusa.
- Também pode aceitar/recusar em `/Me/Profile` (aba Convites) quando autenticado com o e-mail do convite.
- **Membership só é criada após aceite.**
- API: `api/identity/organization-invites` (+ `PATCH …/{id}/cancel`, `PATCH …/{id}/resend`).

## Contexto (F00003-D3)

Sessão guarda o resultado do switch; claims `organization_id` / `permissions` são enriquecidas no request do portal a partir do client `sso-admin-api` (equivalente ao grant `switch_context`).
