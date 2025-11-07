using System.Collections.Generic;
using System.Linq;

namespace Verifactu.Client.Services;

/// <summary>
/// Catálogo de códigos de error AEAT con clasificación y tratamiento.
/// Basado en el documento oficial de validaciones VERI*FACTU.
/// </summary>
/// <remarks>
/// Referencias:
/// - docs/Veri-Factu_Descripcion_SWeb.md (sección "Validaciones y errores")
/// - docs/Verifactu-Guia-Tecnica.md (sección 4.4)
/// </remarks>
public static class ErrorCatalog
{
    /// <summary>
    /// Tipos de errores según su clasificación AEAT
    /// </summary>
    public enum TipoError
    {
        /// <summary>
        /// Errores de validación sintáctica del XML (estructura, formato, tipos de datos)
        /// </summary>
        Sintactico,
        
        /// <summary>
        /// Errores de validación de reglas de negocio
        /// </summary>
        Negocio,
        
        /// <summary>
        /// Errores de configuración o autenticación
        /// </summary>
        Configuracion,
        
        /// <summary>
        /// Errores temporales del servicio AEAT
        /// </summary>
        Temporal,
        
        /// <summary>
        /// Error desconocido o no catalogado
        /// </summary>
        Desconocido
    }

    /// <summary>
    /// Categorías de recuperabilidad de errores
    /// </summary>
    public enum CategoriaRecuperabilidad
    {
        /// <summary>
        /// Error recuperable mediante reintento automático (temporal, timeout, etc.)
        /// </summary>
        Recuperable,
        
        /// <summary>
        /// Error que requiere corrección de datos antes de reenviar
        /// </summary>
        RequiereCorreccion,
        
        /// <summary>
        /// Error que requiere subsanación (envío con indicador de subsanación)
        /// </summary>
        RequiereSubsanacion,
        
        /// <summary>
        /// Error irrecuperable que requiere intervención manual
        /// </summary>
        NoRecuperable
    }

    /// <summary>
    /// Información de un código de error AEAT
    /// </summary>
    public class InfoError
    {
        /// <summary>
        /// Código del error (ej: "4001", "5001")
        /// </summary>
        public string Codigo { get; init; } = string.Empty;
        
        /// <summary>
        /// Descripción general del error
        /// </summary>
        public string Descripcion { get; init; } = string.Empty;
        
        /// <summary>
        /// Tipo de error según clasificación AEAT
        /// </summary>
        public TipoError Tipo { get; init; }
        
        /// <summary>
        /// Categoría de recuperabilidad
        /// </summary>
        public CategoriaRecuperabilidad Categoria { get; init; }
        
        /// <summary>
        /// Indica si el error es admisible (no provoca rechazo del registro)
        /// </summary>
        public bool EsAdmisible { get; init; }
        
        /// <summary>
        /// Acción recomendada para resolver el error
        /// </summary>
        public string AccionRecomendada { get; init; } = string.Empty;
    }

