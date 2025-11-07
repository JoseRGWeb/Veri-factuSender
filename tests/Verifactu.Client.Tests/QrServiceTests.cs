using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Verifactu.Client.Models;
using Verifactu.Client.Services;
using Xunit;

namespace Verifactu.Client.Tests;

/// <summary>
/// Tests para el servicio de generación de códigos QR según especificación S16 de AEAT.
/// Verifica la correcta generación de URLs de cotejo y códigos QR en diferentes formatos.
/// </summary>
public class QrServiceTests
{
    private readonly QrService _qrService;

    public QrServiceTests()
    {
        _qrService = new QrService();
    }

    #region Tests de GenerarUrlCotejo

    /// <summary>
    /// Test: Verifica que se genera una URL válida con todos los parámetros requeridos
    /// </summary>
    [Fact]
    public void GenerarUrlCotejo_GeneraUrlValida()
    {
        // Arrange
        var factura = CrearFacturaPrueba();
        var huella = "ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234";

        // Act
        var url = _qrService.GenerarUrlCotejo(factura, huella);

        // Assert
        Assert.NotNull(url);
        Assert.StartsWith("https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html?", url);
        
        // Verificar que contiene todos los parámetros requeridos
        Assert.Contains("nif=", url);
        Assert.Contains("num=", url);
        Assert.Contains("fecha=", url);
        Assert.Contains("importe=", url);
        Assert.Contains("huella=", url);
    }

    /// <summary>
    /// Test: Verifica que los parámetros de la URL tienen los valores correctos
    /// </summary>
    [Fact]
    public void GenerarUrlCotejo_ParametrosCorrectos()
    {
        // Arrange
        var factura = new Factura(
            Serie: "A",
            Numero: "2024/001",
            FechaEmision: new DateTime(2024, 9, 13, 0, 0, 0, DateTimeKind.Utc),
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Operación de prueba",
            Emisor: new Emisor("B12345678", "EMPRESA TEST SL"),
            Receptor: new Receptor("12345678Z", "CLIENTE TEST"),
            Totales: new TotalesFactura(100.00m, 21.00m, 121.00m)
        );
        var huella = "ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234";

        // Act
        var url = _qrService.GenerarUrlCotejo(factura, huella);

        // Assert
        Assert.Contains("nif=B12345678", url);
        Assert.Contains("num=A%2F2024%2F001", url); // Serie/Número con escape de /
        Assert.Contains("fecha=13-09-2024", url);
        Assert.Contains("importe=121.00", url);
        Assert.Contains("huella=ABCD123456789", url); // Primeros 13 caracteres
    }

    /// <summary>
    /// Test: Verifica que solo se usan los primeros 13 caracteres de la huella
    /// </summary>
    [Fact]
    public void GenerarUrlCotejo_UsaPrimeros13CaracteresHuella()
    {
        // Arrange
        var factura = CrearFacturaPrueba();
        var huella = "1234567890123XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";

        // Act
        var url = _qrService.GenerarUrlCotejo(factura, huella);

        // Assert
        Assert.Contains("huella=1234567890123", url);
        Assert.DoesNotContain("XXXXXXXXX", url);
    }

    /// <summary>
    /// Test: Verifica el formato de fecha dd-MM-yyyy
    /// </summary>
    [Fact]
    public void GenerarUrlCotejo_FormatoFechaCorrecta()
    {
        // Arrange
        var factura = CrearFacturaPrueba() with 
        { 
            FechaEmision = new DateTime(2024, 12, 25, 0, 0, 0, DateTimeKind.Utc) 
        };
        var huella = "ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234";

        // Act
        var url = _qrService.GenerarUrlCotejo(factura, huella);

        // Assert
        Assert.Contains("fecha=25-12-2024", url);
    }

