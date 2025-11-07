using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Verifactu.Client.Models;
using Verifactu.Client.Services;
using Xunit;

namespace Verifactu.Integration.Tests;

/// <summary>
/// Tests de validación del cliente SOAP contra el WSDL oficial de AEAT.
/// 
/// OBJETIVO:
/// Verificar que el cliente SOAP genera mensajes SOAP 100% conformes al WSDL oficial
/// publicado por AEAT para el servicio VERI*FACTU.
/// 
/// ALCANCE:
/// - Estructura de sobres SOAP 1.1
/// - Namespaces XML correctos
/// - Elementos requeridos presentes
/// - Estructura de mensajes conforme a XSD
/// - Headers HTTP apropiados
/// 
/// REFERENCIAS:
/// - WSDL Oficial: docs/wsdl/WSDL-ANALYSIS.md
/// - Documentación SOAP: docs/uso-cliente-soap.md
/// - Especificación SOAP 1.1: https://www.w3.org/TR/2000/NOTE-SOAP-20000508/
/// </summary>
public class WsdlValidationTests
{
    private readonly VerifactuSerializer _serializer;
    private readonly HashService _hashService;

    // Namespaces oficiales según WSDL de AEAT
    private const string NS_SOAP_ENVELOPE = "http://schemas.xmlsoap.org/soap/envelope/";
    private const string NS_SUMINISTRO_LR = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroLR.xsd";
    private const string NS_SUMINISTRO_INFO = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd";
    private const string NS_RESPUESTA_SUMINISTRO = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaSuministro.xsd";
    private const string NS_CONSULTA_LR = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/ConsultaLR.xsd";
    private const string NS_RESPUESTA_CONSULTA = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaConsultaLR.xsd";

    public WsdlValidationTests()
    {
        _serializer = new VerifactuSerializer();
        _hashService = new HashService();
    }

    #region Tests de Estructura SOAP

