# BPMN — Modelagem de Processos de Negócio — MechSystem

**Projeto**: MechSystem — Sistema de Gestão para Oficinas Mecânicas  
**Versão**: 1.0  
**Data**: Maio/2026  
**Autor**: Ryan Cristian  
**Disciplina**: Engenharia de Software III — Prof. Alessandro Fukuta

---

## 1. Objetivo

Modelar os **processos de negócio principais** do MechSystem utilizando a notação **BPMN (Business Process Model and Notation)**, representando os fluxos de trabalho entre os atores do sistema, desde o atendimento inicial até a entrega do veículo.

---

## 2. Processos Modelados

| # | Processo | Complexidade | Atores Envolvidos |
|---|---------|-------------|-------------------|
| 1 | Atendimento e Criação de OS | Alta | Cliente, Atendente, Sistema |
| 2 | Execução da Ordem de Serviço | Alta | Mecânico, Atendente, Sistema, Estoque |
| 3 | Gestão de Estoque | Média | Administrador, Sistema |
| 4 | Processo Completo (Ponta a Ponta) | Alta | Todos |

---

## 3. Processo 1 — Atendimento e Criação de Ordem de Serviço

### Descrição
Fluxo que inicia com a chegada do cliente à oficina e termina com a OS criada no sistema, incluindo orçamento, vistoria obrigatória e autorização do cliente.

### Diagrama BPMN

```mermaid
flowchart TD
    START(("🟢 Início")) --> A["Cliente chega<br/>à oficina"]
    A --> B{"Cliente já<br/>cadastrado?"}
    
    B -->|Não| C["Atendente cadastra<br/>Cliente (Nome, CPF,<br/>Telefone, Email)"]
    C --> D["Atendente cadastra<br/>Veículo (Placa, Marca,<br/>Modelo, Ano, KM)"]
    D --> E["Atendente coleta<br/>relato do problema"]
    
    B -->|Sim| B2{"Veículo já<br/>cadastrado?"}
    B2 -->|Não| D
    B2 -->|Sim| E
    
    E --> F["Atendente cria<br/>Ordem de Serviço<br/>(Status: Orçamento)"]
    F --> G{"Vistoria de entrada<br/>obrigatória?"}
    
    G -->|Sim| H["Atendente realiza<br/>Vistoria de Entrada"]
    H --> H1["Registra nível de<br/>combustível e KM"]
    H1 --> H2["Preenche checklist<br/>(estepe, macaco,<br/>triângulo, rádio,<br/>chave de roda)"]
    H2 --> H3["Mapeia avarias<br/>existentes no veículo"]
    H3 --> I["Sistema salva<br/>Vistoria vinculada à OS"]
    
    G -->|Não| J["Atendente seleciona<br/>serviços do catálogo"]
    I --> J
    
    J --> K["Sistema calcula<br/>valor de mão de obra"]
    K --> L{"Peças necessárias?"}
    
    L -->|Sim| M["Atendente vincula<br/>peças à OS"]
    M --> N["Sistema calcula<br/>valor total<br/>(MO + Peças)"]
    
    L -->|Não| N2["Sistema calcula<br/>valor total<br/>(apenas MO)"]
    
    N --> O["Sistema gera<br/>Orçamento com validade"]
    N2 --> O
    
    O --> P["Atendente apresenta<br/>orçamento ao cliente"]
    P --> Q{"Cliente autoriza<br/>o serviço?"}
    
    Q -->|Sim| R["Atendente registra<br/>autorização<br/>(Nome, Meio, Data)"]
    R --> S["Sistema atualiza<br/>Status: Em Andamento"]
    S --> END1(("🔴 Fim:<br/>OS Autorizada"))
    
    Q -->|Não| T{"Cliente deseja<br/>cancelar?"}
    T -->|Sim| U["Sistema atualiza<br/>Status: Cancelada"]
    U --> END2(("🔴 Fim:<br/>OS Cancelada"))
    T -->|Não| V["OS permanece como<br/>Orçamento aguardando<br/>resposta"]
    V --> END3(("🔴 Fim:<br/>Aguardando"))
```

