using System;
using System.Collections.Generic;
using Verifactu.Client.Models;
using Xunit;

namespace Verifactu.Client.Tests;

/// <summary>
/// Tests para validar la creación de facturas rectificativas según especificación VERI*FACTU
/// </summary>
public class FacturasRectificativasTests
{
    [Fact]
    public void FacturaRectificativa_R1_DebeCrearseCorrectamente()
    {
        // Arrange
        var fechaEmision = new DateTime(2024, 2, 15);
        var emisor = new Emisor("B12345678", "EMPRESA TEST SL");
        var receptor = new Receptor("12345678Z", "CLIENTE TEST");
        var facturasRectificadas = new List<FacturaRectificada>
        {
            new FacturaRectificada("A/2024/001", new DateTime(2024, 1, 15))
        };
        
        // Act
        var factura = new Factura(
            Serie: "AR",
            Numero: "001",
            FechaEmision: fechaEmision,
            TipoFactura: TipoFactura.R1,
            DescripcionOperacion: "Rectificación por error fundado en derecho",
            Emisor: emisor,
            Receptor: receptor,
            TipoRectificativa: TipoRectificativa.S,
            FacturasRectificadas: facturasRectificadas,
            ImporteRectificacionSustitutiva: 242m
        );
        
        // Assert
        Assert.Equal(TipoFactura.R1, factura.TipoFactura);
        Assert.Equal(TipoRectificativa.S, factura.TipoRectificativa);
        Assert.NotNull(factura.FacturasRectificadas);
        Assert.Single(factura.FacturasRectificadas);
        Assert.Equal("A/2024/001", factura.FacturasRectificadas[0].NumSerieFactura);
        Assert.Equal(242m, factura.ImporteRectificacionSustitutiva);
    }

    [Fact]
    public void FacturaRectificativa_R2_ConTipoRectificativaI_DebeCrearseCorrectamente()
    {
        // Arrange
        var fechaEmision = new DateTime(2024, 2, 15);
        var emisor = new Emisor("B12345678", "EMPRESA TEST SL");
        var receptor = new Receptor("12345678Z", "CLIENTE TEST");
        var facturasRectificadas = new List<FacturaRectificada>
        {
            new FacturaRectificada("A/2024/001", new DateTime(2024, 1, 15))
        };
        
        // Act
        var factura = new Factura(
            Serie: "AR",
            Numero: "002",
            FechaEmision: fechaEmision,
            TipoFactura: TipoFactura.R2,
            DescripcionOperacion: "Rectificación Art. 80.3 LIVA",
            Emisor: emisor,
            Receptor: receptor,
            TipoRectificativa: TipoRectificativa.I, // Por diferencias
            FacturasRectificadas: facturasRectificadas
        );
        
        // Assert
        Assert.Equal(TipoFactura.R2, factura.TipoFactura);
        Assert.Equal(TipoRectificativa.I, factura.TipoRectificativa);
        Assert.NotNull(factura.FacturasRectificadas);
    }

    [Fact]
    public void FacturaRectificativa_VariasFacturasRectificadas_DebeCrearseCorrectamente()
    {
        // Arrange
        var fechaEmision = new DateTime(2024, 2, 15);
        var emisor = new Emisor("B12345678", "EMPRESA TEST SL");
        var receptor = new Receptor("12345678Z", "CLIENTE TEST");
        var facturasRectificadas = new List<FacturaRectificada>
        {
            new FacturaRectificada("A/2024/001", new DateTime(2024, 1, 15)),
            new FacturaRectificada("A/2024/002", new DateTime(2024, 1, 16)),
            new FacturaRectificada("A/2024/003", new DateTime(2024, 1, 17))
        };
        
        // Act
        var factura = new Factura(
            Serie: "AR",
            Numero: "003",
            FechaEmision: fechaEmision,
            TipoFactura: TipoFactura.R4,
            DescripcionOperacion: "Rectificación múltiples facturas",
            Emisor: emisor,
            Receptor: receptor,
            TipoRectificativa: TipoRectificativa.S,
            FacturasRectificadas: facturasRectificadas
        );
        
        // Assert
        Assert.Equal(3, factura.FacturasRectificadas?.Count);
    }

    [Fact]
    public void FacturaRectificativa_R5_SimplificadaRectificativa_DebeCrearseCorrectamente()
    {
        // Arrange
        var fechaEmision = new DateTime(2024, 2, 15);
        var emisor = new Emisor("B12345678", "EMPRESA TEST SL");
        var facturasRectificadas = new List<FacturaRectificada>
        {
            new FacturaRectificada("S/2024/001", new DateTime(2024, 1, 15))
        };
        
        // Act - Factura simplificada rectificativa (sin destinatario)
        var factura = new Factura(
            Serie: "SR",
            Numero: "001",
            FechaEmision: fechaEmision,
            TipoFactura: TipoFactura.R5,
            DescripcionOperacion: "Rectificación de factura simplificada",
            Emisor: emisor,
            TipoRectificativa: TipoRectificativa.S,
            FacturasRectificadas: facturasRectificadas
        );
        
        // Assert
        Assert.Equal(TipoFactura.R5, factura.TipoFactura);
        Assert.Null(factura.Receptor); // Las simplificadas pueden no tener receptor
        Assert.NotNull(factura.FacturasRectificadas);
    }

