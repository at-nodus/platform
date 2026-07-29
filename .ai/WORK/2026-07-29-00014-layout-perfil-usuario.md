# Feature Plan — 00014 Customização e padronização do layout + Perfil do Usuário

> Arquivo: `.ai/WORK/2026-07-29-00014-layout-perfil-usuario.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Planejamento** — decisões a fechar antes da implementação  
> Data: 2026-07-29  
> Depende de: 00003 (shell `/Admin` + convites), 00011 (cadastros Admin / orgs-branches), 00013 (ProductEnablement na visão de empresa)  
> Relaciona: 00001 (User/Membership/Invite), identidade visual `visual-identity/brands/at-nodus`  
> Fonte de UI: `F:/DEV/cursor/at-nodus/visual-identity/brands/at-nodus/templates/`

## Objetivo

1. **Padronizar o layout** do portal SSO (login, shell autenticado, listagens, formulários, detalhe de empresa e perfil) conforme os templates e tokens da marca **atNodus**.
2. Entregar a tela de **Perfil do Usuário** (self-service), alinhada a `user.html`, para o usuário gerenciar **apenas os próprios dados** e consultar vínculos, acessos e convites — com ações condicionadas às permissões.

## Contexto

- Templates estáticos de referência já existem em `visual-identity/brands/at-nodus/templates/`:

| Template | Uso no produto |
|----------|----------------|
| `login.html` | Login / Account |
| `index.html` | Dashboard / início |
| `listing.html` | Listagem de empresas |
| `company.html` | Perfil/visualização da empresa (abas) |
| `form.html` | Cadastro/edição (empresa/branch) |
| `user.html` | Perfil do usuário (abas) |
| `assets/atnodus.css` + `atnodus.js` | Tema Bootstrap + marca; hash→tab |
| `../fonts/` (Play, Ubuntu) + `../logo/svg/` | Tipografia e assets |

- Stack dos templates: **Bootstrap 5.3** + **Font Awesome 6** + CSS de marca (tokens `--ink`, `--primary`, sidebar escura, topbar clara).
- Portal atual (`Areas/Admin`) e páginas `/Account/*` ainda não seguem esse design system; 00003 deixou **self-service de perfil** fora de escopo.
- APIs e domínio já cobrem grande parte do comportamento (User, Membership, Branch, OrganizationInvite accept/decline, UserRoleAssignment, ProductEnablement). O gap principal é **UI padronizada + composição do perfil**.

```text
Brand kit (fonte da verdade visual)
  visual-identity/brands/at-nodus/
    README.md          → tokens, tipografia, princípios
    templates/         → shell, páginas, fluxos
         ↓ portar
SSO.Web.Api
  wwwroot/css|fonts|img  + Layouts Razor (Account + Area autenticada)
  Páginas: Login, shell, listing, company, form, profile (user)
```

## Decisões abertas (bloquear código se não fechadas)

### D-00014-1 — Escopo de superfícies a reestilizar

| Opção | Descrição |
|-------|-----------|
| **A (sugerida)** | Account (login/consent/2FA/sessions) + shell autenticado do portal + páginas já existentes `/Admin` + novas páginas de perfil/empresa no padrão template |
| B | Só Account + Perfil do Usuário; Admin fica no layout atual |
| C | White-label multi-tenant (temas por org) | Fora — brand kit é atNodus único neste MVP |

### D-00014-2 — Onde vive o “app shell” do usuário final

| Opção | Descrição |
|-------|-----------|
| **A (sugerida)** | Area Razor dedicada (ex.: `/App` ou `/Portal`) com shell dos templates; `/Admin` continua para operação admin, mas **compartilha** CSS/partials de marca |
| B | Reusar só `/Admin` e embutir “Meu perfil” lá | Mistura papéis admin × self-service |
| C | SPA front separado | Fora — stack do produto é Razor (D-00003-1 / D-00011-6) |

### D-00014-3 — Navegação Empresa a partir do Perfil

Ao abrir uma empresa na aba **Empresas**, reutilizar a **mesma tela** de visualização da empresa (`company.html` → página Razor equivalente), com abas:

- Dados Básicos  
- Branches (Filiais)  
- Contatos  
- Produtos Associados  
- Usuários  

Ações (criar/editar branch, editar empresa, etc.) **somente** se o usuário tiver as permissions `sso.admin.*` (ou equivalentes) no contexto daquela org — senão, somente leitura.

| Opção | Descrição |
|-------|-----------|
| **A (sugerida)** | Uma página `Organization/Details` (ou `Companies/Details`) compartilhada entre listagem e deep-link do perfil; botões gated por permission |
| B | Duplicar UI read-only no perfil vs CRUD no Admin | Drift visual e de comportamento |

### D-00014-4 — Contatos da empresa

Templates mostram aba Contato; modelo Identity atual pode não ter aggregate `OrganizationContact` dedicado.

| Opção | Descrição |
|-------|-----------|
| **A (sugerida)** | MVP: exibir campos de contato já existentes na Organization (e-mail/telefone/endereço se houver) ou seção read-only “em breve” se não existirem — **sem** novo aggregate nesta feature |
| B | Criar aggregate Contatos nesta feature | Aumenta escopo (Domain/API/migration) |
| C | Omite a aba Contatos até feature de cadastro | Divergência do `company.html` |

### D-00014-5 — Edição de dados pessoais

| Opção | Descrição |
|-------|-----------|
| **A (sugerida)** | Usuário autenticado edita **somente o próprio** User (nome, telefone, preferências de idioma/fuso se existirem no modelo); e-mail com fluxo de confirmação se alteração for permitida; senha via fluxo Account já existente |
| B | Qualquer campo Identity sem restrição de e-mail | Risco de account takeover |
| C | Só leitura dos dados + “Alterar senha” | Não atende o requisito de edição |

### D-00014-6 — Assets (CDN vs self-host)

| Opção | Descrição |
|-------|-----------|
| **A (sugerida)** | Self-host fontes Play/Ubuntu + logos SVG no `wwwroot`; Bootstrap/FA via libman/npm ou CDN com SRI (como nos templates) |
| B | Tudo CDN | Dependência de rede em runtime |
| C | Bundle custom sem Bootstrap | Reescrever templates — custo alto |

## Escopo

### Inclui

#### A — Design system / layout

- Portar tokens e shell dos templates para o host `SSO.Web.Api` (`atnodus.css`, partials de sidebar/topbar/offcanvas, tipografia, logos).
- Aplicar padrão visual a:
  - Login e fluxos Account relevantes;
  - Shell autenticado (sidebar Plataforma + Conta);
  - Listagem de empresas (`listing.html`);
  - Detalhe/formulário de empresa e branch (`company.html`, `form.html`);
  - Perfil do usuário (`user.html`).
- Comportamentos de UI dos templates: tabs Bootstrap, hash → aba (`atnodus.js`), badges soft, botões primary/outline conforme brand kit.
- Documentar no CONTEXT (`admin-portal.md` ou `ui-brand.md`) a origem dos assets e regras de uso.

#### B — Perfil do Usuário

Tela **Meu perfil** com cabeçalho (avatar/iniciais, nome, e-mail, status, resumo de vínculos/convites) e abas:

##### 1. Dados Pessoais

- Exibir dados cadastrais do usuário autenticado.
- Permitir **apenas** edição dos **próprios** dados (D-00014-5).
- Ação “Alterar senha” reutiliza fluxo Account existente.

##### 2. Empresas

- Listar organizações às quais o usuário está vinculado (memberships ativos).
- **Somente visualização** na lista; ao abrir, navega para a tela de visualização da empresa (mesma de `company.html`).
- Na tela da empresa, abas e ações respeitam permissões:
  - **Branches:** criar / editar / visualizar se houver permission; senão, só o que for permitido (tipicamente view).
  - **Dados da empresa:** editar se houver permission de edição da org; senão read-only.
  - **Produtos Associados:** read conforme ProductEnablement + permissions de visualização.
  - **Usuários:** listagem conforme permissions de membership/users da org.
  - **Contatos:** conforme D-00014-4.

##### 3. Acessos

- Listar produtos aos quais o usuário possui acesso (via assignments / enablement + contexto).
- Para cada produto × empresa (e branch quando aplicável), exibir **roles** associadas ao usuário.
- Somente leitura nesta aba (gestão de roles continua no Admin / assignments).

##### 4. Convites

- Listar convites **recebidos** pelo usuário (e-mail do User logado).
- Campos: Empresa; Filial (quando aplicável); Data de envio; Status (`Pending`, `Accepted`, `Declined`, `Expired`, outros previstos no domínio).
- Se `Pending` (e não expirado): ações **Aceitar** e **Recusar** (reusar `PatchAccept` / `PatchDecline` + Domain Services já existentes).
- Demais status: somente visualização.

Todas as ações e CTAs ocultos/desabilitados quando a permission ou o estado do domínio não autorizar (UI gate + API já enforce).

### Fora de escopo

- White-label / temas por organização.
- SPA / Blazor / redesign com outra stack além de Razor + Bootstrap.
- Novo motor de billing ou marketplace.
- Workflows joiner/mover/leaver avançados além de accept/decline de convite.
- Admin de terceiros editando o perfil “como se fosse o usuário” nesta tela (isso permanece em `/Admin/Users`).
- Aggregate completo de Contatos (salvo se D-00014-4 = B for aceito).
- Dark mode completo além do previsto no brand kit (pode ficar evolutiva).

## Abordagem — fases

### Fase A — Fundação visual

1. Copiar/adaptar `atnodus.css`, fontes, logos para `wwwroot`.
2. Layouts compartilhados: `_BrandLayout` / partials sidebar + topbar + mobile offcanvas.
3. Aplicar ao Login e ao shell autenticado (smoke visual).

### Fase B — Páginas de shell e cadastro visual

1. Listagem de empresas no padrão `listing.html`.
2. Detalhe de empresa (`company.html`) com abas e gating de botões.
3. Formulário branch/empresa (`form.html`) ligado aos Commands/APIs existentes.

### Fase C — Perfil do Usuário

1. Página Perfil com 4 abas + hash routing.
2. Queries “me”: dados pessoais, memberships/orgs, assignments/acessos, invites do e-mail.
3. Commands: update self; accept/decline invite (já existem — wire UI).
4. Deep-link Empresa → detalhe compartilhado (Fase B).

### Fase D — AuthZ UI + docs

1. Esconder CTAs sem permission; garantir que APIs continuam fonte de verdade.
2. Atualizar `admin-portal.md`, `modules.md`, backlog; checklist visual vs brand kit.
3. Testes manuais E2E por papel (usuário comum, org admin, platform admin).

## APIs / Application (previsto)

Preferir endpoints **“me”** (usuário autenticado) em vez de expor filtros admin genéricos no self-service.

| Necessidade | Caminho sugerido | Notas |
|-------------|------------------|-------|
| Ler/atualizar próprio User | `GET/PUT` ou `Patch` `api/identity/users/me` (ou Account + Application) | Só self; sem `sso.admin.users` |
| Empresas vinculadas | Query memberships do user atual | Já há Memberships; filtro `userId = me` |
| Acessos (product × roles) | Query assignments do user (+ product/org/branch) | Read-only |
| Convites recebidos | Filter invites por e-mail do user | Accept/Decline já em Account/Commands |
| Org detalhe / branches | APIs Identity existentes | Permissions `sso.admin.*` |

Sem novas regras de domínio nos PageModels: orquestração via MediatR/API (padrão 00011).

## Critérios de aceite

- [ ] Login e shell autenticado usam tipografia/cores/logos do brand kit atNodus (tokens alinhados ao README da marca).
- [ ] Páginas listagem, empresa, formulário e perfil correspondem estruturalmente aos templates (`listing` / `company` / `form` / `user`).
- [ ] Usuário acessa **Meu perfil** e vê as 4 abas: Dados, Empresas, Acessos, Convites.
- [ ] Edita apenas os próprios dados pessoais; não edita dados de outro usuário por essa tela.
- [ ] Aba Empresas lista só orgs com vínculo; abrir empresa mostra a mesma UX de `company.html`.
- [ ] Branches: criar/editar só com permission; sem permission, view (ou oculto) conforme regra.
- [ ] Com permission de edição da org, editar dados da empresa funciona como na tela de empresa.
- [ ] Aba Acessos lista products + roles por empresa/produto (e branch quando houver).
- [ ] Aba Convites mostra status corretos; Pending permite Aceitar/Recusar; demais não.
- [ ] Accept/Decline atualizam membership/status conforme Domain já implementado.
- [ ] Usuário sem `sso.admin.*` não vê CTAs admin; APIs continuam 403 se chamado direto.
- [ ] CONTEXT/docs atualizados; backlog marca 00014.

## Arquivos impactados (previsto)

| Camada | Caminhos |
|--------|----------|
| Brand (fonte) | `visual-identity/brands/at-nodus/templates/**`, `fonts/**`, `logo/svg/**` |
| Web wwwroot | `SSO.Web.Api/wwwroot/css/atnodus.css`, `fonts/`, `img/brand/` |
| Web Layouts | `Pages/Shared/_*.cshtml`, `Areas/*/Pages/_Layout.cshtml` |
| Web Account | `Pages/Account/Login*.cshtml` (+ consent/2fa polish se no escopo D1=A) |
| Web Portal/Profile | `Areas/.../Profile.cshtml`, `Organizations/Details`, listing, forms |
| Application | Queries/Commands `users/me`, filtros invites/memberships/assignments “me” se faltarem |
| API | Controllers/endpoints self-service mínimos |
| Tests | Integration self-update; invite accept from profile; negative cross-user edit |
| Docs | este plano; `admin-portal.md`; opcional `ui-brand.md`; backlog |

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Escopo visual explode (todas páginas Admin de uma vez) | Fase A–B com checklist de páginas; polish Admin incremental após shell |
| Drift template HTML × Razor | Partials reutilizáveis; comparar abas/campos com `user.html` / `company.html` no aceite |
| Contatos sem modelo | Fechar D-00014-4 antes da Fase B |
| Vazamento de dados de outras orgs no perfil | Queries sempre escopadas ao `userId`/e-mail autenticado |
| CTAs sem permission aparentes | Gate UI + AuthZ API (00002) |
| CDN fora do ar | Preferir self-host fontes/logos (D-00014-6 A) |

## Estratégia de testes

- [ ] Visual/manual: checklist brand (Play/Ubuntu, primary `#2D7CFF`, sidebar ink, badges soft)
- [ ] Integration: `PATCH/PUT me` só altera o próprio user; 403/404 cross-user
- [ ] Integration: list invites by me; accept/decline Pending; rejeita não-Pending
- [ ] Integration: org details buttons — 403 sem permission nas APIs de escrita
- [ ] Regression: fluxos Admin existentes não quebram após troca de layout
- [ ] Smoke: hash tabs `#empresas`, `#acessos`, `#convites`, `#branches`

