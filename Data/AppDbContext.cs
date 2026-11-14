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
        public DbSet<ApontamentoTempo> ApontamentosTempo { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            configurationBuilder.Properties<DateTime>()
                .HaveConversion<UtcDateTimeConverter>();

            configurationBuilder.Properties<DateTime?>()
                .HaveConversion<NullableUtcDateTimeConverter>();
        }

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
                .HasIndex(c => c.Cpf)
                .IsUnique();

            modelBuilder.Entity<Veiculo>()
                .Property(v => v.Placa)
                .HasConversion(
                    v => string.IsNullOrEmpty(v) ? v : v.Replace("-", "").Trim().ToUpperInvariant(),
                    v => v
                );

            modelBuilder.Entity<Veiculo>()
                .HasIndex(v => v.Placa)
                .IsUnique();

            modelBuilder.Entity<OrdemServico>()
                .HasIndex(o => o.TokenAcompanhamento)
                .IsUnique();

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

            // Peca: SKU único e controle de concorrência otimista (PostgreSQL xmin)
            modelBuilder.Entity<Peca>().HasIndex(p => p.Sku).IsUnique();
            modelBuilder.Entity<Peca>()
                .Property(p => p.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

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

            // Soft-delete padronizado (IInativavel) — sem HasQueryFilter global para preservar histórico
            modelBuilder.Entity<Servico>()
                .Property(s => s.Ativo)
                .HasDefaultValue(true);

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.InativadoPorUsuario)
                .WithMany()
                .HasForeignKey(c => c.InativadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Veiculo>()
                .HasOne(v => v.InativadoPorUsuario)
                .WithMany()
                .HasForeignKey(v => v.InativadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Peca>()
                .HasOne(p => p.InativadoPorUsuario)
                .WithMany()
                .HasForeignKey(p => p.InativadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Servico>()
                .HasOne(s => s.InativadoPorUsuario)
                .WithMany()
                .HasForeignKey(s => s.InativadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.CanceladaPorUsuario)
                .WithMany()
                .HasForeignKey(o => o.CanceladaPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.MecanicoResponsavel)
                .WithMany()
                .HasForeignKey(o => o.MecanicoResponsavelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.AprovadoPorUsuario)
                .WithMany()
                .HasForeignKey(o => o.AprovadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdemServicoServico>()
                .HasOne(os => os.Mecanico)
                .WithMany()
                .HasForeignKey(os => os.MecanicoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ApontamentoTempo: FKs Restrict e índice parcial por (MecanicoId, Fim nulo)
            modelBuilder.Entity<ApontamentoTempo>()
                .HasOne(a => a.OrdemServico)
                .WithMany(o => o.ApontamentosTempo)
                .HasForeignKey(a => a.OrdemServicoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApontamentoTempo>()
                .HasOne(a => a.OrdemServicoServico)
                .WithMany()
                .HasForeignKey(a => a.OrdemServicoServicoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApontamentoTempo>()
                .HasOne(a => a.Mecanico)
                .WithMany()
                .HasForeignKey(a => a.MecanicoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApontamentoTempo>()
                .HasIndex(a => new { a.MecanicoId, a.Fim })
                .HasFilter("fim IS NULL");

            // Pagamento: FKs Restrict
            modelBuilder.Entity<Pagamento>()
                .HasOne(p => p.OrdemServico)
                .WithMany(o => o.Pagamentos)
                .HasForeignKey(p => p.OrdemServicoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pagamento>()
                .HasOne(p => p.RegistradoPorUsuario)
                .WithMany()
                .HasForeignKey(p => p.RegistradoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices compostos/parciais e CHECK constraints (PostgreSQL)
            modelBuilder.Entity<OrdemServico>()
                .HasIndex(o => new { o.Status, o.DataConclusao });

            modelBuilder.Entity<OrdemServico>()
                .HasIndex(o => new { o.Status, o.DataEmissao });

            modelBuilder.Entity<OrdemServico>()
                .HasIndex(o => new { o.Status, o.MecanicoResponsavelId });

            modelBuilder.Entity<OrdemServico>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint("ck_ordens_servico_nivel_combustivel", "nivel_combustivel_entrada BETWEEN 1 AND 5");
                    t.HasCheckConstraint("ck_ordens_servico_valor_desconto", "valor_desconto >= 0");
                });

            modelBuilder.Entity<OrdemServicoPeca>()
                .HasIndex(op => new { op.PecaId, op.OrdemServicoId })
                .HasFilter("origem = 0 AND status_item = 1 AND aplicada = false");

            modelBuilder.Entity<MovimentacaoEstoque>()
                .HasIndex(m => new { m.DataHora, m.Tipo });
        }
    }

    public class UtcDateTimeConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(
                v => v.Kind == DateTimeKind.Utc
                    ? v
                    : (v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : DateTime.SpecifyKind(v, DateTimeKind.Utc)),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }

    public class NullableUtcDateTimeConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>
    {
        public NullableUtcDateTimeConverter()
            : base(
                v => !v.HasValue
                    ? null
                    : (v.Value.Kind == DateTimeKind.Utc
                        ? v.Value
                        : (v.Value.Kind == DateTimeKind.Local ? v.Value.ToUniversalTime() : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc))),
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null)
        {
        }
    }
}
