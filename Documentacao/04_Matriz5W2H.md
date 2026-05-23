# Matriz 5W2H — MechSystem

**Projeto**: MechSystem — Sistema de Gestão para Oficinas Mecânicas  
**Versão**: 1.0  
**Data**: Maio/2026  
**Autor**: Ryan Cristian  
**Disciplina**: Engenharia de Software III — Prof. Alessandro Fukuta

---

## 1. Objetivo

Aplicar a ferramenta **5W2H** para detalhar o planejamento do projeto MechSystem, respondendo às 7 perguntas fundamentais que orientam a execução do projeto.

---

## 2. Matriz 5W2H — Visão Geral do Projeto

| Pergunta | Resposta |
|----------|---------|
| **What (O quê?)** | Desenvolvimento do **MechSystem**, uma aplicação web para gestão completa de oficinas mecânicas, cobrindo atendimento, ordens de serviço, estoque, financeiro e relatórios gerenciais. |
| **Why (Por quê?)** | O mercado de oficinas mecânicas no Brasil é predominantemente **manual e desorganizado**. A maioria opera com fichas de papel, planilhas ou sistemas caros e complexos. O MechSystem resolve esse problema com uma solução **acessível, moderna e integrada**. |
| **Where (Onde?)** | O sistema será acessível via **navegador web** (Chrome, Edge, Firefox), rodando localmente na rede da oficina ou em servidor na nuvem. O desenvolvimento é realizado em ambiente Windows com deploy preparado para Linux/Docker. |
| **When (Quando?)** | **Início**: Março/2026. **Entrega v1.0**: Junho/2026. **Manutenção evolutiva**: Contínua a partir de Julho/2026. |
| **Who (Quem?)** | **Desenvolvimento**: Ryan Cristian (Full-Stack Developer). **Stakeholders**: Proprietários de oficinas mecânicas, atendentes, mecânicos. **Orientação acadêmica**: Prof. Alessandro Fukuta (ES3). |
| **How (Como?)** | Utilizando stack **.NET 10 + Blazor Server + EF Core 10 + SQLite**, seguindo arquitetura em camadas (Models → Repositories → Services → Components), metodologia iterativa incremental e versionamento Git. |
| **How Much (Quanto?)** | **Custo de desenvolvimento**: ~R$ 45.000 (estimativa baseada em 450h × R$ 100/h). **Custo de infraestrutura**: R$ 0 em desenvolvimento (SQLite local), R$ 50-200/mês em produção (VPS/Cloud). **Licenciamento ao cliente**: R$ 150-250/mês (modelo SaaS). |

---

## 3. Detalhamento por Módulo (5W2H Operacional)

### 3.1 Módulo de Autenticação e Autorização

| Pergunta | Resposta |
|----------|---------|
| **What** | Sistema de login seguro com controle de acesso baseado em perfis (RBAC) |
| **Why** | Proteger dados sensíveis e garantir que cada colaborador acesse apenas o que compete ao seu perfil |
| **Where** | Tela de login (`/login`) e middleware de autorização global |
| **When** | Sprint 1 — Primeira funcionalidade implementada |
| **Who** | Administrador configura usuários; Atendimento e Mecânico consomem |
| **How** | Cookie Authentication + BCrypt.Net-Next para hash de senhas + Enum `PerfilUsuario` (Administrador, Atendimento, Mecânico) |
| **How Much** | ~20h de desenvolvimento |

### 3.2 Módulo de Clientes e Veículos

| Pergunta | Resposta |
|----------|---------|
| **What** | CRUD completo de clientes (CPF, nome, contato) e veículos (placa, marca, modelo, ano, KM) vinculados |
| **Why** | Centralizar o cadastro e eliminar fichas em papel, permitindo busca rápida e histórico |
| **Where** | Páginas `/clientes` e `/veiculos` |
| **When** | Sprint 2 |
| **Who** | Atendimento cadastra; Administrador gerencia |
| **How** | Entity Framework Core + Repository Pattern (`ClienteRepository`, `VeiculoRepository`) |
| **How Much** | ~30h de desenvolvimento |

### 3.3 Módulo de Ordens de Serviço

