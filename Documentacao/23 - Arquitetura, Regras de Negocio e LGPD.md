# Arquitetura, Regras de Negócio e Conformidade LGPD — MechSystem

Este documento consolida a especificação técnica da máquina de estados da Ordem de Serviço (O.S.), a matriz de autorização por perfil, a regra transacional de estoque, a política de inativação lógica (*Soft Delete*), as decisões arquiteturais (ADRs) e o mapeamento de proteção de dados pessoais (LGPD) para apresentação à banca avaliadora.

---

## 1. Máquina de Estados da Ordem de Serviço (`OrdemServicoService`)

Toda transição de status e edição de uma Ordem de Serviço é centralizada e validada no servidor por `Services/OrdemServicoService.cs` (`TransicoesPermitidas` e `AtualizarOSAsync`). Nenhuma página Blazor altera `os.Status` diretamente.

```mermaid
stateDiagram-v2
    [*] --> Orcamento : Criação da O.S. (gera TokenAcompanhamento)

    Orcamento --> Aprovada : AprovarOSAsync()\n[Admin / Atendimento]\nReserva peças de estoque
    Orcamento --> Recusada : RecusarOrcamentoAsync(motivo)\n[Admin / Atendimento]\nSem impacto no estoque
    Orcamento --> Cancelada : CancelarOSAsync(motivo)\n[Admin / Atendimento]\nSem impacto no estoque

    Aprovada --> EmAndamento : IniciarApontamentoAsync()\n[Admin / Atendimento / Mecânico]\nPrimeira execução
    Aprovada --> Cancelada : CancelarOSAsync(motivo)\n[Admin / Atendimento]\nLibera reservas

    EmAndamento --> Pausada : PausarServicoAsync() / PausarApontamentoAsync()\n[Admin / Atendimento / Mecânico]
    Pausada --> EmAndamento : IniciarApontamentoAsync() / IniciarOuRetomarServicoAsync()\n[Admin / Atendimento / Mecânico]

    EmAndamento --> AguardandoPecas : AguardarPecasAsync()\n[Admin / Atendimento / Mecânico]
    AguardandoPecas --> EmAndamento : IniciarApontamentoAsync() / IniciarOuRetomarServicoAsync()\n[Admin / Atendimento / Mecânico]

    EmAndamento --> AguardandoCliente : AdicionarServicoAditivoAsync() / AdicionarPecaAditivaAsync()\n[Admin / Atendimento / Mecânico]\nCria aditivo Pendente
    AguardandoCliente --> EmAndamento : AprovarAditivoAsync()\n[Admin / Atendimento]\nAprova/Recusa aditivos e reserva

    EmAndamento --> Concluida : ConcluirOSAsync()\n[Admin / Atendimento / Mecânico]\nExige itens executados/aplicados e sem aditivos
    Concluida --> Entregue : EntregarOSAsync(kmSaida, pagamentos)\n[Admin / Atendimento]\nExige KmSaida >= KmEntrada e quitação

    EmAndamento --> Cancelada : CancelarOSAsync(motivo)\n[Admin / Atendimento]\nEstorna peças aplicadas e libera reservas
    Pausada --> Cancelada : CancelarOSAsync(motivo)\n[Admin / Atendimento]\nEstorna peças aplicadas e libera reservas
    AguardandoPecas --> Cancelada : CancelarOSAsync(motivo)\n[Admin / Atendimento]\nEstorna peças aplicadas e libera reservas
    AguardandoCliente --> Cancelada : CancelarOSAsync(motivo)\n[Admin / Atendimento]\nEstorna peças aplicadas e libera reservas

    Recusada --> [*]
    Entregue --> [*]
    Cancelada --> [*]
```

