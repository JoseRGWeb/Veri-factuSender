using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using Verifactu.Client.Models;
using Verifactu.Client.Services;
using Xunit;
using Xunit.Abstractions;

namespace Verifactu.Client.Tests;

/// <summary>
/// Tests de validación XML contra esquemas XSD oficiales de AEAT.
/// Estos tests solo se ejecutan si los archivos XSD están disponibles en docs/wsdl/schemas/
/// </summary>
public class XmlValidationTests
{
    private readonly ITestOutputHelper _output;
    private readonly string _xsdPath;
    private readonly bool _xsdDisponibles;

    public XmlValidationTests(ITestOutputHelper output)
    {
        _output = output;
        
        // Buscar directorio de XSD desde diferentes rutas posibles
        _xsdPath = BuscarDirectorioXsd();
        _xsdDisponibles = Directory.Exists(_xsdPath) && 
                         File.Exists(Path.Combine(_xsdPath, "SuministroInformacion.xsd"));
        
        if (_xsdDisponibles)
        {
            _output.WriteLine($"✓ Esquemas XSD encontrados en: {_xsdPath}");
        }
        else
        {
            _output.WriteLine($"⚠ Esquemas XSD NO encontrados en: {_xsdPath}");
            _output.WriteLine("  Los tests de validación XSD se omitirán.");
            _output.WriteLine("  Para habilitar validación XSD completa:");
            _output.WriteLine("  1. Descargar esquemas desde AEAT");
            _output.WriteLine("  2. Colocarlos en docs/wsdl/schemas/");
            _output.WriteLine("  Ver docs/wsdl/schemas/README.md para instrucciones");
        }
    }

    private static string BuscarDirectorioXsd()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var dirInfo = new DirectoryInfo(currentDir);
        
        // Buscar hacia arriba hasta encontrar el archivo .sln
        while (dirInfo != null)
        {
            if (dirInfo.GetFiles("*.sln").Length > 0)
            {
                var xsdPath = Path.Combine(dirInfo.FullName, "docs", "wsdl", "schemas");
                if (Directory.Exists(xsdPath))
                {
                    return xsdPath;
                }
            }
            dirInfo = dirInfo.Parent;
        }
        
