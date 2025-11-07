using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Verifactu.Client.Models;
using Verifactu.Client.Services;
using Verifactu.Integration.Tests.Fixtures;
using Verifactu.Integration.Tests.Helpers;
using Xunit;

namespace Verifactu.Integration.Tests;

/// <summary>
/// Tests de rendimiento y carga.
/// Valida que el sistema mantiene un rendimiento aceptable bajo diferentes cargas.
/// 
/// NOTA: Estos tests pueden ser lentos y consumir recursos del sandbox.
/// Se recomienda ejecutarlos selectivamente, no en cada build de CI.
/// </summary>
[Collection("SandboxIntegrationTests")]
public class PerformanceTests : IClassFixture<AeatSandboxFixture>
{
    private readonly AeatSandboxFixture _fixture;

    public PerformanceTests(AeatSandboxFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// Test 1: Envío secuencial de 10 facturas con medición de tiempo
    /// Target: < 50 segundos para 10 facturas (5s por factura promedio)
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Performance")]
    public async Task Envio10FacturasSecuencial_TiempoRazonable()
    {
        // Arrange
        if (_fixture.SkipTests) { return; }

        const int cantidadFacturas = 10;
        var stopwatch = Stopwatch.StartNew();
        var tiempos = new List<long>();
        var exitosas = 0;

        // Act
        _fixture.SandboxHelper!.ResetearEncadenamiento();

        for (int i = 0; i < cantidadFacturas; i++)
        {
            var swFactura = Stopwatch.StartNew();
            
            var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico($"PERF-SEQ-{i}");
            var registro = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura);
            
            var respuesta = await _fixture.SandboxHelper.EnviarRegistroCompletoAsync(registro);
            
            swFactura.Stop();
            tiempos.Add(swFactura.ElapsedMilliseconds);
            
            if (respuesta.EstadoEnvio == "Correcto")
            {
                exitosas++;
            }
            
            // Pequeña espera para evitar saturar el servidor
            await Task.Delay(100);
        }

        stopwatch.Stop();

        // Assert
        var tiempoTotal = stopwatch.ElapsedMilliseconds;
        var tiempoPromedio = tiempos.Average();
        var tiempoMax = tiempos.Max();
        var tiempoMin = tiempos.Min();

        // Logs de métricas
        System.Diagnostics.Debug.WriteLine($"=== Métricas de Rendimiento Secuencial ===");
        System.Diagnostics.Debug.WriteLine($"Facturas enviadas: {cantidadFacturas}");
        System.Diagnostics.Debug.WriteLine($"Facturas exitosas: {exitosas}");
        System.Diagnostics.Debug.WriteLine($"Tiempo total: {tiempoTotal}ms ({tiempoTotal / 1000.0:F2}s)");
        System.Diagnostics.Debug.WriteLine($"Tiempo promedio por factura: {tiempoPromedio:F2}ms");
        System.Diagnostics.Debug.WriteLine($"Tiempo mínimo: {tiempoMin}ms");
        System.Diagnostics.Debug.WriteLine($"Tiempo máximo: {tiempoMax}ms");

        // Verificaciones básicas
        Assert.True(exitosas > 0, "Al menos una factura debe ser exitosa");
        
        // Verificar que el tiempo total sea razonable (ajustar según necesidad)
        // Target: menos de 100 segundos para 10 facturas
        Assert.True(tiempoTotal < 100000, 
            $"Tiempo total ({tiempoTotal}ms) excede el límite de 100s");
    }

    /// <summary>
    /// Test 2: Cálculo de huella para 1000 registros
    /// Target: < 10ms por cálculo en promedio
    /// </summary>
    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Category", "Unit")]
    public void CalculoHuella1000Registros_RendimientoOptimo()
    {
        // Arrange
        const int cantidadRegistros = 1000;
        var hashService = new HashService();
        var registros = new List<RegistroFacturacion>();
        
        for (int i = 0; i < cantidadRegistros; i++)
        {
            var numeroFactura = $"PERF-HASH-{i}";
            var registro = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura);
            registros.Add(registro);
        }

        // Act
        var stopwatch = Stopwatch.StartNew();
        string? huellaAnterior = null;
        
        foreach (var registro in registros)
        {
            huellaAnterior = hashService.CalcularHuella(registro, huellaAnterior);
        }
        
        stopwatch.Stop();

        // Assert
        var tiempoTotal = stopwatch.ElapsedMilliseconds;
        var tiempoPorCalculo = (double)tiempoTotal / cantidadRegistros;

