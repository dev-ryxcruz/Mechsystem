# Entrega de CRUDs e Módulo de Relatórios — MechSystem

Este documento sintetiza a matriz de operações (**Criar / Listar / Detalhar / Editar / Inativar-Reativar / Cancelar**), as validações de negócio, os perfis autorizados no servidor e a cobertura de testes automatizados (`mechsystem.Tests`) para cada módulo entregue.

---

## 1. CRUD de Clientes (`/clientes`)

Implementação: [`Clientes/Listar.razor`](../Components/Pages/Clientes/Listar.razor), [`Clientes/Cadastro.razor`](../Components/Pages/Clientes/Cadastro.razor), [` Clientes/Editar.razor`](../Components/Pages/Clientes/Editar.razor), [`ClienteRepository.cs`](../Repositories/ClienteRepository.cs) e [`InativacaoService.cs`](../Services/InativacaoService.cs).

| Operação | Rota / Método | Validações e Regras de Negócio | Perfis Permitidos | Testes que Cobrem (`mechsystem.Tests`) |
|---|---|---|---|---|
| **Criar** | `/clientes/cadastro` (`ClienteRepository.Salvar`) | Nome obrigatório (máx. 100), CPF obrigatório com dígitos verificadores válidos (`[CpfValidation]`), normalizado apenas para dígitos. Se o CPF já pertencer a um cliente **inativo**, exibe: *"Já existe um cliente inativo com este CPF: \<nome\>. Reative-o em Clientes > Inativos."* | `Administrador`, `Atendimento` | `InativacaoTests.DuplicidadeComRegistroInativo_DeveInformarMensagemDeReativacaoParaCpfPlacaESku` |
| **Listar** | `/clientes` (`ClienteRepository.ListarPaginadoAsync`) | Paginação no banco (10/25/50) com busca server-side por nome, CPF ou telefone. Filtro explícito `.Ativos()` / `.Inativos()` (aba *Inativos* visível para `Administrador`). | `Administrador`, `Atendimento` | `InativacaoTests.PaginacaoEBuscaNoBanco_DeveFiltrarEPaginarCorretamenteNosCincoCruds` |
| **Detalhar** | `/clientes` (Card/Linha + Veículos vinculados) | Exibe CPF e telefone formatados e, quando inativo, exibe o motivo da inativação. | `Administrador`, `Atendimento` | `InativacaoTests.InativarCliente_SemOSAberta_DeveInativarVeiculosEmCascata_EReativarNaoReativaVeiculos` |
| **Editar** | `/clientes/editar/{id}` (`ClienteRepository.Atualizar`) | Valida formato e unicidade do CPF (incluindo detecção de conflito com outro registro inativo). | `Administrador`, `Atendimento` | `InativacaoTests.DuplicidadeComRegistroInativo_DeveInformarMensagemDeReativacaoParaCpfPlacaESku` |
| **Inativar / Reativar** | Modal em `/clientes` (`InativacaoService.InativarClienteAsync` / `ReativarClienteAsync`) | Exige perfil `Administrador` no servidor e motivo obrigatório (mín. 5 caracteres). **Bloqueia** se qualquer veículo do cliente tiver O.S. em aberto (`Orcamento`, `EmAndamento`, `Pausada`, `AguardandoPecas`). Caso permitido, **inativa em cascata os veículos ativos** do cliente (o modal informa a quantidade). **Reativar cliente NÃO reativa seus veículos automaticamente.** | `Administrador` | `InativacaoTests.InativacaoEReativacao_ComPerfilNaoAdmin_DeveLancarUnauthorizedAccessException`, `InativacaoTests.InativarCliente_ComOSAbertaEmVeiculo_DeveBloquear`, `InativacaoTests.InativarCliente_SemOSAberta_DeveInativarVeiculosEmCascata_EReativarNaoReativaVeiculos` |

---

## 2. CRUD de Veículos (`/veiculos`)

Implementação: [`Veiculos/Listar.razor`](../Components/Pages/Veiculos/Listar.razor), [`Veiculos/Cadastro.razor`](../Components/Pages/Veiculos/Cadastro.razor), [`Veiculos/Editar.razor`](../Components/Pages/Veiculos/Editar.razor), [`Veiculos/Detalhes.razor`](../Components/Pages/Veiculos/Detalhes.razor), [`VeiculoRepository.cs`](../Repositories/VeiculoRepository.cs) e [`InativacaoService.cs`](../Services/InativacaoService.cs).

