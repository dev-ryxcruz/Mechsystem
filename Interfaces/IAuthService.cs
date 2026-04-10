using mechsystem.Models;

namespace mechsystem.Interfaces
{
    public interface IAuthService
    {
        Task<Usuario?> ValidateUserAsync(string username, string password);
        Task<Usuario?> GetUsuarioByUsernameAsync(string username);
    }
}
