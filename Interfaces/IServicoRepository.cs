using mechsystem.Models;

namespace mechsystem.Interfaces
{
    public interface IServicoRepository
    {
        Task<IEnumerable<Servico>> GetAllAsync(bool apenasAtivos = true);
        Task<IEnumerable<Servico>> GetInativosAsync();
        Task<(List<Servico> Itens, int TotalCount)> GetPaginadoAsync(bool inativos, string? termoBusca, int pagina, int tamanhoPagina);
        Task<Servico?> GetByIdAsync(int id);
        Task<Servico> AddAsync(Servico servico);
        Task UpdateAsync(Servico servico);
    }
}
