using System.Security.Claims;
using mechsystem.Interfaces;
using mechsystem.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace mechsystem.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public CurrentUserService(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<UsuarioContexto> ObterContextoAtualAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var usuarioId))
        {
            throw new UnauthorizedAccessException("Identificador de usuário inválido.");
        }

        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
        if (!Enum.TryParse<PerfilUsuario>(roleClaim, out var perfil))
        {
            throw new UnauthorizedAccessException("Perfil de usuário inválido.");
        }

        var nomeUsuario = user.FindFirst(ClaimTypes.GivenName)?.Value
            ?? user.Identity?.Name
            ?? "Usuário";

        return new UsuarioContexto(usuarioId, perfil, nomeUsuario);
    }
}