### Regras de Transição e Edição (`AtualizarOSAsync`) e Modelo da Fase 2
1. **`Orcamento` (Estado Inicial)**: Peças e serviços podem ser orçados livremente sem reservar nem descontar saldo físico do estoque. Na abertura da O.S., são registrados obrigatoriamente `QueixaCliente`, `KmEntrada`, `NivelCombustivelEntrada` e `ValidadeOrcamentoAte` (`DataEmissao + Configuracao.ValidadeOrcamentoDias`). Permite editar queixa do cliente (`QueixaCliente`), diagnóstico técnico (`Diagnostico`), previsão de entrega (`DataPrevisaoEntrega >= DataEmissao`), catálogo de serviços vinculados e desconto (`ValorDesconto`, restrito ao perfil `Administrador`).
2. **Aprovação do Orçamento (`AprovarOSAsync`)**:
   - Restrito a `Administrador` e `Atendimento`.
   - Bloqueado se `ValidadeOrcamentoAte < hoje` (horário de Brasília). Exige renovação prévia por `Administrador` (`RenovarValidadeOrcamentoAsync`), que registra `ContatoOS`.
   - Se `Configuracao.ObrigarVistoriaParaOS == true`, exige vistoria com `Status == Concluida`.
   - Permite aprovação parcial por item: itens aprovados ficam `StatusItem.Aprovado`, os demais ficam `StatusItem.Recusado`. Exige ao menos um item aprovado.
   - **Reserva de Estoque**: Peças de origem Estoque aprovadas são **reservadas** (não baixadas fisicamente). A disponibilidade é validada como `EstoqueAtual - QuantidadeReservada`.
   - Grava `AprovadoPorUsuarioId`, `DataAutorizacao`, `AutorizadoPor`, `TipoContatoAutorizacao`, transita para `Aprovada` e registra `ContatoOS` automático.
3. **Recusa do Orçamento (`RecusarOrcamentoAsync`)**: Transita `Orcamento → Recusada`, grava motivo obrigatório (≥ 5 caracteres) e registra `ContatoOS`.
4. **Atribuição de Mecânico (`AtribuirMecanicoAsync`)**: `Atendimento` e `Administrador` podem atribuir qualquer mecânico. Um `Mecanico` pode assumir para si uma O.S. sem responsável.
5. **Execução e Apontamentos de Tempo (`IniciarApontamentoAsync` / `PausarApontamentoAsync`)**:
   - Cria `ApontamentoTempo` vinculado ao mecânico e à O.S./item de serviço. Bloqueia a criação de dois apontamentos abertos concorrentes para o mesmo mecânico.
   - O primeiro apontamento move a O.S. de `Aprovada → EmAndamento`.
   - `PausarApontamentoAsync` fecha o apontamento registrando `Fim = DateTime.UtcNow`.
6. **Aplicação de Peças (`MarcarPecaAplicadaAsync`)**:
   - Converte a reserva em saída física real no inventário (`_estoqueService.BaixarPecaAplicadaAsync`), marcando `Aplicada = true`.
7. **Aditivos (`AdicionarServicoAditivoAsync` / `AdicionarPecaAditivaAsync` / `AprovarAditivoAsync`)**:
   - Mecânicos, Atendimento ou Administrador podem propor aditivos durante a execução. O item nasce com `EhAditivo = true`, `StatusItem = Pendente` e move a O.S. para `AguardandoCliente`.
   - Itens pendentes não entram em `ValorTotal` aprovado, mas são totalizados em `ValorTotalAditivosPendentes`.
   - Apenas `Administrador` e `Atendimento` podem aprovar aditivos (`AprovarAditivoAsync`). Itens aprovados passam a compor o escopo aprovado (e peças de estoque são reservadas); itens rejeitados tornam-se `Recusado`. A O.S. retorna para `EmAndamento` com registro de `ContatoOS`.
