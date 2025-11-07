using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Verifactu.Client.Models;

namespace Verifactu.Client.Services;

/// <summary>
/// Servicio para gestión y análisis de errores de respuestas AEAT.
/// Proporciona clasificación, logging estructurado y recomendaciones de manejo.
/// </summary>
public class ErrorHandler : IErrorHandler
{
    private readonly ILogger<ErrorHandler>? _logger;

    /// <summary>
    /// Constructor con logging opcional
    /// </summary>
    /// <param name="logger">Logger para registro estructurado de errores (opcional)</param>
    public ErrorHandler(ILogger<ErrorHandler>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Analiza una respuesta AEAT y extrae información de errores
    /// </summary>
    /// <param name="respuesta">Respuesta del servicio AEAT</param>
    /// <returns>Resultado del análisis con errores clasificados</returns>
    public ResultadoAnalisisErrores AnalizarRespuesta(RespuestaSuministro respuesta)
    {
        if (respuesta == null)
            throw new ArgumentNullException(nameof(respuesta));

        var resultado = new ResultadoAnalisisErrores
        {
            EstadoEnvio = respuesta.EstadoEnvio ?? "Desconocido",
            TiempoEsperaEnvio = respuesta.TiempoEsperaEnvio,
            CSV = respuesta.CSV
        };

        // Analizar estado global
        resultado.EsExitoso = EsEstadoExitoso(respuesta.EstadoEnvio);
        resultado.TieneErroresParciales = respuesta.EstadoEnvio == "ParcialmenteCorrecto";

        // Analizar cada línea de respuesta
        if (respuesta.RespuestasLinea != null)
        {
            foreach (var linea in respuesta.RespuestasLinea)
            {
                AnalizarLineaRespuesta(linea, resultado);
            }
        }

        // Clasificar errores
        ClasificarErrores(resultado);

        // Registrar en log
        RegistrarEnLog(resultado);

        return resultado;
    }

    /// <summary>
    /// Determina si un error específico es recuperable mediante reintento
    /// </summary>
    /// <param name="codigoError">Código del error AEAT</param>
    /// <returns>True si el error es recuperable</returns>
    public bool EsErrorRecuperable(string? codigoError)
    {
        return ErrorCatalog.EsErrorRecuperable(codigoError);
    }

    /// <summary>
    /// Obtiene la acción recomendada para un código de error
    /// </summary>
    /// <param name="codigoError">Código del error AEAT</param>
    /// <returns>Descripción de la acción recomendada</returns>
    public string ObtenerAccionRecomendada(string? codigoError)
    {
        var info = ErrorCatalog.ObtenerInfoError(codigoError);
        return info.AccionRecomendada;
    }

    /// <summary>
    /// Determina si una respuesta requiere reintentos automáticos
    /// </summary>
    /// <param name="respuesta">Respuesta AEAT</param>
    /// <returns>True si se debe reintentar el envío</returns>
    public bool DebeReintentarse(RespuestaSuministro respuesta)
    {
        if (respuesta == null || respuesta.RespuestasLinea == null)
            return false;

        // Reintentar si todos los errores son recuperables
        var erroresNoRecuperables = respuesta.RespuestasLinea
            .Where(l => l.EstadoRegistro == "Incorrecto")
            .Where(l => !string.IsNullOrWhiteSpace(l.CodigoErrorRegistro))
            .Where(l => !EsErrorRecuperable(l.CodigoErrorRegistro))
            .ToList();

        // Solo reintentar si no hay errores irrecuperables
        return erroresNoRecuperables.Count == 0 && 
               respuesta.RespuestasLinea.Any(l => l.EstadoRegistro == "Incorrecto");
    }

    /// <summary>
    /// Calcula el tiempo de espera antes del próximo intento basándose en la respuesta AEAT
    /// </summary>
    /// <param name="respuesta">Respuesta AEAT</param>
    /// <param name="numeroIntento">Número del intento actual (0-based)</param>
    /// <returns>Tiempo de espera recomendado</returns>
    public TimeSpan CalcularTiempoEspera(RespuestaSuministro? respuesta, int numeroIntento)
    {
        // Si AEAT especifica tiempo de espera, respetarlo
        if (respuesta?.TiempoEsperaEnvio > 0)
        {
            return TimeSpan.FromSeconds(respuesta.TiempoEsperaEnvio.Value);
        }

        // Backoff exponencial: 2^intento segundos (max 5 minutos)
        var segundos = Math.Min(Math.Pow(2, numeroIntento), 300);
        return TimeSpan.FromSeconds(segundos);
    }

    private void AnalizarLineaRespuesta(RespuestaLinea linea, ResultadoAnalisisErrores resultado)
    {
        var estado = linea.EstadoRegistro ?? "Desconocido";
        resultado.RegistrosProcesados++;

        switch (estado)
        {
            case "Correcto":
                resultado.RegistrosCorrectos++;
                break;
            
            case "AceptadoConErrores":
                resultado.RegistrosAceptadosConErrores++;
                if (!string.IsNullOrWhiteSpace(linea.CodigoErrorRegistro))
                {
                    var infoError = ErrorCatalog.ObtenerInfoError(linea.CodigoErrorRegistro);
                    resultado.ErroresAdmisibles.Add(new DetalleError
                    {
                        CodigoError = linea.CodigoErrorRegistro,
                        Descripcion = linea.DescripcionErrorRegistro ?? infoError.Descripcion,
                        TipoError = infoError.Tipo,
                        EsAdmisible = true,
                        FacturaAfectada = FormatearIdFactura(linea.IDFactura),
                        AccionRecomendada = infoError.AccionRecomendada
                    });
                }
                break;
            
            case "Incorrecto":
                resultado.RegistrosRechazados++;
                if (!string.IsNullOrWhiteSpace(linea.CodigoErrorRegistro))
                {
                    var infoError = ErrorCatalog.ObtenerInfoError(linea.CodigoErrorRegistro);
                    resultado.ErroresNoAdmisibles.Add(new DetalleError
                    {
                        CodigoError = linea.CodigoErrorRegistro,
                        Descripcion = linea.DescripcionErrorRegistro ?? infoError.Descripcion,
                        TipoError = infoError.Tipo,
                        EsAdmisible = false,
                        FacturaAfectada = FormatearIdFactura(linea.IDFactura),
                        AccionRecomendada = infoError.AccionRecomendada
                    });
                }
                break;
        }

        // Registrar duplicados
        if (linea.RegistroDuplicado != null)
        {
            resultado.RegistrosDuplicados++;
            resultado.InformacionAdicional.Add($"Duplicado detectado para factura {FormatearIdFactura(linea.IDFactura)} " +
                $"(IdPeticion original: {linea.RegistroDuplicado.IdPeticionRegistroDuplicado})");
        }
    }

    private void ClasificarErrores(ResultadoAnalisisErrores resultado)
    {
        // Clasificar errores no admisibles por tipo
        resultado.ErroresPorTipo = resultado.ErroresNoAdmisibles
            .GroupBy(e => e.TipoError)
            .ToDictionary(g => g.Key, g => g.Count());

        // Determinar si hay errores recuperables
        resultado.TieneErroresRecuperables = resultado.ErroresNoAdmisibles
            .Any(e => ErrorCatalog.EsErrorRecuperable(e.CodigoError));

        // Determinar si requiere subsanación
        resultado.RequiereSubsanacion = resultado.ErroresAdmisibles
            .Any(e => ErrorCatalog.RequiereSubsanacion(e.CodigoError));
    }

    private void RegistrarEnLog(ResultadoAnalisisErrores resultado)
    {
        if (_logger == null)
            return;

        // Log según severidad
        if (resultado.EsExitoso)
        {
            _logger.LogInformation(
                "Envío AEAT exitoso. CSV: {CSV}, Registros correctos: {Correctos}/{Total}",
                resultado.CSV,
                resultado.RegistrosCorrectos,
                resultado.RegistrosProcesados);
        }
        else if (resultado.TieneErroresParciales)
        {
            _logger.LogWarning(
                "Envío AEAT parcialmente correcto. Correctos: {Correctos}, Rechazados: {Rechazados}, Admisibles con error: {Admisibles}",
                resultado.RegistrosCorrectos,
                resultado.RegistrosRechazados,
                resultado.RegistrosAceptadosConErrores);

            foreach (var error in resultado.ErroresNoAdmisibles)
            {
                _logger.LogWarning(
                    "Error no admisible {Codigo}: {Descripcion}. Factura: {Factura}. Acción: {Accion}",
                    error.CodigoError,
                    error.Descripcion,
                    error.FacturaAfectada,
                    error.AccionRecomendada);
            }
        }
        else
        {
            _logger.LogError(
                "Envío AEAT rechazado completamente. Total errores: {TotalErrores}",
                resultado.ErroresNoAdmisibles.Count);

            foreach (var error in resultado.ErroresNoAdmisibles.Take(5)) // Primeros 5 errores
            {
                _logger.LogError(
                    "Error {Codigo}: {Descripcion}. Factura: {Factura}",
                    error.CodigoError,
                    error.Descripcion,
                    error.FacturaAfectada);
            }

            if (resultado.ErroresNoAdmisibles.Count > 5)
            {
                _logger.LogError("... y {Mas} errores adicionales", 
                    resultado.ErroresNoAdmisibles.Count - 5);
            }
        }

        // Registrar errores admisibles
        foreach (var error in resultado.ErroresAdmisibles)
        {
            _logger.LogInformation(
                "Error admisible {Codigo}: {Descripcion}. Factura: {Factura}. Requiere subsanación: {Subsanacion}",
                error.CodigoError,
                error.Descripcion,
                error.FacturaAfectada,
                ErrorCatalog.RequiereSubsanacion(error.CodigoError));
        }
    }

    private static bool EsEstadoExitoso(string? estado)
    {
        return estado == "Correcto";
    }

    private static string FormatearIdFactura(IDFactura? idFactura)
    {
        if (idFactura == null)
            return "N/A";

        return $"{idFactura.NumSerieFactura} ({idFactura.FechaExpedicionFactura:dd-MM-yyyy})";
    }
}

/// <summary>
/// Resultado del análisis de errores de una respuesta AEAT
/// </summary>
public class ResultadoAnalisisErrores
{
    /// <summary>
    /// Estado global del envío: "Correcto", "ParcialmenteCorrecto", "Incorrecto"
    /// </summary>
    public string EstadoEnvio { get; set; } = string.Empty;

