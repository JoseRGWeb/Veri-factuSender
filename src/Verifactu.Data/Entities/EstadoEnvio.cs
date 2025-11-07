namespace Verifactu.Data.Entities;

/// <summary>
/// Estado del envío de un registro de facturación a AEAT
/// </summary>
public enum EstadoEnvio
{
    /// <summary>
    /// Registro pendiente de envío
    /// </summary>
    Pendiente = 0,

    /// <summary>
    /// Registro en proceso de envío
    /// </summary>
    EnviandoAhora = 1,

    /// <summary>
    /// Registro enviado correctamente y aceptado por AEAT
    /// </summary>
    Correcto = 2,

    /// <summary>
    /// Registro enviado pero rechazado por AEAT
    /// </summary>
    Rechazado = 3,

    /// <summary>
    /// Error temporal en el envío (requiere reintento)
    /// </summary>
    ErrorTemporal = 4,

    /// <summary>
    /// Error permanente (no reintentar)
    /// </summary>
    ErrorPermanente = 5,

    /// <summary>
    /// Registro aceptado con errores por AEAT
    /// </summary>
    AceptadoConErrores = 6
}
