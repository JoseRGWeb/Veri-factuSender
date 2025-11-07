using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Verifactu.Data.Entities;

/// <summary>
/// Entidad que representa un registro de facturación persistido en base de datos
/// Incluye campos para auditoría y trazabilidad de envíos a AEAT
/// </summary>
[Table("RegistrosFacturacion")]
[Index(nameof(Serie), nameof(Numero), IsUnique = true, Name = "IX_RegistrosFacturacion_Serie_Numero")]
[Index(nameof(FechaHoraExpedicionUTC), Name = "IX_RegistrosFacturacion_FechaHoraExpedicionUTC")]
[Index(nameof(EstadoEnvio), Name = "IX_RegistrosFacturacion_EstadoEnvio")]
[Index(nameof(Huella), Name = "IX_RegistrosFacturacion_Huella")]
public class RegistroFacturacionEntity
{
    /// <summary>
    /// Identificador único del registro (clave primaria)
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Serie de la factura
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Serie { get; set; } = string.Empty;

    /// <summary>
    /// Número de la factura
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora de expedición de la factura en UTC
    /// </summary>
    [Required]
    public DateTime FechaHoraExpedicionUTC { get; set; }

    /// <summary>
    /// Huella SHA-256 del registro de facturación (64 caracteres hexadecimales)
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string Huella { get; set; } = string.Empty;

    /// <summary>
    /// Huella del registro anterior en la cadena (encadenamiento)
    /// Null si es el primer registro
    /// </summary>
    [MaxLength(64)]
    public string? HuellaAnterior { get; set; }

    /// <summary>
    /// Estado actual del envío a AEAT
    /// </summary>
    [Required]
    public EstadoEnvio EstadoEnvio { get; set; }

    /// <summary>
    /// Código de error devuelto por AEAT (si aplica)
    /// </summary>
    public int? CodigoErrorAEAT { get; set; }

    /// <summary>
    /// Descripción del error devuelto por AEAT (si aplica)
    /// </summary>
    [MaxLength(1000)]
    public string? DescripcionErrorAEAT { get; set; }

    /// <summary>
    /// XML firmado del registro de facturación
    /// </summary>
    [Required]
    public string XmlFirmado { get; set; } = string.Empty;

    /// <summary>
    /// Acuse de recibo de AEAT (respuesta XML completa)
    /// </summary>
    public string? AcuseRecibo { get; set; }

    /// <summary>
    /// CSV (Código Seguro de Verificación) devuelto por AEAT
    /// </summary>
    [MaxLength(100)]
    public string? CSV { get; set; }

    /// <summary>
    /// Fecha y hora del primer envío a AEAT
    /// </summary>
    public DateTime? FechaEnvio { get; set; }

    /// <summary>
    /// Fecha y hora del último envío a AEAT
    /// </summary>
    public DateTime? FechaUltimoEnvio { get; set; }

    /// <summary>
    /// Número de reintentos realizados
    /// </summary>
    [Required]
    public int Reintentos { get; set; }

    /// <summary>
    /// NIF del emisor de la factura
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string NifEmisor { get; set; } = string.Empty;

    /// <summary>
    /// Nombre o razón social del emisor
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string NombreEmisor { get; set; } = string.Empty;

    /// <summary>
    /// Importe total de la factura
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ImporteTotal { get; set; }

    /// <summary>
    /// Cuota total de impuestos
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CuotaTotal { get; set; }

    /// <summary>
    /// Fecha de creación del registro en la base de datos
    /// </summary>
    [Required]
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha de última modificación del registro
    /// </summary>
    [Required]
    public DateTime FechaModificacion { get; set; }

    /// <summary>
    /// Indica si el registro ha sido anulado
    /// </summary>
    public bool Anulado { get; set; }

    /// <summary>
    /// Fecha de anulación (si aplica)
    /// </summary>
    public DateTime? FechaAnulacion { get; set; }

    /// <summary>
    /// Referencia externa opcional
    /// </summary>
    [MaxLength(100)]
    public string? RefExterna { get; set; }
}
