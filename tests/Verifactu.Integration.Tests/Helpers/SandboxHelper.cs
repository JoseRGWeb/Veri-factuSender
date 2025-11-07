using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Verifactu.Client.Models;
using Verifactu.Client.Services;

namespace Verifactu.Integration.Tests.Helpers;

/// <summary>
/// Helper para interacciones con el sandbox de AEAT.
/// Facilita operaciones comunes en tests de integración.
/// </summary>
public class SandboxHelper : IDisposable
{
    private readonly HashService _hashService;
    private readonly XmlSignerService _xmlSignerService;
    private readonly VerifactuSerializer _serializer;
    private readonly VerifactuSoapClient _soapClient;
    private readonly X509Certificate2 _certificado;
    private string? _ultimaHuella;

    public SandboxHelper(
        string endpointUrl,
        X509Certificate2 certificado)
    {
        _hashService = new HashService();
        _xmlSignerService = new XmlSignerService();
        _serializer = new VerifactuSerializer();
        _soapClient = new VerifactuSoapClient(endpointUrl, "RegFacturacionAlta");
        _certificado = certificado;
    }

    /// <summary>
    /// Envía un registro completo al sandbox: calcula huella, serializa, firma y envía
    /// </summary>
    public async Task<RespuestaSuministro> EnviarRegistroCompletoAsync(
        RegistroFacturacion registro,
        CancellationToken cancellationToken = default)
    {
        // Calcular huella
        var huella = _hashService.CalcularHuella(registro, _ultimaHuella);
        var registroConHuella = registro with { Huella = huella };

        // Serializar a XML
        var xml = _serializer.CrearXmlRegistro(registroConHuella);

        // Firmar XML
        var xmlFirmado = _xmlSignerService.Firmar(xml, _certificado);

        // Enviar a AEAT
        var respuestaXml = await _soapClient.EnviarRegistroAsync(xmlFirmado, _certificado, cancellationToken);
        var respuesta = VerifactuSoapClient.ParsearRespuestaSuministro(respuestaXml);

        // Actualizar última huella si el envío fue exitoso
        if (respuesta.EstadoEnvio == "Correcto")
        {
            _ultimaHuella = huella;
        }

        return respuesta;
    }

    /// <summary>
    /// Calcula la huella de un registro sin enviarlo
    /// </summary>
    public string CalcularHuella(RegistroFacturacion registro)
    {
        return _hashService.CalcularHuella(registro, _ultimaHuella);
    }

    /// <summary>
    /// Prepara un registro con huella calculada
    /// </summary>
    public RegistroFacturacion PrepararRegistroConHuella(RegistroFacturacion registro)
    {
        var huella = _hashService.CalcularHuella(registro, _ultimaHuella);
        return registro with { Huella = huella };
    }

    /// <summary>
    /// Resetea el encadenamiento de huellas (útil entre tests)
    /// </summary>
    public void ResetearEncadenamiento()
    {
        _ultimaHuella = null;
    }

    /// <summary>
    /// Obtiene la última huella utilizada (para validación de encadenamiento)
    /// </summary>
    public string? ObtenerUltimaHuella()
    {
        return _ultimaHuella;
    }

    /// <summary>
    /// Establece manualmente la última huella (para tests específicos)
    /// </summary>
    public void EstablecerUltimaHuella(string? huella)
    {
        _ultimaHuella = huella;
    }

    /// <summary>
    /// Verifica si una respuesta indica éxito
    /// </summary>
    public static bool EsRespuestaExitosa(RespuestaSuministro respuesta)
    {
        return respuesta.EstadoEnvio == "Correcto";
    }

    /// <summary>
    /// Extrae el primer error de una respuesta fallida
    /// </summary>
    public static (string? codigo, string? descripcion) ObtenerPrimerError(RespuestaSuministro respuesta)
    {
        if (respuesta.RespuestasLinea == null || respuesta.RespuestasLinea.Count == 0)
        {
            return (null, null);
        }

        var primeraLinea = respuesta.RespuestasLinea[0];
        return (primeraLinea.CodigoErrorRegistro, primeraLinea.DescripcionErrorRegistro);
    }

    public void Dispose()
    {
        // El certificado es gestionado externamente, no lo disponemos aquí
    }
}
