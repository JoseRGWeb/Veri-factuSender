namespace Verifactu.Client.Models;

/// <summary>
/// Tipo de factura según especificación VERI*FACTU de AEAT
/// </summary>
public enum TipoFactura
{
    /// <summary>
    /// F1 - Factura completa (con desglose)
    /// </summary>
    F1,
    
    /// <summary>
    /// F2 - Factura simplificada (sin identificación del destinatario - Art. 6.1.d) RD 1619/2012)
    /// </summary>
    F2,
    
    /// <summary>
    /// F3 - Factura emitida en sustitución de facturas simplificadas facturadas y declaradas
    /// </summary>
    F3,
    
    /// <summary>
    /// F4 - Asiento resumen de facturas
    /// </summary>
    F4,
    
    /// <summary>
    /// R1 - Factura rectificativa por error fundado en derecho y Art. 80 Uno, Dos y Seis LIVA
    /// </summary>
    R1,
    
    /// <summary>
    /// R2 - Factura rectificativa (Art. 80.3)
    /// </summary>
    R2,
    
    /// <summary>
    /// R3 - Factura rectificativa (Art. 80.4)
    /// </summary>
    R3,
    
    /// <summary>
    /// R4 - Factura rectificativa (Resto)
    /// </summary>
    R4,
    
    /// <summary>
    /// R5 - Factura rectificativa en facturas simplificadas
    /// </summary>
    R5
}
