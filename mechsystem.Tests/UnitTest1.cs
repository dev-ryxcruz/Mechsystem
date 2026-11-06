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
        // Transições válidas
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Orcamento, OrdemServicoStatus.EmAndamento));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Orcamento, OrdemServicoStatus.Cancelada));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.EmAndamento, OrdemServicoStatus.Pausada));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Pausada, OrdemServicoStatus.EmAndamento));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.EmAndamento, OrdemServicoStatus.AguardandoPecas));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.AguardandoPecas, OrdemServicoStatus.EmAndamento));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.EmAndamento, OrdemServicoStatus.Concluida));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.EmAndamento, OrdemServicoStatus.Cancelada));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Pausada, OrdemServicoStatus.Cancelada));
        Assert.True(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.AguardandoPecas, OrdemServicoStatus.Cancelada));

        // Transições inválidas bloqueadas
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Orcamento, OrdemServicoStatus.Concluida));
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Orcamento, OrdemServicoStatus.Pausada));
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Pausada, OrdemServicoStatus.Concluida));
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.AguardandoPecas, OrdemServicoStatus.Concluida));
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Concluida, OrdemServicoStatus.Cancelada));
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Concluida, OrdemServicoStatus.EmAndamento));
        Assert.False(OrdemServicoService.PodeTransicionar(OrdemServicoStatus.Cancelada, OrdemServicoStatus.EmAndamento));
    }

    [Fact]
    public async Task FluxoEstoqueOS_AprovacaoAdicaoRemocaoECancelamento_SemBaixaDuplicada()
    {
        var (db, _, _, osService) = CreateSut();
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

        // 1. Adicionar peça em Orçamento NÃO deve baixar estoque
        await osService.AdicionarPecaAsync(os.Id, peca1.Id, 2, null, atendente.Id, PerfilUsuario.Atendimento);
        Assert.Equal(10, (await db.Pecas.FindAsync(peca1.Id))!.EstoqueAtual);

        // 2. Aprovar O.S. deve baixar as 2 unidades de peca1 (10 -> 8)
        await osService.AprovarOSAsync(os.Id, "Cliente Teste", "Presencial", atendente.Id, PerfilUsuario.Atendimento);
        Assert.Equal(OrdemServicoStatus.EmAndamento, (await db.OrdensServico.FindAsync(os.Id))!.Status);
        Assert.Equal(8, (await db.Pecas.FindAsync(peca1.Id))!.EstoqueAtual);

        // 3. Transitar EmAndamento -> AguardandoPecas -> EmAndamento NÃO deve baixar estoque novamente
        await osService.AguardarPecasAsync(os.Id, PerfilUsuario.Mecanico);
        await osService.IniciarOuRetomarServicoAsync(os.Id, PerfilUsuario.Mecanico);
        Assert.Equal(8, (await db.Pecas.FindAsync(peca1.Id))!.EstoqueAtual);

        // 4. Mecânico adicionando peca2 durante EmAndamento deve baixar imediatamente (5 -> 3)
        await osService.AdicionarPecaAsync(os.Id, peca2.Id, 2, null, mecanico.Id, PerfilUsuario.Mecanico);
        Assert.Equal(3, (await db.Pecas.FindAsync(peca2.Id))!.EstoqueAtual);

        // 5. Remover peca1 da O.S. em andamento deve estornar imediatamente (8 -> 10)
        var itemPeca1 = await db.OrdemServicoPecas.FirstAsync(op => op.OrdemServicoId == os.Id && op.PecaId == peca1.Id);
        await osService.RemoverPecaAsync(os.Id, itemPeca1.Id, atendente.Id, PerfilUsuario.Atendimento);
        Assert.Equal(10, (await db.Pecas.FindAsync(peca1.Id))!.EstoqueAtual);

        // 6. Cancelar a O.S. deve estornar apenas peca2 que permaneceu na O.S. (3 -> 5), sem duplicar estorno de peca1
        await osService.CancelarOSAsync(os.Id, "Cliente desistiu do reparo", admin.Id, PerfilUsuario.Administrador);
        Assert.Equal(OrdemServicoStatus.Cancelada, (await db.OrdensServico.FindAsync(os.Id))!.Status);
        Assert.Equal(10, (await db.Pecas.FindAsync(peca1.Id))!.EstoqueAtual);
        Assert.Equal(5, (await db.Pecas.FindAsync(peca2.Id))!.EstoqueAtual);
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

        var emissao = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc);
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
        Assert.Single(contatos);
        Assert.Equal(
            $"Previsão alterada de {emissao.AddDays(4):dd/MM/yyyy} para {novaPrevisao:dd/MM/yyyy} por Atendente Teste",
            contatos[0].Descricao);

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
|A
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
}
