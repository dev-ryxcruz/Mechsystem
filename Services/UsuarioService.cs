using mechsystem.Data;
using mechsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace mechsystem.Services;

public class UsuarioService
{
    private readonly AppDbContext _context;

    public UsuarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Usuario>> GetUsuariosAsync()
    {
        return await _context.Usuarios.OrderBy(u => u.NomeCompleto).ToListAsync();
    }

    public async Task<Usuario?> GetUsuarioByIdAsync(int id)
    {
        return await _context.Usuarios.FindAsync(id);
    }

    public async Task<bool> IsUsernameTakenAsync(string username, int? excludeUserId = null)
    {
        var query = _context.Usuarios.Where(u => u.Username.ToLower() == username.ToLower());
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }

    private static readonly HashSet<string> SenhasFracasProibidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin123",
        "12345678",
        "123456789",
        "password",
        "password123",
        "senha123",
        "qwerty123",
        "11111111",
        "00000000",
        "mechsystem"
    };

    public static void ValidarPoliticaSenha(string password, string? username = null)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Trim().Length < 8)
        {
            throw new InvalidOperationException("A senha deve conter pelo menos 8 caracteres.");
        }

        var senhaLimpa = password.Trim();

        if (!string.IsNullOrWhiteSpace(username) &&
            string.Equals(senhaLimpa, username.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("A senha não pode ser igual ao nome de usuário.");
        }

        if (SenhasFracasProibidas.Contains(senhaLimpa))
        {
            throw new InvalidOperationException("A senha informada é muito fraca ou comum. Escolha uma senha mais segura.");
        }
    }

    public async Task CreateUsuarioAsync(Usuario usuario, string password)
    {
        ValidarPoliticaSenha(password, usuario.Username);

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        usuario.DataCriacao = DateTime.UtcNow;

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUsuarioAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePasswordAsync(int userId, string newPassword)
    {
        var user = await _context.Usuarios.FindAsync(userId)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        ValidarPoliticaSenha(newPassword, user.Username);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _context.Usuarios.Update(user);
        await _context.SaveChangesAsync();
    }
}
