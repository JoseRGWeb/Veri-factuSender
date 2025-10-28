using System.Collections.Generic;

namespace Verifactu.Client.Models;

public record Emisor(
    string Nif,
    string Nombre,
    string? Direccion = null,
    string? Provincia = null,
    string? Municipio = null,
    string? Pais = "ES"
);

public record Receptor(
    string? Nif,
    string Nombre,
    string? Direccion = null,
    string? Provincia = null,
    string? Municipio = null,
    string? Pais = "ES"
);

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

public record TotalesFactura(
    decimal BaseImponible,
    decimal CuotaImpuestos,
    decimal ImporteTotal
);

public record Factura(
    string Serie,
    string Numero,
    DateTime FechaEmision,
    Emisor Emisor,
    Receptor Receptor,
    List<Linea> Lineas,
    TotalesFactura Totales,
    string Moneda = "EUR",
    string? Observaciones = null
);
