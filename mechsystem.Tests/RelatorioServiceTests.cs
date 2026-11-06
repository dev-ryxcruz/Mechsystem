using System.Globalization;
using mechsystem.Data;
using mechsystem.Models;
using mechsystem.Repositories;
using mechsystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace mechsystem.Tests;

public class RelatorioServiceTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"mechsystem_relatorio_test_{Guid.NewGuid():N}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task ObterMetricasConcluidasAsync_DeveProduzirResultadoIdenticoAoCalculoEmMemoria_SomandoApenasItensNaoRecusados()
    {
        using var db = CreateInMemoryContext();
        var pecaRepo = new PecaRepository(db);
        var estoqueService = new EstoqueService(db, pecaRepo);
        var relatorioService = new RelatorioService(db, estoqueService);

        var cliente = new Cliente { Nome = "Cliente Relatório", Cpf = "52998224725" };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var veiculo = new Veiculo { ClienteId = cliente.Id, Placa = "REL1A23", Marca = "Toyota", Modelo = "Hilux", Ano = 2022 };
        var servico1 = new Servico { Nome = "Revisão Completa", ValorPadrao = 300m, TempoEstimadoMinutos = 120 };
        var peca1 = new Peca { Sku = "REL-001", Nome = "Kit Filtros", PrecoCusto = 90m, PrecoVenda = 180m, EstoqueAtual = 10, EstoqueMinimo = 2 };
        db.Veiculos.Add(veiculo);
        db.Servicos.Add(servico1);
        db.Pecas.Add(peca1);
        await db.SaveChangesAsync();

        var inicioPeriodo = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimPeriodo = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        // OS 1: Com itens aprovados + desconto
        var os1 = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 40000,
            NivelCombustivelEntrada = NivelCombustivel.Meio,
            QueixaCliente = "OS 1 aprovada",
            DataEmissao = inicioPeriodo.AddDays(1),
            DataPrevisaoEntrega = inicioPeriodo.AddDays(3),
            ValidadeOrcamentoAte = inicioPeriodo.AddDays(11),
            DataConclusao = inicioPeriodo.AddDays(2),
            ValorDesconto = 30m,
            Status = OrdemServicoStatus.Concluida
        };

        // OS 2: Com itens aprovados e itens recusados (recusados não entram no total) + desconto
        var os2 = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 42000,
            NivelCombustivelEntrada = NivelCombustivel.Cheio,
            QueixaCliente = "OS 2 itens vinculados",
            DataEmissao = inicioPeriodo.AddDays(4),
            DataPrevisaoEntrega = inicioPeriodo.AddDays(6),
            ValidadeOrcamentoAte = inicioPeriodo.AddDays(14),
            DataConclusao = inicioPeriodo.AddDays(5),
            ValorDesconto = 40m,
            Status = OrdemServicoStatus.Concluida
        };

        // OS 3: Entregue no período
        var os3 = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 45000,
            KmSaida = 45050,
            NivelCombustivelEntrada = NivelCombustivel.UmQuarto,
            QueixaCliente = "OS 3 entregue",
            DataEmissao = inicioPeriodo.AddDays(7),
            DataPrevisaoEntrega = inicioPeriodo.AddDays(9),
            ValidadeOrcamentoAte = inicioPeriodo.AddDays(17),
            DataConclusao = inicioPeriodo.AddDays(8),
            DataEntrega = inicioPeriodo.AddDays(8),
            ValorDesconto = 0m,
            Status = OrdemServicoStatus.Entregue
        };

        // OS 4: Concluída fora do período (não deve entrar)
        var osForaPeriodo = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 35000,
            NivelCombustivelEntrada = NivelCombustivel.Reserva,
            QueixaCliente = "OS fora do período",
            DataEmissao = inicioPeriodo.AddMonths(-2),
            DataPrevisaoEntrega = inicioPeriodo.AddMonths(-2).AddDays(2),
            ValidadeOrcamentoAte = inicioPeriodo.AddMonths(-2).AddDays(10),
            DataConclusao = inicioPeriodo.AddMonths(-1),
            Status = OrdemServicoStatus.Concluida
        };

        db.OrdensServico.AddRange(os1, os2, os3, osForaPeriodo);
        await db.SaveChangesAsync();

        db.OrdemServicoServicos.AddRange(
            new OrdemServicoServico
            {
                OrdemServicoId = os1.Id,
                ServicoId = servico1.Id,
                Quantidade = 1,
                PrecoBase = 250m,
                ValorCobrado = 250m, // Subtotal = 250
                StatusItem = StatusItem.Aprovado
            },
            new OrdemServicoServico
            {
                OrdemServicoId = os2.Id,
                ServicoId = servico1.Id,
                Quantidade = 2,
                PrecoBase = 300m,
                ValorCobrado = 280m, // Subtotal = 560
                StatusItem = StatusItem.Aprovado
            },
            new OrdemServicoServico
            {
                OrdemServicoId = os2.Id,
                ServicoId = servico1.Id,
                Quantidade = 1,
                PrecoBase = 300m,
                ValorCobrado = 300m, // Recusado -> ignorado!
                StatusItem = StatusItem.Recusado
            },
            new OrdemServicoServico
            {
                OrdemServicoId = os3.Id,
                ServicoId = servico1.Id,
                Quantidade = 1,
                PrecoBase = 180m,
                ValorCobrado = 180m, // Subtotal = 180
                StatusItem = StatusItem.Aprovado
            });

        db.OrdemServicoPecas.AddRange(
            new OrdemServicoPeca
            {
                OrdemServicoId = os1.Id,
                PecaId = peca1.Id,
                Quantidade = 1,
                PrecoBase = 120m,
                ValorCobrado = 120m, // Subtotal = 120, Lucro = 30
                PrecoCustoSnapshot = 90m,
                StatusItem = StatusItem.Aprovado
            },
            new OrdemServicoPeca
            {
                OrdemServicoId = os2.Id,
                PecaId = peca1.Id,
                Quantidade = 2,
                PrecoBase = 180m,
                ValorCobrado = 170m, // Subtotal = 340, Lucro = 2 * (170 - 90) = 160
                PrecoCustoSnapshot = 90m,
                StatusItem = StatusItem.Aprovado
            },
            new OrdemServicoPeca
            {
                OrdemServicoId = os2.Id,
                PecaId = peca1.Id,
                Quantidade = 5,
                PrecoBase = 180m,
                ValorCobrado = 180m, // Recusado -> ignorado!
                PrecoCustoSnapshot = 90m,
                StatusItem = StatusItem.Recusado
            },
            new OrdemServicoPeca
            {
                OrdemServicoId = os3.Id,
                PecaId = peca1.Id,
                Quantidade = 1,
                PrecoBase = 180m,
                ValorCobrado = 180m, // Subtotal = 180, Lucro = 1 * (180 - 90) = 90
                PrecoCustoSnapshot = 90m,
                StatusItem = StatusItem.Aprovado
            });
        await db.SaveChangesAsync();

        // 1. Cálculo via RelatorioService (agregação no banco)
        var resultadoBanco = await relatorioService.ObterMetricasConcluidasAsync(inicioPeriodo, fimPeriodo);

        // 2. Cálculo em memória carregando entidades completas
        var ordensEmMemoria = await db.OrdensServico
            .Include(o => o.PecasUtilizadas)
            .Include(o => o.ServicosAExecutarList)
            .Where(o => (o.Status == OrdemServicoStatus.Concluida || o.Status == OrdemServicoStatus.Entregue)
                        && (o.DataConclusao ?? o.DataEntrega) >= inicioPeriodo
                        && (o.DataConclusao ?? o.DataEntrega) < fimPeriodo)
            .ToListAsync();
        var resultadoMemoria = RelatorioService.CalcularMetricasConcluidasEmMemoria(ordensEmMemoria);

        Assert.Equal(3, resultadoBanco.TotalConcluidas);
        Assert.Equal(resultadoMemoria.TotalConcluidas, resultadoBanco.TotalConcluidas);
        Assert.Equal(990m, resultadoBanco.ReceitaMaoDeObra); // 250 + 560 + 180 = 990
        Assert.Equal(resultadoMemoria.ReceitaMaoDeObra, resultadoBanco.ReceitaMaoDeObra);
        Assert.Equal(640m, resultadoBanco.ReceitaPecas);       // 120 + 340 + 180 = 640
        Assert.Equal(resultadoMemoria.ReceitaPecas, resultadoBanco.ReceitaPecas);
        Assert.Equal(70m, resultadoBanco.TotalDescontos);   // 30 + 40 + 0 = 70
        Assert.Equal(resultadoMemoria.TotalDescontos, resultadoBanco.TotalDescontos);
        Assert.Equal(1560m, resultadoBanco.FaturamentoTotal); // 990 + 640 - 70 = 1560
        Assert.Equal(resultadoMemoria.FaturamentoTotal, resultadoBanco.FaturamentoTotal);
        Assert.Equal(520m, resultadoBanco.TicketMedio);         // 1560 / 3 = 520
        Assert.Equal(resultadoMemoria.TicketMedio, resultadoBanco.TicketMedio);
        Assert.Equal(280m, resultadoBanco.LucroRealPecas);   // 30 + 160 + 90 = 280
        Assert.Equal(resultadoMemoria.LucroRealPecas, resultadoBanco.LucroRealPecas);
    }

    [Fact]
    public async Task GerarPacoteRelatoriosECsv_DeveRespeitarPermissaoFinanceiroSepararEstoqueEFormatarPtBr()
    {
        using var db = CreateInMemoryContext();
        var pecaRepo = new PecaRepository(db);
        var estoqueService = new EstoqueService(db, pecaRepo);
        var relatorioService = new RelatorioService(db, estoqueService);

        var usuario = new Usuario
        {
            Username = "atendente_rel",
            NomeCompleto = "Carla Atendimento",
            PasswordHash = "hash",
            Perfil = PerfilUsuario.Atendimento,
            Ativo = true
        };
        var pecaAtiva = new Peca { Sku = "ATV-01", Nome = "Peça Ativa", PrecoCusto = 50m, PrecoVenda = 100m, EstoqueAtual = 3, EstoqueMinimo = 1, Ativo = true };
        var pecaInativa = new Peca { Sku = "INA-01", Nome = "Peça Inativa", PrecoCusto = 40m, PrecoVenda = 80m, EstoqueAtual = 0, EstoqueMinimo = 1, Ativo = false };
        db.Usuarios.Add(usuario);
        db.Pecas.AddRange(pecaAtiva, pecaInativa);
        await db.SaveChangesAsync();

        var inicio = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim = inicio.AddMonths(1);

        // Movimentação fora do período vs dentro do período
        db.MovimentacoesEstoque.AddRange(
            new MovimentacaoEstoque
            {
                PecaId = pecaAtiva.Id,
                Tipo = TipoMovimentacao.Entrada,
                Quantidade = 5,
                DataHora = inicio.AddMonths(-1),
                Referencia = "Entrada antiga",
                UsuarioId = usuario.Id
            },
            new MovimentacaoEstoque
            {
                PecaId = pecaAtiva.Id,
                Tipo = TipoMovimentacao.Saida,
                Quantidade = -2,
                DataHora = inicio.AddDays(5),
                Referencia = "Saída no período",
                UsuarioId = usuario.Id
            });
        await db.SaveChangesAsync();

        // Atendimento (incluirFinanceiro: false) -> apenas RelatorioOS e RelatorioEstoque
        var (relatoriosAtendimento, _, _, estoqueResumo) = await relatorioService.GerarPacoteRelatoriosAsync(
            inicio, fim, "Carla Atendimento", incluirFinanceiro: false);

        Assert.Equal(2, relatoriosAtendimento.Count);
        Assert.DoesNotContain(relatoriosAtendimento, r => r is RelatorioFinanceiro);
        Assert.All(relatoriosAtendimento, r => Assert.Equal("Carla Atendimento", r.GeradoPor));

        // Estoque deve separar posição atual (1 ativa, 1 inativa) de movimentações no período (apenas 1 saída no período)
        Assert.Equal(1, estoqueResumo.PecasAtivas);
        Assert.Equal(1, estoqueResumo.PecasInativas);
        Assert.Equal(1, estoqueResumo.TotalMovimentacoesPeriodo);
        Assert.Equal(1, estoqueResumo.SaidasPeriodo);

        // CSV em formato pt-BR com separador ';'
        var ptBR = new CultureInfo("pt-BR");
        var csv = RelatorioService.GerarCsvPtBr(
            new[] { "Serviço", "Receita (R$)" },
            new[] { new string?[] { "Alinhamento; Balanceamento", 1250.50m.ToString("N2", ptBR) } });

        Assert.Contains("Serviço;Receita (R$)", csv);
        Assert.Contains("\"Alinhamento; Balanceamento\";1.250,50", csv);
    }
}
