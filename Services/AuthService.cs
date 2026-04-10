using mechsystem.Data;
using mechsystem.Interfaces;
using mechsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace mechsystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ValidateUserAsync(string username, string password)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == username && u.Ativo);

            if (usuario == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
                return null;

            return usuario;
        }

        public async Task<Usuario?> GetUsuarioByUsernameAsync(string username)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == username && u.Ativo);
        }
    }
}
