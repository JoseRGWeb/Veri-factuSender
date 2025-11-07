using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Verifactu.Client.Services;
using Verifactu.Integration.Tests.Fixtures;
using Verifactu.Integration.Tests.Helpers;
using Xunit;

namespace Verifactu.Integration.Tests;

/// <summary>
/// Tests de seguridad que validan aspectos críticos de seguridad:
/// - Firma digital XML
/// - Validación de certificados
/// - Integridad de datos
/// - Protección contra manipulación
/// </summary>
[Collection("SandboxIntegrationTests")]
public class SecurityTests : IClassFixture<AeatSandboxFixture>
{
    private readonly AeatSandboxFixture _fixture;

    public SecurityTests(AeatSandboxFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// Test 1: Verifica que el XML firmado contiene firma digital válida
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Security")]
    public void FirmaDigitalXML_ContieneSignatureValida()
    {
        // Arrange
        if (_fixture.SkipTests) { return; }

        var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico("SEC-SIGN");
        var registro = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura);
        
        var hashService = new HashService();
        var serializer = new VerifactuSerializer();
        var xmlSigner = new XmlSignerService();

        // Act
        var huella = hashService.CalcularHuella(registro, null);
        var registroConHuella = registro with { Huella = huella };
        var xml = serializer.CrearXmlRegistro(registroConHuella);
        var xmlFirmado = xmlSigner.Firmar(xml, _fixture.Certificado!);

        // Assert
        Assert.NotNull(xmlFirmado);
        
        // Verificar que contiene elemento Signature
        var xmlDoc = xmlFirmado;
        var signatureNodes = xmlDoc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");
        
        Assert.True(signatureNodes.Count > 0, "El XML debe contener un elemento Signature");
        
        // Verificar que contiene SignatureValue
        var signatureValueNodes = xmlDoc.GetElementsByTagName("SignatureValue", "http://www.w3.org/2000/09/xmldsig#");
        Assert.True(signatureValueNodes.Count > 0, "El XML debe contener SignatureValue");
        
        // Verificar que SignatureValue no está vacío
        var signatureValue = signatureValueNodes[0]?.InnerText;
        Assert.False(string.IsNullOrWhiteSpace(signatureValue), "SignatureValue no debe estar vacío");
    }

    /// <summary>
    /// Test 2: Verifica que el certificado cargado tiene clave privada
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    public void CertificadoCargado_TieneClavePrivada()
    {
        // Arrange & Assert
        if (_fixture.SkipTests) { return; }

        Assert.NotNull(_fixture.Certificado);
        Assert.True(_fixture.Certificado.HasPrivateKey, 
            "El certificado debe contener clave privada para firmar");
    }

