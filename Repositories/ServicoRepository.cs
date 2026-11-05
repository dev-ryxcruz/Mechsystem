using mechsystem.Data;
using mechsystem.Interfaces;
using mechsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace mechsystem.Repositories
{
    public class ServicoRepository : IServicoRepository
    {
        private readonly AppDbContext _context;

        public ServicoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Servico>> GetAllAsync(bool apenasAtivos = true)
        {
            var query = _context.Servicos.AsQueryable();
            if (apenasAtivos)
                query = query.Ativos();
            return await query.OrderBy(s => s.Nome).ToListAsync();
        }

        public async Task<IEnumerable<Servico>> GetInativosAsync()
        {
            return await _context.Servicos
                .Inativos()
                .OrderBy(s => s.Nome)
                .ToListAsync();
        }

        public async Task<(List<Servico> Itens, int TotalCount)> GetPaginadoAsync(bool inativos, string? termoBusca, int pagina, int tamanhoPagina)
        {
            if (pagina < 1) pagina = 1;
            if (tamanhoPagina <= 0) tamanhoPagina = 10;

            var query = inativos
                ? _context.Servicos.Inativos()
                : _context.Servicos.Ativos();

            if (!string.IsNullOrWhiteSpace(termoBusca))
            {
                var termo = termoBusca.Trim().ToLower();
                query = query.Where(s =>
                    s.Nome.ToLower().Contains(termo) ||
                    (s.Descricao != null && s.Descricao.ToLower().Contains(termo)));
            }

            var totalCount = await query.CountAsync();
            var itens = await query
                .OrderBy(s => s.Nome)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (itens, totalCount);
        }

        public async Task<Servico?> GetByIdAsync(int id)
        {
            return await _context.Servicos.FindAsync(id);
        }

        public async Task<Servico> AddAsync(Servico servico)
        {
            _context.Servicos.Add(servico);
            await _context.SaveChangesAsync();
            return servico;
        }

        public async Task UpdateAsync(Servico servico)
        {
            _context.Entry(servico).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
