using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Verifactu.Client.Services;
using Verifactu.Integration.Tests.Helpers;

namespace Verifactu.Integration.Tests.Fixtures;

/// <summary>
/// Fixture compartido para tests de integración contra sandbox de AEAT.
/// Inicializa recursos compartidos y los dispone correctamente.
/// </summary>
public class AeatSandboxFixture : IDisposable
{
    public IConfiguration Configuration { get; }
    public X509Certificate2? Certificado { get; }
    public bool SkipTests { get; }
    public string EndpointUrl { get; }
    public string EmisorNif { get; }
    public string EmisorNombre { get; }
    public string ReceptorNif { get; }
    public string ReceptorNombre { get; }
    public TestDataBuilder DataBuilder { get; }
    public SandboxHelper? SandboxHelper { get; }

    public AeatSandboxFixture()
    {
        // Cargar configuración
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Sandbox.json", optional: false, reloadOnChange: false)
            .AddUserSecrets<AeatSandboxFixture>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Configuración de endpoint
        EndpointUrl = Configuration["Verifactu:EndpointUrl"] 
            ?? "https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion";

        // Datos de emisor y receptor
        EmisorNif = Configuration["IntegrationTests:Emisor:Nif"] ?? "B12345678";
        EmisorNombre = Configuration["IntegrationTests:Emisor:Nombre"] ?? "EMPRESA PRUEBAS SANDBOX SL";
        ReceptorNif = Configuration["IntegrationTests:Receptor:Nif"] ?? "12345678A";
        ReceptorNombre = Configuration["IntegrationTests:Receptor:Nombre"] ?? "CLIENTE PRUEBAS SANDBOX";

        // Inicializar builder de datos
        DataBuilder = new TestDataBuilder(
            emisorNif: EmisorNif,
            emisorNombre: EmisorNombre,
            receptorNif: ReceptorNif,
            receptorNombre: ReceptorNombre,
            sistemaInformaticoNombre: Configuration["IntegrationTests:SistemaInformatico:NombreSistemaInformatico"] ?? "VerifactuSender",
            sistemaInformaticoId: Configuration["IntegrationTests:SistemaInformatico:IdSistemaInformatico"] ?? "TEST001",
            sistemaInformaticoVersion: Configuration["IntegrationTests:SistemaInformatico:Version"] ?? "1.0",
            numeroInstalacion: Configuration["IntegrationTests:SistemaInformatico:NumeroInstalacion"] ?? "1"
        );

        // Cargar certificado si está configurado
        var certPath = Configuration["Certificado:PfxPath"];
        var certPassword = Configuration["Certificado:PfxPassword"];
        var skipIfNoCert = bool.TryParse(Configuration["IntegrationTests:SkipIfNoCertificate"], out var skip) 
            ? skip : true;

        if (!string.IsNullOrWhiteSpace(certPath) && File.Exists(certPath))
        {
            try
            {
                var certLoader = new CertificateLoader();
                Certificado = certLoader.CargarDesdePfx(certPath, certPassword ?? string.Empty);
                SandboxHelper = new SandboxHelper(EndpointUrl, Certificado);
                SkipTests = false;
            }
            catch (Exception ex)
            {
                SkipTests = skipIfNoCert;
                if (!SkipTests)
                {
                    throw new InvalidOperationException(
                        $"Error al cargar certificado desde {certPath}: {ex.Message}", ex);
                }
            }
        }
        else
        {
            SkipTests = skipIfNoCert;
            if (!SkipTests)
            {
                throw new InvalidOperationException(
                    "No se ha configurado certificado válido. " +
                    "Configure Certificado:PfxPath y Certificado:PfxPassword en appsettings o user-secrets");
            }
        }
    }

    public void Dispose()
    {
        Certificado?.Dispose();
        SandboxHelper?.Dispose();
    }
}
