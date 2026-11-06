using mechsystem.Data;
using mechsystem.Models;
using mechsystem.Repositories;
using mechsystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace mechsystem.Tests;

public class InativacaoTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"mechsystem_inativacao_test_{Guid.NewGuid():N}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    private static async Task<(Usuario admin, Usuario atendente, Usuario mecanico)> SeedUsuariosAsync(AppDbContext db)
    {
        var admin = new Usuario
        {
            Username = "admin_inat",
            NomeCompleto = "Admin Inativacao",
            PasswordHash = "hash",
            Perfil = PerfilUsuario.Administrador,
            Ativo = true
        };
        var atendente = new Usuario
        {
            Username = "atendente_inat",
            NomeCompleto = "Atendente Inativacao",
            PasswordHash = "hash",
            Perfil = PerfilUsuario.Atendimento,
            Ativo = true
        };
        var mecanico = new Usuario
        {
            Username = "mecanico_inat",
            NomeCompleto = "Mecanico Inativacao",
            PasswordHash = "hash",
            Perfil = PerfilUsuario.Mecanico,
            Ativo = true
        };

        db.Usuarios.AddRange(admin, atendente, mecanico);
        await db.SaveChangesAsync();
        return (admin, atendente, mecanico);
    }

    [Fact]
    public async Task NaoAdmin_DeveReceberUnauthorizedAccess_EmTodasOperacoesDeInativacaoEReativacao()
    {
        using var db = CreateInMemoryContext();
        var (_, atendente, mecanico) = await SeedUsuariosAsync(db);
        var service = new InativacaoService(db);

        // Atendente
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.InativarClienteAsync(1, "Motivo válido", atendente.Id, PerfilUsuario.Atendimento));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ReativarClienteAsync(1, atendente.Id, PerfilUsuario.Atendimento));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.InativarVeiculoAsync(1, "Motivo válido", atendente.Id, PerfilUsuario.Atendimento));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ReativarVeiculoAsync(1, atendente.Id, PerfilUsuario.Atendimento));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.InativarPecaAsync(1, "Motivo válido", atendente.Id, PerfilUsuario.Atendimento));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ReativarPecaAsync(1, atendente.Id, PerfilUsuario.Atendimento));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.InativarServicoAsync(1, "Motivo válido", atendente.Id, PerfilUsuario.Atendimento));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ReativarServicoAsync(1, atendente.Id, PerfilUsuario.Atendimento));

        // Mecânico e não autenticado
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.InativarClienteAsync(1, "Motivo válido", mecanico.Id, PerfilUsuario.Mecanico));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.InativarVeiculoAsync(1, "Motivo válido", 0, null));
    }

    [Fact]
    public async Task MotivoInvalido_DeveBloquearInativacaoComMenosDeCincoCaracteres()
    {
        using var db = CreateInMemoryContext();
        var (admin, _, _) = await SeedUsuariosAsync(db);
        var service = new InativacaoService(db);

        var cliente = new Cliente { Nome = "Maria Souza", Cpf = "52998224725" };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InativarClienteAsync(cliente.Id, "abc", admin.Id, PerfilUsuario.Administrador));
        Assert.Contains("5 caracteres", ex.Message);
    }

    [Fact]
    public async Task Cliente_DeveBloquearComOSAberta_ECascatearParaVeiculosSemReativarAutomaticamente()
    {
        using var db = CreateInMemoryContext();
        var (admin, _, _) = await SeedUsuariosAsync(db);
        var service = new InativacaoService(db);

        var cliente = new Cliente { Nome = "Carlos Cascata", Cpf = "52998224725", Ativo = true };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var veiculo1 = new Veiculo { ClienteId = cliente.Id, Placa = "AAA1A11", Marca = "Fiat", Modelo = "Uno", Ano = 2020, Ativo = true };
        var veiculo2 = new Veiculo { ClienteId = cliente.Id, Placa = "BBB2B22", Marca = "Ford", Modelo = "Ka", Ano = 2021, Ativo = true };
        db.Veiculos.AddRange(veiculo1, veiculo2);
        await db.SaveChangesAsync();

        // Cria O.S. em aberto (EmAndamento) no veiculo1 -> deve bloquear inativação do cliente e do veiculo1
        var osAberta = new OrdemServico
        {
            VeiculoId = veiculo1.Id,
            KmEntrada = 25000,
            NivelCombustivelEntrada = NivelCombustivel.Meio,
            QueixaCliente = "Motor falhando",
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(2),
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(10),
            Status = OrdemServicoStatus.EmAndamento
        };
        db.OrdensServico.Add(osAberta);
        await db.SaveChangesAsync();

        var exCliente = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InativarClienteAsync(cliente.Id, "Solicitação do titular", admin.Id, PerfilUsuario.Administrador));
        Assert.Contains("em aberto", exCliente.Message, StringComparison.OrdinalIgnoreCase);

        var exVeiculo = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InativarVeiculoAsync(veiculo1.Id, "Veículo vendido", admin.Id, PerfilUsuario.Administrador));
        Assert.Contains("em aberto", exVeiculo.Message, StringComparison.OrdinalIgnoreCase);

        // Conclui a O.S. -> agora pode inativar o cliente em cascata
        osAberta.Status = OrdemServicoStatus.Concluida;
        osAberta.DataConclusao = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var veiculosInativados = await service.InativarClienteAsync(cliente.Id, "Cliente mudou de cidade", admin.Id, PerfilUsuario.Administrador);
        Assert.Equal(2, veiculosInativados);

        var clienteDb = await db.Clientes.FindAsync(cliente.Id);
        var v1Db = await db.Veiculos.FindAsync(veiculo1.Id);
        var v2Db = await db.Veiculos.FindAsync(veiculo2.Id);

        Assert.False(clienteDb!.Ativo);
        Assert.NotNull(clienteDb.DataInativacao);
        Assert.Equal(admin.Id, clienteDb.InativadoPorUsuarioId);
        Assert.Equal("Cliente mudou de cidade", clienteDb.MotivoInativacao);

        Assert.False(v1Db!.Ativo);
        Assert.False(v2Db!.Ativo);

        // Tentar reativar veículo com cliente ainda inativo -> deve bloquear
        var exReativarVeiculo = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ReativarVeiculoAsync(veiculo1.Id, admin.Id, PerfilUsuario.Administrador));
        Assert.Contains("proprietário", exReativarVeiculo.Message);

        // Reativar cliente NÃO deve reativar os veículos automaticamente
        await service.ReativarClienteAsync(cliente.Id, admin.Id, PerfilUsuario.Administrador);
        Assert.True((await db.Clientes.FindAsync(cliente.Id))!.Ativo);
        Assert.Null((await db.Clientes.FindAsync(cliente.Id))!.MotivoInativacao);
        Assert.False((await db.Veiculos.FindAsync(veiculo1.Id))!.Ativo);
        Assert.False((await db.Veiculos.FindAsync(veiculo2.Id))!.Ativo);

        // Agora reativar veiculo1 individualmente é permitido
        await service.ReativarVeiculoAsync(veiculo1.Id, admin.Id, PerfilUsuario.Administrador);
        Assert.True((await db.Veiculos.FindAsync(veiculo1.Id))!.Ativo);
        Assert.False((await db.Veiculos.FindAsync(veiculo2.Id))!.Ativo);
    }

    [Fact]
    public async Task Peca_DeveBloquearSeEmOSAbertaOuComSaldoPositivo_EPermitirQuandoZerada()
    {
        using var db = CreateInMemoryContext();
        var (admin, _, _) = await SeedUsuariosAsync(db);
        var service = new InativacaoService(db);

        var cliente = new Cliente { Nome = "Ana Peças", Cpf = "52998224725" };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var veiculo = new Veiculo { ClienteId = cliente.Id, Placa = "CCC3C33", Marca = "VW", Modelo = "Gol", Ano = 2019 };
        var peca = new Peca { Sku = "SKU-TESTE-01", Nome = "Vela de Ignição", PrecoCusto = 15m, PrecoVenda = 35m, EstoqueAtual = 4, EstoqueMinimo = 1 };
        db.Veiculos.Add(veiculo);
        db.Pecas.Add(peca);
        await db.SaveChangesAsync();

        // 1. Bloqueio por saldo > 0
        var exSaldo = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InativarPecaAsync(peca.Id, "Peça fora de linha", admin.Id, PerfilUsuario.Administrador));
        Assert.Contains("zere o saldo por Ajuste antes de desativar", exSaldo.Message, StringComparison.OrdinalIgnoreCase);

        // 2. Zera saldo, mas vincula a O.S. em Orçamento -> bloqueia por O.S. em aberto
        peca.EstoqueAtual = 0;
        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 40000,
            NivelCombustivelEntrada = NivelCombustivel.Reserva,
            QueixaCliente = "Falha de ignição",
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(1),
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(10),
            Status = OrdemServicoStatus.Orcamento
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        db.OrdemServicoPecas.Add(new OrdemServicoPeca
        {
            OrdemServicoId = os.Id,
            PecaId = peca.Id,
            Quantidade = 4,
            PrecoBase = 35m,
            ValorCobrado = 35m,
            PrecoCustoSnapshot = 15m
        });
        await db.SaveChangesAsync();

        var exOS = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InativarPecaAsync(peca.Id, "Peça fora de linha", admin.Id, PerfilUsuario.Administrador));
        Assert.Contains("em aberto", exOS.Message, StringComparison.OrdinalIgnoreCase);

        // 3. Conclui a O.S. -> com saldo 0 e sem O.S. aberta, permite inativar e reativar
        os.Status = OrdemServicoStatus.Concluida;
        os.DataConclusao = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await service.InativarPecaAsync(peca.Id, "Peça descontinuada pelo fabricante", admin.Id, PerfilUsuario.Administrador);
        var pecaInativa = await db.Pecas.FindAsync(peca.Id);
        Assert.False(pecaInativa!.Ativo);
        Assert.Equal("Peça descontinuada pelo fabricante", pecaInativa.MotivoInativacao);

        await service.ReativarPecaAsync(peca.Id, admin.Id, PerfilUsuario.Administrador);
        Assert.True((await db.Pecas.FindAsync(peca.Id))!.Ativo);
    }

    [Fact]
    public async Task Servico_DeveBloquearSeEmOSAberta_EPreservarHistoricoEmOSAntiga()
    {
        using var db = CreateInMemoryContext();
        var (admin, _, _) = await SeedUsuariosAsync(db);
        var service = new InativacaoService(db);

        var cliente = new Cliente { Nome = "Pedro Serviços", Cpf = "52998224725" };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var veiculo = new Veiculo { ClienteId = cliente.Id, Placa = "DDD4D44", Marca = "GM", Modelo = "Onix", Ano = 2021 };
        var servico = new Servico { Nome = "Alinhamento 3D", Descricao = "Alinhamento computadorizado", ValorPadrao = 120m, TempoEstimadoMinutos = 45 };
        db.Veiculos.Add(veiculo);
        db.Servicos.Add(servico);
        await db.SaveChangesAsync();

        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            KmEntrada = 15000,
            NivelCombustivelEntrada = NivelCombustivel.UmQuarto,
            QueixaCliente = "Puxando para a direita",
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(1),
            ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(10),
            Status = OrdemServicoStatus.AguardandoPecas
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        db.OrdemServicoServicos.Add(new OrdemServicoServico
        {
            OrdemServicoId = os.Id,
            ServicoId = servico.Id,
            Quantidade = 1,
            PrecoBase = 120m,
            ValorCobrado = 120m
        });
        await db.SaveChangesAsync();

        // Bloqueado enquanto O.S. está em aberto
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InativarServicoAsync(servico.Id, "Equipamento desativado", admin.Id, PerfilUsuario.Administrador));
        Assert.Contains("em aberto", ex.Message, StringComparison.OrdinalIgnoreCase);

        // Conclui O.S. -> permite inativar serviço e mantém histórico da O.S.
        os.Status = OrdemServicoStatus.Concluida;
        os.DataConclusao = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await service.InativarServicoAsync(servico.Id, "Equipamento substituído", admin.Id, PerfilUsuario.Administrador);
        Assert.False((await db.Servicos.FindAsync(servico.Id))!.Ativo);
        Assert.Empty(await db.Servicos.Ativos().ToListAsync());

        // Histórico da O.S. antiga continua visível (sem HasQueryFilter global)
        var osHistorico = await db.OrdensServico
            .Include(o => o.ServicosAExecutarList)
            .ThenInclude(s => s.Servico)
            .FirstAsync(o => o.Id == os.Id);
        Assert.Single(osHistorico.ServicosAExecutarList);
        Assert.Equal("Alinhamento 3D", osHistorico.ServicosAExecutarList.First().Servico!.Nome);
    }

    [Fact]
    public async Task DuplicidadeComRegistroInativo_DeveInformarMensagemDeReativacaoParaCpfPlacaESku()
    {
        using var db = CreateInMemoryContext();
        var (admin, _, _) = await SeedUsuariosAsync(db);
        var inativacaoService = new InativacaoService(db);
        var clienteRepo = new ClienteRepository(db);
        var veiculoRepo = new VeiculoRepository(db);
        var pecaRepo = new PecaRepository(db);

        // 1. Cliente inativo com CPF
        var cliente = new Cliente { Nome = "João Inativo", Cpf = "52998224725" };
        await clienteRepo.Salvar(cliente);
        await inativacaoService.InativarClienteAsync(cliente.Id, "Inativação para teste", admin.Id, PerfilUsuario.Administrador);

        var exCpf = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            clienteRepo.Salvar(new Cliente { Nome = "Outro Nome", Cpf = "52998224725" }));
        Assert.Equal("Já existe um cliente inativo com este CPF: João Inativo. Reative-o em Clientes > Inativos.", exCpf.Message);

        // 2. Veículo inativo com Placa
        await inativacaoService.ReativarClienteAsync(cliente.Id, admin.Id, PerfilUsuario.Administrador);
        var veiculo = new Veiculo { ClienteId = cliente.Id, Placa = "XYZ9Z99", Marca = "Honda", Modelo = "Civic", Ano = 2020 };
        await veiculoRepo.Salvar(veiculo);
        await inativacaoService.InativarVeiculoAsync(veiculo.Id, "Inativação para teste", admin.Id, PerfilUsuario.Administrador);

        var exPlaca = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            veiculoRepo.Salvar(new Veiculo { ClienteId = cliente.Id, Placa = "XYZ-9Z99", Marca = "Honda", Modelo = "Fit", Ano = 2021 }));
        Assert.Contains("Já existe um veículo inativo com esta placa", exPlaca.Message);
        Assert.Contains("Reative-o em Veículos > Inativos.", exPlaca.Message);

        // 3. Peça inativa com SKU
        var peca = new Peca { Sku = "OLEO-5W30", Nome = "Óleo Sintético 5W30", PrecoCusto = 25m, PrecoVenda = 50m, EstoqueAtual = 0, EstoqueMinimo = 2 };
        await pecaRepo.AddAsync(peca);
        await inativacaoService.InativarPecaAsync(peca.Id, "Inativação para teste", admin.Id, PerfilUsuario.Administrador);

        var exSku = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pecaRepo.AddAsync(new Peca { Sku = "oleo-5w30", Nome = "Outro Óleo", PrecoCusto = 30m, PrecoVenda = 60m, EstoqueAtual = 5, EstoqueMinimo = 1 }));
        Assert.Equal("Já existe uma peça inativa com este SKU: Óleo Sintético 5W30. Reative-a em Estoque > Inativos.", exSku.Message);
    }

    [Fact]
    public async Task PaginacaoEBuscaNoBanco_DeveFiltrarEPaginarCorretamenteNosCincoCruds()
    {
        using var db = CreateInMemoryContext();
        var clienteRepo = new ClienteRepository(db);
        var veiculoRepo = new VeiculoRepository(db);
        var pecaRepo = new PecaRepository(db);
        var servicoRepo = new ServicoRepository(db);
        var osService = new OrdemServicoService(db, new EstoqueService(db, pecaRepo));

        for (int i = 1; i <= 15; i++)
        {
            var cliente = new Cliente { Nome = $"Cliente {i:D2}", Cpf = $"{i:D11}", Telefone = $"1199999{i:D4}", Ativo = i <= 12 };
            db.Clientes.Add(cliente);
            await db.SaveChangesAsync();

            var veiculo = new Veiculo { ClienteId = cliente.Id, Placa = $"AAA{i:D1}A{i:D2}", Marca = "MarcaX", Modelo = $"Modelo {i:D2}", Ano = 2020, Ativo = i <= 12 };
            db.Veiculos.Add(veiculo);

            db.Pecas.Add(new Peca { Sku = $"SKU-{i:D2}", Nome = $"Peça {i:D2}", PrecoCusto = 10m, PrecoVenda = 20m, EstoqueAtual = 5, EstoqueMinimo = 1, Ativo = i <= 12 });
            db.Servicos.Add(new Servico { Nome = $"Serviço {i:D2}", ValorPadrao = 50m, Ativo = i <= 12 });
            await db.SaveChangesAsync();

            db.OrdensServico.Add(new OrdemServico
            {
                VeiculoId = veiculo.Id,
                KmEntrada = 10000 + i * 100,
                NivelCombustivelEntrada = NivelCombustivel.Meio,
                QueixaCliente = $"Problema {i:D2}",
                DataEmissao = DateTime.UtcNow,
                DataPrevisaoEntrega = DateTime.UtcNow.AddDays(1),
                ValidadeOrcamentoAte = DateTime.UtcNow.AddDays(10),
                Status = OrdemServicoStatus.Orcamento
            });
            await db.SaveChangesAsync();
        }

        // Clientes: 12 ativos, página 1 (10 itens) e página 2 (2 itens)
        var (clientesP1, totalClientesAtivos) = await clienteRepo.ListarPaginadoAsync(inativos: false, termoBusca: null, pagina: 1, tamanhoPagina: 10);
        var (clientesP2, _) = await clienteRepo.ListarPaginadoAsync(inativos: false, termoBusca: null, pagina: 2, tamanhoPagina: 10);
        Assert.Equal(12, totalClientesAtivos);
        Assert.Equal(10, clientesP1.Count);
        Assert.Equal(2, clientesP2.Count);

        // Busca específica no banco
        var (clientesBusca, totalBuscaCliente) = await clienteRepo.ListarPaginadoAsync(inativos: false, termoBusca: "Cliente 05", pagina: 1, tamanhoPagina: 10);
        Assert.Equal(1, totalBuscaCliente);
        Assert.Single(clientesBusca);

        var (veiculosP1, totalVeiculosInativos) = await veiculoRepo.ListarPaginadoAsync(inativos: true, termoBusca: null, pagina: 1, tamanhoPagina: 10);
        Assert.Equal(3, totalVeiculosInativos);
        Assert.Equal(3, veiculosP1.Count);

        var (pecasBusca, totalPecasBusca) = await pecaRepo.GetPaginadoAsync(inativas: false, termoBusca: "SKU-07", pagina: 1, tamanhoPagina: 10);
        Assert.Equal(1, totalPecasBusca);
        Assert.Equal("SKU-07", pecasBusca[0].Sku);

        var (servicosP1, totalServicosAtivos) = await servicoRepo.GetPaginadoAsync(inativos: false, termoBusca: null, pagina: 1, tamanhoPagina: 10);
        Assert.Equal(12, totalServicosAtivos);
        Assert.Equal(10, servicosP1.Count);

        var (osP1, totalOs) = await osService.ListarPaginadoAsync(termoBusca: null, pagina: 1, tamanhoPagina: 10);
        var (osP2, _) = await osService.ListarPaginadoAsync(termoBusca: null, pagina: 2, tamanhoPagina: 10);
        Assert.Equal(15, totalOs);
        Assert.Equal(10, osP1.Count);
        Assert.Equal(5, osP2.Count);
    }
}
