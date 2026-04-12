using System.Diagnostics.CodeAnalysis;
using mechsystem.Components;
using Microsoft.EntityFrameworkCore;
using mechsystem.Data;
using mechsystem.Interfaces;
using mechsystem.Repositories;
using mechsystem.Services;
using mechsystem.Endpoints;
using Microsoft.AspNetCore.Authentication.Cookies;




var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));



// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UsuarioService>();

// Authentication via Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/api/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets().AllowAnonymous();

// Auth endpoints (login/logout via HTTP POST)
app.MapAuthEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Seed / Reset admin user
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();

    var resetAdmin = args.Contains("--reset-admin");
    var admin = await db.Usuarios.FirstOrDefaultAsync(u => u.Username == "admin");

    if (admin == null)
    {
        // Create default admin
        db.Usuarios.Add(new mechsystem.Models.Usuario
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            NomeCompleto = "Administrador",
            Ativo = true,
            Perfil = mechsystem.Models.PerfilUsuario.Administrador
        });
        await db.SaveChangesAsync();
        Console.WriteLine(">>> Usuário admin criado. Senha: admin123");
    }
    else
    {
        // Força que o admin original sempre tenha Perfil Administrador pós-migração
        var mudou = false;
        if (admin.Perfil != mechsystem.Models.PerfilUsuario.Administrador)
        {
            admin.Perfil = mechsystem.Models.PerfilUsuario.Administrador;
            mudou = true;
        }

        if (resetAdmin)
        {
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
            admin.Ativo = true;
            mudou = true;
            Console.WriteLine(">>> Senha do admin resetada para: admin123");
        }

        if (mudou)
        {
            await db.SaveChangesAsync();
        }
    }
}

app.Run();
