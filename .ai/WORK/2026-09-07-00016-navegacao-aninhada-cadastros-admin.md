# Feature Plan — 00016 Navegação aninhada dos cadastros Admin

> Arquivo: `.ai/WORK/2026-09-07-00016-navegacao-aninhada-cadastros-admin.md`  
> Template: `.ai/TEMPLATES/feature-plan.md`  
> Status: **Implementado** — D-00016-1..8 **aceitas** (2026-09-07): A / A / A / A / A / A / B / A  
> Data: 2026-09-07  
> Depende de: **00011** (cadastros Admin), **00014** (detalhe da empresa com abas + Area `/Me`)  
> Relaciona: 00003 (shell/menu por permission), 00013 (ProductEnablement na aba Produtos)  
> Não altera: Domain aggregates, FKs, APIs `api/identity/*` (salvo gap de filtro, se aparecer na implementação)

## Objetivo

Trocar a informação architecture **plana** do portal (cada cadastro = item de menu) por **hubs pai** com dependentes **dentro** da tela do pai: cabeçalho/formulário da entidade + abas ou listas dos cadastros filhos.

O menu passa a expor só **raízes** (catálogos e ferramentas). Cadastro que só existe em função de outro deixa de ser entrada de primeiro nível.

Não criar aggregates novos. Reusar o padrão já iniciado em `/Admin/Organizations/Details/{id}` e `/Me/Organizations/Details/{id}` (00014 / template `company.html`).

## Contexto

Hoje o sidebar Admin lista ~20 cadastros lado a lado (`_Layout.cshtml`): Filiais, Convites, Memberships, Atribuições, Claims de usuário, Habilitações, Organizações, Produtos, Permissões, Roles, Permissões por role, Bindings, Auth Clients, Provedores, Definições de claim, Claims por role, Usuários, LDAP, Itens de menu, Sessões, Auditoria.

Isso ignora o modelo de negócio (`data-dictionary.md`):

```text
User → Organization → Branch → Product → Role → Claims → Permissions
```

A 00014 **já alinhou a empresa** ao template `company.html` (abas Dados · Branches · Contatos · Produtos · Usuários), mas:

1. As páginas irmãs da 00011 **continuam no menu** (duplicata: `/Admin/Branches` vs aba Branches).
2. Filhos da org **não** foram para o detalhe: Convites, Atribuições, Claims de usuário, LDAP, IdPs por org, Sessões.
3. Catálogos **Product / Role / User / AuthClient** não têm hub equivalente — só list+form na mesma página, estilo 00011.
4. Org Admin **não** tem item Organizações: opera via Contexto + listas globais filtradas pelo `switch_context`. Se as listas saírem do menu, precisa de entrada “Minha organização”.
5. `Details.cshtml` / `Details.cshtml.cs` (~500 linhas) já misturam org + branch + contato + enablement + membership. Expandir o mesmo PageModel para o restante **não escala**.

Referências: [admin-portal.md](../CONTEXT/admin-portal.md), [ui-brand.md](../CONTEXT/ui-brand.md), [00011](2026-07-20-00011-expansao-cadastros-admin.md), [00014](2026-07-29-00014-layout-perfil-usuario.md), template `visual-identity/brands/at-nodus/templates/company.html`.

---

## 1. Diagnóstico da IA atual

### Como as telas são exibidas hoje

| Padrão | Onde | Comportamento |
|--------|------|----------------|
| Lista + formulário na mesma página | Quase todos `/Admin/{Entity}` (ex.: `Roles.cshtml`) | Menu direto; `EditId` query; sem breadcrumb de pai |
| Detalhe com abas (hub) | `Areas/Me/Pages/Organizations/Details` (também rota Admin) | Pai visível; filhos parciais; hash/tab |
| Perfil com abas | `/Me/Profile` | Self-service; não é Admin |
| Ferramenta | SwitchContext, Audit, Sessions | Não são cadastro filho |

