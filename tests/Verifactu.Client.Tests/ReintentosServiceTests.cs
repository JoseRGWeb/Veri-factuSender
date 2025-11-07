using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xunit;
using Verifactu.Client.Services;
using Verifactu.Client.Models;

namespace Verifactu.Client.Tests;

/// <summary>
/// Tests para el servicio de reintentos
/// </summary>
public class ReintentosServiceTests
{
    [Fact]
    public void OpcionesReintento_PorDefecto_DebeTenerValoresCorrectos()
    {
        // Act
        var opciones = OpcionesReintento.PorDefecto;

        // Assert
        Assert.Equal(3, opciones.MaximoIntentos);
        Assert.Equal(2, opciones.TiempoBaseSegundos);
        Assert.Equal(300, opciones.TiempoMaximoEsperaSegundos);
    }

    [Fact]
    public void OpcionesReintento_Produccion_DebeTenerValoresConservadores()
    {
        // Act
        var opciones = OpcionesReintento.Produccion;

        // Assert
        Assert.Equal(5, opciones.MaximoIntentos);
        Assert.Equal(5, opciones.TiempoBaseSegundos);
        Assert.Equal(600, opciones.TiempoMaximoEsperaSegundos);
    }

    [Fact]
    public void OpcionesReintento_Pruebas_DebeTenerValoresRapidos()
    {
        // Act
        var opciones = OpcionesReintento.Pruebas;

        // Assert
        Assert.Equal(2, opciones.MaximoIntentos);
        Assert.Equal(1, opciones.TiempoBaseSegundos);
        Assert.Equal(60, opciones.TiempoMaximoEsperaSegundos);
    }

    [Fact]
    public async Task EnviarConReintentosAsync_ExitoEnPrimerIntento_DebeRetornarExitoso()
    {
        // Arrange
        var mockSoapClient = new MockVerifactuSoapClient();
        var errorHandler = new ErrorHandler();
        var reintentosService = new ReintentosService(mockSoapClient, errorHandler);

        mockSoapClient.ConfigurarRespuesta(new RespuestaSuministro
        {
            EstadoEnvio = "Correcto",
            CSV = "TEST123",
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea { EstadoRegistro = "Correcto" }
            }
        });

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<test/>");
        var cert = new X509Certificate2();

        // Act
        var resultado = await reintentosService.EnviarConReintentosAsync(
            xmlDoc, 
            cert, 
            OpcionesReintento.Pruebas);

