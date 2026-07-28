# Feature Plan — 00013 Product Enablement (Organization × Product)

> Arquivo: `.ai/WORK/2026-07-28-00013-product-enablement.md`  
> Template: `.ai/TEMPLATES/feature-plan.md` + `module.md`  
> Status: **Planejamento** — aguarda aceite das decisões D-00013-*  
> Data: 2026-07-28  
> Depende de: 00001 (Organization/Product), 00002 (AuthZ admin), 00011 (portal Admin)  
> Relaciona: ADR-003 (switch-context), ADR-005 (permissions no JWT), F00001-D10 (Product ≠ AuthClient), 00012 (FKs explícitas)

## Objetivo

Permitir **habilitar quais Products cada Organization contratou/pode usar** (`ProductEnablement`), como pré-condição comercial do runtime SSO.

Acesso fino do **usuário** permanece em Roles/Permissions/`UserRoleAssignment` (e claims tipadas). Esta feature **não** substitui authz de usuário — ela responde: *“esta empresa tem o produto?”*.

## Contexto

- Modelo previsto desde 00001: `ProductEnablement  # Organization → Products habilitados`.
- MVP atual: Product é catálogo global; Org/Branch entram só via assignments + `switch_context`; Product no token vem de `client_id` → `ClientProductBinding`.
- Sem enablement, qualquer org com membership + assignment no product “funciona” — inadequado para cobrança multi-produto.
- Pendente explícito em `modules.md` / 00011: “SMTP real, ProductEnablement avançado”.

```text
Camada comercial (esta feature)
  Organization ──ProductEnablement──► Product

Camada operacional (já existe)
  User × Org × Branch? × Product → Roles → Permissions (JWT)
```

## Decisões (propostas — aguardam aceite)

### D-00013-1 — Aggregate `ProductEnablement` — **Proposto: A**

| Opção | Descrição |
|-------|-----------|
| **A (recomendada)** | Aggregate Identity: `OrganizationId` + `ProductId`, unicidade, soft-delete, FKs Restrict (padrão 00012) |
| B | Flag/coleção embutida em Organization | Polui tenant; pior para auditoria/cobrança |
| C | Só config externa (billing) | SSO não enforce; risco de vazamento de acesso |

### D-00013-2 — Semântica fail-closed — **Proposto: A**

| Opção | Descrição |
|-------|-----------|
| **A (recomendada)** | Sem linha ativa Org×Product ⇒ **nega** uso do product no contexto da org |
| B | Fail-open (só nega se “explicitamente desabilitado”) | Frágil para cobrança |

**Migração de dados:** seed/script habilita products já usados pelas orgs existentes (dev + qualquer org em homolog) antes de ligar o gate.

### D-00013-3 — Onde enforce — **Proposto: A**

| Opção | Descrição |
|-------|-----------|
| **A (recomendada)** | Gate em emissão de token com `organization_id` presente: `switch_context`, refresh com org, e `TokenClaimsFactory` quando org + product do `client_id` estão definidos |
| B | Só `switch_context` | Refresh/login com org residual pode burlar |
| C | Só nas APIs de produto | SSO não é fonte de verdade comercial |

**Client Credentials / M2M** (sem usuário/org): **fora do gate org** neste MVP da feature — product apps M2M continuam confiando em client+scopes; org enablement aplica-se a fluxos com tenant. (Reabrir se surgir client “por org”.)

**Platform-scoped** (`OrganizationId == null` em admin assignments / product `sso-platform`): **isento** do check de enablement de tenant (admin da plataforma não é “produto contratado pela org”).

### D-00013-4 — Modelo de campos — **Proposto: A (mínimo cobrável)**

| Opção | Campos |
|-------|--------|
| **A (recomendada)** | `OrganizationId`, `ProductId` (+ auditoria/soft-delete). Enable = POST; Disable = soft-delete (ou Delete command) |
| B | + `StartsAt` / `EndsAt` / `Status` (Trial/Active/Suspended) | Útil p/ cobrança; +escopo |
| C | + referência externa `SubscriptionId` | Integra billing; fora do SSO MVP |

Orçamento pode fatiar: **MVP = A**; evolutiva B/C em feature seguinte se produto comercial exigir.

### D-00013-5 — Admin / permissions — **Proposto: A**

| Opção | Descrição |
|-------|-----------|
| **A (recomendada)** | CRUD Platform Admin (`sso.admin.platform`); Org Admin **GET** só da própria org (`sso.admin.org`) — transparência do que a empresa contratou, sem poder habilitar |
| B | Só Platform Admin (CRUD + list) | Mais simples; org não vê catálogo contratado na UI |
| C | Org Admin também POST/DELETE | Org “se auto-contrata” — indesejado p/ cobrança |

