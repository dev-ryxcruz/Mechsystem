using mechsystem.Models;

namespace mechsystem.Interfaces
{
    public interface IServicoRepository
    {
        Task<IEnumerable<Servico>> GetAllAsync(bool apenasAtivos = true);
        Task<IEnumerable<Servico>> GetInativosAsync();
        Task<Servico?> GetByIdAsync(int id);
        Task<Servico> AddAsync(Servico servico);
        Task UpdateAsync(Servico servico);
    }
}
