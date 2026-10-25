# Relatório de Estabilização — MechSystem

**Branch:** `chore/estabilizacao`  
**Stack:** C# / .NET 10, Blazor Server (InteractiveServer), EF Core 10, PostgreSQL (`Npgsql`), Cookie Authentication  
**Objetivo:** Estabilizar integralmente o núcleo operacional, estoque, segurança, testes, containerização e documentação do sistema antes de qualquer funcionalidade de pacotes futuros.

---

## 1. Resumo das Entregas por Fase e Tarefa

### FASE 1 — Integridade do Núcleo

| Tarefa | Status | O que foi verificado e corrigido |
|---|---|---|
| **1.1 Máquina de estados em `Services/OrdemServicoService.cs`** | ✅ Concluído | Criado [`OrdemServicoService.cs`](../Services/OrdemServicoService.cs) com dicionário explícito `TransicoesPermitidas`: `Orcamento → EmAndamento \| Cancelada`; `EmAndamento ↔ Pausada`; `EmAndamento ↔ AguardandoPecas`; `EmAndamento → Concluida`; `EmAndamento/Pausada/AguardandoPecas → Cancelada`; `Concluida` e `Cancelada` como estados finais imutáveis. Toda lógica de mudança de status e acúmulo de tempo (`TempoGastoMinutos`) antes espalhada em `Details.razor` e `PainelMecanico.razor` foi movida para o serviço. |
| **1.2 Estoque com único ponto de verdade (`EstoqueService` / `PecaRepository`)** | ✅ Concluído | Confirmado o bug de **baixa duplicada**: `PainelMecanico.SolicitarPeca` baixava o estoque diretamente e `AprovarOS` baixava novamente todas as peças da O.S. Corrigido centralizando em `OrdemServicoService.AdicionarPecaAsync` / `RemoverPecaAsync` e `EstoqueService`: peças em `Orcamento` não movimentam estoque; `AprovarOSAsync` baixa todas as peças uma única vez; peças adicionadas/removidas em O.S. com estoque já baixado (`EmAndamento`, `Pausada`, `AguardandoPecas`) baixam/estornam imediatamente; `CancelarOSAsync` estorna o saldo líquido apenas se a O.S. já estava aprovada. Padronizadas saídas com quantidade negativa, datas em UTC (`DateTime.UtcNow`) e `UsuarioId` obtido obrigatoriamente do claim `ClaimTypes.NameIdentifier` (removido o `UsuarioId = 1` fixo em `PainelMecanico.razor`, `Details.razor` e `Estoque/Entrada.razor`). |
| **1.3 Atomicidade e concorrência (`xmin`)** | ✅ Concluído | `AprovarOSAsync`, `CancelarOSAsync`, `AdicionarPecaAsync` e `RemoverPecaAsync` executam dentro de transação (`ExecutarEmTransacaoAsync` / `BeginTransactionAsync`). Se faltar estoque em qualquer peça durante a aprovação, toda a transação sofre rollback e nada é gravado. Adicionado token de concorrência otimista nativo do PostgreSQL (`xmin`) em `Peca.Version` (`IsRowVersion()`) via migration `20261024180501_AddPecaRowVersion`, tratando `DbUpdateConcurrencyException` com mensagem amigável. |
| **1.4 Autorização no servidor** | ✅ Concluído | Adicionada validação server-side de perfil (`ObterPerfilAsync` / `EhPerfilValido`) em `AprovarOSAsync`, `CancelarOSAsync`, `ConcluirOSAsync`, `IniciarOuRetomarServicoAsync`, `PausarServicoAsync`, `AguardarPecasAsync`, `AdicionarPecaAsync` e `RemoverPecaAsync`. Desconto abaixo de `PrecoVenda` (`cobrado < peca.PrecoVenda`) agora lança `UnauthorizedAccessException` no servidor se o usuário não for `Administrador` — inclusive quando um não-admin tenta incrementar a quantidade de uma peça já inserida com desconto na O.S. Adicionado `[Authorize(Roles = "Administrador, Atendimento")]` nas páginas de criação/edição de estoque e O.S. |
| **1.5 Seeder (`Data/DatabaseSeeder.cs`)** | ✅ Concluído | Confirmado que `DatabaseSeeder.cs` possuía um bloco que reativava todos os clientes inativos (`Ativo = true`) a cada inicialização, desfazendo o soft delete. Esse trecho foi removido. O seeder agora só é executado quando a aplicação recebe a flag `--seed-demo` em `Program.cs`, populando de forma idempotente 4 clientes com CPFs válidos no módulo 11, 4 veículos, 6 peças, 5 serviços e 5 O.S. distribuídas nos status `Concluida`, `EmAndamento`, `Pausada`, `AguardandoPecas` e `Orcamento`. |
| **1.6 `DataConclusao` em `OrdemServico`** | ✅ Concluído | Adicionada propriedade `DataConclusao` (`DateTime?` em UTC) em `OrdemServico` + migration `20261024181845_AddDataConclusaoToOrdemServico` (com backfill usando ` COALESCE(data_autorizacao, data_emissao)` para O.S. já concluídas). Atualizados `Home.razor`, `EstoqueService.GetMargemLucroRealAsync` e `Relatorios/Index.razor` para filtrar receitas e margem de O.S. concluídas por `DataConclusao`. |
| **1.7 Índices únicos com validação amigável** | ✅ Concluído | Verificado o banco antes da migration (0 duplicatas em `clientes.cpf`, `veiculos.placa` e `ordens_servico.token_acompanhamento`). Criada a migration `20261024182341_AddUniqueIndexesPlacaCpfToken` higienizando dados legados e criando os índices únicos `ix_clientes_cpf`, `ix_veiculos_placa` e `ix_ordens_servico_token_acompanhamento`. Adicionada validação prévia amigável em `ClienteRepository.Salvar` e `VeiculoRepository.Salvar`. |
| **1.8 Página pública `/acompanhar/{token}` e QR Code local** | ✅ Concluído | Em `Components/Pages/Acompanhar/Index.razor`, corrigido o tratamento de `OrdemServicoStatus.Pausada` (passo 3 "Na Oficina", ícone `bi-pause-circle`, cor âmbar e rótulo "Serviço Pausado"). `TokenAcompanhamento` agora é gerado na instanciação/criação da O.S. (`OrdemServico.cs` e `Create.razor`), removendo a gravação tardia na leitura de `Details.razor`. Substituído o serviço externo `api.qrserver.com` pela biblioteca local `QRCoder` (`PngByteQRCode` Base64) em `Details.razor`. |
| **1.9 Padronização de Datas em UTC** | ✅ Concluído | Confirmado que `Npgsql.EnableLegacyTimestampBehavior` já não estava presente em `Program.cs`, mas as colunas do banco ainda estavam como `timestamp without time zone` e o código misturava `DateTime.Now`/`DateTime.Today`. Criado `UtcDateTimeConverter` global em `AppDbContext`, criada a migration `20261024185622_ConvertTimestampsToUtc` convertendo as colunas para `timestamp with time zone` sem perda de dados, padronizado `DateTime.UtcNow` nas gravações e aplicado `.ToBrasiliaTime()` / `DateTimeExtensions.TodayBrasilia()` na exibição e nos filtros de páginas. |

