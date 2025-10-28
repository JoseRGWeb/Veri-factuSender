using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Verifactu.Client.Services;

public class VerifactuSoapClient : IVerifactuSoapClient
{
    private readonly string _endpointUrl;
    private readonly string _soapAction; // Ajusta al WSDL real

    public VerifactuSoapClient(string endpointUrl, string soapAction)
    {
        _endpointUrl = endpointUrl;
        _soapAction = soapAction;
    }

    public async Task<string> EnviarRegistroAsync(XmlDocument xmlFirmado, X509Certificate2 cert, CancellationToken ct = default)
    {
        // Cliente con TLS mutuo
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(cert);
        using var http = new HttpClient(handler);

        var soapEnvelope = ConstruirSobreSoap(xmlFirmado);
        var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        if (!string.IsNullOrWhiteSpace(_soapAction))
        {
            content.Headers.Add("SOAPAction", _soapAction);
        }

        using var resp = await http.PostAsync(_endpointUrl, content, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync(ct);
    }

    private static string ConstruirSobreSoap(XmlDocument payload)
    {
        // SOAP 1.1 simplificado
        var sb = new StringBuilder();
        sb.Append("""
<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
  <soapenv:Header/>
  <soapenv:Body>
    <EnviarRegistroRequest xmlns="urn:aeat:verifactu:placeholder">
      <RegistroXml>
""");
        sb.Append(payload.OuterXml);
        sb.Append("""
      </RegistroXml>
    </EnviarRegistroRequest>
  </soapenv:Body>
</soapenv:Envelope>
""");
        return sb.ToString();
    }
}
