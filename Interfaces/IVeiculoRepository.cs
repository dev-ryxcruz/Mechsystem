using mechsystem.Models;

namespace mechsystem.Interfaces
{
    public interface IVeiculoRepository
    {
        Task<List<Veiculo>> Listar();
        Task<List<Veiculo>> ListarInativos();
        Task<Veiculo?> BuscarPorId(int id);
        Task Salvar(Veiculo veiculo);
        Task Atualizar(Veiculo veiculo);
        Task Deletar(int id);
        Task Reativar(int id);
    }
}
