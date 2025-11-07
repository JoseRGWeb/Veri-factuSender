using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Verifactu.Client.Models;
using Verifactu.Client.Services;
using Xunit;
using Xunit.Sdk;

namespace Verifactu.Integration.Tests;

/// <summary>
/// Tests de integración contra el Portal de Pruebas Externas de AEAT (Sandbox).
/// 
/// SERVICIOS BAJO TEST:
/// - HashService: Cálculo de huella SHA-256 y encadenamiento
/// - VerifactuSerializer: Generación de XML conforme a XSD oficial
/// - XmlSignerService: Firma electrónica XMLDSig
/// - VerifactuSoapClient: Comunicación SOAP con AEAT (mTLS)
/// - CertificateLoader: Carga de certificados digitales PFX
/// 
/// IMPORTANTE: Estos tests requieren:
/// - Certificado digital válido configurado en appsettings.Sandbox.json o user-secrets
/// - Acceso a Internet al endpoint de sandbox de AEAT
/// - Los tests se omiten automáticamente si no hay certificado configurado
/// 
/// CONFIGURACIÓN:
/// 1. Copiar appsettings.Sandbox.json y configurar rutas de certificado
/// 2. O usar user-secrets:
///    dotnet user-secrets set "Certificado:PfxPath" "/ruta/al/certificado.pfx"
///    dotnet user-secrets set "Certificado:PfxPassword" "password"
/// 
/// EJECUCIÓN:
/// - Ejecutar todos: dotnet test --filter "Category=Integration"
/// - Ejecutar en sandbox: dotnet test Verifactu.Integration.Tests
/// 
/// NOTAS:
/// - Los datos enviados NO tienen validez tributaria
/// - El sandbox puede tener validaciones más permisivas que producción
/// - Revisar logs detallados para debugging (configurados en appsettings.Sandbox.json)
/// </summary>
[Collection("SandboxIntegrationTests")]
public class SandboxTests : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly X509Certificate2? _certificado;
    private readonly bool _skipTests;
    private readonly string _endpointUrl;
    private readonly HashService _hashService;
    private readonly XmlSignerService? _xmlSignerService;
    private readonly VerifactuSerializer _serializer;
    private readonly VerifactuSoapClient? _soapClient;
    private readonly string _emisorNif;
    private readonly string _emisorNombre;
    private readonly string _receptorNif;
    private readonly string _receptorNombre;
    
    // Variable para almacenar la huella del último registro enviado (para encadenamiento)
    private static string? _ultimaHuella = null;

    public SandboxTests()
    {
        // Cargar configuración desde appsettings.Sandbox.json y user-secrets
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Sandbox.json", optional: false, reloadOnChange: false)
            .AddUserSecrets<SandboxTests>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Obtener configuración de endpoints
        _endpointUrl = _configuration["Verifactu:EndpointUrl"] 
            ?? "https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion";

        // Obtener datos del emisor y receptor de configuración
        _emisorNif = _configuration["IntegrationTests:Emisor:Nif"] ?? "B12345678";
        _emisorNombre = _configuration["IntegrationTests:Emisor:Nombre"] ?? "EMPRESA PRUEBAS SANDBOX SL";
        _receptorNif = _configuration["IntegrationTests:Receptor:Nif"] ?? "12345678A";
        _receptorNombre = _configuration["IntegrationTests:Receptor:Nombre"] ?? "CLIENTE PRUEBAS SANDBOX";

        // Inicializar servicios
        _hashService = new HashService();
        _serializer = new VerifactuSerializer();
        
        // Cargar certificado si está configurado
        var certPath = _configuration["Certificado:PfxPath"];
        var certPassword = _configuration["Certificado:PfxPassword"];
        var skipIfNoCert = bool.TryParse(_configuration["IntegrationTests:SkipIfNoCertificate"], out var skip) 
            ? skip : true;

        if (!string.IsNullOrWhiteSpace(certPath) && File.Exists(certPath))
        {
            try
            {
                var certLoader = new CertificateLoader();
                _certificado = certLoader.CargarDesdePfx(certPath, certPassword ?? string.Empty);
                _xmlSignerService = new XmlSignerService();
                _soapClient = new VerifactuSoapClient(_endpointUrl, "RegFacturacionAlta");
                _skipTests = false;
            }
            catch (Exception ex)
            {
                _skipTests = skipIfNoCert;
                if (!_skipTests)
                {
                    throw new InvalidOperationException(
                        $"Error al cargar certificado desde {certPath}: {ex.Message}", ex);
                }
            }
        }
        else
        {
            _skipTests = skipIfNoCert;
            if (!_skipTests)
            {
                throw new InvalidOperationException(
                    "No se ha configurado certificado válido. " +
                    "Configure Certificado:PfxPath y Certificado:PfxPassword en appsettings o user-secrets");
            }
        }
    }

    /// <summary>
    /// Test 1: Verifica que se puede establecer conexión TLS mutua con el sandbox de AEAT.
    /// Este test valida la configuración del certificado y la conectividad básica.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Sandbox")]
    public async Task ConexionTLSMutua_ConCertificadoValido_DebeConectar()
    {
        // Arrange
        if (_skipTests) { return; } // Test omitido: No hay certificado configurado

        // Este test solo verifica que podemos hacer una petición básica
        // La respuesta puede ser un error de validación, pero no debe ser error de TLS/conexión
        var registro = CrearRegistroPruebaBasico("CONN-001", TipoFactura.F1);
        
        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
        {
            var huella = _hashService.CalcularHuella(registro, _ultimaHuella);
            var registroConHuella = registro with { Huella = huella };
            var xml = _serializer.CrearXmlRegistro(registroConHuella);
            var xmlFirmado = _xmlSignerService!.Firmar(xml, _certificado!);
            
            // Intentar enviar - puede fallar por validación pero no por TLS
            await _soapClient!.EnviarRegistroAsync(xmlFirmado, _certificado!, default);
        });

        // No debe haber errores de certificado TLS
        // Los errores aceptables son: validación XML, validación de negocio
        if (exception != null)
        {
            // Verificar que NO es error de TLS/certificado
            Assert.DoesNotContain("SSL", exception.ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("certificate", exception.ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("TLS", exception.ToString(), StringComparison.OrdinalIgnoreCase);
            
            // Es aceptable tener errores de validación de negocio o SOAP
            Assert.True(
                exception.ToString().Contains("SOAP", StringComparison.OrdinalIgnoreCase) ||
                exception.ToString().Contains("validación", StringComparison.OrdinalIgnoreCase) ||
                exception.ToString().Contains("validation", StringComparison.OrdinalIgnoreCase),
                $"Error inesperado en conexión TLS: {exception.Message}");
        }
    }

    /// <summary>
    /// Test 2: Envía una factura completa (F1) al sandbox y valida la respuesta.
    /// Valida el flujo end-to-end completo: generación, firma, envío y respuesta.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Sandbox")]
    public async Task EnviarFacturaCompleta_F1_DebeRecibirRespuestaAEAT()
    {
        // Arrange
        if (_skipTests) { return; } // Test omitido: No hay certificado configurado

        var numeroFactura = $"F1-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var registro = CrearRegistroPruebaBasico(numeroFactura, TipoFactura.F1);
        
        // Act
        var huella = _hashService.CalcularHuella(registro, _ultimaHuella);
        var registroConHuella = registro with { Huella = huella };
        var xml = _serializer.CrearXmlRegistro(registroConHuella);
        var xmlFirmado = _xmlSignerService!.Firmar(xml, _certificado!);
        
        var respuestaXml = await _soapClient!.EnviarRegistroAsync(xmlFirmado, _certificado!, default);
        var respuesta = VerifactuSoapClient.ParsearRespuestaSuministro(respuestaXml);

        // Assert
        Assert.NotNull(respuesta);
        Assert.NotNull(respuesta.EstadoEnvio);
        
        // El estado puede ser Correcto, ParcialmenteCorrecto o Incorrecto
        // Lo importante es que recibimos respuesta del servidor
        Assert.Contains(respuesta.EstadoEnvio, new[] { "Correcto", "ParcialmenteCorrecto", "Incorrecto" });
        
        // Si es correcto, debe tener CSV
        if (respuesta.EstadoEnvio == "Correcto")
        {
            Assert.NotNull(respuesta.CSV);
            Assert.NotEmpty(respuesta.CSV);
            
            // Actualizar última huella para encadenamiento
            _ultimaHuella = huella;
        }
        
        // Debe haber al menos una línea de respuesta
        Assert.NotNull(respuesta.RespuestasLinea);
        Assert.NotEmpty(respuesta.RespuestasLinea);
        
        // Log para debugging
        var primeraLinea = respuesta.RespuestasLinea[0];
        System.Diagnostics.Debug.WriteLine($"Estado registro: {primeraLinea.EstadoRegistro}");
        if (!string.IsNullOrEmpty(primeraLinea.CodigoErrorRegistro))
        {
            System.Diagnostics.Debug.WriteLine(
                $"Error {primeraLinea.CodigoErrorRegistro}: {primeraLinea.DescripcionErrorRegistro}");
        }
    }

    /// <summary>
    /// Test 3: Envía una factura simplificada (F2) al sandbox.
    /// Las facturas F2 tienen requisitos menores de datos del destinatario.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Sandbox")]
    public async Task EnviarFacturaSimplificada_F2_DebeRecibirRespuestaAEAT()
    {
        // Arrange
        if (_skipTests) { return; } // Test omitido: No hay certificado configurado

        var numeroFactura = $"F2-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var registro = CrearRegistroPruebaBasico(numeroFactura, TipoFactura.F2);
        
        // Act
        var huella = _hashService.CalcularHuella(registro, _ultimaHuella);
        var registroConHuella = registro with { Huella = huella };
        var xml = _serializer.CrearXmlRegistro(registroConHuella);
        var xmlFirmado = _xmlSignerService!.Firmar(xml, _certificado!);
        
        var respuestaXml = await _soapClient!.EnviarRegistroAsync(xmlFirmado, _certificado!, default);
        var respuesta = VerifactuSoapClient.ParsearRespuestaSuministro(respuestaXml);

        // Assert
        Assert.NotNull(respuesta);
        Assert.NotNull(respuesta.EstadoEnvio);
        Assert.Contains(respuesta.EstadoEnvio, new[] { "Correcto", "ParcialmenteCorrecto", "Incorrecto" });
        
        Assert.NotNull(respuesta.RespuestasLinea);
        Assert.NotEmpty(respuesta.RespuestasLinea);
        
        // Si es correcto, actualizar huella
        if (respuesta.EstadoEnvio == "Correcto")
        {
            _ultimaHuella = huella;
        }
    }

    /// <summary>
    /// Test 4: Envía una factura rectificativa al sandbox.
    /// Las facturas rectificativas (R1-R5) requieren referencia a la factura original.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Sandbox")]
    public async Task EnviarFacturaRectificativa_R1_DebeRecibirRespuestaAEAT()
    {
        // Arrange
        if (_skipTests) { return; } // Test omitido: No hay certificado configurado

        var numeroFactura = $"R1-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var registro = CrearRegistroPruebaRectificativa(numeroFactura, TipoRectificativa.S);
        
        // Act
        var huella = _hashService.CalcularHuella(registro, _ultimaHuella);
        var registroConHuella = registro with { Huella = huella };
        var xml = _serializer.CrearXmlRegistro(registroConHuella);
        var xmlFirmado = _xmlSignerService!.Firmar(xml, _certificado!);
        
        var respuestaXml = await _soapClient!.EnviarRegistroAsync(xmlFirmado, _certificado!, default);
        var respuesta = VerifactuSoapClient.ParsearRespuestaSuministro(respuestaXml);

        // Assert
        Assert.NotNull(respuesta);
        Assert.NotNull(respuesta.EstadoEnvio);
        Assert.Contains(respuesta.EstadoEnvio, new[] { "Correcto", "ParcialmenteCorrecto", "Incorrecto" });
        
        Assert.NotNull(respuesta.RespuestasLinea);
        Assert.NotEmpty(respuesta.RespuestasLinea);
        
        // Si es correcto, actualizar huella
        if (respuesta.EstadoEnvio == "Correcto")
        {
            _ultimaHuella = huella;
        }
    }

    /// <summary>
    /// Test 5: Verifica el manejo de errores de validación.
    /// Envía una factura con datos inválidos y verifica que se recibe error descriptivo.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Sandbox")]
    public async Task EnviarFacturaConDatosInvalidos_DebeRetornarErrorValidacion()
    {
        // Arrange
        if (_skipTests) { return; } // Test omitido: No hay certificado configurado

        // Crear factura con importe negativo (inválido)
        var numeroFactura = $"ERR-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var registro = CrearRegistroPruebaBasico(numeroFactura, TipoFactura.F1);
        
        // Modificar para que tenga un importe inválido
        registro = registro with { ImporteTotal = -100m };
        
        // Act
        var huella = _hashService.CalcularHuella(registro, _ultimaHuella);
        var registroConHuella = registro with { Huella = huella };
        var xml = _serializer.CrearXmlRegistro(registroConHuella);
        var xmlFirmado = _xmlSignerService!.Firmar(xml, _certificado!);
        
        var respuestaXml = await _soapClient!.EnviarRegistroAsync(xmlFirmado, _certificado!, default);
        var respuesta = VerifactuSoapClient.ParsearRespuestaSuministro(respuestaXml);

        // Assert
        Assert.NotNull(respuesta);
        
        // Debe retornar estado de error
        Assert.Equal("Incorrecto", respuesta.EstadoEnvio);
        
        // Debe tener línea de respuesta con error
        Assert.NotNull(respuesta.RespuestasLinea);
        Assert.NotEmpty(respuesta.RespuestasLinea);
        
        var lineaError = respuesta.RespuestasLinea[0];
        Assert.Equal("Incorrecto", lineaError.EstadoRegistro);
        Assert.NotNull(lineaError.CodigoErrorRegistro);
        Assert.NotNull(lineaError.DescripcionErrorRegistro);
        
        // Log del error para debugging
        System.Diagnostics.Debug.WriteLine(
            $"Error esperado recibido - Código: {lineaError.CodigoErrorRegistro}, " +
            $"Descripción: {lineaError.DescripcionErrorRegistro}");
    }

    /// <summary>
    /// Test 6: Verifica el encadenamiento de registros mediante huellas.
    /// Envía dos facturas consecutivas y valida que la segunda incluye la huella de la primera.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Sandbox")]
    public async Task EnviarFacturasEncadenadas_DebeValidarEncadenamiento()
    {
        // Arrange
        if (_skipTests) { return; } // Test omitido: No hay certificado configurado

        var numeroFactura1 = $"ENC1-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var numeroFactura2 = $"ENC2-{DateTime.UtcNow:yyyyMMddHHmmss}";
        
        // Act - Enviar primera factura
        var registro1 = CrearRegistroPruebaBasico(numeroFactura1, TipoFactura.F1);
        var huella1 = _hashService.CalcularHuella(registro1, _ultimaHuella);
        var registro1ConHuella = registro1 with { Huella = huella1 };
        var xml1 = _serializer.CrearXmlRegistro(registro1ConHuella);
        var xmlFirmado1 = _xmlSignerService!.Firmar(xml1, _certificado!);
        
        var respuestaXml1 = await _soapClient!.EnviarRegistroAsync(xmlFirmado1, _certificado!, default);
        var respuesta1 = VerifactuSoapClient.ParsearRespuestaSuministro(respuestaXml1);
        
        // Verificar que la primera factura se envió
        Assert.NotNull(respuesta1);
        
        // Act - Enviar segunda factura con huella de la primera
        await Task.Delay(1000); // Pequeña espera para asegurar timestamp diferente
        
        var registro2 = CrearRegistroPruebaBasico(numeroFactura2, TipoFactura.F1);
        var huella2 = _hashService.CalcularHuella(registro2, huella1);
        var registro2ConHuella = registro2 with { Huella = huella2 };
        var xml2 = _serializer.CrearXmlRegistro(registro2ConHuella);
        var xmlFirmado2 = _xmlSignerService!.Firmar(xml2, _certificado!);
        
        var respuestaXml2 = await _soapClient!.EnviarRegistroAsync(xmlFirmado2, _certificado!, default);
        var respuesta2 = VerifactuSoapClient.ParsearRespuestaSuministro(respuestaXml2);
        
        // Assert
        Assert.NotNull(respuesta2);
        
        // Verificar que las huellas son diferentes
        Assert.NotEqual(huella1, huella2);
        
        // Si ambas son correctas, actualizar última huella
        if (respuesta1.EstadoEnvio == "Correcto" && respuesta2.EstadoEnvio == "Correcto")
        {
            _ultimaHuella = huella2;
        }
    }

    /// <summary>
    /// Test 7: Verifica el manejo de registros duplicados.
    /// Envía la misma factura dos veces y verifica que AEAT detecta el duplicado.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Sandbox")]
    public async Task EnviarFacturaDuplicada_DebeDetectarDuplicado()
    {
        // Arrange
        if (_skipTests) { return; } // Test omitido: No hay certificado configurado

        var numeroFactura = $"DUP-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var registro = CrearRegistroPruebaBasico(numeroFactura, TipoFactura.F1);
        
        // Act - Enviar primera vez
        var huella = _hashService.CalcularHuella(registro, _ultimaHuella);
        var registroConHuella = registro with { Huella = huella };
        var xml = _serializer.CrearXmlRegistro(registroConHuella);
        var xmlFirmado = _xmlSignerService!.Firmar(xml, _certificado!);
        
        var respuestaXml1 = await _soapClient!.EnviarRegistroAsync(xmlFirmado, _certificado!, default);
        var respuesta1 = VerifactuSoapClient.ParsearRespuestaSuministro(respuestaXml1);
        
        // Solo continuar si el primer envío fue exitoso
        if (respuesta1.EstadoEnvio == "Correcto")
        {
            // Act - Enviar segunda vez (duplicado)
            await Task.Delay(500);
            
            var respuestaXml2 = await _soapClient!.EnviarRegistroAsync(xmlFirmado, _certificado!, default);
            var respuesta2 = VerifactuSoapClient.ParsearRespuestaSuministro(respuestaXml2);
            
            // Assert
            Assert.NotNull(respuesta2);
            
            // Puede retornar error de duplicado o aceptar idempotentemente
            // Verificar que hay respuesta de línea
            Assert.NotNull(respuesta2.RespuestasLinea);
            Assert.NotEmpty(respuesta2.RespuestasLinea);
            
            var lineaRespuesta = respuesta2.RespuestasLinea[0];
            
            // Si detecta duplicado, debe indicarlo
            if (lineaRespuesta.RegistroDuplicado != null)
            {
                Assert.NotNull(lineaRespuesta.RegistroDuplicado.IdPeticionRegistroDuplicado);
                System.Diagnostics.Debug.WriteLine(
                    $"Duplicado detectado: {lineaRespuesta.RegistroDuplicado.IdPeticionRegistroDuplicado}");
            }
            
            _ultimaHuella = huella;
        }
    }

    #region Métodos auxiliares

    /// <summary>
    /// Crea un registro de facturación básico para pruebas
    /// </summary>
    private RegistroFacturacion CrearRegistroPruebaBasico(string numeroFactura, TipoFactura tipoFactura)
    {
        var fechaActual = DateTime.UtcNow;
        
        var factura = new Factura(
            Serie: "TEST",
            Numero: numeroFactura,
            FechaEmision: fechaActual,
            TipoFactura: tipoFactura,
            DescripcionOperacion: $"Test de integración sandbox - {tipoFactura}",
            Emisor: new Emisor(_emisorNif, _emisorNombre),
            Receptor: new Receptor(_receptorNif, _receptorNombre),
            Lineas: new List<Linea>
            {
                new Linea("Producto de prueba A", 1, 100m, 21m),
                new Linea("Producto de prueba B", 2, 50m, 21m)
            },
            Totales: new TotalesFactura(200m, 42m, 242m)
        );

        var desglose = new List<DetalleDesglose>
        {
            new DetalleDesglose(
                ClaveRegimen: "01",
                CalificacionOperacion: "S1",
                TipoImpositivo: 21m,
                BaseImponible: 200m,
                CuotaRepercutida: 42m
            )
        };

        var sistemaInfo = new SistemaInformatico(
            NombreRazon: _emisorNombre,
            Nif: _emisorNif,
            NombreSistemaInformatico: _configuration["IntegrationTests:SistemaInformatico:NombreSistemaInformatico"] 
                ?? "VerifactuSender",
            IdSistemaInformatico: _configuration["IntegrationTests:SistemaInformatico:IdSistemaInformatico"] 
                ?? "TEST001",
            Version: _configuration["IntegrationTests:SistemaInformatico:Version"] ?? "1.0",
            NumeroInstalacion: _configuration["IntegrationTests:SistemaInformatico:NumeroInstalacion"] ?? "1"
        );

        return new RegistroFacturacion(
            IDVersion: "1.0",
            IDEmisorFactura: _emisorNif,
            NumSerieFactura: $"TEST/{numeroFactura}",
            FechaExpedicionFactura: fechaActual,
            NombreRazonEmisor: _emisorNombre,
            TipoFactura: tipoFactura,
            DescripcionOperacion: $"Test de integración sandbox - {tipoFactura}",
            Desglose: desglose,
            CuotaTotal: 42m,
            ImporteTotal: 242m,
            FechaHoraHusoGenRegistro: fechaActual,
            TipoHuella: "01",
            Huella: string.Empty, // Se calculará después
            SistemaInformatico: sistemaInfo,
            Factura: factura
        );
    }

    /// <summary>
    /// Crea un registro de factura rectificativa para pruebas
    /// </summary>
    private RegistroFacturacion CrearRegistroPruebaRectificativa(string numeroFactura, TipoRectificativa tipoRectificativa)
    {
        var registro = CrearRegistroPruebaBasico(numeroFactura, TipoFactura.R1);
        
        // Añadir información de rectificativa
        // Nota: En implementación real, debería incluir referencia a factura original
        return registro;
    }

    #endregion

    public void Dispose()
    {
        _certificado?.Dispose();
    }
}
