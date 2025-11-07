using System;
using System.Collections.Generic;

namespace Verifactu.Client.Models;

/// <summary>
/// Respuesta del servicio SOAP de AEAT para operaciones de RegFacturacionAlta
/// Corresponde al esquema RespuestaSuministro.xsd
/// </summary>
public class RespuestaSuministro
{
    /// <summary>
    /// Código seguro de verificación (CSV) asociado a la remisión enviada.
    /// IMPORTANTE: El CSV debe ser almacenado por el SIF en el momento de alta,
    /// no podrá ser recuperado a través de consultas posteriores.
    /// </summary>
    public string? CSV { get; set; }

    /// <summary>
    /// Datos de presentación (NIF presentador, timestamp, etc.)
    /// </summary>
    public DatosPresentacion? DatosPresentacion { get; set; }

    /// <summary>
    /// Cabecera equivalente a la enviada en la remisión
    /// </summary>
    public CabeceraRespuesta? Cabecera { get; set; }

    /// <summary>
    /// Segundos de espera entre envíos (mecanismo de control de flujo).
    /// Para poder realizar el siguiente envío, el sistema informático deberá esperar
    /// a que transcurran este número de segundos desde el anterior envío.
    /// </summary>
    public int? TiempoEsperaEnvio { get; set; }

    /// <summary>
    /// Estado global del envío.
    /// Valores posibles: "Correcto", "ParcialmenteCorrecto", "Incorrecto"
    /// </summary>
    public string? EstadoEnvio { get; set; }

    /// <summary>
    /// Lista de respuestas individuales por cada registro de facturación enviado
    /// </summary>
    public List<RespuestaLinea>? RespuestasLinea { get; set; }
}

/// <summary>
/// Datos de presentación de la remisión
/// </summary>
public class DatosPresentacion
{
    /// <summary>
    /// NIF del presentador
    /// </summary>
    public string? NIFPresentador { get; set; }

    /// <summary>
    /// Timestamp de presentación en huso horario de los servidores de la AEAT.
    /// Formato: YYYY-MM-DDThh:mm:ssTZD (ISO 8601)
    /// Ejemplo: 2024-01-01T19:20:30+01:00
    /// </summary>
    public DateTime? TimestampPresentacion { get; set; }
}

/// <summary>
/// Cabecera de la respuesta
/// </summary>
public class CabeceraRespuesta
{
    /// <summary>
    /// Obligado a emisión (NIF y nombre/razón)
    /// </summary>
    public ObligadoEmision? ObligadoEmision { get; set; }
}

/// <summary>
/// Obligado a emisión de facturas
/// </summary>
public class ObligadoEmision
{
    public string? NombreRazon { get; set; }
    public string? NIF { get; set; }
}

/// <summary>
/// Respuesta individual para cada registro de facturación enviado
/// </summary>
public class RespuestaLinea
{
    /// <summary>
    /// Identificador del registro de facturación
    /// </summary>
    public IDFactura? IDFactura { get; set; }

    /// <summary>
    /// Información de la operación realizada
    /// </summary>
    public Operacion? Operacion { get; set; }

    /// <summary>
    /// Dato adicional de contenido libre (RefExterna)
    /// </summary>
    public string? RefExterna { get; set; }

    /// <summary>
    /// Estado del registro individual.
    /// Valores: "Correcto", "AceptadoConErrores", "Incorrecto"
    /// </summary>
    public string? EstadoRegistro { get; set; }

    /// <summary>
    /// Código de error (si aplica)
    /// </summary>
    public string? CodigoErrorRegistro { get; set; }

    /// <summary>
    /// Descripción del error (si aplica)
    /// </summary>
    public string? DescripcionErrorRegistro { get; set; }

    /// <summary>
    /// Información sobre registro duplicado (si aplica)
    /// </summary>
    public RegistroDuplicado? RegistroDuplicado { get; set; }
}

/// <summary>
/// Identificación de factura
/// </summary>
public class IDFactura
{
    public string? IDEmisorFactura { get; set; }
    public string? NumSerieFactura { get; set; }
    public DateTime? FechaExpedicionFactura { get; set; }
}

/// <summary>
/// Información de la operación realizada
/// </summary>
public class Operacion
{
    /// <summary>
    /// Tipo de operación: "Alta" o "Anulacion"
    /// </summary>
    public string? TipoOperacion { get; set; }

    /// <summary>
    /// Indicador de subsanación ("S" o null)
    /// </summary>
    public string? Subsanacion { get; set; }

    /// <summary>
    /// Indicador de rechazo previo ("S" o null)
    /// </summary>
    public string? RechazoPrevio { get; set; }