    [Fact]
    public void FacturaCompleta_F1_ConTodosLosCamposObligatorios_DebeCrearseCorrectamente()
    {
        // Arrange
        var fechaEmision = new DateTime(2024, 2, 15);
        var emisor = new Emisor("B12345678", "EMPRESA TEST SL");
        var receptor = new Receptor("12345678Z", "CLIENTE TEST");
        var desglose = new List<DetalleDesglose>
        {
            new DetalleDesglose("01", "S1", 21, 100, 21)
        };
        
        // Act
        var factura = new Factura(
            Serie: "A",
            Numero: "001",
            FechaEmision: fechaEmision,
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Venta de productos",
            Emisor: emisor,
            Receptor: receptor,
            Desglose: desglose,
            ClaveRegimenEspecialOTrascendencia: ClaveRegimenEspecialOTrascendencia.RegimenGeneral01
        );
        
        // Assert
        Assert.Equal(TipoFactura.F1, factura.TipoFactura);
        Assert.Equal("Venta de productos", factura.DescripcionOperacion);
        Assert.NotNull(factura.Desglose);
        Assert.Single(factura.Desglose);
        Assert.Equal(ClaveRegimenEspecialOTrascendencia.RegimenGeneral01, factura.ClaveRegimenEspecialOTrascendencia);
    }

    [Fact]
    public void FacturaSimplificada_F2_SinDestinatario_DebeCrearseCorrectamente()
    {
        // Arrange
        var fechaEmision = new DateTime(2024, 2, 15);
        var emisor = new Emisor("B12345678", "EMPRESA TEST SL");
        var desglose = new List<DetalleDesglose>
        {
            new DetalleDesglose("01", "S1", 21, 50, 10.5m)
        };
        
        // Act
        var factura = new Factura(
            Serie: "S",
            Numero: "001",
            FechaEmision: fechaEmision,
            TipoFactura: TipoFactura.F2,
            DescripcionOperacion: "Venta al público",
            Emisor: emisor,
            Desglose: desglose
        );
        
        // Assert
        Assert.Equal(TipoFactura.F2, factura.TipoFactura);
        Assert.Null(factura.Receptor); // Factura simplificada sin destinatario
    }

    [Fact]
    public void FacturaConFacturacionTerceros_DebeCrearseCorrectamente()
    {
        // Arrange
        var fechaEmision = new DateTime(2024, 2, 15);
        var emisor = new Emisor("B12345678", "EMPRESA TEST SL");
        var receptor = new Receptor("12345678Z", "CLIENTE TEST");
        var terceros = new FacturacionTerceros("B99999999", "EMPRESA TERCERA SL");
        
        // Act
        var factura = new Factura(
            Serie: "A",
            Numero: "001",
            FechaEmision: fechaEmision,
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Venta por cuenta de terceros",
            Emisor: emisor,
            Receptor: receptor,
            FacturacionTerceros: terceros
        );
        
        // Assert
        Assert.NotNull(factura.FacturacionTerceros);
        Assert.Equal("B99999999", factura.FacturacionTerceros.NIF);
        Assert.Equal("EMPRESA TERCERA SL", factura.FacturacionTerceros.NombreRazon);
    }

    [Fact]
    public void FacturaConDesgloseIVA_DebeCrearseCorrectamente()
    {
        // Arrange
        var fechaEmision = new DateTime(2024, 2, 15);
        var emisor = new Emisor("B12345678", "EMPRESA TEST SL");
        var receptor = new Receptor("12345678Z", "CLIENTE TEST");
        var desgloseIVA = new List<DetalleIVA>
        {
            new DetalleIVA(100, 21, 21),
            new DetalleIVA(50, 10, 5)
        };
        
        // Act
        var factura = new Factura(
            Serie: "A",
            Numero: "001",
            FechaEmision: fechaEmision,
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Venta con múltiples tipos de IVA",
            Emisor: emisor,
            Receptor: receptor,
            DesgloseIVA: desgloseIVA
        );
        
        // Assert
        Assert.NotNull(factura.DesgloseIVA);
        Assert.Equal(2, factura.DesgloseIVA.Count);
    }

    [Fact]
    public void FacturaConDesgloseIRPF_DebeCrearseCorrectamente()
    {
        // Arrange
        var fechaEmision = new DateTime(2024, 2, 15);
        var emisor = new Emisor("B12345678", "EMPRESA TEST SL");
        var receptor = new Receptor("12345678Z", "CLIENTE TEST");
        var desgloseIRPF = new List<DetalleIRPF>
        {
            new DetalleIRPF(1000, 15, 150)
        };
        
        // Act
        var factura = new Factura(
            Serie: "A",
            Numero: "001",
            FechaEmision: fechaEmision,
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Servicios profesionales con retención IRPF",
            Emisor: emisor,
            Receptor: receptor,
            DesgloseIRPF: desgloseIRPF
        );
        
        // Assert
        Assert.NotNull(factura.DesgloseIRPF);
        Assert.Single(factura.DesgloseIRPF);
        Assert.Equal(150, factura.DesgloseIRPF[0].CuotaRetenida);
    }
}
