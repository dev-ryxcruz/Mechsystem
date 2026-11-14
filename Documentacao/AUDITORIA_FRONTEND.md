# Auditoria de Frontend — MechSystem

**Data:** 13/11/2026  
**Escopo:** `Components/` e `wwwroot/` (Blazor Server .NET 10 + Bootstrap 5 local + CSS isolado)  
**Restrições respeitadas:** Sem troca de framework, sem Tailwind/React/MudBlazor, sem alteração de `Services/`, `Models/`, `Data/`, `Migrations/`, sem redesenho de layout/paleta/sidebar.

---

## Skills utilizadas

- **`ui-a11y`**: Aplicada na auditoria WCAG 2.2 AA (contraste de texto/badges, alvos de toque, `aria-label` em botões só com ícone, `role="dialog"` / `aria-modal` / `Esc` em modais, `role="alert"` / `aria-live`, e `@media (prefers-reduced-motion: reduce)`).
- **`ui-states`**: Aplicada no mapeamento dos estados de tela (carregamento, vazio vs. busca sem resultados, erro recuperável, sucesso, ação ocupada e reconexão) e na identificação da ausência de `ErrorBoundary`.
- **`ux-audit`**: Aplicada com base nas heurísticas de Nielsen e boas práticas mobile/tablet (visibilidade de status, controle e liberdade em modais, prevenção de erros e eliminação de interações dependentes apenas de `:hover` no mapa de avarias).
- **`ui-review`**: Aplicada na verificação de coerência visual, mapeamento dos estilos inline repetidos e padronização via classes utilitárias (adaptada de React/Tailwind para Blazor Server/Bootstrap 5).
- **`design-token-contrast`**: Aplicada no cálculo exato das razões de contraste WCAG 2.2 AA sobre os tokens de `wwwroot/app.css` e cores de status Bootstrap, propondo ajustes que preservam o matiz sem alterar a paleta.
- **`ui-tokens`**: Aplicada na proposta de organização de variáveis CSS semânticas `--mech-*` em `wwwroot/app.css` (adaptada de StyleSeed JSON/Tailwind para CSS custom properties).
- **`web-design-guidelines`**: Aplicada na revisão estrutural de formulários, tabelas responsivas, foco visível e feedback. *(Nota: a etapa de fetch remoto da skill não foi executada por estarmos em auditoria estática local/offline.)*
- **`ux-copy`**: Aplicada na revisão de microtextos em pt-BR (tradução de telas de sistema em inglês, clareza de mensagens de erro, rótulos de botões e textos de estado vazio).
- **`ux-feedback`**: Aplicada na avaliação de feedbacks visuais de carregamento, confirmação de ações, prevenção de duplo envio e recuperação de falhas.
- **`webapp-testing`**: Planejada para a **Etapa B** (bateria Playwright em 360, 390, 768 e 1280 px com capturas antes/depois em `Documentacao/capturas-frontend/`); não executada nesta Etapa A.
- **`web-performance-optimization`**: Aplicada na identificação de bloqueios por CDNs externos (fontes, ícones e IMask), prevenção de layout shift e suporte a `prefers-reduced-motion` (adaptada para Blazor Server).

> **Sugestões de skills ignoradas pelas restrições do projeto:** Recomendações específicas de React/Radix/Tailwind (`data-slot`, `cn()`, `React.ComponentProps`, `animate-pulse` do Tailwind), reestruturação de design system completo com tokens JSON/dark-mode e `BottomNav` mobile com safe-areas não se aplicam à arquitetura Blazor Server + Bootstrap 5 atual.

---

## 1. Dependências Externas (Funcionamento Offline)

