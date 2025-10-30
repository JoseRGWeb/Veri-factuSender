# Guía de Uso de VerifactuSender

Esta guía proporciona ejemplos prácticos de cómo usar VerifactuSender para enviar facturas al sistema VERI\*FACTU de la AEAT.

## Flujo Básico de Uso

El proceso completo para enviar una factura a VERI\*FACTU consta de los siguientes pasos:

1. **Crear una factura** con todos sus datos
2. **Construir el registro de facturación**
3. **Calcular la huella (hash)** y encadenarla
4. **Serializar a XML** conforme a los esquemas AEAT
5. **Firmar el XML** con certificado digital
6. **Enviar por SOAP** al servicio de la AEAT
7. **Procesar la respuesta** y almacenar el resultado

## Ejemplo Completo: Aplicación de Consola

La aplicación de demostración (`Verifactu.ConsoleDemo`) muestra el flujo completo.

### Ejecutar la Demo

```bash
cd src/Verifactu.ConsoleDemo
dotnet run
```

### Código de la Demo

```csharp
using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Verifactu.Client.Models;
using Verifactu.Client.Services;
using Verifactu.Client.Soap;

namespace Verifactu.ConsoleDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("VerifactuSender - Demo de envío a VERI*FACTU");
            Console.WriteLine("=".PadRight(50, '='));

            // 1. Cargar configuración
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();

            // 2. Cargar certificado
            var certLoader = new CertificateLoader();
            var certificado = certLoader.CargarDesdeArchivo(
                config["Certificado:PfxPath"],
                config["Certificado:PfxPassword"]
            );

            // 3. Leer factura de prueba
            var facturaJson = await File.ReadAllTextAsync("factura-demo.json");
            var factura = JsonSerializer.Deserialize<Factura>(facturaJson);

            // 4. Construir registro
            var registro = new RegistroFacturacion
            {
                Uuid = Guid.NewGuid().ToString(),
                Factura = factura,
                FechaHoraCreacion = DateTime.UtcNow
            };

            // 5. Calcular huella
            var hashService = new HashService();
            var huellaAnterior = config["Verifactu:HuellaAnterior"] ?? "";
            registro.Huella = hashService.CalcularHuella(registro, huellaAnterior);

            // 6. Serializar a XML
            var serializer = new VerifactuSerializer();
            var xml = serializer.SerializarRegistro(registro);

            // 7. Firmar XML
            var signer = new XmlSignerService();
            var xmlFirmado = signer.FirmarXml(xml, certificado);

            // 8. Enviar por SOAP
            var soapClient = new VerifactuSoapClient();
            var respuesta = await soapClient.EnviarRegistroAsync(
                xmlFirmado,
                config["Verifactu:EndpointUrl"],
                certificado,
                config["Verifactu:SoapAction"]
            );

            Console.WriteLine("Respuesta recibida:");
            Console.WriteLine(respuesta);
        }
    }
}
```

## Casos de Uso Específicos

### Caso 1: Factura Simple

```csharp
using Verifactu.Client.Models;

var factura = new Factura
{
    Serie = "A",
    Numero = "2025-0001",
    FechaExpedicion = DateTime.UtcNow,
    
    Emisor = new Emisor
    {
        Nif = "B12345678",
        Nombre = "Mi Empresa S.L."
    },
    
    Receptor = new Receptor
    {
        Nif = "12345678A",
        Nombre = "Juan Pérez"
    },
    
    Lineas = new List<LineaFactura>
    {
        new LineaFactura
        {
            Descripcion = "Servicio de consultoría",
            Cantidad = 10,
            PrecioUnitario = 100.00m,
            TipoIva = 21
        }
    },
    
    TotalSinIva = 1000.00m,
    TotalIva = 210.00m,
    TotalFactura = 1210.00m
};
```

### Caso 2: Factura con Múltiples Líneas

```csharp
var factura = new Factura
{
    Serie = "B",
    Numero = "2025-0042",
    FechaExpedicion = DateTime.UtcNow,
    
    Emisor = new Emisor { /* ... */ },
    Receptor = new Receptor { /* ... */ },
    
    Lineas = new List<LineaFactura>
    {
        new LineaFactura
        {
            Descripcion = "Producto A",
            Cantidad = 5,
            PrecioUnitario = 50.00m,
            TipoIva = 21
        },
        new LineaFactura
        {
            Descripcion = "Producto B",
            Cantidad = 2,
            PrecioUnitario = 150.00m,
            TipoIva = 21
        },
        new LineaFactura
        {
            Descripcion = "Servicio C",
            Cantidad = 1,
            PrecioUnitario = 200.00m,
            TipoIva = 21
        }
    },
    
    TotalSinIva = 750.00m,  // (5*50) + (2*150) + 200
    TotalIva = 157.50m,     // 750 * 0.21
    TotalFactura = 907.50m
};
```

