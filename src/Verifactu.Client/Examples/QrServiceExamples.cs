using System;
using Verifactu.Client.Models;
using Verifactu.Client.Services;

namespace Verifactu.Client.Examples;

/// <summary>
/// Ejemplos de uso del servicio de generación de códigos QR para VERI*FACTU.
/// Demuestra cómo generar URLs de cotejo y códigos QR en diferentes formatos.
/// </summary>
public static class QrServiceExamples
{
    /// <summary>
    /// Ejemplo básico: Genera URL de cotejo y código QR en PNG
    /// </summary>
    public static void EjemploBasico()
    {
        Console.WriteLine("=== Ejemplo Básico: Generar QR para Factura ===\n");

        // 1. Crear una factura de ejemplo
        var factura = new Factura(
            Serie: "A",
            Numero: "2024/001",
            FechaEmision: new DateTime(2024, 11, 7, 10, 30, 0, DateTimeKind.Utc),
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Servicios de consultoría",
            Emisor: new Emisor("B12345678", "MI EMPRESA SL"),
            Receptor: new Receptor("12345678Z", "CLIENTE EJEMPLO"),
            Totales: new TotalesFactura(1000.00m, 210.00m, 1210.00m)
        );

        // 2. Supongamos que ya calculamos la huella del registro (normalmente usando HashService)
        var huella = "ABCD1234567890FEDCBA9876543210ABCD1234567890FEDCBA9876543210ABCD";

        // 3. Crear el servicio de QR
        var qrService = new QrService();

        // 4. Generar la URL de cotejo
        var urlCotejo = qrService.GenerarUrlCotejo(factura, huella);
        Console.WriteLine("URL de cotejo generada:");
        Console.WriteLine(urlCotejo);
        Console.WriteLine();

        // 5. Generar el código QR en formato PNG
        var qrPngBytes = qrService.GenerarQrPng(urlCotejo, pixelsPorModulo: 20);
        Console.WriteLine($"Código QR PNG generado: {qrPngBytes.Length} bytes");
        Console.WriteLine("Puedes guardar estos bytes en un archivo .png");
        Console.WriteLine();
    }

    /// <summary>
    /// Ejemplo avanzado: Genera QR en todos los formatos disponibles
    /// </summary>
    public static void EjemploTodosLosFormatos()
    {
        Console.WriteLine("=== Ejemplo Avanzado: Generar QR en Todos los Formatos ===\n");

        // Crear factura y huella
        var factura = new Factura(
            Serie: "B",
            Numero: "2024/500",
            FechaEmision: DateTime.UtcNow,
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Venta de productos",
            Emisor: new Emisor("B87654321", "OTRA EMPRESA SL"),
            Totales: new TotalesFactura(5000.00m, 1050.00m, 6050.00m)
        );

        var huella = "1234567890ABCDEF1234567890ABCDEF1234567890ABCDEF1234567890ABCDEF";
        var qrService = new QrService();

        // Generar URL de cotejo
        var urlCotejo = qrService.GenerarUrlCotejo(factura, huella);
        Console.WriteLine("1. URL de Cotejo:");
        Console.WriteLine($"   {urlCotejo}");
        Console.WriteLine();

        // Generar QR en PNG
        var qrPng = qrService.GenerarQrPng(urlCotejo, pixelsPorModulo: 20);
        Console.WriteLine($"2. Código QR PNG: {qrPng.Length} bytes");
        Console.WriteLine("   Uso: Guardar en archivo para impresión");
        Console.WriteLine("   Ejemplo: File.WriteAllBytes(\"qr-factura.png\", qrPng);");
        Console.WriteLine();

        // Generar QR en SVG
        var qrSvg = qrService.GenerarQrSvg(urlCotejo, pixelsPorModulo: 10);
        Console.WriteLine($"3. Código QR SVG: {qrSvg.Length} caracteres");
        Console.WriteLine("   Uso: Formato vectorial escalable");
        Console.WriteLine($"   Primeros 100 caracteres: {qrSvg.Substring(0, Math.Min(100, qrSvg.Length))}...");
        Console.WriteLine();

        // Generar QR en Base64
        var qrBase64 = qrService.GenerarQrBase64(urlCotejo, pixelsPorModulo: 20);
        Console.WriteLine($"4. Código QR Base64 (data URI): {qrBase64.Length} caracteres");
        Console.WriteLine("   Uso: Incrustar directamente en HTML");
        Console.WriteLine("   Ejemplo HTML:");
        Console.WriteLine($"   <img src=\"{qrBase64.Substring(0, Math.Min(60, qrBase64.Length))}...\" alt=\"QR VERI*FACTU\" />");
        Console.WriteLine();
    }