| Arquivo | Linha(s) | Achado | Prioridade | Esforço | Correção Proposta |
|---|---|---|---|---|---|
| `Components/App.razor` | 9–11 | Carrega Google Fonts (`Inter`) via `https://fonts.googleapis.com` e `https://fonts.gstatic.com`. Offline a fonte cai para fallback do SO. | **Alta** | **P** | Baixar os arquivos `.woff2` da fonte Inter (pesos 400, 500, 600, 700) para `wwwroot/lib/inter/` e declarar `@font-face` em `wwwroot/lib/inter/inter.css`, referenciando localmente via `@Assets[...]`. |
| `Components/App.razor` | 13 | Carrega Bootstrap Icons `1.11.3` via CDN (`cdn.jsdelivr.net`). Offline **todos os ícones do sistema desaparecem**. | **Alta** | **P** | Baixar `bootstrap-icons.min.css` e a pasta `fonts/` (`bootstrap-icons.woff2` e `.woff`) para `wwwroot/lib/bootstrap-icons/` e trocar o `<link>` para `@Assets["lib/bootstrap-icons/bootstrap-icons.min.css"]`. |
| `Components/App.razor` | 25–48 | **Achado extra:** Carrega `imask` via CDN (`https://unpkg.com/imask`). Offline, `IMask` fica indefinido e `window.applyMasks()` lança `ReferenceError`, quebrando máscaras de CPF, CNPJ e telefone. | **Alta** | **P** | Baixar `imask.min.js` para `wwwroot/lib/imask/imask.min.js`, referenciar via `@Assets["lib/imask/imask.min.js"]` e adicionar guarda `if (typeof IMask === 'undefined') return;` em `window.applyMasks`. |

---

## 2. Responsividade (360, 390, 768 e 1280 px)

| Arquivo | Linha(s) | Achado | Prioridade | Esforço | Correção Proposta |
|---|---|---|---|---|---|
| `Components/Pages/Acompanhar/Index.razor.css` | 27–55, 78–99 | Em **360 e 390 px**, `.tracking-header` e `.tracking-body` usam `padding: 2rem` (64px) somado a `1rem` do `.public-container` (total 96px), deixando apenas 264px para `.progress-track` (que tem 5 steps de `60px` = 300px), causando aperto/quebra nos rótulos. | **Alta** | **P** | Adicionar `@media (max-width: 576px)` reduzindo padding de `.tracking-header`/`.tracking-body` para `1.25rem 1rem`, `.progress-step` para `width: auto; flex: 1`, `.step-icon` para `36px` e `.step-label` para `0.65rem`. |
| `Components/Pages/Acompanhar/Index.razor` | 138–159 | Em 360px, data de previsão + badge ("Atrasado X dia(s)") podem quebrar layout sem alinhamento adequado. | **Média** | **P** | Envolver data e badge em container `d-flex flex-wrap align-items-center gap-2` removendo `ms-2` fixo. |
| `Components/Pages/Operacional/PainelMecanico.razor` | 47–55, 418–600 | Em **768 px (tablet)**: os cards usam botões `.btn-sm`, que no `wwwroot/app.css` (linha 994) têm `min-height: 36px` — abaixo dos **44 px** recomendados para toque em tablet na oficina. Em **1280 px**, as 5 colunas (`col-xl`) ficam estreitas (~220px). | **Alta** | **M** | Criar classe `.painel-action-btn { min-height: 44px; min-width: 44px; }` para os botões operacionais do mecânico (Iniciar, Pausar, Concluir, Aditivo, Reportar). Ajustar grid do Kanban para `col-lg-6 col-xl-4 col-xxl` ou scroll horizontal suave quando necessário em 1280px. |
| `Components/Pages/OrdensServico/Details.razor` | 30–170, 1291–1335 | Em **360/390 px**, a barra de ações tem até 8+ botões e `.os-stepper` não possui rolagem horizontal ou redução de fonte, podendo espremer rótulos. Tabelas internas dependem de `table-responsive`, mas cabeçalhos com `style="width:..."` podem comprimir textos. | **Alta** | **M** | Garantir `flex-wrap` e `gap-2` consistentes na barra de ações; adicionar `overflow-x: auto` / tamanho compacto em `.os-stepper` abaixo de 576px; revisar larguras de colunas em tabelas. |
| `Components/Pages/OrdensServico/Create.razor` | 19–28, 45–59, 129–160 | Em **360 px**, `<div class="input-group">` (select de veículo com textos longos + botão "Novo") e a tabela de serviços/peças com larguras inline (`100px`, `150px`, `130px`) apertam inputs numéricos. | **Média** | **P** | Usar `min-width` adequado nos inputs da tabela dentro de `.table-responsive` e garantir quebra limpa do cabeçalho (`flex-wrap gap-2`). |
| `Components/Pages/OrdensServico/VistoriaChecklist.razor` & `.css` | 155–200, 246–262; CSS 159–180 | 1) `.avaria-marker:hover .marker-tooltip` **depende exclusivamente de `:hover`** — em celular e tablet não há hover, impedindo ver a descrição ou tocar no `X` no mapa. 2) Rodapé (linha 247) usa `d-flex justify-content-between` sem `flex-wrap`, quebrando em 360px. 3) Botão remover avaria (`p-0`) tem alvo de toque minúsculo. | **Alta** | **M** | Adicionar suporte a toque/clique no marcador (alternar classe `.active` ao tocar para exibir tooltip), adicionar `flex-wrap gap-3` no card de rodapé e aumentar área clicável do botão de remoção (mínimo 36–44px na lista). |

