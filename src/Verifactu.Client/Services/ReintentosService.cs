using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Verifactu.Client.Models;

namespace Verifactu.Client.Services;

/// <summary>
/// Servicio para gestión de reintentos con backoff exponencial en envíos a AEAT.
/// Implementa lógica de reintentos inteligente basada en el tipo de error.
/// </summary>
public class ReintentosService : IReintentosService
{
    private readonly IVerifactuSoapClient _soapClient;
    private readonly IErrorHandler _errorHandler;
    private readonly ILogger<ReintentosService>? _logger;

    /// <summary>
    /// Constructor del servicio de reintentos
    /// </summary>
    /// <param name="soapClient">Cliente SOAP para envío a AEAT</param>
    /// <param name="errorHandler">Manejador de errores</param>
    /// <param name="logger">Logger opcional para registro de actividad</param>
    public ReintentosService(
        IVerifactuSoapClient soapClient, 
        IErrorHandler errorHandler,
        ILogger<ReintentosService>? logger = null)
    {
        _soapClient = soapClient ?? throw new ArgumentNullException(nameof(soapClient));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _logger = logger;
    }

    /// <summary>
    /// Envía un registro de facturación con reintentos automáticos en caso de errores recuperables.
    /// </summary>
    /// <param name="xmlFirmado">Documento XML firmado del registro</param>
    /// <param name="certificado">Certificado para autenticación mTLS</param>
    /// <param name="opciones">Opciones de configuración de reintentos (opcional)</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Resultado del envío con información de reintentos</returns>
    /// <exception cref="AggregateException">Si se alcanza el máximo de reintentos sin éxito</exception>
    public async Task<ResultadoEnvioConReintentos> EnviarConReintentosAsync(
        XmlDocument xmlFirmado,
        X509Certificate2 certificado,
        OpcionesReintento? opciones = null,
        CancellationToken ct = default)
    {
        opciones ??= OpcionesReintento.PorDefecto;
        
        var resultado = new ResultadoEnvioConReintentos
        {
            FechaInicio = DateTime.UtcNow
        };

        Exception? ultimaExcepcion = null;

        for (int intento = 0; intento < opciones.MaximoIntentos; intento++)
        {
            resultado.NumeroIntentos = intento + 1;

            try
            {
                _logger?.LogInformation(
                    "Intento {Intento}/{MaxIntentos} de envío a AEAT",
                    intento + 1,
                    opciones.MaximoIntentos);

                // Enviar registro
                var respuesta = await _soapClient.EnviarRegFacturacionAltaAsync(
                    xmlFirmado, 
                    certificado, 
                    ct);

                resultado.UltimaRespuesta = respuesta;

                // Analizar respuesta
                var analisis = _errorHandler.AnalizarRespuesta(respuesta);
                resultado.AnalisisErrores = analisis;

                // Si es exitoso o parcialmente correcto, retornar
                if (analisis.EsExitoso || !_errorHandler.DebeReintentarse(respuesta))
                {
                    // Si es realmente exitoso, marcarlo como tal
                    if (analisis.EsExitoso)
                    {
                        resultado.Exitoso = true;
                        resultado.ExitosoEnIntento = intento + 1;
                        
                        _logger?.LogInformation(
                            "Envío exitoso en intento {Intento}. CSV: {CSV}",
                            intento + 1,
                            respuesta.CSV);
                    }
                    else
                    {
                        // No es exitoso pero no se debe reintentar (errores no recuperables)
                        resultado.Exitoso = false;
                        resultado.ExitosoEnIntento = null;
                        resultado.MotivoFallo = "Errores no recuperables en la respuesta";
                        
                        _logger?.LogWarning(
                            "Errores no recuperables detectados. No se reintentará.");
                    }
                    
                    resultado.FechaFin = DateTime.UtcNow;
                    return resultado;
                }

                // Errores recuperables - preparar para reintento
                _logger?.LogWarning(
                    "Errores recuperables detectados en intento {Intento}. Se reintentará.",
                    intento + 1);

                // Esperar antes del siguiente intento (si no es el último)
                if (intento < opciones.MaximoIntentos - 1)
                {
                    // Calcular tiempo de espera
                    var tiempoEspera = _errorHandler.CalcularTiempoEspera(respuesta, intento);
                    resultado.TiemposEspera.Add(tiempoEspera);
                    
                    _logger?.LogInformation(
                        "Esperando {Segundos}s antes del siguiente intento",
                        tiempoEspera.TotalSeconds);

                    await Task.Delay(tiempoEspera, ct);
                }
            }
            catch (HttpRequestException ex) when (EsErrorRecuperable(ex))
            {
                ultimaExcepcion = ex;
                
                _logger?.LogWarning(ex,
                    "Error HTTP recuperable en intento {Intento}: {Mensaje}",
                    intento + 1,
                    ex.Message);

                // Esperar antes del siguiente intento (si no es el último)
                if (intento < opciones.MaximoIntentos - 1)
                {
                    // Calcular tiempo de espera con backoff exponencial
                    var tiempoEspera = CalcularBackoffExponencial(intento, opciones);
                    resultado.TiemposEspera.Add(tiempoEspera);
                    
                    _logger?.LogInformation(
                        "Esperando {Segundos}s antes del siguiente intento (backoff exponencial)",
                        tiempoEspera.TotalSeconds);

                    await Task.Delay(tiempoEspera, ct);
                }
            }
            catch (TimeoutException ex)
            {
                ultimaExcepcion = ex;
                
                _logger?.LogWarning(ex,
                    "Timeout en intento {Intento}",
                    intento + 1);

                // Esperar antes del siguiente intento (si no es el último)
                if (intento < opciones.MaximoIntentos - 1)
                {
                    var tiempoEspera = CalcularBackoffExponencial(intento, opciones);
                    resultado.TiemposEspera.Add(tiempoEspera);
                    await Task.Delay(tiempoEspera, ct);
                }
            }
            catch (Exception ex)
            {
                // Excepciones no recuperables
                _logger?.LogError(ex,
                    "Error no recuperable en intento {Intento}: {Tipo}",
                    intento + 1,
                    ex.GetType().Name);

                resultado.Exitoso = false;
                resultado.FechaFin = DateTime.UtcNow;
                resultado.MotivoFallo = $"Error no recuperable: {ex.Message}";
                resultado.ExcepcionFinal = ex;
                return resultado;
            }
        }

        // Se alcanzó el máximo de reintentos sin éxito
        resultado.Exitoso = false;
        resultado.FechaFin = DateTime.UtcNow;
        resultado.MotivoFallo = $"Máximo de {opciones.MaximoIntentos} intentos alcanzado";
        resultado.ExcepcionFinal = ultimaExcepcion;

        _logger?.LogError(
            "Máximo de reintentos ({MaxIntentos}) alcanzado sin éxito",
            opciones.MaximoIntentos);

        return resultado;
    }