8. **Conclusão da O.S. (`ConcluirOSAsync`)**:
   - Exige que todos os itens aprovados estejam marcados como `Executado` / `Aplicada` (ou justificativa formal informada).
   - Bloqueia a conclusão se houver qualquer aditivo pendente.
   - Fecha automaticamente apontamentos em aberto.
   - Persiste o checklist de conclusão (fluidos, limpeza, ferramentas etc.) no campo `ChecklistConclusaoJson`.
   - Transita para `Concluida` e preenche `DataConclusao = DateTime.UtcNow`.
9. **Entrega do Veículo (`EntregarOSAsync`)**:
   - Transita `Concluida → Entregue`.
   - Valida `KmSaida >= KmEntrada`.
   - Valida que a soma dos pagamentos cobre `ValorTotal` (ou autoriza com saldo em aberto exclusivamente pelo perfil `Administrador`, registrando observação em `ContatoOS`).
   - Persiste os registros em `Pagamento`, grava snapshot de `GarantiaDias` (vindo de `Configuracao`), preenche `DataEntrega = DateTime.UtcNow` e atualiza `Veiculo.Quilometragem` com `KmSaida`.
10. **Cancelamento da O.S. (`CancelarOSAsync`)**:
    - Exige motivo obrigatório (≥ 5 caracteres) e perfil `Administrador` ou `Atendimento`.
    - Estorna fisicamente apenas peças que estavam marcadas como `Aplicada = true`. Peças apenas reservadas têm suas reservas liberadas automaticamente.

---

## 2. Matriz de Papéis × Ações (Autorização Server-Side)

Além da ocultação visual na interface (`<AuthorizeView>`), todas as operações críticas validam o perfil do usuário autenticado diretamente na camada de serviço (`OrdemServicoService`, `InativacaoService`, `UsuarioService` e atributos `[Authorize(Roles = ...)]`):

| Ação / Operação | `Administrador` | `Atendimento` | `Mecanico` | Público (`AllowAnonymous`) |
|---|:---:|:---:|:---:|:---:|
| Criar O.S. / Orçamento (`/ordens-servico/nova`) | ✅ | ✅ | ❌ | ❌ |
| Editar O.S. (`AtualizarOSAsync`) | ✅ (inclui Desconto) | ✅ (sem alterar Desconto) | ❌ | ❌ |
| Realizar Checklist de Vistoria | ✅ | ✅ | ✅ | ❌ |
| Aprovar Orçamento (`AprovarOSAsync`) | ✅ | ✅ | ❌ (Bloqueado) | ❌ |
| Renovar Validade do Orçamento (`RenovarValidadeOrcamentoAsync`) | ✅ | ❌ (Bloqueado) | ❌ (Bloqueado) | ❌ |
| Recusar Orçamento (`RecusarOrcamentoAsync`) | ✅ | ✅ | ❌ (Bloqueado) | ❌ |
| Atribuir Mecânico Responsável (`AtribuirMecanicoAsync`) | ✅ | ✅ | ✅ (Apenas assumir O.S. sem dono) | ❌ |
| Iniciar / Pausar Apontamento de Tempo (`IniciarApontamentoAsync`) | ✅ | ✅ | ✅ | ❌ |
| Marcar Peça Aplicada / Serviço Executado | ✅ | ✅ | ✅ | ❌ |
| Propor Aditivo (Serviço ou Peça) | ✅ | ✅ | ✅ | ❌ |
| Aprovar / Recusar Aditivo (`AprovarAditivoAsync`) | ✅ | ✅ | ❌ (Bloqueado) | ❌ |
| Concluir O.S. com Checklist (`ConcluirOSAsync`) | ✅ | ✅ | ✅ | ❌ |
| Entregar Veículo e Registrar Pagamentos (`EntregarOSAsync`) | ✅ (Permite saldo em aberto) | ✅ (Apenas com quitação integral) | ❌ (Bloqueado) | ❌ |
| Cancelar O.S. com Motivo (`CancelarOSAsync`) | ✅ | ✅ | ❌ | ❌ |
| Adicionar Peça pelo Preço Base (`AdicionarPecaAsync`) | ✅ | ✅ | ✅ (Painel) | ❌ |
| Adicionar Peça **com Desconto** (`ValorCobrado < PrecoVenda`) | ✅ | ❌ (Bloqueado) | ❌ (Bloqueado) | ❌ |
| Remover Peça da O.S. (`RemoverPecaAsync`) | ✅ | ✅ | ❌ | ❌ |
| Inativar / Reativar Cadastros (`InativacaoService`) | ✅ | ❌ (Bloqueado) | ❌ (Bloqueado) | ❌ |
| Acompanhar O.S. via Token (`/acompanhar/{token}`) | ✅ | ✅ | ✅ | ✅ (Somente leitura) |
| Iniciar / Retomar / Pausar / Concluir Execução | ✅ | ✅ | ✅ | ❌ |
| Adicionar Peça pelo Preço Base (`AdicionarPecaAsync`) | ✅ | ✅ | ✅ (Painel) | ❌ |
| Adicionar Peça **com Desconto** (`ValorCobrado < PrecoVenda`) | ✅ | ❌ (Bloqueado) | ❌ (Bloqueado) | ❌ |
| Remover Peça da O.S. (`RemoverPecaAsync`) | ✅ | ✅ | ❌ | ❌ |
| Cadastrar / Editar Clientes, Veículos, Peças e Serviços | ✅ | ✅ | ❌ (Consulta Veículos/Serviços) | ❌ |
| Inativar / Reativar Clientes, Veículos, Peças e Serviços (`InativacaoService`) | ✅ | ❌ (Bloqueado) | ❌ (Bloqueado) | ❌ |
| Acessar Central de Relatórios (`/relatorios`) — Abas Clientes / Serviços / Estoque | ✅ | ✅ | ❌ (Bloqueado) | ❌ |
| Acessar Aba **Financeiro** em Relatórios | ✅ | ❌ (Bloqueado no servidor) | ❌ (Bloqueado) | ❌ |
| Gerenciar Usuários e Configurações da Oficina | ✅ | ❌ | ❌ | ❌ |
| Acompanhar O.S. via Token (`/acompanhar/{token}`) | ✅ | ✅ | ✅ | ✅ (Somente leitura) |

