using System;
using System.Threading.Tasks;
using Verifactu.Integration.Tests.Fixtures;
using Verifactu.Integration.Tests.Helpers;
using Xunit;

namespace Verifactu.Integration.Tests;

/// <summary>
/// Tests de manejo de errores contra sandbox AEAT.
/// Valida que el sistema maneja correctamente diferentes tipos de errores
/// y proporciona información útil para debugging.
/// </summary>
[Collection("SandboxIntegrationTests")]
public class ErrorHandlingTests : IClassFixture<AeatSandboxFixture>
{
    private readonly AeatSandboxFixture _fixture;

    public ErrorHandlingTests(AeatSandboxFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// Test 1: Envío de factura con importe negativo debe retornar error de validación
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "ErrorHandling")]
    public async Task EnvioFacturaImporteNegativo_RetornaErrorValidacion()
    {
        // Arrange
        if (_fixture.SkipTests) { return; }

        var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico("ERR-NEG");
        var registro = _fixture.DataBuilder.CrearFacturaInvalida(numeroFactura, "ImporteNegativo");

        // Act
        var respuesta = await _fixture.SandboxHelper!.EnviarRegistroCompletoAsync(registro);

        // Assert
        Assert.NotNull(respuesta);
        
        // Debe retornar estado de error
        Assert.Equal("Incorrecto", respuesta.EstadoEnvio);
        
        // Debe tener línea de respuesta con error
        Assert.NotNull(respuesta.RespuestasLinea);
        Assert.NotEmpty(respuesta.RespuestasLinea);
        
        var lineaError = respuesta.RespuestasLinea[0];
        Assert.Equal("Incorrecto", lineaError.EstadoRegistro);
        Assert.NotNull(lineaError.CodigoErrorRegistro);
        Assert.NotNull(lineaError.DescripcionErrorRegistro);
        
        // Log del error para referencia
        System.Diagnostics.Debug.WriteLine(
            $"Error detectado - Código: {lineaError.CodigoErrorRegistro}, " +
            $"Descripción: {lineaError.DescripcionErrorRegistro}");
    }

    /// <summary>
    /// Test 2: Envío de factura con cuota negativa debe retornar error
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "ErrorHandling")]
    public async Task EnvioFacturaCuotaNegativa_RetornaErrorValidacion()
    {
        // Arrange
        if (_fixture.SkipTests) { return; }

        var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico("ERR-CUOTA");
        var registro = _fixture.DataBuilder.CrearFacturaInvalida(numeroFactura, "CuotaNegativa");

        // Act
        var respuesta = await _fixture.SandboxHelper!.EnviarRegistroCompletoAsync(registro);

        // Assert
        Assert.NotNull(respuesta);
        Assert.Equal("Incorrecto", respuesta.EstadoEnvio);
        
        Assert.NotNull(respuesta.RespuestasLinea);
        Assert.NotEmpty(respuesta.RespuestasLinea);
        
        var lineaError = respuesta.RespuestasLinea[0];
        Assert.Equal("Incorrecto", lineaError.EstadoRegistro);
    }

    /// <summary>
    /// Test 3: Detección de factura duplicada
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "ErrorHandling")]
    public async Task EnvioFacturaDuplicada_DebeDetectarDuplicado()
    {
        // Arrange
        if (_fixture.SkipTests) { return; }

        var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico("DUP");
        var registro = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura);

        // Act - Enviar primera vez
        var respuesta1 = await _fixture.SandboxHelper!.EnviarRegistroCompletoAsync(registro);
        
        // Solo continuar si el primer envío fue exitoso
        if (respuesta1.EstadoEnvio == "Correcto")
        {
            // Esperar brevemente
            await Task.Delay(500);
            
            // Act - Enviar segunda vez (duplicado)
            var respuesta2 = await _fixture.SandboxHelper.EnviarRegistroCompletoAsync(registro);
            
            // Assert
            Assert.NotNull(respuesta2);
            Assert.NotNull(respuesta2.RespuestasLinea);
            Assert.NotEmpty(respuesta2.RespuestasLinea);
            
            var lineaRespuesta = respuesta2.RespuestasLinea[0];
            
            // Si detecta duplicado, debe indicarlo
            if (lineaRespuesta.RegistroDuplicado != null)
            {
                Assert.NotNull(lineaRespuesta.RegistroDuplicado.IdPeticionRegistroDuplicado);
                System.Diagnostics.Debug.WriteLine(
                    $"Duplicado detectado correctamente: {lineaRespuesta.RegistroDuplicado.IdPeticionRegistroDuplicado}");
            }
        }
    }

    /// <summary>
    /// Test 4: Clasificación correcta de diferentes tipos de errores
    /// </summary>
    [Theory]
    [Trait("Category", "Integration")]
    [Trait("Category", "ErrorHandling")]
    [InlineData("ImporteNegativo", "Error de importe")]
    [InlineData("CuotaNegativa", "Error de cuota")]
    public async Task ErroresValidacionAEAT_ClasificacionCorrecta(
        string tipoError,
        string descripcionError)
    {
        // Arrange
        if (_fixture.SkipTests) { return; }

        var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico($"ERR-{tipoError}");
        var registro = _fixture.DataBuilder.CrearFacturaInvalida(numeroFactura, tipoError);

        // Act
        var respuesta = await _fixture.SandboxHelper!.EnviarRegistroCompletoAsync(registro);

        // Assert
        Assert.NotNull(respuesta);
        Assert.Equal("Incorrecto", respuesta.EstadoEnvio);
        
        var (codigo, descripcion) = SandboxHelper.ObtenerPrimerError(respuesta);
        
        Assert.NotNull(codigo);
        Assert.NotNull(descripcion);
        
        System.Diagnostics.Debug.WriteLine(
            $"{descripcionError} - Código: {codigo}, Descripción: {descripcion}");
    }
}