Org Admin (`sso.admin.org`): Filiais, Convites, Memberships, Atribuições, Claims, Habilitações — **sempre no menu**, escopo = org do token (`RequireOrgContext` em Branches).

Platform Admin: o mesmo + catálogos globais.

### Duplicidade já existente (org)

| Filho | No hub Org Details | Página solta no menu |
|-------|--------------------|----------------------|
| Branch | Aba + create/edit inline | `/Admin/Branches` (CRUD completo, inclusive `ParentBranchId`) |
| Membership | Aba Usuários (lista + roles, **sem remove**) | `/Admin/Memberships` (list/remove) |
| ProductEnablement | Aba Produtos (**só leitura** do nome/código) | `/Admin/ProductEnablements` (CRUD) |
| OrganizationContact | Aba Contatos | **Não** tem página solta (correto) |
| OrganizationInvite | — | `/Admin/Invites` |
| UserRoleAssignment | Só nomes de role na aba Usuários | `/Admin/UserRoleAssignments` |
| UserClaimAssignment | — | `/Admin/UserClaimAssignments` |

Conclusão: o usuário já “abre o pai e vê filhos” **só na empresa, e pela metade**. O pedido desta feature é generalizar isso e **tirar os filhos do menu**.

---

## 2. Mapa pai → filho (todas as telas Admin)

Classificação pela **dependência estrutural** (FK / sidecar), não pelo menu atual.

### 2.1 Hubs raiz (permanecem no menu)

| Hub | Por que é raiz | Filhos a absorver |
|-----|----------------|-------------------|
| **Organizações** | Tenant | Branches, Contatos, Memberships, Convites, ProductEnablements, assignments org-scoped, IdPs da org, LDAP maps, (opcional) sessões da org |
| **Produtos** | Catálogo global (sem `OrganizationId`) | MenuItems, ClientProductBindings, ClaimDefinitions com `ProductId`, enablements **como visão inversa** (orgs que habilitam) |
| **Usuários** | Conta SSO | Memberships, assignments, user claims, sessões do usuário |
| **Roles** | Catálogo global (sem `ProductId`) | RolePermissions, RoleClaims; assignments que usam a role (lista) |
| **Permissões** | Catálogo global | RolePermissions **inversos** (quais roles concedem); MenuItems por `PermissionCode` (leitura) |
| **Auth Clients** | OpenIddict + sidecar | Bindings, metadados/lifecycle; webhooks **fora** (00011) |
| **Provedores externos** | Catálogo (global **ou** por org) | LDAP maps se IdP = Ldap; maps também cabem na org |

### 2.2 Ferramentas (permanecem no menu, não são cadastro)

| Tela | Motivo |
|------|--------|
| Início | Dashboard |
| Contexto | `switch_context` (F00003-D3) — pré-condição operacional |
| Sessões | Console de revoke (permission própria `sso.admin.sessions.revoke`); também aparece no hub User |
| Auditoria | Append-only, sem pai de cadastro |

### 2.3 Dependentes (saem do menu; vivem no pai)

| Tela atual | Pai primário proposto | Pai secundário (visão inversa, sem create obrigatório) |
|------------|----------------------|--------------------------------------------------------|
| Branches | Organization | — (2º nível: hub Branch, ver D-00016-7) |
| Invites | Organization | User (convites daquele e-mail, se útil) |
| Memberships | Organization | User |
| ProductEnablements | Organization | Product |
| UserRoleAssignments | Organization **ou** User (D-00016-3) | Role, Branch |
| UserClaimAssignments | Organization **ou** User | ClaimDefinition, Branch |
| RolePermissions | Role | Permission |
| RoleClaims | Role | ClaimDefinition |
| ClientProductBindings | AuthClient | Product |
| MenuItems | Product | Permission (leitura) |
| ClaimDefinitions | Product se `ProductId` set; senão catálogo no Product “global” ou tela Permissões/Claims | — |
| LdapMaps | Organization | Role, Branch, ExternalIdP |
| ExternalIdPs com `OrganizationId` | Organization | Catálogo global só para IdPs `OrganizationId` null |

