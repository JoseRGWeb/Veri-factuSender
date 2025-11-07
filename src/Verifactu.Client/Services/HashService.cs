using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Verifactu.Client.Models;

namespace Verifactu.Client.Services;

/// <summary>
/// Servicio para calcular la huella (hash) de registros de facturación según especificación AEAT.
/// Implementa SHA-256 conforme a la documentación oficial de VERI*FACTU.
/// 
/// IMPORTANTE: El orden de los campos y su formato son críticos y están definidos
/// por la especificación oficial de AEAT. Cualquier cambio puede invalidar la huella.
/// 
/// Ver: docs/algoritmo-huella.md para especificación completa
/// </summary>
public class HashService : IHashService
{
    /// <summary>
    /// Calcula la huella SHA-256 de un registro de facturación según especificación oficial AEAT.
    /// 
    /// Orden de campos (según AEAT):
    /// 1. IDVersion
    /// 2. IDEmisorFactura (NIF)
    /// 3. NumSerieFactura
    /// 4. FechaExpedicionFactura (formato dd-MM-yyyy)
    /// 5. TipoFactura
    /// 6. CuotaTotal (2 decimales, punto como separador)
    /// 7. ImporteTotal (2 decimales, punto como separador)
    /// 8. Huella del registro anterior (cadena vacía si es el primero)
    /// 9. FechaHoraHusoGenRegistro (formato ISO 8601: yyyy-MM-ddTHH:mm:sszzz)
    /// 
    /// Nota: Los campos se concatenan SIN separadores entre ellos.
    /// </summary>
    /// <param name="registro">Registro de facturación del que calcular la huella</param>
    /// <param name="huellaAnterior">Huella del registro anterior (null o vacío para el primer registro)</param>
    /// <returns>Huella SHA-256 en formato hexadecimal MAYÚSCULAS</returns>
    public string CalcularHuella(RegistroFacturacion registro, string? huellaAnterior)
    {
        // StringBuilder para construir la cadena a hashear
        var sb = new StringBuilder();

        // 1. IDVersion (ej: "1.0")
        sb.Append(registro.IDVersion);

        // 2. IDEmisorFactura (NIF del emisor)
        sb.Append(registro.IDEmisorFactura);

        // 3. NumSerieFactura (número de serie completo)
        sb.Append(registro.NumSerieFactura);

        // 4. FechaExpedicionFactura (formato dd-MM-yyyy)
        sb.Append(registro.FechaExpedicionFactura.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));

        // 5. TipoFactura (ej: "F1")
        sb.Append(registro.TipoFactura);

        // 6. CuotaTotal (2 decimales, punto como separador, sin agrupación de miles)
        sb.Append(NormalizarDecimal(registro.CuotaTotal));

        // 7. ImporteTotal (2 decimales, punto como separador, sin agrupación de miles)
        sb.Append(NormalizarDecimal(registro.ImporteTotal));

        // 8. Huella del registro anterior (cadena vacía si es el primero)
        sb.Append(huellaAnterior ?? string.Empty);

        // 9. FechaHoraHusoGenRegistro (formato ISO 8601 con huso horario)
        // El formato debe ser: yyyy-MM-ddTHH:mm:sszzz (ej: 2024-09-13T19:20:30+01:00)
        sb.Append(registro.FechaHoraHusoGenRegistro.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));

        // Convertir a bytes UTF-8 (sin BOM)
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());

        // Calcular SHA-256
        var hash = SHA256.HashData(bytes);

        // Convertir a hexadecimal en MAYÚSCULAS
        return Convert.ToHexString(hash);
    }

    /// <summary>
    /// Normaliza un valor decimal al formato requerido por AEAT:
    /// - 2 decimales exactos
    /// - Punto (.) como separador decimal
    /// - Sin separador de miles
    /// - CultureInfo.InvariantCulture
    /// </summary>
    /// <param name="valor">Valor decimal a normalizar</param>
    /// <returns>Cadena normalizada (ej: "1234.50")</returns>
    private static string NormalizarDecimal(decimal valor)
    {
        // Formato "F2" = Fixed-point con 2 decimales
        // InvariantCulture = punto como separador decimal, sin separador de miles
        return valor.ToString("F2", CultureInfo.InvariantCulture);
    }
}
