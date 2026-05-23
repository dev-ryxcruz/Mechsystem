# Diagramas de Atividade — MechSystem

**Projeto**: MechSystem — Sistema de Gestão para Oficinas Mecânicas  
**Versão**: 1.0  
**Data**: Maio/2026  
**Autor**: Ryan Cristian  
**Disciplina**: Engenharia de Software III — Prof. Alessandro Fukuta

---

## 1. Objetivo

Apresentar **3 Diagramas de Atividade** representando os fluxos operacionais mais importantes do sistema MechSystem, detalhando as ações, decisões e partições de responsabilidade (swimlanes).

---

## 2. Diagrama de Atividade 1 — Fluxo de Criação de Ordem de Serviço

### Descrição
Modela o fluxo completo desde a chegada do cliente até a OS ser registrada no sistema com status "Orçamento".

```mermaid
flowchart TD
    START(("●")) --> A["Cliente chega<br/>à oficina"]
    A --> B{"Cliente já<br/>cadastrado?"}
    
    B -->|Não| C["Cadastrar<br/>novo cliente"]
    C --> D["Preencher:<br/>Nome, CPF,<br/>Telefone, Email"]
    D --> E{"Veículo já<br/>cadastrado?"}
    
    B -->|Sim| E
    
    E -->|Não| F["Cadastrar<br/>novo veículo"]
    F --> G["Preencher:<br/>Placa, Marca,<br/>Modelo, Ano, KM"]
    G --> H["Vincular veículo<br/>ao cliente"]
    H --> I["Coletar relato<br/>do problema"]
    
    E -->|Sim| I
    
    I --> J["Criar OS<br/>(Status: Orçamento)"]
    J --> K["Preencher:<br/>diagnóstico,<br/>previsão de entrega"]
    K --> L{"Vistoria<br/>obrigatória?"}
    
    L -->|Sim| M["Realizar vistoria<br/>de entrada"]
    M --> M1["Registrar:<br/>combustível, KM,<br/>checklist, avarias"]
    M1 --> N["Selecionar serviços<br/>do catálogo"]
    
    L -->|Não| N
    
    N --> O["Sistema calcula<br/>valor de mão de obra"]
    O --> P{"Adicionar<br/>peças?"}
    
    P -->|Sim| Q["Selecionar peças<br/>do estoque"]
    Q --> R["Informar<br/>quantidade"]
    R --> S["Sistema cria<br/>snapshot de preço"]
    S --> T["Sistema calcula<br/>valor total<br/>(MO + Peças)"]
    
    P -->|Não| T2["Sistema calcula<br/>valor total<br/>(apenas MO)"]
    
    T --> U["Salvar OS"]
    T2 --> U
    
    U --> V["Sistema gera<br/>token de<br/>acompanhamento"]
    V --> W["Sistema exibe<br/>confirmação"]
    W --> X{"Imprimir<br/>orçamento?"}
    
    X -->|Sim| Y["Sistema gera<br/>impressão<br/>formatada"]
    Y --> END(("◉"))
    X -->|Não| END
```

### Partições (Swimlanes)

| Ator | Atividades |
|------|-----------|
| **Atendente** | Cadastrar cliente/veículo, coletar relato, criar OS, selecionar serviços/peças, realizar vistoria |
| **Sistema** | Calcular valores, criar snapshot, gerar token, gerar impressão |

---

## 3. Diagrama de Atividade 2 — Fluxo de Vistoria de Entrada

### Descrição
Detalha o processo de inspeção do veículo na entrada da oficina, incluindo checklist de itens, nível de combustível e mapeamento de avarias.

```mermaid
flowchart TD
    START(("●")) --> A["Abrir formulário<br/>de vistoria"]
    A --> B["Selecionar nível<br/>de combustível"]
    B --> B1{"Nível<br/>selecionado?"}
    B1 -->|"Reserva"| C["NivelCombustivel = 1"]
    B1 -->|"1/4 Tanque"| C2["NivelCombustivel = 2"]
    B1 -->|"Meio Tanque"| C3["NivelCombustivel = 3"]
    B1 -->|"3/4 Tanque"| C4["NivelCombustivel = 4"]
    B1 -->|"Cheio"| C5["NivelCombustivel = 5"]
    
    C --> D["Informar<br/>quilometragem<br/>de entrada"]
    C2 --> D
    C3 --> D
    C4 --> D
    C5 --> D
    
    D --> E["📋 Preencher<br/>Checklist de Itens"]
    
    E --> F["☐ Estepe?"]
    F --> G["☐ Macaco?"]
    G --> H["☐ Rádio?"]
    H --> I["☐ Triângulo?"]
    I --> J["☐ Chave de Roda?"]
    
    J --> K{"Existem avarias<br/>visíveis?"}
    
    K -->|Sim| L["Mapear avarias<br/>no diagrama visual"]
    L --> L1["Para cada avaria:<br/>registrar posição<br/>(X, Y) e descrição"]
    L1 --> L2["Sistema armazena<br/>em JSON<br/>(AvariasJson)"]
    L2 --> M["Adicionar<br/>observações"]
    
    K -->|Não| M
    
    M --> N["Clicar em<br/>Salvar Vistoria"]
    N --> O["Sistema valida<br/>campos obrigatórios"]
    O --> P{"Validação<br/>OK?"}
    
    P -->|Não| Q["Exibir mensagens<br/>de erro"]
    Q --> D
    
    P -->|Sim| R["Sistema salva<br/>vistoria com<br/>Status: Concluída"]
    R --> S["Sistema vincula<br/>vistoria à OS<br/>(relação 1:1)"]
    S --> END(("◉"))
```