### 2.4 O que **não** alinha bem (impedimentos de modelo)

Estes **não** são bloqueio absoluto; exigem regra de “hub primário” e, às vezes, uma lista residual.

| Entidade | Impedimento | Mitigação proposta |
|----------|-------------|-------------------|
| **UserRoleAssignment / UserClaimAssignment** | N pais: User × Role × Product × Org? × Branch?. `OrganizationId` **null** = platform-scoped (F00002-D2) | Hub de **escrita** = User (a pessoa recebe o acesso) **e** lista filtrada na Org/Branch. Platform-scoped: só no hub User (e opcionalmente Roles), **não** órfão no menu |
| **ProductEnablement** | Junção Org × Product | Escrita no hub **Organization** (decisão comercial da empresa). Product Details mostra orgs habilitadas |
| **ClientProductBinding** | Junção Client × Product | Escrita no hub **AuthClient**. Product mostra clients ligados |
| **RolePermission** | Junção Role × Permission | Escrita no hub **Role** (a role “contém” permissões). Permission mostra roles que a usam |
| **Membership** | User × Org; create **proibido** na UI (D-00003-2 / D-00011-3) | Lista/remove na Org; no User só leitura dos vínculos. Create continua só via convite |
| **ExternalIdentityProvider** | Pode ser global (`OrganizationId` null) ou da org | Dois lugares: catálogo Platform (globais) + aba na Org (os daquela org). **Não** forçar todos para dentro da org |
| **ClaimDefinition** | `ProductId` opcional (null = global) | Aba no Product quando scoped; globais no hub Product via filtro “sem produto” **ou** permanecer como sub-aba de um hub “Catálogo de claims” sob Produtos — **não** item de menu irmão |
| **Branch** | Filho da org, mas também pai de assignments/sessões/LDAP | 1º nível: lista na org. 2º nível: Details da branch (D-00016-7). Sem item de menu “Filiais” |
| **UserSession** | Ops + filho de User (e org/client) | Console `/Admin/Sessions` permanece (revoke em massa). Hub User: sessões daquela conta |
| **AuthAuditEvent** | Sem pai de cadastro | Fica ferramenta |
| **MenuItem** | Filho de Product; `PermissionCode` sem FK (D-00015-8) | Só no Product. Não confundir com o **sidebar Admin** (este é hardcoded, não `MenuItems`) |
| **/Me vs /Admin** | Mesmo PageModel de Org Details (D-00014-3) | Abas/CTAs extras **só** se `IsAdminRoute` + permission. `/Me` não vira segundo Admin |

### 2.5 Telas que **já estão** no padrão alvo

| Tela | Estado | Gap para 00016 |
|------|--------|----------------|
| Org Details | Abas 00014 | Completar filhos; extrair composição; Org Admin entrar aqui |
| `/Me/Profile` | Abas Dados/Empresas/Acessos/Convites | Fora (self-service). Hub Admin User pode **espelhar** o layout, com ações de admin |
| Contatos | Só no pai | Manter |

---

## 3. Menu alvo (depois da feature)

```text
Admin
├── Início
├── Contexto                         (ferramenta)
├── Minha organização                (Org Admin: Details do token; Platform: opcional atalho)
├── Organizações                     (Platform: listagem → Details)
├── Produtos                         (Platform: listagem → Details)
├── Usuários                         (Platform)
├── Roles                            (Platform)
├── Permissões                       (Platform)
├── Auth Clients                     (Platform)
├── Provedores externos              (Platform — só catálogo global)
├── Sessões                          (se sso.admin.sessions.revoke)
└── Auditoria                        (se sso.admin.audit.read)

Conta
├── Meu perfil
└── Sair
```

