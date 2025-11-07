using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

using Verifactu.Client.Models;

namespace Verifactu.Client.Services;

public interface IHashService
{
    /// <summary>
    /// Calcula la huella/encadenado del registro.
    /// Debes reemplazar por el algoritmo oficial de AEAT.
    /// </summary>
    string CalcularHuella(RegistroFacturacion registro, string? huellaAnterior);
}

public interface IVerifactuSerializer
{
    /// <summary>
    /// Crea el XML del registro conforme a los XSD de AEAT.
    /// </summary>
    XmlDocument CrearXmlRegistro(RegistroFacturacion registro);
}

public interface IXmlSignerService
{
    /// <summary>
    /// Firma XML (enveloped) con XMLDSig.
    /// Ajusta canonicalización y políticas a lo exigido por AEAT.
    /// </summary>
    XmlDocument Firmar(XmlDocument xml, X509Certificate2 cert);
}

public interface ICertificateLoader
{
    X509Certificate2 CargarDesdePfx(string rutaPfx, string password);
    // (Opcional) método para cargar desde almacén del sistema
}

public interface IVerifactuSoapClient
{
    /// <summary>
    /// Envía el XML firmado al servicio SOAP de AEAT.
    /// Retorna la respuesta SOAP cruda (para parseo posterior).
    /// </summary>
    Task<string> EnviarRegistroAsync(XmlDocument xmlFirmado, X509Certificate2 cert, CancellationToken ct = default);
    
    /// <summary>
    /// Envía un registro de facturación (alta o anulación) y parsea la respuesta.
    /// </summary>
    Task<RespuestaSuministro> EnviarRegFacturacionAltaAsync(XmlDocument xmlFirmado, X509Certificate2 cert, CancellationToken ct = default);
    
    /// <summary>
    /// Realiza una consulta de registros de facturación y parsea la respuesta.
    /// </summary>
    Task<RespuestaConsultaLR> ConsultarLRFacturasAsync(XmlDocument xmlConsulta, X509Certificate2 cert, CancellationToken ct = default);
}

public interface IXmlValidationService
{
    /// <summary>
    /// Valida un documento XML contra los esquemas XSD oficiales de AEAT.
    /// Retorna true si es válido, false si hay errores.
    /// Los errores se reportan mediante el callback validationEventHandler.
    /// </summary>
    bool ValidarContraXsd(XmlDocument xmlDoc, ValidationEventHandler? validationEventHandler = null);
}

public interface IErrorHandler
{
    /// <summary>
    /// Analiza una respuesta AEAT y extrae información de errores clasificados.
    /// </summary>
    /// <param name="respuesta">Respuesta del servicio AEAT</param>
    /// <returns>Resultado del análisis con errores clasificados</returns>
    ResultadoAnalisisErrores AnalizarRespuesta(RespuestaSuministro respuesta);

    /// <summary>
    /// Determina si un error específico es recuperable mediante reintento.
    /// </summary>
    /// <param name="codigoError">Código del error AEAT</param>
    /// <returns>True si el error es recuperable</returns>
    bool EsErrorRecuperable(string? codigoError);

    /// <summary>
    /// Obtiene la acción recomendada para un código de error.
    /// </summary>
    /// <param name="codigoError">Código del error AEAT</param>
    /// <returns>Descripción de la acción recomendada</returns>
    string ObtenerAccionRecomendada(string? codigoError);

    /// <summary>
    /// Determina si una respuesta requiere reintentos automáticos.
    /// </summary>
    /// <param name="respuesta">Respuesta AEAT</param>
    /// <returns>True si se debe reintentar el envío</returns>
    bool DebeReintentarse(RespuestaSuministro respuesta);

    /// <summary>
    /// Calcula el tiempo de espera antes del próximo intento basándose en la respuesta AEAT.
    /// </summary>
    /// <param name="respuesta">Respuesta AEAT</param>
    /// <param name="numeroIntento">Número del intento actual (0-based)</param>
    /// <returns>Tiempo de espera recomendado</returns>
    TimeSpan CalcularTiempoEspera(RespuestaSuministro? respuesta, int numeroIntento);
}

public interface IReintentosService
{
    /// <summary>
    /// Envía un registro de facturación con reintentos automáticos en caso de errores recuperables.
    /// Implementa backoff exponencial y respeta los tiempos de espera indicados por AEAT.
    /// </summary>
    /// <param name="xmlFirmado">Documento XML firmado del registro</param>
    /// <param name="certificado">Certificado para autenticación mTLS</param>
    /// <param name="opciones">Opciones de configuración de reintentos (opcional)</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Resultado del envío con información de reintentos</returns>
    Task<ResultadoEnvioConReintentos> EnviarConReintentosAsync(
        XmlDocument xmlFirmado,
        X509Certificate2 certificado,
        OpcionesReintento? opciones = null,
        CancellationToken ct = default);
}

public interface IQrService
{
    /// <summary>
    /// Genera la URL de cotejo para verificar el registro en la sede electrónica de AEAT.
    /// Según especificación oficial S16 de AEAT.
    /// </summary>
    /// <param name="factura">Factura para la que generar la URL de cotejo</param>
    /// <param name="huella">Huella SHA-256 del registro de facturación (64 caracteres hexadecimales)</param>
    /// <returns>URL completa de cotejo con todos los parámetros necesarios</returns>
    string GenerarUrlCotejo(Factura factura, string huella);

    /// <summary>
    /// Genera código QR en formato PNG como array de bytes.
    /// </summary>
    /// <param name="urlCotejo">URL de cotejo a codificar en el QR</param>
    /// <param name="pixelsPorModulo">Tamaño del QR (píxeles por módulo, por defecto 20)</param>
    /// <returns>Imagen QR en formato PNG como array de bytes</returns>
    byte[] GenerarQrPng(string urlCotejo, int pixelsPorModulo = 20);

    /// <summary>
    /// Genera código QR en formato SVG.
    /// </summary>
    /// <param name="urlCotejo">URL de cotejo a codificar en el QR</param>
    /// <param name="pixelsPorModulo">Tamaño del QR (píxeles por módulo, por defecto 10)</param>
    /// <returns>Imagen QR en formato SVG como cadena XML</returns>
    string GenerarQrSvg(string urlCotejo, int pixelsPorModulo = 10);

    /// <summary>
    /// Genera código QR en formato Base64 (PNG codificado).
    /// Útil para incrustar en HTML/CSS.
    /// </summary>
    /// <param name="urlCotejo">URL de cotejo a codificar en el QR</param>
    /// <param name="pixelsPorModulo">Tamaño del QR (píxeles por módulo, por defecto 20)</param>
    /// <returns>Imagen QR en formato Base64 (data URI completo para HTML)</returns>
    string GenerarQrBase64(string urlCotejo, int pixelsPorModulo = 20);
}