### Caso 3: Calcular Huella Encadenada

El encadenamiento es crucial en VERI\*FACTU. Cada registro debe incluir la huella del anterior:

```csharp
using Verifactu.Client.Services;

var hashService = new HashService();

// Primera factura (sin huella anterior)
var registro1 = new RegistroFacturacion
{
    Uuid = Guid.NewGuid().ToString(),
    Factura = factura1,
    HuellaAnterior = "" // Primer registro
};
registro1.Huella = hashService.CalcularHuella(registro1, "");

// Guardar la huella para el siguiente registro
var huellaAnterior = registro1.Huella;

// Segunda factura (con huella de la anterior)
var registro2 = new RegistroFacturacion
{
    Uuid = Guid.NewGuid().ToString(),
    Factura = factura2,
    HuellaAnterior = huellaAnterior
};
registro2.Huella = hashService.CalcularHuella(registro2, huellaAnterior);

// Y así sucesivamente...
```

### Caso 4: Cargar Certificado desde Almacén del Sistema

```csharp
using System.Security.Cryptography.X509Certificates;

// Cargar desde el almacén de certificados del usuario actual
var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
store.Open(OpenFlags.ReadOnly);

var certificado = store.Certificates
    .Find(X509FindType.FindByThumbprint, "THUMBPRINT_DEL_CERTIFICADO", false)
    .FirstOrDefault();

if (certificado == null)
{
    throw new Exception("Certificado no encontrado");
}

store.Close();
```

### Caso 5: Manejo de Errores en Envío SOAP

```csharp
using Verifactu.Client.Soap;

var soapClient = new VerifactuSoapClient();

try
{
    var respuesta = await soapClient.EnviarRegistroAsync(
        xmlFirmado,
        endpointUrl,
        certificado
    );
    
    // Analizar respuesta
    if (respuesta.Contains("Aceptado"))
    {
        Console.WriteLine("Registro enviado correctamente");
        // Guardar acuse de recibo
    }
    else
    {
        Console.WriteLine("Error en el envío:");
        Console.WriteLine(respuesta);
        // Registrar error para reintento
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Error de comunicación: {ex.Message}");
    // Implementar política de reintento
}
catch (Exception ex)
{
    Console.WriteLine($"Error inesperado: {ex.Message}");
    // Registrar para análisis
}
```

### Caso 6: Validar XML contra XSD

**Nota**: Esta funcionalidad debe implementarse para producción.

```csharp
using System.Xml;
using System.Xml.Schema;

public bool ValidarXmlContraXsd(string xml, string rutaXsd)
{
    var settings = new XmlReaderSettings();
    settings.Schemas.Add(null, rutaXsd);
    settings.ValidationType = ValidationType.Schema;
    
    bool esValido = true;
    settings.ValidationEventHandler += (sender, args) =>
    {
        Console.WriteLine($"Error de validación: {args.Message}");
        esValido = false;
    };
    
    using (var reader = XmlReader.Create(new StringReader(xml), settings))
    {
        while (reader.Read()) { }
    }
    
    return esValido;
}
```

## Integración en Aplicación Web

### Ejemplo con ASP.NET Core

#### Program.cs

```csharp
using Verifactu.Client.Services;
using Verifactu.Client.Soap;

var builder = WebApplication.CreateBuilder(args);

// Registrar servicios de VerifactuSender
builder.Services.AddSingleton<IHashService, HashService>();
builder.Services.AddSingleton<IVerifactuSerializer, VerifactuSerializer>();
builder.Services.AddSingleton<IXmlSignerService, XmlSignerService>();
builder.Services.AddSingleton<ICertificateLoader, CertificateLoader>();
builder.Services.AddSingleton<VerifactuSoapClient>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
```

#### FacturasController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using Verifactu.Client.Models;
using Verifactu.Client.Services;

