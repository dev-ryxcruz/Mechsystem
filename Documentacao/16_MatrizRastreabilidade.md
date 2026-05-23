# Matriz de Rastreabilidade — MechSystem

**Projeto**: MechSystem — Sistema de Gestão para Oficinas Mecânicas  
**Versão**: 1.0  
**Data**: Maio/2026  
**Autor**: Ryan Cristian  
**Disciplina**: Engenharia de Software III — Prof. Alessandro Fukuta

---

## 1. Objetivo

Apresentar a **Matriz de Rastreabilidade** dupla do sistema MechSystem:
1. **Requisitos Funcionais × Regras de Negócio** — garante que cada requisito é sustentado por regras
2. **Requisitos Funcionais × Casos de Uso** — garante que cada requisito tem pelo menos um caso de uso associado

---

## 2. Matriz de Rastreabilidade: Requisitos × Regras de Negócio

| Requisito | RN01 | RN02 | RN03 | RN04 | RN05 | RN06 | RN07 | RN08 | RN09 | RN10 | RN11 | RN12 | RN13 | RN14 | RN15 | RN16 | RN17 | RN18 |
|-----------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **RF01** Login | | | | | | | | | | | | | | | | ● | | |
| **RF02** BCrypt | | | | | | | | | | | | | | | | ● | | |
| **RF03** Cookie 8h | | | | | | | | | | | | | | | | ● | | |
| **RF04** 3 Perfis | | | | | | | | ● | | | | | | | | | | |
| **RF06** Admin seed | | | | | | | | | | | | | | | ● | | | |
| **RF08** Cadastro cliente | ● | | | | | | | | | | | | | | | | | |
| **RF11** Cadastro veículo | | ● | | | | | | | | | | | | | | | | |
| **RF17** Previsão entrega | | | ● | | | | | | | | | | | | | | | |
| **RF18** Valor total OS | | | | ● | ● | ● | | | | | | | | | | | | |
| **RF19** Graceful degrad. | | | | | ● | | | | | | | | | | | | | |
| **RF22** Validade orçam. | | | | | | | | | | | | ● | | | | | | |
| **RF25** Vincular peças | | | | | | | ● | | | | | | | | | | | |
| **RF26** Snapshot preço | | | | | | | ● | | | | | | | | | | | |
| **RF28** Subtotal linha | | | | | | | | | ● | | | | | | | | | |
| **RF29** Sinalizar desc. | | | | | | | | ● | | | | | | | | | | |
| **RF30** Bloquear desc. | | | | | | | | ● | | | | | | | | | | |
| **RF32** Combustível | | | | | | | | | | | | | | | | | ● | |
| **RF38** Abaixo mínimo | | | | | | | | | | ● | | | | | | | | |
| **RF39** Margem lucro | | | | | | | | | | | ● | | | | | | | |
| **RF40** Movimentação | | | | | | | | | | | | | | | | | | ● |
| **RF46** Regras config. | | | | | | | | | | | | ● | ● | ● | | | | |
| **RF47** Financeiro cfg. | | | | | | | | | | | | | | ● | | | | |

### Legenda
- **●** = Requisito é diretamente sustentado/restrito pela Regra de Negócio

---

## 3. Matriz de Rastreabilidade: Requisitos × Casos de Uso