Itens **removidos do sidebar** (rotas internas / redirect): Branches, Invites, Memberships, UserRoleAssignments, UserClaimAssignments, ProductEnablements, RolePermissions, RoleClaims, ClientProductBindings, MenuItems, LdapMaps, ClaimDefinitions.

Offcanvas mobile hoje está **desatualizado** vs desktop (só um subconjunto). A 00016 deve **unificar** a nav (partial compartilhada) para não regressar o mobile.

---

## 4. Fluxos alvo (como o usuário opera)

### Organização

1. Listagem `/Admin/Organizations` (ou “Minha organização”).
2. Details: formulário/dados do pai no topo.
3. Abas de filhos: Branches · Contatos · Produtos (enablements **com CRUD**) · Usuários (memberships + remove) · Convites · Acessos (assignments) · Claims · LDAP · IdPs da org.
4. Clicar uma **branch** abre o hub da unidade (2º nível), não uma página irmã no menu.

### Produto

1. Listagem `/Admin/Products`.
2. Details: dados do produto.
3. Abas: Menus · Bindings (visão) · Enablements (orgs) · Claims do produto.

### Role

1. Listagem `/Admin/Roles`.
2. Details: código/nome.
3. Abas: Permissões · Claims da role · (opcional) quem está atribuído.

### Usuário (Admin)

1. Listagem `/Admin/Users`.
2. Details: conta + abas Memberships · Acessos · Claims · Sessões. Create de usuário permanece na listagem (não depende de pai).

### Auth Client

1. Listagem `/Admin/AuthClients`.
2. Details: lifecycle 00007 + aba Bindings.

---

## 5. Decisões (propostas — não implementar até aceite)

### D-00016-1 — Escopo = UI / IA do portal — **Aceito: A** (2026-09-07)

Sem novos aggregates, sem migration, sem mudar contratos `api/identity/*`. PageModels continuam orquestrando MediatR / Application (D-00011-2). Filtro por pai: queries já usam ModelWrapper `FullSearch` **ou** o hub filtra no reader como o Org Details já faz.

| Opção | Descrição |
|-------|-----------|
| **A (recomendada)** | Só Razor/nav/composição; APIs intactas |
| B | Também redesenhar APIs “by parent” (`GET .../organizations/{id}/branches`) |
| C | Gerar menus Admin a partir de `MenuItems` | Fora — `MenuItems` é catálogo **de produto**, não do portal SSO |

### D-00016-2 — Filhos saem do menu; URLs antigas redirecionam — **Aceito: A** (2026-09-07)

| Opção | Descrição |
|-------|-----------|
| **A (recomendada)** | Remover do sidebar; `Redirect` 302 da URL antiga para o hub (`/Admin/Branches` → Org Details `#branches` do contexto, ou Organizations se Platform sem contexto) |
| B | Manter páginas soltas como “avançado” no menu | Não atende o pedido |
| C | 404 nas URLs antigas | Quebra bookmarks / treino |

### D-00016-3 — Hub primário de junções — **Aceito: A** (2026-09-07)

| Junção | Escrita | Leitura inversa |
|--------|---------|-----------------|
| ProductEnablement | Organization | Product |
| RolePermission | Role | Permission |
| RoleClaim | Role | ClaimDefinition |
| ClientProductBinding | AuthClient | Product |
| Membership | Organization (remove only) | User |
| UserRoleAssignment / UserClaimAssignment | **User** (form completo) + atalho na **Organization** (pré-preenche OrgId) | Role / Branch |
| OrganizationInvite | Organization | — |

| Opção | Descrição |
|-------|-----------|
| **A (recomendada)** | Tabela acima: um lugar canônico de create; o outro lado lista |
| B | Create nos dois lados (duplica forms) |
| C | Só no “primeiro” FK da entidade | Ruim para assignment (User é o sujeito) |

