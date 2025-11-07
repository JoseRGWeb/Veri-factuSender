namespace Verifactu.Client.Models;

/// <summary>
/// Tipo de factura rectificativa según especificación VERI*FACTU de AEAT
/// </summary>
public enum TipoRectificativa
{
    /// <summary>
    /// S - Por sustitución
    /// </summary>
    S,
    
    /// <summary>
    /// I - Por diferencias
    /// </summary>
    I
}