---

## 3. Impressão (`@media print`)

| Arquivo | Linha(s) | Achado | Prioridade | Esforço | Correção Proposta |
|---|---|---|---|---|---|
| `ImprimirOS.razor`, `ImprimirTermoGarantia.razor`, `ImprimirVistoria.razor` | Cabeçalho `.doc-header` | **Nenhum dos 3 documentos exibe o logo (`/logo.png`)** no cabeçalho de impressão. | **Alta** | **P** | Incluir `<img src="/logo.png" alt="Logo da oficina" class="doc-logo" />` ao lado de `.doc-oficina` nos 3 componentes, com `max-height: 48px; width: auto;`. |
| `ImprimirOS.razor.css`, `ImprimirTermoGarantia.razor.css`, `ImprimirVistoria.razor.css` | `@media print` | Faltam regras de quebra de página: `break-inside: avoid` em `.doc-section`, `.doc-assinatura-grid`, `.doc-aprovado-box`, `.doc-text-box`, `tr`; e `thead { display: table-header-group; }` para tabelas de itens que passem de uma página. | **Alta** | **P** | Adicionar regras `break-inside: avoid; page-break-inside: avoid;` nos blocos indivisíveis e `thead { display: table-header-group; }` nas tabelas dos 3 `.razor.css`. |
| `wwwroot/app.css` & `Imprimir*.razor.css` | `app.css:1041–1076` | No `@media print` global falta ocultar `.sidebar-backdrop`, `#components-reconnect-modal` e `.modal-backdrop`. | **Média** | **P** | Incluir `.sidebar-backdrop, #components-reconnect-modal, .modal-backdrop { display: none !important; }` no `@media print` de `wwwroot/app.css`. |
| `OrdensServico/Details.razor` | 1308–1318 | Possui `@@media print` inline no componente, duplicando regras do `app.css` (e não oculta `.top-row`). | **Baixa** | **P** | Alinhar com `app.css` garantindo ocultação de `.top-row` e `.sidebar-backdrop` caso o usuário imprima direto de `Details`. |

---

## 4. Acessibilidade (WCAG 2.2 AA)

