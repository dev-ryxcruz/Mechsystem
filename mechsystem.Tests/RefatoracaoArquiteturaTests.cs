using mechsystem.Data;
using mechsystem.Models;
using mechsystem.Repositories;
using mechsystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace mechsystem.Tests;

public class RefatoracaoArquiteturaTests
{
    private static (AppDbContext db, EstoqueService estoqueService, OrdemServicoService osService) CreateSut()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"mechsystem_refac_{Guid.NewGuid():N}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        var pecaRepo = new PecaRepository(db);
        var estoqueService = new EstoqueService(db, pecaRepo);
        var osService = new OrdemServicoService(db, estoqueService);
        return (db, estoqueService, osService);
    }

    private static async Task<(Usuario admin, Usuario atendente, Usuario mecanico, Veiculo veiculo, Peca peca1, Peca peca2, Servico servico)> SeedAsync(AppDbContext db)
    {
        var admin = new Usuario { Username = "admin_r", NomeCompleto = "Admin", PasswordHash = "h", Perfil = PerfilUsuario.Administrador, Ativo = true };
        var atendente = new Usuario { Username = "atend_r", NomeCompleto = "Atendente", PasswordHash = "h", Perfil = PerfilUsuario.Atendimento, Ativo = true };
        var mecanico = new Usuario { Username = "mec_r", NomeCompleto = "Mecânico", PasswordHash = "h", Perfil = PerfilUsuario.Mecanico, Ativo = true };
        db.Usuarios.AddRange(admin, atendente, mecanico);

        var cliente = new Cliente { Nome = "Cliente Teste", Cpf = "12345678901", Telefone = "11999999999", Ativo = true };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var veiculo = new Veiculo { ClienteId = cliente.Id, Placa = "ABC1D23", Marca = "VW", Modelo = "Gol", Ano = 2020, Quilometragem = 50000, Ativo = true };
        var peca1 = new Peca { Sku = "SKU-001", Nome = "Filtro de Óleo", PrecoCusto = 20m, PrecoVenda = 50m, EstoqueAtual = 5, EstoqueMinimo = 1, Ativo = true };
        var peca2 = new Peca { Sku = "SKU-002", Nome = "Pastilha de Freio", PrecoCusto = 60m, PrecoVenda = 150m, EstoqueAtual = 2, EstoqueMinimo = 1, Ativo = true };
        var servico = new Servico { Nome = "Troca de Óleo", ValorPadrao = 100m, TempoEstimadoMinutos = 60, Ativo = true };

        db.Veiculos.Add(veiculo);
        db.Pecas.AddRange(peca1, peca2);
        db.Servicos.Add(servico);
        await db.SaveChangesAsync();

        return (admin, atendente, mecanico, veiculo, peca1, peca2, servico);
    }

    [Fact]
    public async Task ValidarDisponibilidadePecasAprovadas_LoteComReservaConcorrente_DeveValidarSemNPlusOne()
    {
        var (db, estoqueService, _) = CreateSut();
        var (_, _, _, veiculo, peca1, peca2, _) = await SeedAsync(db);

        // OS 1 já reserva 2 unidades de peca2 (estoque físico = 2 => disponível = 0)
        var os1 = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 50000,
            NivelCombustivelEntrada = NivelCombustivel.Meio,
            QueixaCliente = "Revisão 1",
            Status = OrdemServicoStatus.Aprovada,
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(1),
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(5),
            PecasUtilizadas = new List<OrdemServicoPeca>
            {
                new() { PecaId = peca2.Id, Quantidade = 2, PrecoBase = 150m, ValorCobrado = 150m, PrecoCustoSnapshot = 60m, Origem = OrigemPeca.Estoque, StatusItem = StatusItem.Aprovado, Aplicada = false }
            }
        };

        // OS 2 tenta aprovar peca1 (disponível) e peca2 (esgotada por reserva)
        var os2 = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 50100,
            NivelCombustivelEntrada = NivelCombustivel.Meio,
            QueixaCliente = "Revisão 2",
            Status = OrdemServicoStatus.Orcamento,
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(1),
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(5),
            PecasUtilizadas = new List<OrdemServicoPeca>
            {
                new() { PecaId = peca1.Id, Quantidade = 3, PrecoBase = 50m, ValorCobrado = 50m, PrecoCustoSnapshot = 20m, Origem = OrigemPeca.Estoque, StatusItem = StatusItem.Pendente },
                new() { PecaId = peca2.Id, Quantidade = 1, PrecoBase = 150m, ValorCobrado = 150m, PrecoCustoSnapshot = 60m, Origem = OrigemPeca.Estoque, StatusItem = StatusItem.Pendente }
            }
        };

        db.OrdensServico.AddRange(os1, os2);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => estoqueService.ValidarDisponibilidadePecasAprovadasAsync(os2.Id));

        Assert.Contains("Pastilha de Freio", ex.Message);
    }

    [Fact]
    public async Task CriarOrcamentoRequest_AtendimentoNaoPodeCobrarAbaixoDoPrecoBase()
    {
        var (db, _, osService) = CreateSut();
        var (_, atendente, _, veiculo, _, _, servico) = await SeedAsync(db);

        var request = new CriarOrcamentoRequest(
            veiculo.Id,
            50000,
            NivelCombustivel.Meio,
            "Barulho no motor",
            null,
            DateTime.UtcNow.AddDays(2),
            0m,
            new List<CriarOrcamentoServicoItem>
            {
                new(servico.Id, 1, servico.ValorPadrao, servico.ValorPadrao - 10m, servico.TempoEstimadoMinutos)
            },
            new List<CriarOrcamentoPecaItem>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => osService.CriarOrcamentoAsync(request, atendente.Id, PerfilUsuario.Atendimento, atendente.NomeCompleto));
    }

    [Fact]
    public async Task ListarResumoPaginado_DeveCalcularValorTotalSemProdutoCartesiano()
    {
        var (db, _, osService) = CreateSut();
        var (admin, _, _, veiculo, peca1, peca2, servico) = await SeedAsync(db);

        var request = new CriarOrcamentoRequest(
            veiculo.Id,
            50000,
            NivelCombustivel.Cheio,
            "Revisão completa",
            null,
            DateTime.UtcNow.AddDays(3),
            20m,
            new List<CriarOrcamentoServicoItem>
            {
                new(servico.Id, 2, 100m, 100m, 60) // 200
            },
            new List<CriarOrcamentoPecaItem>
            {
                new(peca1.Id, 1, 50m, 20m, 50m, OrigemPeca.Estoque), // 50
                new(peca2.Id, 1, 150m, 60m, 150m, OrigemPeca.Estoque) // 150
            });

        var osId = await osService.CriarOrcamentoAsync(request, admin.Id, PerfilUsuario.Administrador, admin.NomeCompleto);

        var (itens, total) = await osService.ListarResumoPaginadoAsync(null, 1, 10);
        Assert.Equal(1, total);
        var item = Assert.Single(itens);
        Assert.Equal(osId, item.Id);
        // 200 (serviços) + 200 (peças) - 20 (desconto) = 380
        Assert.Equal(380m, item.ValorTotal);
        Assert.Equal("ABC1D23", item.VeiculoPlaca);
        Assert.Equal("Cliente Teste", item.ClienteNome);
    }

    [Fact]
    public async Task ReportarProblema_FaltaDePeca_DeveRegistrarContatoEAlterarStatus()
    {
        var (db, _, osService) = CreateSut();
        var (admin, _, mecanico, veiculo, _, _, servico) = await SeedAsync(db);

        var request = new CriarOrcamentoRequest(
            veiculo.Id,
            50000,
            NivelCombustivel.Meio,
            "Revisão",
            null,
            DateTime.UtcNow.AddDays(2),
            0m,
            new List<CriarOrcamentoServicoItem> { new(servico.Id, 1, 100m, 100m, 60) },
            new List<CriarOrcamentoPecaItem>());

        var osId = await osService.CriarOrcamentoAsync(request, admin.Id, PerfilUsuario.Administrador, admin.NomeCompleto);
        await osService.AprovarOSAsync(osId, "Cliente", "Presencial", admin.Id, PerfilUsuario.Administrador);
        await osService.IniciarOuRetomarServicoAsync(osId, PerfilUsuario.Mecanico, mecanico.Id);

        await osService.ReportarProblemaAsync(osId, "Falta de peça", "Sem filtro em estoque", mecanico.Id, PerfilUsuario.Mecanico, mecanico.NomeCompleto);

        var osAtualizada = await db.OrdensServico.Include(o => o.Contatos).FirstAsync(o => o.Id == osId);
        Assert.Equal(OrdemServicoStatus.AguardandoPecas, osAtualizada.Status);
        Assert.Contains(osAtualizada.Contatos, c => c.Descricao.Contains("[Falta de peça] Sem filtro em estoque"));
    }
}