    /// <summary>
    /// Indicador de sin registro previo ("S" o null)
    /// </summary>
    public string? SinRegistroPrevio { get; set; }
}

/// <summary>
/// Información sobre registro duplicado
/// </summary>
public class RegistroDuplicado
{
    /// <summary>
    /// IdPeticion del registro almacenado previamente
    /// </summary>
    public string? IdPeticionRegistroDuplicado { get; set; }

    /// <summary>
    /// Estado del registro duplicado: "Correcta", "AceptadaConErrores", "Anulada"
    /// </summary>
    public string? EstadoRegistroDuplicado { get; set; }

    /// <summary>
    /// Código de error del registro duplicado
    /// </summary>
    public string? CodigoErrorRegistro { get; set; }

    /// <summary>
    /// Descripción del error del registro duplicado
    /// </summary>
    public string? DescripcionErrorRegistro { get; set; }
}

/// <summary>
/// Respuesta del servicio de consulta de registros de facturación
/// Corresponde al esquema RespuestaConsultaLR.xsd
/// </summary>
public class RespuestaConsultaLR
{
    /// <summary>
    /// Cabecera de la consulta
    /// </summary>
    public CabeceraConsulta? Cabecera { get; set; }

    /// <summary>
    /// Periodo de imputación consultado
    /// </summary>
    public PeriodoImputacion? PeriodoImputacion { get; set; }

    /// <summary>
    /// Indicador de paginación ("S" o "N")
    /// Si es "S", hay más datos pendientes y se debe realizar nueva consulta paginada
    /// </summary>
    public string? IndicadorPaginacion { get; set; }

    /// <summary>
    /// Resultado de la consulta: "ConDatos" o "SinDatos"
    /// </summary>
    public string? ResultadoConsulta { get; set; }

    /// <summary>
    /// Lista de registros de facturación encontrados (máximo 10.000)
    /// </summary>
    public List<RegistroRespuestaConsulta>? RegistrosRespuesta { get; set; }

    /// <summary>
    /// Clave de paginación para obtener los siguientes registros
    /// Solo se informa si IndicadorPaginacion = "S"
    /// </summary>
    public ClavePaginacion? ClavePaginacion { get; set; }
}

/// <summary>
/// Cabecera de la respuesta de consulta
/// </summary>
public class CabeceraConsulta
{
    public string? IDVersion { get; set; }
    public ObligadoEmision? ObligadoEmision { get; set; }
    public Destinatario? Destinatario { get; set; }
    public string? IndicadorRepresentante { get; set; }
}

/// <summary>
/// Destinatario de factura
/// </summary>
public class Destinatario
{
    public string? NombreRazon { get; set; }
    public string? NIF { get; set; }
}

/// <summary>
/// Periodo de imputación
/// </summary>
public class PeriodoImputacion
{
    public int Ejercicio { get; set; }
    public string? Periodo { get; set; } // "01" a "12"
}

/// <summary>
/// Registro individual de respuesta de consulta
/// </summary>
public class RegistroRespuestaConsulta
{
    public IDFactura? IDFactura { get; set; }
    public DatosRegistroFacturacion? DatosRegistroFacturacion { get; set; }
    public DatosPresentacionConsulta? DatosPresentacion { get; set; }
    public EstadoRegistro? EstadoRegistro { get; set; }
}

/// <summary>
/// Datos del registro de facturación
/// </summary>
public class DatosRegistroFacturacion
{
    public string? TipoFactura { get; set; }
    public string? DescripcionOperacion { get; set; }
    public decimal? CuotaTotal { get; set; }
    public decimal? ImporteTotal { get; set; }
    public string? Huella { get; set; }
    public DateTime? FechaHoraHusoGenRegistro { get; set; }
}

/// <summary>
/// Datos de presentación en consulta
/// </summary>
public class DatosPresentacionConsulta
{
    public string? NIFPresentador { get; set; }
    public DateTime? TimestampPresentacion { get; set; }
    public string? IdPeticion { get; set; }
}

/// <summary>
/// Estado del registro consultado
/// </summary>
public class EstadoRegistro
{
    public DateTime? TimestampUltimaModificacion { get; set; }
    public string? EstadoRegistro_ { get; set; } // "Correcta", "AceptadaConErrores", "Anulada"
    public string? CodigoErrorRegistro { get; set; }
    public string? DescripcionErrorRegistro { get; set; }
}

/// <summary>
/// Clave de paginación para consultas paginadas
/// </summary>
public class ClavePaginacion
{
    public string? IDEmisorFactura { get; set; }
    public string? NumSerieFactura { get; set; }
    public DateTime? FechaExpedicionFactura { get; set; }
}
