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
        public DbSet<Configuracao> Configuracoes { get; set; }
        public DbSet<Peca> Pecas { get; set; }
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
        public DbSet<OrdemServicoPeca> OrdemServicoPecas { get; set; }
        public DbSet<OrdemServicoServico> OrdemServicoServicos { get; set; }
        public DbSet<ContatoOS> ContatosOS { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Cpf)
                .HasConversion(
                    v => string.IsNullOrEmpty(v) ? v : new string(v.Where(char.IsDigit).ToArray()),
                    v => v
                );

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Telefone)
                .HasConversion(
                    v => string.IsNullOrEmpty(v) ? v : new string(v.Where(char.IsDigit).ToArray()),
                    v => v
                );

            modelBuilder.Entity<Configuracao>()
                .Property(c => c.Telefone)
                .HasConversion(
                    v => string.IsNullOrEmpty(v) ? v : new string(v.Where(char.IsDigit).ToArray()),
                    v => v
                );

            modelBuilder.Entity<Configuracao>()
                .Property(c => c.WhatsApp)
                .HasConversion(
                    v => string.IsNullOrEmpty(v) ? v : new string(v.Where(char.IsDigit).ToArray()),
                    v => v
                );

            modelBuilder.Entity<Configuracao>()
                .Property(c => c.Cnpj)
                .HasConversion(
                    v => string.IsNullOrEmpty(v) ? v : new string(v.Where(char.IsDigit).ToArray()),
                    v => v
                );

            modelBuilder.Entity<Usuario>().HasIndex(u => u.Username).IsUnique();

            // Peca: SKU único
            modelBuilder.Entity<Peca>().HasIndex(p => p.Sku).IsUnique();

            // MovimentacaoEstoque: FKs
            modelBuilder.Entity<MovimentacaoEstoque>()
                .HasOne(m => m.Peca)
                .WithMany(p => p.Movimentacoes)
                .HasForeignKey(m => m.PecaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimentacaoEstoque>()
                .HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrdemServicoPeca: FKs
            modelBuilder.Entity<OrdemServicoPeca>()
                .HasOne(op => op.OrdemServico)
                .WithMany(os => os.PecasUtilizadas)
                .HasForeignKey(op => op.OrdemServicoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrdemServicoPeca>()
                .HasOne(op => op.Peca)
                .WithMany()
                .HasForeignKey(op => op.PecaId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrdemServicoServico: FKs
            modelBuilder.Entity<OrdemServicoServico>()
                .HasOne(os => os.OrdemServico)
                .WithMany(o => o.ServicosAExecutarList)
                .HasForeignKey(os => os.OrdemServicoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrdemServicoServico>()
                .HasOne(os => os.Servico)
                .WithMany()
                .HasForeignKey(os => os.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);

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