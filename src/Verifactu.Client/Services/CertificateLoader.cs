using System.Security.Cryptography.X509Certificates;

namespace Verifactu.Client.Services;

/// <summary>
/// Servicio para cargar certificados digitales X.509 desde diferentes fuentes.
/// Utilizado para autenticación mutua TLS (mTLS) con los servicios VERI*FACTU de la AEAT.
/// </summary>
/// <remarks>
/// REQUISITOS DEL CERTIFICADO PARA VERI*FACTU:
/// 
/// Producción:
/// - Certificado de representante de persona jurídica válido
/// - Emitido por autoridad certificadora reconocida (FNMT-RCM, ACCV, Camerfirma, etc.)
/// - NIF del certificado debe coincidir con NIF del emisor de facturas
/// 
/// Sandbox/Pruebas:
/// - Cualquier certificado digital válido y no caducado
/// - Certificados de prueba son aceptados
/// 
/// FORMATO Y REQUISITOS TÉCNICOS:
/// - Formato: PFX/PKCS#12 (debe incluir clave privada)
/// - Algoritmo de firma: RSA con SHA-256 o superior / ECDSA
/// - Longitud de clave: RSA mínimo 2048 bits / ECDSA mínimo 256 bits
/// - Estado: Válido (no caducado, no revocado)
/// - Propósito: Digital Signature + Client Authentication (1.3.6.1.5.5.7.3.2)
/// 
/// SEGURIDAD:
/// - Proteger archivos PFX con permisos restrictivos (chmod 600 en Linux)
/// - No versionar certificados PFX en repositorios Git
/// - Usar contraseñas fuertes para proteger el PFX
/// - En producción, considerar almacén de certificados del sistema o Azure Key Vault
/// - Rotar certificados antes de su caducidad
/// 
/// Para más información, consultar: docs/protocolos-comunicacion.md
/// </remarks>
public class CertificateLoader : ICertificateLoader
{
    /// <summary>
    /// Carga un certificado digital desde un archivo PFX/PKCS#12.
    /// </summary>
    /// <param name="rutaPfx">Ruta completa al archivo PFX (ej: "C:\certs\certificado.pfx")</param>
    /// <param name="password">Contraseña del archivo PFX</param>
    /// <returns>Certificado X.509 con clave privada incluida</returns>
    /// <remarks>
    /// IMPORTANTE: Este método utiliza el constructor obsoleto de X509Certificate2.
    /// En .NET 9 se recomienda usar X509CertificateLoader.LoadPkcs12() en su lugar.
    /// 
    /// X509KeyStorageFlags utilizados:
    /// - MachineKeySet: Almacena la clave en el almacén de la máquina (no del usuario)
    /// - Exportable: Permite exportar la clave privada posteriormente
    /// - PersistKeySet: Persiste la clave en el almacén del sistema
    /// 
    /// VALIDACIONES RECOMENDADAS ANTES DE USAR EL CERTIFICADO:
    /// 1. Verificar que HasPrivateKey == true
    /// 2. Verificar fechas de validez (NotBefore y NotAfter)
    /// 3. Verificar cadena de confianza con X509Chain
    /// 4. Verificar que tiene propósito Client Authentication
    /// 5. Verificar que no está revocado (CRL/OCSP)
    /// 
    /// ALTERNATIVA RECOMENDADA (.NET 9+):
    /// <code>
    /// byte[] pfxBytes = File.ReadAllBytes(rutaPfx);
    /// return X509CertificateLoader.LoadPkcs12(
    ///     pfxBytes, 
    ///     password,
    ///     X509KeyStorageFlags.MachineKeySet | 
    ///     X509KeyStorageFlags.Exportable |
    ///     X509KeyStorageFlags.PersistKeySet
    /// );
    /// </code>
    /// </remarks>
    /// <exception cref="System.Security.Cryptography.CryptographicException">
    /// Si el archivo PFX no existe, la contraseña es incorrecta, o el formato es inválido
    /// </exception>
    public X509Certificate2 CargarDesdePfx(string rutaPfx, string password)
    {
        // NOTA: Constructor obsoleto en .NET 9 (SYSLIB0057)
        // Se mantiene temporalmente para compatibilidad
        // TODO: Migrar a X509CertificateLoader.LoadPkcs12() según recomendación de .NET 9
        return new X509Certificate2(
            rutaPfx, password,
            X509KeyStorageFlags.MachineKeySet |
            X509KeyStorageFlags.Exportable |
            X509KeyStorageFlags.PersistKeySet);
    }
}
