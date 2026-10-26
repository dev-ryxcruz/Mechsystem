using mechsystem.Models;

namespace mechsystem.Interfaces
{
    public interface IClienteRepository
    {
        Task<List<Cliente>> Listar();
        Task<List<Cliente>> ListarInativos();
        Task<Cliente?> BuscarPorId(int id);
        Task Salvar(Cliente cliente);
        Task Atualizar(Cliente cliente);
        Task Deletar(int id);
        Task Reativar(int id);
    }
}