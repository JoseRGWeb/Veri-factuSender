using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Verifactu.Client.Models;

/// <summary>
/// Información detallada sobre un certificado X.509.
/// Proporciona datos útiles para diagnóstico y validación.
/// </summary>
public record CertificateInfo
{
    /// <summary>
    /// Nombre completo del sujeto del certificado (Distinguished Name).
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Nombre del emisor del certificado.
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// Huella digital SHA-1 del certificado (thumbprint).
    /// </summary>
    public string Thumbprint { get; init; } = string.Empty;

    /// <summary>
    /// Número de serie del certificado.
    /// </summary>
    public string SerialNumber { get; init; } = string.Empty;

    /// <summary>
    /// Fecha y hora desde la cual el certificado es válido (UTC).
    /// </summary>
    public DateTime NotBefore { get; init; }

    /// <summary>
    /// Fecha y hora hasta la cual el certificado es válido (UTC).
    /// </summary>
    public DateTime NotAfter { get; init; }

    /// <summary>
    /// Tiempo restante hasta la expiración del certificado.
    /// Negativo si ya expiró.
    /// </summary>
    public TimeSpan TiempoHastaExpiracion { get; init; }

    /// <summary>
    /// Indica si el certificado tiene clave privada.
    /// </summary>
    public bool TieneClavePrivada { get; init; }

    /// <summary>
    /// Indica si el certificado está actualmente válido (dentro del período de validez).
    /// </summary>
    public bool EsValido { get; init; }

    /// <summary>
    /// Versión del certificado (normalmente 3).
    /// </summary>
    public int Version { get; init; }

    /// <summary>
    /// Algoritmo de firma utilizado.
    /// </summary>
    public string AlgoritmoFirma { get; init; } = string.Empty;

    /// <summary>
    /// Tipo de clave pública (RSA, ECDSA, etc.).
    /// </summary>
    public string TipoClave { get; init; } = string.Empty;

    /// <summary>
    /// Tamaño de la clave en bits (ej: 2048, 4096 para RSA).
    /// </summary>
    public int? TamanoClaveBits { get; init; }

    /// <summary>
    /// Lista de propósitos del certificado (Enhanced Key Usage).
    /// </summary>
    public string[] Propositos { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Indica si el certificado es autofirmado.
    /// </summary>
    public bool EsAutofirmado { get; init; }

    /// <summary>
    /// Crea una instancia de CertificateInfo desde un X509Certificate2.
    /// </summary>
    public static CertificateInfo FromCertificate(X509Certificate2 certificate)
    {
        if (certificate == null)
            throw new ArgumentNullException(nameof(certificate));

        var ahora = DateTime.UtcNow;
        var tiempoHastaExpiracion = certificate.NotAfter - ahora;

        // Obtener tipo y tamaño de clave
        string tipoClave = "Desconocido";
        int? tamanoClave = null;

        var rsaKey = certificate.GetRSAPublicKey();
        if (rsaKey != null)
        {
            tipoClave = "RSA";
            tamanoClave = rsaKey.KeySize;
        }
        else
        {
            var ecdsaKey = certificate.GetECDsaPublicKey();
            if (ecdsaKey != null)
            {
                tipoClave = "ECDSA";
                tamanoClave = ecdsaKey.KeySize;
            }
        }

        // Obtener propósitos (Enhanced Key Usage)
        var propositos = new List<string>();
        foreach (var extension in certificate.Extensions)
        {
            if (extension is X509EnhancedKeyUsageExtension ekuExtension)
            {
                foreach (var oid in ekuExtension.EnhancedKeyUsages)
                {
                    var nombre = oid.FriendlyName ?? oid.Value ?? "Desconocido";
                    propositos.Add($"{nombre} ({oid.Value})");
                }
            }
        }

        // Verificar si es autofirmado (Subject == Issuer)
        var esAutofirmado = certificate.Subject == certificate.Issuer;

        return new CertificateInfo
        {
            Subject = certificate.Subject,
            Issuer = certificate.Issuer,
            Thumbprint = certificate.Thumbprint,
            SerialNumber = certificate.SerialNumber,
            NotBefore = certificate.NotBefore.ToUniversalTime(),
            NotAfter = certificate.NotAfter.ToUniversalTime(),
            TiempoHastaExpiracion = tiempoHastaExpiracion,
            TieneClavePrivada = certificate.HasPrivateKey,
            EsValido = ahora >= certificate.NotBefore && ahora <= certificate.NotAfter,
            Version = certificate.Version,
            AlgoritmoFirma = certificate.SignatureAlgorithm.FriendlyName ?? "Desconocido",
            TipoClave = tipoClave,
            TamanoClaveBits = tamanoClave,
            Propositos = propositos.ToArray(),
            EsAutofirmado = esAutofirmado
        };
    }
}