    /// <summary>
    /// Catálogo completo de errores AEAT.
    /// Nota: Esta es una muestra representativa. El documento oficial contiene >900 validaciones.
    /// Se deben agregar todos los códigos según el documento de validaciones oficial.
    /// </summary>
    private static readonly Dictionary<string, InfoError> _errores = new()
    {
        // Errores de autenticación y certificados (1xxx)
        ["1001"] = new InfoError
        {
            Codigo = "1001",
            Descripcion = "Certificado no válido o caducado",
            Tipo = TipoError.Configuracion,
            Categoria = CategoriaRecuperabilidad.NoRecuperable,
            EsAdmisible = false,
            AccionRecomendada = "Verificar certificado digital y renovar si está caducado"
        },
        ["1002"] = new InfoError
        {
            Codigo = "1002",
            Descripcion = "NIF del certificado no coincide con NIF emisor",
            Tipo = TipoError.Configuracion,
            Categoria = CategoriaRecuperabilidad.NoRecuperable,
            EsAdmisible = false,
            AccionRecomendada = "Verificar que el certificado corresponde al NIF emisor declarado"
        },

        // Errores de validación sintáctica (2xxx)
        ["2001"] = new InfoError
        {
            Codigo = "2001",
            Descripcion = "XML no conforme al esquema XSD",
            Tipo = TipoError.Sintactico,
            Categoria = CategoriaRecuperabilidad.RequiereCorreccion,
            EsAdmisible = false,
            AccionRecomendada = "Verificar estructura XML contra esquema XSD oficial"
        },
        ["2002"] = new InfoError
        {
            Codigo = "2002",
            Descripcion = "Campo obligatorio no presente",
            Tipo = TipoError.Sintactico,
            Categoria = CategoriaRecuperabilidad.RequiereCorreccion,
            EsAdmisible = false,
            AccionRecomendada = "Añadir campo obligatorio faltante"
        },
        ["2003"] = new InfoError
        {
            Codigo = "2003",
            Descripcion = "Formato de campo incorrecto",
            Tipo = TipoError.Sintactico,
            Categoria = CategoriaRecuperabilidad.RequiereCorreccion,
            EsAdmisible = false,
            AccionRecomendada = "Verificar formato del campo según especificación"
        },

        // Errores de validación de negocio - NIF (4xxx)
        ["4001"] = new InfoError
        {
            Codigo = "4001",
            Descripcion = "El NIF del emisor no está identificado en la base de datos de la AEAT",
            Tipo = TipoError.Negocio,
            Categoria = CategoriaRecuperabilidad.NoRecuperable,
            EsAdmisible = false,
            AccionRecomendada = "Verificar NIF del emisor en la base de datos de la AEAT"
        },
        ["4002"] = new InfoError
        {
            Codigo = "4002",
            Descripcion = "NIF no válido o formato incorrecto",
            Tipo = TipoError.Negocio,
            Categoria = CategoriaRecuperabilidad.RequiereCorreccion,
            EsAdmisible = false,
            AccionRecomendada = "Corregir formato del NIF según normativa española"
        },

        // Errores de huella y encadenado (5xxx)
        ["5001"] = new InfoError
        {
            Codigo = "5001",
            Descripcion = "Huella calculada incorrecta",
            Tipo = TipoError.Negocio,
            Categoria = CategoriaRecuperabilidad.RequiereCorreccion,
            EsAdmisible = false,
            AccionRecomendada = "Recalcular huella SHA-256 según algoritmo oficial"
        },
        ["5002"] = new InfoError
        {
            Codigo = "5002",
            Descripcion = "Encadenamiento incorrecto - huella anterior no coincide",
            Tipo = TipoError.Negocio,
            Categoria = CategoriaRecuperabilidad.RequiereCorreccion,
            EsAdmisible = false,
            AccionRecomendada = "Verificar encadenamiento con registro anterior"
        },

        // Errores de duplicados (6xxx)
        ["6001"] = new InfoError
        {
            Codigo = "6001",
            Descripcion = "Registro duplicado - factura ya registrada",
            Tipo = TipoError.Negocio,
            Categoria = CategoriaRecuperabilidad.RequiereSubsanacion,
            EsAdmisible = false,
            AccionRecomendada = "Verificar si el registro ya fue enviado. Si requiere corrección, enviar como subsanación"
        },

        // Errores de validación de importes y datos fiscales (7xxx)
        ["7001"] = new InfoError
        {
            Codigo = "7001",
            Descripcion = "Importe total no coincide con suma de líneas",
            Tipo = TipoError.Negocio,
            Categoria = CategoriaRecuperabilidad.RequiereCorreccion,
            EsAdmisible = false,
            AccionRecomendada = "Recalcular totales de factura"
        },
        ["7002"] = new InfoError
        {
            Codigo = "7002",
            Descripcion = "Base imponible incorrecta",
            Tipo = TipoError.Negocio,
            Categoria = CategoriaRecuperabilidad.RequiereCorreccion,
            EsAdmisible = false,
            AccionRecomendada = "Verificar cálculo de base imponible"
        },
        ["7003"] = new InfoError
        {
            Codigo = "7003",
            Descripcion = "Tipo impositivo no válido para el tipo de operación",
            Tipo = TipoError.Negocio,
            Categoria = CategoriaRecuperabilidad.RequiereCorreccion,
            EsAdmisible = false,
            AccionRecomendada = "Verificar tipo de IVA aplicable según normativa"
        },

        // Errores admisibles - advertencias (8xxx)
        ["8001"] = new InfoError
        {
            Codigo = "8001",
            Descripcion = "Campo opcional con formato no recomendado",
            Tipo = TipoError.Negocio,
            Categoria = CategoriaRecuperabilidad.RequiereSubsanacion,
            EsAdmisible = true,
            AccionRecomendada = "Ajustar formato del campo opcional según recomendaciones"
        },
        ["8002"] = new InfoError
        {
            Codigo = "8002",
            Descripcion = "Información complementaria incompleta",
            Tipo = TipoError.Negocio,
            Categoria = CategoriaRecuperabilidad.RequiereSubsanacion,
            EsAdmisible = true,
            AccionRecomendada = "Completar información complementaria en subsanación"
        },

        // Errores temporales del servicio (9xxx)
        ["9001"] = new InfoError
        {
            Codigo = "9001",
            Descripcion = "Servicio temporalmente no disponible",
            Tipo = TipoError.Temporal,
            Categoria = CategoriaRecuperabilidad.Recuperable,
            EsAdmisible = false,
            AccionRecomendada = "Reintentar envío después del tiempo de espera indicado"
        },
        ["9002"] = new InfoError
        {
            Codigo = "9002",
            Descripcion = "Timeout procesando petición",
            Tipo = TipoError.Temporal,
            Categoria = CategoriaRecuperabilidad.Recuperable,
            EsAdmisible = false,
            AccionRecomendada = "Reintentar envío con backoff exponencial"
        },
        ["9003"] = new InfoError
        {
            Codigo = "9003",
            Descripcion = "Servidor en mantenimiento",
            Tipo = TipoError.Temporal,
            Categoria = CategoriaRecuperabilidad.Recuperable,
            EsAdmisible = false,
            AccionRecomendada = "Esperar fin de ventana de mantenimiento y reintentar"
        }
    };