### D-00016-4 — Entrada do Org Admin — **Aceito: A** (2026-09-07)

| Opção | Descrição |
|-------|-----------|
| **A (recomendada)** | Item **Minha organização** → `/Admin/Organizations/Details/{Portal.OrganizationId}`. Sem contexto → mesma trava de hoje (“Selecione uma organização em Contexto”) |
| B | Org Admin continua vendo Filiais/Convites no menu | Contraria o objetivo |
| C | Depois do switch_context, redirect automático para o Details | Mais mágico; pode irritar quem só queria trocar contexto |

Platform Admin: listagem Organizações permanece; atalho “Minha organização” só se houver contexto.

### D-00016-5 — Composição das telas (não inflar Details) — **Aceito: A** (2026-09-07)

`DetailsModel` já viola YAGNI/SRP se ganhar convites + assignments + LDAP.

| Opção | Descrição |
|-------|-----------|
| **A (recomendada)** | **ViewComponents** (ou nested Razor Pages) por filho: `/Admin/Organizations/{id}` só carrega o pai + abas; cada aba é componente com seus handlers **ou** páginas filhas `/Admin/Organizations/{id}/Invites`. Visual continua “uma tela com abas” |
| B | Continuar amontoando handlers no `DetailsModel` | Dívida imediata |
| C | SPA / Blazor | Fora (F00003-D1, D-00014-2) |

Rotas filhas (se A com nested pages) devem **não** aparecer no sidebar; breadcrumb: Empresas › {Org} › Convites.

### D-00016-6 — `/Me` não ganha abas de operação Admin — **Aceito: A** (2026-09-07)

D-00014-3 (página única, duas rotas) permanece. Abas novas (Convites admin, CRUD enablement, assignments) gated `IsAdminRoute && permission`.

| Opção | Descrição |
|-------|-----------|
| **A (recomendada)** | Mesmo PageModel/layout; abas extras só Admin |
| B | Separar Admin hub de Me Details | Revisa D-00014-3; mais drift visual |
| C | Liberar as mesmas abas em `/Me` | Mistura papéis |

### D-00016-7 — 2º nível (Branch como pai) — **Aceito: B** (2026-09-07; hub Branch fica follow-up)

O pedido (“abrir o pai e embaixo os dependentes; o filho pode abrir outro nível”) descreve Branch Details. Assignments com `BranchId`, sessões da unidade e LDAP por branch fazem sentido **dentro da branch**, não só na org.

| Opção | Descrição |
|-------|-----------|
| A | Hub Branch completo nesta mesma feature |
| **B (recomendada para o 1º corte)** | Feature entrega hubs de 1º nível (Org, Product, Role, User, AuthClient) + lista de branches na org; Branch Details = **fase E** da mesma feature se o aceite incluir, senão follow-up 00017 |
| C | Nunca hub Branch; assignments só na org | Perde o “alinhamento” da unidade |

Template `form.html` já trata branch como tela própria a partir da empresa — alinhado a A/B, não a C.

### D-00016-8 — Padronizar visual dos hubs no kit 00014 — **Aceito: A** (2026-09-07)

Páginas 00011 (`Roles`, `Users`, `Permissions`, …) ainda usam markup pré-marca (`<h1>`, `.flash`, tabela crua). Hubs novos seguem card + `nav-tabs` + breadcrumb do Org Details.

| Opção | Descrição |
|-------|-----------|
| **A (recomendada)** | Ao criar o hub, aplicar o shell 00014 naquela superfície |
| B | Só mover itens de menu, visual antigo | Meia entrega |
| C | Redesign de templates em `visual-identity` | Fora |

---

## 6. Escopo

### Inclui

