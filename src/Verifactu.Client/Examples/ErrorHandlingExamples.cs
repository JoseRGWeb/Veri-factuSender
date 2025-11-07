using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Collections.Generic;
using Verifactu.Client.Services;
using Verifactu.Client.Models;

namespace Verifactu.Client.Examples;

/// <summary>
/// Ejemplos de uso del sistema de gestión de errores y reintentos AEAT
/// </summary>
public class ErrorHandlingExamples
{
    /// <summary>
    /// Ejemplo 1: Uso básico de ErrorCatalog para consultar información de errores
    /// </summary>
    public static void EjemploConsultarCatalogoErrores()
    {
        Console.WriteLine("=== Ejemplo 1: Consultar Catálogo de Errores ===\n");

        // Obtener información de un error específico
        var infoError = ErrorCatalog.ObtenerInfoError("4001");
        
        Console.WriteLine($"Código: {infoError.Codigo}");
        Console.WriteLine($"Descripción: {infoError.Descripcion}");
        Console.WriteLine($"Tipo: {infoError.Tipo}");
        Console.WriteLine($"Es admisible: {infoError.EsAdmisible}");
        Console.WriteLine($"Acción recomendada: {infoError.AccionRecomendada}");
        
        Console.WriteLine("\n--- Verificar si un error es recuperable ---");
        Console.WriteLine($"Error 9001 es recuperable: {ErrorCatalog.EsErrorRecuperable("9001")}");
        Console.WriteLine($"Error 4001 es recuperable: {ErrorCatalog.EsErrorRecuperable("4001")}");
        
        Console.WriteLine("\n--- Listar errores por tipo ---");
        var erroresTemporales = ErrorCatalog.ObtenerErroresPorTipo(ErrorCatalog.TipoError.Temporal);
        Console.WriteLine($"Errores temporales catalogados: {string.Join(", ", erroresTemporales.Select(e => e.Codigo))}");
    }

    /// <summary>
    /// Ejemplo 2: Análisis de respuesta AEAT con ErrorHandler
    /// </summary>
    public static void EjemploAnalisisRespuestaAEAT()
    {
        Console.WriteLine("\n=== Ejemplo 2: Análisis de Respuesta AEAT ===\n");

        var errorHandler = new ErrorHandler();

        // Simular respuesta con error no admisible
        var respuestaConError = new RespuestaSuministro
        {
            EstadoEnvio = "Incorrecto",
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea
                {
                    EstadoRegistro = "Incorrecto",
                    CodigoErrorRegistro = "4001",
                    DescripcionErrorRegistro = "El NIF del emisor no está identificado",
                    IDFactura = new IDFactura
                    {
                        NumSerieFactura = "FAC-2024-001",
                        FechaExpedicionFactura = new DateTime(2024, 11, 7)
                    }
                }
            }
        };

        var analisis = errorHandler.AnalizarRespuesta(respuestaConError);

        Console.WriteLine($"Estado del envío: {analisis.EstadoEnvio}");
        Console.WriteLine($"¿Es exitoso?: {analisis.EsExitoso}");
        Console.WriteLine($"Registros procesados: {analisis.RegistrosProcesados}");
        Console.WriteLine($"Registros rechazados: {analisis.RegistrosRechazados}");
        Console.WriteLine($"\nErrores no admisibles:");
        
        foreach (var error in analisis.ErroresNoAdmisibles)
        {
            Console.WriteLine($"  - Código {error.CodigoError}: {error.Descripcion}");
            Console.WriteLine($"    Factura afectada: {error.FacturaAfectada}");
            Console.WriteLine($"    Acción recomendada: {error.AccionRecomendada}");
        }

