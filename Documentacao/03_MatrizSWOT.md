# Matriz SWOT — MechSystem

**Projeto**: MechSystem — Sistema de Gestão para Oficinas Mecânicas  
**Versão**: 1.0  
**Data**: Maio/2026  
**Autor**: Ryan Cristian  
**Disciplina**: Engenharia de Software III — Prof. Alessandro Fukuta

---

## 1. Objetivo

Realizar a análise estratégica do projeto MechSystem utilizando a ferramenta **SWOT** (Strengths, Weaknesses, Opportunities, Threats), identificando fatores internos e externos que influenciam o sucesso do produto.

---

## 2. Matriz SWOT

### 2.1 Ambiente Interno

#### 🟢 Forças (Strengths)

| # | Força | Impacto |
|---|-------|---------|
| S1 | **Tecnologia moderna** (.NET 10, Blazor Server, EF Core 10) — stack atualizada e com suporte de longo prazo | Alta performance e manutenibilidade |
| S2 | **Arquitetura limpa** (Repository Pattern, DI, separação Models/Services/Repositories) | Facilidade de extensão e testes |
| S3 | **Banco de dados embutido** (SQLite) — zero configuração para implantação | Barreira de entrada mínima para oficinas |
| S4 | **Sistema de perfis RBAC** (Administrador, Atendimento, Mecânico) | Segurança e controle granular de acesso |
| S5 | **Dashboard com BI** (ticket médio, funil de serviços, receitas) | Tomada de decisão baseada em dados |
| S6 | **Vistoria de entrada documentada** (checklist + mapeamento de avarias) | Proteção jurídica contra reclamações |
| S7 | **Controle de estoque inteligente** (alertas, movimentação automática, margem de lucro) | Redução de perdas e rupturas |
| S8 | **UI/UX moderna** com CSS nativo, sidebar dinâmica e identidade visual coesa (#ff751f) | Experiência de uso premium |
| S9 | **Polimorfismo e herança** aplicados (Relatorio abstrato → subclasses) | Código extensível e didático |
| S10 | **Portabilidade de banco** (SQLite → PostgreSQL preparado) | Escalabilidade quando necessário |

#### 🔴 Fraquezas (Weaknesses)

| # | Fraqueza | Impacto |
|---|---------|---------|
| W1 | **Aplicação single-server** (Blazor Server depende de conexão WebSocket ativa) | Latência em conexões instáveis |
| W2 | **Sem módulo financeiro completo** (não gera NF-e, não integra com gateways) | Limitação para oficinas maiores |
| W3 | **Sem app mobile nativo** — apenas responsivo via browser | Experiência mobile limitada |
| W4 | **Equipe de desenvolvimento pequena** (1 desenvolvedor) | Risco de gargalo na manutenção |
| W5 | **Sem testes automatizados** na versão atual | Risco de regressão em atualizações |
| W6 | **Sem integração com sistemas externos** (DETRAN, seguradoras, fornecedores) | Limitação de automação |
| W7 | **Documentação de API limitada** | Dificulta integrações futuras |

---

### 2.2 Ambiente Externo

#### 🔵 Oportunidades (Opportunities)

| # | Oportunidade | Potencial |
|---|-------------|-----------|
| O1 | **Mercado de oficinas pouco digitalizado** — maioria ainda usa papel e planilhas | Grande base de clientes potenciais |
| O2 | **Crescimento do setor automotivo** — frota brasileira ultrapassa 115 milhões de veículos | Demanda crescente por manutenção |
| O3 | **Tendência de digitalização pós-pandemia** — MEIs e PMEs buscando ferramentas digitais | Aceleração da adoção |
| O4 | **Modelo SaaS acessível** — assinatura mensal atrativa para micro e pequenas oficinas | Receita recorrente |
| O5 | **Ecossistema .NET robusto** — comunidade ativa, atualizações constantes, suporte Microsoft | Longevidade da plataforma |
| O6 | **Possibilidade de marketplace** — integrar com fornecedores de peças | Nova fonte de receita |
| O7 | **IA e diagnóstico preditivo** — usar dados históricos para prever falhas | Diferencial competitivo |
| O8 | **LGPD** — oficinas precisam se adequar à proteção de dados | O sistema pode ser ferramenta de compliance |

#### 🟡 Ameaças (Threats)

| # | Ameaça | Risco |
|---|--------|-------|
| T1 | **Concorrentes estabelecidos** (Ultracar, Oficina Inteligente, Gestão Click) com maior market share | Perda de mercado |
| T2 | **Resistência à mudança** — proprietários tradicionais avessos à tecnologia | Baixa adoção |
| T3 | **Pirataria e cópia de software** no mercado brasileiro | Perda de receita |
| T4 | **Instabilidade econômica** — oficinas pequenas cortam custos de TI primeiro | Churn de clientes |
| T5 | **Evolução rápida de tecnologias** — risco de obsolescência se não atualizar | Necessidade de investimento contínuo |
| T6 | **Regulamentação tributária complexa** — NF-e, SPED exigem integrações caras | Aumento de escopo |

---

## 3. Análise Cruzada (Estratégias)

### 3.1 Estratégias SO (Forças × Oportunidades) — Ofensivas

| Estratégia | Combina |
|-----------|---------|
| Explorar a tecnologia moderna (S1, S2) para penetrar no mercado pouco digitalizado (O1) com custo competitivo | S1+S2 × O1+O4 |
| Usar o dashboard BI (S5) como diferencial de marketing para atrair oficinas que querem inteligência de dados | S5 × O3 |
| Oferecer banco SQLite embutido (S3) como facilidade de implantação instantânea para micro oficinas | S3 × O1+O4 |
| Preparar migração para PostgreSQL (S10) visando escalabilidade para redes de oficinas e franquias | S10 × O6 |

### 3.2 Estratégias WO (Fraquezas × Oportunidades) — Melhoria

| Estratégia | Combina |
|-----------|---------|
| Desenvolver app mobile (W3) para capturar a tendência de digitalização mobile (O3) | W3 × O3 |
| Implementar módulo NF-e (W2) para atender oficinas que buscam compliance tributário (O8) | W2 × O8 |
| Expandir equipe de desenvolvimento (W4) aproveitando o ecossistema .NET (O5) com pool de desenvolvedores | W4 × O5 |
| Criar suíte de testes automatizados (W5) para garantir qualidade em iterações rápidas | W5 × O5 |

### 3.3 Estratégias ST (Forças × Ameaças) — Defensivas

| Estratégia | Combina |
|-----------|---------|
| Usar a UI/UX premium (S8) e vistoria documentada (S6) como diferenciais contra concorrentes (T1) | S8+S6 × T1 |
| Manter custo acessível com SQLite (S3) para reter clientes em cenários de instabilidade (T4) | S3 × T4 |
| Investir em atualização constante (S1) com stack .NET para evitar obsolescência (T5) | S1 × T5 |
| RBAC robusto (S4) e criptografia BCrypt como barreira de segurança contra ameaças de dados | S4 × T3 |

### 3.4 Estratégias WT (Fraquezas × Ameaças) — Sobrevivência

| Estratégia | Combina |
|-----------|---------|
| Priorizar testes (W5) para evitar bugs que causem perda de clientes para concorrentes (T1) | W5 × T1 |
| Criar material de treinamento simples (W4) para reduzir a resistência à mudança (T2) | W4 × T2 |
| Documentar API (W7) para permitir integrações que mantenham competitividade (T5) | W7 × T5 |

---

## 4. Conclusão

A análise SWOT revela que o MechSystem possui uma **base tecnológica sólida** e está bem posicionado para atender um **mercado com baixa digitalização**. As principais ameaças vêm de concorrentes já estabelecidos e da resistência natural à mudança. A estratégia recomendada é **penetração de mercado via custo acessível e implantação simplificada**, com foco na experiência do usuário como diferencial competitivo.

---

*Documento elaborado como artefato da disciplina de Engenharia de Software III — FATEC 2026/1*