        // Assert
        Assert.True(resultado.Exitoso);
        Assert.Equal(1, resultado.NumeroIntentos);
        Assert.Equal(1, resultado.ExitosoEnIntento);
        Assert.Equal("TEST123", resultado.CSV);
        Assert.NotNull(resultado.UltimaRespuesta);
        Assert.Empty(resultado.TiemposEspera);
    }

    [Fact]
    public async Task EnviarConReintentosAsync_ErrorNoRecuperable_NoDebeReintentar()
    {
        // Arrange
        var mockSoapClient = new MockVerifactuSoapClient();
        var errorHandler = new ErrorHandler();
        var reintentosService = new ReintentosService(mockSoapClient, errorHandler);

        mockSoapClient.ConfigurarRespuesta(new RespuestaSuministro
        {
            EstadoEnvio = "Incorrecto",
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea 
                { 
                    EstadoRegistro = "Incorrecto",
                    CodigoErrorRegistro = "4001", // Error no recuperable
                    DescripcionErrorRegistro = "NIF no identificado"
                }
            }
        });

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<test/>");
        var cert = new X509Certificate2();

        // Act
        var resultado = await reintentosService.EnviarConReintentosAsync(
            xmlDoc, 
            cert, 
            OpcionesReintento.Pruebas);

        // Assert
        Assert.False(resultado.Exitoso);
        Assert.Equal(1, resultado.NumeroIntentos); // Solo 1 intento, no reintenta
        Assert.Null(resultado.ExitosoEnIntento);
        Assert.Contains("no recuperable", resultado.MotivoFallo);
    }

    [Fact]
    public async Task EnviarConReintentosAsync_ErrorRecuperable_DebeReintentar()
    {
        // Arrange
        var mockSoapClient = new MockVerifactuSoapClient();
        var errorHandler = new ErrorHandler();
        var reintentosService = new ReintentosService(mockSoapClient, errorHandler);

        // Primera llamada retorna error recuperable, segunda llamada es exitosa
        var respuestas = new Queue<RespuestaSuministro>();
        respuestas.Enqueue(new RespuestaSuministro
        {
            EstadoEnvio = "Incorrecto",
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea 
                { 
                    EstadoRegistro = "Incorrecto",
                    CodigoErrorRegistro = "9001", // Error temporal recuperable
                }
            }
        });
        respuestas.Enqueue(new RespuestaSuministro
        {
            EstadoEnvio = "Correcto",
            CSV = "SUCCESS",
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea { EstadoRegistro = "Correcto" }
            }
        });

        mockSoapClient.ConfigurarRespuestasMultiples(respuestas);

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<test/>");
        var cert = new X509Certificate2();

        // Act
        var resultado = await reintentosService.EnviarConReintentosAsync(
            xmlDoc, 
            cert, 
            OpcionesReintento.Pruebas);

        // Assert
        Assert.True(resultado.Exitoso);
        Assert.Equal(2, resultado.NumeroIntentos);
        Assert.Equal(2, resultado.ExitosoEnIntento);
        Assert.Equal("SUCCESS", resultado.CSV);
        Assert.Single(resultado.TiemposEspera); // 1 espera entre los 2 intentos
    }

    [Fact]
    public async Task EnviarConReintentosAsync_MaximoIntentosAlcanzado_DebeRetornarFallo()
    {
        // Arrange
        var mockSoapClient = new MockVerifactuSoapClient();
        var errorHandler = new ErrorHandler();
        var reintentosService = new ReintentosService(mockSoapClient, errorHandler);

        // Siempre retorna error recuperable
        mockSoapClient.ConfigurarRespuesta(new RespuestaSuministro
        {
            EstadoEnvio = "Incorrecto",
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea 
                { 
                    EstadoRegistro = "Incorrecto",
                    CodigoErrorRegistro = "9001", // Error temporal
                }
            }
        });

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<test/>");
        var cert = new X509Certificate2();

        var opciones = new OpcionesReintento
        {
            MaximoIntentos = 2,
            TiempoBaseSegundos = 0.1, // Muy corto para test rápido
            TiempoMaximoEsperaSegundos = 1
        };

        // Act
        var resultado = await reintentosService.EnviarConReintentosAsync(
            xmlDoc, 
            cert, 
            opciones);

        // Assert
        Assert.False(resultado.Exitoso);
        Assert.Equal(2, resultado.NumeroIntentos);
        Assert.Null(resultado.ExitosoEnIntento);
        Assert.Contains("Máximo", resultado.MotivoFallo);
        Assert.Equal(1, resultado.TiemposEspera.Count); // 1 espera entre los 2 intentos
    }

    [Fact]
    public async Task EnviarConReintentosAsync_RespetaTiempoEsperaAEAT()
    {
        // Arrange
        var mockSoapClient = new MockVerifactuSoapClient();
        var errorHandler = new ErrorHandler();
        var reintentosService = new ReintentosService(mockSoapClient, errorHandler);

        var respuestas = new Queue<RespuestaSuministro>();
        respuestas.Enqueue(new RespuestaSuministro
        {
            EstadoEnvio = "Incorrecto",
            TiempoEsperaEnvio = 2, // AEAT indica esperar 2 segundos
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea 
                { 
                    EstadoRegistro = "Incorrecto",
                    CodigoErrorRegistro = "9001",
                }
            }
        });
        respuestas.Enqueue(new RespuestaSuministro
        {
            EstadoEnvio = "Correcto",
            CSV = "OK",
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea { EstadoRegistro = "Correcto" }
            }
        });

        mockSoapClient.ConfigurarRespuestasMultiples(respuestas);

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<test/>");
        var cert = new X509Certificate2();

        // Act
        var inicio = DateTime.UtcNow;
        var resultado = await reintentosService.EnviarConReintentosAsync(
            xmlDoc, 
            cert, 
            OpcionesReintento.Pruebas);
        var duracion = DateTime.UtcNow - inicio;

        // Assert
        Assert.True(resultado.Exitoso);
        Assert.Equal(2, resultado.NumeroIntentos);
        Assert.Single(resultado.TiemposEspera);
        Assert.Equal(TimeSpan.FromSeconds(2), resultado.TiemposEspera[0]);
        Assert.True(duracion >= TimeSpan.FromSeconds(2)); // Debe haber esperado al menos 2 segundos
    }

    [Fact]
    public void ResultadoEnvioConReintentos_CSV_DebeRetornarCSVDeRespuesta()
    {
        // Arrange
        var resultado = new ResultadoEnvioConReintentos
        {
            UltimaRespuesta = new RespuestaSuministro
            {
                CSV = "TEST-CSV-123"
            }
        };

        // Act
        var csv = resultado.CSV;

        // Assert
        Assert.Equal("TEST-CSV-123", csv);
    }

    [Fact]
    public void ResultadoEnvioConReintentos_DuracionTotal_DebeCalcularCorrectamente()
    {
        // Arrange
        var inicio = DateTime.UtcNow;
        var resultado = new ResultadoEnvioConReintentos
        {
            FechaInicio = inicio,
            FechaFin = inicio.AddSeconds(10)
        };

        // Act
        var duracion = resultado.DuracionTotal;

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(10), duracion);
    }
}

/// <summary>
/// Mock del cliente SOAP para testing
/// </summary>
internal class MockVerifactuSoapClient : IVerifactuSoapClient
{
    private RespuestaSuministro? _respuestaFija;
    private Queue<RespuestaSuministro>? _respuestasMultiples;

    public void ConfigurarRespuesta(RespuestaSuministro respuesta)
    {
        _respuestaFija = respuesta;
    }

    public void ConfigurarRespuestasMultiples(Queue<RespuestaSuministro> respuestas)
    {
        _respuestasMultiples = respuestas;
    }

    public Task<string> EnviarRegistroAsync(XmlDocument xmlFirmado, X509Certificate2 cert, CancellationToken ct = default)
    {
        return Task.FromResult("<xml/>");
    }

    public Task<RespuestaSuministro> EnviarRegFacturacionAltaAsync(XmlDocument xmlFirmado, X509Certificate2 cert, CancellationToken ct = default)
    {
        if (_respuestasMultiples != null && _respuestasMultiples.Count > 0)
        {
            return Task.FromResult(_respuestasMultiples.Dequeue());
        }

        return Task.FromResult(_respuestaFija ?? new RespuestaSuministro());
    }

    public Task<RespuestaConsultaLR> ConsultarLRFacturasAsync(XmlDocument xmlConsulta, X509Certificate2 cert, CancellationToken ct = default)
    {
        return Task.FromResult(new RespuestaConsultaLR());
    }
}
