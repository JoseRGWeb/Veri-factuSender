namespace Verifactu.Client.Models;

/// <summary>
/// Identificación de factura rectificada según XSD oficial de AEAT
/// </summary>
public record FacturaRectificada(
    string NumSerieFactura,           // Número de serie de la factura rectificada
    DateTime FechaExpedicionFactura   // Fecha de expedición de la factura rectificada
);

/// <summary>
/// Identificación del destinatario de la factura según XSD oficial de AEAT
/// Amplía el modelo Receptor para incluir identificación extranjera
/// </summary>
public record IDOtro(
    string CodigoPais,    // Código de país ISO 3166-1 alpha-2
    string IDType,        // Tipo de identificación (02, 03, 04, 05, 06, 07)
    string ID             // Número de identificación en el país de residencia
);

/// <summary>
/// Desglose de IVA por tipo impositivo y régimen según XSD oficial de AEAT
/// </summary>
public record DetalleIVA(
    decimal BaseImponible,
    decimal TipoImpositivo,
    decimal CuotaRepercutida,
    decimal? TipoRecargoEquivalencia = null,
    decimal? CuotaRecargoEquivalencia = null
);

/// <summary>
/// Desglose de IGIC (Canarias) según XSD oficial de AEAT
/// </summary>
public record DetalleIGIC(
    decimal BaseImponible,
    decimal TipoImpositivo,
    decimal CuotaRepercutida,
    decimal? TipoRecargoEquivalencia = null,
    decimal? CuotaRecargoEquivalencia = null
);

/// <summary>
/// Desglose de IRPF según XSD oficial de AEAT
/// </summary>
public record DetalleIRPF(
    decimal BaseImponible,
    decimal TipoRetencion,
    decimal CuotaRetenida
);

/// <summary>
/// Información de facturación por cuenta de terceros o destinatario según XSD oficial de AEAT
/// </summary>
public record FacturacionTerceros(
    string NIF,
    string NombreRazon
);

/// <summary>
/// Información completa del destinatario con soporte para identificación extranjera
/// </summary>
public record DestinatarioCompleto(
    string NombreRazon,
    string? NIF = null,
    IDOtro? IDOtro = null,
    string? CodigoPostal = null,
    string? Direccion = null
);