## Estimativa para orçamento (ordem de grandeza)

| Fatia | Conteúdo | Complexidade relativa |
|-------|----------|----------------------|
| A | CSS/fonts/logos + layouts Account/shell | M |
| B | Listing + company + form no padrão | M/L |
| C | Perfil 4 abas + wire APIs me/invites | M |
| D | Gating + docs + polish | S |
| **MVP (D1–D6 sugeridos A)** | A+B+C+D | **~1 feature média/grande (UI-heavy)** |
| Evolutiva | Contatos aggregate, dark mode completo, white-label | Feature(s) separada(s) |

Não é compromisso de prazo em dias — serve para priorizar no backlog.

## Checklist

- [ ] D-00014-1..6 fechadas
- [ ] Alinhado a PLAYBOOK/architecture.md + domain-rules (UI só orquestra)
- [ ] Naming HTTP verbs nos Commands novos (`Patch`/`Put` self-profile)
- [ ] AuthZ self vs admin considerado
- [ ] Migrations só se D-00014-4 = B
- [ ] CONTEXT + backlog atualizados
- [ ] Pronto para implementação

## Referências rápidas

- Brand: `visual-identity/brands/at-nodus/README.md`
- Templates: `visual-identity/brands/at-nodus/templates/README.md`
- Portal atual: `.ai/CONTEXT/admin-portal.md`
- Convites (domínio): 00003 — Accept/Decline
- Cadastros Admin: 00011
- Product enablement (aba Produtos): 00013
