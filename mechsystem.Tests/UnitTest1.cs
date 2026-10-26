using mechsystem.Data;
using mechsystem.Models;
using mechsystem.Repositories;
using mechsystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            DescricaoProblemaRelatado = "Revisão geral",
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(3),
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
        await osService.CancelarOSAsync(os.Id, admin.Id, PerfilUsuario.Administrador);
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
            DescricaoProblemaRelatado = "Freios e motor",
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(2),
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
            DescricaoProblemaRelatado = "Troca de filtro",
            DataEmissao = DateTime.UtcNow,
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(1),
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
    public void CalculoTotaisOS_DeveRespeitarFallbackESomaDeItensEDesconto()
    {
        var os = new OrdemServico
        {
            ValorMaoDeObra = 150m,
            ValorPecas = 100m,
            ValorDesconto = 20m
        };

        // Sem itens vinculados: usa valores manuais
        Assert.Equal(150m, os.ValorMaoDeObraEfetivo);
        Assert.Equal(100m, os.ValorPecasEfetivo);
        Assert.Equal(230m, os.ValorTotal);

        // Com serviços e peças vinculados: sobrescreve pelos subtotais dos itens
        os.ServicosAExecutarList.Add(new OrdemServicoServico
        {
            Quantidade = 2,
            PrecoBase = 100m,
            ValorCobrado = 90m
        });
        os.PecasUtilizadas.Add(new OrdemServicoPeca
        {
            Quantidade = 3,
            PrecoBase = 50m,
            ValorCobrado = 40m
        });

        Assert.Equal(180m, os.ValorMaoDeObraEfetivo); // 2 * 90
        Assert.Equal(120m, os.ValorPecasEfetivo);     // 3 * 40
        Assert.Equal(280m, os.ValorTotal);            // 180 + 120 - 20
    }

    [Fact]
    public async Task ConcluirOS_DevePreencherDataConclusaoEmUtcECalcularMargemNoPeriodo()
    {
        var (db, _, estoqueService, osService) = CreateSut();
        var (admin, _, _, veiculo, peca1, _) = await SeedBasicScenarioAsync(db);

        var os = new OrdemServico
        {
            VeiculoId = veiculo.Id,
            DescricaoProblemaRelatado = "Revisão concluída",
            DataEmissao = DateTime.UtcNow.AddMonths(-1),
            DataPrevisaoEntrega = DateTime.UtcNow.AddDays(1),
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
    public async Task PostgresRelationalTransactionRollback_DeveReverterBaixaParcialNoBancoReal()
    {
        string? connectionString = null;
        PostgreSqlContainer? container = null;
        string? tempDbName = null;
        const string localAdminConn = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=root";

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
                    connectionString = $"Host=localhost;Port=5432;Database={tempDbName};Username=postgres;Password=root";
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
                    DescricaoProblemaRelatado = "Teste transacional PostgreSQL",
                    DataEmissao = DateTime.UtcNow,
                    DataPrevisaoEntrega = DateTime.UtcNow.AddDays(2),
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
}