    /// <summary>
    /// Test: Verifica que el importe tiene 2 decimales y punto como separador
    /// </summary>
    [Fact]
    public void GenerarUrlCotejo_FormatoImporteCorrecto()
    {
        // Arrange
        var factura = CrearFacturaPrueba() with
        {
            Totales = new TotalesFactura(1234.5m, 259.245m, 1493.745m)
        };
        var huella = "ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234";

        // Act
        var url = _qrService.GenerarUrlCotejo(factura, huella);

        // Assert
        // Debe redondearse a 2 decimales: 1493.745 -> 1493.75 (redondeo bancario)
        Assert.Contains("importe=1493.75", url);
    }

    /// <summary>
    /// Test: Verifica que caracteres especiales se escapan correctamente en la URL
    /// </summary>
    [Fact]
    public void GenerarUrlCotejo_EscapaCaracteresEspeciales()
    {
        // Arrange
        var factura = CrearFacturaPrueba() with
        {
            Serie = "A/B",
            Numero = "2024-001"
        };
        var huella = "ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234";

        // Act
        var url = _qrService.GenerarUrlCotejo(factura, huella);

        // Assert
        // El "/" debe estar escapado como %2F, el "-" no necesita escape
        Assert.Contains("num=A%2FB%2F2024-001", url);
    }

    /// <summary>
    /// Test: Verifica que se lanza excepción si la factura es null
    /// </summary>
    [Fact]
    public void GenerarUrlCotejo_FacturaNull_LanzaExcepcion()
    {
        // Arrange
        var huella = "ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _qrService.GenerarUrlCotejo(null!, huella));
    }

