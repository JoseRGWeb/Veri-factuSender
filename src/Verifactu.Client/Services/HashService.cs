using System.Security.Cryptography;
using System.Text;
using Verifactu.Client.Models;

namespace Verifactu.Client.Services;

/// <summary>
/// Servicio para calcular la huella (hash) de registros de facturación según especificación AEAT.
/// Implementa SHA-256 conforme a la documentación oficial de VERI*FACTU.
/// </summary>
public class HashService : IHashService
{
    /// <summary>
    /// Calcula la huella SHA-256 de un registro de facturación.
    /// NOTA: Esta es una implementación placeholder. La especificación oficial detalla
    /// qué campos exactos y en qué orden deben incluirse en el cálculo del hash.
    /// Ver: docs/Verifactu-Guia-Tecnica.md sección 5 "Algoritmo de huella"
    /// </summary>
    public string CalcularHuella(RegistroFacturacion registro, string? huellaAnterior)
    {
        // Implementación placeholder basada en campos principales
        // TODO: Implementar según especificación oficial completa de AEAT
        var sb = new StringBuilder();
        sb.Append(registro.NumSerieFactura)
          .Append("|").Append(registro.FechaExpedicionFactura.ToString("dd-MM-yyyy"))
          .Append("|").Append(registro.IDEmisorFactura)
          .Append("|").Append(registro.NombreRazonEmisor)
          .Append("|").Append(registro.ImporteTotal.ToString("F2"))
          .Append("|").Append(registro.FechaHoraHusoGenRegistro.ToString("yyyy-MM-ddTHH:mm:sszzz"))
          .Append("|").Append(huellaAnterior ?? "");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
