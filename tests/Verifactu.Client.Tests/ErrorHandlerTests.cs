using System;
using System.Collections.Generic;
using Xunit;
using Verifactu.Client.Services;
using Verifactu.Client.Models;

namespace Verifactu.Client.Tests;

/// <summary>
/// Tests para el catálogo de errores AEAT
/// </summary>
public class ErrorCatalogTests
{
    [Fact]
    public void ObtenerInfoError_ConCodigoValido_DebeRetornarInformacion()
    {
        // Act
        var info = ErrorCatalog.ObtenerInfoError("4001");

        // Assert
        Assert.NotNull(info);
        Assert.Equal("4001", info.Codigo);
        Assert.Contains("NIF", info.Descripcion);
        Assert.Equal(ErrorCatalog.TipoError.Negocio, info.Tipo);
        Assert.False(info.EsAdmisible);
    }

    [Fact]
    public void ObtenerInfoError_ConCodigoNoExistente_DebeRetornarGenerico()
    {
        // Act
        var info = ErrorCatalog.ObtenerInfoError("9999");

        // Assert
        Assert.NotNull(info);
        Assert.Equal("9999", info.Codigo);
        Assert.Contains("no catalogado", info.Descripcion);
        Assert.False(info.EsAdmisible);
    }

    [Fact]
    public void ObtenerInfoError_ConCodigoNulo_DebeRetornarGenerico()
    {
        // Act
        var info = ErrorCatalog.ObtenerInfoError(null);

        // Assert
        Assert.NotNull(info);
        Assert.Equal("0000", info.Codigo);
        Assert.Equal(ErrorCatalog.TipoError.Desconocido, info.Tipo);
    }

    [Theory]
    [InlineData("9001", true)]  // Error temporal
    [InlineData("9002", true)]  // Timeout
    [InlineData("4001", false)] // Error de negocio
    [InlineData("2001", false)] // Error sintáctico
    public void EsErrorRecuperable_DebeClasificarCorrectamente(string codigoError, bool esperado)
    {
        // Act
        var resultado = ErrorCatalog.EsErrorRecuperable(codigoError);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData("8001", true)]  // Error admisible
    [InlineData("8002", true)]  // Error admisible
    [InlineData("4001", false)] // Error no admisible
    [InlineData("5001", false)] // Error no admisible
    public void EsErrorAdmisible_DebeClasificarCorrectamente(string codigoError, bool esperado)
    {
        // Act
        var resultado = ErrorCatalog.EsErrorAdmisible(codigoError);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData("6001", true)]  // Duplicado
    [InlineData("8001", true)]  // Error admisible
    [InlineData("4001", false)] // No requiere subsanación
    public void RequiereSubsanacion_DebeClasificarCorrectamente(string codigoError, bool esperado)
    {
        // Act
        var resultado = ErrorCatalog.RequiereSubsanacion(codigoError);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void ObtenerTodosLosErrores_DebeRetornarListaNoVacia()
    {
        // Act
        var errores = ErrorCatalog.ObtenerTodosLosErrores();

        // Assert
        Assert.NotNull(errores);
        Assert.NotEmpty(errores);
    }

    [Fact]
    public void ObtenerErroresPorTipo_TipoNegocio_DebeRetornarSoloNegocio()
    {
        // Act
        var errores = ErrorCatalog.ObtenerErroresPorTipo(ErrorCatalog.TipoError.Negocio);

        // Assert
        Assert.NotNull(errores);
        Assert.NotEmpty(errores);
        Assert.All(errores, e => Assert.Equal(ErrorCatalog.TipoError.Negocio, e.Tipo));
    }

    [Fact]
    public void ObtenerErroresPorTipo_TipoTemporal_DebeRetornarSoloTemporales()
    {
        // Act
        var errores = ErrorCatalog.ObtenerErroresPorTipo(ErrorCatalog.TipoError.Temporal);

        // Assert
        Assert.NotNull(errores);
        Assert.NotEmpty(errores);
        Assert.All(errores, e => Assert.Equal(ErrorCatalog.TipoError.Temporal, e.Tipo));
    }

    [Theory]
    [InlineData("1001", ErrorCatalog.TipoError.Configuracion)]
    [InlineData("2001", ErrorCatalog.TipoError.Sintactico)]
    [InlineData("4001", ErrorCatalog.TipoError.Negocio)]
    [InlineData("9001", ErrorCatalog.TipoError.Temporal)]
    public void InferirTipoPorPrefijo_DebeClasificarCorrectamente(string codigoError, ErrorCatalog.TipoError esperado)
    {
        // Act
        var info = ErrorCatalog.ObtenerInfoError(codigoError);

        // Assert
        Assert.Equal(esperado, info.Tipo);
    }
}

/// <summary>
/// Tests para el manejador de errores
/// </summary>
public class ErrorHandlerTests
{
    private readonly ErrorHandler _errorHandler;