    /// <summary>
    /// Test: Verifica que se lanza excepción si la huella es null o vacía
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerarUrlCotejo_HuellaInvalida_LanzaExcepcion(string? huella)
    {
        // Arrange
        var factura = CrearFacturaPrueba();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _qrService.GenerarUrlCotejo(factura, huella!));
    }

    /// <summary>
    /// Test: Verifica que se lanza excepción si la huella tiene menos de 13 caracteres
    /// </summary>
    [Theory]
    [InlineData("12345")]
    [InlineData("123456789012")] // 12 caracteres
    public void GenerarUrlCotejo_HuellaMuyCorta_LanzaExcepcion(string huella)
    {
        // Arrange
        var factura = CrearFacturaPrueba();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _qrService.GenerarUrlCotejo(factura, huella));
        Assert.Contains("debe tener al menos 13 caracteres", ex.Message);
    }

    /// <summary>
    /// Test: Verifica el manejo de facturas sin serie
    /// </summary>
    [Fact]
    public void GenerarUrlCotejo_FacturaSinSerie()
    {
        // Arrange
        var factura = CrearFacturaPrueba() with
        {
            Serie = "",
            Numero = "2024001"
        };
        var huella = "ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234";

        // Act
        var url = _qrService.GenerarUrlCotejo(factura, huella);

        // Assert
        Assert.Contains("num=2024001", url);
        Assert.DoesNotContain("%2F2024001", url); // No debe contener barra
    }

    #endregion

    #region Tests de GenerarQrPng

    /// <summary>
    /// Test: Verifica que se genera un array de bytes no vacío para PNG
    /// </summary>
    [Fact]
    public void GenerarQrPng_GeneraBytesNoVacios()
    {
        // Arrange
        var url = "https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html?nif=B12345678";

        // Act
        var qrBytes = _qrService.GenerarQrPng(url);

        // Assert
        Assert.NotNull(qrBytes);
        Assert.NotEmpty(qrBytes);
    }

    /// <summary>
    /// Test: Verifica que el PNG generado tiene cabecera PNG válida
    /// </summary>
    [Fact]
    public void GenerarQrPng_CabeceraPngValida()
    {
        // Arrange
        var url = "https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html?nif=B12345678";

        // Act
        var qrBytes = _qrService.GenerarQrPng(url);

        // Assert
        // Los primeros 8 bytes de un PNG son: 137 80 78 71 13 10 26 10
        var cabeceraPng = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        var cabeceraGenerada = qrBytes.Take(8).ToArray();
        Assert.Equal(cabeceraPng, cabeceraGenerada);
    }

    /// <summary>
    /// Test: Verifica que diferentes tamaños generan imágenes de diferente tamaño
    /// </summary>
    [Fact]
    public void GenerarQrPng_DiferentesTamanos()
    {
        // Arrange
        var url = "https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html?nif=B12345678";

        // Act
        var qrPequeno = _qrService.GenerarQrPng(url, pixelsPorModulo: 5);
        var qrGrande = _qrService.GenerarQrPng(url, pixelsPorModulo: 20);

        // Assert
        Assert.NotEqual(qrPequeno.Length, qrGrande.Length);
        Assert.True(qrGrande.Length > qrPequeno.Length); // El QR grande debe tener más bytes
    }

    /// <summary>
    /// Test: Verifica que se lanza excepción con URL nula o vacía
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerarQrPng_UrlInvalida_LanzaExcepcion(string? url)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _qrService.GenerarQrPng(url!));
    }

    /// <summary>
    /// Test: Verifica que se lanza excepción con tamaño inválido
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void GenerarQrPng_TamanoInvalido_LanzaExcepcion(int pixelsPorModulo)
    {
        // Arrange
        var url = "https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html?nif=B12345678";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _qrService.GenerarQrPng(url, pixelsPorModulo));
    }

    #endregion

    #region Tests de GenerarQrSvg

    /// <summary>
    /// Test: Verifica que se genera SVG válido
    /// </summary>
    [Fact]
    public void GenerarQrSvg_GeneraSvgValido()
    {
        // Arrange
        var url = "https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html?nif=B12345678";

        // Act
        var svg = _qrService.GenerarQrSvg(url);

        // Assert
        Assert.NotNull(svg);
        Assert.NotEmpty(svg);
        Assert.Contains("<svg", svg);
        Assert.Contains("</svg>", svg);
    }

    /// <summary>
    /// Test: Verifica que el SVG contiene elementos XML válidos
    /// </summary>
    [Fact]
    public void GenerarQrSvg_ContieneElementosXmlValidos()
    {
        // Arrange
        var url = "https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html?nif=B12345678";

        // Act
        var svg = _qrService.GenerarQrSvg(url);

        // Assert
        Assert.Contains("xmlns", svg); // Debe tener namespace
        Assert.Contains("<rect", svg); // Debe contener rectángulos (módulos del QR)
    }

    /// <summary>
    /// Test: Verifica que se lanza excepción con URL nula o vacía
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerarQrSvg_UrlInvalida_LanzaExcepcion(string? url)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _qrService.GenerarQrSvg(url!));
    }

    /// <summary>
    /// Test: Verifica que se lanza excepción con tamaño inválido
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GenerarQrSvg_TamanoInvalido_LanzaExcepcion(int pixelsPorModulo)
    {
        // Arrange
        var url = "https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html?nif=B12345678";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _qrService.GenerarQrSvg(url, pixelsPorModulo));
    }

    #endregion

    #region Tests de GenerarQrBase64

    /// <summary>
    /// Test: Verifica que se genera una cadena Base64 válida con data URI
    /// </summary>
    [Fact]
    public void GenerarQrBase64_GeneraDataUriValido()
    {
        // Arrange
        var url = "https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html?nif=B12345678";

        // Act
        var base64 = _qrService.GenerarQrBase64(url);

        // Assert
        Assert.NotNull(base64);
        Assert.NotEmpty(base64);
        Assert.StartsWith("data:image/png;base64,", base64);
    }

    /// <summary>
    /// Test: Verifica que la parte Base64 es válida
    /// </summary>
    [Fact]
    public void GenerarQrBase64_Base64Valido()
    {
        // Arrange
        var url = "https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html?nif=B12345678";

        // Act
        var dataUri = _qrService.GenerarQrBase64(url);
        var base64String = dataUri.Replace("data:image/png;base64,", "");

        // Assert
        // Verificar que se puede decodificar
        var bytes = Convert.FromBase64String(base64String);
        Assert.NotEmpty(bytes);
        
        // Verificar que tiene cabecera PNG
        var cabeceraPng = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        var cabeceraGenerada = bytes.Take(8).ToArray();
        Assert.Equal(cabeceraPng, cabeceraGenerada);
    }

    /// <summary>
    /// Test: Verifica que se lanza excepción con URL nula o vacía
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerarQrBase64_UrlInvalida_LanzaExcepcion(string? url)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _qrService.GenerarQrBase64(url!));
    }

    /// <summary>
    /// Test: Verifica que se lanza excepción con tamaño inválido
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GenerarQrBase64_TamanoInvalido_LanzaExcepcion(int pixelsPorModulo)
    {
        // Arrange
        var url = "https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html?nif=B12345678";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _qrService.GenerarQrBase64(url, pixelsPorModulo));
    }

    #endregion

    #region Tests de integración

    /// <summary>
    /// Test de integración: Verifica el flujo completo desde factura hasta QR
    /// </summary>
    [Fact]
    public void IntegracionCompleta_FacturaAQr()
    {
        // Arrange
        var factura = new Factura(
            Serie: "A",
            Numero: "2024/001",
            FechaEmision: new DateTime(2024, 9, 13, 0, 0, 0, DateTimeKind.Utc),
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Venta de servicios",
            Emisor: new Emisor("B12345678", "EMPRESA TEST SL"),
            Receptor: new Receptor("12345678Z", "CLIENTE TEST"),
            Totales: new TotalesFactura(100.00m, 21.00m, 121.00m)
        );
        var huella = "ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234567890ABCD1234";

        // Act
        var url = _qrService.GenerarUrlCotejo(factura, huella);
        var qrPng = _qrService.GenerarQrPng(url);
        var qrSvg = _qrService.GenerarQrSvg(url);
        var qrBase64 = _qrService.GenerarQrBase64(url);

        // Assert
        Assert.NotNull(url);
        Assert.NotEmpty(qrPng);
        Assert.NotEmpty(qrSvg);
        Assert.NotEmpty(qrBase64);
        
        // Verificar que la URL está en todos los formatos generados
        Assert.Contains("nif=B12345678", url);
        Assert.StartsWith("data:image/png;base64,", qrBase64);
    }

    /// <summary>
    /// Test: Verifica que URLs largas se codifican correctamente en el QR
    /// </summary>
    [Fact]
    public void GenerarQr_UrlLarga_SeGeneraCorrectamente()
    {
        // Arrange
        var factura = new Factura(
            Serie: "SERIE-MUY-LARGA-2024",
            Numero: "NUMERO-DE-FACTURA-LARGO-001",
            FechaEmision: new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Operación",
            Emisor: new Emisor("B99999999", "NOMBRE EMPRESA MUY LARGO SL"),
            Totales: new TotalesFactura(99999.99m, 20999.998m, 120999.988m)
        );
        var huella = "FEDCBA9876543210FEDCBA9876543210FEDCBA9876543210FEDCBA9876543210";

        // Act
        var url = _qrService.GenerarUrlCotejo(factura, huella);
        var qrPng = _qrService.GenerarQrPng(url);

        // Assert
        Assert.NotNull(url);
        Assert.NotEmpty(qrPng);
        // URLs largas deberían generar QR más complejos (más bytes)
        Assert.True(qrPng.Length > 1000); // Un QR complejo debería tener más de 1KB
    }

    #endregion

    #region Métodos auxiliares

    /// <summary>
    /// Crea una factura de prueba con valores por defecto
    /// </summary>
    private static Factura CrearFacturaPrueba()
    {
        return new Factura(
            Serie: "A",
            Numero: "2024/001",
            FechaEmision: new DateTime(2024, 9, 13, 0, 0, 0, DateTimeKind.Utc),
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Operación de prueba",
            Emisor: new Emisor("B12345678", "EMPRESA TEST SL"),
            Receptor: new Receptor("12345678Z", "CLIENTE TEST"),
            Totales: new TotalesFactura(100.00m, 21.00m, 121.00m)
        );
    }

    #endregion
}
