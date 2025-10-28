using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Verifactu.Client.Services;

public class XmlSignerService : IXmlSignerService
{
    public XmlDocument Firmar(XmlDocument xml, X509Certificate2 cert)
    {
        // Asegurar que preserve whitespace si es necesario
        xml.PreserveWhitespace = true;

        var signedXml = new SignedXml(xml);
        signedXml.SigningKey = cert.GetRSAPrivateKey();

        // Referencia a todo el documento (enveloped signature)
        var reference = new Reference(string.Empty);
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        reference.AddTransform(new XmlDsigC14NTransform());

        signedXml.AddReference(reference);

        // Incluir la info del certificado
        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(cert));
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();
        var xmlDigitalSignature = signedXml.GetXml();

        // Adjuntar Signature al documento
        xml.DocumentElement!.AppendChild(xml.ImportNode(xmlDigitalSignature, true));
        return xml;
    }
}