### Regras de Negócio Envolvidas

| Regra | Descrição |
|-------|----------|
| RN01 | A vistoria de entrada pode ser configurada como obrigatória nas Configurações do sistema |
| RN02 | O orçamento tem validade configurável (padrão: 10 dias) |
| RN03 | A autorização exige registro de responsável, meio e data |
| RN04 | O valor total da OS = Mão de Obra + Valor Efetivo de Peças |

---

## 4. Processo 2 — Execução da Ordem de Serviço

### Descrição
Fluxo que cobre a execução dos serviços aprovados, desde a alocação do mecânico até a conclusão e entrega do veículo.

### Diagrama BPMN

```mermaid
flowchart TD
    START(("🟢 Início:<br/>OS Autorizada")) --> A["Mecânico recebe<br/>OS designada"]
    A --> B["Mecânico consulta<br/>detalhes da OS"]
    B --> C{"Peças disponíveis<br/>em estoque?"}
    
    C -->|Não| D["Sistema atualiza<br/>Status: Aguardando Peças"]
    D --> E["Atendente contata<br/>fornecedor"]
    E --> F["Peças chegam e são<br/>registradas no estoque<br/>(Entrada)"]
    F --> G["Sistema atualiza<br/>Status: Em Andamento"]
    
    C -->|Sim| G
    
    G --> H["Mecânico inicia<br/>execução do serviço"]
    H --> I["Sistema realiza baixa<br/>automática de peças<br/>no estoque"]
    I --> J["Mecânico executa<br/>serviços listados na OS"]
    
    J --> K{"Problema adicional<br/>identificado?"}
    K -->|Sim| L["Mecânico informa<br/>atendente"]
    L --> M["Atendente contata<br/>cliente para<br/>autorização adicional"]
    M --> N{"Cliente autoriza<br/>serviço adicional?"}
    N -->|Sim| O["Atendente atualiza<br/>OS com novos<br/>serviços/peças"]
    O --> J
    N -->|Não| J
    
    K -->|Não| P["Mecânico conclui<br/>todos os serviços"]
    P --> Q["Mecânico realiza<br/>teste de qualidade"]
    Q --> R{"Teste OK?"}
    
    R -->|Não| J
    R -->|Sim| S["Sistema atualiza<br/>Status: Concluída"]
    S --> T["Atendente notifica<br/>cliente para retirada"]
    T --> U["Cliente retira<br/>veículo"]
    U --> END(("🔴 Fim:<br/>OS Concluída"))
```

### Regras de Negócio Envolvidas

| Regra | Descrição |
|-------|----------|
| RN05 | A baixa de estoque é automática quando peças são vinculadas à OS |
| RN06 | Peças com estoque abaixo do mínimo geram alerta no sistema |
| RN07 | Serviços adicionais identificados durante execução requerem nova autorização |
| RN08 | Toda comunicação com cliente é registrada no sistema (ContatoOS) |

---

## 5. Processo 3 — Gestão de Estoque

### Descrição
Fluxo de gestão do inventário de peças, cobrindo entrada, saída e ajuste de estoque.

### Diagrama BPMN

