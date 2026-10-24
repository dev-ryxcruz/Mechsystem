using mechsystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace mechsystem.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger? logger = null)
    {
        // Idempotency check: only seed demo dataset if demo SKU is not yet present
        if (await context.Pecas.AnyAsync(p => p.Sku == "DEMO-FILT-01"))
        {
            logger?.LogInformation("Dados de demonstração (--seed-demo) já estão presentes no banco.");
            return;
        }

        var agora = DateTime.UtcNow;

        // 1. Clientes com CPFs válidos pelo algoritmo de módulo 11
        var clientesDemo = new List<Cliente>
        {
            new()
            {
                Nome = "Carlos Eduardo Mendes",
                Cpf = "52998224725",
                Telefone = "11987654321",
                Email = "carlos.mendes@exemplo.com.br",
                Endereco = "Rua Vergueiro, 1450 - Vila Mariana, São Paulo/SP",
                Ativo = true
            },
            new()
            {
                Nome = "Fernanda Souza Lima",
                Cpf = "12345678909",
                Telefone = "11976543210",
                Email = "fernanda.lima@exemplo.com.br",
                Endereco = "Av. Paulista, 900 - Bela Vista, São Paulo/SP",
                Ativo = true
            },
            new()
            {
                Nome = "Roberto Alves Ferreira",
                Cpf = "11144477735",
                Telefone = "11965432109",
                Email = "roberto.ferreira@exemplo.com.br",
                Endereco = "Rua Domingos de Morais, 2100 - Vila Mariana, São Paulo/SP",
                Ativo = true
            },
            new()
            {
                Nome = "Juliana Costa Ribeiro",
                Cpf = "98765432100",
                Telefone = "11954321098",
                Email = "juliana.ribeiro@exemplo.com.br",
                Endereco = "Rua Augusta, 1200 - Consolação, São Paulo/SP",
                Ativo = true
            }
        };

        foreach (var c in clientesDemo)
        {
            if (!await context.Clientes.IgnoreQueryFilters().AnyAsync(x => x.Cpf == c.Cpf))
            {
                context.Clientes.Add(c);
            }
        }
        await context.SaveChangesAsync();

        var cliente1 = await context.Clientes.FirstAsync(c => c.Cpf == "52998224725");
        var cliente2 = await context.Clientes.FirstAsync(c => c.Cpf == "12345678909");
        var cliente3 = await context.Clientes.FirstAsync(c => c.Cpf == "11144477735");
        var cliente4 = await context.Clientes.FirstAsync(c => c.Cpf == "98765432100");

        // 2. Veículos
        var veiculosDemo = new List<Veiculo>
        {
            new() { Placa = "BRA2E19", Marca = "Ford", Modelo = "Ka SE 1.5", Ano = 2019, Cor = "Branco", Quilometragem = 64200, ClienteId = cliente1.Id },
            new() { Placa = "MER4C21", Marca = "Chevrolet", Modelo = "Onix LT Turbo", Ano = 2021, Cor = "Prata", Quilometragem = 41500, ClienteId = cliente2.Id },
            new() { Placa = "PAU3A45", Marca = "Volkswagen", Modelo = "Gol 1.6 MSI", Ano = 2020, Cor = "Cinza", Quilometragem = 58900, ClienteId = cliente3.Id },
            new() { Placa = "SPX9F88", Marca = "Fiat", Modelo = "Strada Freedom 1.3", Ano = 2022, Cor = "Vermelho", Quilometragem = 32800, ClienteId = cliente4.Id }
        };

        foreach (var v in veiculosDemo)
        {
            if (!await context.Veiculos.AnyAsync(x => x.Placa == v.Placa))
            {
                context.Veiculos.Add(v);
            }
        }
        await context.SaveChangesAsync();

        var veiculo1 = await context.Veiculos.FirstAsync(v => v.Placa == "BRA2E19");
        var veiculo2 = await context.Veiculos.FirstAsync(v => v.Placa == "MER4C21");
        var veiculo3 = await context.Veiculos.FirstAsync(v => v.Placa == "PAU3A45");
        var veiculo4 = await context.Veiculos.FirstAsync(v => v.Placa == "SPX9F88");

        // 3. Peças
        var pecaFiltro = new Peca { Nome = "Filtro de Óleo Blindado", Sku = "DEMO-FILT-01", Marca = "Bosch", PrecoCusto = 22.00m, PrecoVenda = 48.00m, EstoqueAtual = 18, EstoqueMinimo = 5, Localizacao = "Corredor A - Prat. 1", Ativo = true };
        var pecaOleo = new Peca { Nome = "Óleo Motor 5W30 Sintético 1L", Sku = "DEMO-OLEO-01", Marca = "Mobil", PrecoCusto = 34.00m, PrecoVenda = 65.00m, EstoqueAtual = 32, EstoqueMinimo = 10, Localizacao = "Corredor A - Prat. 2", Ativo = true };
        var pecaPastilha = new Peca { Nome = "Jogo de Pastilhas de Freio Diant.", Sku = "DEMO-FREI-01", Marca = "Cobreq", PrecoCusto = 75.00m, PrecoVenda = 155.00m, EstoqueAtual = 12, EstoqueMinimo = 4, Localizacao = "Corredor B - Prat. 1", Ativo = true };
        var pecaVela = new Peca { Nome = "Jogo de Velas de Ignição Iridium", Sku = "DEMO-VELA-01", Marca = "NGK", PrecoCusto = 110.00m, PrecoVenda = 210.00m, EstoqueAtual = 8, EstoqueMinimo = 3, Localizacao = "Corredor B - Prat. 3", Ativo = true };
        var pecaCorreia = new Peca { Nome = "Kit Correia Dentada + Tensor", Sku = "DEMO-CORR-01", Marca = "Gates", PrecoCusto = 185.00m, PrecoVenda = 340.00m, EstoqueAtual = 3, EstoqueMinimo = 4, Localizacao = "Corredor C - Prat. 1", Ativo = true };
        var pecaAmort = new Peca { Nome = "Par Amortecedor Dianteiro Pressurizado", Sku = "DEMO-AMOR-01", Marca = "Cofap", PrecoCusto = 420.00m, PrecoVenda = 760.00m, EstoqueAtual = 4, EstoqueMinimo = 2, Localizacao = "Corredor C - Prat. 4", Ativo = true };

        context.Pecas.AddRange(pecaFiltro, pecaOleo, pecaPastilha, pecaVela, pecaCorreia, pecaAmort);

        // 4. Serviços
        var servTrocaOleo = new Servico { Nome = "Troca de Óleo e Filtro", Descricao = "Drenagem, substituição do filtro e abastecimento com óleo sintético", ValorPadrao = 120.00m, TempoEstimadoMinutos = 45 };
        var servFreios = new Servico { Nome = "Revisão Completa de Freios", Descricao = "Substituição de pastilhas, limpeza de pinças e verificação de fluido", ValorPadrao = 220.00m, TempoEstimadoMinutos = 90 };
        var servAlinhamento = new Servico { Nome = "Alinhamento e Balanceamento 3D", Descricao = "Alinhamento computadorizado e balanceamento das 4 rodas", ValorPadrao = 160.00m, TempoEstimadoMinutos = 60 };
        var servCorreia = new Servico { Nome = "Substituição de Kit Correia Dentada", Descricao = "Troca da correia sincronizadora e rolamento tensor", ValorPadrao = 380.00m, TempoEstimadoMinutos = 180 };
        var servScanner = new Servico { Nome = "Diagnóstico Eletrônico via Scanner", Descricao = "Leitura de parâmetros de injeção eletrônica e reset de falhas", ValorPadrao = 140.00m, TempoEstimadoMinutos = 40 };

        context.Servicos.AddRange(servTrocaOleo, servFreios, servAlinhamento, servCorreia, servScanner);
        await context.SaveChangesAsync();

        // 5. Ordens de Serviço em estados variados
        var osConcluida = new OrdemServico
        {
            VeiculoId = veiculo1.Id,
            DataEmissao = agora.AddDays(-5),
            DataPrevisaoInicio = agora.AddDays(-5),
            DataPrevisaoEntrega = agora.AddDays(-4),
            DataAutorizacao = agora.AddDays(-5).AddHours(1),
            DataConclusao = agora.AddDays(-4),
            AutorizadoPor = cliente1.Nome,
            MeioAutorizacao = "Assinatura Presencial",
            DescricaoProblemaRelatado = "Revisão periódica de 60.000 km e barulho ao frear.",
            ServicoAExecutar = "1x Troca de Óleo e Filtro\n1x Revisão Completa de Freios",
            Status = OrdemServicoStatus.Concluida,
            TempoGastoMinutos = 125,
            ValorMaoDeObra = 340.00m,
            ValorPecas = 463.00m,
            TokenAcompanhamento = Guid.NewGuid().ToString("N")
        };

        var osEmAndamento = new OrdemServico
        {
            VeiculoId = veiculo2.Id,
            DataEmissao = agora.AddDays(-1),
            DataPrevisaoInicio = agora.AddDays(-1),
            DataPrevisaoEntrega = agora.AddDays(1),
            DataAutorizacao = agora.AddHours(-6),
            AutorizadoPor = cliente2.Nome,
            MeioAutorizacao = "WhatsApp",
            DescricaoProblemaRelatado = "Luz de injeção acesa no painel e falha em retomadas.",
            ServicoAExecutar = "1x Diagnóstico Eletrônico via Scanner",
            Status = OrdemServicoStatus.EmAndamento,
            InicioUltimaExecucao = agora.AddMinutes(-25),
            TempoGastoMinutos = 15,
            ValorMaoDeObra = 140.00m,
            ValorPecas = 210.00m,
            TokenAcompanhamento = Guid.NewGuid().ToString("N")
        };

        var osPausada = new OrdemServico
        {
            VeiculoId = veiculo3.Id,
            DataEmissao = agora.AddDays(-2),
            DataPrevisaoInicio = agora.AddDays(-2),
            DataPrevisaoEntrega = agora.AddDays(1),
            DataAutorizacao = agora.AddDays(-2).AddHours(2),
            AutorizadoPor = cliente3.Nome,
            MeioAutorizacao = "Telefone",
            DescricaoProblemaRelatado = "Preventiva de sincronismo do motor.",
            ServicoAExecutar = "1x Substituição de Kit Correia Dentada",
            Status = OrdemServicoStatus.Pausada,
            TempoGastoMinutos = 65,
            ValorMaoDeObra = 380.00m,
            ValorPecas = 340.00m,
            TokenAcompanhamento = Guid.NewGuid().ToString("N")
        };

        var osAguardandoPecas = new OrdemServico
        {
            VeiculoId = veiculo4.Id,
            DataEmissao = agora.AddDays(-3),
            DataPrevisaoInicio = agora.AddDays(-3),
            DataPrevisaoEntrega = agora.AddDays(2),
            DataAutorizacao = agora.AddDays(-3).AddHours(3),
            AutorizadoPor = cliente4.Nome,
            MeioAutorizacao = "WhatsApp",
            DescricaoProblemaRelatado = "Instabilidade na suspensão dianteira em piso irregular.",
            ServicoAExecutar = "1x Alinhamento e Balanceamento 3D",
            Status = OrdemServicoStatus.AguardandoPecas,
            TempoGastoMinutos = 40,
            ValorMaoDeObra = 160.00m,
            ValorPecas = 760.00m,
            TokenAcompanhamento = Guid.NewGuid().ToString("N")
        };

        var osOrcamento = new OrdemServico
        {
            VeiculoId = veiculo1.Id,
            DataEmissao = agora,
            DataPrevisaoInicio = agora.AddDays(1),
            DataPrevisaoEntrega = agora.AddDays(2),
            DescricaoProblemaRelatado = "Cliente solicitou orçamento para alinhamento e troca de velas.",
            ServicoAExecutar = "1x Alinhamento e Balanceamento 3D",
            Status = OrdemServicoStatus.Orcamento,
            ValorMaoDeObra = 160.00m,
            ValorPecas = 210.00m,
            TokenAcompanhamento = Guid.NewGuid().ToString("N")
        };

        context.OrdensServico.AddRange(osConcluida, osEmAndamento, osPausada, osAguardandoPecas, osOrcamento);
        await context.SaveChangesAsync();

        context.OrdemServicoPecas.AddRange(
            new OrdemServicoPeca { OrdemServicoId = osConcluida.Id, PecaId = pecaFiltro.Id, Quantidade = 1, PrecoBase = pecaFiltro.PrecoVenda, ValorCobrado = pecaFiltro.PrecoVenda, PrecoCustoSnapshot = pecaFiltro.PrecoCusto },
            new OrdemServicoPeca { OrdemServicoId = osConcluida.Id, PecaId = pecaOleo.Id, Quantidade = 4, PrecoBase = pecaOleo.PrecoVenda, ValorCobrado = pecaOleo.PrecoVenda, PrecoCustoSnapshot = pecaOleo.PrecoCusto },
            new OrdemServicoPeca { OrdemServicoId = osConcluida.Id, PecaId = pecaPastilha.Id, Quantidade = 1, PrecoBase = pecaPastilha.PrecoVenda, ValorCobrado = pecaPastilha.PrecoVenda, PrecoCustoSnapshot = pecaPastilha.PrecoCusto },
            new OrdemServicoPeca { OrdemServicoId = osEmAndamento.Id, PecaId = pecaVela.Id, Quantidade = 1, PrecoBase = pecaVela.PrecoVenda, ValorCobrado = pecaVela.PrecoVenda, PrecoCustoSnapshot = pecaVela.PrecoCusto },
            new OrdemServicoPeca { OrdemServicoId = osPausada.Id, PecaId = pecaCorreia.Id, Quantidade = 1, PrecoBase = pecaCorreia.PrecoVenda, ValorCobrado = pecaCorreia.PrecoVenda, PrecoCustoSnapshot = pecaCorreia.PrecoCusto },
            new OrdemServicoPeca { OrdemServicoId = osAguardandoPecas.Id, PecaId = pecaAmort.Id, Quantidade = 1, PrecoBase = pecaAmort.PrecoVenda, ValorCobrado = pecaAmort.PrecoVenda, PrecoCustoSnapshot = pecaAmort.PrecoCusto },
            new OrdemServicoPeca { OrdemServicoId = osOrcamento.Id, PecaId = pecaVela.Id, Quantidade = 1, PrecoBase = pecaVela.PrecoVenda, ValorCobrado = pecaVela.PrecoVenda, PrecoCustoSnapshot = pecaVela.PrecoCusto }
        );

        context.OrdemServicoServicos.AddRange(
            new OrdemServicoServico { OrdemServicoId = osConcluida.Id, ServicoId = servTrocaOleo.Id, Quantidade = 1, PrecoBase = servTrocaOleo.ValorPadrao, ValorCobrado = servTrocaOleo.ValorPadrao, TempoEstimadoMinutosSnapshot = servTrocaOleo.TempoEstimadoMinutos },
            new OrdemServicoServico { OrdemServicoId = osConcluida.Id, ServicoId = servFreios.Id, Quantidade = 1, PrecoBase = servFreios.ValorPadrao, ValorCobrado = servFreios.ValorPadrao, TempoEstimadoMinutosSnapshot = servFreios.TempoEstimadoMinutos },
            new OrdemServicoServico { OrdemServicoId = osEmAndamento.Id, ServicoId = servScanner.Id, Quantidade = 1, PrecoBase = servScanner.ValorPadrao, ValorCobrado = servScanner.ValorPadrao, TempoEstimadoMinutosSnapshot = servScanner.TempoEstimadoMinutos },
            new OrdemServicoServico { OrdemServicoId = osPausada.Id, ServicoId = servCorreia.Id, Quantidade = 1, PrecoBase = servCorreia.ValorPadrao, ValorCobrado = servCorreia.ValorPadrao, TempoEstimadoMinutosSnapshot = servCorreia.TempoEstimadoMinutos },
            new OrdemServicoServico { OrdemServicoId = osAguardandoPecas.Id, ServicoId = servAlinhamento.Id, Quantidade = 1, PrecoBase = servAlinhamento.ValorPadrao, ValorCobrado = servAlinhamento.ValorPadrao, TempoEstimadoMinutosSnapshot = servAlinhamento.TempoEstimadoMinutos },
            new OrdemServicoServico { OrdemServicoId = osOrcamento.Id, ServicoId = servAlinhamento.Id, Quantidade = 1, PrecoBase = servAlinhamento.ValorPadrao, ValorCobrado = servAlinhamento.ValorPadrao, TempoEstimadoMinutosSnapshot = servAlinhamento.TempoEstimadoMinutos }
        );

        await context.SaveChangesAsync();
        logger?.LogInformation("Banco de dados populado com dados de demonstração (--seed-demo).");
    }
}
