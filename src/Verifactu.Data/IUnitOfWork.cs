using System;
using System.Threading;
using System.Threading.Tasks;
using Verifactu.Data.Repositories;

namespace Verifactu.Data;

/// <summary>
/// Interfaz del patrón Unit of Work
/// Coordina el trabajo de múltiples repositorios y gestiona transacciones
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Repositorio de registros de facturación
    /// </summary>
    IRegistroFacturacionRepository RegistrosFacturacion { get; }

    /// <summary>
    /// Guarda todos los cambios pendientes en la base de datos
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia una nueva transacción
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma la transacción actual
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Revierte la transacción actual
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
