# Guía de Uso del Cliente SOAP VERI*FACTU

## Descripción General

El cliente SOAP actualizado implementa la comunicación con los servicios web oficiales de AEAT para VERI*FACTU, incluyendo:
- Estructura SOAP conforme al WSDL oficial
- Namespaces correctos según la documentación de AEAT
- Parseo automático de respuestas
- Manejo robusto de errores y timeouts
- Soporte para operaciones RegFacturacionAlta y ConsultaLRFacturas

## Operaciones Disponibles

### 1. RegFacturacionAlta (Alta/Anulación de Registros)

Envía registros de facturación (alta o anulación) al servicio VERI*FACTU de la AEAT.

**Método de alto nivel:**
```csharp
Task<RespuestaSuministro> EnviarRegFacturacionAltaAsync(
    XmlDocument xmlFirmado, 
    X509Certificate2 cert, 
    CancellationToken ct = default)
```

**Ejemplo de uso:**
```csharp
// Configurar cliente SOAP
var endpointUrl = "https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion"; // Sandbox
var soapAction = ""; // Vacío para este servicio
var client = new VerifactuSoapClient(endpointUrl, soapAction);

// Cargar certificado
var cert = CertificateLoader.CargarDesdePfx("ruta/certificado.pfx", "password");

// Crear y firmar registro XML (ver ejemplos en docs/Veri-Factu_Descripcion_SWeb.md)
XmlDocument registroFirmado = /* ... */;

// Enviar y parsear respuesta
try 
{
    var respuesta = await client.EnviarRegFacturacionAltaAsync(registroFirmado, cert);
    
    // Verificar estado global
    if (respuesta.EstadoEnvio == "Correcto")
    {
        Console.WriteLine($"Envío exitoso. CSV: {respuesta.CSV}");
        Console.WriteLine($"Tiempo de espera siguiente envío: {respuesta.TiempoEsperaEnvio}s");
    }
    else if (respuesta.EstadoEnvio == "ParcialmenteCorrecto")
    {
        Console.WriteLine("Envío parcialmente correcto. Revisar registros individuales.");
    }
    else // "Incorrecto"
    {
        Console.WriteLine("Envío rechazado completamente.");
    }
    
    // Revisar respuestas individuales
    foreach (var linea in respuesta.RespuestasLinea)
    {
        if (linea.EstadoRegistro == "Incorrecto")
        {
            Console.WriteLine($"Error en factura {linea.IDFactura?.NumSerieFactura}");
            Console.WriteLine($"  Código: {linea.CodigoErrorRegistro}");
            Console.WriteLine($"  Descripción: {linea.DescripcionErrorRegistro}");
            
            // Verificar si es duplicado
            if (linea.RegistroDuplicado != null)
            {
                Console.WriteLine($"  Registro duplicado. IdPeticion previo: {linea.RegistroDuplicado.IdPeticionRegistroDuplicado}");
            }
        }
    }
}
catch (TimeoutException ex)
{
    Console.WriteLine($"Timeout al enviar registro: {ex.Message}");
    // Implementar lógica de reintento
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Error de comunicación HTTP: {ex.Message}");
    // Verificar conectividad y certificados
}
```

### 2. ConsultaLRFacturas (Consulta de Registros)

Consulta registros de facturación previamente enviados.

**Método de alto nivel:**
```csharp
Task<RespuestaConsultaLR> ConsultarLRFacturasAsync(
    XmlDocument xmlConsulta, 
    X509Certificate2 cert, 
    CancellationToken ct = default)
```

**Ejemplo de uso:**
```csharp
// Crear XML de consulta (ver ejemplos en docs/Veri-Factu_Descripcion_SWeb.md)
XmlDocument xmlConsulta = /* ... */;

try
{
    var respuesta = await client.ConsultarLRFacturasAsync(xmlConsulta, cert);
    
    if (respuesta.ResultadoConsulta == "ConDatos")
    {
        Console.WriteLine($"Encontrados {respuesta.RegistrosRespuesta?.Count ?? 0} registros");
        
        foreach (var registro in respuesta.RegistrosRespuesta)
        {
            var factura = registro.IDFactura;
            var datos = registro.DatosRegistroFacturacion;
            
            Console.WriteLine($"Factura: {factura?.NumSerieFactura}");
            Console.WriteLine($"  Tipo: {datos?.TipoFactura}");
            Console.WriteLine($"  Importe: {datos?.ImporteTotal:C}");
            Console.WriteLine($"  Huella: {datos?.Huella}");
        }
        
        // Verificar paginación
        if (respuesta.IndicadorPaginacion == "S")
        {
            Console.WriteLine("Hay más datos. Usar ClavePaginacion para siguiente consulta:");
            Console.WriteLine($"  Emisor: {respuesta.ClavePaginacion?.IDEmisorFactura}");
            Console.WriteLine($"  Serie: {respuesta.ClavePaginacion?.NumSerieFactura}");
            Console.WriteLine($"  Fecha: {respuesta.ClavePaginacion?.FechaExpedicionFactura}");
        }
    }
    else // "SinDatos"
    {
        Console.WriteLine("No se encontraron registros para los criterios de búsqueda.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error en consulta: {ex.Message}");
}
```