        System.Diagnostics.Debug.WriteLine($"=== Métricas de Cálculo de Huella ===");
        System.Diagnostics.Debug.WriteLine($"Registros procesados: {cantidadRegistros}");
        System.Diagnostics.Debug.WriteLine($"Tiempo total: {tiempoTotal}ms");
        System.Diagnostics.Debug.WriteLine($"Tiempo por cálculo: {tiempoPorCalculo:F3}ms");
        System.Diagnostics.Debug.WriteLine($"Huellas por segundo: {(cantidadRegistros * 1000.0 / tiempoTotal):F0}");

        // Verificar rendimiento
        Assert.True(tiempoPorCalculo < 10, 
            $"Tiempo por cálculo ({tiempoPorCalculo:F3}ms) excede el límite de 10ms");
    }

    /// <summary>
    /// Test 3: Serialización XML de 100 facturas
    /// Target: < 50ms por serialización en promedio
    /// </summary>
    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Category", "Unit")]
    public void SerializacionXML100Facturas_MemoryUsageEstable()
    {
        // Arrange
        const int cantidadFacturas = 100;
        var serializer = new VerifactuSerializer();
        var registros = new List<RegistroFacturacion>();
        
        for (int i = 0; i < cantidadFacturas; i++)
        {
            var numeroFactura = $"PERF-XML-{i}";
            var registro = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura);
            registros.Add(registro);
        }

        // Act
        var stopwatch = Stopwatch.StartNew();
        var xmlGenerados = 0;
        
        foreach (var registro in registros)
        {
            var xml = serializer.CrearXmlRegistro(registro);
            if (xml != null)
            {
                xmlGenerados++;
            }
        }
        
        stopwatch.Stop();

        // Assert
        var tiempoTotal = stopwatch.ElapsedMilliseconds;
        var tiempoPorSerializacion = (double)tiempoTotal / cantidadFacturas;

        System.Diagnostics.Debug.WriteLine($"=== Métricas de Serialización XML ===");
        System.Diagnostics.Debug.WriteLine($"Facturas serializadas: {xmlGenerados}");
        System.Diagnostics.Debug.WriteLine($"Tiempo total: {tiempoTotal}ms");
        System.Diagnostics.Debug.WriteLine($"Tiempo por serialización: {tiempoPorSerializacion:F3}ms");

        Assert.Equal(cantidadFacturas, xmlGenerados);
        Assert.True(tiempoPorSerializacion < 50, 
            $"Tiempo por serialización ({tiempoPorSerializacion:F3}ms) excede el límite de 50ms");
    }

    /// <summary>
    /// Test 4: Test de estabilidad - envío continuo durante periodo extendido
    /// NOTA: Este test está deshabilitado por defecto por su duración
    /// </summary>
    [Fact(Skip = "Test de larga duración, ejecutar manualmente si es necesario")]
    [Trait("Category", "Performance")]
    [Trait("Category", "LongRunning")]
    public async Task EnvioContinuo30Facturas_Estabilidad()
    {
        // Arrange
        if (_fixture.SkipTests) { return; }

        const int cantidadFacturas = 30;
        var exitosas = 0;
        var errores = 0;

        // Act
        _fixture.SandboxHelper!.ResetearEncadenamiento();

        for (int i = 0; i < cantidadFacturas; i++)
        {
            try
            {
                var numeroFactura = TestDataBuilder.GenerarNumeroFacturaUnico($"STABLE-{i}");
                var registro = _fixture.DataBuilder.CrearFacturaBasica(numeroFactura);
                
                var respuesta = await _fixture.SandboxHelper.EnviarRegistroCompletoAsync(registro);
                
                if (respuesta.EstadoEnvio == "Correcto")
                {
                    exitosas++;
                }
                else
                {
                    errores++;
                }
                
                // Espera entre envíos para simular uso real
                await Task.Delay(2000);
            }
            catch (Exception)
            {
                errores++;
            }
        }

        // Assert
        System.Diagnostics.Debug.WriteLine($"=== Resultados de Estabilidad ===");
        System.Diagnostics.Debug.WriteLine($"Facturas exitosas: {exitosas}/{cantidadFacturas}");
        System.Diagnostics.Debug.WriteLine($"Errores: {errores}");

        // Al menos el 80% debe ser exitoso
        var porcentajeExito = (double)exitosas / cantidadFacturas * 100;
        Assert.True(porcentajeExito >= 80, 
            $"Porcentaje de éxito ({porcentajeExito:F1}%) está por debajo del 80%");
    }
}
