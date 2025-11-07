# Guía de Integración de la Capa de Persistencia

## Introducción

Esta guía explica cómo integrar la capa de persistencia (`Verifactu.Data`) en tu aplicación para almacenar y gestionar registros de facturación.

## Instalación

### 1. Agregar referencia al proyecto

En tu proyecto, agrega una referencia a `Verifactu.Data`:

```bash
dotnet add reference ../Verifactu.Data/Verifactu.Data.csproj
```

### 2. Configurar servicios en `Program.cs` o `Startup.cs`

Elige el proveedor de base de datos que prefieras:

#### Opción A: SQLite (recomendado para desarrollo y pruebas)

```csharp
using Verifactu.Data.Configuration;

// En Program.cs o Startup.ConfigureServices
services.AddVerifactuDataSqlite("Data Source=verifactu.db");
```

#### Opción B: SQL Server (producción)

```csharp
services.AddVerifactuDataSqlServer(
    "Server=localhost;Database=Verifactu;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
);
```

#### Opción C: PostgreSQL (producción)

```csharp
services.AddVerifactuDataPostgreSQL(
    "Host=localhost;Database=verifactu;Username=postgres;Password=YourPassword"
);
```

### 3. Aplicar migraciones

Antes de ejecutar la aplicación por primera vez, aplica las migraciones:

```bash
# Desde la raíz del proyecto
dotnet ef database update --project src/Verifactu.Data --startup-project src/TuProyecto
```

O en tiempo de ejecución (no recomendado para producción):

```csharp
using Microsoft.EntityFrameworkCore;
using Verifactu.Data;

// En Program.cs o Startup.Configure
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VerifactuDbContext>();
    context.Database.Migrate();
}
```

## Uso Básico

### Ejemplo 1: Guardar un registro de facturación

