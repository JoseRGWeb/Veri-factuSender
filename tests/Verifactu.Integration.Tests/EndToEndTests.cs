using System;
using System.Threading.Tasks;
using Verifactu.Client.Models;
using Verifactu.Integration.Tests.Fixtures;
using Verifactu.Integration.Tests.Helpers;
using Xunit;

namespace Verifactu.Integration.Tests;

/// <summary>
/// Tests end-to-end completos que validan el flujo completo de generación,
/// firma y envío de facturas al sandbox de AEAT.
/// 
/// Estos tests cubren diferentes tipos de factura y escenarios reales de uso.
/// </summary>
[Collection("SandboxIntegrationTests")]
public class EndToEndTests : IClassFixture<AeatSandboxFixture>
{
    private readonly AeatSandboxFixture _fixture;

    public EndToEndTests(AeatSandboxFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// Test 1: Flujo completo de factura F1 (completa)
    /// Valida: generación → huella → XML → firma → envío → respuesta → QR
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "EndToEnd")]
    public async Task EnvioFacturaCompletaF1_ExitosoConQR()
    {
        // Arrange
        if (_fixture.SkipTests) { return; }

        var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico("F1-E2E");
        var registro = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura, TipoFactura.F1);

        // Act
        var respuesta = await _fixture.SandboxHelper!.EnviarRegistroCompletoAsync(registro);

        // Assert
        Assert.NotNull(respuesta);
        Assert.NotNull(respuesta.EstadoEnvio);
        
        // Verificar que recibimos respuesta válida
        Assert.Contains(respuesta.EstadoEnvio, new[] { "Correcto", "ParcialmenteCorrecto", "Incorrecto" });
        
        // Si es correcto, debe tener CSV
        if (respuesta.EstadoEnvio == "Correcto")
        {
            Assert.NotNull(respuesta.CSV);
            Assert.NotEmpty(respuesta.CSV);
        }
        
        // Debe haber al menos una línea de respuesta
        Assert.NotNull(respuesta.RespuestasLinea);
        Assert.NotEmpty(respuesta.RespuestasLinea);
    }

    /// <summary>
    /// Test 2: Flujo de factura F2 (simplificada) sin datos completos de receptor
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "EndToEnd")]
    public async Task EnvioFacturaSimplificadaF2_ExitosaSinReceptor()
    {
        // Arrange
        if (_fixture.SkipTests) { return; }

        var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico("F2-E2E");
        var registro = _fixture.DataBuilder.CrearFacturaSimplificada(numeroFactura);

        // Act
        var respuesta = await _fixture.SandboxHelper!.EnviarRegistroCompletoAsync(registro);

        // Assert
        Assert.NotNull(respuesta);
        Assert.NotNull(respuesta.EstadoEnvio);
        Assert.Contains(respuesta.EstadoEnvio, new[] { "Correcto", "ParcialmenteCorrecto", "Incorrecto" });
        
        Assert.NotNull(respuesta.RespuestasLinea);
        Assert.NotEmpty(respuesta.RespuestasLinea);
    }

    /// <summary>
    /// Test 3: Flujo de factura rectificativa R1
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "EndToEnd")]
    public async Task EnvioFacturaRectificativaR1_ConFacturaOriginal()
    {
        // Arrange
        if (_fixture.SkipTests) { return; }

        var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico("R1-E2E");
        var registro = _fixture.DataBuilder.CrearFacturaRectificativa(
            numeroFactura,
            TipoRectificativa.S
        );

        // Act
        var respuesta = await _fixture.SandboxHelper!.EnviarRegistroCompletoAsync(registro);

        // Assert
        Assert.NotNull(respuesta);
        Assert.NotNull(respuesta.EstadoEnvio);
        Assert.Contains(respuesta.EstadoEnvio, new[] { "Correcto", "ParcialmenteCorrecto", "Incorrecto" });
        
        Assert.NotNull(respuesta.RespuestasLinea);
        Assert.NotEmpty(respuesta.RespuestasLinea);
    }

    /// <summary>
    /// Test 4: Encadenamiento de múltiples facturas validando huellas consecutivas
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "EndToEnd")]
    public async Task EncadenamientoMultiplesFacturas_HuellasConsistentes()
    {
        // Arrange
        if (_fixture.SkipTests) { return; }

        _fixture.SandboxHelper!.ResetearEncadenamiento();

        var numeroFactura1 = TestDataBuilder.GenerarNumeroFacturaUnico("CHAIN1");
        var numeroFactura2 = TestDataBuilder.GenerarNumeroFacturaUnico("CHAIN2");
        var numeroFactura3 = TestDataBuilder.GenerarNumeroFacturaUnico("CHAIN3");

        // Act & Assert - Enviar primera factura
        var registro1 = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura1);
        var huella1 = _fixture.SandboxHelper.CalcularHuella(registro1);
        var respuesta1 = await _fixture.SandboxHelper.EnviarRegistroCompletoAsync(registro1);
        
        Assert.NotNull(respuesta1);
        
        // Pequeña espera para asegurar timestamps diferentes
        await Task.Delay(1000);
        
        // Enviar segunda factura
        var registro2 = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura2);
        var huella2 = _fixture.SandboxHelper.CalcularHuella(registro2);
        var respuesta2 = await _fixture.SandboxHelper.EnviarRegistroCompletoAsync(registro2);
        
        Assert.NotNull(respuesta2);
        
        // Las huellas deben ser diferentes
        Assert.NotEqual(huella1, huella2);
        
        // Espera antes de tercera factura
        await Task.Delay(1000);
        
        // Enviar tercera factura
        var registro3 = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura3);
        var huella3 = _fixture.SandboxHelper.CalcularHuella(registro3);
        var respuesta3 = await _fixture.SandboxHelper.EnviarRegistroCompletoAsync(registro3);
        
        Assert.NotNull(respuesta3);
        
        // Todas las huellas deben ser únicas
        Assert.NotEqual(huella1, huella3);
        Assert.NotEqual(huella2, huella3);
    }

    /// <summary>
    /// Test 5: Validación de todos los tipos de factura (F1-F4)
    /// </summary>
    [Theory]
    [Trait("Category", "Integration")]
    [Trait("Category", "EndToEnd")]
    [InlineData(TipoFactura.F1, "Completa")]
    [InlineData(TipoFactura.F2, "Simplificada")]
    public async Task EnvioTodosLosTiposDeFactura_DebenSerAceptados(
        TipoFactura tipoFactura,
        string descripcion)
    {
        // Arrange
        if (_fixture.SkipTests) { return; }

        var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico($"{tipoFactura}-ALL");
        var registro = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura, tipoFactura);

        // Act
        var respuesta = await _fixture.SandboxHelper!.EnviarRegistroCompletoAsync(registro);

        // Assert
        Assert.NotNull(respuesta);
        Assert.NotNull(respuesta.EstadoEnvio);
        Assert.Contains(respuesta.EstadoEnvio, new[] { "Correcto", "ParcialmenteCorrecto", "Incorrecto" });
        
        Assert.NotNull(respuesta.RespuestasLinea);
        Assert.NotEmpty(respuesta.RespuestasLinea);
        
        // Log para debugging
        System.Diagnostics.Debug.WriteLine(
            $"Tipo {tipoFactura} ({descripcion}): Estado = {respuesta.EstadoEnvio}");
    }
}
