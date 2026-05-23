# Documento de Elicitação de Requisitos — MechSystem

**Projeto**: MechSystem — Sistema de Gestão para Oficinas Mecânicas  
**Versão**: 1.0  
**Data**: Maio/2026  
**Autor**: Ryan Cristian  
**Disciplina**: Engenharia de Software III — Prof. Alessandro Fukuta

---

## 1. Objetivo

Este documento registra o processo de **elicitação de requisitos** do sistema MechSystem, utilizando a técnica de **entrevista semiestruturada** com o stakeholder principal (proprietário da oficina mecânica) e **brainstorming** com a equipe de desenvolvimento.

---

## 2. Stakeholders Identificados

| Stakeholder | Papel | Interesse no Sistema |
|------------|-------|---------------------|
| Proprietário da oficina | Sponsor / Dono do negócio | Controle total do fluxo operacional e financeiro |
| Atendente / Recepcionista | Usuário primário | Cadastro de clientes, abertura de OS, comunicação |
| Mecânico | Usuário operacional | Consulta de OS, registro de serviços executados |
| Cliente da oficina | Usuário externo (indireto) | Acompanhamento do status do veículo |
| Equipe de TI | Desenvolvedor/Mantenedor | Manutenibilidade, escalabilidade, portabilidade |

---

## 3. Técnica 1 — Entrevista Semiestruturada

### Entrevistado: João Silva (Proprietário fictício da oficina "MechSystem Auto Center")
### Data da Entrevista: 10/03/2026

---

### Bloco A — Contexto e Dores Atuais

**P1: Como funciona hoje o fluxo de atendimento na sua oficina?**

> R: O cliente chega, o atendente anota os dados em uma ficha de papel. Depois abre um orçamento no caderno ou numa planilha. Se o cliente aprova, a gente começa o serviço. No final, faz a nota na mão. É tudo muito manual e a gente perde informação o tempo todo.

**P2: Quais são os maiores problemas que você enfrenta com o processo atual?**

> R: Primeiro, a gente perde ficha de cliente. Segundo, não tem controle de peças — às vezes o mecânico usa uma peça e não avisa, aí o estoque fica errado. Terceiro, não consigo saber quanto faturei no mês sem ficar somando nota por nota. E o pior: cliente liga perguntando do carro e ninguém sabe responder rápido.

**P3: Você já tentou usar algum software para gestão?**

> R: Já tentei umas planilhas no Excel, mas era difícil manter atualizado. Também testei um sistema online, mas era muito caro e complicado. Quero algo simples que meus funcionários consigam usar.

**P4: Quantos funcionários a oficina tem? Quais funções?**

> R: Somos 5: eu (administrador), 1 atendente na recepção, 2 mecânicos e 1 ajudante. O atendente faz cadastro, orçamento e fala com o cliente. Os mecânicos executam o serviço.

---

### Bloco B — Funcionalidades Desejadas

**P5: O que você considera essencial em um sistema de gestão para oficina?**

> R: Cadastro de clientes e veículos, controle de ordens de serviço do começo ao fim, controle de estoque de peças, e um painel pra eu ver como anda o faturamento e os serviços. Ah, e login com senha — não quero qualquer um mexendo nos relatórios financeiros.

**P6: Como deveria funcionar a Ordem de Serviço ideal?**

> R: O atendente abre o orçamento com os dados do carro, descreve o problema e detalha bem tudo o que vai ser feito: lista cada um dos serviços com o tempo que vai levar e todas as peças. Se precisar, tem que poder dar um desconto direto no valor final. Manda pro cliente aprovar. Quando o cliente aprova, vira um contrato. O mecânico executa, e quando termina, marca como concluído. Se o cliente cancelar, tem que registrar também.

**P7: Você sente necessidade de fazer uma vistoria de entrada no veículo?**

> R: Com certeza! Já tive problema de cliente reclamar de risco no carro que já existia antes. Preciso documentar como o carro chegou — combustível, quilometragem, se tem estepe, triângulo, se já tinha avaria.

**P8: Como você gostaria de controlar o estoque de peças?**

> R: Quero saber quanto tem de cada peça, preço de custo e de venda, e receber alerta quando tiver pouca peça. Quando o mecânico usa uma peça na OS, tem que descontar automaticamente do estoque.

**P9: E sobre relatórios, o que seria útil?**

> R: Faturamento mensal, quantas OS foram feitas, qual o ticket médio, quanto gastei em peças versus quanto cobrei de mão de obra. Se tiver um gráfico bonito, melhor ainda.

**P10: O cliente precisa acessar o sistema?**

