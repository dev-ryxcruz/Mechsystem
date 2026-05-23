# Métricas e Custo do Projeto — MechSystem

**Projeto**: MechSystem — Sistema de Gestão para Oficinas Mecânicas  
**Versão**: 1.0  
**Data**: Maio/2026  
**Autor**: Ryan Cristian  
**Disciplina**: Engenharia de Software III — Prof. Alessandro Fukuta

---

## 1. Objetivo

Estimar o **custo do projeto MechSystem** utilizando métricas de software baseadas no **Diagrama de Classe** e na técnica de **Análise por Pontos de Função (APF)**, complementada pela contagem de entidades do MER (Modelo Entidade-Relacionamento).

---

## 2. Métricas Estruturais (Baseadas no Diagrama de Classe)

### 2.1 Contagem de Entidades

| Tipo | Quantidade | Itens |
|------|-----------|-------|
| Classes Concretas | 15 | Cliente, Veiculo, OrdemServico, OrdemServicoPeca, Vistoria, ContatoOS, Peca, MovimentacaoEstoque, Servico, Usuario, Configuracao, Login, RelatorioOS, RelatorioFinanceiro, RelatorioEstoque |
| Classes Abstratas | 1 | Relatorio |
| Enumerações | 6 | OrdemServicoStatus, VistoriaStatus, NivelCombustivel, PerfilUsuario, TipoMovimentacao, TipoContato |
| Interfaces | 5 | IClienteRepository, IVeiculoRepository, IServicoRepository, IPecaRepository, IAuthService |
| Services | 4 | AuthService, UsuarioService, ConfiguracaoService, EstoqueService |
| Repositories | 4 | ClienteRepository, VeiculoRepository, ServicoRepository, PecaRepository |
| **Total de Classes/Interfaces** | **35** | |

### 2.2 Contagem de Atributos e Métodos

| Entidade | Atributos | Métodos / Props Calculadas | Relacionamentos |
|----------|-----------|---------------------------|----------------|
| Cliente | 6 | 0 | 1 (→ Veiculo) |
| Veiculo | 8 | 0 | 2 (→ Cliente, → OS) |
| OrdemServico | 15 | 3 (ValorPecasEfetivo, ValorTotal, GetValidadeOrcamento) | 4 (→ Veiculo, Vistoria, PecasUtilizadas, Contatos) |
| OrdemServicoPeca | 7 | 2 (Subtotal, TemDescontoAbaixoDoMinimo) | 2 (→ OS, → Peca) |
| Vistoria | 12 | 0 | 1 (→ OS) |
| ContatoOS | 6 | 0 | 1 (→ OS) |
| Peca | 11 | 2 (AbaixoDoMinimo, MargemLucro) | 1 (→ Movimentacoes) |
| MovimentacaoEstoque | 7 | 0 | 2 (→ Peca, → Usuario) |
| Servico | 5 | 0 | 0 |
| Usuario | 7 | 0 | 0 |
| Configuracao | 13 | 0 | 0 |
| Login | 2 | 0 | 0 |
| Relatorio (abstrata) | 3 | 5 (3 abstratos + 2 concretos) | 0 |
| RelatorioOS | 8 | 3 (overrides) | 0 |
| RelatorioFinanceiro | 6 | 5 (2 props + 3 overrides) | 0 |
| RelatorioEstoque | 6 | 3 (overrides) | 0 |
| **TOTAL** | **~122** | **~23** | **~14** |

---

## 3. Análise por Pontos de Função (APF)

### 3.1 Identificação das Funções de Dados

#### ALI — Arquivos Lógicos Internos (dados mantidos pela aplicação)

| # | ALI | DER (Dados) | RLR (Registros) | Complexidade |
|---|-----|------------|-----------------|-------------|
| 1 | Cliente | 6 | 1 | Simples |
| 2 | Veiculo | 8 | 1 | Simples |
| 3 | OrdemServico | 15 | 3 (OS, Peças, Contatos) | Complexa |
| 4 | Vistoria | 12 | 1 | Média |
| 5 | Peca | 11 | 2 (Peca, Movimentações) | Média |
| 6 | Servico | 5 | 1 | Simples |
| 7 | Usuario | 7 | 1 | Simples |
| 8 | Configuracao | 13 | 1 | Média |
| 9 | MovimentacaoEstoque | 7 | 1 | Simples |
| 10 | ContatoOS | 6 | 1 | Simples |
| 11 | OrdemServicoPeca | 7 | 1 | Simples |

**Contagem ALI:**

| Complexidade | Quantidade | Peso | Subtotal |
|-------------|-----------|------|----------|
| Simples | 7 | 7 | 49 |
| Média | 3 | 10 | 30 |
| Complexa | 1 | 15 | 15 |
| **Total ALI** | **11** | | **94 PF** |

