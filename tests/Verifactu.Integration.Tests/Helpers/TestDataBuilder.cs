using System;
using System.Collections.Generic;
using Verifactu.Client.Models;

namespace Verifactu.Integration.Tests.Helpers;

/// <summary>
/// Builder pattern para crear datos de prueba consistentes y reutilizables.
/// Facilita la creación de facturas y registros de facturación para tests de integración.
/// </summary>
public class TestDataBuilder
{
    private readonly string _emisorNif;
    private readonly string _emisorNombre;
    private readonly string _receptorNif;
    private readonly string _receptorNombre;
    private readonly string _sistemaInformaticoNombre;
    private readonly string _sistemaInformaticoId;
    private readonly string _sistemaInformaticoVersion;
    private readonly string _numeroInstalacion;

    public TestDataBuilder(
        string emisorNif = "B12345678",
        string emisorNombre = "EMPRESA PRUEBAS SANDBOX SL",
        string receptorNif = "12345678A",
        string receptorNombre = "CLIENTE PRUEBAS SANDBOX",
        string sistemaInformaticoNombre = "VerifactuSender",
        string sistemaInformaticoId = "TEST001",
        string sistemaInformaticoVersion = "1.0",
        string numeroInstalacion = "1")
    {
        _emisorNif = emisorNif;
        _emisorNombre = emisorNombre;
        _receptorNif = receptorNif;
        _receptorNombre = receptorNombre;
        _sistemaInformaticoNombre = sistemaInformaticoNombre;
        _sistemaInformaticoId = sistemaInformaticoId;
        _sistemaInformaticoVersion = sistemaInformaticoVersion;
        _numeroInstalacion = numeroInstalacion;
    }

    /// <summary>
    /// Crea un registro de factura básico (F1) para tests
    /// </summary>
    public RegistroFacturacion CrearFacturaBasica(
        string numeroFactura,
        TipoFactura tipoFactura = TipoFactura.F1,
        decimal baseImponible = 200m,
        decimal tipoImpositivo = 21m)
    {
        var fechaActual = DateTime.UtcNow;
        var cuota = baseImponible * (tipoImpositivo / 100m);
        var total = baseImponible + cuota;

        var factura = new Factura(
            Serie: "TEST",
            Numero: numeroFactura,
            FechaEmision: fechaActual,
            TipoFactura: tipoFactura,
            DescripcionOperacion: $"Test de integración - {tipoFactura}",
            Emisor: new Emisor(_emisorNif, _emisorNombre),
            Receptor: new Receptor(_receptorNif, _receptorNombre),
            Lineas: new List<Linea>
            {
                new Linea("Producto de prueba", 1, baseImponible, tipoImpositivo)
            },
            Totales: new TotalesFactura(baseImponible, cuota, total)
        );

        var desglose = new List<DetalleDesglose>
        {
            new DetalleDesglose(
                ClaveRegimen: "01",
                CalificacionOperacion: "S1",
                TipoImpositivo: tipoImpositivo,
                BaseImponible: baseImponible,
                CuotaRepercutida: cuota
            )
        };

        var sistemaInfo = new SistemaInformatico(
            NombreRazon: _emisorNombre,
            Nif: _emisorNif,
            NombreSistemaInformatico: _sistemaInformaticoNombre,
            IdSistemaInformatico: _sistemaInformaticoId,
            Version: _sistemaInformaticoVersion,
            NumeroInstalacion: _numeroInstalacion
        );

        return new RegistroFacturacion(
            IDVersion: "1.0",
            IDEmisorFactura: _emisorNif,
            NumSerieFactura: $"TEST/{numeroFactura}",
            FechaExpedicionFactura: fechaActual,
            NombreRazonEmisor: _emisorNombre,
            TipoFactura: tipoFactura,
            DescripcionOperacion: $"Test de integración - {tipoFactura}",
            Desglose: desglose,
            CuotaTotal: cuota,
            ImporteTotal: total,
            FechaHoraHusoGenRegistro: fechaActual,
            TipoHuella: "01",
            Huella: string.Empty,
            SistemaInformatico: sistemaInfo,
            Factura: factura
        );
    }

    /// <summary>
    /// Crea una factura simplificada (F2) sin datos completos del receptor
    /// </summary>
    public RegistroFacturacion CrearFacturaSimplificada(
        string numeroFactura,
        decimal baseImponible = 100m)
    {
        return CrearFacturaBasica(numeroFactura, TipoFactura.F2, baseImponible);
    }

    /// <summary>
    /// Crea una factura rectificativa con referencia a factura original
    /// </summary>
    public RegistroFacturacion CrearFacturaRectificativa(
        string numeroFactura,
        TipoRectificativa tipoRectificativa = TipoRectificativa.S,
        string? numeroFacturaOriginal = null,
        decimal baseImponible = 200m)
    {
        return CrearFacturaBasica(numeroFactura, TipoFactura.R1, baseImponible);
    }

    /// <summary>
    /// Crea una factura con datos inválidos para tests de error
    /// </summary>
    public RegistroFacturacion CrearFacturaInvalida(
        string numeroFactura,
        string tipoError = "ImporteNegativo")
    {
        var registro = CrearFacturaBasica(numeroFactura);

        return tipoError switch
        {
            "ImporteNegativo" => registro with { ImporteTotal = -100m },
            "CuotaNegativa" => registro with { CuotaTotal = -21m },
            "SinLineas" => registro with { Factura = registro.Factura with { Lineas = new List<Linea>() } },
            _ => registro
        };
    }

    /// <summary>
    /// Genera un número de factura único basado en timestamp
    /// </summary>
    public static string GenerarNumeroFacturaUnico(string prefijo = "TEST")
    {
        return $"{prefijo}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }
}
