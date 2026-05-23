# Documentação de Casos de Uso — MechSystem

**Projeto**: MechSystem — Sistema de Gestão para Oficinas Mecânicas  
**Versão**: 1.0  
**Data**: Maio/2026  
**Autor**: Ryan Cristian  
**Disciplina**: Engenharia de Software III — Prof. Alessandro Fukuta

---

## 1. Objetivo

Documentar de forma expandida os **casos de uso principais** do sistema MechSystem, incluindo fluxo principal, fluxos alternativos, exceções, pré-condições e pós-condições.

---

## 2. UC01 — Realizar Login

| Campo | Descrição |
|-------|----------|
| **ID** | UC01 |
| **Nome** | Realizar Login |
| **Ator Primário** | Atendente / Mecânico / Administrador |
| **Descrição** | O usuário autentica-se no sistema informando username e senha. |
| **Pré-condições** | O usuário deve estar cadastrado e ativo no sistema. |
| **Pós-condições** | Sessão autenticada via Cookie com validade de 8h. Usuário redirecionado ao Dashboard. |
| **Requisitos** | RF01, RF02, RF03, RF05 |

### Fluxo Principal

| Passo | Ação |
|-------|------|
| 1 | O sistema exibe a tela de login com campos Username e Senha |
| 2 | O usuário preenche Username e Senha |
| 3 | O usuário clica em "Entrar" |
| 4 | O sistema valida o preenchimento dos campos obrigatórios |
| 5 | O sistema busca o usuário pelo Username no banco de dados |
| 6 | O sistema verifica o hash BCrypt da senha informada |
| 7 | O sistema cria um Cookie de autenticação com Claims (Id, Username, NomeCompleto, Perfil) |
| 8 | O sistema redireciona o usuário para o Dashboard (`/`) |

### Fluxos Alternativos

| ID | Condição | Ação |
|----|---------|------|
| FA01 | Usuário acessa qualquer página sem estar autenticado | Sistema redireciona para `/login` |
| FA02 | Usuário já está autenticado e acessa `/login` | Sistema redireciona para `/` |

### Fluxos de Exceção

| ID | Condição | Ação |
|----|---------|------|
| FE01 | Username não encontrado no banco de dados | Sistema exibe "Credenciais inválidas" |
| FE02 | Senha incorreta (BCrypt.Verify retorna false) | Sistema exibe "Credenciais inválidas" |
| FE03 | Usuário encontrado mas com status Inativo | Sistema exibe "Usuário desativado" |
| FE04 | Campos obrigatórios não preenchidos | Sistema exibe mensagens de validação inline |

---

## 3. UC04 — Gerenciar Clientes

| Campo | Descrição |
|-------|----------|
| **ID** | UC04 |
| **Nome** | Gerenciar Clientes (CRUD) |
| **Ator Primário** | Atendente |
| **Descrição** | Permite cadastrar, consultar, editar e excluir clientes da oficina. |
| **Pré-condições** | Usuário autenticado com perfil Atendente ou Administrador. |
| **Pós-condições** | Cliente salvo no banco de dados com os campos validados. |
| **Requisitos** | RF08, RF09, RF10 |

### Fluxo Principal — Cadastro

| Passo | Ação |
|-------|------|
| 1 | O atendente acessa a página de Clientes (`/clientes`) |
| 2 | O sistema exibe a lista de clientes cadastrados |
| 3 | O atendente clica em "Novo Cliente" |
| 4 | O sistema exibe formulário com campos: Nome*, CPF*, E-mail, Telefone, Endereço |
| 5 | O atendente preenche os campos obrigatórios (marcados com *) |
| 6 | O atendente clica em "Salvar" |
| 7 | O sistema valida os campos (Nome ≤ 100 chars, CPF ≤ 14 chars) |
| 8 | O sistema persiste o cliente no banco de dados |
| 9 | O sistema exibe mensagem de sucesso e retorna à lista |

### Fluxos Alternativos

| ID | Condição | Ação |
|----|---------|------|
| FA01 | Atendente deseja editar cliente existente | Clica em "Editar" → formulário preenchido → altera dados → Salvar |
| FA02 | Atendente deseja excluir cliente | Clica em "Excluir" → confirmação → sistema remove do banco |
| FA03 | Atendente busca cliente específico | Utiliza campo de busca/filtro na listagem |