#### AIE — Arquivos de Interface Externa
Não há arquivos de interface externa na versão 1.0 (sem integração com sistemas externos).

| **Total AIE** | **0** | | **0 PF** |

### 3.2 Identificação das Funções Transacionais

#### EE — Entradas Externas

| # | EE | Complexidade | Justificativa |
|---|-----|-------------|--------------|
| 1 | Cadastrar Cliente | Simples | 1 ALI, ≤ 5 DER |
| 2 | Editar Cliente | Simples | 1 ALI, ≤ 5 DER |
| 3 | Excluir Cliente | Simples | 1 ALI, 1 DER |
| 4 | Cadastrar Veículo | Simples | 1 ALI, ≤ 5 DER |
| 5 | Editar Veículo | Simples | 1 ALI, ≤ 5 DER |
| 6 | Cadastrar Serviço | Simples | 1 ALI, ≤ 5 DER |
| 7 | Criar OS | Complexa | 3+ ALI, 15+ DER |
| 8 | Atualizar Status OS | Média | 1 ALI, ≤ 10 DER |
| 9 | Registrar Autorização | Média | 1 ALI, ≤ 5 DER |
| 10 | Vincular Peça à OS | Complexa | 3 ALI (OS, Peca, Mov), 7+ DER |
| 11 | Realizar Vistoria | Média | 1 ALI, 12 DER |
| 12 | Cadastrar Peça | Média | 1 ALI, 11 DER |
| 13 | Movimentação Estoque | Média | 2 ALI (Peca, Mov), 7 DER |
| 14 | Cadastrar Usuário | Simples | 1 ALI, ≤ 7 DER |
| 15 | Login | Média | 1 ALI, validação + Cookie |
| 16 | Configurar Sistema | Média | 1 ALI, 13 DER |
| 17 | Registrar Contato OS | Simples | 1 ALI, 6 DER |

**Contagem EE:**

| Complexidade | Quantidade | Peso | Subtotal |
|-------------|-----------|------|----------|
| Simples | 7 | 3 | 21 |
| Média | 8 | 4 | 32 |
| Complexa | 2 | 6 | 12 |
| **Total EE** | **17** | | **65 PF** |

#### SE — Saídas Externas

| # | SE | Complexidade | Justificativa |
|---|-----|-------------|--------------|
| 1 | Dashboard BI | Complexa | Múltiplos ALI, cálculos derivados |
| 2 | Relatório de OS | Complexa | Múltiplos ALI, agregações |
| 3 | Relatório Financeiro | Complexa | Múltiplos ALI, cálculos |
| 4 | Relatório de Estoque | Média | 2 ALI, agregações |
| 5 | Impressão de OS | Média | 3 ALI, formatação especial |
| 6 | Alerta de Estoque | Simples | 1 ALI, comparação |

**Contagem SE:**

| Complexidade | Quantidade | Peso | Subtotal |
|-------------|-----------|------|----------|
| Simples | 1 | 4 | 4 |
| Média | 2 | 5 | 10 |
| Complexa | 3 | 7 | 21 |
| **Total SE** | **6** | | **35 PF** |

#### CE — Consultas Externas

| # | CE | Complexidade |
|---|-----|-------------|
| 1 | Listar Clientes | Simples |
| 2 | Listar Veículos | Simples |
| 3 | Listar Serviços | Simples |
| 4 | Listar OS | Média |
| 5 | Detalhar OS | Complexa |
| 6 | Listar Peças | Média |
| 7 | Listar Movimentações | Simples |
| 8 | Listar Usuários | Simples |
| 9 | Consultar Configuração | Simples |
| 10 | Acompanhar OS (Token) | Simples |

**Contagem CE:**

| Complexidade | Quantidade | Peso | Subtotal |
|-------------|-----------|------|----------|
| Simples | 7 | 3 | 21 |
| Média | 2 | 4 | 8 |
| Complexa | 1 | 6 | 6 |
| **Total CE** | **10** | | **35 PF** |

### 3.3 Total de Pontos de Função (Não Ajustados)

| Tipo | Quantidade | Pontos |
|------|-----------|--------|
| ALI (Arq. Lógicos Internos) | 11 | 94 |
| AIE (Arq. Interface Externa) | 0 | 0 |
| EE (Entradas Externas) | 17 | 65 |
| SE (Saídas Externas) | 6 | 35 |
| CE (Consultas Externas) | 10 | 35 |
| **TOTAL PF (Não Ajustados)** | **44 funções** | **229 PF** |

### 3.4 Fator de Ajuste