---

### FASE 2 — Segurança Básica

| Tarefa | Status | O que foi verificado e corrigido |
|---|---|---|
| **2.1 Remoção de senha fixa do admin e unificação do bootstrap** | ✅ Concluído | Removida a senha fixa do código, README e console, bem como o `HasData` duplicado em `AppDbContext` (sem apagar o registro de admin já existente no banco). Em `Program.cs`, a senha inicial do admin vem de `Admin:InitialPassword` (user-secrets / variável de ambiente) ou é gerada aleatoriamente com 16 caracteres (`RandomNumberGenerator`) e exibida uma única vez no `ILogger` caso o admin seja novo, possua o hash legado da migration inicial ou seja executado com `--reset-admin`. |
| **2.2 Cookie `SecurePolicy` e `OnValidatePrincipal`** | ✅ Concluído | Configurado `CookieSecurePolicy.Always` fora de `Development` (`SameAsRequest` apenas em `Development`). Implementado `OnValidatePrincipal` consultando `db.Usuarios.AsNoTracking()` para rejeitar a sessão e efetuar sign-out imediato se o usuário for desativado (`!usuario.Ativo`) ou tiver seu perfil alterado. |
| **2.3 Política de senha e Rate Limiting no login** | ✅ Concluído | Implementado `UsuarioService.ValidarPoliticaSenha` (mínimo de 8 caracteres, proibição de senha igual ao `username` e bloqueio de lista de senhas fracas/comuns). Configurado Rate Limiting (`FixedWindowLimiter`: máximo de 5 tentativas por minuto por IP, retornando HTTP 429) no endpoint `/api/auth/login` com mensagem amigável na tela de login. |
| **2.4 Tratamento seguro de exceções e `ILogger`** | ✅ Concluído | Criado `DomainErrorHelper.ObterMensagemSegura` que preserva mensagens de validação de negócio (`InvalidOperationException`, `UnauthorizedAccessException`, `ArgumentException`), mas oculta erros internos de banco/infraestrutura do usuário final registrando os detalhes no `ILogger`. Substituídos todos os `Console.WriteLine` por `ILogger`. |