---

## 3. Regra de Estoque: Ponto Único de Verdade (`EstoqueService` + `PecaRepository`)

O controle de inventário segue estritamente o ciclo de vida da O.S., garantindo que nunca ocorra baixa em duplicidade nem saldo físico negativo:

| Momento no Ciclo da O.S. | Status da O.S. | Efeito no `Peca.EstoqueAtual` e Reserva | Registro em `MovimentacaoEstoque` |
|---|---|---|---|
| Adição/remoção de peça durante orçamento | `Orcamento` | **Nenhum** (0) | Nenhum |
| Aprovação do orçamento (`AprovarOSAsync`) | `Orcamento` → `Aprovada` | **Reserva** peças de origem `Estoque` aprovadas (`Aplicada = false`), reduzindo `EstoqueDisponivel` sem baixar `EstoqueAtual` | Nenhum (apenas reserva lógica) |
| Aplicação efetiva da peça na execução (`MarcarPecaAplicadaAsync`) | `Aprovada` / `EmAndamento` | **Baixa física** no `EstoqueAtual` (`-Quantidade`) e marca `Aplicada = true` (converte reserva em saída) | `TipoMovimentacao.Saida`, `Quantidade = -qtd`, `DataHora = UTC`, `UsuarioId` do claim |
| Remoção de item de peça em O.S. já aprovada (`RemoverPecaAsync`) | Pós-aprovação | Se `Aplicada == true`, **estorna fisicamente** (`+Quantidade`); se `Aplicada == false`, apenas libera a reserva | `TipoMovimentacao.Estorno` (somente se `Aplicada == true`) |
| Cancelamento de O.S. (`CancelarOSAsync`) | Qualquer não terminal → `Cancelada` | **Libera reservas** (`Aplicada == false`) e **estorna fisicamente** apenas peças com `Aplicada == true` | `TipoMovimentacao.Estorno` para cada peça `Aplicada == true` |