| Operação | Rota / Método | Validações e Regras de Negócio | Perfis Permitidos | Testes que Cobrem (`mechsystem.Tests`) |
|---|---|---|---|---|
| **Criar** | `/veiculos/cadastro` (`VeiculoRepository.Salvar`) | Placa obrigatória (padrão antigo ou Mercosul, normalizada sem hífen e em maiúsculas), proprietário ativo obrigatório, ano entre 1950 e 2100, KM >= 0. Se a placa já pertencer a um veículo **inativo**, informa e orienta reativar em *Veículos > Inativos*. | `Administrador`, `Atendimento` | `InativacaoTests.DuplicidadeComRegistroInativo_DeveInformarMensagemDeReativacaoParaCpfPlacaESku` |
| **Listar** | `/veiculos` (`VeiculoRepository.ListarPaginadoAsync`) | Paginação no banco (10/25/50) com busca server-side por placa, marca, modelo ou nome do proprietário. Filtro `.Ativos()` / `.Inativos()`. | `Administrador`, `Atendimento`, `Mecanico` | `InativacaoTests.PaginacaoEBuscaNoBanco_DeveFiltrarEPaginarCorretamenteNosCincoCruds` |
| **Detalhar (Prontuário)** | `/veiculos/detalhes/{id}` (`VeiculoRepository.BuscarPorId`) | Exibe dados cadastrais e todo o histórico de Ordens de Serviço do veículo (mesmo se o veículo ou cliente estiver inativo). | `Administrador`, `Atendimento`, `Mecanico` | `InativacaoTests.InativarVeiculo_ComOSAberta_DeveBloquear_EReativarExigeClienteAtivo` |
| **Editar** | `/veiculos/editar/{id}` (`VeiculoRepository.Atualizar`) | Valida unicidade da placa e preserva vínculo com o proprietário (incluindo proprietário inativo caso já estivesse vinculado). | `Administrador`, `Atendimento` | `InativacaoTests.DuplicidadeComRegistroInativo_DeveInformarMensagemDeReativacaoParaCpfPlacaESku` |
| **Inativar / Reativar** | Modal em `/veiculos` (`InativacaoService.InativarVeiculoAsync` / `ReativarVeiculoAsync`) | Exige perfil `Administrador` no servidor e motivo obrigatório (mín. 5 caracteres). **Bloqueia inativação** se houver O.S. em aberto. **Bloqueia reativação** se o cliente proprietário estiver inativo. | `Administrador` | `InativacaoTests.InativarVeiculo_ComOSAberta_DeveBloquear_EReativarExigeClienteAtivo`, `InativacaoTests.InativacaoEReativacao_ComPerfilNaoAdmin_DeveLancarUnauthorizedAccessException` |

---

## 3. CRUD de Estoque de Peças (`/estoque`)

Implementação: [`Estoque/Index.razor`](../Components/Pages/Estoque/Index.razor), [`Estoque/Cadastro.razor`](../Components/Pages/Estoque/Cadastro.razor), [`Estoque/Entrada.razor`](../Components/Pages/Estoque/Entrada.razor), [`Estoque/Ajuste.razor`](../Components/Pages/Estoque/Ajuste.razor), [`PecaRepository.cs`](../Repositories/PecaRepository.cs) e [`InativacaoService.cs`](../Services/InativacaoService.cs).

