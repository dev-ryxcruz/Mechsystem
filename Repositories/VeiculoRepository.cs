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
            // O Include traz os dados do Cliente dono do veículo automaticamente
            return await _context.Veiculos.Include(v => v.Cliente).ToListAsync();
        }

        public async Task<Veiculo?> BuscarPorId(int id)
        {
            return await _context.Veiculos.FindAsync(id);
        }

        public async Task Salvar(Veiculo veiculo)
        {
            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();
        }

        public async Task Atualizar(Veiculo veiculo)
        {
            _context.Veiculos.Update(veiculo);
            await _context.SaveChangesAsync();
        }

        public async Task Deletar(int id)
        {
            var veiculo = await BuscarPorId(id);
            if (veiculo != null)
            {
                _context.Veiculos.Remove(veiculo);
                await _context.SaveChangesAsync();
            }
        }
    }
}