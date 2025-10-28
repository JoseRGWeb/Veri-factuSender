using System.Xml;
using System.Xml.Linq;
using Verifactu.Client.Models;

namespace Verifactu.Client.Services;

public class VerifactuSerializer : IVerifactuSerializer
{
    public XmlDocument CrearXmlRegistro(RegistroFacturacion reg)
    {
        // Placeholder: Crea un XML simple. Cambia a los elementos y namespaces de los XSD de AEAT.
        XNamespace ns = "urn:aeat:verifactu:placeholder";
        var doc = new XDocument(
            new XElement(ns + "RegistroFacturacion",
                new XElement(ns + "Cabecera",
                    new XElement(ns + "Uuid", reg.Uuid),
                    new XElement(ns + "FechaHoraExpedicionUtc", reg.FechaHoraExpedicionUtc.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                    new XElement(ns + "Serie", reg.Serie),
                    new XElement(ns + "Numero", reg.Numero),
                    new XElement(ns + "HashPrevio", reg.HashPrevio),
                    new XElement(ns + "Huella", reg.Huella)
                ),
                new XElement(ns + "Factura",
                    new XElement(ns + "Emisor",
                        new XElement(ns + "Nif", reg.Factura.Emisor.Nif),
                        new XElement(ns + "Nombre", reg.Factura.Emisor.Nombre)
                    ),
                    new XElement(ns + "Receptor",
                        new XElement(ns + "Nif", reg.Factura.Receptor.Nif ?? ""),
                        new XElement(ns + "Nombre", reg.Factura.Receptor.Nombre)
                    ),
                    new XElement(ns + "Totales",
                        new XElement(ns + "BaseImponible", reg.Factura.Totales.BaseImponible),
                        new XElement(ns + "CuotaImpuestos", reg.Factura.Totales.CuotaImpuestos),
                        new XElement(ns + "ImporteTotal", reg.Factura.Totales.ImporteTotal),
                        new XElement(ns + "Moneda", reg.Factura.Moneda)
                    )
                )
            )
        );

        var xmlDoc = new XmlDocument();
        using var reader = doc.CreateReader();
        xmlDoc.Load(reader);
        return xmlDoc;
    }
}
