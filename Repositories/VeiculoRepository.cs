using mechsystem.Models;
using mechsystem.Interfaces;
using Microsoft.EntityFrameworkCore;
using mechsystem.Data;

namespace mechsystem.Repositories
{
    public class VeiculoRepository : IVeiculoRepository
    {
        private readonly AppDbContext _context;

        public VeiculoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Veiculo>> Listar()
        {
            return await _context.Veiculos
                .Include(v => v.Cliente)
                .Ativos()
                .OrderBy(v => v.Placa)
                .ToListAsync();
        }

        public async Task<List<Veiculo>> ListarInativos()
        {
            return await _context.Veiculos
                .Include(v => v.Cliente)
                .Inativos()
                .OrderBy(v => v.Placa)
                .ToListAsync();
        }

        public async Task<Veiculo?> BuscarPorId(int id)
        {
            return await _context.Veiculos.FindAsync(id);
        }

        public async Task Salvar(Veiculo veiculo)
        {
            veiculo.Placa = NormalizarPlaca(veiculo.Placa);

            var existente = await _context.Veiculos
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Placa == veiculo.Placa);

            if (existente != null)
            {
                if (!existente.Ativo)
                {
                    var descricao = $"{existente.Marca} {existente.Modelo}".Trim();
                    throw new InvalidOperationException(
                        $"Já existe um veículo inativo com esta placa: {(string.IsNullOrWhiteSpace(descricao) ? existente.Placa : descricao)}. Reative-o em Veículos > Inativos.");
                }
                throw new InvalidOperationException("Já existe um veículo cadastrado com esta placa.");
            }

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();
        }

        public async Task Atualizar(Veiculo veiculo)
        {
            veiculo.Placa = NormalizarPlaca(veiculo.Placa);

            var existente = await _context.Veiculos
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Placa == veiculo.Placa && v.Id != veiculo.Id);

            if (existente != null)
            {
                if (!existente.Ativo)
                {
                    var descricao = $"{existente.Marca} {existente.Modelo}".Trim();
                    throw new InvalidOperationException(
                        $"Já existe um veículo inativo com esta placa: {(string.IsNullOrWhiteSpace(descricao) ? existente.Placa : descricao)}. Reative-o em Veículos > Inativos.");
                }
                throw new InvalidOperationException("Já existe outro veículo cadastrado com esta placa.");
            }

            _context.Veiculos.Update(veiculo);
            await _context.SaveChangesAsync();
        }

        private static string NormalizarPlaca(string? placa)
        {
            return string.IsNullOrEmpty(placa)
                ? string.Empty
                : placa.Replace("-", "").Trim().ToUpperInvariant();
        }
    }
}
