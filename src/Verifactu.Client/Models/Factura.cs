using System.Collections.Generic;

namespace Verifactu.Client.Models;

/// <summary>
/// Representa el emisor de la factura según XSD oficial de AEAT
/// </summary>
public record Emisor(
    string Nif,
    string Nombre,
    string? Direccion = null,
    string? Provincia = null,
    string? Municipio = null,
    string? Pais = "ES"
);

/// <summary>
/// Representa el destinatario/receptor de la factura según XSD oficial de AEAT
/// </summary>
public record Receptor(
    string? Nif,
    string Nombre,
    string? Direccion = null,
    string? Provincia = null,
    string? Municipio = null,
    string? Pais = "ES"
);

/// <summary>
/// Detalle de desglose de IVA según XSD oficial (DetalleDesglose)
/// </summary>
public record DetalleDesglose(
    string ClaveRegimen,              // Ej: "01" - Régimen general
    string CalificacionOperacion,      // Ej: "S1" - Sujeta y no exenta
    decimal TipoImpositivo,            // Ej: 21
    decimal BaseImponible,             // Base imponible o importe no sujeto
    decimal CuotaRepercutida           // Cuota de IVA
);

/// <summary>
/// Línea de factura (simplificada, no forma parte del XSD oficial pero útil internamente)
/// </summary>
public record Linea(
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal TipoImpositivo
)
{
    public decimal Base => decimal.Round(Cantidad * PrecioUnitario, 2);
    public decimal Cuota => decimal.Round(Base * (TipoImpositivo / 100m), 2);
    public decimal Total => Base + Cuota;
}

/// <summary>
/// Totales de factura
/// </summary>
public record TotalesFactura(
    decimal BaseImponible,
    decimal CuotaImpuestos,
    decimal ImporteTotal
);

/// <summary>
/// Información del sistema informático de facturación según XSD oficial (SistemaInformatico)
/// </summary>
public record SistemaInformatico(
    string NombreRazon,
    string Nif,
    string NombreSistemaInformatico,
    string IdSistemaInformatico,
    string Version,
    string NumeroInstalacion,
    string TipoUsoPosibleSoloVerifactu = "N",
    string TipoUsoPosibleMultiOT = "S",
    string IndicadorMultiplesOT = "S"
);

/// <summary>
/// Factura completa con todos los campos obligatorios y opcionales según especificación VERI*FACTU
/// </summary>
public record Factura(
    // Campos obligatorios básicos
    string Serie,
    string Numero,
    DateTime FechaEmision,
    TipoFactura TipoFactura,                  // Obligatorio: F1, F2, F3, F4, R1-R5
    string DescripcionOperacion,              // Obligatorio: Descripción de la operación
    
    // Emisor y destinatario
    Emisor Emisor,
    Receptor? Receptor = null,                // Opcional en facturas simplificadas (F2)
    
    // Datos de la factura
    List<Linea>? Lineas = null,               // Uso interno (no se envía a AEAT)
    TotalesFactura? Totales = null,           // Calculado
    List<DetalleDesglose>? Desglose = null,   // Desglose de IVA obligatorio
    
    // Campos opcionales comunes
    string Moneda = "EUR",
    string? Observaciones = null,
    DateTime? FechaOperacion = null,          // Si difiere de FechaEmision
    
    // Campos para facturas rectificativas (R1-R5)
    TipoRectificativa? TipoRectificativa = null,              // Obligatorio si TipoFactura es R1-R5
    List<FacturaRectificada>? FacturasRectificadas = null,    // Obligatorio si TipoFactura es R1-R5
    decimal? ImporteRectificacionSustitutiva = null,          // Para rectificativa por sustitución
    
    // Clave de régimen especial
    ClaveRegimenEspecialOTrascendencia? ClaveRegimenEspecialOTrascendencia = null,
    
    // Destinatarios múltiples (para facturas con varios destinatarios)
    List<DestinatarioCompleto>? Destinatarios = null,
    
    // Facturación por terceros
    FacturacionTerceros? FacturacionTerceros = null,
    FacturacionTerceros? FacturacionDestinatario = null,
    
    // Desglose de impuestos (alternativo/complementario a Desglose)
    List<DetalleIVA>? DesgloseIVA = null,
    List<DetalleIGIC>? DesgloseIGIC = null,
    List<DetalleIRPF>? DesgloseIRPF = null,
    
    // Macrodato (para facturas que superan ciertos importes)
    bool Macrodato = false,
    
    // Referencia externa
    string? RefExterna = null
);
