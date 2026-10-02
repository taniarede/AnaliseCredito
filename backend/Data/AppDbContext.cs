using AnaliseCredito.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AnaliseCredito.Api.Data;

// Acesso à base de dados com Entity Framework Core.
// O esquema é criado pelos scripts da pasta /database (fonte única da verdade);
// aqui apenas se diz ao EF como as classes correspondem às tabelas.
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<PedidoCredito> PedidosCredito => Set<PedidoCredito>();
    public DbSet<MotivoDecisao> MotivosDecisao => Set<MotivoDecisao>();
    public DbSet<HistoricoEstado> HistoricoEstados => Set<HistoricoEstado>();
    public DbSet<Simulacao> Simulacoes => Set<Simulacao>();

    public IQueryable<PedidoCredito> PedidosCompletos() =>
        PedidosCredito
            .Include(p => p.Motivos)
            .Include(p => p.Historico)
            .AsSplitQuery();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("Clientes");
            e.HasKey(c => c.Id);
            e.Property(c => c.Nif).HasColumnType("char(9)").IsRequired();
            e.HasIndex(c => c.Nif).IsUnique();
            e.Property(c => c.DataCriacao).HasColumnType("datetime2(0)");
        });

        modelBuilder.Entity<PedidoCredito>(e =>
        {
            e.ToTable("PedidosCredito");
            e.HasKey(p => p.Id);

            e.Property(p => p.NumeroPedido)
                .HasColumnType("varchar(20)")
                .ValueGeneratedOnAddOrUpdate();   // calculada pelo SQL Server

            e.Property(p => p.NifSubmetido).HasMaxLength(20).IsRequired();
            e.Property(p => p.RendimentoMensalLiquido).HasPrecision(18, 2);
            e.Property(p => p.PrestacoesAtuais).HasPrecision(18, 2);
            e.Property(p => p.ValorPretendido).HasPrecision(18, 2);
            e.Property(p => p.SituacaoProfissional).HasMaxLength(30).IsRequired();
            e.Property(p => p.PrestacaoEstimada).HasPrecision(18, 2);
            e.Property(p => p.TaxaEsforco).HasPrecision(18, 2);
            e.Property(p => p.IdadeFinalContrato).HasPrecision(18, 2);

            // Enums (base byte) são guardados como TINYINT = Id da tabela Estados
            e.Property(p => p.EstadoAutomatico).HasColumnName("EstadoAutomaticoId");
            e.Property(p => p.EstadoAtual).HasColumnName("EstadoAtualId");

            e.Property(p => p.VersaoRegras).HasColumnType("varchar(10)").IsRequired();
            e.Property(p => p.DataPedido).HasColumnType("datetime2(0)");
            e.Property(p => p.DataAtualizacao).HasColumnType("datetime2(0)");

            e.HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.ClienteId);

            e.HasMany(p => p.Motivos)
                .WithOne()
                .HasForeignKey(m => m.PedidoId);

            e.HasMany(p => p.Historico)
                .WithOne()
                .HasForeignKey(h => h.PedidoId);
        });

        modelBuilder.Entity<MotivoDecisao>(e =>
        {
            e.ToTable("MotivosDecisao");
            e.HasKey(m => m.Id);
            e.Property(m => m.CodigoRegra).HasColumnType("varchar(10)").IsRequired();
            e.Property(m => m.Estado).HasColumnName("EstadoId");
            e.Property(m => m.Descricao).HasMaxLength(300).IsRequired();
            e.Property(m => m.Campo).HasColumnType("varchar(30)");
        });

        modelBuilder.Entity<HistoricoEstado>(e =>
        {
            e.ToTable("HistoricoEstados");
            e.HasKey(h => h.Id);
            e.Property(h => h.EstadoAnterior).HasColumnName("EstadoAnteriorId");
            e.Property(h => h.EstadoNovo).HasColumnName("EstadoNovoId");
            e.Property(h => h.DataAlteracao).HasColumnType("datetime2(0)");
            e.Property(h => h.Utilizador).HasMaxLength(100).IsRequired();
            e.Property(h => h.Observacao).HasMaxLength(500);
        });

        modelBuilder.Entity<Simulacao>(e =>
        {
            e.ToTable("Simulacoes");
            e.HasKey(s => s.Id);
            e.Property(s => s.NifSubmetido).HasMaxLength(20).IsRequired();
            e.Property(s => s.RendimentoMensalLiquido).HasPrecision(18, 2);
            e.Property(s => s.PrestacoesAtuais).HasPrecision(18, 2);
            e.Property(s => s.ValorPretendido).HasPrecision(18, 2);
            e.Property(s => s.SituacaoProfissional).HasMaxLength(30).IsRequired();
            e.Property(s => s.PrestacaoEstimada).HasPrecision(18, 2);
            e.Property(s => s.TaxaEsforco).HasPrecision(18, 2);
            e.Property(s => s.IdadeFinalContrato).HasPrecision(18, 2);
            e.Property(s => s.Decisao).HasColumnName("DecisaoId");   // TINYINT = Id da tabela Estados
            e.Property(s => s.CodigosRegras).HasColumnType("varchar(100)");
            e.Property(s => s.VersaoRegras).HasColumnType("varchar(10)").IsRequired();
            e.Property(s => s.DataSimulacao).HasColumnType("datetime2(0)");
        });
    }
}
