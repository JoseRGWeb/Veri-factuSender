namespace Verifactu.Client.Models;

/// <summary>
/// Calificación de la operación según especificación VERI*FACTU de AEAT
/// </summary>
public enum CalificacionOperacion
{
    /// <summary>
    /// S1 - Operación sujeta y no exenta - sin inversión del sujeto pasivo
    /// </summary>
    S1,
    
    /// <summary>
    /// S2 - Operación sujeta y no exenta - con inversión del sujeto pasivo
    /// </summary>
    S2,
    
    /// <summary>
    /// S3 - Operación sujeta y exenta
    /// </summary>
    S3,
    
    /// <summary>
    /// N1 - Operación no sujeta por reglas de localización (excepto las incluidas en N2)
    /// </summary>
    N1,
    
    /// <summary>
    /// N2 - Operación no sujeta por otras causas
    /// </summary>
    N2
}
