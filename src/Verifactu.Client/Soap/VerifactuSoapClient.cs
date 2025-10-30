using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Verifactu.Client.Services;

/// <summary>
/// Cliente SOAP para comunicación con los servicios web VERI*FACTU de la AEAT.
/// Implementa autenticación mutua TLS (mTLS) mediante certificados digitales X.509.
/// </summary>
/// <remarks>
/// PROTOCOLOS DE COMUNICACIÓN:
/// - Protocolo de transporte: HTTPS sobre TLS 1.2 o superior
/// - Arquitectura: Servicios web SOAP 1.1
/// - Autenticación: TLS mutuo (mutual TLS) con certificados digitales
/// - Formato de datos: XML conforme a esquemas XSD de AEAT
/// - Puerto: 443 (HTTPS estándar)
/// - Encoding: UTF-8
/// 
/// Para más detalles sobre los protocolos de comunicación, consultar:
/// docs/protocolos-comunicacion.md
/// </remarks>
public class VerifactuSoapClient : IVerifactuSoapClient
{
    private readonly string _endpointUrl;
    private readonly string _soapAction; // Acción SOAP según WSDL (ej: "RegFacturacionAlta")

    /// <summary>
    /// Inicializa una nueva instancia del cliente SOAP VERI*FACTU.
    /// </summary>
    /// <param name="endpointUrl">URL del endpoint del servicio SOAP de AEAT.
    /// Sandbox: https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion
    /// Producción: https://www1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion</param>
    /// <param name="soapAction">Acción SOAP a ejecutar (ej: "RegFacturacionAlta", "ConsultaLRFacturas")</param>
    public VerifactuSoapClient(string endpointUrl, string soapAction)
    {
        _endpointUrl = endpointUrl;
        _soapAction = soapAction;
    }

    /// <summary>
    /// Envía un registro de facturación firmado al servicio VERI*FACTU de la AEAT
    /// utilizando autenticación mutua TLS (mTLS).
    /// </summary>
    /// <param name="xmlFirmado">Documento XML firmado digitalmente (XMLDSig) del registro de facturación</param>
    /// <param name="cert">Certificado digital X.509 del representante para autenticación mTLS.
    /// Debe incluir la clave privada y estar en formato PFX/PKCS#12.</param>
    /// <param name="ct">Token de cancelación para la operación asíncrona</param>
    /// <returns>Respuesta XML del servidor AEAT (RespuestaSuministro)</returns>
    /// <remarks>
    /// FLUJO DE AUTENTICACIÓN mTLS:
    /// 1. Cliente inicia handshake TLS con servidor AEAT
    /// 2. Servidor envía su certificado SSL/TLS
    /// 3. Cliente valida certificado del servidor
    /// 4. Servidor solicita certificado de cliente
    /// 5. Cliente envía certificado de representante (parámetro 'cert')
    /// 6. Servidor valida certificado del cliente (NIF, vigencia, revocación)
    /// 7. Si ambos certificados son válidos, se establece canal seguro
    /// 8. Intercambio de mensajes SOAP sobre canal TLS
    /// 
    /// REQUISITOS DEL CERTIFICADO:
    /// - Tipo: Certificado de representante de persona jurídica
    /// - Formato: PFX/PKCS#12 con clave privada
    /// - Algoritmo: RSA 2048+ bits o ECDSA 256+ bits
    /// - Estado: Válido, no caducado, no revocado
    /// - NIF del certificado debe coincidir con NIF del emisor de facturas
    /// 
    /// HEADERS HTTP ENVIADOS:
    /// - Content-Type: text/xml; charset=utf-8
    /// - SOAPAction: [valor configurado en constructor]
    /// - User-Agent: (implícito por HttpClient)
    /// </remarks>
    public async Task<string> EnviarRegistroAsync(XmlDocument xmlFirmado, X509Certificate2 cert, CancellationToken ct = default)
    {
        // Configurar HttpClientHandler con autenticación mutua TLS (mTLS)
        // El certificado del cliente se utiliza para autenticar al cliente ante el servidor AEAT
        // durante el handshake TLS. Esto es obligatorio para los servicios VERI*FACTU.
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(cert); // Agregar certificado para autenticación mTLS
        
        // NOTA IMPORTANTE: .NET 9 utiliza TLS 1.2+ por defecto
        // Si se requiere configuración explícita de TLS:
        // handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
        
        using var http = new HttpClient(handler);

        // Construir sobre SOAP 1.1 con el registro XML firmado
        var soapEnvelope = ConstruirSobreSoap(xmlFirmado);
        
        // Preparar contenido HTTP con encoding UTF-8 y Content-Type text/xml
        var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        
        // Agregar header SOAPAction si está configurado (requerido por SOAP 1.1)
        // El valor debe coincidir con la operación definida en el WSDL de AEAT
        if (!string.IsNullOrWhiteSpace(_soapAction))
        {
            content.Headers.Add("SOAPAction", _soapAction);
        }

        // Enviar petición POST al endpoint de AEAT
        // La conexión se establece sobre TLS 1.2+ con autenticación mutua
        using var resp = await http.PostAsync(_endpointUrl, content, ct);
        
        // Verificar que la respuesta sea exitosa (2xx)
        // Si hay error, lanza HttpRequestException con el código de estado
        resp.EnsureSuccessStatusCode();
        
        // Retornar el cuerpo de la respuesta (XML de respuesta SOAP)
        return await resp.Content.ReadAsStringAsync(ct);
    }

    /// <summary>
    /// Construye el sobre SOAP 1.1 que envuelve el registro de facturación XML.
    /// </summary>
    /// <param name="payload">Documento XML del registro de facturación (ya firmado)</param>
    /// <returns>Mensaje SOAP completo listo para enviar</returns>
    /// <remarks>
    /// ESTRUCTURA SOAP 1.1:
    /// - Namespace: http://schemas.xmlsoap.org/soap/envelope/
    /// - Encoding: UTF-8
    /// - El payload (registro firmado) se incluye dentro del Body
    /// 
    /// IMPORTANTE: Esta es una implementación placeholder.
    /// En producción debe ajustarse al WSDL oficial de AEAT:
    /// https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl
    /// 
    /// La estructura real debe incluir:
    /// - Namespace correcto del servicio AEAT (xmlns:sfe=...)
    /// - Elemento de operación correcto (SuministroLRFacturasEmitidas, etc.)
    /// - Cabecera con IDVersion, Titular, etc.
    /// </remarks>
    private static string ConstruirSobreSoap(XmlDocument payload)
    {
        // SOAP 1.1 simplificado - PLACEHOLDER
        // TODO: Reemplazar con estructura conforme al WSDL oficial de AEAT
        var sb = new StringBuilder();
        sb.Append("""
<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
  <soapenv:Header/>
  <soapenv:Body>
    <EnviarRegistroRequest xmlns="urn:aeat:verifactu:placeholder">
      <RegistroXml>
""");
        sb.Append(payload.OuterXml);
        sb.Append("""
      </RegistroXml>
    </EnviarRegistroRequest>
  </soapenv:Body>
</soapenv:Envelope>
""");
        return sb.ToString();
    }
}
