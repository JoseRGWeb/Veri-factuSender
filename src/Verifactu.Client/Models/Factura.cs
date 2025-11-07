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
/// Factura simplificada para uso interno
/// </summary>
public record Factura(
    string Serie,
    string Numero,
    DateTime FechaEmision,
    Emisor Emisor,
    Receptor Receptor,
    List<Linea> Lineas,
    TotalesFactura Totales,
    string Moneda = "EUR",
    string? Observaciones = null,
    string TipoFactura = "F1",               // F1: Factura completa
    string? DescripcionOperacion = null,
    List<DetalleDesglose>? Desglose = null   // Desglose de IVA
);