- Redução e agrupamento do sidebar Admin (+ partial única desktop/mobile).
- Hubs Details: Organization (completar), Product, Role, User, AuthClient, Permission (visão inversa).
- Absorver páginas-filho; redirect das URLs antigas.
- “Minha organização” para Org Admin.
- Gates de permission por aba (espelhar matriz 00011).
- Breadcrumb + tab/hash (`atnodus.js` já faz hash→tab).
- Extração de composição para não crescer `DetailsModel` (D-00016-5).
- Atualizar CONTEXT (`admin-portal.md`, `ui-brand.md`) e este backlog ao concluir.

### Fora de escopo

- Mudança de Domain / FKs / OpenIddict / permissions JWT.
- SPA, Blazor, gerar Admin a partir de `MenuItems`.
- CRUD de webhooks / outbox (já fora na 00011).
- White-label por org.
- Playwright obrigatório (checklist E2E manual).
- Alterar `/Me/Profile` além do necessário para não vazar abas Admin.
- Código de produtos consumidores (RoadCrew, samples) — o portal SSO é a superfície.

### Inclusão condicional

- Hub Branch (D-00016-7): fase E se aceite A; senão não nesta feature.

---

## 7. Abordagem — fases

| Fase | Entrega | Critério de pronto |
|------|---------|-------------------|
| **A — IA** | Partial de nav alvo; redirects; “Minha organização”; esconder filhos do menu | Menu raiz só; deep links antigos não 404 |
| **B — Org hub** | Completar abas da empresa (Convites, Enablement CRUD, Membership remove, Assignments, Claims, LDAP, IdPs org); extrair ViewComponents do Details atual | Org Admin opera **só** pelo Details + Contexto |
| **C — Catálogo Product / Role / Permission** | Details com abas de filhos | MenuItems, RolePermissions, RoleClaims, Bindings (visão product) fora do menu |
| **D — User / AuthClient** | Hubs equivalentes | Assignments platform-scoped no User; Bindings no client |
| **E — Branch (se D-00016-7 A)** | Details da unidade a partir da lista da org | Sem `/Admin/Branches` como destino final |
| **F — Docs + E2E manual** | CONTEXT + checklist por papel | `admin-portal.md` descreve hubs, não a lista plana 00011 |

```text
Listagem raiz (Organizações | Produtos | …)
  → Details do pai (header + dados)
      → abas = listas de filhos (ViewComponent / nested page)
          → [opcional] Details do filho (Branch)
```

APIs e Domain **não** mudam de fase. AuthZ continua `sso.admin.*` na PageModel (esconder aba + Forbid no POST).

---

## 8. Arquivos impactados (previsto)

| Camada | Caminhos previstos |
|--------|--------------------|
| Web.Api — nav | `Areas/Admin/Pages/_Layout.cshtml`; nova partial `_AdminNav.cshtml` (desktop + offcanvas) |
| Web.Api — org hub | `Areas/Me/Pages/Organizations/Details.*` (extrair); novos componentes/páginas filhas |
| Web.Api — hubs | `Areas/Admin/Pages/Products/Details`, `Roles/Details`, `Users/Details`, `AuthClients/Details`, `Permissions/Details`; opcional `Branches/Details` |
| Web.Api — legado | PageModels atuais: redirect ou virar implementação interna do componente |
| Web.Api — rotas | `Program.cs` (`AddAreaPageRoute` / conventions) |
| Application / API / Domain / Data | **Nenhum** no plano A (D-00016-1). Só se um Filter query não permitir o recorte do pai |
| Tests | Smoke de rotas Admin; redirect; gate 403 em aba sem permission; regressão Me Details (abas Admin ausentes) |
| Docs (.ai) | Este plano; `CONTEXT/admin-portal.md`; `CONTEXT/ui-brand.md`; `CONTEXT/Decisions.md`; `WORK/2026-07-16-backlog-pos-mvp.md` |

---

## 9. Riscos