| Arquivo | Linha(s) | Achado | Prioridade | Esforço | Correção Proposta |
|---|---|---|---|---|---|
| `wwwroot/app.css`, `Acompanhar/Index.razor`, `PainelMecanico.razor`, `Details.razor` | `app.css:6,21`; `Acompanhar:240–251`; `Painel:53`; `Details:62` | **Falha de contraste medida:**<br>- `#ffc107` (`warning`) sobre `#ffffff` = **1.63:1**; texto branco sobre `#ffc107` = **1.63:1**<br>- `#0dcaf0` (`info`) sobre `#ffffff` = **1.96:1**; `bg-info text-white` (`Details.razor:62`) = **1.96:1**<br>- `--accent (#ff751f)` sobre `#ffffff` = **2.69:1**<br>- `--text-muted (#9ca3af)` sobre `#ffffff` = **2.54:1** (sobre `#f0f2f5` = **2.26:1**)<br>- `#fd7e14` sobre `#ffffff` = **2.57:1** | **Alta** | **P** | Sem mudar a paleta:<br>1) Usar `text-dark` (`#1a1a2e` sobre `#ffc107` = **10.46:1**) em todos os badges/botões `bg-warning` e `bg-info`.<br>2) Em `Acompanhar/Index.razor` (`GetStatusColor`) e `PainelMecanico.razor` (`text-warning`/`text-info`), usar variantes escuras acessíveis para texto sobre fundo claro (ex.: `#997404` para warning = **4.57:1**, `#087990` para info = **5.05:1**, `#b84400` para laranja = **5.18:1**).<br>3) Criar `--mech-accent-text: #b84400` para textos/links pequenos em fundo claro e ajustar `--text-muted` para `#5c636a` (**5.74:1** sobre branco). |
| `PainelMecanico.razor`, `Clientes/Listar.razor`, `Veiculos/Listar.razor`, `Servicos/Listar.razor`, `Estoque/Index.razor` | Várias (`modal fade show d-block`) | Os 12 modais Blazor customizados não possuem `role="dialog"`, `aria-modal="true"`, `aria-labelledby`, nem fechamento por tecla `Escape`. Botões `.btn-close` sem `aria-label="Fechar"`. | **Alta** | **M** | Adicionar `role="dialog" aria-modal="true" aria-labelledby="..."`, `@onkeydown` para fechar com `Escape` e `aria-label="Fechar"` em todos os `.btn-close`. |
| `PainelMecanico.razor` | 181, 206 | Itens selecionáveis no modal de aditivo são `<div style="cursor: pointer;" @onclick=...>` — inacessíveis por teclado (`Tab`/`Enter`). | **Média** | **P** | Trocar para `<button type="button" class="... text-start w-100">` ou adicionar `role="button" tabindex="0"`. |
| `Configuracoes/Index.razor`, `Home.razor`, `Create.razor`, `Clientes/Listar.razor`, `VistoriaChecklist.razor` | `Config:47,55`; `Home:54,60`; `Create:159`; `Vistoria:43,195,234` | Botões apenas com ícone possuem apenas `title` (ou nem isso) e faltam `aria-label` em pt-BR. Em `VistoriaChecklist.razor:43`, há `aria-label="Close"` em inglês. | **Média** | **P** | Adicionar `aria-label` descritivo em português em todos os botões só com ícone e corrigir `"Close"` para `"Fechar"`. |
| `OrdensServico/Index.razor`, `Acompanhar/Index.razor`, `Create.razor`, `Configuracoes/Index.razor` | Alertas dinâmicos | Alertas de erro/sucesso dinâmicos e toasts em várias páginas não possuem `role="alert"` ou `aria-live="polite"`. | **Média** | **P** | Padronizar `role="alert"` em mensagens de erro/aviso e `aria-live="polite"` em alertas de sucesso/carregamento. |
| `wwwroot/app.css`, `Acompanhar/Index.razor.css`, `VistoriaChecklist.razor.css`, `ReconnectModal.razor.css` | Animações CSS | Nenhuma folha de estilo respeita `@media (prefers-reduced-motion: reduce)`. | **Baixa** | **P** | Adicionar bloco `@media (prefers-reduced-motion: reduce)` em `wwwroot/app.css` desabilitando transições/animações longas. |

---

## 5. Estados de Tela e `ErrorBoundary`

