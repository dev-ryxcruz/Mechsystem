using mechsystem.Models;

namespace mechsystem.Interfaces
{
    public interface IVeiculoRepository
    {
        Task<List<Veiculo>> Listar();
        Task<List<Veiculo>> ListarInativos();
        Task<(List<Veiculo> Itens, int TotalCount)> ListarPaginadoAsync(bool inativos, string? termoBusca, int pagina, int tamanhoPagina);
        Task<Veiculo?> BuscarPorId(int id);
        Task Salvar(Veiculo veiculo);
        Task Atualizar(Veiculo veiculo);
    }
}