    /// <summary>
    /// CSV (Código Seguro de Verificación) devuelto por AEAT
    /// </summary>
    public string? CSV { get; set; }

    /// <summary>
    /// Tiempo de espera en segundos antes del próximo envío
    /// </summary>
    public int? TiempoEsperaEnvio { get; set; }

    /// <summary>
    /// Indica si el envío fue completamente exitoso
    /// </summary>
    public bool EsExitoso { get; set; }

    /// <summary>
    /// Indica si hubo aceptación parcial (algunos correctos, otros rechazados)
    /// </summary>
    public bool TieneErroresParciales { get; set; }

    /// <summary>
    /// Total de registros procesados
    /// </summary>
    public int RegistrosProcesados { get; set; }

    /// <summary>
    /// Registros aceptados correctamente
    /// </summary>
    public int RegistrosCorrectos { get; set; }

    /// <summary>
    /// Registros aceptados pero con errores admisibles
    /// </summary>
    public int RegistrosAceptadosConErrores { get; set; }

    /// <summary>
    /// Registros rechazados
    /// </summary>
    public int RegistrosRechazados { get; set; }

    /// <summary>
    /// Registros detectados como duplicados
    /// </summary>
    public int RegistrosDuplicados { get; set; }

    /// <summary>
    /// Lista de errores no admisibles (provocan rechazo)
    /// </summary>
    public List<DetalleError> ErroresNoAdmisibles { get; set; } = new();