```csharp
using Verifactu.Data;
using Verifactu.Data.Entities;

public class FacturacionService
{
    private readonly IUnitOfWork _unitOfWork;

    public FacturacionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task GuardarRegistroAsync(
        string serie, 
        string numero, 
        string xmlFirmado, 
        string huella)
    {
        var registro = new RegistroFacturacionEntity
        {
            Id = Guid.NewGuid(),
            Serie = serie,
            Numero = numero,
            FechaHoraExpedicionUTC = DateTime.UtcNow,
            Huella = huella,
            EstadoEnvio = EstadoEnvio.Pendiente,
            XmlFirmado = xmlFirmado,
            NifEmisor = "B12345678",
            NombreEmisor = "Mi Empresa SL",
            ImporteTotal = 121.00m,
            CuotaTotal = 21.00m
        };

        await _unitOfWork.RegistrosFacturacion.AddAsync(registro);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

### Ejemplo 2: Actualizar estado después del envío

```csharp
public async Task ActualizarEstadoEnvioAsync(
    string serie, 
    string numero, 
    bool exito, 
    string? csv = null,
    int? codigoError = null,
    string? descripcionError = null)
{
    var registro = await _unitOfWork.RegistrosFacturacion
        .GetBySerieNumeroAsync(serie, numero);

    if (registro == null)
        throw new Exception("Registro no encontrado");

    registro.EstadoEnvio = exito ? EstadoEnvio.Correcto : EstadoEnvio.Rechazado;
    registro.CSV = csv;
    registro.CodigoErrorAEAT = codigoError;
    registro.DescripcionErrorAEAT = descripcionError;
    registro.FechaEnvio = DateTime.UtcNow;

    _unitOfWork.RegistrosFacturacion.Update(registro);
    await _unitOfWork.SaveChangesAsync();
}
```

### Ejemplo 3: Obtener registros pendientes de envío

```csharp
public async Task<List<RegistroFacturacionEntity>> ObtenerPendientesAsync()
{
    var pendientes = await _unitOfWork.RegistrosFacturacion
        .GetPendientesEnvioAsync();

    return pendientes.ToList();
}
```

### Ejemplo 4: Procesar reintentos

```csharp
public async Task ProcesarReintentosAsync(int maxReintentos = 3)
{
    var paraReintento = await _unitOfWork.RegistrosFacturacion
        .GetParaReintentoAsync(maxReintentos);

    foreach (var registro in paraReintento)
    {
        try
        {
            // Intentar reenviar
            var resultado = await ReenviarRegistroAsync(registro);
            
            if (resultado.Exito)
            {
                registro.EstadoEnvio = EstadoEnvio.Correcto;
                registro.CSV = resultado.CSV;
            }
            else
            {
                registro.Reintentos++;
                if (registro.Reintentos >= maxReintentos)
                {
                    registro.EstadoEnvio = EstadoEnvio.ErrorPermanente;
                }
            }
            
            registro.FechaUltimoEnvio = DateTime.UtcNow;
            _unitOfWork.RegistrosFacturacion.Update(registro);
        }
        catch (Exception ex)
        {
            // Manejar error
            registro.Reintentos++;
            registro.EstadoEnvio = EstadoEnvio.ErrorTemporal;
            registro.DescripcionErrorAEAT = ex.Message;
            _unitOfWork.RegistrosFacturacion.Update(registro);
        }
    }

    await _unitOfWork.SaveChangesAsync();
}
```

### Ejemplo 5: Usar transacciones

```csharp
public async Task ProcesarLoteFacturasAsync(List<FacturaDto> facturas)
{
    await _unitOfWork.BeginTransactionAsync();

    try
    {
        foreach (var factura in facturas)
        {
            var registro = MapearFacturaARegistro(factura);
            await _unitOfWork.RegistrosFacturacion.AddAsync(registro);
        }

        await _unitOfWork.CommitTransactionAsync();
    }
    catch (Exception)
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### Ejemplo 6: Consultas avanzadas

```csharp
// Obtener último registro correcto para encadenamiento
public async Task<string?> ObtenerUltimaHuellaAsync()
{
    var ultimoRegistro = await _unitOfWork.RegistrosFacturacion
        .GetUltimoRegistroCorretoAsync();

    return ultimoRegistro?.Huella;
}

// Obtener registros por rango de fechas
public async Task<List<RegistroFacturacionEntity>> ObtenerPorRangoFechasAsync(
    DateTime desde, 
    DateTime hasta)
{
    var registros = await _unitOfWork.RegistrosFacturacion
        .GetByFechaRangoAsync(desde, hasta);

    return registros.ToList();
}

// Obtener por NIF de emisor
public async Task<List<RegistroFacturacionEntity>> ObtenerPorEmisorAsync(string nif)
{
    var registros = await _unitOfWork.RegistrosFacturacion
        .GetByNifEmisorAsync(nif);

    return registros.ToList();
}

// Contar registros por estado
public async Task<int> ContarPorEstadoAsync(EstadoEnvio estado)
{
    return await _unitOfWork.RegistrosFacturacion
        .CountAsync(r => r.EstadoEnvio == estado);
}
```

## Integración con el Flujo de Facturación

```csharp
public class VerifactuWorkflow
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHashService _hashService;
    private readonly IXmlSignerService _signerService;
    private readonly IVerifactuSoapClient _soapClient;

    public async Task<ResultadoEnvio> ProcesarFacturaAsync(
        Factura factura,
        X509Certificate2 certificado)
    {
        // 1. Obtener huella anterior para encadenamiento
        var huellaAnterior = await ObtenerUltimaHuellaAsync();

        // 2. Crear registro de facturación
        var registro = CrearRegistroFacturacion(factura, huellaAnterior);

        // 3. Calcular huella
        var huella = _hashService.CalcularHuella(registro, huellaAnterior);

        // 4. Crear y firmar XML
        var xml = _serializerService.CrearXmlRegistro(registro);
        var xmlFirmado = _signerService.Firmar(xml, certificado);

        // 5. Guardar en base de datos como Pendiente
        var entidad = new RegistroFacturacionEntity
        {
            Id = Guid.NewGuid(),
            Serie = factura.Serie,
            Numero = factura.Numero,
            FechaHoraExpedicionUTC = factura.FechaEmision.ToUniversalTime(),
            Huella = huella,
            HuellaAnterior = huellaAnterior,
            EstadoEnvio = EstadoEnvio.Pendiente,
            XmlFirmado = xmlFirmado.OuterXml,
            NifEmisor = factura.Emisor.Nif,
            NombreEmisor = factura.Emisor.Nombre,
            ImporteTotal = factura.Totales?.ImporteTotal ?? 0,
            CuotaTotal = factura.Totales?.CuotaImpuestos ?? 0
        };

        await _unitOfWork.RegistrosFacturacion.AddAsync(entidad);
        await _unitOfWork.SaveChangesAsync();

        // 6. Intentar enviar
        try
        {
            entidad.EstadoEnvio = EstadoEnvio.EnviandoAhora;
            _unitOfWork.RegistrosFacturacion.Update(entidad);
            await _unitOfWork.SaveChangesAsync();

            var respuesta = await _soapClient.EnviarRegFacturacionAltaAsync(
                xmlFirmado, 
                certificado
            );

            // 7. Actualizar con resultado
            entidad.EstadoEnvio = respuesta.EstadoEnvio == "Correcto" 
                ? EstadoEnvio.Correcto 
                : EstadoEnvio.Rechazado;
            entidad.CSV = respuesta.CSV;
            entidad.AcuseRecibo = SerializarRespuesta(respuesta);
            entidad.FechaEnvio = DateTime.UtcNow;
            entidad.FechaUltimoEnvio = DateTime.UtcNow;

            if (respuesta.RespuestasLinea?.FirstOrDefault() is { } lineaRespuesta)
            {
                if (int.TryParse(lineaRespuesta.CodigoErrorRegistro, out int codigo))
                {
                    entidad.CodigoErrorAEAT = codigo;
                }
                entidad.DescripcionErrorAEAT = lineaRespuesta.DescripcionErrorRegistro;
            }

            _unitOfWork.RegistrosFacturacion.Update(entidad);
            await _unitOfWork.SaveChangesAsync();

            return new ResultadoEnvio { Exito = true, CSV = respuesta.CSV };
        }
        catch (Exception ex)
        {
            // 8. Marcar como error temporal para reintento
            entidad.EstadoEnvio = EstadoEnvio.ErrorTemporal;
            entidad.DescripcionErrorAEAT = ex.Message;
            entidad.Reintentos = 0;
            entidad.FechaUltimoEnvio = DateTime.UtcNow;

            _unitOfWork.RegistrosFacturacion.Update(entidad);
            await _unitOfWork.SaveChangesAsync();

            return new ResultadoEnvio { Exito = false, Error = ex.Message };
        }
    }
}
```

## Configuración en appsettings.json

```json
{
  "ConnectionStrings": {
    "VerifactuDb": "Data Source=verifactu.db"
  },
  
  "Database": {
    "Provider": "SQLite",
    "AutoMigrate": false
  }
}
```

## Buenas Prácticas

1. **Siempre usar Unit of Work**: No acceder directamente al DbContext
2. **Transacciones para operaciones múltiples**: Usar `BeginTransaction/Commit/Rollback`
3. **Gestión de errores**: Capturar excepciones y marcar registros como ErrorTemporal
4. **Auditoría**: Las fechas se gestionan automáticamente
5. **Índices**: Aprovecha los índices existentes en las consultas
6. **Reintentos**: Implementa lógica de backoff exponencial
7. **Logging**: Añade logging en operaciones críticas
8. **Validación**: Valida datos antes de guardar

## Troubleshooting

### Error: "No database provider has been configured"
**Solución**: Asegúrate de llamar a uno de los métodos `AddVerifactuData*` en Program.cs

### Error: "Unable to create an object of type 'VerifactuDbContext'"
**Solución**: Asegúrate de que VerifactuDbContextFactory existe y está configurado correctamente

### Error: "Pending model changes"
**Solución**: Ejecuta `dotnet ef migrations add NombreMigracion` para crear una nueva migración

### Lentitud en consultas
**Solución**: Verifica que estás usando los índices definidos (Serie+Numero, EstadoEnvio, etc.)

## Soporte

Para más información, consulta:
- [README.md](README.md) - Documentación completa del proyecto
- [Tests](../../tests/Verifactu.Data.Tests/) - Ejemplos de uso
