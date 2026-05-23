# Diagrama de Caso de Uso — MechSystem

**Projeto**: MechSystem — Sistema de Gestão para Oficinas Mecânicas  
**Versão**: 1.0  
**Data**: Maio/2026  
**Autor**: Ryan Cristian  
**Disciplina**: Engenharia de Software III — Prof. Alessandro Fukuta

---

## 1. Objetivo

Apresentar o **Diagrama de Caso de Uso** do sistema MechSystem, identificando os atores, seus casos de uso e os relacionamentos entre eles (inclusão, extensão e generalização).

---

## 2. Atores do Sistema

| Ator | Tipo | Descrição |
|------|------|----------|
| **Administrador** | Primário | Proprietário/gerente da oficina. Acesso total ao sistema. Herda todos os casos de uso dos demais perfis. |
| **Atendente** | Primário | Responsável pelo atendimento, cadastros, criação de OS e comunicação com cliente. |
| **Mecânico** | Primário | Responsável pela execução dos serviços e consulta de OS. Acesso restrito. |
| **Cliente** | Secundário (Externo) | Dono do veículo. Acessa o sistema apenas via token de acompanhamento. |
| **Sistema** | Interno | Automação de processos (cálculos, alertas, movimentações automáticas). |

---

## 3. Diagrama de Caso de Uso — Visão Geral

```mermaid
graph LR
    %% Atores
    ADM(("👤 Administrador"))
    ATE(("👤 Atendente"))
    MEC(("👤 Mecânico"))
    CLI(("👤 Cliente"))
    SIS(("⚙️ Sistema"))
    
    %% Herança de Atores
    ADM -.->|"herda"| ATE
    
    %% === Módulo de Autenticação ===
    subgraph AUTH ["🔐 Autenticação"]
        UC01["UC01: Realizar Login"]
        UC02["UC02: Realizar Logout"]
        UC03["UC03: Gerenciar Usuários"]
    end
    
    ATE --> UC01
    ATE --> UC02
    MEC --> UC01
    MEC --> UC02
    ADM --> UC03
    
    %% === Módulo de Cadastros ===
    subgraph CAD ["📋 Cadastros"]
        UC04["UC04: Gerenciar Clientes"]
        UC05["UC05: Gerenciar Veículos"]
        UC06["UC06: Gerenciar Serviços"]
    end
    
    ATE --> UC04
    ATE --> UC05
    ATE --> UC06
    
    %% === Módulo de OS ===
    subgraph OS ["📝 Ordens de Serviço"]
        UC07["UC07: Criar Ordem de Serviço"]
        UC08["UC08: Consultar OS"]
        UC09["UC09: Atualizar Status da OS"]
        UC10["UC10: Registrar Autorização"]
        UC11["UC11: Vincular Peças à OS"]
        UC12["UC12: Registrar Contato com Cliente"]
        UC13["UC13: Imprimir OS"]
    end
    
    ATE --> UC07
    ATE --> UC08
    ATE --> UC09
    ATE --> UC10
    ATE --> UC11
    ATE --> UC12
    ATE --> UC13
    MEC --> UC08
    
    %% === Módulo de Vistoria ===
    subgraph VIS ["🔍 Vistoria"]
        UC14["UC14: Realizar Vistoria de Entrada"]
        UC15["UC15: Registrar Avarias"]
    end
    
    ATE --> UC14
    UC14 -.->|"include"| UC15
    
    %% === Módulo de Estoque ===
    subgraph EST ["📦 Estoque"]
        UC16["UC16: Gerenciar Peças"]
        UC17["UC17: Registrar Movimentação"]
        UC18["UC18: Consultar Estoque"]
    end
    
    ADM --> UC16
    ADM --> UC17
    ATE --> UC18
    
    %% === Módulo de Relatórios ===
    subgraph REL ["📊 Relatórios e Dashboard"]
        UC19["UC19: Visualizar Dashboard"]
        UC20["UC20: Gerar Relatório de OS"]
        UC21["UC21: Gerar Relatório Financeiro"]
        UC22["UC22: Gerar Relatório de Estoque"]
    end
    
    ADM --> UC19
    ADM --> UC20
    ADM --> UC21
    ADM --> UC22
    
    %% === Módulo de Configurações ===
    subgraph CFG ["⚙️ Configurações"]
        UC23["UC23: Configurar Dados da Oficina"]
        UC24["UC24: Configurar Regras do Sistema"]
    end
    
    ADM --> UC23
    ADM --> UC24
    
    %% === Módulo de Acompanhamento ===
    subgraph ACO ["🔗 Acompanhamento"]
        UC25["UC25: Acompanhar OS via Token"]
    end
    
    CLI --> UC25
    
    %% === Automações do Sistema ===
    SIS --> UC11
    SIS --> UC18
```

