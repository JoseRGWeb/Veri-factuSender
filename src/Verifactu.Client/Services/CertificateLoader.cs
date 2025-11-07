using System;
using System.IO;
using System.Linq;
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
    /// MIGRACIÓN A .NET 9:
    /// Este método ha sido actualizado para usar X509CertificateLoader.LoadPkcs12() 
    /// en lugar del constructor obsoleto de X509Certificate2.
    /// 
    /// X509KeyStorageFlags utilizados:
    /// - MachineKeySet: Almacena la clave en el almacén de la máquina (no del usuario)
    /// - Exportable: Permite exportar la clave privada posteriormente
    /// - PersistKeySet: Persiste la clave en el almacén del sistema
    /// 
    /// VALIDACIONES REALIZADAS:
    /// 1. Verificación que HasPrivateKey == true
    /// 2. Verificación de fechas de validez (NotBefore y NotAfter)
    /// 3. Verificación básica de cadena de confianza con X509Chain
    /// 
    /// VALIDACIONES ADICIONALES RECOMENDADAS EN PRODUCCIÓN:
    /// 1. Verificar que tiene propósito Client Authentication (EKU 1.3.6.1.5.5.7.3.2)
    /// 2. Verificar que no está revocado mediante CRL/OCSP
    /// 3. Validar que el NIF del certificado coincide con el NIF del emisor
    /// </remarks>
    /// <exception cref="System.IO.FileNotFoundException">
    /// Si el archivo PFX no existe en la ruta especificada
    /// </exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">
    /// Si la contraseña es incorrecta, el formato es inválido, o el certificado no cumple los requisitos
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Si el certificado no contiene clave privada, está caducado o no es válido
    /// </exception>
    public X509Certificate2 CargarDesdePfx(string rutaPfx, string password)
    {
        // Verificar que el archivo existe
        if (!File.Exists(rutaPfx))
        {
            throw new FileNotFoundException($"No se encontró el archivo de certificado en la ruta especificada: {rutaPfx}");
        }

        // Cargar certificado usando la nueva API de .NET 9
        byte[] pfxBytes = File.ReadAllBytes(rutaPfx);
        var certificado = X509CertificateLoader.LoadPkcs12(
            pfxBytes,
            password,
            X509KeyStorageFlags.MachineKeySet |
            X509KeyStorageFlags.Exportable |
            X509KeyStorageFlags.PersistKeySet);

        // Validación 1: Verificar que tiene clave privada
        if (!certificado.HasPrivateKey)
        {
            certificado.Dispose();
            throw new InvalidOperationException(
                "El certificado no contiene clave privada. " +
                "VERI*FACTU requiere certificados con clave privada para firma digital y autenticación mTLS.");
        }

        // Validación 2: Verificar fechas de validez
        var ahora = DateTime.UtcNow;
        if (ahora < certificado.NotBefore)
        {
            certificado.Dispose();
            throw new InvalidOperationException(
                $"El certificado aún no es válido. Será válido desde: {certificado.NotBefore:yyyy-MM-dd HH:mm:ss} UTC");
        }

        if (ahora > certificado.NotAfter)
        {
            certificado.Dispose();
            throw new InvalidOperationException(
                $"El certificado ha caducado. Caducó el: {certificado.NotAfter:yyyy-MM-dd HH:mm:ss} UTC");
        }

        // Validación 3: Verificar cadena de confianza (advertencia si no es confiable)
        using (var chain = new X509Chain())
        {
            // Configurar opciones de validación
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // No verificar revocación aquí
            // No ignorar problemas de tiempo: dejar que se validen correctamente
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

            bool chainIsValid = chain.Build(certificado);
            
            if (!chainIsValid)
            {
                // En sandbox/desarrollo es aceptable tener certificados autofirmados
                // En producción se debe revisar esto más estrictamente
                var chainErrors = string.Join(", ", 
                    chain.ChainStatus.Select(status => status.StatusInformation));
                
                // Log de advertencia pero no lanzar excepción
                System.Diagnostics.Debug.WriteLine(
                    $"ADVERTENCIA: La cadena de confianza del certificado tiene problemas: {chainErrors}. " +
                    "Esto es aceptable en entorno sandbox/pruebas, pero debe corregirse en producción.");
            }
        }

        return certificado;
    }
}