---

### FASE 3 — Testes, CI e Ambiente Reproduzível

| Tarefa | Status | O que foi verificado e corrigido |
|---|---|---|
| **3.1 Projeto `mechsystem.Tests` (xUnit)** | ✅ Concluído | Criado `mechsystem.Tests` e adicionado à solution `mechsystem.sln`. Implementados testes cobrindo: transições válidas e inválidas da máquina de estados; fluxo completo de estoque (orçamento, aprovação, pausa/retomada sem baixa duplicada, adição/remoção em andamento e cancelamento com estorno líquido); estoque insuficiente na aprovação com rollback total; bloqueio server-side de desconto para não-admin (na adição inicial e na multiplicação de item existente) e validação de perfil em `ConcluirOSAsync`; cálculo de totais e fallback; preenchimento de `DataConclusao` em UTC e cálculo de margem real; rollback transacional relacional em PostgreSQL real (usando `Testcontainers.PostgreSql` com fallback automático para PostgreSQL local ou skip gracioso se indisponível). |
| **3.2 GitHub Actions CI (`.github/workflows/ci.yml`)** | ✅ Concluído | Criado workflow com container de serviço `postgres:16-alpine`, `.NET 10 SDK`, executando `dotnet restore`, `dotnet build --no-restore` e `dotnet test --no-build --verbosity normal`. |
| **3.3 `Dockerfile` e `docker-compose.yml`** | ✅ Concluído | Criados `Dockerfile` multi-stage (`mcr.microsoft.com/dotnet/sdk:10.0` / `aspnet:10.0`), `.dockerignore` e `docker-compose.yml` (serviços `db` PostgreSQL 16 com healthcheck e `app` na porta `8080`, com instruções para `--seed-demo`). |
| **3.4 Remoção de `migration_log.txt`** | ✅ Concluído | Removido `migration_log.txt` do controle de versão (`git rm`) e adicionado ao `.gitignore`. |

---

### FASE 4 — Documentação para a Banca

| Tarefa | Status | O que foi verificado e corrigido |
|---|---|---|
| **4.1 Atualização do `README.md`** | ✅ Concluído | Atualizado com stack real (.NET 10 + PostgreSQL), instruções de execução com e sem Docker, configuração da senha inicial do admin (`Admin:InitialPassword` / `--reset-admin`), uso da flag `--seed-demo`, execução dos testes e resumo de LGPD. |
| **4.2 e 4.3 Documentação Técnica e LGPD em `Documentacao/`** | ✅ Concluído | Criado [`Documentacao/23 - Arquitetura, Regras de Negocio e LGPD.md`](23%20-%20Arquitetura,%20Regras%20de%20Negocio%20e%20LGPD.md) contendo: diagrama de estados Mermaid da O.S., matriz Papéis × Ações, especificação da regra de baixa/estorno de estoque, decisões de arquitetura (ADRs) e seção completa de LGPD. Atualizado também [`Documentacao/21 - Modelagem de Dados (MER).md`](21%20-%20Modelagem%20de%20Dados%20%28MER%29.md) para refletir os nomes exatos do enum `OrdemServicoStatus`. |
| **4.4 Atualização de `analysis_results.md`** | ✅ Concluído | Atualizado [`analysis_results.md`](../analysis_results.md) demarcando todos os itens resolvidos no ciclo de estabilização e os itens que permanecem como evolução futura. |