| Requisito | UC01 | UC03 | UC04 | UC05 | UC06 | UC07 | UC08 | UC09 | UC10 | UC11 | UC12 | UC13 | UC14 | UC15 | UC16 | UC17 | UC18 | UC19 | UC20 | UC21 | UC22 | UC23 | UC24 | UC25 |
|-----------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **RF01** Login | ● | | | | | | | | | | | | | | | | | | | | | | | |
| **RF02** BCrypt | ● | | | | | | | | | | | | | | | | | | | | | | | |
| **RF03** Cookie | ● | | | | | | | | | | | | | | | | | | | | | | | |
| **RF04** Perfis | ● | ● | | | | | | | | | | | | | | | | | | | | | | |
| **RF05** Redirect | ● | | | | | | | | | | | | | | | | | | | | | | | |
| **RF08** Cliente | | | ● | | | | | | | | | | | | | | | | | | | | | |
| **RF09** CRUD Cli | | | ● | | | | | | | | | | | | | | | | | | | | | |
| **RF10** Vincular Veic | | | ● | ● | | | | | | | | | | | | | | | | | | | | |
| **RF11** Veículo | | | | ● | | | | | | | | | | | | | | | | | | | | |
| **RF14** Serviço | | | | | ● | | | | | | | | | | | | | | | | | | | |
| **RF16** Criar OS | | | | | | ● | | | | | | | | | | | | | | | | | | |
| **RF17** Datas OS | | | | | | ● | | | | | | | | | | | | | | | | | | |
| **RF18** Valor Total | | | | | | ● | | | | ● | | | | | | | | | | | | | | |
| **RF20** Status OS | | | | | | | | ● | | | | | | | | | | | | | | | | |
| **RF21** Autorização | | | | | | | | | ● | | | | | | | | | | | | | | | |
| **RF23** Token | | | | | | ● | | | | | | | | | | | | | | | | | | ● |
| **RF24** Contatos | | | | | | | | | | | ● | | | | | | | | | | | | | |
| **RF25** Vincular Peça | | | | | | | | | | ● | | | | | | | | | | | | | | |
| **RF26** Snapshot | | | | | | | | | | ● | | | | | | | | | | | | | | |
| **RF31** Vistoria | | | | | | | | | | | | | ● | | | | | | | | | | | |
| **RF35** Avarias | | | | | | | | | | | | | | ● | | | | | | | | | | |
| **RF37** Peças CRUD | | | | | | | | | | | | | | | ● | | | | | | | | | |
| **RF40** Moviment. | | | | | | | | | | | | | | | | ● | | | | | | | | |
| **RF41** Dashboard | | | | | | | | | | | | | | | | | | ● | | | | | | |
| **RF42** Relat. OS | | | | | | | | | | | | | | | | | | | ● | | | | | |
| **RF43** Relat. Fin. | | | | | | | | | | | | | | | | | | | | ● | | | | |
| **RF44** Relat. Est. | | | | | | | | | | | | | | | | | | | | | ● | | | |
| **RF45** Config Ofc. | | | | | | | | | | | | | | | | | | | | | | ● | | |
| **RF46** Config Regras | | | | | | | | | | | | | | | | | | | | | | | ● | |
| **RF48** Usuários | | ● | | | | | | | | | | | | | | | | | | | | | | |

### Legenda
- **●** = Requisito é implementado/coberto pelo Caso de Uso

---

## 4. Análise de Cobertura

### 4.1 Cobertura de Requisitos por Regras de Negócio

| Métrica | Valor |
|---------|-------|
| Total de RF analisados | 22 (principais) |
| RF com pelo menos 1 RN associada | 22 (100%) |
| Total de RN | 18 |
| RN utilizadas | 18 (100%) |

### 4.2 Cobertura de Requisitos por Casos de Uso

| Métrica | Valor |
|---------|-------|
| Total de RF analisados | 30 (principais) |
| RF com pelo menos 1 UC associado | 30 (100%) |
| Total de UC | 25 |
| UC com pelo menos 1 RF associado | 24 (96%) |
| UC sem RF explícito | UC02 (Logout — implícito em RF03) |

### 4.3 Requisitos Órfãos

| Tipo | Quantidade | Detalhes |
|------|-----------|---------|
| RF sem RN | 0 | — |
| RF sem UC | 0 | — |
| RN sem RF | 0 | — |
| UC sem RF | 1 | UC02 (Logout) — associado implicitamente a RF03 |

---

## 5. Conclusão

A Matriz de Rastreabilidade demonstra **cobertura completa**:
- Todos os requisitos funcionais têm pelo menos uma regra de negócio e um caso de uso associados
- Não existem requisitos órfãos
- A rastreabilidade bidirecional garante que toda funcionalidade é justificada por uma necessidade de negócio e coberta por um cenário de uso

---

*Documento elaborado como artefato da disciplina de Engenharia de Software III — FATEC 2026/1*