    public ErrorHandlerTests()
    {
        _errorHandler = new ErrorHandler();
    }

    [Fact]
    public void AnalizarRespuesta_RespuestaNula_DebeLanzarExcepcion()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _errorHandler.AnalizarRespuesta(null!));
    }

    [Fact]
    public void AnalizarRespuesta_RespuestaCorrecta_DebeIndicarExito()
    {
        // Arrange
        var respuesta = new RespuestaSuministro
        {
            EstadoEnvio = "Correcto",
            CSV = "ABC123",
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea
                {
                    EstadoRegistro = "Correcto",
                    IDFactura = new IDFactura
                    {
                        NumSerieFactura = "FAC-001",
                        FechaExpedicionFactura = DateTime.Now
                    }
                }
            }
        };

        // Act
        var resultado = _errorHandler.AnalizarRespuesta(respuesta);

        // Assert
        Assert.True(resultado.EsExitoso);
        Assert.Equal("Correcto", resultado.EstadoEnvio);
        Assert.Equal("ABC123", resultado.CSV);
        Assert.Equal(1, resultado.RegistrosProcesados);
        Assert.Equal(1, resultado.RegistrosCorrectos);
        Assert.Empty(resultado.ErroresNoAdmisibles);
        Assert.Empty(resultado.ErroresAdmisibles);
    }

    [Fact]
    public void AnalizarRespuesta_RespuestaConErrorNoAdmisible_DebeDetectarError()
    {
        // Arrange
        var respuesta = new RespuestaSuministro
        {
            EstadoEnvio = "Incorrecto",
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea
                {
                    EstadoRegistro = "Incorrecto",
                    CodigoErrorRegistro = "4001",
                    DescripcionErrorRegistro = "NIF no identificado",
                    IDFactura = new IDFactura
                    {
                        NumSerieFactura = "FAC-002",
                        FechaExpedicionFactura = DateTime.Now
                    }
                }
            }
        };

        // Act
        var resultado = _errorHandler.AnalizarRespuesta(respuesta);

        // Assert
        Assert.False(resultado.EsExitoso);
        Assert.Equal("Incorrecto", resultado.EstadoEnvio);
        Assert.Equal(1, resultado.RegistrosProcesados);
        Assert.Equal(0, resultado.RegistrosCorrectos);
        Assert.Equal(1, resultado.RegistrosRechazados);
        Assert.Single(resultado.ErroresNoAdmisibles);
        Assert.Equal("4001", resultado.ErroresNoAdmisibles[0].CodigoError);
        Assert.False(resultado.ErroresNoAdmisibles[0].EsAdmisible);
    }

    [Fact]
    public void AnalizarRespuesta_RespuestaConErrorAdmisible_DebeDetectarError()
    {
        // Arrange
        var respuesta = new RespuestaSuministro
        {
            EstadoEnvio = "Correcto",
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea
                {
                    EstadoRegistro = "AceptadoConErrores",
                    CodigoErrorRegistro = "8001",
                    DescripcionErrorRegistro = "Campo opcional con formato no recomendado",
                    IDFactura = new IDFactura
                    {
                        NumSerieFactura = "FAC-003",
                        FechaExpedicionFactura = DateTime.Now
                    }
                }
            }
        };

        // Act
        var resultado = _errorHandler.AnalizarRespuesta(respuesta);

        // Assert
        Assert.True(resultado.EsExitoso);
        Assert.Equal(1, resultado.RegistrosAceptadosConErrores);
        Assert.Single(resultado.ErroresAdmisibles);
        Assert.Equal("8001", resultado.ErroresAdmisibles[0].CodigoError);
        Assert.True(resultado.ErroresAdmisibles[0].EsAdmisible);
    }

    [Fact]
    public void AnalizarRespuesta_RespuestaParcial_DebeDetectarMixto()
    {
        // Arrange
        var respuesta = new RespuestaSuministro
        {
            EstadoEnvio = "ParcialmenteCorrecto",
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea
                {
                    EstadoRegistro = "Correcto",
                    IDFactura = new IDFactura { NumSerieFactura = "FAC-001" }
                },
                new RespuestaLinea
                {
                    EstadoRegistro = "Incorrecto",
                    CodigoErrorRegistro = "5001",
                    IDFactura = new IDFactura { NumSerieFactura = "FAC-002" }
                },
                new RespuestaLinea
                {
                    EstadoRegistro = "AceptadoConErrores",
                    CodigoErrorRegistro = "8001",
                    IDFactura = new IDFactura { NumSerieFactura = "FAC-003" }
                }
            }
        };

        // Act
        var resultado = _errorHandler.AnalizarRespuesta(respuesta);

        // Assert
        Assert.False(resultado.EsExitoso);
        Assert.True(resultado.TieneErroresParciales);
        Assert.Equal(3, resultado.RegistrosProcesados);
        Assert.Equal(1, resultado.RegistrosCorrectos);
        Assert.Equal(1, resultado.RegistrosRechazados);
        Assert.Equal(1, resultado.RegistrosAceptadosConErrores);
        Assert.Single(resultado.ErroresNoAdmisibles);
        Assert.Single(resultado.ErroresAdmisibles);
    }

    [Fact]
    public void AnalizarRespuesta_ConDuplicado_DebeRegistrarDuplicado()
    {
        // Arrange
        var respuesta = new RespuestaSuministro
        {
            EstadoEnvio = "Incorrecto",
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea
                {
                    EstadoRegistro = "Incorrecto",
                    CodigoErrorRegistro = "6001",
                    IDFactura = new IDFactura { NumSerieFactura = "FAC-001" },
                    RegistroDuplicado = new RegistroDuplicado
                    {
                        IdPeticionRegistroDuplicado = "PREV-123",
                        EstadoRegistroDuplicado = "Correcta"
                    }
                }
            }
        };

        // Act
        var resultado = _errorHandler.AnalizarRespuesta(respuesta);

        // Assert
        Assert.Equal(1, resultado.RegistrosDuplicados);
        Assert.Contains("Duplicado", resultado.InformacionAdicional[0]);
        Assert.Contains("PREV-123", resultado.InformacionAdicional[0]);
    }

    [Theory]
    [InlineData("9001", true)]
    [InlineData("4001", false)]
    public void EsErrorRecuperable_DebeRetornarCorrectamente(string codigoError, bool esperado)
    {
        // Act
        var resultado = _errorHandler.EsErrorRecuperable(codigoError);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void DebeReintentarse_ConErroresRecuperables_DebeRetornarTrue()
    {
        // Arrange
        var respuesta = new RespuestaSuministro
        {
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea
                {
                    EstadoRegistro = "Incorrecto",
                    CodigoErrorRegistro = "9001" // Error temporal recuperable
                }
            }
        };

        // Act
        var resultado = _errorHandler.DebeReintentarse(respuesta);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void DebeReintentarse_ConErroresNoRecuperables_DebeRetornarFalse()
    {
        // Arrange
        var respuesta = new RespuestaSuministro
        {
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea
                {
                    EstadoRegistro = "Incorrecto",
                    CodigoErrorRegistro = "4001" // Error de negocio no recuperable
                }
            }
        };

        // Act
        var resultado = _errorHandler.DebeReintentarse(respuesta);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void CalcularTiempoEspera_ConTiempoAEAT_DebeRespetarlo()
    {
        // Arrange
        var respuesta = new RespuestaSuministro
        {
            TiempoEsperaEnvio = 120 // 2 minutos
        };

        // Act
        var resultado = _errorHandler.CalcularTiempoEspera(respuesta, 0);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(120), resultado);
    }

    [Fact]
    public void CalcularTiempoEspera_SinTiempoAEAT_DebeUsarBackoff()
    {
        // Arrange
        var respuesta = new RespuestaSuministro();

        // Act - Primer intento
        var resultado0 = _errorHandler.CalcularTiempoEspera(respuesta, 0);
        var resultado1 = _errorHandler.CalcularTiempoEspera(respuesta, 1);
        var resultado2 = _errorHandler.CalcularTiempoEspera(respuesta, 2);

        // Assert - Backoff exponencial: 1, 2, 4 segundos
        Assert.Equal(TimeSpan.FromSeconds(1), resultado0);
        Assert.Equal(TimeSpan.FromSeconds(2), resultado1);
        Assert.Equal(TimeSpan.FromSeconds(4), resultado2);
    }

    [Fact]
    public void CalcularTiempoEspera_ConIntentoAlto_NoDebeExcederMaximo()
    {
        // Arrange
        var respuesta = new RespuestaSuministro();

        // Act - Intento muy alto que excedería el máximo
        var resultado = _errorHandler.CalcularTiempoEspera(respuesta, 20);

        // Assert - No debe exceder 5 minutos (300 segundos)
        Assert.True(resultado <= TimeSpan.FromSeconds(300));
    }

    [Fact]
    public void ObtenerAccionRecomendada_DebeRetornarAccion()
    {
        // Act
        var accion = _errorHandler.ObtenerAccionRecomendada("4001");

        // Assert
        Assert.NotNull(accion);
        Assert.NotEmpty(accion);
        Assert.Contains("NIF", accion);
    }
}
