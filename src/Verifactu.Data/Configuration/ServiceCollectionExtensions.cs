using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Verifactu.Data.Repositories;

namespace Verifactu.Data.Configuration;

/// <summary>
/// Métodos de extensión para configuración de servicios de persistencia
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra los servicios de persistencia usando SQLite
    /// </summary>
    public static IServiceCollection AddVerifactuDataSqlite(
        this IServiceCollection services, 
        string connectionString)
    {
        services.AddDbContext<VerifactuDbContext>(options =>
            options.UseSqlite(connectionString));

        return AddCommonServices(services);
    }

    /// <summary>
    /// Registra los servicios de persistencia usando SQL Server
    /// </summary>
    public static IServiceCollection AddVerifactuDataSqlServer(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<VerifactuDbContext>(options =>
            options.UseSqlServer(connectionString));

        return AddCommonServices(services);
    }

    /// <summary>
    /// Registra los servicios de persistencia usando PostgreSQL
    /// </summary>
    public static IServiceCollection AddVerifactuDataPostgreSQL(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<VerifactuDbContext>(options =>
            options.UseNpgsql(connectionString));

        return AddCommonServices(services);
    }

    /// <summary>
    /// Registra servicios comunes de persistencia
    /// </summary>
    private static IServiceCollection AddCommonServices(IServiceCollection services)
    {
        // Registrar Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Registrar repositorios
        services.AddScoped<IRegistroFacturacionRepository, RegistroFacturacionRepository>();

        return services;
    }
}
