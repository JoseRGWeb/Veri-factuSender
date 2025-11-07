using System;
using System.Collections.Generic;
using Verifactu.Client.Models;
using Xunit;

namespace Verifactu.Client.Tests;

/// <summary>
/// Tests para validar los nuevos modelos de datos y enums según especificación VERI*FACTU
/// </summary>
public class ModelosComplementariosTests
{
    [Fact]
    public void TipoFactura_DebeContenerTodosLosValores()
    {
        // Arrange & Act
        var tiposFactura = Enum.GetValues<TipoFactura>();
        
        // Assert
        Assert.Equal(9, tiposFactura.Length);
        Assert.Contains(TipoFactura.F1, tiposFactura);
        Assert.Contains(TipoFactura.F2, tiposFactura);
        Assert.Contains(TipoFactura.F3, tiposFactura);
        Assert.Contains(TipoFactura.F4, tiposFactura);
        Assert.Contains(TipoFactura.R1, tiposFactura);
        Assert.Contains(TipoFactura.R2, tiposFactura);
        Assert.Contains(TipoFactura.R3, tiposFactura);
        Assert.Contains(TipoFactura.R4, tiposFactura);
        Assert.Contains(TipoFactura.R5, tiposFactura);
    }

    [Fact]
    public void TipoRectificativa_DebeContenerValoresCorrectos()
    {
        // Arrange & Act
        var tiposRectificativa = Enum.GetValues<TipoRectificativa>();
        
        // Assert
        Assert.Equal(2, tiposRectificativa.Length);
        Assert.Contains(TipoRectificativa.S, tiposRectificativa); // Por sustitución
        Assert.Contains(TipoRectificativa.I, tiposRectificativa); // Por diferencias
    }

    [Fact]
    public void CalificacionOperacion_DebeContenerTodosLosValores()
    {
        // Arrange & Act
        var calificaciones = Enum.GetValues<CalificacionOperacion>();
        
        // Assert
        Assert.Equal(5, calificaciones.Length);
        Assert.Contains(CalificacionOperacion.S1, calificaciones);
        Assert.Contains(CalificacionOperacion.S2, calificaciones);
        Assert.Contains(CalificacionOperacion.S3, calificaciones);
        Assert.Contains(CalificacionOperacion.N1, calificaciones);
        Assert.Contains(CalificacionOperacion.N2, calificaciones);
    }

    [Fact]
    public void FacturaRectificada_DebeCrearseCorrectamente()
    {
        // Arrange
        var numSerie = "A/2024/001";
        var fecha = new DateTime(2024, 1, 15);
        
        // Act
        var facturaRectificada = new FacturaRectificada(numSerie, fecha);
        
        // Assert
        Assert.Equal(numSerie, facturaRectificada.NumSerieFactura);
        Assert.Equal(fecha, facturaRectificada.FechaExpedicionFactura);
    }

    [Fact]
    public void IDOtro_DebeCrearseCorrectamente_ParaIdentificacionExtranjera()
    {
        // Arrange
        var codigoPais = "FR";
        var idType = "02";
        var id = "123456789";
        
        // Act
        var idOtro = new IDOtro(codigoPais, idType, id);
        
        // Assert
        Assert.Equal(codigoPais, idOtro.CodigoPais);
        Assert.Equal(idType, idOtro.IDType);
        Assert.Equal(id, idOtro.ID);
    }

    [Fact]
    public void DetalleIVA_DebeCrearseCorrectamente()
    {
        // Arrange
        var baseImponible = 100m;
        var tipoImpositivo = 21m;
        var cuotaRepercutida = 21m;
        
        // Act
        var detalleIVA = new DetalleIVA(baseImponible, tipoImpositivo, cuotaRepercutida);
        
        // Assert
        Assert.Equal(baseImponible, detalleIVA.BaseImponible);
        Assert.Equal(tipoImpositivo, detalleIVA.TipoImpositivo);
        Assert.Equal(cuotaRepercutida, detalleIVA.CuotaRepercutida);
        Assert.Null(detalleIVA.TipoRecargoEquivalencia);
        Assert.Null(detalleIVA.CuotaRecargoEquivalencia);
    }

    [Fact]
    public void DetalleIVA_DebeIncluirRecargoEquivalencia_CuandoSeEspecifica()
    {
        // Arrange & Act
        var detalleIVA = new DetalleIVA(
            BaseImponible: 100m,
            TipoImpositivo: 21m,
            CuotaRepercutida: 21m,
            TipoRecargoEquivalencia: 5.2m,
            CuotaRecargoEquivalencia: 5.2m
        );
        
        // Assert
        Assert.Equal(5.2m, detalleIVA.TipoRecargoEquivalencia);
        Assert.Equal(5.2m, detalleIVA.CuotaRecargoEquivalencia);
    }

    [Fact]
    public void DetalleIRPF_DebeCrearseCorrectamente()
    {
        // Arrange
        var baseImponible = 1000m;
        var tipoRetencion = 15m;
        var cuotaRetenida = 150m;
        
        // Act
        var detalleIRPF = new DetalleIRPF(baseImponible, tipoRetencion, cuotaRetenida);
        
        // Assert
        Assert.Equal(baseImponible, detalleIRPF.BaseImponible);
        Assert.Equal(tipoRetencion, detalleIRPF.TipoRetencion);
        Assert.Equal(cuotaRetenida, detalleIRPF.CuotaRetenida);
    }

    [Fact]
    public void FacturacionTerceros_DebeCrearseCorrectamente()
    {
        // Arrange
        var nif = "B12345678";
        var nombreRazon = "EMPRESA TERCERA SL";
        
        // Act
        var terceros = new FacturacionTerceros(nif, nombreRazon);
        
        // Assert
        Assert.Equal(nif, terceros.NIF);
        Assert.Equal(nombreRazon, terceros.NombreRazon);
    }

    [Fact]
    public void DestinatarioCompleto_DebeCrearseCorrectamente_ConNIF()
    {
        // Arrange
        var nombreRazon = "CLIENTE SL";
        var nif = "B87654321";
        
        // Act
        var destinatario = new DestinatarioCompleto(nombreRazon, NIF: nif);
        
        // Assert
        Assert.Equal(nombreRazon, destinatario.NombreRazon);
        Assert.Equal(nif, destinatario.NIF);
        Assert.Null(destinatario.IDOtro);
    }

    [Fact]
    public void DestinatarioCompleto_DebeCrearseCorrectamente_ConIDOtro()
    {
        // Arrange
        var nombreRazon = "CLIENTE EXTRANJERO";
        var idOtro = new IDOtro("FR", "02", "123456789");
        
        // Act
        var destinatario = new DestinatarioCompleto(nombreRazon, IDOtro: idOtro);
        
        // Assert
        Assert.Equal(nombreRazon, destinatario.NombreRazon);
        Assert.Null(destinatario.NIF);
        Assert.NotNull(destinatario.IDOtro);
        Assert.Equal("FR", destinatario.IDOtro.CodigoPais);
    }
}