    /// <summary>
    /// Ejemplo de integración completa: desde factura hasta guardar QR
    /// </summary>
    public static void EjemploIntegracionCompleta()
    {
        Console.WriteLine("=== Ejemplo de Integración Completa ===\n");

        // Paso 1: Crear factura completa
        var factura = new Factura(
            Serie: "C",
            Numero: "2024/1000",
            FechaEmision: new DateTime(2024, 11, 7, 0, 0, 0, DateTimeKind.Utc),
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Prestación de servicios profesionales",
            Emisor: new Emisor(
                Nif: "B99887766",
                Nombre: "SERVICIOS PROFESIONALES SL",
                Direccion: "Calle Mayor 123",
                Provincia: "Madrid",
                Municipio: "Madrid",
                Pais: "ES"
            ),
            Receptor: new Receptor(
                Nif: "98765432X",
                Nombre: "EMPRESA CLIENTE SA",
                Direccion: "Avenida Principal 456",
                Provincia: "Barcelona",
                Municipio: "Barcelona"
            ),
            Desglose: new System.Collections.Generic.List<DetalleDesglose>
            {
                new DetalleDesglose(
                    ClaveRegimen: "01",
                    CalificacionOperacion: "S1",
                    TipoImpositivo: 21m,
                    BaseImponible: 10000.00m,
                    CuotaRepercutida: 2100.00m
                )
            },
            Totales: new TotalesFactura(10000.00m, 2100.00m, 12100.00m)
        );

        Console.WriteLine("Paso 1: Factura creada");
        Console.WriteLine($"   Serie/Número: {factura.Serie}/{factura.Numero}");
        Console.WriteLine($"   Fecha: {factura.FechaEmision:dd-MM-yyyy}");
        Console.WriteLine($"   Importe Total: {factura.Totales?.ImporteTotal:N2} EUR");
        Console.WriteLine();

        // Paso 2: Calcular huella (en un caso real, usarías HashService)
        // Para este ejemplo, usamos una huella ficticia
        var huella = "FEDCBA9876543210FEDCBA9876543210FEDCBA9876543210FEDCBA9876543210";
        Console.WriteLine("Paso 2: Huella calculada");
        Console.WriteLine($"   Huella completa: {huella}");
        Console.WriteLine($"   Primeros 13 caracteres: {huella.Substring(0, 13)} (usados en QR)");
        Console.WriteLine();

        // Paso 3: Generar URL de cotejo
        var qrService = new QrService();
        var urlCotejo = qrService.GenerarUrlCotejo(factura, huella);
        Console.WriteLine("Paso 3: URL de cotejo generada");
        Console.WriteLine($"   {urlCotejo}");
        Console.WriteLine();

        // Paso 4: Generar QR en PNG
        var qrPng = qrService.GenerarQrPng(urlCotejo, pixelsPorModulo: 20);
        Console.WriteLine("Paso 4: Código QR generado");
        Console.WriteLine($"   Formato: PNG ({qrPng.Length} bytes)");
        Console.WriteLine($"   Tamaño: 20 píxeles por módulo");
        Console.WriteLine();

        // Paso 5: Guardar QR (simulado - en producción usarías File.WriteAllBytes)
        var nombreArchivo = $"qr-{factura.Serie.Replace("/", "-")}-{factura.Numero}.png";
        Console.WriteLine("Paso 5: Guardar código QR");
        Console.WriteLine($"   Nombre sugerido: {nombreArchivo}");
        Console.WriteLine($"   Código ejemplo: File.WriteAllBytes(\"{nombreArchivo}\", qrPng);");
        Console.WriteLine();

        Console.WriteLine("✓ Proceso completo finalizado");
        Console.WriteLine();
    }

    /// <summary>
    /// Ejemplo de manejo de errores comunes
    /// </summary>
    public static void EjemploManejoErrores()
    {
        Console.WriteLine("=== Ejemplo de Manejo de Errores ===\n");

        var qrService = new QrService();
        var factura = new Factura(
            Serie: "A",
            Numero: "2024/001",
            FechaEmision: DateTime.UtcNow,
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Test",
            Emisor: new Emisor("B12345678", "TEST SL"),
            Totales: new TotalesFactura(100m, 21m, 121m)
        );

        // Error 1: Huella demasiado corta
        Console.WriteLine("1. Intentar generar URL con huella demasiado corta:");
        try
        {
            var huellaCorta = "12345"; // Solo 5 caracteres
            qrService.GenerarUrlCotejo(factura, huellaCorta);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"   ✗ Error capturado: {ex.Message}");
        }
        Console.WriteLine();

        // Error 2: URL nula al generar QR
        Console.WriteLine("2. Intentar generar QR con URL nula:");
        try
        {
            qrService.GenerarQrPng(null!);
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"   ✗ Error capturado: {ex.ParamName} es null");
        }
        Console.WriteLine();

        // Error 3: Tamaño inválido
        Console.WriteLine("3. Intentar generar QR con tamaño inválido:");
        try
        {
            var url = "https://example.com";
            qrService.GenerarQrPng(url, pixelsPorModulo: -1);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"   ✗ Error capturado: {ex.Message}");
        }
        Console.WriteLine();

        Console.WriteLine("✓ Todos los errores fueron manejados correctamente");
        Console.WriteLine();
    }

    /// <summary>
    /// Ejecuta todos los ejemplos
    /// </summary>
    public static void EjecutarTodosLosEjemplos()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  EJEMPLOS DE USO DEL SERVICIO DE CÓDIGO QR VERI*FACTU     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        EjemploBasico();
        Console.WriteLine(new string('─', 60));
        Console.WriteLine();

        EjemploTodosLosFormatos();
        Console.WriteLine(new string('─', 60));
        Console.WriteLine();

        EjemploIntegracionCompleta();
        Console.WriteLine(new string('─', 60));
        Console.WriteLine();

        EjemploManejoErrores();
    }
}