### D-00013-6 — Efeito ao desabilitar — **Proposto: A**

| Opção | Descrição |
|-------|-----------|
| **A (recomendada)** | Próximos tokens com aquela org+product falham o gate; tokens já emitidos expiram pelo TTL (sem revoke em massa obrigatório) |
| B | Revogar sessões da org×client/product no disable | Mais correto comercialmente; +trabalho (00005) |
| C | Manter tokens até logout | Fraco p/ “cortar acesso” |

## Escopo

### Inclui

- Aggregate `ProductEnablement` (Domain/Application/Data/API/Tests) no pattern Sample/ClientProductBinding.
- Specs: unicidade Org×Product (não deletado); Org e Product existem; não soft-deleted.
- API `api/identity/productenablements` (CRUD alinhado a HTTP verbs: `Post`/`Put` ou só Post+Delete / `Patch` se status entrar).
- Gate runtime (D-00013-3): erro claro (ex.: `product_not_enabled_for_organization`) em switch-context / emissão com org.
- Seed: habilitar products de desenvolvimento para a org seed; migration/backfill para orgs já existentes conforme inventário.
- Admin UI (Razor `/Admin`): página Platform (CRUD); opcional lista read-only Org Admin.
- Docs: `modules.md`, `business.md`, `glossary.md`, `product-integration.md`, `Decisions.md`, backlog.
- Audit: evento AuthN/AuthZ ou audit admin em enable/disable (mínimo: log + AuthAudit se padrão do projeto couber sem inflar escopo).

### Fora de escopo

- Motor de billing, fatura, preço, trial automático, gateway de pagamento.
- “Usuário tem acesso ao product” como entidade separada (usa permissions/assignments).
- Habilitar product por **Branch** (só Organization neste plano).
- Marketplace / self-service de contratação pela org.
- DCR / novos AuthClients (já 00007).
- SMTP real.
- Revogação em massa obrigatória no disable (salvo se D-00013-6 = B for aceito).

## Module — Identity / ProductEnablement

| Item | Valor |
|------|-------|
| Context | Identity (`IdentityDb`) |
| Entity | `ProductEnablement` |
| Id | Guid |
| Table | `ProductEnablements` |
| Aggregate root | Sim |
| Auditable | Sim (`IdentityAuditableEntity`, soft-delete F00001-D12) |
| FKs | `OrganizationId` → `Organizations`; `ProductId` → `Products` (`Restrict`, 00012) |

### Domain

```text
SSO.Core.Domain/Identity/ProductEnablements/
  Entity/ProductEnablement.cs
  Services/CreateProductEnablementService.cs
  Services/DeleteProductEnablementService.cs   # soft-delete = desabilitar
  Specifications/…AlreadyExists… / org-product exists
  Validations/EntityValidations + DomainValidations
  Resources/
```

### Application / Data / API

```text
Application: Commands Post/Delete (+ Put/Patch se D4=B), Queries Filter/GetById, Notifications
Data: ProductEnablementMap + migration Phase15ProductEnablement (+ backfill seed)
API: api/identity/productenablements
DI: AddValidationsConfigurations / AddDomainServices (padrão existente)
Admin: Areas/Admin/Pages/ProductEnablements.cshtml(+.cs)
```

### Serviço de domínio transversal (gate)

```text
IProductEnablementGuard / ProductEnablementGuard
  IsEnabledAsync(organizationId, productId) → bool
Consumido por: switch_context handler + TokenClaimsFactory (quando org + product resolvido via ClientProductBinding)
```

## Abordagem — fases

### Fase A — Domínio + persistência

1. Entity, validators, specs, Create/Delete services.
2. Map + FKs + índice único filtrado `(OrganizationId, ProductId)` where not deleted.
3. Migration + seed/backfill.

### Fase B — API + AuthZ

1. Controller CRUD + `[RequiresPermission]`.
2. Filter por `organizationId` / `productId`.
3. Testes integration HTTP.

### Fase C — Gate runtime

1. `IProductEnablementGuard`.
2. Plug em switch-context + `TokenClaimsFactory` (org presente).
3. Mensagem/código de erro estável p/ clients.
4. Testes: org sem enablement não obtém token contextual; com enablement + assignment obtém permissions.

### Fase D — Admin + docs

1. UI Platform CRUD; GET org-admin se D5=A.
2. Atualizar CONTEXT + Decisions + backlog status.
3. Nota em `product-integration.md`: pré-requisito comercial.

## Critérios de aceite

