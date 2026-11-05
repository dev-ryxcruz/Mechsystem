using mechsystem.Models;

namespace mechsystem.Interfaces
{
    public interface IPecaRepository
    {
        Task<IEnumerable<Peca>> GetAllAsync(bool apenasAtivas = true);
        Task<IEnumerable<Peca>> GetInativasAsync();
        Task<(List<Peca> Itens, int TotalCount)> GetPaginadoAsync(bool inativas, string? termoBusca, int pagina, int tamanhoPagina);
        Task<Peca?> GetByIdAsync(int id);
        Task<Peca?> GetBySkuAsync(string sku);
        Task<Peca> AddAsync(Peca peca);
        Task UpdateAsync(Peca peca);

        // Movimentações de Estoque com Auditoria
        Task RegistrarEntradaAsync(int pecaId, int quantidade, decimal? novoPrecoCusto, string? referencia, int usuarioId);
        Task RegistrarSaidaAsync(int pecaId, int quantidade, string? referencia, int usuarioId);
        Task RegistrarAjusteAsync(int pecaId, int novaQuantidade, string motivo, int usuarioId);
        Task<IEnumerable<MovimentacaoEstoque>> GetHistoricoAsync(int pecaId);
    }
}