    /// <summary>
    /// Obtiene información sobre un código de error específico
    /// </summary>
    /// <param name="codigoError">Código del error (ej: "4001")</param>
    /// <returns>Información del error, o información genérica si no está catalogado</returns>
    public static InfoError ObtenerInfoError(string? codigoError)
    {
        if (string.IsNullOrWhiteSpace(codigoError))
        {
            return new InfoError
            {
                Codigo = "0000",
                Descripcion = "Error sin código especificado",
                Tipo = TipoError.Desconocido,
                Categoria = CategoriaRecuperabilidad.NoRecuperable,
                EsAdmisible = false,
                AccionRecomendada = "Revisar respuesta AEAT completa para más detalles"
            };
        }

        if (_errores.TryGetValue(codigoError, out var info))
        {
            return info;
        }

        // Si el código no está catalogado, inferir tipo por prefijo
        var tipo = InferirTipoPorPrefijo(codigoError);
        return new InfoError
        {
            Codigo = codigoError,
            Descripcion = $"Error no catalogado: {codigoError}",
            Tipo = tipo,
            Categoria = CategoriaRecuperabilidad.RequiereCorreccion,
            EsAdmisible = false,
            AccionRecomendada = "Consultar documento oficial de validaciones AEAT"
        };
    }

    /// <summary>
    /// Determina si un código de error es recuperable mediante reintento automático
    /// </summary>
    /// <param name="codigoError">Código del error</param>
    /// <returns>True si el error es recuperable automáticamente</returns>
    public static bool EsErrorRecuperable(string? codigoError)
    {
        var info = ObtenerInfoError(codigoError);
        return info.Categoria == CategoriaRecuperabilidad.Recuperable;
    }

    /// <summary>
    /// Determina si un error es admisible (no provoca rechazo del registro)
    /// </summary>
    /// <param name="codigoError">Código del error</param>
    /// <returns>True si el error es admisible</returns>
    public static bool EsErrorAdmisible(string? codigoError)
    {
        var info = ObtenerInfoError(codigoError);
        return info.EsAdmisible;
    }

    /// <summary>
    /// Determina si un error requiere subsanación
    /// </summary>
    /// <param name="codigoError">Código del error</param>
    /// <returns>True si requiere subsanación</returns>
    public static bool RequiereSubsanacion(string? codigoError)
    {
        var info = ObtenerInfoError(codigoError);
        return info.Categoria == CategoriaRecuperabilidad.RequiereSubsanacion;
    }

    /// <summary>
    /// Obtiene todos los códigos de error catalogados
    /// </summary>
    /// <returns>Colección de todos los códigos de error</returns>
    public static IEnumerable<InfoError> ObtenerTodosLosErrores()
    {
        return _errores.Values.OrderBy(e => e.Codigo);
    }

    /// <summary>
    /// Obtiene errores por tipo
    /// </summary>
    /// <param name="tipo">Tipo de error a buscar</param>
    /// <returns>Errores del tipo especificado</returns>
    public static IEnumerable<InfoError> ObtenerErroresPorTipo(TipoError tipo)
    {
        return _errores.Values.Where(e => e.Tipo == tipo).OrderBy(e => e.Codigo);
    }

    /// <summary>
    /// Infiere el tipo de error basándose en el prefijo del código
    /// </summary>
    private static TipoError InferirTipoPorPrefijo(string codigoError)
    {
        if (string.IsNullOrWhiteSpace(codigoError) || codigoError.Length < 2)
            return TipoError.Desconocido;

        var prefijo = codigoError.Substring(0, 1);
        return prefijo switch
        {
            "1" => TipoError.Configuracion,
            "2" => TipoError.Sintactico,
            "4" or "5" or "6" or "7" => TipoError.Negocio,
            "8" => TipoError.Negocio, // Admisibles
            "9" => TipoError.Temporal,
            _ => TipoError.Desconocido
        };
    }
}