| Operação | Rota / Método | Validações e Regras de Negócio | Perfis Permitidos | Testes que Cobrem (`mechsystem.Tests`) |
|---|---|---|---|---|
| **Criar** | `/estoque/cadastro` (`PecaRepository.AddAsync`) | SKU obrigatório e normalizado em maiúsculas, Nome obrigatório, Preço de Custo >= 0, Preço de Venda >= 0, Estoque Mínimo >= 0. Se o SKU já pertencer a uma peça **inativa**, informa: *"Já existe uma peça inativa com este SKU: \<nome\>. Reative-a em Estoque > Inativos."* | `Administrador`, `Atendimento` | `InativacaoTests.DuplicidadeComRegistroInativo_DeveInformarMensagemDeReativacaoParaCpfPlacaESku` |
| **Listar** | `/estoque` (`PecaRepository.GetPaginadoAsync`) | Paginação no banco (10/25/50) com busca server-side por SKU, nome, marca ou localização. Alerta de ruptura calculado apenas sobre peças ativas. Filtro `.Ativos()` / `.Inativos()`. | `Administrador`, `Atendimento` | `InativacaoTests.PaginacaoEBuscaNoBanco_DeveFiltrarEPaginarCorretamenteNosCincoCruds` |
| **Detalhar (Histórico)** | Modal em `/estoque` (`PecaRepository.GetHistoricoAsync`) | Lista todas as movimentações (`Entrada`, `Saida`, `Ajuste`, `Estorno`) com data/hora em Brasília, quantidade, referência e usuário responsável. | `Administrador`, `Atendimento` | `UnitTest1.FluxoEstoqueOS_Orcamento_Aprovacao_Remocao_Cancelamento_DeveManterSaldoExato` |
| **Editar / Movimentar** | `/estoque/cadastro/{id}`, `/estoque/entrada`, `/estoque/ajuste` | Edição de dados cadastrais valida SKU único; Entrada e Ajuste registram `MovimentacaoEstoque` auditada. Ajuste restrito a `Administrador`. | `Administrador`, `Atendimento` (Ajuste: só `Administrador`) | `InativacaoTests.InativarPeca_ComOSAbertaOuEstoquePositivo_DeveBloquear_ESoPermitirComSaldoZero` |
| **Inativar / Reativar** | Modal em `/estoque` (`InativacaoService.InativarPecaAsync` / `ReativarPecaAsync`) | Exige perfil `Administrador` no servidor e motivo obrigatório (mín. 5 caracteres). **Bloqueia** se a peça estiver em O.S. em aberto **ou** se `EstoqueAtual > 0` (*"zere o saldo por Ajuste antes de desativar"*). Reativação permitida para `Administrador`. | `Administrador` | `InativacaoTests.InativarPeca_ComOSAbertaOuEstoquePositivo_DeveBloquear_ESoPermitirComSaldoZero`, `InativacaoTests.InativacaoEReativacao_ComPerfilNaoAdmin_DeveLancarUnauthorizedAccessException` |

---

## 4. CRUD de Catálogo de Serviços (`/servicos`)

Implementação: [`Servicos/Listar.razor`](../Components/Pages/Servicos/Listar.razor), [`Servicos/Cadastro.razor`](../Components/Pages/Servicos/Cadastro.razor), [`Servicos/Editar.razor`](../Components/Pages/Servicos/Editar.razor), [`ServicoRepository.cs`](../Repositories/ServicoRepository.cs) e [`InativacaoService.cs`](../Services/InativacaoService.cs).

| Operação | Rota / Método | Validações e Regras de Negócio | Perfis Permitidos | Testes que Cobrem (`mechsystem.Tests`) |
|---|---|---|---|---|
| **Criar** | `/servicos/cadastro` (`ServicoRepository.AddAsync`) | Nome obrigatório (máx. 100), Valor Padrão >= 0, Tempo Estimado entre 0 e 10.000 minutos. Nasce com `Ativo = true`. | `Administrador`, `Atendimento` | `InativacaoTests.InativarServico_ComOSAberta_DeveBloquear_ESemOSAberta_MantemHistoricoEmOSAntiga` |
| **Listar** | `/servicos` (`ServicoRepository.GetPaginadoAsync`) | Paginação no banco (10/25/50) com busca server-side por nome ou descrição. Filtro `.Ativos()` / `.Inativos()`. | `Administrador`, `Atendimento`, `Mecanico` | `InativacaoTests.PaginacaoEBuscaNoBanco_DeveFiltrarEPaginarCorretamenteNosCincoCruds` |
| **Detalhar** | `/servicos` (Listagem e vínculo em O.S.) | Exibe código, descrição, tempo estimado, valor padrão e motivo de inativação (quando inativo). O.S. antigas continuam exibindo o serviço mesmo após inativado. | `Administrador`, `Atendimento`, `Mecanico` | `InativacaoTests.InativarServico_ComOSAberta_DeveBloquear_ESemOSAberta_MantemHistoricoEmOSAntiga` |
| **Editar** | `/servicos/editar/{id}` (`ServicoRepository.UpdateAsync`) | Atualiza dados do catálogo sem alterar o snapshot (`PrecoBase`, `ValorCobrado`, `TempoEstimadoSnapshot`) das O.S. já emitidas. | `Administrador`, `Atendimento` | `InativacaoTests.InativarServico_ComOSAberta_DeveBloquear_ESemOSAberta_MantemHistoricoEmOSAntiga` |
| **Inativar / Reativar** | Modal em `/servicos` (`InativacaoService.InativarServicoAsync` / `ReativarServicoAsync`) | Substitui a antiga exclusão física (`Remove`) por inativação lógica. Exige `Administrador` e motivo (mín. 5 caracteres). **Bloqueia** se o serviço estiver vinculado a O.S. em aberto. O catálogo de criação/edição de O.S. lista apenas serviços ativos. | `Administrador` | `InativacaoTests.InativarServico_ComOSAberta_DeveBloquear_ESemOSAberta_MantemHistoricoEmOSAntiga`, `InativacaoTests.InativacaoEReativacao_ComPerfilNaoAdmin_DeveLancarUnauthorizedAccessException` |

