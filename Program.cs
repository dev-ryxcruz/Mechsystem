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
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention());



// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IPecaRepository, PecaRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<ConfiguracaoService>();
builder.Services.AddScoped<EstoqueService>();
builder.Services.AddScoped<OrdemServicoService>();

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
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var startupLogger = loggerFactory.CreateLogger("AdminBootstrap");

    await db.Database.MigrateAsync();

    var resetAdmin = args.Contains("--reset-admin");
    var admin = await db.Usuarios.FirstOrDefaultAsync(u => u.Username == "admin");
    var configuredAdminPassword = builder.Configuration["Admin:InitialPassword"];

    static string GerarSenhaForteAleatoria()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%&*";
        return System.Security.Cryptography.RandomNumberGenerator.GetString(chars, 16);
    }

    if (admin == null)
    {
        var senhaInicial = !string.IsNullOrWhiteSpace(configuredAdminPassword)
            ? configuredAdminPassword
            : GerarSenhaForteAleatoria();

        db.Usuarios.Add(new mechsystem.Models.Usuario
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(senhaInicial),
            NomeCompleto = "Administrador",
            Ativo = true,
            Perfil = mechsystem.Models.PerfilUsuario.Administrador,
            DataCriacao = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        if (string.IsNullOrWhiteSpace(configuredAdminPassword))
        {
            startupLogger.LogWarning("Usuário 'admin' criado no primeiro boot. Senha temporária gerada automaticamente (altere imediatamente): {SenhaInicial}", senhaInicial);
        }
        else
        {
            startupLogger.LogInformation("Usuário 'admin' criado utilizando a configuração Admin:InitialPassword.");
        }
    }
    else
    {
        // Garante que o admin original sempre tenha Perfil Administrador
        var mudou = false;
        if (admin.Perfil != mechsystem.Models.PerfilUsuario.Administrador)
        {
            admin.Perfil = mechsystem.Models.PerfilUsuario.Administrador;
            mudou = true;
        }

        // Se o admin veio da migration histórica inicial (hash legado), rotaciona a senha no primeiro boot
        const string legacySeedHashPrefix = "$2a$11$uegyC1YafmGAkp2uMJmfje/";
        var possuiHashLegado = admin.PasswordHash != null && admin.PasswordHash.StartsWith(legacySeedHashPrefix, StringComparison.Ordinal);

        if (resetAdmin || possuiHashLegado)
        {
            var novaSenha = !string.IsNullOrWhiteSpace(configuredAdminPassword)
                ? configuredAdminPassword
                : GerarSenhaForteAleatoria();

            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
            admin.Ativo = true;
            mudou = true;

            if (string.IsNullOrWhiteSpace(configuredAdminPassword))
            {
                startupLogger.LogWarning("Senha inicial/temporária do usuário 'admin' gerada (altere imediatamente): {NovaSenha}", novaSenha);
            }
            else
            {
                startupLogger.LogInformation("Senha do usuário 'admin' definida utilizando Admin:InitialPassword.");
            }
        }

        if (mudou)
        {
            await db.SaveChangesAsync();
        }
    }

    // Popula dados de demonstração apenas se solicitado via flag --seed-demo
    if (args.Contains("--seed-demo"))
    {
        var seederLogger = loggerFactory.CreateLogger("DatabaseSeeder");
        await mechsystem.Data.DatabaseSeeder.SeedAsync(db, seederLogger);
    }
}

app.Run();