    /// <summary>
    /// Determina si una excepción HTTP es recuperable mediante reintento
    /// </summary>
    private bool EsErrorRecuperable(HttpRequestException ex)
    {
        // Errores 5xx del servidor (temporales)
        // Timeouts de red
        // Errores de conexión
        var mensaje = ex.Message.ToLowerInvariant();
        
        return mensaje.Contains("timeout") ||
               mensaje.Contains("temporarily") ||
               mensaje.Contains("503") ||
               mensaje.Contains("502") ||
               mensaje.Contains("504") ||
               mensaje.Contains("connection") ||
               ex.InnerException is TimeoutException;
    }

    /// <summary>
    /// Calcula el tiempo de espera usando backoff exponencial
    /// </summary>
    private TimeSpan CalcularBackoffExponencial(int numeroIntento, OpcionesReintento opciones)
    {
        // Backoff exponencial: TiempoBase * 2^intento
        var segundos = opciones.TiempoBaseSegundos * Math.Pow(2, numeroIntento);
        
        // Aplicar jitter aleatorio (±25%) para evitar "thundering herd"
        var jitter = Random.Shared.NextDouble() * 0.5 - 0.25; // -25% a +25%
        segundos *= (1 + jitter);
        
        // Limitar al máximo configurado
        segundos = Math.Min(segundos, opciones.TiempoMaximoEsperaSegundos);
        
        return TimeSpan.FromSeconds(segundos);
    }
}