---

## 2. Itens Confirmados como Não Existentes (ou Já Resolvidos Antes da Tarefa)

1. **`Npgsql.EnableLegacyTimestampBehavior` em `Program.cs` (Tarefa 1.9)**: Ao inspecionar `Program.cs` antes da modificação, o switch `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` já não estava presente no código, porém as migrations anteriores haviam criado colunas como `timestamp without time zone`. O ajuste foi completado com `UtcDateTimeConverter` e a migration `ConvertTimestampsToUtc`.
2. **Duplicatas em `clientes.cpf`, `veiculos.placa` e `ordens_servico.token_acompanhamento` (Tarefa 1.7)**: Foi realizada verificação antes de aplicar a migration `AddUniqueIndexesPlacaCpfToken` e confirmado que não existiam registros duplicados no banco; ainda assim, a migration incluiu higienização preventiva (`regexp_replace`, `upper/replace`, `md5(random()...)`) antes de criar os índices `UNIQUE`.

---

## 3. Decisões Conservadoras de Regra de Negócio

1. **Adição de peça já existente com desconto em uma O.S. (Tarefa 1.4)**: Quando um Administrador adiciona uma peça com preço promocional (`ValorCobrado < PrecoVenda`), apenas outro Administrador pode incrementar a quantidade dessa peça mantendo o preço com desconto; se um usuário `Atendimento` ou `Mecanico` tentar incrementar a quantidade sem informar um novo preço igual ou superior ao `PrecoVenda`, o servidor bloqueia a operação (`UnauthorizedAccessException`) para impedir multiplicação não autorizada de desconto.
2. **Backfill de `DataConclusao` em O.S. legadas (Tarefa 1.6)**: Para ordens de serviço que já estavam com `Status = Concluida` antes da nova coluna `data_conclusao`, a migration preencheu `data_conclusao = COALESCE(data_autorizacao, data_emissao)` para não perder o histórico nos relatórios financeiros.
3. **Rotação automática do hash legado do `admin` (Tarefa 2.1)**: Para não excluir o usuário `admin` existente (que poderia possuir vínculo de chave estrangeira em `movimentacoes_estoque`), `Program.cs` preserva o registro `Id = 1` e detecta especificamente o prefixo do hash BCrypt legado da migration inicial, rotacionando a senha automaticamente no primeiro boot.

---

## 4. Resultado de `dotnet build` e `dotnet test`

- **`dotnet build`**: Compilação concluída com **0 erros e 0 avisos** nos projetos `mechsystem` e `mechsystem.Tests`.
- **`dotnet test --verbosity normal`**: **7 de 7 testes aprovados (100% de aprovação)** em `mechsystem.Tests`:
  1. `TransicoesDeEstado_ValidasEInvalidas_DevemRespeitarMaquinaDeEstados` — **Passed**
  2. `FluxoEstoqueOS_AprovacaoAdicaoRemocaoECancelamento_SemBaixaDuplicada` — **Passed**
  3. `EstoqueInsuficienteNaAprovacao_DeveAbortarSemBaixarNenhumaPecaEManterOrcamento` — **Passed**
  4. `DescontoAbaixoDoPrecoBase_DeveBloquearNaoAdminEPermitirAdmin` — **Passed**
  5. `CalculoTotaisOS_DeveRespeitarFallbackESomaDeItensEDesconto` — **Passed**
  6. `ConcluirOS_DevePreencherDataConclusaoEmUtcECalcularMargemNoPeriodo` — **Passed**
  7. `PostgresRelationalTransactionRollback_DeveReverterBaixaParcialNoBancoReal` — **Passed**

---

## 5. Pendências / Escopo Futuro (Não Implementado por Diretriz do Escopo)

Conforme instrução expressa da tarefa, **não foram implementados** itens pertencentes aos pacotes futuros de evolução:
- Fila de compras / sugestão de pedidos a fornecedores;
- Cálculo de comissões de mecânicos;
- Módulo de contas a receber / frente de caixa diário (meios de pagamento PIX/Cartão);
- Integrações externas (FIPE, ViaCEP, links automáticos de WhatsApp / CRM);
- Upload de fotos na vistoria de entrada.
