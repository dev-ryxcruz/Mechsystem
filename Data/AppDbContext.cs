using Microsoft.EntityFrameworkCore;
using mechsystem.Models;

namespace mechsystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<OrdemServico> OrdensServico { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Vistoria> Vistorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>().HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<Usuario>().HasData(new Usuario
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "$2a$11$uegyC1YafmGAkp2uMJmfje/iz6TbLTTiqJgps2rWvCNQFzymrQTza",
                NomeCompleto = "Administrador",
                Ativo = true,
                DataCriacao = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}