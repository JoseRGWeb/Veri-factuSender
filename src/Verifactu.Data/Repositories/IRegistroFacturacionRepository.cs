using Verifactu.Data.Entities;

namespace Verifactu.Data.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de registros de facturación
/// Extiende el repositorio genérico con operaciones específicas del dominio
/// </summary>
public interface IRegistroFacturacionRepository : IRepository<RegistroFacturacionEntity>
{
    /// <summary>
    /// Obtiene un registro por serie y número
    /// </summary>
    Task<RegistroFacturacionEntity?> GetBySerieNumeroAsync(string serie, string numero, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene registros por estado de envío
    /// </summary>
    Task<IEnumerable<RegistroFacturacionEntity>> GetByEstadoAsync(EstadoEnvio estado, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene registros pendientes de envío o con errores temporales
    /// </summary>
    Task<IEnumerable<RegistroFacturacionEntity>> GetPendientesEnvioAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene registros por rango de fechas
    /// </summary>
    Task<IEnumerable<RegistroFacturacionEntity>> GetByFechaRangoAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el último registro enviado correctamente (para encadenamiento)
    /// </summary>
    Task<RegistroFacturacionEntity?> GetUltimoRegistroCorretoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene registros por NIF del emisor
    /// </summary>
    Task<IEnumerable<RegistroFacturacionEntity>> GetByNifEmisorAsync(string nifEmisor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene registros que necesitan reintento
    /// </summary>
    Task<IEnumerable<RegistroFacturacionEntity>> GetParaReintentoAsync(int maxReintentos = 3, CancellationToken cancellationToken = default);
}