| Pergunta | Resposta |
|----------|---------|
| **What** | Gestão do ciclo de vida completo da OS: Orçamento → Aguardando Peças → Em Andamento → Concluída/Cancelada |
| **Why** | É o **core business** do sistema — sem OS não há controle de serviços |
| **Where** | Páginas `/ordens-servico`, `/os/criar`, `/os/{id}` |
| **When** | Sprint 3 — Maior módulo do sistema |
| **Who** | Atendimento cria; Mecânico executa; Administrador supervisiona |
| **How** | Máquina de estados (`OrdemServicoStatus`), vínculo de peças (`OrdemServicoPeca`), cálculo automático de valores, autorização do cliente |
| **How Much** | ~80h de desenvolvimento |

### 3.4 Módulo de Vistoria de Entrada

| Pergunta | Resposta |
|----------|---------|
| **What** | Checklist de entrada do veículo: combustível, KM, itens (estepe, macaco, triângulo), mapeamento visual de avarias |
| **Why** | Proteger a oficina contra reclamações indevidas e garantir conformidade com CDC |
| **Where** | Vinculada à OS (1:1) — tela dentro do fluxo de criação de OS |
| **When** | Sprint 3 (junto com OS) |
| **Who** | Atendimento ou Mecânico registra |
| **How** | Modelo `Vistoria` com checklist booleano + avarias em JSON (`AvariasJson`) |
| **How Much** | ~25h de desenvolvimento |

### 3.5 Módulo de Estoque

| Pergunta | Resposta |
|----------|---------|
| **What** | Controle de peças com SKU, preço de custo/venda, estoque mínimo, localização, movimentações (entrada/saída/ajuste) |
| **Why** | Evitar rupturas de estoque, controlar margem de lucro e rastrear uso de peças nas OS |
| **Where** | Páginas `/estoque`, integrado com OS via `OrdemServicoPeca` |
| **When** | Sprint 4 |
| **Who** | Administrador gerencia; Sistema movimenta automaticamente |
| **How** | `EstoqueService` com regras de negócio, `MovimentacaoEstoque` para auditoria |
| **How Much** | ~40h de desenvolvimento |

### 3.6 Módulo de Relatórios e Dashboard

| Pergunta | Resposta |
|----------|---------|
| **What** | Dashboard com KPIs (ticket médio, funil, receitas) + relatórios especializados (Financeiro, OS, Estoque) |
| **Why** | Fornecer inteligência de negócio para tomada de decisão |
| **Where** | Página `/` (Home/Dashboard) e `/relatorios` |
| **When** | Sprint 5 |
| **Who** | Administrador consulta |
| **How** | Classe abstrata `Relatorio` com herança (`RelatorioOS`, `RelatorioFinanceiro`, `RelatorioEstoque`) — demonstra polimorfismo |
| **How Much** | ~50h de desenvolvimento |

### 3.7 Módulo de Configurações

| Pergunta | Resposta |
|----------|---------|
| **What** | Parâmetros dinâmicos: nome da oficina, CNPJ, validade de orçamento, garantia padrão, moeda, taxa de mão de obra |
| **Why** | Permitir que cada oficina personalize o sistema sem alterar código |
| **Where** | Página `/configuracoes` |
| **When** | Sprint 2 |
| **Who** | Apenas Administrador |
| **How** | Modelo `Configuracao` singleton no banco + `ConfiguracaoService` |
| **How Much** | ~15h de desenvolvimento |

---

## 4. Resumo de Investimento

| Item | Horas | Custo Estimado |
|------|-------|---------------|
| Módulo de Autenticação | 20h | R$ 2.000 |
| Módulo de Clientes/Veículos | 30h | R$ 3.000 |
| Módulo de Ordens de Serviço | 80h | R$ 8.000 |
| Módulo de Vistoria | 25h | R$ 2.500 |
| Módulo de Estoque | 40h | R$ 4.000 |
| Módulo de Relatórios/Dashboard | 50h | R$ 5.000 |
| Módulo de Configurações | 15h | R$ 1.500 |
| Infraestrutura e Deploy | 20h | R$ 2.000 |
| UI/UX e Design | 40h | R$ 4.000 |
| Testes e Documentação | 30h | R$ 3.000 |
| Gestão de Projeto | 20h | R$ 2.000 |
| Contingência (15%) | 55h | R$ 5.500 |
| **TOTAL** | **425h** | **R$ 42.500** |

---

## 5. Conclusão

A Matriz 5W2H demonstra que o MechSystem é um projeto **bem definido e planejado**, com escopo claro, tecnologias justificadas, responsabilidades atribuídas e orçamento estimado. Cada módulo foi detalhado individualmente para garantir rastreabilidade e controle de execução.

---

*Documento elaborado como artefato da disciplina de Engenharia de Software III — FATEC 2026/1*
