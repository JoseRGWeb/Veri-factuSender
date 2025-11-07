using Microsoft.Extensions.Configuration;
using Verifactu.Client.Models;
using Verifactu.Client.Services;

Console.WriteLine("== VERI*FACTU Console Demo ==");

// 1) Cargar configuración
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var certPath = config["Certificado:PfxPath"] ?? "/path/certificado.pfx";
var certPass = config["Certificado:PfxPassword"] ?? "PFX_PASSWORD";
var endpoint = config["Verifactu:EndpointUrl"] ?? "https://example.com/verifactu/ws";
var soapAction = config["Verifactu:SoapAction"] ?? "urn:EnviarRegistroFacturacion";

// 2) Cargar certificado
ICertificateLoader certLoader = new CertificateLoader();
var cert = certLoader.CargarDesdePfx(certPath, certPass);
Console.WriteLine($"Certificado cargado: {cert.Subject}");

// 3) Cargar factura demo (json) y construir modelo
var facturaJson = await File.ReadAllTextAsync("factura-demo.json");
var factura = System.Text.Json.JsonSerializer.Deserialize<Factura>(facturaJson)
    ?? throw new InvalidOperationException("No se pudo deserializar 'factura-demo.json'");

// Calcular desglose de IVA desde las líneas de factura
var desglose = new List<DetalleDesglose>();
var desgloseAgrupado = factura.Lineas
    .GroupBy(l => l.TipoImpositivo)
    .Select(g => new DetalleDesglose(
        ClaveRegimen: "01",  // Régimen general
        CalificacionOperacion: "S1",  // Sujeta y no exenta
        TipoImpositivo: g.Key,
        BaseImponible: g.Sum(l => l.Base),
        CuotaRepercutida: g.Sum(l => l.Cuota)
    ))
    .ToList();

desglose.AddRange(desgloseAgrupado);

// Información del sistema informático
var sistemaInfo = new SistemaInformatico(
    NombreRazon: config["Verifactu:SistemaInfo:NombreRazon"] ?? "JRWEB, S.L.U.",
    Nif: config["Verifactu:SistemaInfo:Nif"] ?? "B12345678",
    NombreSistemaInformatico: "VerifactuSender",
    IdSistemaInformatico: config["Verifactu:SistemaInfo:IdSistema"] ?? "1",
    Version: "1.0.0",
    NumeroInstalacion: config["Verifactu:SistemaInfo:NumeroInstalacion"] ?? "1"
);

// 4) Construir Registro + Huella encadenada
IHashService hashService = new HashService();
var huellaAnterior = config["Verifactu:HuellaAnterior"]; // ej. persistida en BD

var reg = new RegistroFacturacion(
    IDVersion: "1.0",
    IDEmisorFactura: factura.Emisor.Nif,
    NumSerieFactura: $"{factura.Serie}{factura.Numero}",
    FechaExpedicionFactura: factura.FechaEmision,
    NombreRazonEmisor: factura.Emisor.Nombre,
    TipoFactura: factura.TipoFactura,
    DescripcionOperacion: factura.DescripcionOperacion ?? "Venta de productos/servicios",
    Desglose: desglose,
    CuotaTotal: factura.Totales.CuotaImpuestos,
    ImporteTotal: factura.Totales.ImporteTotal,
    FechaHoraHusoGenRegistro: DateTime.Now, // Fecha/hora con huso horario local
    TipoHuella: "01", // SHA-256
    Huella: string.Empty, // se completa abajo
    SistemaInformatico: sistemaInfo,
    Factura: factura,
    Destinatario: factura.Receptor,
    HuellaAnterior: huellaAnterior
);

var huella = hashService.CalcularHuella(reg, huellaAnterior);
reg = reg with { Huella = huella };
Console.WriteLine($"Huella calculada: {huella}");

// 5) Serializar a XML conforme a XSD oficial
IVerifactuSerializer serializer = new VerifactuSerializer();
var xml = serializer.CrearXmlRegistro(reg);

// 6) Firmar XML
IXmlSignerService signer = new XmlSignerService();
var xmlFirmado = signer.Firmar(xml, cert);
Console.WriteLine("XML firmado correctamente.");

// Mostrar XML generado (opcional, para debug)
Console.WriteLine("\n=== XML Generado (primeros 500 caracteres) ===");
Console.WriteLine(xmlFirmado.OuterXml.Substring(0, Math.Min(500, xmlFirmado.OuterXml.Length)));
Console.WriteLine("...\n");

// 7) Enviar vía SOAP
IVerifactuSoapClient soapClient = new VerifactuSoapClient(endpoint, soapAction);

try
{
    var respuesta = await soapClient.EnviarRegistroAsync(xmlFirmado, cert);
    Console.WriteLine("Respuesta SOAP:");
    Console.WriteLine(respuesta);
}
catch (Exception ex)
{
    Console.Error.WriteLine("Error al enviar registro: " + ex.Message);
    Environment.ExitCode = 1;
}
