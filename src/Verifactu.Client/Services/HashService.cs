using System.Security.Cryptography;
using System.Text;
using Verifactu.Client.Models;

namespace Verifactu.Client.Services;

public class HashService : IHashService
{
    public string CalcularHuella(RegistroFacturacion registro, string? huellaAnterior)
    {
        // Placeholder de ejemplo: concatenamos campos clave y calculamos SHA-256
        // Sustituye por el algoritmo oficial (incluyendo encadenado, normalización, etc.).
        var sb = new StringBuilder();
        sb.Append(registro.Serie)
          .Append("|").Append(registro.Numero)
          .Append("|").Append(registro.FechaHoraExpedicionUtc.ToString("yyyy-MM-ddTHH:mm:ssZ"))
          .Append("|").Append(registro.Factura.Emisor.Nif)
          .Append("|").Append(registro.Factura.Receptor.Nombre)
          .Append("|").Append(registro.Factura.Totales.ImporteTotal.ToString("F2"))
          .Append("|").Append(huellaAnterior ?? "");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