/// <summary>
/// Opciones de configuración para reintentos
/// </summary>
public class OpcionesReintento
{
    /// <summary>
    /// Número máximo de intentos de envío (incluye el intento inicial)
    /// Por defecto: 3 intentos
    /// </summary>
    public int MaximoIntentos { get; set; } = 3;

    /// <summary>
    /// Tiempo base en segundos para el cálculo de backoff exponencial
    /// Por defecto: 2 segundos
    /// </summary>
    public double TiempoBaseSegundos { get; set; } = 2;

    /// <summary>
    /// Tiempo máximo de espera en segundos entre reintentos
    /// Por defecto: 300 segundos (5 minutos)
    /// </summary>
    public double TiempoMaximoEsperaSegundos { get; set; } = 300;

    /// <summary>
    /// Opciones por defecto recomendadas
    /// </summary>
    public static OpcionesReintento PorDefecto => new()
    {
        MaximoIntentos = 3,
        TiempoBaseSegundos = 2,
        TiempoMaximoEsperaSegundos = 300
    };

    /// <summary>
    /// Opciones para entorno de producción (reintentos más conservadores)
    /// </summary>
    public static OpcionesReintento Produccion => new()
    {
        MaximoIntentos = 5,
        TiempoBaseSegundos = 5,
        TiempoMaximoEsperaSegundos = 600
    };

    /// <summary>
    /// Opciones para entorno de pruebas (reintentos más rápidos)
    /// </summary>
    public static OpcionesReintento Pruebas => new()
    {
        MaximoIntentos = 2,
        TiempoBaseSegundos = 1,
        TiempoMaximoEsperaSegundos = 60
    };
}

/// <summary>
/// Resultado de un envío con información de reintentos
/// </summary>
public class ResultadoEnvioConReintentos
{
    /// <summary>
    /// Indica si el envío fue exitoso
    /// </summary>
    public bool Exitoso { get; set; }

    /// <summary>
    /// Número total de intentos realizados
    /// </summary>
    public int NumeroIntentos { get; set; }

    /// <summary>
    /// Intento en el que se logró el éxito (1-based), o null si falló
    /// </summary>
    public int? ExitosoEnIntento { get; set; }

    /// <summary>
    /// Última respuesta recibida de AEAT
    /// </summary>
    public RespuestaSuministro? UltimaRespuesta { get; set; }

    /// <summary>
    /// Análisis de errores de la última respuesta
    /// </summary>
    public ResultadoAnalisisErrores? AnalisisErrores { get; set; }

    /// <summary>
    /// Tiempos de espera utilizados entre intentos
    /// </summary>
    public List<TimeSpan> TiemposEspera { get; set; } = new();

    /// <summary>
    /// Fecha y hora de inicio del proceso
    /// </summary>
    public DateTime FechaInicio { get; set; }

    /// <summary>
    /// Fecha y hora de finalización del proceso
    /// </summary>
    public DateTime FechaFin { get; set; }

    /// <summary>
    /// Duración total del proceso (incluyendo esperas)
    /// </summary>
    public TimeSpan DuracionTotal => FechaFin - FechaInicio;

    /// <summary>
    /// Motivo del fallo (si aplica)
    /// </summary>
    public string? MotivoFallo { get; set; }

    /// <summary>
    /// Última excepción capturada (si aplica)
    /// </summary>
    public Exception? ExcepcionFinal { get; set; }

    /// <summary>
    /// CSV obtenido de AEAT (si fue exitoso)
    /// </summary>
    public string? CSV => UltimaRespuesta?.CSV;
}
