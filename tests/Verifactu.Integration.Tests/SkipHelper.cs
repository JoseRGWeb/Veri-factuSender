using System;
using Xunit;

namespace Verifactu.Integration.Tests;

/// <summary>
/// Atributo de test personalizado que omite tests si no hay certificado configurado.
/// </summary>
public sealed class IntegrationFactAttribute : FactAttribute
{
    private const string SkipMessage = "Test de integración omitido: Requiere configurar certificado digital válido en appsettings.Sandbox.json o user-secrets.";

    public IntegrationFactAttribute()
    {
        // Por defecto, asumimos que no hay certificado y el test debe saltarse
        // Los tests mismos verificarán si hay certificado y se ejecutarán si está disponible
        // Si no hay certificado, el Skip helper lanzará una excepción con mensaje claro
    }

    /// <summary>
    /// Indica si el test se debe saltar cuando no hay certificado.
    /// Por defecto es false porque preferimos que el test falle con un mensaje claro.
    /// </summary>
    public bool SkipWhenNoCertificate { get; set; } = false;
}
