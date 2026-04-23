using mechsystem.Models;

namespace mechsystem.Interfaces
{
    public interface IPecaRepository
    {
        Task<IEnumerable<Peca>> GetAllAsync(bool apenasAtivas = true);
        Task<Peca?> GetByIdAsync(int id);
        Task<Peca?> GetBySkuAsync(string sku);
        Task<Peca> AddAsync(Peca peca);
        Task UpdateAsync(Peca peca);
        Task DeleteAsync(int id);
        Task<IEnumerable<MovimentacaoEstoque>> GetHistoricoAsync(int pecaId);

        // Movimentações
        Task RegistrarEntradaAsync(int pecaId, int quantidade, decimal? novoPrecoCusto, string? referencia, int usuarioId);
        Task RegistrarSaidaAsync(int pecaId, int quantidade, string? referencia, int usuarioId);
        Task RegistrarAjusteAsync(int pecaId, int novaQuantidade, string motivo, int usuarioId);
    }
}
