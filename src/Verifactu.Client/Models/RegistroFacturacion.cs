namespace Verifactu.Client.Models;

/// <summary>
/// Modelo de "Registro de Facturación" conforme a XSD oficial de AEAT.
/// Representa un RegistroAlta en el esquema SuministroInformacion.xsd
/// </summary>
public record RegistroFacturacion(
    string IDVersion,                      // Ej: "1.0"
    string IDEmisorFactura,                // NIF del emisor
    string NumSerieFactura,                // Número de serie de la factura
    DateTime FechaExpedicionFactura,       // Fecha de expedición
    string NombreRazonEmisor,              // Nombre/razón social del emisor
    string TipoFactura,                    // Ej: "F1" (Factura completa)
    string DescripcionOperacion,           // Descripción de la operación
    List<DetalleDesglose> Desglose,        // Desglose de IVA
    decimal CuotaTotal,                    // Cuota total de impuestos
    decimal ImporteTotal,                  // Importe total de la factura
    DateTime FechaHoraHusoGenRegistro,     // Fecha/hora generación con huso horario
    string TipoHuella,                     // Ej: "01" (SHA-256)
    string Huella,                         // Hash del registro actual
    SistemaInformatico SistemaInformatico, // Información del sistema
    Factura Factura,                       // Factura original (para uso interno)
    
    // Campos opcionales
    string? IDEmisorFacturaAnterior = null,       // Para encadenamiento
    string? NumSerieFacturaAnterior = null,       // Para encadenamiento
    DateTime? FechaExpedicionFacturaAnterior = null, // Para encadenamiento
    string? HuellaAnterior = null,                // Hash del registro anterior
    Receptor? Destinatario = null,                // Destinatario (opcional en algunos casos)
    bool Subsanacion = false,                     // Indica si es subsanación
    bool RechazoPrevio = false                    // Indica si hubo rechazo previo
);