## Estructura de Respuestas

### RespuestaSuministro (RegFacturacionAlta)

```csharp
public class RespuestaSuministro
{
    public string? CSV { get; set; }                          // Código Seguro de Verificación
    public DatosPresentacion? DatosPresentacion { get; set; } // NIF, Timestamp
    public CabeceraRespuesta? Cabecera { get; set; }          // Obligado emisión
    public int? TiempoEsperaEnvio { get; set; }               // Segundos entre envíos
    public string? EstadoEnvio { get; set; }                  // "Correcto", "ParcialmenteCorrecto", "Incorrecto"
    public List<RespuestaLinea>? RespuestasLinea { get; set; } // Respuestas individuales
}
```

### RespuestaConsultaLR (ConsultaLRFacturas)

```csharp
public class RespuestaConsultaLR
{
    public CabeceraConsulta? Cabecera { get; set; }
    public PeriodoImputacion? PeriodoImputacion { get; set; }   // Ejercicio y periodo
    public string? IndicadorPaginacion { get; set; }             // "S" o "N"
    public string? ResultadoConsulta { get; set; }               // "ConDatos" o "SinDatos"
    public List<RegistroRespuestaConsulta>? RegistrosRespuesta { get; set; }
    public ClavePaginacion? ClavePaginacion { get; set; }        // Para consultas paginadas
}
```

## Estados y Códigos de Error

### Estados Globales de Envío (EstadoEnvio)

| Estado | Descripción |
|--------|-------------|
| `Correcto` | Todos los registros aceptados correctamente |
| `ParcialmenteCorrecto` | Algunos registros aceptados, otros rechazados o con errores |
| `Incorrecto` | Todos los registros rechazados |

### Estados de Registro Individual (EstadoRegistro)

| Estado | Descripción |
|--------|-------------|
| `Correcto` | Registro aceptado sin errores |
| `AceptadoConErrores` | Registro aceptado pero con errores admisibles |
| `Incorrecto` | Registro rechazado |

### Códigos de Error Comunes

Consultar el documento oficial de validaciones de AEAT para lista completa:
- `4001` - NIF del emisor no identificado
- `3001` - Registro duplicado
- `5002` - Fecha inválida
- Ver `docs/Veri-Factu_Descripcion_SWeb.md` para lista completa

## Manejo de Errores y Reintentos

### Timeout

El cliente tiene configurado un timeout de 120 segundos. Si se excede:
```csharp
catch (TimeoutException ex)
{
    // Implementar reintento con backoff exponencial
    await Task.Delay(TimeSpan.FromSeconds(30));
    // Reintentar...
}
```

### Errores de Red

```csharp
catch (HttpRequestException ex)
{
    // Verificar:
    // 1. Conectividad con servidor AEAT
    // 2. Certificado válido y no revocado
    // 3. Fecha/hora del sistema correctas
}
```

### Mecanismo de Control de Flujo

La AEAT devuelve `TiempoEsperaEnvio` (inicialmente 60 segundos). Respetar este valor:

```csharp
var respuesta = await client.EnviarRegFacturacionAltaAsync(registro, cert);
var tiempoEspera = respuesta.TiempoEsperaEnvio ?? 60;

// Esperar antes del siguiente envío
await Task.Delay(TimeSpan.FromSeconds(tiempoEspera));
```

## Configuración de Endpoints

### Entorno de Pruebas (Sandbox)
```csharp
const string SANDBOX_ENDPOINT = "https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion";
```

### Entorno de Producción
```csharp
const string PRODUCCION_ENDPOINT = "https://www1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion";
```

## Namespaces SOAP Oficiales

Los siguientes namespaces están configurados en el cliente:

```xml
xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
xmlns:sfLR="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroLR.xsd"
xmlns:sf="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd"
xmlns:con="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/ConsultaLR.xsd"
```

## Referencias

- **Documentación WSDL**: `docs/wsdl/README.md`
- **Descripción servicios web**: `docs/Veri-Factu_Descripcion_SWeb.md`
- **Guía técnica**: `docs/Verifactu-Guia-Tecnica.md`
- **Tests de ejemplo**: `tests/Verifactu.Client.Tests/SoapClientTests.cs`
- **Modelos de respuesta**: `src/Verifactu.Client/Models/RespuestaAeat.cs`

## Notas Importantes

1. **CSV (Código Seguro de Verificación)**: Debe almacenarse inmediatamente al recibirlo. No puede recuperarse posteriormente mediante consultas.

2. **Límite de registros**: Máximo 1.000 registros por envío (alta/anulación).

3. **Consultas paginadas**: Las consultas devuelven máximo 10.000 registros. Si `IndicadorPaginacion = "S"`, usar `ClavePaginacion` para obtener más resultados.

4. **Certificados**: Asegurar que el NIF del certificado coincida con el NIF del emisor de facturas.

5. **TLS**: El cliente usa automáticamente TLS 1.2+ (configuración por defecto en .NET 9).