| Arquivo | Linha(s) | Achado | Prioridade | Esforço | Correção Proposta |
|---|---|---|---|---|---|
| `Components/Layout/MainLayout.razor` e páginas complexas (`Home.razor`, `Details.razor`, `PainelMecanico.razor`, `Relatorios/Index.razor`) | Geral | **Zero ocorrências de `<ErrorBoundary>`** em todo o projeto. Qualquer exceção não tratada de renderização derruba a tela para o `#blazor-error-ui` global. | **Alta** | **P** | Envolver `@Body` no `MainLayout.razor` (ou seções críticas das páginas grandes) com `<ErrorBoundary>` contendo `<ErrorContent>` amigável em pt-BR e botão "Tentar novamente" (`Recover()`). |
| `Acompanhar/Index.razor` | 15–31, 197–211 | Se `OnInitializedAsync` lançar exceção (ex.: falha de banco/rede), `os` fica `null` e o bloco `else` exibe **"Link Inválido"** ao cliente final. | **Alta** | **P** | Adicionar flag `erroCarregamento` no `catch` exibindo mensagem específica com botão "Tentar novamente", reservando "Link Inválido" apenas quando a consulta concluir sem encontrar o token. |
| `ImprimirOS.razor`, `ImprimirTermoGarantia.razor`, `ImprimirVistoria.razor` | 12–18 | Se o `Id` informado não existir, `os` permanece `null` e a tela exibe o spinner `"Preparando documento..."` **infinitamente**. | **Média** | **P** | Adicionar `bool carregando = true;` e tratar o estado `os == null && !carregando` com alerta `"Ordem de Serviço não encontrada."` e botão "Voltar". |
| `OrdensServico/Details.razor` & `Create.razor` | `Details:23–26`; `Create:32–35` | Carregamento exibe apenas texto simples (`Carregando...`) sem spinner padrão; em `Details.razor`, se `os == null` após carregar, não há estado de "não encontrada". | **Média** | **P** | Padronizar spinner de carregamento e tratar `os == null && !carregando` em `Details.razor`. |
| `OrdensServico/Index.razor`, `Clientes/Listar.razor`, `Veiculos/Listar.razor`, `Estoque/Index.razor`, `Servicos/Listar.razor` | Blocos de lista vazia | Não diferenciam "nenhum registro cadastrado ainda" de "nenhum resultado para o filtro/busca atual", nem oferecem botão para limpar filtro ou criar o primeiro item. | **Baixa** | **P** | Exibir mensagem contextual: se houver filtro/busca ativo, oferecer botão "Limpar filtros"; se lista geral vazia, orientar o cadastro inicial. |

---

## 6. Tela de Reconexão do Blazor Server (`#components-reconnect-modal`)

| Arquivo | Linha(s) | Achado | Prioridade | Esforço | Correção Proposta |
|---|---|---|---|---|---|
| `Components/Layout/ReconnectModal.razor` | 9–29 | Todos os textos estão em **inglês** (`Rejoining the server...`, `Rejoin failed...`, `Retry`, `Resume`). | **Alta** | **P** | Traduzir integralmente para pt-BR amigável:<br>- `"Reconectando ao servidor..."`<br>- `"Falha na reconexão. Tentando novamente em X segundos."`<br>- `"Não foi possível reconectar. Tente novamente ou recarregue a página."`<br>- Botões: `"Tentar novamente"` e `"Retomar sessão"`. |
| `Components/Layout/ReconnectModal.razor.css` | 91–101 | Botão usa azul genérico (`#6b9ed2`) desconectado da identidade visual do MechSystem (`--accent`) e o modal não tem largura adaptada para 360px (`width: 20rem` fixo). | **Média** | **P** | Usar `var(--accent)` / `var(--accent-hover)` no botão e `width: min(22rem, calc(100vw - 2rem))` no `#components-reconnect-modal`. |
| `Components/Layout/MainLayout.razor` | 36–40 | Barra `#blazor-error-ui` também está em **inglês** (`An unhandled error has occurred.`, `Reload`). | **Alta** | **P** | Traduzir para `"Ocorreu um erro inesperado. "` e `"Recarregar"`, com `aria-label="Fechar"` no `.dismiss`. |

---

## 7. Estilos Inline (`style="..."`) e Tokens CSS (`--mech-*`)

Foram encontradas **127 ocorrências** de `style="..."` em **20 arquivos `.razor`** (principais: `Home.razor` com 42, `Relatorios/Index.razor` com 17, `Operacional/PainelMecanico.razor` com 16, `OrdensServico/Details.razor` com 14, `OrdensServico/Create.razor` com 9, `Acompanhar/Index.razor` com 7, `ImprimirTermoGarantia.razor` com 6).

