using mechsystem.Models;

namespace mechsystem.Interfaces;

public record UsuarioContexto(int UsuarioId, PerfilUsuario Perfil, string NomeUsuario);

public interface ICurrentUserService
{
    Task<UsuarioContexto> ObterContextoAtualAsync();
}