### Fluxo Operacional nas Telas (Fase 3 — BPMN Textual)
1. **Abertura de Orçamento (`/ordens-servico/nova` — `Create.razor`)**: estruturada em 3 blocos: (a) Veículo + KM de entrada + Combustível; (b) Queixa do Cliente + Diagnóstico opcional; (c) Linhas de Serviços e Peças (com seleção de `Origem`: `Estoque`, `SobDemanda` ou `ClienteTrouxe`). Calcula total em tempo real e sugere `DataPrevisaoEntrega` em dias úteis com base na soma de `TempoEstimadoMinutos`.
2. **Vistoria (`/ordensdeservico/{id}/vistoria` — `VistoriaChecklist.razor`)**: herda o KM da O.S. (somente leitura) e exibe alerta caso `ObrigarVistoriaParaOS` esteja ativo.
3. **Gestão e Ciclo de Vida (`/ordens-servico/{id}` — `Details.razor`)**: exibe stepper do ciclo (`Orçamento → Aprovada → Execução → Concluída → Entregue`), modal de aprovação parcial por checkbox de item, seção de aditivos pendentes, atribuição de mecânico, apontamentos por mecânico, modal de conclusão com checklist persistido (`ChecklistConclusaoJson`) e modal de entrega com `KmSaida` e múltiplas formas de `Pagamento`.
4. **Painel Operacional (`/operacional/painel` — `PainelMecanico.razor`)**: filtra O.S. do mecânico logado + coluna "Sem responsável (assumir)", oculta orçamentos e preços de custo, e permite iniciar/pausar apontamento por serviço, aplicar peças, propor aditivos e concluir com checklist.
5. **Listagem e Agenda (`/ordens-servico` e `/agenda`)**: filtros por todos os novos status, coluna de mecânico responsável e badge de orçamento vencido.

### Atomicidade e Concorrência Otimista
- **Transação de Banco (`BeginTransactionAsync`)**: Na aprovação da O.S., caso a 2ª ou N-ésima peça não possua estoque suficiente, toda a transação sofre *rollback*, nenhuma peça é baixada e a O.S. permanece em `Orcamento`.
- **Token de Concorrência (`xmin`)**: A entidade `Peca` mapeia a coluna de sistema `xmin` do PostgreSQL (`IsRowVersion()`). Se dois operadores movimentarem a mesma peça simultaneamente, o EF Core lança `DbUpdateConcurrencyException`, a transação é revertida e o usuário recebe mensagem clara orientando a atualizar a tela.

---

## 4. Política de Soft Delete Padronizado (`IInativavel` + `InativacaoService`) e Imutabilidade Histórica de O.S.

### 4.1 Por que Inativação Lógica (*Soft Delete*) e não Exclusão Física (`DELETE`)?
Em uma oficina mecânica, Clientes, Veículos, Peças e Serviços participam diretamente de registros contratuais, fiscais e de garantia (Ordens de Serviço e Movimentações de Estoque). A exclusão física (`DELETE`) causaria um de dois problemas graves:
1. **Perda de histórico**: Excluir em cascata apagaria ordens de serviço concluídas, distorcendo o faturamento histórico, o livro de movimentações de estoque e o prontuário técnico do veículo.
2. **Erros de chave estrangeira (`DeleteBehavior.Restrict`)**: Impediria a limpeza operacional do catálogo quando um serviço deixa de ser oferecido ou quando uma peça sai de linha após já ter sido usada no passado.

Por isso, `Cliente`, `Veiculo`, `Peca` e `Servico` implementam a interface comum `IInativavel`:
- `Ativo` (`bool`, padrão `true`)
- `DataInativacao` (`DateTime?` em UTC)
- `InativadoPorUsuarioId` (`int?`, FK para `Usuario` com `DeleteBehavior.SetNull`)
- `MotivoInativacao` (`string?`, até 300 caracteres, obrigatório ao inativar com mínimo de 5 caracteres)