### Fluxos de Exceção

| ID | Condição | Ação |
|----|---------|------|
| FE01 | Nome não preenchido | Sistema exibe "O nome é obrigatório" |
| FE02 | CPF não preenchido | Sistema exibe "O CPF é obrigatório" |
| FE03 | Nome excede 100 caracteres | Sistema exibe "O nome não pode exceder 100 caracteres" |
| FE04 | Tentativa de excluir cliente com veículos vinculados | Sistema bloqueia e exibe alerta de dependência |

---

## 4. UC07 — Criar Ordem de Serviço

| Campo | Descrição |
|-------|----------|
| **ID** | UC07 |
| **Nome** | Criar Ordem de Serviço |
| **Ator Primário** | Atendente |
| **Descrição** | Cria uma nova OS vinculada a um veículo, com diagnóstico, serviços, peças e valores. |
| **Pré-condições** | Cliente e veículo cadastrados. Usuário autenticado com perfil Atendente ou Administrador. |
| **Pós-condições** | OS criada com Status "Orçamento" e Data de Emissão automática. |
| **Requisitos** | RF16, RF17, RF18, RF19, RF20, RF21, RF22 |
| **Includes** | UC14 (Vistoria, se obrigatória), UC11 (Vincular Peças) |

### Fluxo Principal

| Passo | Ação |
|-------|------|
| 1 | O atendente acessa "Nova OS" |
| 2 | O sistema exibe formulário de criação |
| 3 | O atendente seleciona o veículo (pesquisa por placa ou cliente) |
| 4 | O sistema preenche automaticamente os dados do veículo e cliente |
| 5 | O atendente informa o problema relatado/diagnóstico (obrigatório) |
| 6 | O atendente seleciona serviços do catálogo |
| 7 | O sistema calcula o valor de mão de obra |
| 8 | O atendente informa a previsão de entrega (obrigatória — CDC) |
| 9 | O atendente opcionalmente vincula peças do estoque |
| 10 | O sistema calcula o valor total (MO + Peças) |
| 11 | O atendente clica em "Salvar" |
| 12 | O sistema cria a OS com Status "Orçamento" e gera token de acompanhamento |
| 13 | O sistema exibe confirmação com opção de imprimir |

### Fluxos Alternativos

| ID | Condição | Ação |
|----|---------|------|
| FA01 | Vistoria de entrada é obrigatória (configuração) | Sistema exige preenchimento da vistoria antes de salvar |
| FA02 | Cliente ainda não está cadastrado | Atendente cadastra cliente inline → retorna ao fluxo |
| FA03 | Veículo ainda não está cadastrado | Atendente cadastra veículo inline → retorna ao fluxo |
| FA04 | Atendente não vincula peças (apenas mão de obra) | Sistema usa ValorPecas manual (pode ser R$ 0) |

### Fluxos de Exceção

| ID | Condição | Ação |
|----|---------|------|
| FE01 | Veículo não selecionado | Sistema exibe "O veículo é obrigatório" |
| FE02 | Diagnóstico não preenchido | Sistema exibe "O diagnóstico/problema relatado é obrigatório" |
| FE03 | Previsão de entrega não informada | Sistema exibe "A previsão de entrega é obrigatória (CDC)" |
| FE04 | Valor negativo informado | Sistema exibe "O valor não pode ser negativo" |
| FE05 | Peça sem estoque suficiente | Sistema alerta e permite continuar (backorder) |

---

## 5. UC09 — Atualizar Status da OS

| Campo | Descrição |
|-------|----------|
| **ID** | UC09 |
| **Nome** | Atualizar Status da Ordem de Serviço |
| **Ator Primário** | Atendente |
| **Descrição** | Permite avançar ou alterar o status da OS conforme o ciclo de vida definido. |
| **Pré-condições** | OS existente no sistema. Usuário autenticado. |
| **Pós-condições** | Status da OS atualizado e registrado. |
| **Requisitos** | RF20 |

### Fluxo Principal

