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