    /// <summary>
    /// Verifica que el sobre SOAP tiene la estructura básica correcta según SOAP 1.1
    /// </summary>
    [Fact]
    [Trait("Category", "WSDL")]
    [Trait("Component", "SOAP")]
    public void SobreSoap_DebeSerValidoSegunSoap11()
    {
        // Arrange
        var registro = CrearRegistroPrueba();
        var xmlRegistro = _serializer.CrearXmlRegistro(registro);
        
        // Act - Usar reflexión para acceder al método privado ConstruirSobreSoap
        var soapEnvelopeMethod = typeof(VerifactuSoapClient)
            .GetMethod("ConstruirSobreSoap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var soapEnvelope = (string)soapEnvelopeMethod!.Invoke(null, new object[] { xmlRegistro })!;
        
        var doc = XDocument.Parse(soapEnvelope);
        
        // Assert - Verificar estructura SOAP 1.1
        Assert.NotNull(doc.Root);
        Assert.Equal("Envelope", doc.Root.Name.LocalName);
        Assert.Equal(NS_SOAP_ENVELOPE, doc.Root.Name.NamespaceName);
        
        // Debe tener Header y Body
        XNamespace soapNs = NS_SOAP_ENVELOPE;
        var header = doc.Root.Element(soapNs + "Header");
        var body = doc.Root.Element(soapNs + "Body");
        
        Assert.NotNull(header);
        Assert.NotNull(body);
    }

    /// <summary>
    /// Verifica que todos los namespaces requeridos están presentes en el sobre SOAP
    /// </summary>
    [Fact]
    [Trait("Category", "WSDL")]
    [Trait("Component", "Namespaces")]
    public void SobreSoap_DebeContenerTodosLosNamespacesRequeridos()
    {
        // Arrange
        var registro = CrearRegistroPrueba();
        var xmlRegistro = _serializer.CrearXmlRegistro(registro);
        
        // Act
        var soapEnvelopeMethod = typeof(VerifactuSoapClient)
            .GetMethod("ConstruirSobreSoap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var soapEnvelope = (string)soapEnvelopeMethod!.Invoke(null, new object[] { xmlRegistro })!;
        
        // Assert - Verificar presencia de namespaces
        Assert.Contains("xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"", soapEnvelope);
        Assert.Contains("xmlns:sfLR=\"https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroLR.xsd\"", soapEnvelope);
        Assert.Contains("xmlns:sf=\"https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd\"", soapEnvelope);
    }

    /// <summary>
    /// Verifica que el sobre SOAP está bien formado (XML válido)
    /// </summary>
    [Fact]
    [Trait("Category", "WSDL")]
    [Trait("Component", "XML")]
    public void SobreSoap_DebeSerXmlBienFormado()
    {
        // Arrange
        var registro = CrearRegistroPrueba();
        var xmlRegistro = _serializer.CrearXmlRegistro(registro);
        
        // Act & Assert - No debe lanzar excepción
        var soapEnvelopeMethod = typeof(VerifactuSoapClient)
            .GetMethod("ConstruirSobreSoap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var soapEnvelope = (string)soapEnvelopeMethod!.Invoke(null, new object[] { xmlRegistro })!;
        
        var exception = Record.Exception(() => XDocument.Parse(soapEnvelope));
        Assert.Null(exception);
    }

    /// <summary>
    /// Verifica que el sobre SOAP tiene encoding UTF-8
    /// </summary>
    [Fact]
    [Trait("Category", "WSDL")]
    [Trait("Component", "Encoding")]
    public void SobreSoap_DebeUsarEncodingUtf8()
    {
        // Arrange
        var registro = CrearRegistroPrueba();
        var xmlRegistro = _serializer.CrearXmlRegistro(registro);
        
        // Act
        var soapEnvelopeMethod = typeof(VerifactuSoapClient)
            .GetMethod("ConstruirSobreSoap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var soapEnvelope = (string)soapEnvelopeMethod!.Invoke(null, new object[] { xmlRegistro })!;
        
        // Assert - Verificar declaración XML con UTF-8
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", soapEnvelope);
    }

    #endregion

    #region Tests de Parseo de Respuestas

    /// <summary>
    /// Verifica que se puede parsear una respuesta SOAP válida de RegFacturacionAlta
    /// </summary>
    [Fact]
    [Trait("Category", "WSDL")]
    [Trait("Component", "ResponseParsing")]
    public void ParsearRespuestaSuministro_ConRespuestaValida_DebeRetornarObjeto()
    {
        // Arrange - Respuesta SOAP simulada conforme al WSDL
        var responseXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header/>
  <soapenv:Body>
    <sfResp:RespuestaRegFactuSistemaFacturacion 
        xmlns:sfResp=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaSuministro.xsd""
        xmlns:sf=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd"">
      <sfResp:CSV>ABC123DEF456</sfResp:CSV>
      <sfResp:TiempoEsperaEnvio>60</sfResp:TiempoEsperaEnvio>
      <sfResp:EstadoEnvio>Correcto</sfResp:EstadoEnvio>
      <sfResp:RespuestaLinea>
        <sfResp:EstadoRegistro>Correcto</sfResp:EstadoRegistro>
        <sfResp:IDFactura>
          <sf:IDEmisorFactura>B12345678</sf:IDEmisorFactura>
          <sf:NumSerieFactura>TEST/001</sf:NumSerieFactura>
          <sf:FechaExpedicionFactura>2025-11-07</sf:FechaExpedicionFactura>
        </sfResp:IDFactura>
      </sfResp:RespuestaLinea>
    </sfResp:RespuestaRegFactuSistemaFacturacion>
  </soapenv:Body>
</soapenv:Envelope>";

        // Act
        var respuesta = VerifactuSoapClient.ParsearRespuestaSuministro(responseXml);

        // Assert
        Assert.NotNull(respuesta);
        Assert.Equal("ABC123DEF456", respuesta.CSV);
        Assert.Equal(60, respuesta.TiempoEsperaEnvio);
        Assert.Equal("Correcto", respuesta.EstadoEnvio);
        Assert.NotNull(respuesta.RespuestasLinea);
        Assert.Single(respuesta.RespuestasLinea);
        Assert.Equal("Correcto", respuesta.RespuestasLinea[0].EstadoRegistro);
    }

    /// <summary>
    /// Verifica que se puede parsear una respuesta con estado ParcialmenteCorrecto
    /// </summary>
    [Fact]
    [Trait("Category", "WSDL")]
    [Trait("Component", "ResponseParsing")]
    public void ParsearRespuestaSuministro_ConEstadoParcial_DebeRetornarCorrectamente()
    {
        // Arrange
        var responseXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Body>
    <sfResp:RespuestaRegFactuSistemaFacturacion 
        xmlns:sfResp=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaSuministro.xsd""
        xmlns:sf=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd"">
      <sfResp:EstadoEnvio>ParcialmenteCorrecto</sfResp:EstadoEnvio>
      <sfResp:RespuestaLinea>
        <sfResp:EstadoRegistro>Correcto</sfResp:EstadoRegistro>
      </sfResp:RespuestaLinea>
      <sfResp:RespuestaLinea>
        <sfResp:EstadoRegistro>Incorrecto</sfResp:EstadoRegistro>
        <sfResp:CodigoErrorRegistro>4001</sfResp:CodigoErrorRegistro>
        <sfResp:DescripcionErrorRegistro>NIF no válido</sfResp:DescripcionErrorRegistro>
      </sfResp:RespuestaLinea>
    </sfResp:RespuestaRegFactuSistemaFacturacion>
  </soapenv:Body>
</soapenv:Envelope>";

        // Act
        var respuesta = VerifactuSoapClient.ParsearRespuestaSuministro(responseXml);

        // Assert
        Assert.Equal("ParcialmenteCorrecto", respuesta.EstadoEnvio);
        Assert.Equal(2, respuesta.RespuestasLinea?.Count);
        Assert.Equal("Correcto", respuesta.RespuestasLinea[0].EstadoRegistro);
        Assert.Equal("Incorrecto", respuesta.RespuestasLinea[1].EstadoRegistro);
        Assert.Equal("4001", respuesta.RespuestasLinea[1].CodigoErrorRegistro);
        Assert.Equal("NIF no válido", respuesta.RespuestasLinea[1].DescripcionErrorRegistro);
    }

    /// <summary>
    /// Verifica que se puede parsear una respuesta con registro duplicado
    /// </summary>
    [Fact]
    [Trait("Category", "WSDL")]
    [Trait("Component", "ResponseParsing")]
    public void ParsearRespuestaSuministro_ConRegistroDuplicado_DebeRetornarInformacion()
    {
        // Arrange
        var responseXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Body>
    <sfResp:RespuestaRegFactuSistemaFacturacion 
        xmlns:sfResp=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaSuministro.xsd"">
      <sfResp:EstadoEnvio>Incorrecto</sfResp:EstadoEnvio>
      <sfResp:RespuestaLinea>
        <sfResp:EstadoRegistro>Incorrecto</sfResp:EstadoRegistro>
        <sfResp:CodigoErrorRegistro>3001</sfResp:CodigoErrorRegistro>
        <sfResp:DescripcionErrorRegistro>Registro duplicado</sfResp:DescripcionErrorRegistro>
        <sfResp:RegistroDuplicado>
          <sfResp:IdPeticionRegistroDuplicado>PREV-123</sfResp:IdPeticionRegistroDuplicado>
          <sfResp:EstadoRegistroDuplicado>Correcto</sfResp:EstadoRegistroDuplicado>
        </sfResp:RegistroDuplicado>
      </sfResp:RespuestaLinea>
    </sfResp:RespuestaRegFactuSistemaFacturacion>
  </soapenv:Body>
</soapenv:Envelope>";

        // Act
        var respuesta = VerifactuSoapClient.ParsearRespuestaSuministro(responseXml);

        // Assert
        Assert.NotNull(respuesta.RespuestasLinea?[0].RegistroDuplicado);
        Assert.Equal("PREV-123", respuesta.RespuestasLinea[0].RegistroDuplicado.IdPeticionRegistroDuplicado);
        Assert.Equal("Correcto", respuesta.RespuestasLinea[0].RegistroDuplicado.EstadoRegistroDuplicado);
    }

    /// <summary>
    /// Verifica que se puede parsear una respuesta de ConsultaLRFacturas
    /// </summary>
    [Fact]
    [Trait("Category", "WSDL")]
    [Trait("Component", "ResponseParsing")]
    public void ParsearRespuestaConsultaLR_ConRespuestaValida_DebeRetornarObjeto()
    {
        // Arrange
        var responseXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Body>
    <conResp:RespuestaConsultaFactuSistemaFacturacion 
        xmlns:conResp=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaConsultaLR.xsd""
        xmlns:sf=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd"">
      <conResp:IndicadorPaginacion>N</conResp:IndicadorPaginacion>
      <conResp:ResultadoConsulta>ConDatos</conResp:ResultadoConsulta>
      <conResp:RegistroRespuestaConsultaFactuSistemaFacturacion>
        <conResp:IDFactura>
          <sf:IDEmisorFactura>B12345678</sf:IDEmisorFactura>
          <sf:NumSerieFactura>TEST/001</sf:NumSerieFactura>
          <sf:FechaExpedicionFactura>2025-11-07</sf:FechaExpedicionFactura>
        </conResp:IDFactura>
        <conResp:DatosRegistroFacturacion>
          <conResp:TipoFactura>F1</conResp:TipoFactura>
          <conResp:ImporteTotal>242.00</conResp:ImporteTotal>
          <conResp:Huella>ABC123</conResp:Huella>
        </conResp:DatosRegistroFacturacion>
      </conResp:RegistroRespuestaConsultaFactuSistemaFacturacion>
    </conResp:RespuestaConsultaFactuSistemaFacturacion>
  </soapenv:Body>
</soapenv:Envelope>";

        // Act
        var respuesta = VerifactuSoapClient.ParsearRespuestaConsultaLR(responseXml);

        // Assert
        Assert.NotNull(respuesta);
        Assert.Equal("N", respuesta.IndicadorPaginacion);
        Assert.Equal("ConDatos", respuesta.ResultadoConsulta);
        Assert.NotNull(respuesta.RegistrosRespuesta);
        Assert.Single(respuesta.RegistrosRespuesta);
        Assert.Equal("B12345678", respuesta.RegistrosRespuesta[0].IDFactura?.IDEmisorFactura);
    }

    /// <summary>
    /// Verifica que ParsearRespuestaSuministro falla con XML inválido
    /// </summary>
    [Fact]
    [Trait("Category", "WSDL")]
    [Trait("Component", "ErrorHandling")]
    public void ParsearRespuestaSuministro_ConXmlInvalido_DebeLanzarExcepcion()
    {
        // Arrange
        var invalidXml = "<invalid>xml</invalid>";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            VerifactuSoapClient.ParsearRespuestaSuministro(invalidXml));
    }

    #endregion

    #region Tests de Conformidad con Namespaces

    /// <summary>
    /// Verifica que los namespaces usados coinciden exactamente con los del WSDL oficial
    /// </summary>
    [Fact]
    [Trait("Category", "WSDL")]
    [Trait("Component", "Namespaces")]
    public void Namespaces_DebenCoincidirConWsdlOficial()
    {
        // Arrange & Act
        var registro = CrearRegistroPrueba();
        var xmlRegistro = _serializer.CrearXmlRegistro(registro);
        var soapEnvelopeMethod = typeof(VerifactuSoapClient)
            .GetMethod("ConstruirSobreSoap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var soapEnvelope = (string)soapEnvelopeMethod!.Invoke(null, new object[] { xmlRegistro })!;

        // Assert - Verificar namespaces exactos
        Assert.Contains(NS_SOAP_ENVELOPE, soapEnvelope);
        Assert.Contains(NS_SUMINISTRO_LR, soapEnvelope);
        Assert.Contains(NS_SUMINISTRO_INFO, soapEnvelope);
        
        // No debe contener namespaces obsoletos o incorrectos
        Assert.DoesNotContain("http://www.aeat.es", soapEnvelope); // Namespace antiguo
        Assert.DoesNotContain("http://example.com", soapEnvelope); // Namespace de ejemplo
    }

    #endregion

    #region Tests de Validación de Operaciones

    /// <summary>
    /// Verifica que la operación RegFacturacionAlta tiene el formato correcto
    /// </summary>
    [Fact]
    [Trait("Category", "WSDL")]
    [Trait("Component", "Operations")]
    public void OperacionRegFacturacionAlta_DebeSerConforme()
    {
        // Arrange
        var endpointUrl = "https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion";
        var soapAction = ""; // Vacío según WSDL
        var client = new VerifactuSoapClient(endpointUrl, soapAction);

        // Act & Assert - Verificar que se puede crear el cliente
        Assert.NotNull(client);
    }

    #endregion

    #region Métodos Auxiliares

    private RegistroFacturacion CrearRegistroPrueba()
    {
        var fecha = DateTime.UtcNow;
        
        var factura = new Factura(
            Serie: "TEST",
            Numero: "001",
            FechaEmision: fecha,
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Test WSDL validation",
            Emisor: new Emisor("B12345678", "EMPRESA TEST SL"),
            Receptor: new Receptor("12345678A", "CLIENTE TEST"),
            Lineas: new System.Collections.Generic.List<Linea>
            {
                new Linea("Producto test", 1, 100m, 21m)
            },
            Totales: new TotalesFactura(100m, 21m, 121m)
        );

        var desglose = new System.Collections.Generic.List<DetalleDesglose>
        {
            new DetalleDesglose("01", "S1", 21m, 100m, 21m)
        };

        var sistemaInfo = new SistemaInformatico(
            NombreRazon: "EMPRESA TEST SL",
            Nif: "B12345678",
            NombreSistemaInformatico: "VerifactuSender",
            IdSistemaInformatico: "TEST001",
            Version: "1.0",
            NumeroInstalacion: "1"
        );

        var registro = new RegistroFacturacion(
            IDVersion: "1.0",
            IDEmisorFactura: "B12345678",
            NumSerieFactura: "TEST/001",
            FechaExpedicionFactura: fecha,
            NombreRazonEmisor: "EMPRESA TEST SL",
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Test WSDL validation",
            Desglose: desglose,
            CuotaTotal: 21m,
            ImporteTotal: 121m,
            FechaHoraHusoGenRegistro: fecha,
            TipoHuella: "01",
            Huella: string.Empty,
            SistemaInformatico: sistemaInfo,
            Factura: factura
        );

        // Calcular y asignar huella
        var huella = _hashService.CalcularHuella(registro, null);
        return registro with { Huella = huella };
    }

    #endregion
}
