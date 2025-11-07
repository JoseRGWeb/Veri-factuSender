namespace Verifactu.Client.Models;

/// <summary>
/// Clave de régimen especial o trascendencia según especificación VERI*FACTU de AEAT
/// </summary>
public enum ClaveRegimenEspecialOTrascendencia
{
    /// <summary>
    /// 01 - Régimen general
    /// </summary>
    RegimenGeneral01,
    
    /// <summary>
    /// 02 - Exportación
    /// </summary>
    Exportacion02,
    
    /// <summary>
    /// 03 - Operaciones a las que se aplique el régimen especial de bienes usados, objetos de arte, antigüedades y objetos de colección
    /// </summary>
    BienesUsados03,
    
    /// <summary>
    /// 04 - Régimen especial del oro de inversión
    /// </summary>
    OroInversion04,
    
    /// <summary>
    /// 05 - Régimen especial de las agencias de viajes
    /// </summary>
    AgenciasViajes05,
    
    /// <summary>
    /// 06 - Régimen especial grupo de entidades en IVA (Nivel Avanzado)
    /// </summary>
    GrupoEntidades06,
    
    /// <summary>
    /// 07 - Régimen especial del criterio de caja
    /// </summary>
    CriterioCaja07,
    
    /// <summary>
    /// 08 - Operaciones sujetas al IPSI / IGIC (Impuesto sobre la Producción, los Servicios y la Importación / Impuesto General Indirecto Canario)
    /// </summary>
    IPSI_IGIC08,
    
    /// <summary>
    /// 09 - Facturación de las prestaciones de servicios de agencias de viaje que actúan como mediadoras en nombre y por cuenta ajena (D.A.4ª RD1619/2012)
    /// </summary>
    AgenciasViajesMediacion09,
    
    /// <summary>
    /// 10 - Cobros por cuenta de terceros de honorarios profesionales o de derechos derivados de la propiedad industrial, de autor u otros por cuenta de sus socios, asociados o colegiados efectuados por sociedades, asociaciones, colegios profesionales u otras entidades que realicen estas funciones de cobro
    /// </summary>
    CobrosTerceros10,
    
    /// <summary>
    /// 11 - Operaciones de arrendamiento de local de negocio sujetas a retención
    /// </summary>
    ArrendamientoLocal11,
    
    /// <summary>
    /// 12 - Operaciones de arrendamiento de local de negocio no sujetas a retención
    /// </summary>
    ArrendamientoLocalNoRetencion12,
    
    /// <summary>
    /// 13 - Operaciones de arrendamiento de local de negocio sujetas y no sujetas a retención
    /// </summary>
    ArrendamientoLocalMixto13,
    
    /// <summary>
    /// 14 - Factura con IVA pendiente de devengo en certificaciones de obra cuyo destinatario sea una Administración Pública
    /// </summary>
    CertificacionesObra14,
    
    /// <summary>
    /// 15 - Factura con IVA pendiente de devengo en operaciones de tracto sucesivo
    /// </summary>
    TractoSucesivo15,
    
    /// <summary>
    /// 16 - Primer semestre 2017
    /// </summary>
    PrimerSemestre201716,
    
    /// <summary>
    /// 17 - Operaciones acogidas a alguno de los regímenes previstos en el Capítulo XI del Título IX (OSS e IOSS)
    /// </summary>
    OSS_IOSS17,
    
    /// <summary>
    /// 18 - Exportaciones y operaciones asimiladas a exportaciones
    /// </summary>
    ExportacionesAsimiladas18,
    
    /// <summary>
    /// 19 - Operaciones acogidas al régimen especial de recargo de equivalencia
    /// </summary>
    RecargoEquivalencia19
}
