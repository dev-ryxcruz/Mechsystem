using mechsystem.Data;
using mechsystem.Models;
using mechsystem.Repositories;
using mechsystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Testcontainers.PostgreSql;

namespace mechsystem.Tests;

public class OrdemServicoStabilizationTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"mechsystem_test_{Guid.NewGuid():N}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    private static (AppDbContext db, PecaRepository pecaRepo, EstoqueService estoqueService, OrdemServicoService osService) CreateSut()
    {
        var db = CreateInMemoryContext();
        var pecaRepo = new PecaRepository(db);
        var estoqueService = new EstoqueService(db, pecaRepo);
        var osService = new OrdemServicoService(db, estoqueService);
        return (db, pecaRepo, estoqueService, osService);
    }

    private static async Task<(Usuario admin, Usuario atendente, Usuario mecanico, Veiculo veiculo, Peca peca1, Peca peca2)> SeedBasicScenarioAsync(AppDbContext db)
    {
        var admin = new Usuario
        {
            Username = "admin_test",
            NomeCompleto = "Admin Teste",
            PasswordHash = "hash",
            Perfil = PerfilUsuario.Administrador,
            Ativo = true
        };
        var atendente = new Usuario
        {
            Username = "atendente_test",
            NomeCompleto = "Atendente Teste",
            PasswordHash = "hash",
            Perfil = PerfilUsuario.Atendimento,
            Ativo = true
        };
        var mecanico = new Usuario
        {
            Username = "mecanico_test",
            NomeCompleto = "Mecanico Teste",
            PasswordHash = "hash",
            Perfil = PerfilUsuario.Mecanico,
            Ativo = true
        };

        var cliente = new Cliente
        {
            Nome = "Cliente Teste",
            Cpf = "52998224725",
            Telefone = "11999999999"
        };

        db.Usuarios.AddRange(admin, atendente, mecanico);
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var veiculo = new Veiculo
        {
            ClienteId = cliente.Id,
            Placa = "ABC1D23",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2022,
            Quilometragem = 30000
        };

        var peca1 = new Peca
        {
            Sku = "PEC-001",
            Nome = "Filtro de Óleo",
            PrecoCusto = 20m,
            PrecoVenda = 45m,
            EstoqueAtual = 10,
            EstoqueMinimo = 2,
            Ativo = true
        };

        var peca2 = new Peca
        {
            Sku = "PEC-002",
            Nome = "Pastilha de Freio",
            PrecoCusto = 80m,
            PrecoVenda = 160m,
            EstoqueAtual = 5,
            EstoqueMinimo = 2,
            Ativo = true
        };

        db.Veiculos.Add(veiculo);
        db.Pecas.AddRange(peca1, peca2);
        await db.SaveChangesAsync();

        return (admin, atendente, mecanico, veiculo, peca1, peca2);
    }

    [Fact]
    public void TransicoesDeEstado_ValidasEInvalidas_DevemRespeitarMaquinaDeEstados()
    {
        // Transições válidas Fase 2
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Orcamento, OrdemServicoStatus.Aprovada));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Orcamento, OrdemServicoStatus.Recusada));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Orcamento, OrdemServicoStatus.Cancelada));

        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Aprovada, OrdemServicoStatus.EmAndamento));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Aprovada, OrdemServicoStatus.Cancelada));

        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.EmAndamento, OrdemServicoStatus.Pausada));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Pausada, OrdemServicoStatus.EmAndamento));

        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.EmAndamento, OrdemServicoStatus.AguardandoPecas));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.AguardandoPecas, OrdemServicoStatus.EmAndamento));

        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.EmAndamento, OrdemServicoStatus.AguardandoCliente));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.AguardandoCliente, OrdemServicoStatus.EmAndamento));

        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.EmAndamento, OrdemServicoStatus.Concluida));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Concluida, OrdemServicoStatus.Entregue));

        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.EmAndamento, OrdemServicoStatus.Cancelada));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Pausada, OrdemServicoStatus.Cancelada));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.AguardandoPecas, OrdemServicoStatus.Cancelada));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.AguardandoCliente, OrdemServicoStatus.Cancelada));

        // Transições inválidas bloqueadas
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Orcamento, OrdemServicoStatus.EmAndamento));
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Orcamento, OrdemServicoStatus.Concluida));
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Orcamento, OrdemServicoStatus.Entregue));
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Concluida, OrdemServicoStatus.Cancelada));
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Entregue, OrdemServicoStatus.Cancelada));
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Recusada, OrdemServicoStatus.Aprovada));
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Cancelada, OrdemServicoStatus.EmAndamento));
    }

    [Fact]
    public async Task FluxoEstoqueOS_AprovacaoAdicaoRemocaoECancelamento_SemBaixaDuplicada()
    {
        var (db, _, estoqueService, osService) = CreateSut();
        var (admin, atendente, mecanico, veiculo, peca1, peca2) = await SeedBasicScenarioAsync(db);

        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 30000,
            NivelCombustivelEntrada = NivelCombustivel.Meio,
            QueixaCliente = "Revisão geral",
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(3),
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(10),
            Status = OrdemServicoStatus.Orcamento
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        // 1. Adicionar peça em Orçamento NÃO deve alterar estoque nem reserva
        await osService.AdicionarPecaAsync(os.Id, peca1.Id, 2, null, atendente.Id, PerfilUsuario.Atendimento);
        var res1 = await estoqueService.ObterReservaInfoAsync(peca1.Id);
        Assert.Equal(10, res1.EstoqueAtual);
        Assert.Equal(0, res1.QuantidadeReservada);
        Assert.Equal(10, res1.EstoqueDisponivel);

        // 2. Aprovar O.S. move para Aprovada e RESERVA as 2 unidades (físico: 10, reservado: 2, disponível: 8)
        await osService.AprovarOSAsync(os.Id, "Cliente Teste", "Presencial", atendente.Id, PerfilUsuario.Atendimento);
        var osAprovada = await db.OrdensServico.FindAsync(os.Id);
        Assert.Equal(OrdemServicoStatus.Aprovada, osAprovada!.Status);

        var res2 = await estoqueService.ObterReservaInfoAsync(peca1.Id);
        Assert.Equal(10, res2.EstoqueAtual);
        Assert.Equal(2, res2.QuantidadeReservada);
        Assert.Equal(8, res2.EstoqueDisponivel);

        // 3. Primeira execução (iniciar apontamento) move Aprovada -> EmAndamento
        await osService.IniciarApontamentoAsync(os.Id, null, mecanico.Id, mecanico.Id, PerfilUsuario.Mecanico);
        var osExec = await db.OrdensServico.FindAsync(os.Id);
        Assert.Equal(OrdemServicoStatus.EmAndamento, osExec!.Status);

        // 4. Mecânico marca peca1 como aplicada -> BAIXA física do estoque (10 -> 8), reserva zerada (disponível permanece 8)
        var itemPeca1 = await db.OrdemServicoPecas.FirstAsync(op => op.OrdemServicoId == os.Id && op.PecaId == peca1.Id);
        await osService.MarcarPecaAplicadaAsync(itemPeca1.Id, mecanico.Id, PerfilUsuario.Mecanico);
        var res3 = await estoqueService.ObterReservaInfoAsync(peca1.Id);
        Assert.Equal(8, res3.EstoqueAtual);
        Assert.Equal(0, res3.QuantidadeReservada);
        Assert.Equal(8, res3.EstoqueDisponivel);

        // 5. Adicionar peca2 como aditivo durante a execução (físico: 5)
        var aditivoPeca2Id = await osService.AdicionarPecaAditivaAsync(os.Id, peca2.Id, 2, null, OrigemPeca.Estoque, mecanico.Id, PerfilUsuario.Mecanico);
        var osAguardando = await db.OrdensServico.FindAsync(os.Id);
        Assert.Equal(OrdemServicoStatus.AguardandoCliente, osAguardando!.Status);

        // Cliente aprova aditivo -> reserva 2 unidades de peca2 (físico: 5, reservado: 2, disponível: 3)
        await osService.AprovarAditivoAsync(os.Id, null, new[] { aditivoPeca2Id }, "Cliente", TipoContato.WhatsApp, atendente.Id, PerfilUsuario.Atendimento);
        var resPeca2Aprovada = await estoqueService.ObterReservaInfoAsync(peca2.Id);
        Assert.Equal(5, resPeca2Aprovada.EstoqueAtual);
        Assert.Equal(2, resPeca2Aprovada.QuantidadeReservada);
        Assert.Equal(3, resPeca2Aprovada.EstoqueDisponivel);

        // 6. Cancelar a O.S.: estorna peca1 (que foi aplicada: 8 -> 10) e libera reserva de peca2 (não aplicada: 5 mantido, reserva volta a 0)
        await osService.CancelarOSAsync(os.Id, "Cliente desistiu do reparo", admin.Id, PerfilUsuario.Administrador);
        Assert.Equal(OrdemServicoStatus.Cancelada, (await db.OrdensServico.FindAsync(os.Id))!.Status);

        var resPeca1Final = await estoqueService.ObterReservaInfoAsync(peca1.Id);
        Assert.Equal(10, resPeca1Final.EstoqueAtual);
        Assert.Equal(0, resPeca1Final.QuantidadeReservada);

        var resPeca2Final = await estoqueService.ObterReservaInfoAsync(peca2.Id);
        Assert.Equal(5, resPeca2Final.EstoqueAtual);
        Assert.Equal(0, resPeca2Final.QuantidadeReservada);
        Assert.Equal(5, resPeca2Final.EstoqueDisponivel);
    }

    [Fact]
    public async Task EstoqueInsuficienteNaAprovacao_DeveAbortarSemBaixarNenhumaPecaEManterOrcamento()
    {
        var (db, _, _, osService) = CreateSut();
        var (_, atendente, _, veiculo, peca1, peca2) = await SeedBasicScenarioAsync(db);

        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 30000,
            NivelCombustivelEntrada = NivelCombustivel.Reserva,
            QueixaCliente = "Freios e motor",
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(2),
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(10),
            Status = OrdemServicoStatus.Orcamento
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        // peca1 tem 10 em estoque (pede 3 -> ok), peca2 tem 5 em estoque (pede 8 -> insuficiente!)
        await osService.AdicionarPecaAsync(os.Id, peca1.Id, 3, null, atendente.Id, PerfilUsuario.Atendimento);
        await osService.AdicionarPecaAsync(os.Id, peca2.Id, 8, null, atendente.Id, PerfilUsuario.Atendimento);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.AprovarOSAsync(os.Id, "Cliente Teste", "WhatsApp", atendente.Id, PerfilUsuario.Atendimento));

        // Nenhuma peça pode ter sido baixada e a O.S. deve permanecer em Orçamento
        var peca1Atual = await db.Pecas.AsNoTracking().FirstAsync(p => p.Id == peca1.Id);
        var peca2Atual = await db.Pecas.AsNoTracking().FirstAsync(p => p.Id == peca2.Id);
        var osAtual = await db.OrdensServico.AsNoTracking().FirstAsync(o => o.Id == os.Id);

        Assert.Equal(10, peca1Atual.EstoqueAtual);
        Assert.Equal(5, peca2Atual.EstoqueAtual);
        Assert.Equal(OrdemServicoStatus.Orcamento, osAtual.Status);
        Assert.Empty(await db.MovimentacoesEstoque.ToListAsync());
    }

    [Fact]
    public async Task DescontoAbaixoDoPrecoBase_DeveBloquearNaoAdminEPermitirAdmin()
    {
        var (db, _, _, osService) = CreateSut();
        var (admin, atendente, mecanico, veiculo, peca1, _) = await SeedBasicScenarioAsync(db);

        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 30000,
            NivelCombustivelEntrada = NivelCombustivel.UmQuarto,
            QueixaCliente = "Troca de filtro",
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(1),
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(10),
            Status = OrdemServicoStatus.Orcamento
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        // Atendente tentando aplicar desconto (R$ 35 < R$ 45) -> bloqueado no servidor
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.AdicionarPecaAsync(os.Id, peca1.Id, 1, 35m, atendente.Id, PerfilUsuario.Atendimento));

        // Mecânico tentando aplicar desconto -> bloqueado no servidor
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.AdicionarPecaAsync(os.Id, peca1.Id, 1, 35m, mecanico.Id, PerfilUsuario.Mecanico));

        // Mecânico tentando aprovar O.S. -> bloqueado no servidor
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.AprovarOSAsync(os.Id, "Cliente", "Presencial", mecanico.Id, PerfilUsuario.Mecanico));

        // Administrador aplicando desconto (R$ 35 < R$ 45) -> permitido
        await osService.AdicionarPecaAsync(os.Id, peca1.Id, 1, 35m, admin.Id, PerfilUsuario.Administrador);
        var item = await db.OrdemServicoPecas.FirstAsync(op => op.OrdemServicoId == os.Id && op.PecaId == peca1.Id);
        Assert.Equal(35m, item.ValorCobrado);
        Assert.Equal(45m, item.PrecoBase);

        // Atendente ou Mecânico tentando multiplicar item que já possui desconto na O.S. (valorCobrado = null) -> bloqueado
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.AdicionarPecaAsync(os.Id, peca1.Id, 1, null, atendente.Id, PerfilUsuario.Atendimento));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.AdicionarPecaAsync(os.Id, peca1.Id, 1, null, mecanico.Id, PerfilUsuario.Mecanico));

        // Validação de perfil no servidor em ConcluirOSAsync (e demais transições de execução) quando chamado sem perfil/usuário válido
        await osService.AprovarOSAsync(os.Id, "Cliente", "Presencial", admin.Id, PerfilUsuario.Administrador);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.ConcluirOSAsync(os.Id, null, 0));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.PausarServicoAsync(os.Id, null, 0));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.AguardarPecasAsync(os.Id, null, 0));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.IniciarOuRetomarServicoAsync(os.Id, null, 0));
    }

    [Fact]
    public void CalculoTotaisOS_DeveSomarApenasItensNaoRecusadosEDesconto()
    {
        var os = new OrdemServico
        {
            ValorDesconto = 20m
        };

        // Sem itens vinculados: totais das linhas são zero
        Assert.Equal(0m, os.ValorMaoDeObraEfetivo);
        Assert.Equal(0m, os.ValorPecasEfetivo);
        Assert.Equal(-20m, os.ValorTotal);

        // Com serviços e peças vinculados (Pendente e Aprovado entram; Recusado é ignorado)
        os.ServicosAExecutarList.Add(new OrdemServicoServico
        {
            Quantidade = 2,
            PrecoBase = 100m,
            ValorCobrado = 90m,
            TempoEstimadoMinutosSnapshot = 45,
            StatusItem = StatusItem.Aprovado
        });
        os.ServicosAExecutarList.Add(new OrdemServicoServico
        {
            Quantidade = 1,
            PrecoBase = 200m,
            ValorCobrado = 200m,
            TempoEstimadoMinutosSnapshot = 90,
            StatusItem = StatusItem.Recusado
        });

        os.PecasUtilizadas.Add(new OrdemServicoPeca
        {
            Quantidade = 3,
            PrecoBase = 50m,
            ValorCobrado = 40m,
            StatusItem = StatusItem.Pendente
        });
        os.PecasUtilizadas.Add(new OrdemServicoPeca
        {
            Quantidade = 1,
            PrecoBase = 500m,
            ValorCobrado = 500m,
            StatusItem = StatusItem.Recusado
        });

        Assert.Equal(180m, os.ValorMaoDeObraEfetivo); // 2 * 90 (recusado de 200 ignorado)
        Assert.Equal(120m, os.ValorPecasEfetivo);     // 3 * 40 (recusado de 500 ignorado)
        Assert.Equal(280m, os.ValorTotal);            // 180 + 120 - 20
        Assert.Equal(90, os.TempoTotalEstimadoMinutos); // 2 * 45 (recusado de 90 ignorado)
    }

    [Fact]
    public void OrdemServicoStatus_ValoresExistentesPreservadosENovosAdicionadosAposUltimo()
    {
        Assert.Equal(0, (int)OrdemServicoStatus.Orcamento);
        Assert.Equal(1, (int)OrdemServicoStatus.AguardandoPecas);
        Assert.Equal(2, (int)OrdemServicoStatus.EmAndamento);
        Assert.Equal(3, (int)OrdemServicoStatus.Concluida);
        Assert.Equal(4, (int)OrdemServicoStatus.Cancelada);
        Assert.Equal(5, (int)OrdemServicoStatus.Pausada);
        Assert.Equal(6, (int)OrdemServicoStatus.Aprovada);
        Assert.Equal(7, (int)OrdemServicoStatus.AguardandoCliente);
        Assert.Equal(8, (int)OrdemServicoStatus.Entregue);
        Assert.Equal(9, (int)OrdemServicoStatus.Recusada);
    }

    [Fact]
    public async Task ConcluirOS_DevePreencherDataConclusaoEmUtcECalcularMargemNoPeriodo()
    {
        var (db, _, estoqueService, osService) = CreateSut();
        var (admin, _, _, veiculo, peca1, _) = await SeedBasicScenarioAsync(db);

        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 30000,
            NivelCombustivelEntrada = NivelCombustivel.Cheio,
            QueixaCliente = "Revisão concluída",
            DataEmissao = DateTime.UtcNow.AddMonths(-1),
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(1),
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(9),
            Status = OrdemServicoStatus.Orcamento
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        await osService.AdicionarPecaAsync(os.Id, peca1.Id, 2, 45m, admin.Id, PerfilUsuario.Administrador);
        await osService.AprovarOSAsync(os.Id, "Cliente Teste", "Presencial", admin.Id, PerfilUsuario.Administrador);

        // Inicia apontamento (move Aprovada -> EmAndamento) e marca peça aplicada para liberar conclusão
        await osService.IniciarApontamentoAsync(os.Id, null, admin.Id, admin.Id, PerfilUsuario.Administrador);
        var itemPecaConcluir = await db.OrdemServicoPecas.FirstAsync(op => op.OrdemServicoId == os.Id);
        await osService.MarcarPecaAplicadaAsync(itemPecaConcluir.Id, admin.Id, PerfilUsuario.Administrador);

        var antesConclusao = DateTime.UtcNow.AddSeconds(-2);
        await osService.ConcluirOSAsync(os.Id, PerfilUsuario.Mecanico);

        var osConcluida = await db.OrdensServico.FindAsync(os.Id);
        Assert.NotNull(osConcluida);
        Assert.Equal(OrdemServicoStatus.Concluida, osConcluida!.Status);
        Assert.NotNull(osConcluida.DataConclusao);
        Assert.Equal(DateTimeKind.Utc, osConcluida.DataConclusao!.Value.Kind);
        Assert.True(osConcluida.DataConclusao.Value >= antesConclusao);

        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimMes = inicioMes.AddMonths(1);
        var (lucroReal, receitaPecas) = await estoqueService.GetMargemLucroRealAsync(inicioMes, fimMes);

        Assert.Equal(90m, receitaPecas); // 2 * 45
        Assert.Equal(50m, lucroReal);    // 2 * (45 - 20)
    }

    [Fact]
    public async Task CancelarOS_DeveExigirMotivo_ERegistrarAuditoriaDeCancelamento()
    {
        var (db, _, _, osService) = CreateSut();
        var (admin, atendente, mecanico, veiculo, _, _) = await SeedBasicScenarioAsync(db);

        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 30000,
            NivelCombustivelEntrada = NivelCombustivel.Meio,
            QueixaCliente = "Barulho na suspensão",
            DataEmissao = DateTime.UtcNow.Date,
            DataPrevisaoEntrega = DateTime.UtcNow.Date.AddDays(2),
            ValidadeOrcamentoAte = DateTime.UtcNow.Date.AddDays(10),
            Status = OrdemServicoStatus.Orcamento
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.CancelarOSAsync(os.Id, "", atendente.Id, PerfilUsuario.Atendimento));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.CancelarOSAsync(os.Id, "1234", atendente.Id, PerfilUsuario.Atendimento));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.CancelarOSAsync(os.Id, "Motivo válido pelo mecânico", mecanico.Id, PerfilUsuario.Mecanico));

        await osService.CancelarOSAsync(os.Id, "Cliente optou por vender o veículo", admin.Id, PerfilUsuario.Administrador);

        var osCancelada = await db.OrdensServico.FindAsync(os.Id);
        Assert.NotNull(osCancelada);
        Assert.Equal(OrdemServicoStatus.Cancelada, osCancelada!.Status);
        Assert.Equal("Cliente optou por vender o veículo", osCancelada.MotivoCancelamento);
        Assert.NotNull(osCancelada.DataCancelamento);
        Assert.Equal(DateTimeKind.Utc, osCancelada.DataCancelamento!.Value.Kind);
        Assert.Equal(admin.Id, osCancelada.CanceladaPorUsuarioId);
    }

    [Fact]
    public async Task AtualizarOS_RegrasOrcamentoPosAprovacaoDescontoAdminESomenteLeitura()
    {
        var (db, _, _, osService) = CreateSut();
        var (admin, atendente, mecanico, veiculo, _, _) = await SeedBasicScenarioAsync(db);

        var emissao = DateTime.UtcNow.Date;
        var entregaInicial = emissao.AddDays(3);

        var servico = new Servico
        {
            Nome = "Troca de Correia Dentada",
            ValorPadrao = 350m,
            TempoEstimadoMinutos = 120,
            Ativo = true
        };
        db.Servicos.Add(servico);
        await db.SaveChangesAsync();

        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 30000,
            NivelCombustivelEntrada = NivelCombustivel.Meio,
            QueixaCliente = "Queixa inicial",
            Diagnostico = "Avaliação preliminar",
            DataEmissao = emissao,
            DataPrevisaoEntrega = entregaInicial,
            ValidadeOrcamentoAte = emissao.AddDays(10),
            ValorDesconto = 0m,
            Status = OrdemServicoStatus.Orcamento
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        db.OrdemServicoServicos.Add(new OrdemServicoServico
        {
            OrdemServicoId = os.Id,
            ServicoId = servico.Id,
            Quantidade = 1,
            PrecoBase = 350m,
            ValorCobrado = 350m,
            StatusItem = StatusItem.Pendente
        });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.AtualizarOSAsync(os.Id, "Nova queixa", "Novo diag", entregaInicial, 0m, mecanico.Id, PerfilUsuario.Mecanico));

        await osService.AtualizarOSAsync(
            os.Id,
            "Queixa atualizada pelo atendente",
            "Correia dentada ressecada",
            emissao.AddDays(4),
            0m,
            atendente.Id,
            PerfilUsuario.Atendimento);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.AtualizarOSAsync(
                os.Id,
                "Queixa atualizada pelo atendente",
                "Correia dentada ressecada",
                emissao.AddDays(4),
                50m,
                atendente.Id,
                PerfilUsuario.Atendimento));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.AtualizarOSAsync(
                os.Id,
                "Queixa atualizada pelo atendente",
                "Correia dentada ressecada",
                emissao.AddDays(-1),
                50m,
                admin.Id,
                PerfilUsuario.Administrador));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.AtualizarOSAsync(
                os.Id,
                "Queixa atualizada pelo atendente",
                "Correia dentada ressecada",
                emissao.AddDays(4),
                400m,
                admin.Id,
                PerfilUsuario.Administrador));

        await osService.AtualizarOSAsync(
            os.Id,
            "Queixa revisada pelo admin",
            "Troca de correia dentada + tensor",
            emissao.AddDays(4),
            50m,
            admin.Id,
            PerfilUsuario.Administrador);

        var osOrcamento = await db.OrdensServico
            .Include(o => o.ServicosAExecutarList)
            .FirstAsync(o => o.Id == os.Id);
        Assert.Equal("Queixa revisada pelo admin", osOrcamento.QueixaCliente);
        Assert.Equal("Troca de correia dentada + tensor", osOrcamento.Diagnostico);
        Assert.Equal(350m, osOrcamento.ValorMaoDeObraEfetivo);
        Assert.Equal(50m, osOrcamento.ValorDesconto);
        Assert.Equal(300m, osOrcamento.ValorTotal);

        await osService.AprovarOSAsync(os.Id, "Cliente Teste", "Presencial", admin.Id, PerfilUsuario.Administrador);
        var osAprovada = await db.OrdensServico.FindAsync(os.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.AtualizarOSAsync(
                os.Id,
                "Tentativa de mudar queixa após aprovação",
                osAprovada!.Diagnostico,
                osAprovada.DataPrevisaoEntrega,
                osAprovada.ValorDesconto,
                admin.Id,
                PerfilUsuario.Administrador));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.AtualizarOSAsync(
                os.Id,
                osAprovada!.QueixaCliente,
                osAprovada.Diagnostico,
                osAprovada.DataPrevisaoEntrega,
                20m,
                admin.Id,
                PerfilUsuario.Administrador));

        var novaPrevisao = emissao.AddDays(7);
        await osService.AtualizarOSAsync(
            os.Id,
            osAprovada!.QueixaCliente,
            "Diagnóstico refinado na execução",
            novaPrevisao,
            osAprovada.ValorDesconto,
            atendente.Id,
            PerfilUsuario.Atendimento,
            "Atendente Teste");

        var contatos = await db.ContatosOS.Where(c => c.OrdemServicoId == os.Id).ToListAsync();
        Assert.Equal(2, contatos.Count);
        var contatoPrevisao = contatos.First(c => c.Descricao.Contains("Previsão alterada"));
        Assert.Equal(
            $"Previsão alterada de {emissao.AddDays(4):dd/MM/yyyy} para {novaPrevisao:dd/MM/yyyy} por Atendente Teste",
            contatoPrevisao.Descricao);

        // Inicia apontamento e executa serviço para permitir conclusão
        var itemServConcluir = await db.OrdemServicoServicos.FirstAsync(s => s.OrdemServicoId == os.Id);
        await osService.IniciarApontamentoAsync(os.Id, itemServConcluir.Id, mecanico.Id, mecanico.Id, PerfilUsuario.Mecanico);
        await osService.MarcarServicoExecutadoAsync(itemServConcluir.Id, mecanico.Id, mecanico.Id, PerfilUsuario.Mecanico);

        await osService.ConcluirOSAsync(os.Id, PerfilUsuario.Mecanico, mecanico.Id);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.AtualizarOSAsync(
                os.Id,
                osAprovada.QueixaCliente,
                osAprovada.Diagnostico,
                emissao.AddDays(10),
                osAprovada.ValorDesconto,
                admin.Id,
                PerfilUsuario.Administrador));
    }

    private static string GetLocalPostgresAdminConnectionString()
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var secretsPath = Path.Combine(appData, "Microsoft", "UserSecrets", "3d27d95a-033c-4f25-8306-a5f8026853ca", "secrets.json");
            if (File.Exists(secretsPath))
            {
                using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(secretsPath));
                string? rawConn = null;
                if (doc.RootElement.TryGetProperty("ConnectionStrings:DefaultConnection", out var connProp))
                {
                    rawConn = connProp.GetString();
                }
                else if (doc.RootElement.TryGetProperty("ConnectionStrings", out var csObj) &&
                         csObj.ValueKind == System.Text.Json.JsonValueKind.Object &&
                         csObj.TryGetProperty("DefaultConnection", out var nestedProp))
                {
                    rawConn = nestedProp.GetString();
                }

                if (!string.IsNullOrWhiteSpace(rawConn))
                {
                    var builder = new Npgsql.NpgsqlConnectionStringBuilder(rawConn)
                    {
                        Database = "postgres"
                    };
                    return builder.ConnectionString;
                }
            }
        }
        catch
        {
            // Ignora falha ao ler UserSecrets e usa fallback padrão
        }

        return "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=root";
    }

    [Fact]
    public async Task PostgresRelationalTransactionRollback_DeveReverterBaixaParcialNoBancoReal()
    {
        string? connectionString = null;
        PostgreSqlContainer? container = null;
        string? tempDbName = null;
        var localAdminConn = GetLocalPostgresAdminConnectionString();

        try
        {
            // Tenta Testcontainers primeiro (CI / máquina com Docker)
            try
            {
                container = new PostgreSqlBuilder("postgres:16-alpine")
                    .Build();
                await container.StartAsync();
                connectionString = container.GetConnectionString();
            }
            catch
            {
                container = null;
                // Fallback para PostgreSQL local se disponível
                try
                {
                    await using var conn = new Npgsql.NpgsqlConnection(localAdminConn);
                    await conn.OpenAsync();
                    tempDbName = $"mechsystem_test_{Guid.NewGuid():N}";
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"CREATE DATABASE \"{tempDbName}\";";
                    await cmd.ExecuteNonQueryAsync();
                    var builder = new Npgsql.NpgsqlConnectionStringBuilder(localAdminConn)
                    {
                        Database = tempDbName
                    };
                    connectionString = builder.ConnectionString;
                }
                catch
                {
                    // Sem Docker e sem PostgreSQL local acessível
                    return;
                }
            }

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            await using (var db = new AppDbContext(options))
            {
                await db.Database.MigrateAsync();

                var pecaRepo = new PecaRepository(db);
                var estoqueService = new EstoqueService(db, pecaRepo);
                var osService = new OrdemServicoService(db, estoqueService);

                var (admin, _, _, veiculo, peca1, peca2) = await SeedBasicScenarioAsync(db);

                var os = new OrdemServico
                {
                    VeiculoId = veiculo.Id,
                    KmEntrada = 30000,
                    NivelCombustivelEntrada = NivelCombustivel.Meio,
                    QueixaCliente = "Teste transacional PostgreSQL",
                    DataEmissao = DateTime.UtcNow,
                    DataPrevisaoEntrega = DateTime.UtcNow.AddDays(2),
                    ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(10),
                    Status = OrdemServicoStatus.Orcamento
                };
                db.OrdensServico.Add(os);
                await db.SaveChangesAsync();

                await osService.AdicionarPecaAsync(os.Id, peca1.Id, 2, null, admin.Id, PerfilUsuario.Administrador);
                await osService.AdicionarPecaAsync(os.Id, peca2.Id, 99, null, admin.Id, PerfilUsuario.Administrador);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    osService.AprovarOSAsync(os.Id, "Cliente", "Presencial", admin.Id, PerfilUsuario.Administrador));

                Assert.Equal(10, (await db.Pecas.AsNoTracking().FirstAsync(p => p.Id == peca1.Id)).EstoqueAtual);
                Assert.Equal(OrdemServicoStatus.Orcamento, (await db.OrdensServico.AsNoTracking().FirstAsync(o => o.Id == os.Id)).Status);
            }
        }
        finally
        {
            if (container != null)
            {
                await container.DisposeAsync();
            }
            if (tempDbName != null)
            {
                try
                {
                    Npgsql.NpgsqlConnection.ClearAllPools();
                    await using var conn = new Npgsql.NpgsqlConnection(localAdminConn);
                    await conn.OpenAsync();
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"DROP DATABASE IF EXISTS \"{tempDbName}\" WITH (FORCE);";
                    await cmd.ExecuteNonQueryAsync();
                }
                catch
                {
                    // Ignora erro na limpeza do banco temporário
                }
            }
        }
    }

    [Fact]
    public async Task NovasEntidadesFase1_ApontamentoTempoEPagamento_DevemPersistirECalcularTempoReal()
    {
        var (db, _, _, _) = CreateSut();
        var (admin, _, mecanico, veiculo, _, _) = await SeedBasicScenarioAsync(db);

        var servico = new Servico { Nome = "Alinhamento", ValorPadrao = 150m, TempoEstimadoMinutos = 60, Ativo = true };
        db.Servicos.Add(servico);
        await db.SaveChangesAsync();

        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 30000,
            KmSaida = 30025,
            NivelCombustivelEntrada = NivelCombustivel.TresQuartos,
            QueixaCliente = "Direção desalinhada",
            Diagnostico = "Geometria dianteira fora do padrão",
            DataEmissao = DateTime.UtcNow.AddDays(-1),
            DataPrevisaoEntrega = DateTime.UtcNow,
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(9),
            GarantiaDias = 90,
            MecanicoResponsavelId = mecanico.Id,
            AprovadoPorUsuarioId = admin.Id,
            AutorizadoPor = "Cliente Teste",
            TipoContatoAutorizacao = TipoContato.WhatsApp,
            Status = OrdemServicoStatus.Entregue,
            DataConclusao = DateTime.UtcNow.AddHours(-1),
            DataEntrega = DateTime.UtcNow
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        var osServico = new OrdemServicoServico
        {
            OrdemServicoId = os.Id,
            ServicoId = servico.Id,
            Quantidade = 1,
            PrecoBase = 150m,
            ValorCobrado = 150m,
            StatusItem = StatusItem.Aprovado,
            Executado = true,
            MecanicoId = mecanico.Id,
            EhAditivo = false
        };
        db.OrdemServicoServicos.Add(osServico);
        await db.SaveChangesAsync();

        var inicioApontamento = DateTime.UtcNow.AddMinutes(-50);
        var fimApontamento = DateTime.UtcNow.AddMinutes(-5);
        var apontamento = new ApontamentoTempo
        {
            OrdemServicoId = os.Id,
            OrdemServicoServicoId = osServico.Id,
            MecanicoId = mecanico.Id,
            Inicio = inicioApontamento,
            Fim = fimApontamento
        };

        var pagamento = new Pagamento
        {
            OrdemServicoId = os.Id,
            Forma = FormaPagamento.Pix,
            Valor = 150m,
            DataPagamento = DateTime.UtcNow,
            RegistradoPorUsuarioId = admin.Id
        };

        db.ApontamentosTempo.Add(apontamento);
        db.Pagamentos.Add(pagamento);
        await db.SaveChangesAsync();

        var osCompleta = await db.OrdensServico
            .Include(o => o.ApontamentosTempo)
            .Include(o => o.Pagamentos)
            .Include(o => o.ServicosAExecutarList)
            .FirstAsync(o => o.Id == os.Id);

        Assert.Single(osCompleta.ApontamentosTempo);
        Assert.Equal(45, osCompleta.TempoRealTotalMinutos);
        Assert.Single(osCompleta.Pagamentos);
        Assert.Equal(FormaPagamento.Pix, osCompleta.Pagamentos.First().Forma);
        Assert.Equal(150m, osCompleta.Pagamentos.First().Valor);
    }

    [Fact]
    public async Task MigracaoFase1_DeveAplicarEmBancoLimpoEEmBancoComDadosLegadosSemErro()
    {
        string? connLimpo = null;
        string? connLegado = null;
        PostgreSqlContainer? container = null;
        string? tempDbLimpo = null;
        string? tempDbLegado = null;
        var localAdminConn = GetLocalPostgresAdminConnectionString();

        try
        {
            try
            {
                container = new PostgreSqlBuilder("postgres:16-alpine")
                    .Build();
                await container.StartAsync();
                var baseConn = container.GetConnectionString();
                var builder = new Npgsql.NpgsqlConnectionStringBuilder(baseConn);
                await using var adminConn = new Npgsql.NpgsqlConnection(baseConn);
                await adminConn.OpenAsync();

                tempDbLimpo = $"mech_clean_{Guid.NewGuid():N}";
                tempDbLegado = $"mech_legacy_{Guid.NewGuid():N}";
                await using (var cmd = adminConn.CreateCommand())
                {
                    cmd.CommandText = $"CREATE DATABASE \"{tempDbLimpo}\"; CREATE DATABASE \"{tempDbLegado}\";";
                    await cmd.ExecuteNonQueryAsync();
                }

                builder.Database = tempDbLimpo;
                connLimpo = builder.ConnectionString;
                builder.Database = tempDbLegado;
                connLegado = builder.ConnectionString;
            }
            catch
            {
                container = null;
                try
                {
                    await using var conn = new Npgsql.NpgsqlConnection(localAdminConn);
                    await conn.OpenAsync();
                    tempDbLimpo = $"mech_clean_{Guid.NewGuid():N}";
                    tempDbLegado = $"mech_legacy_{Guid.NewGuid():N}";
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"CREATE DATABASE \"{tempDbLimpo}\"; CREATE DATABASE \"{tempDbLegado}\";";
                    await cmd.ExecuteNonQueryAsync();
                    var builderLimpo = new Npgsql.NpgsqlConnectionStringBuilder(localAdminConn) { Database = tempDbLimpo };
                    var builderLegado = new Npgsql.NpgsqlConnectionStringBuilder(localAdminConn) { Database = tempDbLegado };
                    connLimpo = builderLimpo.ConnectionString;
                    connLegado = builderLegado.ConnectionString;
                }
                catch
                {
                    return;
                }
            }

            // 1. Banco limpo + DatabaseSeeder
            var optionsLimpo = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connLimpo)
                .UseSnakeCaseNamingConvention()
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            await using (var dbLimpo = new AppDbContext(optionsLimpo))
            {
                await dbLimpo.Database.MigrateAsync();
                await DatabaseSeeder.SeedAsync(dbLimpo);
                Assert.True(await dbLimpo.OrdensServico.AnyAsync());
            }

            // 2. Banco com dados antigos (migrado até antes da Fase 1, populado via SQL e depois migrado até o fim)
            var optionsLegado = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connLegado)
                .UseSnakeCaseNamingConvention()
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            await using (var dbLegado = new AppDbContext(optionsLegado))
            {
                var migrator = dbLegado.Database.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
                await migrator.MigrateAsync("20261104230924_AddCancelamentoAuditoriaOrdemServico");

                // Inserir dados no formato anterior à Fase 1
                await dbLegado.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO clientes (nome, cpf, telefone, ativo)
                    VALUES ('Cliente Legado', '52998224725', '11999998888', TRUE);

                    INSERT INTO veiculos (placa, marca, modelo, ano, cor, quilometragem, cliente_id, ativo)
                    VALUES ('LEG1A23', 'Fiat', 'Palio', 2018, 'Prata', 82000,
                            (SELECT id FROM clientes WHERE cpf = '52998224725' LIMIT 1), TRUE);

                    -- OS 1: Concluida (3), sem linhas de serviço/peça mas com valor_mao_de_obra=250 e valor_pecas=150, com vistoria (km=85000, combustivel=4)
                    INSERT INTO ordens_servico (
                        veiculo_id, data_emissao, data_previsao_entrega, data_conclusao,
                        valor_mao_de_obra, valor_pecas, valor_desconto, tempo_gasto_minutos,
                        descricao_problema_relatado, servico_a_executar, status,
                        autorizado_por, meio_autorizacao, token_acompanhamento
                    )
                    VALUES (
                        (SELECT id FROM veiculos WHERE placa = 'LEG1A23' LIMIT 1),
                        '2026-01-10 12:00:00+00'::timestamptz,
                        '2026-01-12 12:00:00+00'::timestamptz,
                        '2026-01-11 18:00:00+00'::timestamptz,
                        250.00, 150.00, 20.00, 90,
                        'Barulho no motor legado', 'Serviço manual antigo', 3,
                        'Cliente Legado', 'Telefone', 'tok_legado_1'
                    );

                    INSERT INTO vistorias (
                        ordem_servico_id, status, nivel_combustivel, quilometragem_entrada,
                        tem_estepe, tem_macaco, tem_radio, tem_triangulo, tem_chave_roda, data_vistoria
                    )
                    VALUES (
                        (SELECT id FROM ordens_servico WHERE token_acompanhamento = 'tok_legado_1' LIMIT 1),
                        1, 4, 85000, TRUE, TRUE, TRUE, TRUE, TRUE, '2026-01-10 12:30:00+00'::timestamptz
                    );

                    -- OS 2: EmAndamento (2), sem vistoria (deve puxar km=82000 do veículo), meio_autorizacao='WhatsApp'
                    INSERT INTO ordens_servico (
                        veiculo_id, data_emissao, data_previsao_entrega,
                        valor_mao_de_obra, valor_pecas, valor_desconto, tempo_gasto_minutos,
                        descricao_problema_relatado, status,
                        autorizado_por, meio_autorizacao, token_acompanhamento
                    )
                    VALUES (
                        (SELECT id FROM veiculos WHERE placa = 'LEG1A23' LIMIT 1),
                        '2026-02-01 10:00:00+00'::timestamptz,
                        '2026-02-03 10:00:00+00'::timestamptz,
                        0.00, 0.00, 0.00, 0,
                        'Revisão sem vistoria', 2,
                        'Cliente Legado', 'WhatsApp', 'tok_legado_2'
                    );
                ");

                // Aplicar as migrations da Fase 1 sobre os dados legados
                await dbLegado.Database.MigrateAsync();

                var os1 = await dbLegado.OrdensServico
                    .Include(o => o.ServicosAExecutarList)
                        .ThenInclude(s => s.Servico)
                    .Include(o => o.PecasUtilizadas)
                        .ThenInclude(p => p.Peca)
                    .FirstAsync(o => o.TokenAcompanhamento == "tok_legado_1");

                // 1) QueixaCliente migrada de DescricaoProblemaRelatado
                Assert.Equal("Barulho no motor legado", os1.QueixaCliente);

                // 2) Concluida -> Entregue com DataEntrega = DataConclusao
                Assert.Equal(OrdemServicoStatus.Entregue, os1.Status);
                Assert.Equal(os1.DataConclusao, os1.DataEntrega);

                // 3) MeioAutorizacao 'Telefone' -> TipoContato.Ligacao
                Assert.Equal(TipoContato.Ligacao, os1.TipoContatoAutorizacao);

                // 4) ValidadeOrcamentoAte = DataEmissao + 10 dias
                Assert.Equal(os1.DataEmissao.AddDays(10), os1.ValidadeOrcamentoAte);

                // 5) KmEntrada e NivelCombustivelEntrada vindos da Vistoria
                Assert.Equal(85000, os1.KmEntrada);
                Assert.Equal(NivelCombustivel.TresQuartos, os1.NivelCombustivelEntrada);

                // 6) Linhas genéricas criadas para preservar ValorMaoDeObra (250) e ValorPecas (150)
                Assert.Single(os1.ServicosAExecutarList);
                Assert.Equal("Mão de obra (migrado)", os1.ServicosAExecutarList.First().Servico!.Nome);
                Assert.Equal(250m, os1.ValorMaoDeObraEfetivo);

                Assert.Single(os1.PecasUtilizadas);
                Assert.Equal("Peças diversas (migrado)", os1.PecasUtilizadas.First().Peca!.Nome);
                Assert.Equal(150m, os1.ValorPecasEfetivo);
                Assert.Equal(380m, os1.ValorTotal); // 250 + 150 - 20

                var os2 = await dbLegado.OrdensServico
                    .FirstAsync(o => o.TokenAcompanhamento == "tok_legado_2");

                // Sem vistoria -> KmEntrada puxado de Veiculo.Quilometragem (82000) e MeioAutorizacao 'WhatsApp' -> TipoContato.WhatsApp
                Assert.Equal(82000, os2.KmEntrada);
                Assert.Equal(TipoContato.WhatsApp, os2.TipoContatoAutorizacao);
            }
        }
        finally
        {
            if (container != null)
            {
                await container.DisposeAsync();
            }
            else if (tempDbLimpo != null || tempDbLegado != null)
            {
                try
                {
                    Npgsql.NpgsqlConnection.ClearAllPools();
                    await using var conn = new Npgsql.NpgsqlConnection(localAdminConn);
                    await conn.OpenAsync();
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = $@"
                        DROP DATABASE IF EXISTS ""{tempDbLimpo}"" WITH (FORCE);
                        DROP DATABASE IF EXISTS ""{tempDbLegado}"" WITH (FORCE);";
                    await cmd.ExecuteNonQueryAsync();
                }
                catch
                {
                    // Ignora erro na limpeza
                }
            }
        }
    }

    [Fact]
    public async Task OrcamentoVencido_DeveBloquearAprovacao_EPermitirApenasRenovacaoPorAdmin()
    {
        var (db, _, _, osService) = CreateSut();
        var (admin, atendente, mecanico, veiculo, _, _) = await SeedBasicScenarioAsync(db);

        var servico = new Servico { Nome = "Troca de Óleo", ValorPadrao = 100m, TempoEstimadoMinutos = 30, Ativo = true };
        db.Servicos.Add(servico);
        await db.SaveChangesAsync();

        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 45000,
            NivelCombustivelEntrada = NivelCombustivel.Meio,
            QueixaCliente = "Revisão periódica",
            DataEmissao = DateTime.UtcNow.AddDays(-20),
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(2),
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(-5), // Vencido há 5 dias!
            Status = OrdemServicoStatus.Orcamento
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        db.OrdemServicoServicos.Add(new OrdemServicoServico
        {
            OrdemServicoId = os.Id,
            ServicoId = servico.Id,
            Quantidade = 1,
            PrecoBase = 100m,
            ValorCobrado = 100m,
            StatusItem = StatusItem.Pendente
        });
        await db.SaveChangesAsync();

        // Tentativa de aprovação bloqueada com mensagem clara de orçamento vencido
        var exAprov = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.AprovarOSAsync(os.Id, "Cliente Teste", "Presencial", atendente.Id, PerfilUsuario.Atendimento));
        Assert.Contains("vencido", exAprov.Message, StringComparison.OrdinalIgnoreCase);

        // Atendente e Mecânico não podem renovar validade
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.RenovarValidadeOrcamentoAsync(os.Id, DateTime.UtcNow.AddDays(10), atendente.Id, PerfilUsuario.Atendimento));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.RenovarValidadeOrcamentoAsync(os.Id, DateTime.UtcNow.AddDays(10), mecanico.Id, PerfilUsuario.Mecanico));

        // Administrador renova a validade
        var novaValidade = DateTime.UtcNow.AddDays(10);
        await osService.RenovarValidadeOrcamentoAsync(os.Id, novaValidade, admin.Id, PerfilUsuario.Administrador);

        var osRenovada = await db.OrdensServico.FindAsync(os.Id);
        Assert.Equal(novaValidade.Date, osRenovada!.ValidadeOrcamentoAte.Date);

        var contatoRenovacao = await db.ContatosOS.FirstOrDefaultAsync(c => c.OrdemServicoId == os.Id);
        Assert.NotNull(contatoRenovacao);
        Assert.Contains("renovada", contatoRenovacao.Descricao, StringComparison.OrdinalIgnoreCase);

        // Agora aprovação pelo Atendente tem sucesso
        await osService.AprovarOSAsync(os.Id, "Cliente Teste", "WhatsApp", atendente.Id, PerfilUsuario.Atendimento);
        var osAprovada = await db.OrdensServico.FindAsync(os.Id);
        Assert.Equal(OrdemServicoStatus.Aprovada, osAprovada!.Status);
    }

    [Fact]
    public async Task AprovacaoParcial_DeveAprovarItensSelecionadosERecusarDemais_ERequisitarAoMenosUmItem()
    {
        var (db, _, _, osService) = CreateSut();
        var (admin, atendente, _, veiculo, peca1, peca2) = await SeedBasicScenarioAsync(db);

        var serv1 = new Servico { Nome = "Alinhamento", ValorPadrao = 120m, TempoEstimadoMinutos = 40, Ativo = true };
        var serv2 = new Servico { Nome = "Balanceamento", ValorPadrao = 80m, TempoEstimadoMinutos = 30, Ativo = true };
        db.Servicos.AddRange(serv1, serv2);
        await db.SaveChangesAsync();

        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 30000,
            NivelCombustivelEntrada = NivelCombustivel.TresQuartos,
            QueixaCliente = "Carro puxando",
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(1),
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(10),
            Status = OrdemServicoStatus.Orcamento
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        var itemServ1 = new OrdemServicoServico { OrdemServicoId = os.Id, ServicoId = serv1.Id, Quantidade = 1, PrecoBase = 120m, ValorCobrado = 120m, StatusItem = StatusItem.Pendente };
        var itemServ2 = new OrdemServicoServico { OrdemServicoId = os.Id, ServicoId = serv2.Id, Quantidade = 1, PrecoBase = 80m, ValorCobrado = 80m, StatusItem = StatusItem.Pendente };
        db.OrdemServicoServicos.AddRange(itemServ1, itemServ2);

        var itemPeca1 = new OrdemServicoPeca { OrdemServicoId = os.Id, PecaId = peca1.Id, Quantidade = 1, PrecoBase = 45m, ValorCobrado = 45m, Origem = OrigemPeca.Estoque, StatusItem = StatusItem.Pendente };
        var itemPeca2 = new OrdemServicoPeca { OrdemServicoId = os.Id, PecaId = peca2.Id, Quantidade = 1, PrecoBase = 90m, ValorCobrado = 90m, Origem = OrigemPeca.Estoque, StatusItem = StatusItem.Pendente };
        db.OrdemServicoPecas.AddRange(itemPeca1, itemPeca2);
        await db.SaveChangesAsync();

        // Tentativa de aprovar sem nenhum item selecionado -> erro
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.AprovarOSAsync(os.Id, Array.Empty<int>(), Array.Empty<int>(), "Cliente", TipoContato.Presencial, atendente.Id, PerfilUsuario.Atendimento));

        // Aprovação parcial: aprova apenas itemServ1 e itemPeca1; rejeita itemServ2 e itemPeca2
        await osService.AprovarOSAsync(os.Id, new[] { itemServ1.Id }, new[] { itemPeca1.Id }, "Cliente", TipoContato.Presencial, atendente.Id, PerfilUsuario.Atendimento);

        var osAprovada = await db.OrdensServico
            .Include(o => o.ServicosAExecutarList)
            .Include(o => o.PecasUtilizadas)
            .FirstAsync(o => o.Id == os.Id);

        Assert.Equal(OrdemServicoStatus.Aprovada, osAprovada.Status);
        Assert.Equal(StatusItem.Aprovado, osAprovada.ServicosAExecutarList.First(s => s.Id == itemServ1.Id).StatusItem);
        Assert.Equal(StatusItem.Recusado, osAprovada.ServicosAExecutarList.First(s => s.Id == itemServ2.Id).StatusItem);
        Assert.Equal(StatusItem.Aprovado, osAprovada.PecasUtilizadas.First(p => p.Id == itemPeca1.Id).StatusItem);
        Assert.Equal(StatusItem.Recusado, osAprovada.PecasUtilizadas.First(p => p.Id == itemPeca2.Id).StatusItem);

        // ValorTotal após aprovação computa apenas itens aprovados (120 + 45 = 165)
        Assert.Equal(120m, osAprovada.ValorMaoDeObraEfetivo);
        Assert.Equal(45m, osAprovada.ValorPecasEfetivo);
        Assert.Equal(165m, osAprovada.ValorTotal);
    }

    [Fact]
    public async Task AditivoPendente_DeveBloquearConclusao_EMoverOSParaAguardandoCliente()
    {
        var (db, _, _, osService) = CreateSut();
        var (admin, atendente, mecanico, veiculo, _, _) = await SeedBasicScenarioAsync(db);

        var servPrincipal = new Servico { Nome = "Revisão", ValorPadrao = 200m, TempoEstimadoMinutos = 60, Ativo = true };
        var servAdicional = new Servico { Nome = "Higienização Ar", ValorPadrao = 80m, TempoEstimadoMinutos = 30, Ativo = true };
        db.Servicos.AddRange(servPrincipal, servAdicional);
        await db.SaveChangesAsync();

        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 50000,
            NivelCombustivelEntrada = NivelCombustivel.Meio,
            QueixaCliente = "Revisão",
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(2),
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(10),
            Status = OrdemServicoStatus.Orcamento
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        var itemServ1 = new OrdemServicoServico { OrdemServicoId = os.Id, ServicoId = servPrincipal.Id, Quantidade = 1, PrecoBase = 200m, ValorCobrado = 200m, StatusItem = StatusItem.Pendente };
        db.OrdemServicoServicos.Add(itemServ1);
        await db.SaveChangesAsync();

        await osService.AprovarOSAsync(os.Id, "Cliente", "Presencial", atendente.Id, PerfilUsuario.Atendimento);
        await osService.IniciarApontamentoAsync(os.Id, itemServ1.Id, mecanico.Id, mecanico.Id, PerfilUsuario.Mecanico);

        // Mecânico propõe aditivo de serviço durante a execução -> O.S. passa para AguardandoCliente
        var aditivoServId = await osService.AdicionarServicoAditivoAsync(os.Id, servAdicional.Id, 1, 80m, mecanico.Id, PerfilUsuario.Mecanico);
        var osEmEspera = await db.OrdensServico.FindAsync(os.Id);
        Assert.Equal(OrdemServicoStatus.AguardandoCliente, osEmEspera!.Status);

        // Enquanto o aditivo estiver pendente, tentar concluir a O.S. deve falhar
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.ConcluirOSAsync(os.Id, PerfilUsuario.Mecanico, mecanico.Id));

        // Mecânico tenta aprovar aditivo -> bloqueado (só Admin/Atendimento)
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.AprovarAditivoAsync(os.Id, new[] { aditivoServId }, null, "Cliente", TipoContato.WhatsApp, mecanico.Id, PerfilUsuario.Mecanico));

        // Atendente aprova aditivo -> O.S. volta para EmAndamento
        await osService.AprovarAditivoAsync(os.Id, new[] { aditivoServId }, null, "Cliente", TipoContato.WhatsApp, atendente.Id, PerfilUsuario.Atendimento);
        var osRetomada = await db.OrdensServico.FindAsync(os.Id);
        Assert.Equal(OrdemServicoStatus.EmAndamento, osRetomada!.Status);

        // Concluir ainda falha se os serviços não estiverem executados (sem justificativa)
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.ConcluirOSAsync(os.Id, PerfilUsuario.Mecanico, mecanico.Id));

        // Marca ambos os serviços como executados
        await osService.MarcarServicoExecutadoAsync(itemServ1.Id, mecanico.Id, mecanico.Id, PerfilUsuario.Mecanico);
        await osService.MarcarServicoExecutadoAsync(aditivoServId, mecanico.Id, mecanico.Id, PerfilUsuario.Mecanico);

        // Agora conclui com sucesso persistindo o ChecklistConclusaoJson
        var checklistJson = "{\"FluidosOk\":true,\"LimpezaOk\":true,\"FerramentasRecolhidas\":true}";
        await osService.ConcluirOSAsync(os.Id, checklistJson, null, mecanico.Id, PerfilUsuario.Mecanico);

        var osConcluida = await db.OrdensServico.FindAsync(os.Id);
        Assert.Equal(OrdemServicoStatus.Concluida, osConcluida!.Status);
        Assert.Equal(checklistJson, osConcluida.ChecklistConclusaoJson);
    }

    [Fact]
    public async Task ApontamentoDuplicado_DeveBloquearSegundoApontamentoAbertoParaOMesmoMecanico()
    {
        var (db, _, _, osService) = CreateSut();
        var (_, atendente, mecanico, veiculo, _, _) = await SeedBasicScenarioAsync(db);

        var os1 = new OrdemServico { VeiculoId = veiculo.Id, KmEntrada = 10000, NivelCombustivelEntrada = NivelCombustivel.Cheio, QueixaCliente = "OS 1", DataEmissao = DateTime.UtcNow, DataPrevisaoEntrega = DateTime.UtcNow.AddDays(1), ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(5), Status = OrdemServicoStatus.Orcamento };
        var os2 = new OrdemServico { VeiculoId = veiculo.Id, KmEntrada = 10000, NivelCombustivelEntrada = NivelCombustivel.Cheio, QueixaCliente = "OS 2", DataEmissao = DateTime.UtcNow, DataPrevisaoEntrega = DateTime.UtcNow.AddDays(1), ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(5), Status = OrdemServicoStatus.Orcamento };
        db.OrdensServico.AddRange(os1, os2);
        await db.SaveChangesAsync();

        var servico = new Servico { Nome = "Diagnóstico", ValorPadrao = 50m, TempoEstimadoMinutos = 30, Ativo = true };
        db.Servicos.Add(servico);
        await db.SaveChangesAsync();

        db.OrdemServicoServicos.Add(new OrdemServicoServico { OrdemServicoId = os1.Id, ServicoId = servico.Id, Quantidade = 1, PrecoBase = 50m, ValorCobrado = 50m, StatusItem = StatusItem.Aprovado });
        db.OrdemServicoServicos.Add(new OrdemServicoServico { OrdemServicoId = os2.Id, ServicoId = servico.Id, Quantidade = 1, PrecoBase = 50m, ValorCobrado = 50m, StatusItem = StatusItem.Aprovado });
        await db.SaveChangesAsync();

        await osService.AprovarOSAsync(os1.Id, "Cliente", "Presencial", atendente.Id, PerfilUsuario.Atendimento);
        await osService.AprovarOSAsync(os2.Id, "Cliente", "Presencial", atendente.Id, PerfilUsuario.Atendimento);

        // Mecânico inicia apontamento na OS 1
        var ap1Id = await osService.IniciarApontamentoAsync(os1.Id, null, mecanico.Id, mecanico.Id, PerfilUsuario.Mecanico);
        Assert.True(ap1Id > 0);

        // Tentativa de abrir apontamento na OS 2 com o mesmo mecânico -> bloqueado!
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.IniciarApontamentoAsync(os2.Id, null, mecanico.Id, mecanico.Id, PerfilUsuario.Mecanico));
        Assert.Contains("já possui um apontamento", ex.Message, StringComparison.OrdinalIgnoreCase);

        // Pausa apontamento na OS 1
        await osService.PausarApontamentoAsync(os1.Id, ap1Id, mecanico.Id, mecanico.Id, PerfilUsuario.Mecanico);

        // Agora consegue iniciar na OS 2
        var ap2Id = await osService.IniciarApontamentoAsync(os2.Id, null, mecanico.Id, mecanico.Id, PerfilUsuario.Mecanico);
        Assert.True(ap2Id > 0);
    }

    [Fact]
    public async Task EntregaOS_ValidacoesKmSaidaEPagamentoERbac()
    {
        var (db, _, _, osService) = CreateSut();
        var (admin, atendente, mecanico, veiculo, _, _) = await SeedBasicScenarioAsync(db);

        var servico = new Servico { Nome = "Manutenção", ValorPadrao = 500m, TempoEstimadoMinutos = 120, Ativo = true };
        db.Servicos.Add(servico);
        await db.SaveChangesAsync();

        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 60000,
            NivelCombustivelEntrada = NivelCombustivel.Meio,
            QueixaCliente = "Revisão geral",
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(1),
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(5),
            Status = OrdemServicoStatus.Orcamento
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        var itemServ = new OrdemServicoServico { OrdemServicoId = os.Id, ServicoId = servico.Id, Quantidade = 1, PrecoBase = 500m, ValorCobrado = 500m, StatusItem = StatusItem.Aprovado };
        db.OrdemServicoServicos.Add(itemServ);
        await db.SaveChangesAsync();

        await osService.AprovarOSAsync(os.Id, "Cliente", "Presencial", atendente.Id, PerfilUsuario.Atendimento);
        await osService.IniciarApontamentoAsync(os.Id, itemServ.Id, mecanico.Id, mecanico.Id, PerfilUsuario.Mecanico);
        await osService.MarcarServicoExecutadoAsync(itemServ.Id, mecanico.Id, mecanico.Id, PerfilUsuario.Mecanico);
        await osService.ConcluirOSAsync(os.Id, PerfilUsuario.Mecanico, mecanico.Id);

        // 1. Mecânico não pode entregar veículo
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.EntregarOSAsync(os.Id, 60010, Array.Empty<Pagamento>(), mecanico.Id, PerfilUsuario.Mecanico));

        // 2. KmSaida menor que KmEntrada -> bloqueado
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.EntregarOSAsync(os.Id, 59990, Array.Empty<Pagamento>(), atendente.Id, PerfilUsuario.Atendimento));

        // 3. Pagamento insuficiente tentado por Atendente (ex: pagou R$ 300 de R$ 500) -> bloqueado
        var pagParcial = new List<Pagamento> { new() { Forma = FormaPagamento.Pix, Valor = 300m } };
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.EntregarOSAsync(os.Id, 60010, pagParcial, atendente.Id, PerfilUsuario.Atendimento));

        // 4. Administrador autoriza entrega com saldo em aberto com observação
        await osService.EntregarOSAsync(os.Id, 60010, pagParcial, admin.Id, PerfilUsuario.Administrador, "Autorizado pelo Diretor com promissória para 10 dias.");

        var osEntregue = await db.OrdensServico.Include(o => o.Veiculo).Include(o => o.Pagamentos).FirstAsync(o => o.Id == os.Id);
        Assert.Equal(OrdemServicoStatus.Entregue, osEntregue.Status);
        Assert.Equal(60010, osEntregue.KmSaida);
        Assert.Equal(60010, osEntregue.Veiculo!.Quilometragem);
        Assert.NotNull(osEntregue.DataEntrega);
        Assert.NotNull(osEntregue.GarantiaDias);
        Assert.Single(osEntregue.Pagamentos);
        Assert.Equal(300m, osEntregue.Pagamentos.First().Valor);
    }

    [Fact]
    public async Task AtribuirMecanico_DeveRespeitarRegrasDeResponsavel()
    {
        var (db, _, _, osService) = CreateSut();
        var (admin, atendente, mecanico, veiculo, _, _) = await SeedBasicScenarioAsync(db);

        var outroMecanico = new Usuario { Username = "mecanico2", NomeCompleto = "Mecânico 2", Perfil = PerfilUsuario.Mecanico, PasswordHash = "hash" };
        db.Usuarios.Add(outroMecanico);
        await db.SaveChangesAsync();

        var os = new OrdemServico { VeiculoId = veiculo.Id, KmEntrada = 10000, NivelCombustivelEntrada = NivelCombustivel.Meio, QueixaCliente = "Queixa", DataEmissao = DateTime.UtcNow, DataPrevisaoEntrega = DateTime.UtcNow.AddDays(2), ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(5), Status = OrdemServicoStatus.Orcamento };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        // Mecânico assume O.S. sem dono para si mesmo -> permitido
        await osService.AtribuirMecanicoAsync(os.Id, mecanico.Id, mecanico.Id, PerfilUsuario.Mecanico);
        var osAssumida = await db.OrdensServico.FindAsync(os.Id);
        Assert.Equal(mecanico.Id, osAssumida!.MecanicoResponsavelId);

        // Outro mecânico tentando assumir uma O.S. que já tem dono -> bloqueado
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.AtribuirMecanicoAsync(os.Id, outroMecanico.Id, outroMecanico.Id, PerfilUsuario.Mecanico));

        // Atendente ou Administrador podem reatribuir
        await osService.AtribuirMecanicoAsync(os.Id, outroMecanico.Id, atendente.Id, PerfilUsuario.Atendimento);
        var osReatribuida = await db.OrdensServico.FindAsync(os.Id);
        Assert.Equal(outroMecanico.Id, osReatribuida!.MecanicoResponsavelId);
    }

    [Fact]
    public async Task RecusarOrcamento_DeveGravarMotivoEStatusRecusada()
    {
        var (db, _, _, osService) = CreateSut();
        var (admin, atendente, mecanico, veiculo, _, _) = await SeedBasicScenarioAsync(db);

        var os = new OrdemServico { VeiculoId = veiculo.Id, KmEntrada = 10000, NivelCombustivelEntrada = NivelCombustivel.Meio, QueixaCliente = "Queixa", DataEmissao = DateTime.UtcNow, DataPrevisaoEntrega = DateTime.UtcNow.AddDays(2), ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(5), Status = OrdemServicoStatus.Orcamento };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        // Mecânico não pode recusar orçamento
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            osService.RecusarOrcamentoAsync(os.Id, "Cliente achou caro", mecanico.Id, PerfilUsuario.Mecanico));

        // Motivo menor que 5 caracteres -> erro
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            osService.RecusarOrcamentoAsync(os.Id, "Caro", atendente.Id, PerfilUsuario.Atendimento));

        // Atendente recusa com motivo válido
        await osService.RecusarOrcamentoAsync(os.Id, "Cliente achou o valor muito alto e decidiu adiar", atendente.Id, PerfilUsuario.Atendimento);

        var osRecusada = await db.OrdensServico.FindAsync(os.Id);
        Assert.Equal(OrdemServicoStatus.Recusada, osRecusada!.Status);
        Assert.Equal("Cliente achou o valor muito alto e decidiu adiar", osRecusada.MotivoCancelamento);
    }

    [Fact]
    public void PrevisaoEntregaSugerida_DevePularFinaisDeSemanaECalcularCorretamente()
    {
        // Sexta-feira: 2026-11-06
        var sexta = new DateTime(2026, 11, 6);

        // 120 minutos (1 dia útil): pula sábado e domingo e sugere segunda-feira (2026-11-09)
        var sug1 = OrdemServicoService.CalcularPrevisaoEntregaSugerida(120, sexta);
        Assert.Equal(DayOfWeek.Monday, sug1.DayOfWeek);
        Assert.Equal(new DateTime(2026, 11, 9), sug1.Date);

        // 960 minutos (2 dias úteis a 480 min/dia): pula sábado e domingo e sugere terça-feira (2026-11-10)
        var sug2 = OrdemServicoService.CalcularPrevisaoEntregaSugerida(960, sexta);
        Assert.Equal(DayOfWeek.Tuesday, sug2.DayOfWeek);
        Assert.Equal(new DateTime(2026, 11, 10), sug2.Date);
    }
}
