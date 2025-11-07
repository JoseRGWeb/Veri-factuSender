using System.Security.Cryptography.X509Certificates;
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