| Risco | Mitigação |
|-------|-----------|
| PageModel deus (Details já ~500 linhas) | D-00016-5 obrigatório antes da fase B crescer |
| Org Admin perder acesso ao tirar itens do menu | D-00016-4; checklist E2E Org vs Platform |
| `/Me` receber CRUD admin | D-00016-6; testes de rota Me |
| Assignment platform-scoped “sumir” | Hub User; não depender de OrgId |
| Enablement/Bindings duplicados nos dois pais | D-00016-3: um create, um list |
| Redirect sem contexto (Platform abre `/Admin/Branches`) | Redirect para Organizations + flash “abra a empresa” |
| Mobile nav incompleta | Partial única |
| Permissões granulares (`menus`, `sessions.revoke`, `audit.read`) | Aba/item só se `HasPermission`; POST Forbid |
| Deep link `#tab` + POST handler | Já usado na 00014 (`ActiveTab`, fragment); repetir o contrato |
| Usuário treinado no menu plano | Redirect + breadcrumb; não 404 (D-00016-2 A) |

**Não é impedimento:** falta de API REST “nested”. Reader + commands atuais bastam (Org Details já prova). **Não é impedimento:** Razor (F00003-D1). **Não é impedimento de domínio:** FKs da 00012/00015 já descrevem os pais.

---

## 10. Matriz de aceite (após implementação)

- [x] Sidebar Admin não lista cadastros dependentes (lista da §3).
- [x] Org Admin, com contexto, gerencia branches/convites/memberships/enablements/assignments **a partir do Details da org**, sem Swagger e sem aqueles itens no menu.
- [x] Platform Admin abre Product/Role/User/AuthClient e vê filhos em abas.
- [x] `/Admin/Branches` (e demais URLs filhas) redirecionam para o hub correto.
- [x] `/Me/Organizations/Details/{id}` **não** mostra abas de operação Admin.
- [x] Membership ainda **não** é criada pela UI (só convite).
- [x] Aba ausente quando falta permission; POST sem permission → 403.
- [x] Nav mobile = nav desktop (subconjunto por permission).
- [x] `admin-portal.md` e `ui-brand.md` descrevem hubs, não o menu plano da 00011.

---

## 11. Estratégia de testes

- [x] Integração: GET Details org (Admin e Me) — abas divergem conforme rota (`AdminHubNavigationScenarios`)
- [x] Integração: GET URL legada → 302 para hub (`/Admin/Branches`, `/Admin/MenuItems`)
- [x] Negativo: nested pages `Forbid()` sem acesso à org / sem platform; POST handlers revalidam `CanAccessOrganization` / `IsPlatformAdmin`
- [x] Sem contexto: `/Admin/MyOrganization` → SwitchContext; Org Admin sem sessão em URL filha → SwitchContext
- [ ] Checklist manual E2E: OrgAdmin vs PlatformAdmin (matriz 00011, **nova IA**)
- [x] Regressão: aceite de convite ainda cria Membership (`OrganizationInviteScenarios`); Domain/API de enablement intocados (D-00016-1)

Sem testes de Domain extras se D-00016-1 A se confirmar.

---

## 12. Checklist de planejamento

- [x] Alinhado a PLAYBOOK/architecture.md (UI orquestra; Domain intocado)
- [x] Naming: hubs em inglês nas rotas (`Details`, `Organizations`)
- [x] Auth/segurança: mesmas `sso.admin.*`; abas gated
- [x] Migrations: nenhuma prevista
- [x] CONTEXT atualizado (admin-portal, ui-brand, modules, Decisions)
- [x] Pronto para implementação — D-00016-1..8 aceitas (2026-09-07)
- [x] Implementado (2026-09-07)

---

## Decisões

D-00016-1..8 **aceitas** (2026-09-07): A / A / A / A / A / A / B / A. Hub Branch fica follow-up (D-00016-7 B).

Épico base cadastros: [00011](2026-07-20-00011-expansao-cadastros-admin.md). Superfície visual: [00014](2026-07-29-00014-layout-perfil-usuario.md).