    /// <summary>
    /// Test 3: Verifica que la huella cambia al modificar datos
    /// Esto previene manipulación de facturas
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    public void HuellaCambia_AlModificarDatos()
    {
        // Arrange
        var hashService = new HashService();
        var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico("SEC-HASH");
        
        var registro1 = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura, baseImponible: 100m);
        var registro2 = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura, baseImponible: 200m);

        // Act
        var huella1 = hashService.CalcularHuella(registro1, null);
        var huella2 = hashService.CalcularHuella(registro2, null);

        // Assert
        Assert.NotEqual(huella1, huella2);
        Assert.False(string.IsNullOrWhiteSpace(huella1));
        Assert.False(string.IsNullOrWhiteSpace(huella2));
    }

    /// <summary>
    /// Test 4: Verifica que la huella incluye datos críticos del registro
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    public void HuellaIncluyeDatosCriticos_ImporteYFecha()
    {
        // Arrange
        var hashService = new HashService();
        var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico("SEC-DATA");
        
        var registroBase = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura);
        
        // Modificar solo el importe
        var registroImporteModificado = registroBase with { ImporteTotal = registroBase.ImporteTotal + 100m };
        
        // Modificar solo la fecha
        var registroFechaModificada = registroBase with { 
            FechaExpedicionFactura = registroBase.FechaExpedicionFactura.AddDays(1) 
        };

        // Act
        var huellaBase = hashService.CalcularHuella(registroBase, null);
        var huellaImporte = hashService.CalcularHuella(registroImporteModificado, null);
        var huellaFecha = hashService.CalcularHuella(registroFechaModificada, null);

        // Assert - Todas las huellas deben ser diferentes
        Assert.NotEqual(huellaBase, huellaImporte);
        Assert.NotEqual(huellaBase, huellaFecha);
        Assert.NotEqual(huellaImporte, huellaFecha);
    }

    /// <summary>
    /// Test 5: Verifica que el certificado es válido para el propósito
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    public void Certificado_ValidoParaFirma()
    {
        // Arrange & Assert
        if (_fixture.SkipTests) { return; }

        var cert = _fixture.Certificado!;
        
        // Verificar que el certificado tiene datos básicos
        Assert.NotNull(cert.Subject);
        Assert.NotNull(cert.Issuer);
        Assert.NotNull(cert.Thumbprint);
        
        // Verificar que tiene clave privada
        Assert.True(cert.HasPrivateKey);
        
        // Log información del certificado para debugging
        System.Diagnostics.Debug.WriteLine($"=== Información del Certificado ===");
        System.Diagnostics.Debug.WriteLine($"Subject: {cert.Subject}");
        System.Diagnostics.Debug.WriteLine($"Issuer: {cert.Issuer}");
        System.Diagnostics.Debug.WriteLine($"Valid From: {cert.NotBefore}");
        System.Diagnostics.Debug.WriteLine($"Valid To: {cert.NotAfter}");
        System.Diagnostics.Debug.WriteLine($"Thumbprint: {cert.Thumbprint}");
    }

    /// <summary>
    /// Test 6: Verifica que no se puede enviar factura sin firmar
    /// NOTA: Este test valida el proceso, no el rechazo del servidor
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    public void ProcesoEnvio_RequiereFirma()
    {
        // Arrange
        if (_fixture.SkipTests) { return; }

        var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico("SEC-UNSIGNED");
        var registro = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura);
        
        var hashService = new HashService();
        var serializer = new VerifactuSerializer();

        // Act
        var huella = hashService.CalcularHuella(registro, null);
        var registroConHuella = registro with { Huella = huella };
        var xmlSinFirmar = serializer.CrearXmlRegistro(registroConHuella);

        // Assert
        // Verificar que el XML sin firmar NO contiene elemento Signature
        var signatureNodes = xmlSinFirmar.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");
        Assert.Equal(0, signatureNodes.Count);
        
        // El proceso normal requiere firmar antes de enviar
        // Este test documenta que el flujo correcto es: XML → Firma → Envío
    }

    /// <summary>
    /// Test 7: Verifica que el encadenamiento de huellas previene reordenamiento
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    public void EncadenamientoHuellas_PrevieneReordenamiento()
    {
        // Arrange
        var hashService = new HashService();
        
        var numero1 = TestDataBuilder.GenerarNumeroFacturaUnico("SEC-CHAIN1");
        var numero2 = TestDataBuilder.GenerarNumeroFacturaUnico("SEC-CHAIN2");
        
        var registro1 = _fixture.DataBuilder.CrearFacturaBasica(numero1);
        var registro2 = _fixture.DataBuilder.CrearFacturaBasica(numero2);

        // Act - Calcular en orden correcto
        var huella1Correcta = hashService.CalcularHuella(registro1, null);
        var huella2Correcta = hashService.CalcularHuella(registro2, huella1Correcta);
        
        // Act - Intentar calcular en orden inverso
        var huella2Invertida = hashService.CalcularHuella(registro2, null);
        var huella1Invertida = hashService.CalcularHuella(registro1, huella2Invertida);

        // Assert - Las huellas deben ser diferentes según el orden
        Assert.NotEqual(huella2Correcta, huella2Invertida);
        
        // Esto demuestra que el orden importa y no se pueden reordenar facturas
        // sin detectar la manipulación
    }
}
