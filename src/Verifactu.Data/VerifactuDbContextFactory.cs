using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Verifactu.Data;

/// <summary>
/// Factory para crear el DbContext en tiempo de diseño (para migraciones)
/// </summary>
public class VerifactuDbContextFactory : IDesignTimeDbContextFactory<VerifactuDbContext>
{
    public VerifactuDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VerifactuDbContext>();
        
        // Usar SQLite por defecto para migraciones
        optionsBuilder.UseSqlite("Data Source=verifactu.db");

        return new VerifactuDbContext(optionsBuilder.Options);
    }
}