| # | Característica Geral do Sistema | Grau (0-5) |
|---|-------------------------------|-----------|
| 1 | Comunicação de dados | 4 |
| 2 | Processamento distribuído | 1 |
| 3 | Performance | 3 |
| 4 | Configuração de equipamento | 2 |
| 5 | Volume de transações | 2 |
| 6 | Entrada de dados online | 5 |
| 7 | Eficiência do usuário final | 4 |
| 8 | Atualização online | 4 |
| 9 | Processamento complexo | 3 |
| 10 | Reusabilidade | 3 |
| 11 | Facilidade de instalação | 4 |
| 12 | Facilidade de operação | 4 |
| 13 | Múltiplos locais | 2 |
| 14 | Facilidade de mudanças | 3 |
| **Total DI** | | **44** |

**Fator de Ajuste (VAF):**
VAF = 0,65 + (0,01 × 44) = **1,09**

**Pontos de Função Ajustados:**
PFA = 229 × 1,09 = **249,61 ≈ 250 PF**

---

## 4. Estimativa de Custo

### 4.1 Produtividade e Custo por PF

| Parâmetro | Valor | Referência |
|-----------|-------|-----------|
| Produtividade (horas/PF) | 8h/PF | Média ISBSG para .NET Web App |
| Custo hora desenvolvedor | R$ 100,00 | Mercado Júnior/Pleno Brasil 2026 |
| Overhead de gestão | 15% | Padrão de projetos de TI |

### 4.2 Cálculo de Esforço

| Item | Cálculo | Resultado |
|------|---------|----------|
| Esforço de desenvolvimento | 250 PF × 8h/PF | **2.000 horas** |
| Overhead de gestão (15%) | 2.000 × 0,15 | **300 horas** |
| **Esforço Total** | | **2.300 horas** |

### 4.3 Cálculo de Custo

| Item | Cálculo | Resultado |
|------|---------|----------|
| Custo de desenvolvimento | 2.000h × R$ 100 | R$ 200.000,00 |
| Custo de gestão | 300h × R$ 100 | R$ 30.000,00 |
| Infraestrutura (dev) | 4 meses × R$ 0 | R$ 0,00 |
| Ferramentas e licenças | .NET (gratuito) + SQLite (gratuito) | R$ 0,00 |
| **Custo Total Estimado** | | **R$ 230.000,00** |

### 4.4 Custo Ajustado (Projeto Acadêmico — 1 Desenvolvedor)

> **Nota**: O cálculo acima representa o custo de mercado para desenvolvimento profissional completo. Para o contexto acadêmico (1 desenvolvedor, reuso de conhecimento, sem overhead de equipe), aplica-se um fator de ajuste:

| Item | Valor |
|------|-------|
| Horas reais investidas (estimativa) | ~450h |
| Custo hora acadêmico | R$ 50,00 |
| **Custo real acadêmico** | **R$ 22.500,00** |
| **Custo de mercado (referência)** | **R$ 230.000,00** |

---

## 5. Métricas Adicionais

### 5.1 Métricas de Código

| Métrica | Valor |
|---------|-------|
| Linhas de código (Models) | ~650 LOC |
| Linhas de código (Services) | ~350 LOC |
| Linhas de código (Repositories) | ~400 LOC |
| Linhas de código (Components/Pages) | ~2.500 LOC (estimativa) |
| Linhas de código (CSS) | ~800 LOC (estimativa) |
| **Total estimado de LOC** | **~4.700 LOC** |
| Ratio LOC/PF | 4.700 / 250 = **18,8 LOC/PF** |

### 5.2 Métricas de Complexidade

| Métrica | Valor | Interpretação |
|---------|-------|-------------|
| Número de entidades | 17 | Média complexidade |
| Número de relacionamentos | 14 | Boa coesão |
| Profundidade de herança máx. | 2 (Relatorio → Subclasses) | Baixa complexidade de herança |
| Acoplamento médio | 2,3 relacionamentos/entidade | Baixo acoplamento |
| Coesão | Alta | Classes com responsabilidade única |

### 5.3 Métricas de Qualidade

| Métrica | Valor |
|---------|-------|
| Requisitos implementados | 49/49 (100%) |
| Regras de negócio cobertas | 18/18 (100%) |
| Casos de uso cobertos | 25/25 (100%) |
| Padrões de design aplicados | 4 (Repository, DI, Snapshot, Graceful Degradation) |

---

## 6. Conclusão

O MechSystem possui **250 Pontos de Função Ajustados**, classificando-o como um sistema de **média complexidade**. O custo de mercado estimado é de **R$ 230.000,00** (2.300 horas profissionais), enquanto o custo real acadêmico ficou em torno de **R$ 22.500,00** (450 horas). As métricas de qualidade demonstram cobertura completa de requisitos e baixo acoplamento no design.

---

*Documento elaborado como artefato da disciplina de Engenharia de Software III — FATEC 2026/1*
