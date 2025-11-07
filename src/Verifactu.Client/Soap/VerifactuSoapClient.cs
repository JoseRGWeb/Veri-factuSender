using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Verifactu.Client.Models;

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
    /// <exception cref="HttpRequestException">Error de comunicación HTTP</exception>
    /// <exception cref="TimeoutException">Timeout en la operación</exception>
    /// <exception cref="OperationCanceledException">Operación cancelada</exception>
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
    /// 
    /// TIMEOUT Y REINTENTOS:
    /// - Timeout predeterminado: 120 segundos
    /// - Se recomienda implementar lógica de reintento con backoff exponencial
    ///   para errores de red transitorios (5xx, timeouts)
    /// - No reintentar errores de validación (4xx, SOAP Fault)
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
        
        // Configurar timeout de 120 segundos (recomendado por AEAT)
        http.Timeout = TimeSpan.FromSeconds(120);

        try
        {
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
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            // Timeout de la operación HTTP (no cancelación explícita)
            throw new TimeoutException($"Timeout al enviar registro a AEAT. Endpoint: {_endpointUrl}", ex);
        }
        catch (HttpRequestException ex)
        {
            // Error de red o HTTP (400, 500, etc.)
            throw new HttpRequestException($"Error HTTP al enviar registro a AEAT. Endpoint: {_endpointUrl}. Detalles: {ex.Message}", ex);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Operación cancelada explícitamente
            throw;
        }
    }

    /// <summary>
    /// Envía un registro de facturación firmado (alta o anulación) y parsea la respuesta automáticamente.
    /// </summary>
    /// <param name="xmlFirmado">Documento XML firmado del registro de facturación</param>
    /// <param name="cert">Certificado digital X.509 para mTLS</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Respuesta parseada de la operación RegFacturacionAlta</returns>
    /// <exception cref="HttpRequestException">Error de comunicación HTTP</exception>
    /// <exception cref="TimeoutException">Timeout en la operación</exception>
    /// <exception cref="ArgumentException">Respuesta SOAP inválida</exception>
    public async Task<RespuestaSuministro> EnviarRegFacturacionAltaAsync(
        XmlDocument xmlFirmado, 
        X509Certificate2 cert, 
        CancellationToken ct = default)
    {
        var responseXml = await EnviarRegistroAsync(xmlFirmado, cert, ct);
        return ParsearRespuestaSuministro(responseXml);
    }

    /// <summary>
    /// Realiza una consulta de registros de facturación y parsea la respuesta automáticamente.
    /// </summary>
    /// <param name="xmlConsulta">Documento XML de consulta</param>
    /// <param name="cert">Certificado digital X.509 para mTLS</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Respuesta parseada de la operación ConsultaLRFacturas</returns>
    /// <exception cref="HttpRequestException">Error de comunicación HTTP</exception>
    /// <exception cref="TimeoutException">Timeout en la operación</exception>
    /// <exception cref="ArgumentException">Respuesta SOAP inválida</exception>
    public async Task<RespuestaConsultaLR> ConsultarLRFacturasAsync(
        XmlDocument xmlConsulta, 
        X509Certificate2 cert, 
        CancellationToken ct = default)
    {
        var responseXml = await EnviarRegistroAsync(xmlConsulta, cert, ct);
        return ParsearRespuestaConsultaLR(responseXml);
    }

    /// <summary>
    /// Construye el sobre SOAP 1.1 para la operación RegFacturacionAlta
    /// según el WSDL oficial de AEAT.
    /// </summary>
    /// <param name="payload">Documento XML del registro de facturación (ya firmado)</param>
    /// <returns>Mensaje SOAP completo listo para enviar</returns>
    /// <remarks>
    /// ESTRUCTURA SOAP 1.1 según WSDL oficial:
    /// - Namespace SOAP: http://schemas.xmlsoap.org/soap/envelope/
    /// - Namespace SuministroLR: https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroLR.xsd
    /// - Namespace SuministroInformacion: https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd
    /// - Encoding: UTF-8
    /// 
    /// Referencia WSDL: docs/wsdl/README.md
    /// Documentación: docs/Veri-Factu_Descripcion_SWeb.md (Anexo II)
    /// </remarks>
    private static string ConstruirSobreSoap(XmlDocument payload)
    {
        // Extraer el contenido del payload para insertarlo en la estructura SOAP correcta
        var payloadContent = payload.OuterXml;
        
        var sb = new StringBuilder();
        sb.Append("""
<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
                  xmlns:sfLR="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroLR.xsd"
                  xmlns:sf="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd">
  <soapenv:Header/>
  <soapenv:Body>
    
""");
        sb.Append(payloadContent);
        sb.Append("""

  </soapenv:Body>
</soapenv:Envelope>
""");
        return sb.ToString();
    }

    /// <summary>
    /// Parsea la respuesta XML del servicio SOAP de AEAT para operaciones de RegFacturacionAlta
    /// </summary>
    /// <param name="responseXml">XML de respuesta del servidor AEAT</param>
    /// <returns>Objeto RespuestaSuministro con los datos parseados</returns>
    /// <exception cref="ArgumentException">Si el XML de respuesta no es válido</exception>
    public static RespuestaSuministro ParsearRespuestaSuministro(string responseXml)
    {
        var doc = XDocument.Parse(responseXml);
        
        // Namespaces según esquema oficial
        XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace sf = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd";
        XNamespace sfResp = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaSuministro.xsd";

        var body = doc.Root?.Element(soapenv + "Body");
        if (body == null)
            throw new ArgumentException("Respuesta SOAP inválida: no se encontró Body");

        var respuesta = body.Element(sfResp + "RespuestaRegFactuSistemaFacturacion");
        if (respuesta == null)
            throw new ArgumentException("Respuesta SOAP inválida: no se encontró RespuestaRegFactuSistemaFacturacion");

        var result = new RespuestaSuministro
        {
            CSV = respuesta.Element(sfResp + "CSV")?.Value,
            TiempoEsperaEnvio = int.TryParse(respuesta.Element(sfResp + "TiempoEsperaEnvio")?.Value, out var tiempo) ? tiempo : null,
            EstadoEnvio = respuesta.Element(sfResp + "EstadoEnvio")?.Value,
            RespuestasLinea = new List<RespuestaLinea>()
        };

        // Parsear DatosPresentacion
        var datosPres = respuesta.Element(sfResp + "DatosPresentacion");
        if (datosPres != null)
        {
            result.DatosPresentacion = new DatosPresentacion
            {
                NIFPresentador = datosPres.Element(sf + "NIFPresentador")?.Value,
                TimestampPresentacion = DateTime.TryParse(datosPres.Element(sf + "TimestampPresentacion")?.Value, out var ts) ? ts : null
            };
        }

        // Parsear Cabecera
        var cabecera = respuesta.Element(sfResp + "Cabecera");
        if (cabecera != null)
        {
            var obligado = cabecera.Element(sf + "ObligadoEmision");
            if (obligado != null)
            {
                result.Cabecera = new CabeceraRespuesta
                {
                    ObligadoEmision = new ObligadoEmision
                    {
                        NombreRazon = obligado.Element(sf + "NombreRazon")?.Value,
                        NIF = obligado.Element(sf + "NIF")?.Value
                    }
                };
            }
        }

        // Parsear RespuestaLinea (puede haber múltiples)
        var respuestasLinea = respuesta.Elements(sfResp + "RespuestaLinea");
        foreach (var linea in respuestasLinea)
        {
            var respLinea = new RespuestaLinea
            {
                EstadoRegistro = linea.Element(sfResp + "EstadoRegistro")?.Value,
                CodigoErrorRegistro = linea.Element(sfResp + "CodigoErrorRegistro")?.Value,
                DescripcionErrorRegistro = linea.Element(sfResp + "DescripcionErrorRegistro")?.Value,
                RefExterna = linea.Element(sfResp + "RefExterna")?.Value
            };

            // IDFactura
            var idFactura = linea.Element(sfResp + "IDFactura");
            if (idFactura != null)
            {
                respLinea.IDFactura = new IDFactura
                {
                    IDEmisorFactura = idFactura.Element(sf + "IDEmisorFactura")?.Value,
                    NumSerieFactura = idFactura.Element(sf + "NumSerieFactura")?.Value,
                    FechaExpedicionFactura = DateTime.TryParse(idFactura.Element(sf + "FechaExpedicionFactura")?.Value, out var fecha) ? fecha : null
                };
            }

            // Operacion
            var operacion = linea.Element(sfResp + "Operacion");
            if (operacion != null)
            {
                respLinea.Operacion = new Operacion
                {
                    TipoOperacion = operacion.Element(sfResp + "TipoOperacion")?.Value,
                    Subsanacion = operacion.Element(sfResp + "Subsanacion")?.Value,
                    RechazoPrevio = operacion.Element(sfResp + "RechazoPrevio")?.Value,
                    SinRegistroPrevio = operacion.Element(sfResp + "SinRegistroPrevio")?.Value
                };
            }

            // RegistroDuplicado (si existe)
            var duplicado = linea.Element(sfResp + "RegistroDuplicado");
            if (duplicado != null)
            {
                respLinea.RegistroDuplicado = new RegistroDuplicado
                {
                    IdPeticionRegistroDuplicado = duplicado.Element(sfResp + "IdPeticionRegistroDuplicado")?.Value,
                    EstadoRegistroDuplicado = duplicado.Element(sfResp + "EstadoRegistroDuplicado")?.Value,
                    CodigoErrorRegistro = duplicado.Element(sfResp + "CodigoErrorRegistro")?.Value,
                    DescripcionErrorRegistro = duplicado.Element(sfResp + "DescripcionErrorRegistro")?.Value
                };
            }

            result.RespuestasLinea.Add(respLinea);
        }

        return result;
    }

    /// <summary>
    /// Parsea la respuesta XML del servicio SOAP de AEAT para operaciones de ConsultaLRFacturas
    /// </summary>
    /// <param name="responseXml">XML de respuesta del servidor AEAT</param>
    /// <returns>Objeto RespuestaConsultaLR con los datos parseados</returns>
    /// <exception cref="ArgumentException">Si el XML de respuesta no es válido</exception>
    public static RespuestaConsultaLR ParsearRespuestaConsultaLR(string responseXml)
    {
        var doc = XDocument.Parse(responseXml);
        
        // Namespaces según esquema oficial
        XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace sf = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd";
        XNamespace conResp = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaConsultaLR.xsd";

        var body = doc.Root?.Element(soapenv + "Body");
        if (body == null)
            throw new ArgumentException("Respuesta SOAP inválida: no se encontró Body");

        var respuesta = body.Element(conResp + "RespuestaConsultaFactuSistemaFacturacion");
        if (respuesta == null)
            throw new ArgumentException("Respuesta SOAP inválida: no se encontró RespuestaConsultaFactuSistemaFacturacion");

        var result = new RespuestaConsultaLR
        {
            IndicadorPaginacion = respuesta.Element(conResp + "IndicadorPaginacion")?.Value,
            ResultadoConsulta = respuesta.Element(conResp + "ResultadoConsulta")?.Value,
            RegistrosRespuesta = new List<RegistroRespuestaConsulta>()
        };

        // Parsear Cabecera
        var cabecera = respuesta.Element(conResp + "Cabecera");
        if (cabecera != null)
        {
            result.Cabecera = new CabeceraConsulta
            {
                IDVersion = cabecera.Element(sf + "IDVersion")?.Value,
                IndicadorRepresentante = cabecera.Element(sf + "IndicadorRepresentante")?.Value
            };
        }

        // Parsear PeriodoImputacion
        var periodo = respuesta.Element(conResp + "PeriodoImputacion");
        if (periodo != null)
        {
            result.PeriodoImputacion = new PeriodoImputacion
            {
                Ejercicio = int.TryParse(periodo.Element(conResp + "Ejercicio")?.Value, out var ej) ? ej : 0,
                Periodo = periodo.Element(conResp + "Periodo")?.Value
            };
        }

        // Parsear registros de respuesta
        var registros = respuesta.Elements(conResp + "RegistroRespuestaConsultaFactuSistemaFacturacion");
        foreach (var registro in registros)
        {
            var regResp = new RegistroRespuestaConsulta();

            // IDFactura
            var idFactura = registro.Element(conResp + "IDFactura");
            if (idFactura != null)
            {
                regResp.IDFactura = new IDFactura
                {
                    IDEmisorFactura = idFactura.Element(sf + "IDEmisorFactura")?.Value,
                    NumSerieFactura = idFactura.Element(sf + "NumSerieFactura")?.Value,
                    FechaExpedicionFactura = DateTime.TryParse(idFactura.Element(sf + "FechaExpedicionFactura")?.Value, out var f) ? f : null
                };
            }

            // DatosRegistroFacturacion (simplificado)
            var datos = registro.Element(conResp + "DatosRegistroFacturacion");
            if (datos != null)
            {
                regResp.DatosRegistroFacturacion = new DatosRegistroFacturacion
                {
                    TipoFactura = datos.Element(conResp + "TipoFactura")?.Value,
                    DescripcionOperacion = datos.Element(conResp + "DescripcionOperacion")?.Value,
                    CuotaTotal = decimal.TryParse(datos.Element(conResp + "CuotaTotal")?.Value, out var ct) ? ct : null,
                    ImporteTotal = decimal.TryParse(datos.Element(conResp + "ImporteTotal")?.Value, out var it) ? it : null,
                    Huella = datos.Element(conResp + "Huella")?.Value,
                    FechaHoraHusoGenRegistro = DateTime.TryParse(datos.Element(conResp + "FechaHoraHusoGenRegistro")?.Value, out var fh) ? fh : null
                };
            }

            result.RegistrosRespuesta.Add(regResp);
        }

        // ClavePaginacion (si existe)
        var clave = respuesta.Element(conResp + "ClavePaginacion");
        if (clave != null)
        {
            result.ClavePaginacion = new ClavePaginacion
            {
                IDEmisorFactura = clave.Element(sf + "IDEmisorFactura")?.Value,
                NumSerieFactura = clave.Element(sf + "NumSerieFactura")?.Value,
                FechaExpedicionFactura = DateTime.TryParse(clave.Element(sf + "FechaExpedicionFactura")?.Value, out var fc) ? fc : null
            };
        }

        return result;
    }
}