- [ ] Platform Admin habilita Product P na Organization O via API/UI.
- [ ] Unicidade: segundo enable O×P rejeitado.
- [ ] `switch_context` (ou emissão com org) para client bound a P **falha** se O não tem enablement ativo.
- [ ] Com enablement + membership + assignment, token sai com permissions efetivas como hoje.
- [ ] Disable (soft-delete) faz novas emissões falharem; comportamento de tokens antigos conforme D-00013-6 aceito.
- [ ] Platform admin / product `sso-platform` sem org não quebra.
- [ ] Client Credentials sem org continua operacional (MVP).
- [ ] Seed/dev: org de desenvolvimento já habilitada nos products usados pelos testes.
- [ ] FKs explícitas Organization/Product (padrão 00012).
- [ ] CONTEXT atualizado.

## Arquivos impactados (previsto)

| Camada | Caminhos |
|--------|----------|
| Domain | `src/SSO.Core.Domain/Identity/ProductEnablements/**` + interface guard em `_Shared` ou Services |
| Application | `src/SSO.Core.Application/Identity/ProductEnablements/**` |
| Data | `EntityMappings/ProductEnablementMap.cs`, `IdentityDbContext`, Migrations `Phase15*`, `IdentitySeed.cs` |
| Infra Services | `ProductEnablementGuard` (+ registro DI) |
| Middleware | `TokenClaimsFactory` (hook); AddServices validations/domain |
| API | `Resources/IdentityDb/ProductEnablementsController.cs`; grant/switch-context path em `AuthorizationController` / handlers |
| Admin | `Areas/Admin/Pages/ProductEnablements*`; nav `_Layout` |
| Shared | código de erro / constant se houver padrão |
| Tests | Unit Domain + `ProductEnablementsScenarios` + gate em OIDC/switch-context scenarios |
| Docs | este plano; `modules.md`, `business.md`, `glossary.md`, `product-integration.md`, `Decisions.md`, `admin-portal.md`, backlog |

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Quebra ambiente/dev sem backfill | Seed + migration de enablement para orgs/products conhecidos **antes** de ativar gate |
| Login/refresh com org sem passar switch | Gate também em `TokenClaimsFactory` quando `organizationId` + product resolvido |
| Confundir enablement com permission de usuário | Docs + naming; UI separada de UserRoleAssignments |
| Cobrança precisa de vigência | MVP A; evolutiva B documentada |
| Disable sem revoke | TTL curto (ADR-005) + opção D6-B se negócio exigir corte imediato |
| Performance no token path | Lookup indexado Org×Product; cache curto opcional (fora do MVP se não necessário) |

## Estratégia de testes

- [ ] Unit: Create (unicidade), Delete/soft-disable, validators
- [ ] Integration API: POST/GET/DELETE + 403 sem `sso.admin.platform`
- [ ] Integration gate: switch-context negado sem enablement; permitido com enablement
- [ ] Regression: fluxos seed atuais (admin, dev-product) verdes após backfill
- [ ] Platform-scoped / M2M sem org não regredem

## Estimativa para orçamento (ordem de grandeza)

| Fatia | Conteúdo | Complexidade relativa |
|-------|----------|----------------------|
| A | Aggregate + migration + seed/backfill | M |
| B | API + AuthZ + testes HTTP | M |
| C | Gate token/switch-context + testes | M/L (caminho crítico AuthN) |
| D | Admin UI + docs CONTEXT | S/M |
| **MVP (D1–D5 A, D4 A, D6 A)** | A+B+C+D | **~1 feature média** (similar a um aggregate completo + hook auth) |
| Evolutiva | Vigência/Status (D4-B), revoke no disable (D6-B), billing ref | Feature(s) separada(s) |

Não é compromisso de prazo em dias — serve para priorizar no backlog comercial.

## Checklist

- [ ] D-00013-1..6 aceitas (ou desvios registrados em Decisions)
- [ ] Alinhado a PLAYBOOK/architecture.md + domain-rules-in-domain-services
- [ ] Naming HTTP verbs nos Commands
- [ ] Migration + soft-delete + FKs Restrict
- [ ] AuthZ `sso.admin.*`
- [ ] CONTEXT + backlog atualizados
- [ ] Pronto para implementação

## Decisões abertas (bloqueiam código se não fechadas)

1. Aceitar D-00013-1..6 conforme proposto?
2. D4: MVP só enable/disable ou já `StartsAt`/`EndsAt`/`Status`?
3. D6: TTL suficiente ou revoke de sessões no disable?
4. Org Admin vê lista read-only (D5-A) na primeira entrega?