| Passo | Ação |
|-------|------|
| 1 | O atendente acessa a OS desejada |
| 2 | O sistema exibe o status atual e as transições permitidas |
| 3 | O atendente seleciona o novo status |
| 4 | O sistema valida a transição de estado |
| 5 | O sistema atualiza o status e registra a data |

### Transições Permitidas

| De | Para | Condição |
|----|------|---------|
| Orçamento | Aguardando Peças | Peças necessárias não disponíveis |
| Orçamento | Em Andamento | Cliente autorizou o serviço (UC10) |
| Orçamento | Cancelada | Cliente cancelou ou orçamento expirou |
| Aguardando Peças | Em Andamento | Peças disponíveis |
| Em Andamento | Concluída | Todos os serviços executados |
| Em Andamento | Cancelada | Cliente solicita cancelamento |

---

## 6. UC11 — Vincular Peças à OS

| Campo | Descrição |
|-------|----------|
| **ID** | UC11 |
| **Nome** | Vincular Peças à Ordem de Serviço |
| **Ator Primário** | Atendente |
| **Ator Secundário** | Sistema (movimentação automática) |
| **Descrição** | Vincula peças do estoque à OS, com snapshot de preço e baixa automática. |
| **Pré-condições** | OS existente. Peça cadastrada e ativa. |
| **Pós-condições** | Peça vinculada com snapshot de preço. Estoque decrementado. |
| **Requisitos** | RF25, RF26, RF27, RF28, RF29, RF30 |

### Fluxo Principal

| Passo | Ação |
|-------|------|
| 1 | O atendente acessa a seção de peças dentro da OS |
| 2 | O atendente busca e seleciona uma peça do estoque |
| 3 | O sistema exibe PrecoVenda e EstoqueAtual da peça |
| 4 | O atendente informa a quantidade desejada |
| 5 | O sistema cria snapshot: PrecoBase = PrecoVenda, PrecoCustoSnapshot = PrecoCusto |
| 6 | O sistema preenche ValorCobrado = PrecoVenda (editável) |
| 7 | O atendente pode ajustar o ValorCobrado |
| 8 | O sistema calcula Subtotal = Quantidade × ValorCobrado |
| 9 | O sistema decrementa EstoqueAtual e cria MovimentacaoEstoque (Saída) |
| 10 | O sistema recalcula ValorTotal da OS |

### Fluxos de Exceção

| ID | Condição | Ação |
|----|---------|------|
| FE01 | ValorCobrado < PrecoBase e perfil = Atendimento | Sistema bloqueia: "Desconto requer perfil Administrador" |
| FE02 | Quantidade > EstoqueAtual | Sistema alerta "Estoque insuficiente" |
| FE03 | Estoque fica abaixo do mínimo após operação | Sistema exibe alerta de ruptura |

---

## 7. UC14 — Realizar Vistoria de Entrada

| Campo | Descrição |
|-------|----------|
| **ID** | UC14 |
| **Nome** | Realizar Vistoria de Entrada |
| **Ator Primário** | Atendente |
| **Descrição** | Documenta o estado do veículo na entrada da oficina. |
| **Pré-condições** | OS criada. Veículo presente na oficina. |
| **Pós-condições** | Vistoria salva com status "Concluída", vinculada à OS (1:1). |
| **Requisitos** | RF31, RF32, RF33, RF34, RF35, RF36 |
| **Includes** | UC15 (Registrar Avarias) |

### Fluxo Principal

| Passo | Ação |
|-------|------|
| 1 | O atendente acessa a vistoria dentro da OS |
| 2 | O sistema exibe formulário de vistoria |
| 3 | O atendente seleciona o nível de combustível (Reserva → Cheio) |
| 4 | O atendente informa a quilometragem de entrada |
| 5 | O atendente preenche o checklist: Estepe, Macaco, Rádio, Triângulo, Chave de Roda |
| 6 | O atendente opcionalmente mapeia avarias no veículo (UC15) |
| 7 | O atendente adiciona observações adicionais |
| 8 | O atendente clica em "Salvar Vistoria" |
| 9 | O sistema salva a vistoria com status "Concluída" e data atual |

### Fluxos de Exceção

