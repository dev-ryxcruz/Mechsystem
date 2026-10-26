# Guia Visual — MechSystem

Referência rápida de tokens visuais, componentes de interface e manutenção do menu principal.

---

## 1. Tokens de Medida e Cor (`wwwroot/app.css` — `:root`)

### Medidas de Layout e Navegação
| Token | Valor | Uso |
|---|---|---|
| `--topbar-height` | `3.5rem` (`56px`) | Altura fixa compartilhada pelo cabeçalho da sidebar e pela barra superior (`main > .top-row`) |
| `--sidebar-width` | `250px` | Largura da sidebar expandida em desktop (`>= 768px`) |
| `--sidebar-width-collapsed` | `4.5rem` (`72px`) | Largura da sidebar recolhida (`.sidebar--collapsed`) |
| `--nav-item-height` | `2.75rem` (`44px`) | Altura dos links e botões de grupo do menu (garante alvo de toque `>= 44px`) |
| `--nav-item-gap` | `.125rem` (`2px`) | Espaçamento vertical entre itens consecutivos da sidebar |
| `--nav-pad-x` | `1rem` (`16px`) | Recuo horizontal único compartilhado pelo cabeçalho, títulos de seção, caixas de item e ícones |

### Cores Principais
| Token | Valor | Uso |
|---|---|---|
| `--accent` / `--accent-hover` | `#ff751f` / `#e05c0d` | Cor de destaque da marca, item ativo, anel de foco (`:focus-visible`) e CTAs primários |
| `--accent-glow` / `--accent-subtle` | `rgba(255,117,31,0.35)` / `rgba(255,117,31,0.10)` | Sombras de foco e fundos sutis de hover/destaque |
| `--bg-body` / `--bg-card` | `#f0f2f5` / `#ffffff` | Fundo geral da aplicação e superfície de cards |
| `--bg-sidebar` / `--bg-sidebar-deep` | `#1a1d21` / `#14161a` | Superfície escura da sidebar e tooltips |
| `--text-primary` / `--text-secondary` | `#1a1a2e` / `#6c757d` | Textos primários e secundários em fundo claro |
| `--text-on-dark` / `--text-on-dark-muted` | `#e4e6ea` / `#8b949e` | Textos em superfície escura (exclusivo para fundos escuros; em fundo claro usar `#4b5563` para contraste `>= 4.5:1`) |
| `--border-light` / `--border-dark` | `rgba(0,0,0,0.06)` / `rgba(255,255,255,0.06)` | Divisores e bordas sutis em fundos claros e escuros |

---

## 2. Convenção de Card e KPI

- **Cards padrão (`.card`, `.dash-card`)**: fundo `--bg-card` (`#ffffff`), `border-radius: var(--radius-md)` (`10px`), sombra `--shadow-md` e borda sutil `1px solid rgba(0,0,0,0.05)`.
- **Cards de KPI (`.kpi-card.stat-card`)**:
  - Borda superior de 4px indicando a categoria semântica (`.border-top-accent`, `.border-top-primary`, `.border-top-success`, `.border-top-dark`).
  - Rótulo superior (`.kpi-label`) em caixa alta (`0.75rem`, `font-weight: 700`, `letter-spacing: 0.5px`).
  - Valor principal (`.kpi-value`) em destaque (`1.6rem`, `font-weight: 800`) e subtexto contextual (`.stat-subtext`, `0.75rem`).
  - Ícone à direita dentro de `.stat-icon` (`50x50px`, `border-radius: 0.5rem`).

---

## 3. Estado Vazio e Badges de Status

- **Estado vazio (Empty State)**:
  - Textos e ícones centralizados com `text-muted`, ícone `fs-2 mb-2` e mensagem curta explicativa.
  - Em gráficos do tipo rosca/donut (`.donut-chart`), quando o total do período é zero (`faturamentoMes == 0`), renderizar o anel em cinza neutro (`#e9ecef`) e os indicadores da legenda em tons neutros (`#adb5bd` / `#ced4da` + `.text-muted`), evitando que uma fatia de 0%–100% simule 100% de uma categoria.
- **Badges de status (`.badge`)**:
  - `Orcamento`: `bg-warning text-dark`
  - `EmAndamento`: `bg-primary`
  - `Pausada` / `AguardandoPecas`: `bg-info text-dark` / `bg-secondary`
  - `Concluida`: `bg-success`
  - `Cancelada`: `bg-danger`

---

## 4. Como Adicionar um Item ao Menu (`Components/Layout/NavConfig.cs`)

O menu lateral é orientado a dados em `Components/Layout/NavConfig.cs` e renderizado por `NavMenu.razor` + `NavGroup.razor`.

- **Link direto (1 linha)** dentro da seção desejada em `NavConfig.Sections`:
  ```csharp
  NavItemDefinition.Link("Fila de Compras", "/compras/fila", "bi-cart-check-fill", NavLinkMatch.Prefix, NavConfig.AdminOrAtendimento)
  ```
- **Grupo expansível** com subitens:
  ```csharp
  NavItemDefinition.Group(
      "fornecedores",
      "Fornecedores",
      "bi-truck",
      NavConfig.AdminOrAtendimento,
      NavItemDefinition.Link("Novo Fornecedor", "/fornecedores/cadastro", "bi-plus-circle-fill"),
      NavItemDefinition.Link("Listar Fornecedores", "/fornecedores", "bi-list-ul", NavLinkMatch.All))
  ```
- **Perfis (`roles`)**:
  - `null`: visível para todos os perfis autenticados (`Administrador`, `Atendimento`, `Mecanico`).
  - `NavConfig.AdminOrAtendimento`: visível para `Administrador` e `Atendimento`.
  - `NavConfig.AdminOnly`: visível apenas para `Administrador`.