### 4.2 Por que NÃO usar `HasQueryFilter` Global no EF Core?
Um filtro global (`HasQueryFilter(e => e.Ativo)`) no `AppDbContext` ocultaria automaticamente registros inativos em **todos** os `Include()` do EF Core:
- Uma O.S. concluída há 6 meses cujo veículo ou cliente foi posteriormente inativado apareceria com `os.Veiculo == null` na tela de Detalhes, na Impressão da O.S. e nos Relatórios financeiros.
- Um serviço antigo que saiu do catálogo (`Servico.Ativo = false`) deixaria de aparecer na lista de serviços executados de O.S. históricas (`os.ServicosAExecutarList`).

**Decisão arquitetural**: Não há `HasQueryFilter` global em `AppDbContext`. Em vez disso, foram criados os métodos de extensão explícitos `.Ativos()` e `.Inativos()` em `IInativavelExtensions` (`Interfaces/IInativavel.cs`):
- **Telas de seleção e criação de novas O.S. / entradas de estoque** consultam explicitamente `.Ativos()`.
- **Consultas de histórico, detalhes de O.S., prontuário do veículo e relatórios de período** preservam a navegação completa sem amputar referências a cadastros posteriormente inativados.

### 4.3 Regras de Bloqueio, Cascata e Reativação (`InativacaoService`)
Todas as operações de inativação e reativação exigem perfil **`Administrador`** validado no servidor, executam em transação (`BeginTransactionAsync`) e seguem a tabela de regras abaixo:

| Entidade | Condição que Bloqueia a Inativação | Efeito Colateral na Inativação | Regra de Reativação | Tratamento de Duplicidade (Cadastro) |
|---|---|---|---|---|
| **Cliente** | Qualquer veículo do cliente possuir O.S. em aberto (`Orcamento`, `Aprovada`, `EmAndamento`, `Pausada`, `AguardandoPecas`, `AguardandoCliente`). | **Cascata controlada**: inativa também todos os veículos ativos do cliente (registrando data, usuário e motivo). | Reativa apenas o cliente; seus veículos permanecem inativos até serem reativados individualmente. | Se o CPF já pertencer a um cliente inativo, bloqueia e orienta: *"Já existe um cliente inativo com este CPF: \<nome\>. Reative-o em Clientes > Inativos."* |
| **Veículo** | Possuir O.S. em aberto (`Orcamento`, `Aprovada`, `EmAndamento`, `Pausada`, `AguardandoPecas`, `AguardandoCliente`). | Nenhum. Prontuário e O.S. concluídas/entregues/canceladas permanecem íntegros. | **Exige cliente proprietário ativo** (bloqueia caso `Veiculo.Cliente.Ativo == false`). | Se a placa já pertencer a um veículo inativo, bloqueia e orienta a reativação em *Veículos > Inativos*. |
| **Peça** | Estar vinculada a O.S. em aberto **OU** possuir `EstoqueAtual > 0` (*"zere o saldo por Ajuste antes de desativar"* — decisão conservadora porque o capital imobilizado soma apenas peças ativas). | Deixa de compor os indicadores de posição atual de peças ativas e capital imobilizado. | Permitida livremente para Administrador. | Se o SKU já pertencer a uma peça inativa, bloqueia e orienta a reativação em *Estoque > Inativos*. |
| **Serviço** | Estar vinculado a O.S. em aberto (`Orcamento`, `Aprovada`, `EmAndamento`, `Pausada`, `AguardandoPecas`, `AguardandoCliente`). | Sai do catálogo de criação/edição de O.S., mas continua visível em todas as O.S. antigas. | Permitida livremente para Administrador. | N/A (serviços são identificados por ID). |

