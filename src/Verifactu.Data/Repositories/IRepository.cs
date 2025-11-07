using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Verifactu.Data.Repositories;

/// <summary>
/// Interfaz genérica de repositorio con operaciones CRUD
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Obtiene una entidad por su ID
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las entidades
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca entidades que cumplan una condición
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la primera entidad que cumpla una condición, o null si no existe
    /// </summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade una nueva entidad
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade múltiples entidades
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una entidad
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Elimina una entidad
    /// </summary>
    void Remove(T entity);

    /// <summary>
    /// Elimina múltiples entidades
    /// </summary>
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>
    /// Cuenta las entidades que cumplen una condición
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe alguna entidad que cumpla una condición
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