[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly IHashService _hashService;
    private readonly IVerifactuSerializer _serializer;
    private readonly IXmlSignerService _signer;
    private readonly VerifactuSoapClient _soapClient;
    
    public FacturasController(
        IHashService hashService,
        IVerifactuSerializer serializer,
        IXmlSignerService signer,
        VerifactuSoapClient soapClient)
    {
        _hashService = hashService;
        _serializer = serializer;
        _signer = signer;
        _soapClient = soapClient;
    }
    
    [HttpPost]
    public async Task<IActionResult> EnviarFactura([FromBody] Factura factura)
    {
        // 1. Construir registro
        var registro = new RegistroFacturacion
        {
            Uuid = Guid.NewGuid().ToString(),
            Factura = factura,
            FechaHoraCreacion = DateTime.UtcNow
        };
        
        // 2. Calcular huella (obtener huellaAnterior de BD)
        var huellaAnterior = ObtenerUltimaHuella();
        registro.Huella = _hashService.CalcularHuella(registro, huellaAnterior);
        
        // 3. Serializar y firmar
        var xml = _serializer.SerializarRegistro(registro);
        var certificado = ObtenerCertificado();
        var xmlFirmado = _signer.FirmarXml(xml, certificado);
        
        // 4. Enviar
        var respuesta = await _soapClient.EnviarRegistroAsync(
            xmlFirmado,
            Configuration["Verifactu:EndpointUrl"],
            certificado
        );
        
        // 5. Guardar en BD
        GuardarRegistro(registro, respuesta);
        
        return Ok(new { Uuid = registro.Uuid, Huella = registro.Huella });
    }
}
```

## Persistencia de Registros

### Modelo de Base de Datos Sugerido

```csharp
public class RegistroFacturacionDb
{
    public int Id { get; set; }
    public string Uuid { get; set; }
    public string Serie { get; set; }
    public string Numero { get; set; }
    public DateTime FechaExpedicion { get; set; }
    public string Huella { get; set; }
    public string HuellaAnterior { get; set; }
    public string XmlFirmado { get; set; }
    public string EstadoEnvio { get; set; } // Pendiente, Enviado, Aceptado, Rechazado
    public string RespuestaAeat { get; set; }
    public DateTime? FechaEnvio { get; set; }
    public int Reintentos { get; set; }
}
```

### Guardar Registro

```csharp
public void GuardarRegistro(RegistroFacturacion registro, string respuesta)
{
    using var context = new AppDbContext();
    
    var registroDb = new RegistroFacturacionDb
    {
        Uuid = registro.Uuid,
        Serie = registro.Factura.Serie,
        Numero = registro.Factura.Numero,
        FechaExpedicion = registro.Factura.FechaExpedicion,
        Huella = registro.Huella,
        HuellaAnterior = registro.HuellaAnterior,
        XmlFirmado = xmlFirmado,
        EstadoEnvio = DeterminarEstado(respuesta),
        RespuestaAeat = respuesta,
        FechaEnvio = DateTime.UtcNow,
        Reintentos = 0
    };
    
    context.Registros.Add(registroDb);
    context.SaveChanges();
}
```

## Buenas Prácticas

### 1. Gestión de Huellas

```csharp
// Siempre obtener la última huella de la BD antes de crear un nuevo registro
public string ObtenerUltimaHuella()
{
    using var context = new AppDbContext();
    return context.Registros
        .OrderByDescending(r => r.FechaExpedicion)
        .Select(r => r.Huella)
        .FirstOrDefault() ?? "";
}
```

### 2. Validación de Facturas

```csharp
public bool ValidarFactura(Factura factura)
{
    if (string.IsNullOrEmpty(factura.Serie))
        return false;
    
    if (string.IsNullOrEmpty(factura.Numero))
        return false;
    
    if (factura.TotalFactura != factura.TotalSinIva + factura.TotalIva)
        return false;
    
    // Más validaciones...
    
    return true;
}
```

### 3. Reintentos con Política Exponencial

```csharp
public async Task<string> EnviarConReintentos(
    string xmlFirmado, 
    string endpoint, 
    X509Certificate2 cert,
    int maxReintentos = 3)
{
    for (int intento = 0; intento < maxReintentos; intento++)
    {
        try
        {
            return await _soapClient.EnviarRegistroAsync(xmlFirmado, endpoint, cert);
        }
        catch (HttpRequestException) when (intento < maxReintentos - 1)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, intento));
            await Task.Delay(delay);
        }
    }
    throw new Exception("Máximo de reintentos alcanzado");
}
```

## Limitaciones Actuales

⚠️ **Importante**: Esta es una implementación de referencia. Antes de usar en producción:

1. **Serialización XML**: Reemplazar por implementación conforme a XSD oficiales de AEAT
2. **Algoritmo de huella**: Implementar según especificación oficial de AEAT
3. **Firma XML**: Verificar parámetros de canonicalización y transformaciones
4. **Endpoint SOAP**: Usar el WSDL oficial y endpoints de AEAT
5. **Validaciones**: Implementar todas las validaciones del documento oficial de AEAT

Consulta el [Roadmap](roadmap.md) para más detalles.

## Próximos Pasos

- Revisa la [Guía Técnica de Integración](Verifactu-Guia-Tecnica.md) para entender los detalles técnicos
- Consulta la [Arquitectura](arquitectura.md) para entender cómo funciona internamente
- Lee el [Roadmap](roadmap.md) para conocer las mejoras planificadas

---

**Última actualización:** 30 de octubre de 2025