> R: Não precisa ter login no sistema. Mas seria bom se ele pudesse consultar o status do carro de alguma forma — tipo um link ou código.

---

### Bloco C — Restrições e Requisitos Não Funcionais

**P11: Quem deve ter acesso a quais partes do sistema?**

> R: O administrador (eu) vejo tudo. O atendente pode cadastrar cliente, abrir OS e ver estoque, mas não pode mexer em configurações nem dar desconto abaixo do preço. O mecânico só consulta a OS dele.

**P12: O sistema precisa funcionar em celular?**

> R: O principal é no computador da recepção, mas se funcionar no celular também seria ótimo. Às vezes preciso ver um relatório de casa.

**P13: Você tem preferência de tecnologia ou infraestrutura?**

> R: Não entendo muito disso. Quero que seja rápido, funcione no Chrome, e que eu não precise instalar nada complicado. Se rodar local sem precisar de internet, melhor ainda.

**P14: Qual é o orçamento disponível para o sistema?**

> R: Sou pequeno, não posso pagar muito. Preciso de algo acessível. Se for por mensalidade, até R$ 200/mês. Se for comprar uma vez, até R$ 5.000.

---

## 4. Técnica 2 — Brainstorming com Equipe de Desenvolvimento

### Data: 15/03/2026 | Participantes: Equipe de desenvolvimento MechSystem

**Ideias Levantadas e Priorizadas:**

| # | Ideia | Prioridade | Viabilidade |
|---|-------|-----------|-------------|
| 1 | Dashboard com KPIs financeiros e operacionais | Alta | Alta |
| 2 | Ciclo completo de OS com máquina de estados | Alta | Alta |
| 3 | Vistoria de entrada com checklist e mapeamento de avarias | Alta | Média |
| 4 | Controle de estoque com movimentação automática | Alta | Alta |
| 5 | Sistema de perfis (RBAC) com 3 níveis | Alta | Alta |
| 6 | Token de acompanhamento de OS para cliente | Média | Alta |
| 7 | Catálogo de serviços pré-definidos com valores | Média | Alta |
| 8 | Impressão de OS formatada | Média | Média |
| 9 | Configurações dinâmicas da oficina (CNPJ, nome, regras) | Média | Alta |
| 10 | Relatórios com herança de classe abstrata (POO) | Média | Alta |
| 11 | Suporte a múltiplos bancos (SQLite + PostgreSQL) | Baixa | Alta |
| 12 | Notificações push / WhatsApp | Baixa | Baixa |
| 13 | App mobile nativo | Baixa | Baixa |

---

## 5. Requisitos Elicitados (Resumo)

### Requisitos Funcionais Identificados

| ID | Requisito | Origem |
|----|----------|--------|
| RF01 | O sistema deve permitir cadastro de clientes com CPF, nome, e-mail, telefone e endereço | P5, P1 |
| RF02 | O sistema deve permitir cadastro de veículos vinculados a clientes | P5, P6 |
| RF03 | O sistema deve gerenciar ordens de serviço com ciclo de vida completo | P6 |
| RF04 | O sistema deve realizar vistoria de entrada obrigatória | P7 |
| RF05 | O sistema deve controlar estoque de peças com alertas | P8 |
| RF06 | O sistema deve gerar relatórios financeiros e operacionais | P9 |
| RF07 | O sistema deve permitir acompanhamento de OS via token | P10 |
| RF08 | O sistema deve implementar controle de acesso por perfis | P11 |
| RF09 | O sistema deve ter dashboard com indicadores de BI | P9, Brainstorming #1 |
| RF10 | O sistema deve permitir configuração dinâmica de parâmetros | Brainstorming #9 |

### Requisitos Não Funcionais Identificados

| ID | Requisito | Origem |
|----|----------|--------|
| RNF01 | O sistema deve ser responsivo (desktop e mobile) | P12 |
| RNF02 | O sistema deve funcionar em navegadores modernos (Chrome, Edge, Firefox) | P13 |
| RNF03 | O sistema deve funcionar localmente sem internet (SQLite) | P13 |
| RNF04 | O sistema deve ter custo acessível | P14 |
| RNF05 | O sistema deve ser de fácil operação (treinamento mínimo) | P3 |

---

## 6. Conclusão

A elicitação revelou que o principal problema do stakeholder é a **falta de controle e rastreabilidade** nos processos da oficina. O MechSystem endereça essas dores com um sistema integrado que cobre todo o fluxo operacional, desde o cadastro do cliente até a entrega do veículo, com controle financeiro e de estoque embutidos.

---

*Documento elaborado como artefato da disciplina de Engenharia de Software III — FATEC 2026/1*