### 4.4 Por que Ordem de Serviço NUNCA é Excluída?
Ordens de Serviço representam documentos contratuais e operacionais numerados sequencialmente (Art. 40 do Código de Defesa do Consumidor) e possuem vínculo direto com movimentações físicas de estoque, vistorias de entrada e registros de aceite/contato do cliente.
- **Decisão**: O sistema **não possui exclusão física nem inativação de O.S.** Caso um orçamento seja recusado ou um serviço precise ser interrompido definitivamente, utiliza-se exclusivamente a operação **Cancelar O.S. (`CancelarOSAsync`)**, que exige motivo obrigatório (`MotivoCancelamento`, mínimo de 5 caracteres), registra data/hora UTC (`DataCancelamento`) e o usuário responsável (`CanceladaPorUsuarioId`), além de estornar automaticamente o saldo de peças caso a O.S. já estivesse aprovada.

---

## 5. Decisões de Arquitetura (ADRs)

### ADR-01: Persistência Universal de Datas em UTC e Exibição em `America/Sao_Paulo`
- **Contexto**: O Npgsql 6+ exige `DateTimeKind.Utc` para colunas `timestamp with time zone` (`timestamptz`). O switch legado `Npgsql.EnableLegacyTimestampBehavior` mascarava inconsistências de fuso horário entre servidor/container (geralmente UTC) e usuários no Brasil (`America/Sao_Paulo`).
- **Decisão**: Removido o switch legado. Adicionado `UtcDateTimeConverter` global em `AppDbContext.ConfigureConventions` e migration convertendo todas as colunas de data para `timestamp with time zone`. Toda gravação no backend usa `DateTime.UtcNow`, e a camada de apresentação utiliza os métodos de extensão `.ToBrasiliaTime()` e `DateTimeExtensions.TodayBrasilia()`.

### ADR-02: Competência Financeira Baseada em `DataConclusao` e `RelatorioService` Único
- **Contexto**: Filtrar faturamento e margem de lucro por `DataEmissao` ou `DataAutorizacao` distorcia os relatórios mensais quando uma O.S. era orçada em um mês e concluída no mês seguinte. Além disso, o Dashboard (`Home.razor`) e a Central de Relatórios (`Relatorios/Index.razor`) possuíam lógicas duplicadas.
- **Decisão**: Adicionada a coluna `DataConclusao` (`DateTime?` em UTC) preenchida em `ConcluirOSAsync` e criado o serviço único `Services/RelatorioService.cs`, consumido tanto pela `Home.razor` quanto por `Relatorios/Index.razor`. As agregações são executadas no banco de dados preservando exatamente a regra de fallback de `ValorMaoDeObraEfetivo` e `ValorPecasEfetivo` (com teste automatizado de paridade comparando o resultado SQL com o cálculo em memória).

### ADR-03: Geração Local de QR Code (`QRCoder`) e Token na Criação
- **Contexto**: O uso anterior de `https://api.qrserver.com/v1/create-qr-code/?data=...` enviava a URL completa de acompanhamento de cada O.S. para um servidor externo de terceiros, expondo o token de acesso do cliente.
- **Decisão**: O `TokenAcompanhamento` passa a ser gerado na criação da O.S. (com índice `UNIQUE` no PostgreSQL), e o QR Code é renderizado em memória no próprio servidor utilizando a biblioteca open-source `QRCoder` (`PngByteQRCode` em Base64 Data URI), eliminando dependências externas e vazamento de metadados.

### ADR-04: Eliminação de Credenciais Fixas e Endurecimento de Sessão
- **Contexto**: Existia senha padrão fixa no código e duplicidade entre seed de migration e inicialização em `Program.cs`.
- **Decisão**: A senha inicial do `admin` é lida de `Admin:InitialPassword` (User Secrets / variável de ambiente) ou gerada com `RandomNumberGenerator.GetString` (16 caracteres) e exibida uma única vez no log de inicialização. Adicionado `OnValidatePrincipal` no Cookie Authentication para invalidar imediatamente sessões de usuários inativados ou cujo perfil tenha sido alterado, além de *Rate Limiting* (5 requisições/minuto por IP) no endpoint `/api/auth/login`.