| Padrão Repetido | Ocorrências | Proposta de Token / Classe em `wwwroot/app.css` | Prioridade | Esforço |
|---|---|---|---|---|
| `background-color: rgba(0,0,0,0.5)` / `0.6; z-index: 1055` (modais) | 10+ | `--mech-modal-backdrop: rgba(0, 0, 0, 0.55);` + classe `.mech-modal-backdrop` | **Média** | **P** |
| `font-size: 0.7rem / 0.75rem / 0.85rem / 0.9rem` | 17+ | `--mech-fs-2xs: 0.7rem; --mech-fs-xs: 0.75rem; --mech-fs-sm: 0.85rem; --mech-fs-md: 0.9rem;` + classes `.mech-text-2xs`, `.mech-text-xs`, `.mech-text-sm` | **Baixa** | **M** |
| `white-space: pre-wrap;` (+ `line-height: 1.6`) | 6 | Classe `.mech-pre-wrap` | **Baixa** | **P** |
| `width: 2rem; height: 2rem;` (checkboxes do mecânico) | 3 | Classe `.mech-check-lg` | **Baixa** | **P** |
| `color: var(--accent);` / `color: #6f42c1;` / `#27ae60` | 8+ | `--mech-purple: #6f42c1; --mech-profit: #1e8449;` (ajustado para contraste) + classes `.mech-text-accent`, `.mech-text-purple` | **Baixa** | **P** |
| `max-width: 620px / 650px / 700px` e larguras de coluna (`60px`, `100px`, `160px`) | 10+ | Classes `.mech-max-w-md` (`650px`) e `.mech-col-w-xs/sm/md` | **Baixa** | **P** |

*(Observação: estilos dinâmicos calculados em runtime, como `left: @(avaria.X)%` em `VistoriaChecklist.razor` ou larguras de barra de gráfico em `Home.razor`, devem permanecer inline.)*

---

## 8. Microtextos (pt-BR)

| Arquivo | Linha(s) | Achado | Prioridade | Esforço | Correção Proposta |
|---|---|---|---|---|---|
| `ReconnectModal.razor` & `MainLayout.razor` | `Reconnect:9–29`; `MainLayout:36–40` | Textos de reconexão e erro fatal em inglês. | **Alta** | **P** | Traduzir integralmente para pt-BR claro e acolhedor. |
| `VistoriaChecklist.razor` | 43, 212 | `aria-label="Close"` em inglês; botão cancelar marcador exibe apenas `"X"` sem `aria-label`. | **Média** | **P** | Trocar para `aria-label="Fechar"` e `aria-label="Cancelar marcação"`. |
| `OrdensServico/Index.razor`, `Clientes/Listar.razor`, `Veiculos/Listar.razor` | Estados vazios | Mensagens secas como `"Nenhuma Ordem de Serviço encontrada."` sem orientar próximo passo. | **Baixa** | **P** | Ajustar para indicar ação (ex.: `"Nenhuma O.S. encontrada. Limpe os filtros ou crie uma nova O.S."`). |

---

## 9. Itens Fora do Escopo Agora (Exigem mudança de lógica ou componentes grandes)

- **Refatoração do mapa de avarias (`VistoriaChecklist.razor`)**: Substituição de `OnMapClickedStatic` (`[JSInvokable]` estático) por referência de instância (`DotNetObjectReference`) ou biblioteca de desenho/canvas — exige mudança de arquitetura JS/Blazor.
- **Quebra de componentes monolíticos (`Details.razor` com 1.899 linhas e `Home.razor` com 853 linhas)**: Extrair abas/modais em subcomponentes independentes aumenta o risco de regressão antes da apresentação de 16/11/2026. **Marcado: fora do escopo agora.**
- **Combobox com busca assíncrona em `Create.razor` (Veículos/Serviços/Peças) e paginação no `PainelMecanico.razor`**: Exige alteração em `Services/` e contratos de `Queries/`. **Marcado: fora do escopo agora.**
- **Toast com ação "Desfazer" (Undo) e skeleton screens com atraso programado de 300ms**: Exige alteração de fluxo de persistência/estado em serviços de domínio. **Marcado: fora do escopo agora.**

