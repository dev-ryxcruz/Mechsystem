using mechsystem.Models;

namespace mechsystem.Interfaces
{
    public interface IClienteRepository
    {
        Task<List<Cliente>> Listar();
        Task<List<Cliente>> ListarInativos();
        Task<(List<Cliente> Itens, int TotalCount)> ListarPaginadoAsync(bool inativos, string? termoBusca, int pagina, int tamanhoPagina);
        Task<Cliente?> BuscarPorId(int id);
        Task Salvar(Cliente cliente);
        Task Atualizar(Cliente cliente);
    }
}