---

## 6. Proteção de Dados Pessoais e Conformidade com a LGPD (Lei nº 13.709/2018)

### 6.1 Mapeamento de Dados Pessoais Coletados
| Titular | Dados Pessoais Coletados | Finalidade Específica | Base Legal (Art. 7º LGPD) |
|---|---|---|---|
| **Cliente** (`Cliente` / `Veiculo`) | Nome completo, CPF, telefone, e-mail, endereço residencial/comercial e placa do veículo | Identificação contratual do proprietário e do bem confiado à guarda da oficina; emissão de orçamento prévio e termo de garantia | **Execução de contrato** (Art. 7º, V) e **Cumprimento de obrigação legal** — Art. 40 do CDC (Lei nº 8.078/1990) (Art. 7º, II) |
| **Cliente** (`ContatoOS` / `OrdemServico`) | Nome do autorizador, meio de aceite, data/hora da autorização e histórico de interações | Comprovação do aceite do orçamento (prevenção de litígios) e comunicação sobre status do reparo | **Execução de contrato** (Art. 7º, V) e **Exercício regular de direitos** (Art. 7º, VI) |
| **Colaborador / Operador** (`Usuario`) | Nome completo, `username`, perfil de acesso e logs de auditoria (`UsuarioId` em movimentações, inativações e cancelamentos) | Controle de acesso ao sistema, segregação de funções e auditoria interna de baixas/entradas de estoque e cancelamentos | **Execução de contrato** (Art. 7º, V) e **Legítimo interesse** do controlador na segurança patrimonial (Art. 7º, IX) |

### 6.2 Medidas Técnicas e Administrativas de Segurança (Art. 46 LGPD)
1. **Minimização na Página Pública (`/acompanhar/{token}`)**: A consulta pública exibe apenas dados operacionais do veículo (marca/modelo, placa, status da O.S., previsão de entrega e descrição dos serviços), **omitindo CPF, telefone, e-mail, endereço e valores financeiros detalhados** do titular.
2. **Prevenção de Vazamento a Terceiros**: Geração 100% local do QR Code (`QRCoder`), garantindo que links de acompanhamento nunca trafeguem para APIs externas.
3. **Controle de Acesso e Autenticação Forte**:
   - Senhas armazenadas exclusivamente como hashes `BCrypt` (`BCrypt.Net-Next`) com validação de política mínima (mínimo 8 caracteres, bloqueio de senhas comuns e proibição de senha igual ao login).
   - Cookies de sessão protegidos com `HttpOnly`, `SameSite=Strict`, `SecurePolicy=Always` fora de desenvolvimento e revogação imediata via `OnValidatePrincipal` caso o operador seja desativado.
   - Proteção contra enumeração de credenciais e força bruta via middleware `RateLimiter` no login.
4. **Sanitização e Unicidade**: CPF e telefones são normalizados (apenas dígitos) via `ValueConverter` no `AppDbContext`, evitando duplicidade cadastral e inconsistências.
5. **Retenção Auditável e Exclusão Lógica (*Soft Delete* via `IInativavel`)**: A inativação de um cliente ou veículo marca `Ativo = false` com registro de data, responsável e motivo, ocultando-o das listagens operacionais padrão via filtro explícito `.Ativos()`, mas preservando a integridade referencial de ordens de serviço pretéritas pelo prazo legal de garantia e obrigações fiscais/contratuais.
6. **Tratamento Seguro de Erros (`DomainErrorHelper`)**: Exceções internas de banco ou infraestrutura são registradas apenas no `ILogger` do servidor, nunca expondo *stack traces*, nomes de tabelas ou dados sensíveis na interface do usuário.
