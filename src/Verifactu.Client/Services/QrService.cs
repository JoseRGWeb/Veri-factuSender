using System;
using System.Globalization;
using System.Linq;
using QRCoder;
using Verifactu.Client.Models;

namespace Verifactu.Client.Services;

/// <summary>
/// Servicio para generar códigos QR de cotejo según especificación oficial S16 de AEAT.
/// 
/// El código QR permite al receptor de la factura verificar el registro en la sede
/// electrónica de AEAT, cumpliendo con los requisitos de VERI*FACTU.
/// 
/// Referencias:
/// - Especificación oficial: docs/Veri-Factu_Descripcion_SWeb.md
/// - Guía técnica: docs/Verifactu-Guia-Tecnica.md (sección 7)
/// </summary>
public class QrService : IQrService
{
    private const string UrlBaseCotejo = "https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html";
    private const int LongitudHuellaCotejo = 13; // Primeros 13 caracteres de la huella según especificación

    /// <summary>
    /// Genera la URL de cotejo para verificar el registro en la sede electrónica de AEAT.
    /// 
    /// Parámetros incluidos en la URL según especificación oficial:
    /// - nif: NIF del emisor de la factura
    /// - num: Número completo de la factura (serie + número)
    /// - fecha: Fecha de emisión en formato dd-MM-yyyy
    /// - importe: Importe total de la factura con 2 decimales
    /// - huella: Primeros 13 caracteres de la huella SHA-256
    /// </summary>
    /// <param name="factura">Factura para la que generar la URL de cotejo</param>
    /// <param name="huella">Huella SHA-256 del registro de facturación (64 caracteres hexadecimales)</param>
    /// <returns>URL completa de cotejo con todos los parámetros necesarios</returns>
    /// <exception cref="ArgumentNullException">Si factura o huella son null</exception>
    /// <exception cref="ArgumentException">Si la huella tiene menos de 13 caracteres</exception>
    public string GenerarUrlCotejo(Factura factura, string huella)
    {
        if (factura == null)
            throw new ArgumentNullException(nameof(factura));
        
        if (string.IsNullOrWhiteSpace(huella))
            throw new ArgumentNullException(nameof(huella));
        
        if (huella.Length < LongitudHuellaCotejo)
            throw new ArgumentException(
                $"La huella debe tener al menos {LongitudHuellaCotejo} caracteres. Recibida: {huella.Length}",
                nameof(huella));

        // Construir número completo de factura (serie + número)
        var numeroCompleto = string.IsNullOrEmpty(factura.Serie) 
            ? factura.Numero 
            : $"{factura.Serie}/{factura.Numero}";

        // Calcular importe total (usar Totales si existe, sino calcular de Lineas)
        var importeTotal = factura.Totales?.ImporteTotal ?? 0m;

        // Formatear fecha en formato dd-MM-yyyy según especificación
        var fechaFormateada = factura.FechaEmision.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);

        // Formatear importe con 2 decimales y punto como separador
        var importeFormateado = importeTotal.ToString("F2", CultureInfo.InvariantCulture);

        // Tomar solo los primeros 13 caracteres de la huella
        var huellaCorta = huella.Substring(0, LongitudHuellaCotejo);

        // Construir parámetros de la URL (orden alfabético para consistencia)
        var parametros = new[]
        {
            $"nif={Uri.EscapeDataString(factura.Emisor.Nif)}",
            $"num={Uri.EscapeDataString(numeroCompleto)}",
            $"fecha={Uri.EscapeDataString(fechaFormateada)}",
            $"importe={Uri.EscapeDataString(importeFormateado)}",
            $"huella={Uri.EscapeDataString(huellaCorta)}"
        };

        var query = string.Join("&", parametros);
        return $"{UrlBaseCotejo}?{query}";
    }

    /// <summary>
    /// Genera código QR en formato PNG como array de bytes.
    /// Utiliza la librería QRCoder para generar el código QR.
    /// </summary>
    /// <param name="urlCotejo">URL de cotejo a codificar en el QR</param>
    /// <param name="pixelsPorModulo">Tamaño del QR (píxeles por módulo, por defecto 20)</param>
    /// <returns>Imagen QR en formato PNG como array de bytes</returns>
    /// <exception cref="ArgumentNullException">Si urlCotejo es null o vacío</exception>
    /// <exception cref="ArgumentOutOfRangeException">Si pixelsPorModulo es menor o igual a 0</exception>
    public byte[] GenerarQrPng(string urlCotejo, int pixelsPorModulo = 20)
    {
        if (string.IsNullOrWhiteSpace(urlCotejo))
            throw new ArgumentNullException(nameof(urlCotejo));
        
        if (pixelsPorModulo <= 0)
            throw new ArgumentOutOfRangeException(nameof(pixelsPorModulo), 
                "El tamaño debe ser mayor que 0");

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(urlCotejo, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        return qrCode.GetGraphic(pixelsPorModulo);
    }

    /// <summary>
    /// Genera código QR en formato SVG.
    /// El SVG es un formato vectorial escalable, ideal para impresión.
    /// </summary>
    /// <param name="urlCotejo">URL de cotejo a codificar en el QR</param>
    /// <param name="pixelsPorModulo">Tamaño del QR (píxeles por módulo, por defecto 10)</param>
    /// <returns>Imagen QR en formato SVG como cadena XML</returns>
    /// <exception cref="ArgumentNullException">Si urlCotejo es null o vacío</exception>
    /// <exception cref="ArgumentOutOfRangeException">Si pixelsPorModulo es menor o igual a 0</exception>
    public string GenerarQrSvg(string urlCotejo, int pixelsPorModulo = 10)
    {
        if (string.IsNullOrWhiteSpace(urlCotejo))
            throw new ArgumentNullException(nameof(urlCotejo));
        
        if (pixelsPorModulo <= 0)
            throw new ArgumentOutOfRangeException(nameof(pixelsPorModulo), 
                "El tamaño debe ser mayor que 0");

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(urlCotejo, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new SvgQRCode(qrCodeData);
        
        return qrCode.GetGraphic(pixelsPorModulo);
    }

    /// <summary>
    /// Genera código QR en formato Base64 (PNG codificado).
    /// Útil para incrustar directamente en HTML/CSS mediante data URI.
    /// 
    /// Ejemplo de uso en HTML:
    /// &lt;img src="data:image/png;base64,{resultado}" alt="QR VERI*FACTU" /&gt;
    /// </summary>
    /// <param name="urlCotejo">URL de cotejo a codificar en el QR</param>
    /// <param name="pixelsPorModulo">Tamaño del QR (píxeles por módulo, por defecto 20)</param>
    /// <returns>Imagen QR en formato Base64 (data URI completo para HTML)</returns>
    /// <exception cref="ArgumentNullException">Si urlCotejo es null o vacío</exception>
    /// <exception cref="ArgumentOutOfRangeException">Si pixelsPorModulo es menor o igual a 0</exception>
    public string GenerarQrBase64(string urlCotejo, int pixelsPorModulo = 20)
    {
        if (string.IsNullOrWhiteSpace(urlCotejo))
            throw new ArgumentNullException(nameof(urlCotejo));
        
        if (pixelsPorModulo <= 0)
            throw new ArgumentOutOfRangeException(nameof(pixelsPorModulo), 
                "El tamaño debe ser mayor que 0");

        var pngBytes = GenerarQrPng(urlCotejo, pixelsPorModulo);
        var base64 = Convert.ToBase64String(pngBytes);
        
        // Retornar data URI completo para uso directo en HTML
        return $"data:image/png;base64,{base64}";
    }
}
