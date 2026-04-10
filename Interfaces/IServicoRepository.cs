using mechsystem.Models;

namespace mechsystem.Interfaces
{
    public interface IServicoRepository
    {
        Task<IEnumerable<Servico>> GetAllAsync();
        Task<Servico?> GetByIdAsync(int id);
        Task<Servico> AddAsync(Servico servico);
        Task UpdateAsync(Servico servico);
        Task DeleteAsync(int id);
    }
}