        Console.WriteLine($"\n¿Se debe reintentar?: {errorHandler.DebeReintentarse(respuestaConError)}");
    }

    /// <summary>
    /// Ejemplo 3: Análisis de respuesta parcialmente correcta
    /// </summary>
    public static void EjemploRespuestaParcial()
    {
        Console.WriteLine("\n=== Ejemplo 3: Respuesta Parcialmente Correcta ===\n");

        var errorHandler = new ErrorHandler();

        var respuestaParcial = new RespuestaSuministro
        {
            EstadoEnvio = "ParcialmenteCorrecto",
            CSV = "ABC123456789",
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
                    DescripcionErrorRegistro = "Huella calculada incorrecta",
                    IDFactura = new IDFactura { NumSerieFactura = "FAC-002" }
                },
                new RespuestaLinea
                {
                    EstadoRegistro = "AceptadoConErrores",
                    CodigoErrorRegistro = "8001",
                    DescripcionErrorRegistro = "Campo opcional con formato no recomendado",
                    IDFactura = new IDFactura { NumSerieFactura = "FAC-003" }
                }
            }
        };

        var analisis = errorHandler.AnalizarRespuesta(respuestaParcial);

        Console.WriteLine($"Estado: {analisis.EstadoEnvio}");
        Console.WriteLine($"CSV: {analisis.CSV}");
        Console.WriteLine($"Registros correctos: {analisis.RegistrosCorrectos}");
        Console.WriteLine($"Registros rechazados: {analisis.RegistrosRechazados}");
        Console.WriteLine($"Registros aceptados con errores: {analisis.RegistrosAceptadosConErrores}");
        Console.WriteLine($"Requiere subsanación: {analisis.RequiereSubsanacion}");

        if (analisis.ErroresNoAdmisibles.Any())
        {
            Console.WriteLine("\nErrores que requieren corrección:");
            foreach (var error in analisis.ErroresNoAdmisibles)
            {
                Console.WriteLine($"  - {error.CodigoError} en {error.FacturaAfectada}: {error.Descripcion}");
            }
        }

        if (analisis.ErroresAdmisibles.Any())
        {
            Console.WriteLine("\nErrores admisibles (registro aceptado):");
            foreach (var error in analisis.ErroresAdmisibles)
            {
                Console.WriteLine($"  - {error.CodigoError} en {error.FacturaAfectada}: {error.Descripcion}");
            }
        }
    }

    /// <summary>
    /// Ejemplo 4: Uso del servicio de reintentos (simulado)
    /// </summary>
    public static async Task EjemploReintentosAsync()
    {
        Console.WriteLine("\n=== Ejemplo 4: Servicio de Reintentos ===\n");

        // Nota: Este ejemplo muestra cómo configurar y usar el servicio de reintentos.
        // En producción, necesitarías instancias reales de IVerifactuSoapClient y certificados.

        Console.WriteLine("Configuración de opciones de reintento:");
        
        // Opción 1: Valores por defecto
        var opcionesPorDefecto = OpcionesReintento.PorDefecto;
        Console.WriteLine($"\nPor defecto:");
        Console.WriteLine($"  - Máximo intentos: {opcionesPorDefecto.MaximoIntentos}");
        Console.WriteLine($"  - Tiempo base: {opcionesPorDefecto.TiempoBaseSegundos}s");
        Console.WriteLine($"  - Tiempo máximo espera: {opcionesPorDefecto.TiempoMaximoEsperaSegundos}s");

        // Opción 2: Configuración para producción
        var opcionesProduccion = OpcionesReintento.Produccion;
        Console.WriteLine($"\nProducción:");
        Console.WriteLine($"  - Máximo intentos: {opcionesProduccion.MaximoIntentos}");
        Console.WriteLine($"  - Tiempo base: {opcionesProduccion.TiempoBaseSegundos}s");
        Console.WriteLine($"  - Tiempo máximo espera: {opcionesProduccion.TiempoMaximoEsperaSegundos}s");

        // Opción 3: Configuración personalizada
        var opcionesPersonalizadas = new OpcionesReintento
        {
            MaximoIntentos = 4,
            TiempoBaseSegundos = 3,
            TiempoMaximoEsperaSegundos = 180
        };
        Console.WriteLine($"\nPersonalizada:");
        Console.WriteLine($"  - Máximo intentos: {opcionesPersonalizadas.MaximoIntentos}");
        Console.WriteLine($"  - Tiempo base: {opcionesPersonalizadas.TiempoBaseSegundos}s");
        Console.WriteLine($"  - Tiempo máximo espera: {opcionesPersonalizadas.TiempoMaximoEsperaSegundos}s");

        Console.WriteLine("\n--- Tiempos de backoff exponencial ---");
        Console.WriteLine("Con configuración por defecto (base 2s):");
        for (int i = 0; i < 5; i++)
        {
            var tiempo = opcionesPorDefecto.TiempoBaseSegundos * Math.Pow(2, i);
            tiempo = Math.Min(tiempo, opcionesPorDefecto.TiempoMaximoEsperaSegundos);
            Console.WriteLine($"  Intento {i + 1}: espera ~{tiempo:F0}s antes del siguiente");
        }

        await Task.CompletedTask; // Para que el método sea async

        /* Ejemplo de uso real (comentado - requiere configuración real):
        
        var soapClient = new VerifactuSoapClient(endpointUrl, soapAction);
        var errorHandler = new ErrorHandler();
        var reintentosService = new ReintentosService(soapClient, errorHandler);

        var xmlDoc = new XmlDocument();
        // ... cargar y firmar XML ...
        
        var certificado = CertificateLoader.CargarDesdePfx("ruta/cert.pfx", "password");

        try
        {
            var resultado = await reintentosService.EnviarConReintentosAsync(
                xmlDoc,
                certificado,
                OpcionesReintento.Produccion
            );

            if (resultado.Exitoso)
            {
                Console.WriteLine($"Envío exitoso en intento {resultado.ExitosoEnIntento}");
                Console.WriteLine($"CSV: {resultado.CSV}");
                Console.WriteLine($"Duración total: {resultado.DuracionTotal}");
            }
            else
            {
                Console.WriteLine($"Envío fallido después de {resultado.NumeroIntentos} intentos");
                Console.WriteLine($"Motivo: {resultado.MotivoFallo}");
                
                if (resultado.AnalisisErrores != null)
                {
                    foreach (var error in resultado.AnalisisErrores.ErroresNoAdmisibles)
                    {
                        Console.WriteLine($"Error: {error.CodigoError} - {error.Descripcion}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado: {ex.Message}");
        }
        */
    }

    /// <summary>
    /// Ejemplo 5: Manejo de errores de duplicado
    /// </summary>
    public static void EjemploDuplicados()
    {
        Console.WriteLine("\n=== Ejemplo 5: Manejo de Duplicados ===\n");

        var errorHandler = new ErrorHandler();

        var respuestaConDuplicado = new RespuestaSuministro
        {
            EstadoEnvio = "Incorrecto",
            RespuestasLinea = new List<RespuestaLinea>
            {
                new RespuestaLinea
                {
                    EstadoRegistro = "Incorrecto",
                    CodigoErrorRegistro = "6001",
                    DescripcionErrorRegistro = "Registro duplicado - factura ya registrada",
                    IDFactura = new IDFactura
                    {
                        NumSerieFactura = "FAC-2024-100",
                        FechaExpedicionFactura = new DateTime(2024, 11, 7)
                    },
                    RegistroDuplicado = new RegistroDuplicado
                    {
                        IdPeticionRegistroDuplicado = "REG-PREV-12345",
                        EstadoRegistroDuplicado = "Correcta",
                        CodigoErrorRegistro = null,
                        DescripcionErrorRegistro = null
                    }
                }
            }
        };

        var analisis = errorHandler.AnalizarRespuesta(respuestaConDuplicado);

        Console.WriteLine($"Registros duplicados detectados: {analisis.RegistrosDuplicados}");
        Console.WriteLine("\nInformación adicional:");
        foreach (var info in analisis.InformacionAdicional)
        {
            Console.WriteLine($"  - {info}");
        }

        var errorDuplicado = analisis.ErroresNoAdmisibles.FirstOrDefault();
        if (errorDuplicado != null)
        {
            Console.WriteLine($"\nCódigo error: {errorDuplicado.CodigoError}");
            Console.WriteLine($"Requiere subsanación: {ErrorCatalog.RequiereSubsanacion(errorDuplicado.CodigoError)}");
            Console.WriteLine($"Acción recomendada: {errorDuplicado.AccionRecomendada}");
        }
    }

    /// <summary>
    /// Ejecuta todos los ejemplos
    /// </summary>
    public static async Task EjecutarTodosLosEjemplosAsync()
    {
        EjemploConsultarCatalogoErrores();
        EjemploAnalisisRespuestaAEAT();
        EjemploRespuestaParcial();
        await EjemploReintentosAsync();
        EjemploDuplicados();

        Console.WriteLine("\n=== Fin de Ejemplos ===");
    }
}