| ID | Condição | Ação |
|----|---------|------|
| FE01 | Quilometragem não informada | Sistema exibe "A KM é obrigatória" |
| FE02 | Nível de combustível não selecionado | Sistema exibe "O nível de combustível é obrigatório" |
| FE03 | OS já possui vistoria (relação 1:1) | Sistema exibe vistoria existente para edição |

---

## 8. UC17 — Registrar Movimentação de Estoque

| Campo | Descrição |
|-------|----------|
| **ID** | UC17 |
| **Nome** | Registrar Movimentação de Estoque |
| **Ator Primário** | Administrador |
| **Descrição** | Registra entrada, saída ou ajuste no estoque de peças. |
| **Pré-condições** | Peça cadastrada no sistema. Usuário com perfil Administrador. |
| **Pós-condições** | Estoque atualizado. Movimentação registrada com auditoria completa. |
| **Requisitos** | RF40, RN09, RN18 |

### Fluxo Principal — Entrada

| Passo | Ação |
|-------|------|
| 1 | O administrador acessa o módulo de estoque |
| 2 | O administrador seleciona a peça |
| 3 | O administrador seleciona tipo "Entrada" |
| 4 | O administrador informa quantidade e referência (NF, fornecedor) |
| 5 | O sistema incrementa EstoqueAtual |
| 6 | O sistema cria registro em MovimentacaoEstoque (Tipo: Entrada, UsuarioId: atual) |
| 7 | O sistema exibe confirmação |

### Fluxos Alternativos

| ID | Condição | Ação |
|----|---------|------|
| FA01 | Tipo "Saída" selecionado | Sistema verifica se há estoque suficiente → decrementa |
| FA02 | Tipo "Ajuste" selecionado | Sistema recalcula diferença e ajusta automaticamente |

### Fluxos de Exceção

| ID | Condição | Ação |
|----|---------|------|
| FE01 | Saída com quantidade maior que estoque | Sistema bloqueia operação |
| FE02 | Estoque resultante ≤ EstoqueMinimo | Sistema exibe alerta de ruptura |

---

## 9. UC19 — Visualizar Dashboard

| Campo | Descrição |
|-------|----------|
| **ID** | UC19 |
| **Nome** | Visualizar Dashboard |
| **Ator Primário** | Administrador |
| **Descrição** | Exibe painel com KPIs financeiros e operacionais. |
| **Pré-condições** | Usuário autenticado com perfil Administrador. |
| **Pós-condições** | Nenhuma (consulta). |
| **Requisitos** | RF41 |

### Fluxo Principal

| Passo | Ação |
|-------|------|
| 1 | O administrador acessa o Dashboard (`/`) |
| 2 | O sistema consulta todas as OS, peças e movimentações |
| 3 | O sistema calcula KPIs: Receita Total, Ticket Médio, Funil de Serviços |
| 4 | O sistema calcula distribuição de receita (Mão de Obra vs. Peças) |
| 5 | O sistema calcula participação por marca de veículo |
| 6 | O sistema exibe os indicadores em cards e gráficos |

---

## 10. UC25 — Acompanhar OS via Token

| Campo | Descrição |
|-------|----------|
| **ID** | UC25 |
| **Nome** | Acompanhar OS via Token |
| **Ator Primário** | Cliente (externo) |
| **Descrição** | O cliente consulta o status da OS usando um token de acompanhamento. |
| **Pré-condições** | Token válido fornecido pela oficina. |
| **Pós-condições** | Nenhuma (consulta). |
| **Requisitos** | RF23 |

### Fluxo Principal

| Passo | Ação |
|-------|------|
| 1 | O cliente acessa a página de acompanhamento |
| 2 | O cliente informa o token recebido |
| 3 | O sistema busca a OS pelo token |
| 4 | O sistema exibe: status atual, serviços, previsão de entrega |

### Fluxos de Exceção

| ID | Condição | Ação |
|----|---------|------|
| FE01 | Token inválido ou não encontrado | Sistema exibe "OS não encontrada" |

---

## 11. Conclusão

Foram documentados **10 casos de uso** em detalhe expandido, cobrindo os fluxos mais críticos do MechSystem. Cada caso de uso está rastreável aos requisitos funcionais (RF) e regras de negócio (RN) definidos no documento de requisitos.

---

*Documento elaborado como artefato da disciplina de Engenharia de Software III — FATEC 2026/1*