---

## Tabela de Resumo

| # | Tema | Prioridade | Esforço | Arquivos Principais |
|---|---|---|---|---|
| 1 | Dependências externas offline (Google Fonts, Bootstrap Icons, IMask) | **Alta** | **P** | `Components/App.razor`, `wwwroot/lib/` |
| 2 | Reconexão e erro global em pt-BR (`#components-reconnect-modal`, `#blazor-error-ui`) | **Alta** | **P** | `ReconnectModal.razor`, `ReconnectModal.razor.css`, `MainLayout.razor` |
| 3 | `ErrorBoundary` + estados de erro/carregamento faltantes | **Alta** | **P** | `MainLayout.razor`, `Acompanhar/Index.razor`, `Imprimir*.razor`, `Details.razor` |
| 4 | Contraste WCAG (badges/textos warning, info, laranja e muted) | **Alta** | **P** | `wwwroot/app.css`, `Acompanhar/Index.razor`, `PainelMecanico.razor`, `Details.razor` |
| 5 | Responsividade crítica (360/390/768px) e alvos de toque 44px | **Alta** | **M** | `Acompanhar/Index.razor.css`, `PainelMecanico.razor`, `VistoriaChecklist.razor(.css)`, `Details.razor`, `Create.razor` |
| 6 | Impressão (`@media print` + logo + quebra de página) | **Alta** | **P** | `ImprimirOS`, `ImprimirTermoGarantia`, `ImprimirVistoria` (`.razor` e `.css`), `wwwroot/app.css` |
| 7 | Acessibilidade de modais, `aria-label` e `prefers-reduced-motion` | **Média** | **M** | `PainelMecanico`, `Clientes/Listar`, `Veiculos/Listar`, `Servicos/Listar`, `Estoque/Index`, `Configuracoes/Index`, `Home` |
| 8 | Consolidação de estilos inline em tokens `--mech-*` e classes CSS | **Baixa** | **M** | `wwwroot/app.css`, `Home.razor`, `Relatorios/Index.razor`, `PainelMecanico.razor`, `Details.razor` |
| 9 | Microtextos de estados vazios | **Baixa** | **P** | `OrdensServico/Index.razor`, `Clientes/Listar.razor`, `Veiculos/Listar.razor` |

---

## Proposta de Ordem de Execução (Etapa B — 1 commit por tema)

1. **`chore(front): hospedar fontes, icones e imask localmente`** — Garante funcionamento 100% offline na apresentação.
2. **`chore(front): traduzir e estilizar modal de reconexao e erro global`** — Elimina textos em inglês de sistema.
3. **`fix(front): adicionar ErrorBoundary e estados de erro/nao encontrado`** — Evita telas travadas em spinner ou "Link Inválido" falso.
4. **`fix(a11y): corrigir contraste de status e tokens de texto`** — Corrige falhas críticas de legibilidade (amarelo/ciano/cinza claro).
5. **`fix(front): ajustar responsividade mobile/tablet e toque na vistoria`** — Cobre 360, 390, 768 px, alvos de 44 px no Painel do Mecânico e toque nos marcadores da Vistoria.
6. **`fix(print): incluir logo, quebras de pagina e ocultacao global na impressao`** — Deixa OS, Termo de Garantia e Vistoria prontos para PDF/impressão.
7. **`fix(a11y): adicionar roles/Esc em modais, aria-labels e reduced-motion`** — Acessibilidade completa de teclado e leitores de tela.
8. **`refactor(css): extrair estilos inline repetidos para tokens --mech-*`** — Limpeza CSS sem alterar o visual.
9. **`test(e2e): validar telas com Playwright em 360, 390, 768 e 1280 px`** — Capturas antes/depois em `Documentacao/capturas-frontend/` e conferência final de `dotnet build` / `dotnet test`.
