namespace Verifactu.Client.Models;

/// <summary>
/// Modelo simplificado de "Registro de Facturación" (placeholder).
/// Adapta las propiedades a los XSD de AEAT.
/// </summary>
public record RegistroFacturacion(
    string Uuid,
    DateTime FechaHoraExpedicionUtc,
    string Serie,
    string Numero,
    string HashPrevio,  // hash encadenado del registro anterior
    string Huella,      // hash del registro actual
    Factura Factura
);