### Dados Registrados

| Campo | Tipo | Obrigatório |
|-------|------|------------|
| Nível de Combustível | Enum (1-5) | ✅ Sim |
| Quilometragem de Entrada | int | ✅ Sim |
| Estepe | bool | Não |
| Macaco | bool | Não |
| Rádio | bool | Não |
| Triângulo | bool | Não |
| Chave de Roda | bool | Não |
| Avarias | JSON [{X, Y, Desc}] | Não |
| Observações | string | Não |

---

## 4. Diagrama de Atividade 3 — Fluxo de Movimentação de Estoque

### Descrição
Modela os três tipos de movimentação de estoque (Entrada, Saída e Ajuste) e seus efeitos no saldo de peças.

```mermaid
flowchart TD
    START(("●")) --> A["Acessar módulo<br/>de estoque"]
    A --> B["Selecionar peça"]
    B --> C{"Tipo de<br/>movimentação?"}
    
    %% === ENTRADA ===
    C -->|"Entrada"| D1["Informar quantidade<br/>de entrada"]
    D1 --> D2["Informar referência<br/>(NF, fornecedor)"]
    D2 --> D3["Sistema incrementa<br/>EstoqueAtual"]
    D3 --> D4["Sistema cria<br/>MovimentacaoEstoque"]
    D4 --> D5["Tipo: Entrada<br/>Usuário: atual<br/>Data: agora"]
    D5 --> FIM["Exibir confirmação"]
    
    %% === SAÍDA MANUAL ===
    C -->|"Saída Manual"| E1["Informar quantidade<br/>de saída"]
    E1 --> E2{"Estoque<br/>suficiente?"}
    E2 -->|Não| E3["❌ Bloquear<br/>operação"]
    E3 --> ERR["Exibir erro:<br/>Estoque insuficiente"]
    ERR --> END2(("◉"))
    
    E2 -->|Sim| E4["Informar motivo<br/>(referência)"]
    E4 --> E5["Sistema decrementa<br/>EstoqueAtual"]
    E5 --> E6["Sistema cria<br/>MovimentacaoEstoque"]
    E6 --> E7["Tipo: Saída<br/>Usuário: atual<br/>Data: agora"]
    E7 --> CHECK{"EstoqueAtual ≤<br/>EstoqueMinimo?"}
    
    %% === SAÍDA VIA OS ===
    C -->|"Saída via OS"| F1["Sistema identifica<br/>peça vinculada à OS"]
    F1 --> F2["Sistema cria<br/>snapshot de preço"]
    F2 --> F3["PrecoCustoSnapshot<br/>PrecoBase"]
    F3 --> F4["Sistema decrementa<br/>EstoqueAtual"]
    F4 --> F5["Sistema cria<br/>MovimentacaoEstoque"]
    F5 --> F6["Tipo: Saída<br/>Ref: OS #XX<br/>Usuário: sistema"]
    F6 --> CHECK
    
    %% === AJUSTE ===
    C -->|"Ajuste/Inventário"| G1["Informar quantidade<br/>real contada"]
    G1 --> G2["Sistema calcula<br/>diferença<br/>(Real - Atual)"]
    G2 --> G3["Sistema ajusta<br/>EstoqueAtual"]
    G3 --> G4["Sistema cria<br/>MovimentacaoEstoque"]
    G4 --> G5["Tipo: Ajuste<br/>Ref: Inventário<br/>Usuário: atual"]
    G5 --> CHECK
    
    %% === VERIFICAÇÃO DE MÍNIMO ===
    CHECK -->|Sim| ALERT["⚠ Sistema exibe<br/>alerta de ruptura"]
    ALERT --> FIM
    CHECK -->|Não| FIM
    FIM --> END(("◉"))
```

### Regras de Negócio Aplicadas

| Regra | Descrição | Diagrama |
|-------|----------|---------|
| RN09 | Toda movimentação registra tipo, quantidade, data, referência e usuário | Todos os fluxos |
| RN10 | Snapshot de preço é criado na saída via OS | Fluxo "Saída via OS" |
| RN12 | Alerta quando EstoqueAtual ≤ EstoqueMinimo | Decisão CHECK |
| RN18 | UsuarioId é obrigatório em toda movimentação | Campo "Usuário: atual" |

---

## 5. Conclusão

Os 3 diagramas de atividade cobrem os fluxos mais críticos do MechSystem:
1. **Criação de OS** — o processo core do negócio
2. **Vistoria de Entrada** — proteção jurídica e documentação
3. **Movimentação de Estoque** — controle de inventário

Cada diagrama está alinhado com os requisitos funcionais e regras de negócio documentados.

---

*Documento elaborado como artefato da disciplina de Engenharia de Software III — FATEC 2026/1*