---

## 4. Diagrama de Relacionamentos (Include / Extend)

```mermaid
graph TD
    UC07["UC07: Criar OS"] -->|"≪include≫"| UC04a["Verificar cadastro<br/>do cliente"]
    UC07 -->|"≪include≫"| UC05a["Verificar cadastro<br/>do veículo"]
    UC07 -->|"≪extend≫"| UC14["UC14: Realizar<br/>Vistoria de Entrada"]
    UC07 -->|"≪extend≫"| UC11["UC11: Vincular<br/>Peças à OS"]
    
    UC11 -->|"≪include≫"| UC18a["Verificar estoque<br/>disponível"]
    UC11 -->|"≪include≫"| UC26["Criar snapshot<br/>de preço"]
    UC11 -->|"≪include≫"| UC27["Baixar estoque<br/>automaticamente"]
    
    UC14 -->|"≪include≫"| UC15["UC15: Registrar<br/>Avarias"]
    UC14 -->|"≪include≫"| UC28["Registrar checklist<br/>de itens"]
    
    UC09["UC09: Atualizar<br/>Status da OS"] -->|"≪extend≫"| UC10["UC10: Registrar<br/>Autorização"]
    UC09 -->|"≪extend≫"| UC12["UC12: Registrar<br/>Contato com Cliente"]
    
    UC17["UC17: Registrar<br/>Movimentação"] -->|"≪include≫"| UC29["Registrar usuário<br/>responsável"]
    UC17 -->|"≪extend≫"| UC30["Alertar estoque<br/>abaixo do mínimo"]
```

---

## 5. Lista Completa de Casos de Uso

| ID | Caso de Uso | Ator Primário | Módulo |
|----|------------|--------------|--------|
| UC01 | Realizar Login | Atendente, Mecânico, Administrador | Autenticação |
| UC02 | Realizar Logout | Atendente, Mecânico, Administrador | Autenticação |
| UC03 | Gerenciar Usuários | Administrador | Autenticação |
| UC04 | Gerenciar Clientes (CRUD) | Atendente | Cadastros |
| UC05 | Gerenciar Veículos (CRUD) | Atendente | Cadastros |
| UC06 | Gerenciar Serviços (CRUD) | Atendente | Cadastros |
| UC07 | Criar Ordem de Serviço | Atendente | OS |
| UC08 | Consultar Ordem de Serviço | Atendente, Mecânico | OS |
| UC09 | Atualizar Status da OS | Atendente | OS |
| UC10 | Registrar Autorização do Cliente | Atendente | OS |
| UC11 | Vincular Peças à OS | Atendente, Sistema | OS |
| UC12 | Registrar Contato com Cliente | Atendente | OS |
| UC13 | Imprimir OS | Atendente | OS |
| UC14 | Realizar Vistoria de Entrada | Atendente | Vistoria |
| UC15 | Registrar Avarias do Veículo | Atendente | Vistoria |
| UC16 | Gerenciar Peças (CRUD) | Administrador | Estoque |
| UC17 | Registrar Movimentação de Estoque | Administrador | Estoque |
| UC18 | Consultar Estoque | Atendente, Administrador | Estoque |
| UC19 | Visualizar Dashboard | Administrador | Relatórios |
| UC20 | Gerar Relatório de OS | Administrador | Relatórios |
| UC21 | Gerar Relatório Financeiro | Administrador | Relatórios |
| UC22 | Gerar Relatório de Estoque | Administrador | Relatórios |
| UC23 | Configurar Dados da Oficina | Administrador | Configurações |
| UC24 | Configurar Regras do Sistema | Administrador | Configurações |
| UC25 | Acompanhar OS via Token | Cliente | Acompanhamento |

---

## 6. Conclusão

O diagrama de caso de uso do MechSystem identifica **5 atores** e **25 casos de uso** distribuídos em **8 módulos funcionais**. O Administrador herda todas as permissões do Atendente, refletindo a hierarquia de perfis (RBAC) implementada no sistema. Os relacionamentos de `<<include>>` e `<<extend>>` demonstram a composição de funcionalidades complexas a partir de funcionalidades atômicas.

---

*Documento elaborado como artefato da disciplina de Engenharia de Software III — FATEC 2026/1*
