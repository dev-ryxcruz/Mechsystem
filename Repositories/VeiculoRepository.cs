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

        public async Task<(List<Veiculo> Itens, int TotalCount)> ListarPaginadoAsync(bool inativos, string? termoBusca, int pagina, int tamanhoPagina)
        {
            if (pagina < 1) pagina = 1;
            if (tamanhoPagina <= 0) tamanhoPagina = 10;

            var query = (inativos ? _context.Veiculos.Inativos() : _context.Veiculos.Ativos())
                .Include(v => v.Cliente)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(termoBusca))
            {
                var termo = termoBusca.Trim().ToLower();
                var placaNorm = termoBusca.Replace("-", "").Trim().ToUpperInvariant();

                query = query.Where(v =>
                    (!string.IsNullOrEmpty(placaNorm) && v.Placa.Contains(placaNorm)) ||
                    (v.Marca != null && v.Marca.ToLower().Contains(termo)) ||
                    (v.Modelo != null && v.Modelo.ToLower().Contains(termo)) ||
                    (v.Cliente != null && v.Cliente.Nome.ToLower().Contains(termo)));
            }

            var totalCount = await query.CountAsync();
            var itens = await query
                .OrderBy(v => v.Placa)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (itens, totalCount);
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
