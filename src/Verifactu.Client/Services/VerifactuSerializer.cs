using System.Xml;
using System.Xml.Linq;
using Verifactu.Client.Models;

namespace Verifactu.Client.Services;

/// <summary>
/// Serializa registros de facturación conforme a los XSD oficiales de AEAT VERI*FACTU.
/// Implementa la estructura definida en SuministroLR.xsd y SuministroInformacion.xsd
/// </summary>
public class VerifactuSerializer : IVerifactuSerializer
{
    // Namespaces oficiales según documentación AEAT
    private static readonly XNamespace NsSuministroLR = 
        "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroLR.xsd";
    
    private static readonly XNamespace NsSuministroInfo = 
        "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd";

    /// <summary>
    /// Crea el XML de un registro de facturación conforme al XSD oficial.
    /// Estructura según ejemplos en Veri-Factu_Descripcion_SWeb.md (Anexo II)
    /// </summary>
    public XmlDocument CrearXmlRegistro(RegistroFacturacion reg)
    {
        var doc = new XDocument(
            new XElement(NsSuministroInfo + "RegistroAlta",
                new XAttribute(XNamespace.Xmlns + "sum1", NsSuministroInfo),
                
                // IDVersion - Versión del esquema
                new XElement(NsSuministroInfo + "IDVersion", reg.IDVersion),
                
                // IDFactura - Identificación de la factura
                new XElement(NsSuministroInfo + "IDFactura",
                    new XElement(NsSuministroInfo + "IDEmisorFactura", reg.IDEmisorFactura),
                    new XElement(NsSuministroInfo + "NumSerieFactura", reg.NumSerieFactura),
                    new XElement(NsSuministroInfo + "FechaExpedicionFactura", 
                        reg.FechaExpedicionFactura.ToString("dd-MM-yyyy"))
                ),
                
                // NombreRazonEmisor
                new XElement(NsSuministroInfo + "NombreRazonEmisor", reg.NombreRazonEmisor),
                
                // TipoFactura
                new XElement(NsSuministroInfo + "TipoFactura", reg.TipoFactura),
                
                // DescripcionOperacion
                new XElement(NsSuministroInfo + "DescripcionOperacion", reg.DescripcionOperacion),
                
                // Destinatarios (si existe)
                CrearDestinatarios(reg.Destinatario),
                
                // Desglose - Desglose de IVA (obligatorio)
                new XElement(NsSuministroInfo + "Desglose",
                    reg.Desglose.Select(d => 
                        new XElement(NsSuministroInfo + "DetalleDesglose",
                            new XElement(NsSuministroInfo + "ClaveRegimen", d.ClaveRegimen),
                            new XElement(NsSuministroInfo + "CalificacionOperacion", d.CalificacionOperacion),
                            new XElement(NsSuministroInfo + "TipoImpositivo", d.TipoImpositivo.ToString("F2")),
                            new XElement(NsSuministroInfo + "BaseImponibleOimporteNoSujeto", 
                                d.BaseImponible.ToString("F2")),
                            new XElement(NsSuministroInfo + "CuotaRepercutida", 
                                d.CuotaRepercutida.ToString("F2"))
                        )
                    )
                ),
                
                // CuotaTotal
                new XElement(NsSuministroInfo + "CuotaTotal", reg.CuotaTotal.ToString("F2")),
                
                // ImporteTotal
                new XElement(NsSuministroInfo + "ImporteTotal", reg.ImporteTotal.ToString("F2")),
                
                // Encadenamiento - Hash del registro anterior
                CrearEncadenamiento(reg),
                
                // SistemaInformatico - Información del sistema
                new XElement(NsSuministroInfo + "SistemaInformatico",
                    new XElement(NsSuministroInfo + "NombreRazon", reg.SistemaInformatico.NombreRazon),
                    new XElement(NsSuministroInfo + "NIF", reg.SistemaInformatico.Nif),
                    new XElement(NsSuministroInfo + "NombreSistemaInformatico", 
                        reg.SistemaInformatico.NombreSistemaInformatico),
                    new XElement(NsSuministroInfo + "IdSistemaInformatico", 
                        reg.SistemaInformatico.IdSistemaInformatico),
                    new XElement(NsSuministroInfo + "Version", reg.SistemaInformatico.Version),
                    new XElement(NsSuministroInfo + "NumeroInstalacion", 
                        reg.SistemaInformatico.NumeroInstalacion),
                    new XElement(NsSuministroInfo + "TipoUsoPosibleSoloVerifactu", 
                        reg.SistemaInformatico.TipoUsoPosibleSoloVerifactu),
                    new XElement(NsSuministroInfo + "TipoUsoPosibleMultiOT", 
                        reg.SistemaInformatico.TipoUsoPosibleMultiOT),
                    new XElement(NsSuministroInfo + "IndicadorMultiplesOT", 
                        reg.SistemaInformatico.IndicadorMultiplesOT)
                ),
                
                // FechaHoraHusoGenRegistro - Fecha/hora de generación con huso horario
                new XElement(NsSuministroInfo + "FechaHoraHusoGenRegistro", 
                    reg.FechaHoraHusoGenRegistro.ToString("yyyy-MM-ddTHH:mm:sszzz")),
                
                // TipoHuella - Tipo de hash (01 = SHA-256)
                new XElement(NsSuministroInfo + "TipoHuella", reg.TipoHuella),
                
                // Huella - Hash del registro
                new XElement(NsSuministroInfo + "Huella", reg.Huella)
            )
        );

        var xmlDoc = new XmlDocument();
        using var reader = doc.CreateReader();
        xmlDoc.Load(reader);
        return xmlDoc;
    }

    /// <summary>
    /// Crea el elemento Destinatarios si existe destinatario
    /// </summary>
    private XElement? CrearDestinatarios(Receptor? destinatario)
    {
        if (destinatario == null)
            return null;

        return new XElement(NsSuministroInfo + "Destinatarios",
            new XElement(NsSuministroInfo + "IDDestinatario",
                new XElement(NsSuministroInfo + "NombreRazon", destinatario.Nombre),
                destinatario.Nif != null 
                    ? new XElement(NsSuministroInfo + "NIF", destinatario.Nif)
                    : null
            )
        );
    }

    /// <summary>
    /// Crea el elemento Encadenamiento con el hash del registro anterior
    /// </summary>
    private XElement? CrearEncadenamiento(RegistroFacturacion reg)
    {
        // Solo crear encadenamiento si hay registro anterior
        if (string.IsNullOrEmpty(reg.HuellaAnterior))
            return null;

        return new XElement(NsSuministroInfo + "Encadenamiento",
            new XElement(NsSuministroInfo + "RegistroAnterior",
                reg.IDEmisorFacturaAnterior != null 
                    ? new XElement(NsSuministroInfo + "IDEmisorFactura", reg.IDEmisorFacturaAnterior)
                    : null,
                reg.NumSerieFacturaAnterior != null 
                    ? new XElement(NsSuministroInfo + "NumSerieFactura", reg.NumSerieFacturaAnterior)
                    : null,
                reg.FechaExpedicionFacturaAnterior != null 
                    ? new XElement(NsSuministroInfo + "FechaExpedicionFactura", 
                        reg.FechaExpedicionFacturaAnterior.Value.ToString("dd-MM-yyyy"))
                    : null,
                new XElement(NsSuministroInfo + "Huella", reg.HuellaAnterior)
            )
        );
    }
}