---

## 5. Ordem de Serviço (`/ordens-servico`)

Implementação: [`OrdensServico/Index.razor`](../Components/Pages/OrdensServico/Index.razor), [`OrdensServico/Create.razor`](../Components/Pages/OrdensServico/Create.razor), [`OrdensServico/Details.razor`](../Components/Pages/OrdensServico/Details.razor), [`Imprimir/ImprimirOS.razor`](../Components/Pages/Imprimir/ImprimirOS.razor) e [`OrdemServicoService.cs`](../Services/OrdemServicoService.cs).

| Operação | Rota / Método | Validações e Regras de Negócio | Perfis Permitidos | Testes que Cobrem (`mechsystem.Tests`) |
|---|---|---|---|---|
| **Criar** | `/ordens-servico/nova` (`Create.razor`) | Seleciona apenas veículos ativos e serviços ativos; exige problema relatado e previsão de entrega (`>= DataEmissao`); gera `TokenAcompanhamento` único e nasce em `Orcamento`. | `Administrador`, `Atendimento` | `UnitTest1.FluxoEstoqueOS_Orcamento_Aprovacao_Remocao_Cancelamento_DeveManterSaldoExato` |
| **Listar** | `/ordens-servico` (`OrdemServicoService.ListarPaginadoAsync`) | Paginação no banco (10/25/50) com busca server-side por nº da O.S., placa, modelo ou nome do cliente. | `Administrador`, `Atendimento`, `Mecanico` | `InativacaoTests.PaginacaoEBuscaNoBanco_DeveFiltrarEPaginarCorretamenteNosCincoCruds` |
| **Detalhar / Imprimir** | `/ordens-servico/{id}` e `/imprimir/os/{id}` | Exibe itens de peças e serviços, vistoria, contatos, **Valor de Desconto** (quando > 0) e, se cancelada, exibe banner de auditoria com **Motivo, Data/Hora e Usuário que cancelou**. | `Administrador`, `Atendimento`, `Mecanico` | `UnitTest1.CancelarOS_ExigeMotivoMinimo5Caracteres_ERegistraAuditoriaEsterornaEstoque` |
| **Editar** | Modal em `/ordens-servico/{id}` (`OrdemServicoService.AtualizarOSAsync`) | Validado no servidor: **Em `Orcamento`**: permite editar diagnóstico, previsão de entrega (`>= DataEmissao`), lista de serviços a executar e `ValorDesconto` (alterar `ValorDesconto` é permitido **apenas para `Administrador`**). **Após aprovação (`EmAndamento`, `Pausada`, `AguardandoPecas`)**: permite editar **somente a previsão de entrega**, registrando automaticamente um `ContatoOS` (*"Previsão alterada de X para Y por \<usuário\>"*). **`Concluida` e `Cancelada`**: somente leitura (bloqueia qualquer edição). | `Administrador` (todos os campos, incl. Desconto), `Atendimento` (sem alterar Desconto) | `UnitTest1.AtualizarOS_EmOrcamento_PermiteEditarCamposEServicos_EDescontoApenasParaAdmin`, `UnitTest1.AtualizarOS_AposAprovacao_PermiteApenasPrevisaoEntregaEGeraContatoOS_EBloqueiaConcluidaCancelada` |
| **Cancelar (Nunca Excluir)** | Modal em `/ordens-servico/{id}` (`OrdemServicoService.CancelarOSAsync`) | O.S. nunca é excluída fisicamente. Cancelar exige motivo obrigatório (mín. 5 caracteres), registra `MotivoCancelamento`, `DataCancelamento` (UTC) e `CanceladaPorUsuarioId`, e estorna o saldo líquido de peças caso a O.S. já estivesse aprovada. | `Administrador`, `Atendimento` | `UnitTest1.CancelarOS_ExigeMotivoMinimo5Caracteres_ERegistraAuditoriaEsterornaEstoque` |

