using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Verifactu.Data.Entities;

namespace Verifactu.Data.Repositories;

/// <summary>
/// Implementación del repositorio de registros de facturación
/// </summary>
public class RegistroFacturacionRepository : Repository<RegistroFacturacionEntity>, IRegistroFacturacionRepository
{
    public RegistroFacturacionRepository(VerifactuDbContext context) : base(context)
    {
    }

    public async Task<RegistroFacturacionEntity?> GetBySerieNumeroAsync(string serie, string numero, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.Serie == serie && r.Numero == numero, cancellationToken);
    }

    public async Task<IEnumerable<RegistroFacturacionEntity>> GetByEstadoAsync(EstadoEnvio estado, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.EstadoEnvio == estado)
            .OrderByDescending(r => r.FechaHoraExpedicionUTC)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RegistroFacturacionEntity>> GetPendientesEnvioAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.EstadoEnvio == EstadoEnvio.Pendiente || r.EstadoEnvio == EstadoEnvio.ErrorTemporal)
            .OrderBy(r => r.FechaHoraExpedicionUTC)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RegistroFacturacionEntity>> GetByFechaRangoAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.FechaHoraExpedicionUTC >= desde && r.FechaHoraExpedicionUTC <= hasta)
            .OrderBy(r => r.FechaHoraExpedicionUTC)
            .ToListAsync(cancellationToken);
    }

    public async Task<RegistroFacturacionEntity?> GetUltimoRegistroCorretoAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.EstadoEnvio == EstadoEnvio.Correcto || r.EstadoEnvio == EstadoEnvio.AceptadoConErrores)
            .OrderByDescending(r => r.FechaHoraExpedicionUTC)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<RegistroFacturacionEntity>> GetByNifEmisorAsync(string nifEmisor, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.NifEmisor == nifEmisor)
            .OrderByDescending(r => r.FechaHoraExpedicionUTC)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RegistroFacturacionEntity>> GetParaReintentoAsync(int maxReintentos = 3, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.EstadoEnvio == EstadoEnvio.ErrorTemporal && r.Reintentos < maxReintentos)
            .OrderBy(r => r.FechaUltimoEnvio ?? r.FechaCreacion)
            .ToListAsync(cancellationToken);
    }
}