```mermaid
flowchart TD
    START(("🟢 Início")) --> A{"Tipo de<br/>movimentação?"}
    
    A -->|Entrada| B["Administrador registra<br/>entrada de peças"]
    B --> B1["Informa: Peça, Quantidade,<br/>Referência/NF"]
    B1 --> B2["Sistema incrementa<br/>estoque atual"]
    B2 --> B3["Sistema registra<br/>MovimentacaoEstoque<br/>(Tipo: Entrada)"]
    B3 --> END1(("🔴 Fim"))
    
    A -->|Saída Manual| C["Administrador registra<br/>saída de peça"]
    C --> C1{"Estoque suficiente?"}
    C1 -->|Sim| C2["Sistema decrementa<br/>estoque atual"]
    C2 --> C3["Sistema registra<br/>MovimentacaoEstoque<br/>(Tipo: Saída)"]
    C3 --> C4{"Estoque abaixo<br/>do mínimo?"}
    C4 -->|Sim| C5["⚠ Sistema exibe<br/>alerta de ruptura"]
    C4 -->|Não| END2(("🔴 Fim"))
    C5 --> END2
    C1 -->|Não| C6["❌ Sistema bloqueia<br/>operação"]
    C6 --> END3(("🔴 Fim:<br/>Estoque Insuficiente"))
    
    A -->|Saída via OS| D["Sistema recebe<br/>vínculo de peça à OS"]
    D --> D1["Sistema verifica<br/>estoque disponível"]
    D1 --> D2["Sistema cria snapshot<br/>de preço (PrecoBase,<br/>PrecoCustoSnapshot)"]
    D2 --> D3["Sistema decrementa<br/>estoque automaticamente"]
    D3 --> D4["Sistema registra<br/>MovimentacaoEstoque<br/>(Tipo: Saída,<br/>Ref: OS #XX)"]
    D4 --> C4
    
    A -->|Ajuste| E["Administrador realiza<br/>ajuste/inventário"]
    E --> E1["Informa nova<br/>quantidade real"]
    E1 --> E2["Sistema calcula<br/>diferença e ajusta"]
    E2 --> E3["Sistema registra<br/>MovimentacaoEstoque<br/>(Tipo: Ajuste)"]
    E3 --> END4(("🔴 Fim"))
```

### Regras de Negócio Envolvidas

| Regra | Descrição |
|-------|----------|
| RN09 | Toda movimentação registra: tipo, quantidade, data/hora, referência e usuário responsável |
| RN10 | Saída via OS cria snapshot imutável do preço (PrecoCusto e PrecoVenda no momento) |
| RN11 | Desconto abaixo do PrecoBase requer perfil Administrador |
| RN12 | Peça com estoque ≤ EstoqueMinimo é sinalizada como "Abaixo do Mínimo" |

---

## 6. Processo 4 — Visão Ponta a Ponta (End-to-End)

### Diagrama BPMN Macro

```mermaid
flowchart LR
    subgraph ATENDIMENTO ["🏢 Atendimento"]
        A1["Cadastro de<br/>Cliente/Veículo"]
        A2["Criação da OS<br/>(Orçamento)"]
        A3["Vistoria de<br/>Entrada"]
        A4["Autorização<br/>do Cliente"]
    end
    
    subgraph EXECUCAO ["🔧 Execução"]
        E1["Verificação de<br/>Peças em Estoque"]
        E2["Execução dos<br/>Serviços"]
        E3["Controle de<br/>Qualidade"]
    end
    
    subgraph ENTREGA ["📦 Entrega"]
        D1["Conclusão da OS"]
        D2["Notificação ao<br/>Cliente"]
        D3["Entrega do<br/>Veículo"]
    end
    
    subgraph GESTAO ["📊 Gestão"]
        G1["Dashboard BI"]
        G2["Relatórios"]
        G3["Configurações"]
    end
    
    A1 --> A2 --> A3 --> A4
    A4 --> E1 --> E2 --> E3
    E3 --> D1 --> D2 --> D3
    D3 -.->|Dados alimentam| G1
    D3 -.->|Dados alimentam| G2
    G3 -.->|Regras configuram| A2
```

---

## 7. Legenda BPMN

| Símbolo | Significado |
|---------|------------|
| 🟢 Círculo verde | Evento de início |
| 🔴 Círculo vermelho | Evento de fim |
| Retângulo | Atividade / Tarefa |
| Losango | Gateway de decisão (exclusivo) |
| Retângulo tracejado | Sub-processo / Pool |
| Seta sólida | Fluxo de sequência |
| Seta tracejada | Fluxo de dados / mensagem |

---

## 8. Conclusão

Os diagramas BPMN demonstram que o MechSystem cobre **todo o fluxo operacional** de uma oficina mecânica, desde o primeiro contato com o cliente até a entrega do veículo. Os processos são interconectados e alimentam automaticamente os módulos de gestão (Dashboard, Relatórios), criando um ciclo virtuoso de dados para tomada de decisão.

---

*Documento elaborado como artefato da disciplina de Engenharia de Software III — FATEC 2026/1*