    /// <summary>
    /// Lista de errores admisibles (no provocan rechazo)
    /// </summary>
    public List<DetalleError> ErroresAdmisibles { get; set; } = new();

    /// <summary>
    /// Diccionario con conteo de errores por tipo
    /// </summary>
    public Dictionary<ErrorCatalog.TipoError, int> ErroresPorTipo { get; set; } = new();

    /// <summary>
    /// Indica si hay errores recuperables mediante reintento
    /// </summary>
    public bool TieneErroresRecuperables { get; set; }

    /// <summary>
    /// Indica si se requiere subsanación
    /// </summary>
    public bool RequiereSubsanacion { get; set; }

    /// <summary>
    /// Información adicional sobre el procesamiento
    /// </summary>
    public List<string> InformacionAdicional { get; set; } = new();
}

/// <summary>
/// Detalle de un error individual
/// </summary>
public class DetalleError
{
    /// <summary>
    /// Código del error AEAT
    /// </summary>
    public string CodigoError { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del error
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de error según clasificación
    /// </summary>
    public ErrorCatalog.TipoError TipoError { get; set; }

    /// <summary>
    /// Indica si el error es admisible
    /// </summary>
    public bool EsAdmisible { get; set; }

    /// <summary>
    /// Identificación de la factura afectada
    /// </summary>
    public string FacturaAfectada { get; set; } = string.Empty;

    /// <summary>
    /// Acción recomendada para resolver el error
    /// </summary>
    public string AccionRecomendada { get; set; } = string.Empty;
}
