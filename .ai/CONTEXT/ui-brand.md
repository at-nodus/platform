# UI brand — atNodus

> Feature **00014** — layout padronizado + Area `/Me`  
> Fonte: `visual-identity/brands/at-nodus/` (templates, fonts, logos)

## Assets no host

| Item | Caminho |
|------|---------|
| Tema CSS | `wwwroot/css/atnodus.css` |
| JS (hash→tab, uploads) | `wwwroot/js/atnodus.js` |
| Fontes Play / Ubuntu | `wwwroot/fonts/` (self-host, D-00014-6) |
| Logos / favicon | `wwwroot/img/brand/*.svg` |
| Partials | `Pages/Shared/_BrandHead.cshtml`, `_BrandScripts.cshtml` |

Bootstrap **5.3.3** + Font Awesome **6.5.2** via CDN com SRI.

## Areas

| Area | Rota | Gate | Uso |
|------|------|------|-----|
| Account | `/Account/*` | público / cookie | Login (layout marca), consent, 2FA, invites |
| Admin | `/Admin/*` | cookie + qualquer `sso.admin.*` | Operação admin (shell marca) |
| **Me** | `/Me/*` | cookie autenticado | Self-service: perfil, empresas, detalhe org |

## Perfil (`/Me/Profile`)

Abas: Dados (self-edit DisplayName/Phone; e-mail **imutável**) · Empresas · Acessos · Convites (accept/decline).

## Empresa — duas rotas (D-00014-3)

Mesma página `Areas/Me/Pages/Organizations/Details`:

- `/Me/Organizations/Details/{id}`
- `/Admin/Organizations/Details/{id}` (`AddAreaPageRoute`)

CTAs de escrita gated por `sso.admin.*` / Platform.

## Contatos

Aggregate `OrganizationContact` — API `api/identity/organization-contacts` + aba Contatos no detalhe da empresa.
