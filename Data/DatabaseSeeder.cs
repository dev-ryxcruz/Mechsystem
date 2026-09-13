using mechsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace mechsystem.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Corrige clientes antigos que possam ter ficado inativos pela migration recente
            var clientesInativos = await context.Clientes.IgnoreQueryFilters().Where(c => !c.Ativo).ToListAsync();
            bool hasUpdates = false;
            foreach (var c in clientesInativos)
            {
                c.Ativo = true;
                hasUpdates = true;
            }
            if (hasUpdates)
            {
                await context.SaveChangesAsync();
                Console.WriteLine(">>> Clientes inativos corrigidos para ativos.");
            }

            // Insere dados de teste se a base não tiver o cliente de teste
            if (!await context.Clientes.AnyAsync(c => c.Nome == "João Silva"))
            {
                var cliente1 = new Cliente { Nome = "João Silva", Cpf = "11111111111", Telefone = "11999999999", Email = "joao@email.com", Endereco = "Rua A, 123", Ativo = true };
                var cliente2 = new Cliente { Nome = "Maria Oliveira", Cpf = "22222222222", Telefone = "11888888888", Email = "maria@email.com", Endereco = "Av B, 456", Ativo = true };
                
                context.Clientes.AddRange(cliente1, cliente2);
                await context.SaveChangesAsync();

                var veiculo1 = new Veiculo { Placa = "ABC1234", Marca = "Ford", Modelo = "Ka", Ano = 2018, Cor = "Branco", Quilometragem = 50000, ClienteId = cliente1.Id };
                var veiculo2 = new Veiculo { Placa = "XYZ9876", Marca = "Chevrolet", Modelo = "Onix", Ano = 2020, Cor = "Prata", Quilometragem = 30000, ClienteId = cliente2.Id };
                
                context.Veiculos.AddRange(veiculo1, veiculo2);

                var peca1 = new Peca { Nome = "Filtro de Óleo", Sku = "FILT01", Marca = "Bosch", PrecoCusto = 15.00m, PrecoVenda = 35.00m, EstoqueAtual = 10, EstoqueMinimo = 5, Ativo = true };
                var peca2 = new Peca { Nome = "Pastilha de Freio", Sku = "FREI01", Marca = "Cobreq", PrecoCusto = 45.00m, PrecoVenda = 90.00m, EstoqueAtual = 20, EstoqueMinimo = 4, Ativo = true };

                context.Pecas.AddRange(peca1, peca2);

                var servico1 = new Servico { Nome = "Troca de Óleo", Descricao = "Troca de óleo do motor e filtro", ValorPadrao = 150.00m, TempoEstimadoMinutos = 60 };
                var servico2 = new Servico { Nome = "Revisão de Freios", Descricao = "Verificação e troca de pastilhas", ValorPadrao = 200.00m, TempoEstimadoMinutos = 120 };

                context.Servicos.AddRange(servico1, servico2);

                await context.SaveChangesAsync();
                
                Console.WriteLine(">>> Banco de dados populado com dados de teste!");
            }
        }
    }
}