---

## 6. Módulo de Relatórios (`/relatorios` e `Home.razor`)

Implementação: [`Relatorios/Index.razor`](../Components/Pages/Relatorios/Index.razor), [`Home.razor`](../Components/Pages/Home.razor) e [`RelatorioService.cs`](../Services/RelatorioService.cs).

| Funcionalidade | Regra Implementada | Perfis Permitidos | Testes que Cobrem (`mechsystem.Tests`) |
|---|---|---|---|
| **Autorização da Página e Menu** | Rota `/relatorios` protegida com `[Authorize(Roles = "Administrador, Atendimento")]` e item de menu restrito em `NavConfig.cs`. O perfil `Mecanico` não vê o menu nem acessa a rota. | `Administrador`, `Atendimento` | `RelatorioServiceTests.GerarRelatorioFinanceiro_DeveExigirPerfilAdministradorNoServidor` |
| **Restrição da Aba Financeiro** | A aba **Financeiro** e o método `RelatorioService.GerarRelatorioFinanceiroAsync` exigem perfil `Administrador` validado no servidor (`UnauthorizedAccessException` para `Atendimento` e `Mecanico`). | `Administrador` | `RelatorioServiceTests.GerarRelatorioFinanceiro_DeveExigirPerfilAdministradorNoServidor` |
| **Filtro por Intervalo (De / Até) e Mês/Ano** | Permite alternar entre *Mês/Ano* e *Intervalo (De / Até)*, convertendo os limites do dia em horário de Brasília para UTC e apurando receitas/serviços exclusivamente pelas O.S. concluídas (`DataConclusao`) no período. | `Administrador`, `Atendimento` | `RelatorioServiceTests.RelatorioService_DeveCoincidirComCalculoEmMemoria_PreservandoFallbackDeMaoDeObraEPecas` |
| **Estoque: Posição Atual vs. Período** | Separa explicitamente os indicadores de **Posição Atual** (Peças Ativas, Peças Inativas, Abaixo do Mínimo e Capital Imobilizado de ativas) das **Movimentações no Período** (total, entradas, saídas e ajustes filtrados pelo intervalo). | `Administrador`, `Atendimento` | `RelatorioServiceTests.RelatorioEstoque_DeveSepararPosicaoAtualDeMovimentacoesDoPeriodo_EExportarCsvPtBr` |
| **Auditoria de Emissão e Impressão** | Preenche `GeradoPor` com o nome do usuário autenticado e exibe no cabeçalho de impressão o Nome Fantasia, CNPJ da oficina (`Configuracao`) e a data/hora de emissão em horário de Brasília. | `Administrador`, `Atendimento` | `RelatorioServiceTests.RelatorioEstoque_DeveSepararPosicaoAtualDeMovimentacoesDoPeriodo_EExportarCsvPtBr` |
| **Tabelas de Detalhe e Exportação CSV** | Exibe tabelas de detalhe (O.S. concluídas no período com link, ranking de serviços por quantidade e receita, e movimentações de estoque do período com usuário) e exporta cada tabela em CSV (UTF-8 com BOM, separador `;`, valores numéricos em `pt-BR`). | `Administrador`, `Atendimento` (O.S. Financeiro: só `Administrador`) | `RelatorioServiceTests.RelatorioEstoque_DeveSepararPosicaoAtualDeMovimentacoesDoPeriodo_EExportarCsvPtBr` |