        // Ruta por defecto
        return Path.Combine(currentDir, "docs", "wsdl", "schemas");
    }

    private RegistroFacturacion CrearRegistroEjemplo()
    {
        var factura = new Factura(
            Serie: "A",
            Numero: "001",
            FechaEmision: new DateTime(2024, 11, 7),
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Venta de productos de prueba",
            Emisor: new Emisor("B12345678", "EMPRESA TEST SL"),
            Receptor: new Receptor("12345678Z", "CLIENTE TEST"),
            Lineas: new List<Linea>
            {
                new Linea("Producto de prueba", 1, 100, 21)
            },
            Totales: new TotalesFactura(100, 21, 121)
        );

        var desglose = new List<DetalleDesglose>
        {
            new DetalleDesglose(
                ClaveRegimen: "01",
                CalificacionOperacion: "S1",
                TipoImpositivo: 21,
                BaseImponible: 100,
                CuotaRepercutida: 21
            )
        };

        var sistemaInfo = new SistemaInformatico(
            NombreRazon: "DESARROLLADOR SL",
            Nif: "B87654321",
            NombreSistemaInformatico: "VerifactuSender",
            IdSistemaInformatico: "VS001",
            Version: "1.0.0",
            NumeroInstalacion: "001"
        );

        return new RegistroFacturacion(
            IDVersion: "1.0",
            IDEmisorFactura: "B12345678",
            NumSerieFactura: "A001",
            FechaExpedicionFactura: new DateTime(2024, 11, 7),
            NombreRazonEmisor: "EMPRESA TEST SL",
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Venta de productos de prueba",
            Desglose: desglose,
            CuotaTotal: 21,
            ImporteTotal: 121,
            FechaHoraHusoGenRegistro: new DateTime(2024, 11, 7, 14, 30, 0, DateTimeKind.Local),
            TipoHuella: "01",
            Huella: "A1B2C3D4E5F6789012345678901234567890123456789012345678901234ABCD",
            SistemaInformatico: sistemaInfo,
            Factura: factura,
            Destinatario: factura.Receptor
        );
    }

    [Fact]
    public void ValidarContraXsd_RegistroBasico_ValidoSiXsdDisponible()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        
        if (!_xsdDisponibles)
        {
            _output.WriteLine("Test omitido: Esquemas XSD no disponibles");
            return; // Skip test si no hay XSD
        }

        var validationService = new XmlValidationService(_xsdPath);
        var errores = new List<string>();

        // Act
        var resultado = validationService.ValidarContraXsd(xmlDoc, (sender, e) =>
        {
            var mensaje = $"{e.Severity}: {e.Message}";
            errores.Add(mensaje);
            _output.WriteLine(mensaje);
        });

        // Assert
        if (!resultado)
        {
            _output.WriteLine("\n❌ ERRORES DE VALIDACIÓN XSD:");
            foreach (var error in errores)
            {
                _output.WriteLine($"  - {error}");
            }
        }

        Assert.True(resultado, 
            $"El XML generado debe ser válido según XSD oficial. Errores encontrados: {string.Join("; ", errores)}");
    }

    [Fact]
    public void ValidarContraXsd_RegistroConEncadenamiento_ValidoSiXsdDisponible()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo() with
        {
            HuellaAnterior = "PREV1234567890123456789012345678901234567890123456789012345678AB",
            IDEmisorFacturaAnterior = "B12345678",
            NumSerieFacturaAnterior = "A000",
            FechaExpedicionFacturaAnterior = new DateTime(2024, 11, 6)
        };
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        
        if (!_xsdDisponibles)
        {
            _output.WriteLine("Test omitido: Esquemas XSD no disponibles");
            return;
        }

        var validationService = new XmlValidationService(_xsdPath);
        var errores = new List<string>();

        // Act
        var resultado = validationService.ValidarContraXsd(xmlDoc, (sender, e) =>
        {
            var mensaje = $"{e.Severity}: {e.Message}";
            errores.Add(mensaje);
            _output.WriteLine(mensaje);
        });

        // Assert
        if (!resultado)
        {
            _output.WriteLine("\n❌ ERRORES DE VALIDACIÓN XSD:");
            foreach (var error in errores)
            {
                _output.WriteLine($"  - {error}");
            }
        }

        Assert.True(resultado,
            $"Registro con encadenamiento debe ser válido según XSD. Errores: {string.Join("; ", errores)}");
    }

    [Fact]
    public void ValidarContraXsd_TiposFacturaF1aF4_ValidosSiXsdDisponible()
    {
        if (!_xsdDisponibles)
        {
            _output.WriteLine("Test omitido: Esquemas XSD no disponibles");
            return;
        }

        var serializer = new VerifactuSerializer();
        var validationService = new XmlValidationService(_xsdPath);
        var tiposFactura = new[] { TipoFactura.F1, TipoFactura.F2, TipoFactura.F3, TipoFactura.F4 };

        foreach (var tipoFactura in tiposFactura)
        {
            // Arrange
            var registro = CrearRegistroEjemplo() with { TipoFactura = tipoFactura };
            var xmlDoc = serializer.CrearXmlRegistro(registro);
            var errores = new List<string>();

            // Act
            var resultado = validationService.ValidarContraXsd(xmlDoc, (sender, e) =>
            {
                errores.Add($"{e.Severity}: {e.Message}");
            });

            // Assert
            Assert.True(resultado,
                $"Factura tipo {tipoFactura} debe ser válida según XSD. Errores: {string.Join("; ", errores)}");
            
            _output.WriteLine($"✓ TipoFactura {tipoFactura} válido");
        }
    }

    [Fact]
    public void ValidarContraXsd_MultipleDesglose_ValidoSiXsdDisponible()
    {
        // Arrange
        var desglose = new List<DetalleDesglose>
        {
            new DetalleDesglose("01", "S1", 21, 100, 21),
            new DetalleDesglose("01", "S1", 10, 50, 5),
            new DetalleDesglose("01", "S1", 4, 25, 1)
        };

        var registro = CrearRegistroEjemplo() with
        {
            Desglose = desglose,
            CuotaTotal = 27,
            ImporteTotal = 202
        };

        var serializer = new VerifactuSerializer();
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        
        if (!_xsdDisponibles)
        {
            _output.WriteLine("Test omitido: Esquemas XSD no disponibles");
            return;
        }

        var validationService = new XmlValidationService(_xsdPath);
        var errores = new List<string>();

        // Act
        var resultado = validationService.ValidarContraXsd(xmlDoc, (sender, e) =>
        {
            errores.Add($"{e.Severity}: {e.Message}");
        });

        // Assert
        Assert.True(resultado,
            $"Registro con múltiple desglose debe ser válido. Errores: {string.Join("; ", errores)}");
    }

    [Fact]
    public void ValidarContraXsd_SinDestinatario_ValidoSiXsdDisponible()
    {
        // Arrange - Factura simplificada sin destinatario
        var registro = CrearRegistroEjemplo() with
        {
            Destinatario = null
        };

        var serializer = new VerifactuSerializer();
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        
        if (!_xsdDisponibles)
        {
            _output.WriteLine("Test omitido: Esquemas XSD no disponibles");
            return;
        }

        var validationService = new XmlValidationService(_xsdPath);
        var errores = new List<string>();

        // Act
        var resultado = validationService.ValidarContraXsd(xmlDoc, (sender, e) =>
        {
            errores.Add($"{e.Severity}: {e.Message}");
        });

        // Assert
        Assert.True(resultado,
            $"Factura sin destinatario debe ser válida. Errores: {string.Join("; ", errores)}");
    }

    [Fact]
    public void ValidarContraXsd_VerificarNamespace()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        
        var namespaceEsperado = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd";

        // Act
        var namespaceActual = xmlDoc.DocumentElement?.NamespaceURI;

        // Assert
        Assert.Equal(namespaceEsperado, namespaceActual);
        _output.WriteLine($"✓ Namespace correcto: {namespaceActual}");
    }

    [Fact]
    public void ValidarContraXsd_EsquemasCargadosCorrectamente()
    {
        if (!_xsdDisponibles)
        {
            _output.WriteLine("Test omitido: Esquemas XSD no disponibles");
            return;
        }

        // Arrange & Act
        var validationService = new XmlValidationService(_xsdPath);
        var advertencias = validationService.ObtenerAdvertencias();

        // Assert - No debe haber advertencias sobre esquemas faltantes
        var advertenciasFaltantes = advertencias.Where(a => 
            a.Contains("No se encontraron") || 
            a.Contains("no encontrado")).ToList();

        Assert.Empty(advertenciasFaltantes);
        
        _output.WriteLine("✓ Esquemas XSD cargados sin advertencias");
        _output.WriteLine($"  Ruta: {_xsdPath}");
    }

    [Fact]
    public void ValidarContraXsd_FormatosFechaCorrectos()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();
        var xmlDoc = serializer.CrearXmlRegistro(registro);

        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", 
            "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd");

        // Act
        var fechaExpedicion = xmlDoc.SelectSingleNode("//sum1:FechaExpedicionFactura", nsmgr)?.InnerText;
        var fechaHoraGen = xmlDoc.SelectSingleNode("//sum1:FechaHoraHusoGenRegistro", nsmgr)?.InnerText;

        // Assert
        Assert.NotNull(fechaExpedicion);
        Assert.NotNull(fechaHoraGen);
        
        // Formato dd-MM-yyyy
        Assert.Matches(@"^\d{2}-\d{2}-\d{4}$", fechaExpedicion);
        
        // Formato ISO 8601 con zona horaria
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+-]\d{2}:\d{2}$", fechaHoraGen);
        
        _output.WriteLine($"✓ FechaExpedicionFactura: {fechaExpedicion} (formato dd-MM-yyyy)");
        _output.WriteLine($"✓ FechaHoraHusoGenRegistro: {fechaHoraGen} (formato ISO 8601)");
    }

    [Fact]
    public void ValidarContraXsd_FormatosNumericosCorrectos()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();
        var xmlDoc = serializer.CrearXmlRegistro(registro);

        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", 
            "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd");

        // Act
        var cuotaTotal = xmlDoc.SelectSingleNode("//sum1:CuotaTotal", nsmgr)?.InnerText;
        var importeTotal = xmlDoc.SelectSingleNode("//sum1:ImporteTotal", nsmgr)?.InnerText;
        var tipoImpositivo = xmlDoc.SelectSingleNode("//sum1:TipoImpositivo", nsmgr)?.InnerText;

        // Assert - Deben tener formato decimal con punto y 2 decimales
        Assert.NotNull(cuotaTotal);
        Assert.NotNull(importeTotal);
        Assert.NotNull(tipoImpositivo);
        
        Assert.Matches(@"^\d+\.\d{2}$", cuotaTotal);
        Assert.Matches(@"^\d+\.\d{2}$", importeTotal);
        Assert.Matches(@"^\d+\.\d{2}$", tipoImpositivo);
        
        _output.WriteLine($"✓ CuotaTotal: {cuotaTotal}");
        _output.WriteLine($"✓ ImporteTotal: {importeTotal}");
        _output.WriteLine($"✓ TipoImpositivo: {tipoImpositivo}");
    }
}
