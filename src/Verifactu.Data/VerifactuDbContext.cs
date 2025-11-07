using Microsoft.EntityFrameworkCore;
using Verifactu.Data.Entities;

namespace Verifactu.Data;

/// <summary>
/// Contexto de base de datos para Verifactu
/// Soporta múltiples proveedores: SQLite, SQL Server, PostgreSQL
/// </summary>
public class VerifactuDbContext : DbContext
{
    /// <summary>
    /// Registros de facturación
    /// </summary>
    public DbSet<RegistroFacturacionEntity> RegistrosFacturacion { get; set; }

    /// <summary>
    /// Constructor para inyección de dependencias
    /// </summary>
    public VerifactuDbContext(DbContextOptions<VerifactuDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Constructor sin parámetros para migraciones
    /// </summary>
    public VerifactuDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de RegistroFacturacionEntity
        modelBuilder.Entity<RegistroFacturacionEntity>(entity =>
        {
            // Clave primaria
            entity.HasKey(e => e.Id);

            // Índices únicos
            entity.HasIndex(e => new { e.Serie, e.Numero })
                .IsUnique()
                .HasDatabaseName("IX_RegistrosFacturacion_Serie_Numero");

            // Índices para consultas frecuentes
            entity.HasIndex(e => e.FechaHoraExpedicionUTC)
                .HasDatabaseName("IX_RegistrosFacturacion_FechaHoraExpedicionUTC");

            entity.HasIndex(e => e.EstadoEnvio)
                .HasDatabaseName("IX_RegistrosFacturacion_EstadoEnvio");

            entity.HasIndex(e => e.Huella)
                .HasDatabaseName("IX_RegistrosFacturacion_Huella");

            entity.HasIndex(e => e.NifEmisor)
                .HasDatabaseName("IX_RegistrosFacturacion_NifEmisor");

            // Propiedades requeridas
            entity.Property(e => e.Serie)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Numero)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.FechaHoraExpedicionUTC)
                .IsRequired();

            entity.Property(e => e.Huella)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.HuellaAnterior)
                .HasMaxLength(64);

            entity.Property(e => e.EstadoEnvio)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.XmlFirmado)
                .IsRequired();

            entity.Property(e => e.DescripcionErrorAEAT)
                .HasMaxLength(1000);

            entity.Property(e => e.CSV)
                .HasMaxLength(100);

            entity.Property(e => e.NifEmisor)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.NombreEmisor)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.ImporteTotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.CuotaTotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.RefExterna)
                .HasMaxLength(100);

            entity.Property(e => e.FechaCreacion)
                .IsRequired();

            entity.Property(e => e.FechaModificacion)
                .IsRequired();

            // Valores por defecto
            entity.Property(e => e.Reintentos)
                .HasDefaultValue(0);

            entity.Property(e => e.Anulado)
                .HasDefaultValue(false);
        });
    }

    /// <summary>
    /// Sobrescribe SaveChanges para auditoría automática
    /// </summary>
    public override int SaveChanges()
    {
        ActualizarAuditoriaAutomatica();
        return base.SaveChanges();
    }

    /// <summary>
    /// Sobrescribe SaveChangesAsync para auditoría automática
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ActualizarAuditoriaAutomatica();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Actualiza automáticamente las fechas de creación y modificación
    /// </summary>
    private void ActualizarAuditoriaAutomatica()
    {
        var entidades = ChangeTracker.Entries<RegistroFacturacionEntity>();
        var ahora = DateTime.UtcNow;

        foreach (var entrada in entidades)
        {
            if (entrada.State == EntityState.Added)
            {
                entrada.Entity.FechaCreacion = ahora;
                entrada.Entity.FechaModificacion = ahora;
            }
            else if (entrada.State == EntityState.Modified)
            {
                entrada.Entity.FechaModificacion = ahora;
            }
        }
    }
}
