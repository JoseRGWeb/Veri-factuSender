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

// 4) Construir Registro + Huella encadenada (placeholder)
IHashService hashService = new HashService();
var huellaAnterior = config["Verifactu:HuellaAnterior"]; // ej. persistida en BD
var reg = new RegistroFacturacion(
    Uuid: Guid.NewGuid().ToString(),
    FechaHoraExpedicionUtc: DateTime.UtcNow,
    Serie: factura.Serie,
    Numero: factura.Numero,
    HashPrevio: huellaAnterior ?? string.Empty,
    Huella: string.Empty, // se completa abajo
    Factura: factura
);

var huella = hashService.CalcularHuella(reg, huellaAnterior);
reg = reg with { Huella = huella };
Console.WriteLine($"Huella calculada: {huella}");

// 5) Serializar a XML (placeholder)
IVerifactuSerializer serializer = new VerifactuSerializer();
var xml = serializer.CrearXmlRegistro(reg);

// 6) Firmar XML
IXmlSignerService signer = new XmlSignerService();
var xmlFirmado = signer.Firmar(xml, cert);
Console.WriteLine("XML firmado correctamente.");

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
