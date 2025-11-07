namespace Verifactu.Client.Models;

/// <summary>
/// Modelo de "Registro de Facturación" conforme a XSD oficial de AEAT.
/// Representa un RegistroAlta en el esquema SuministroInformacion.xsd
/// Incluye todos los campos obligatorios y opcionales según especificación VERI*FACTU
/// </summary>
public record RegistroFacturacion(
    // Campos obligatorios de identificación
    string IDVersion,                      // Ej: "1.0"
    string IDEmisorFactura,                // NIF del emisor
    string NumSerieFactura,                // Número de serie de la factura
    DateTime FechaExpedicionFactura,       // Fecha de expedición
    string NombreRazonEmisor,              // Nombre/razón social del emisor
    TipoFactura TipoFactura,               // Tipo de factura (F1-F4, R1-R5)
    string DescripcionOperacion,           // Descripción de la operación
    
    // Desglose de impuestos
    List<DetalleDesglose> Desglose,        // Desglose de IVA
    decimal CuotaTotal,                    // Cuota total de impuestos
    decimal ImporteTotal,                  // Importe total de la factura
    
    // Huella y sistema
    DateTime FechaHoraHusoGenRegistro,     // Fecha/hora generación con huso horario
    string TipoHuella,                     // Ej: "01" (SHA-256)
    string Huella,                         // Hash del registro actual
    SistemaInformatico SistemaInformatico, // Información del sistema
    Factura Factura,                       // Factura original (para uso interno)
    
    // Campos opcionales de encadenamiento
    string? IDEmisorFacturaAnterior = null,       // Para encadenamiento
    string? NumSerieFacturaAnterior = null,       // Para encadenamiento
    DateTime? FechaExpedicionFacturaAnterior = null, // Para encadenamiento
    string? HuellaAnterior = null,                // Hash del registro anterior
    
    // Destinatario (opcional en facturas simplificadas)
    Receptor? Destinatario = null,
    List<DestinatarioCompleto>? Destinatarios = null,
    
    // Campos para facturas rectificativas
    TipoRectificativa? TipoRectificativa = null,
    List<FacturaRectificada>? FacturasRectificadas = null,
    decimal? ImporteRectificacionSustitutiva = null,
    
    // Régimen especial y calificación
    ClaveRegimenEspecialOTrascendencia? ClaveRegimenEspecialOTrascendencia = null,
    
    // Fecha de operación (si difiere de fecha de expedición)
    DateTime? FechaOperacion = null,
    
    // Facturación por terceros
    FacturacionTerceros? FacturacionTerceros = null,
    FacturacionTerceros? FacturacionDestinatario = null,
    
    // Desglose adicional de impuestos
    List<DetalleIVA>? DesgloseIVA = null,
    List<DetalleIGIC>? DesgloseIGIC = null,
    List<DetalleIRPF>? DesgloseIRPF = null,
    
    // Indicadores de estado
    bool Subsanacion = false,              // Indica si es subsanación
    bool RechazoPrevio = false,            // Indica si hubo rechazo previo
    bool Macrodato = false,                // Indica si es macrodato
    
    // Referencia externa (opcional)
    string? RefExterna = null
);
