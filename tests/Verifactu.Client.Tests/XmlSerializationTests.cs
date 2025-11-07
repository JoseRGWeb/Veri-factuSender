using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Verifactu.Client.Models;
using Verifactu.Client.Services;
using Xunit;

namespace Verifactu.Client.Tests;

/// <summary>
/// Tests para validar la serialización XML conforme a XSD oficial de AEAT
/// </summary>
public class XmlSerializationTests
{
    // Namespace oficial de AEAT para SuministroInformacion
    private const string NsAeat = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd";

    private RegistroFacturacion CrearRegistroEjemplo()
    {
        var factura = new Factura(
            Serie: "A",
            Numero: "123",
            FechaEmision: new DateTime(2024, 2, 13),
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Venta de productos",
            Emisor: new Emisor("B12345678", "EMPRESA TEST SL"),
            Receptor: new Receptor("12345678Z", "CLIENTE TEST"),
            Lineas: new List<Linea>
            {
                new Linea("Producto 1", 1, 100, 21),
                new Linea("Producto 2", 2, 50, 21)
            },
            Totales: new TotalesFactura(200, 42, 242)
        );

        var desglose = new List<DetalleDesglose>
        {
            new DetalleDesglose(
                ClaveRegimen: "01",
                CalificacionOperacion: "S1",
                TipoImpositivo: 21,
                BaseImponible: 200,
                CuotaRepercutida: 42
            )
        };

        var sistemaInfo = new SistemaInformatico(
            NombreRazon: "DESARROLLADOR SL",
            Nif: "B87654321",
            NombreSistemaInformatico: "VerifactuSender",
            IdSistemaInformatico: "1",
            Version: "1.0.0",
            NumeroInstalacion: "1"
        );

        return new RegistroFacturacion(
            IDVersion: "1.0",
            IDEmisorFactura: "B12345678",
            NumSerieFactura: "A123",
            FechaExpedicionFactura: new DateTime(2024, 2, 13),
            NombreRazonEmisor: "EMPRESA TEST SL",
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Venta de productos",
            Desglose: desglose,
            CuotaTotal: 42,
            ImporteTotal: 242,
            FechaHoraHusoGenRegistro: new DateTime(2024, 2, 13, 19, 20, 30, DateTimeKind.Local),
            TipoHuella: "01",
            Huella: "ABCDEF1234567890",
            SistemaInformatico: sistemaInfo,
            Factura: factura,
            Destinatario: factura.Receptor,
            HuellaAnterior: "0000000000000000"
        );
    }

    [Fact]
    public void CrearXmlRegistro_GeneraXmlValido()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);

        // Assert
        Assert.NotNull(xmlDoc);
        Assert.NotNull(xmlDoc.DocumentElement);
    }

    [Fact]
    public void CrearXmlRegistro_UsaNamespaceOficial()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();
        var expectedNamespace = NsAeat;

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);

        // Assert
        Assert.NotNull(xmlDoc.DocumentElement);
        Assert.Equal(expectedNamespace, xmlDoc.DocumentElement.NamespaceURI);
        Assert.Equal("RegistroAlta", xmlDoc.DocumentElement.LocalName);
    }

    [Fact]
    public void CrearXmlRegistro_ContieneElementosObligatorios()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();
        var ns = NsAeat;

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", ns);

        // Assert - Verificar elementos obligatorios según XSD
        Assert.NotNull(xmlDoc.SelectSingleNode("//sum1:IDVersion", nsmgr));
        Assert.NotNull(xmlDoc.SelectSingleNode("//sum1:IDFactura", nsmgr));
        Assert.NotNull(xmlDoc.SelectSingleNode("//sum1:NombreRazonEmisor", nsmgr));
        Assert.NotNull(xmlDoc.SelectSingleNode("//sum1:TipoFactura", nsmgr));
        Assert.NotNull(xmlDoc.SelectSingleNode("//sum1:DescripcionOperacion", nsmgr));
        Assert.NotNull(xmlDoc.SelectSingleNode("//sum1:Desglose", nsmgr));
        Assert.NotNull(xmlDoc.SelectSingleNode("//sum1:CuotaTotal", nsmgr));
        Assert.NotNull(xmlDoc.SelectSingleNode("//sum1:ImporteTotal", nsmgr));
        Assert.NotNull(xmlDoc.SelectSingleNode("//sum1:SistemaInformatico", nsmgr));
        Assert.NotNull(xmlDoc.SelectSingleNode("//sum1:FechaHoraHusoGenRegistro", nsmgr));
        Assert.NotNull(xmlDoc.SelectSingleNode("//sum1:TipoHuella", nsmgr));
        Assert.NotNull(xmlDoc.SelectSingleNode("//sum1:Huella", nsmgr));
    }

    [Fact]
    public void CrearXmlRegistro_IDFacturaContieneElementosCorrectos()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();
        var ns = NsAeat;

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", ns);

        // Assert - Verificar estructura de IDFactura
        var idEmisor = xmlDoc.SelectSingleNode("//sum1:IDFactura/sum1:IDEmisorFactura", nsmgr);
        var numSerie = xmlDoc.SelectSingleNode("//sum1:IDFactura/sum1:NumSerieFactura", nsmgr);
        var fechaExpedicion = xmlDoc.SelectSingleNode("//sum1:IDFactura/sum1:FechaExpedicionFactura", nsmgr);

        Assert.NotNull(idEmisor);
        Assert.Equal("B12345678", idEmisor.InnerText);
        Assert.NotNull(numSerie);
        Assert.Equal("A123", numSerie.InnerText);
        Assert.NotNull(fechaExpedicion);
        Assert.Equal("13-02-2024", fechaExpedicion.InnerText);
    }

    [Fact]
    public void CrearXmlRegistro_DesgloseContieneDetalles()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();
        var ns = NsAeat;

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", ns);

        // Assert - Verificar desglose
        var detalles = xmlDoc.SelectNodes("//sum1:Desglose/sum1:DetalleDesglose", nsmgr);
        Assert.NotNull(detalles);
        Assert.True(detalles.Count > 0);

        var primerDetalle = detalles[0];
        Assert.NotNull(primerDetalle?.SelectSingleNode("sum1:ClaveRegimen", nsmgr));
        Assert.NotNull(primerDetalle?.SelectSingleNode("sum1:CalificacionOperacion", nsmgr));
        Assert.NotNull(primerDetalle?.SelectSingleNode("sum1:TipoImpositivo", nsmgr));
        Assert.NotNull(primerDetalle?.SelectSingleNode("sum1:BaseImponibleOimporteNoSujeto", nsmgr));
        Assert.NotNull(primerDetalle?.SelectSingleNode("sum1:CuotaRepercutida", nsmgr));
    }

    [Fact]
    public void CrearXmlRegistro_EncadenamientoSiHayHuellaAnterior()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();
        var ns = NsAeat;

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", ns);

        // Assert - Verificar encadenamiento
        var encadenamiento = xmlDoc.SelectSingleNode("//sum1:Encadenamiento", nsmgr);
        Assert.NotNull(encadenamiento);

        var huellaAnterior = xmlDoc.SelectSingleNode("//sum1:Encadenamiento/sum1:RegistroAnterior/sum1:Huella", nsmgr);
        Assert.NotNull(huellaAnterior);
        Assert.Equal("0000000000000000", huellaAnterior.InnerText);
    }

    [Fact]
    public void CrearXmlRegistro_SistemaInformaticoCompleto()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();
        var ns = NsAeat;

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", ns);

        // Assert - Verificar SistemaInformatico
        var sistema = xmlDoc.SelectSingleNode("//sum1:SistemaInformatico", nsmgr);
        Assert.NotNull(sistema);

        Assert.NotNull(sistema.SelectSingleNode("sum1:NombreRazon", nsmgr));
        Assert.NotNull(sistema.SelectSingleNode("sum1:NIF", nsmgr));
        Assert.NotNull(sistema.SelectSingleNode("sum1:NombreSistemaInformatico", nsmgr));
        Assert.NotNull(sistema.SelectSingleNode("sum1:IdSistemaInformatico", nsmgr));
        Assert.NotNull(sistema.SelectSingleNode("sum1:Version", nsmgr));
        Assert.NotNull(sistema.SelectSingleNode("sum1:NumeroInstalacion", nsmgr));
    }

    [Fact]
    public void CrearXmlRegistro_FormatosNumericosCorrectos()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();
        var ns = NsAeat;

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", ns);

        // Assert - Verificar formatos numéricos (deben tener 2 decimales)
        var cuotaTotal = xmlDoc.SelectSingleNode("//sum1:CuotaTotal", nsmgr);
        var importeTotal = xmlDoc.SelectSingleNode("//sum1:ImporteTotal", nsmgr);
        
        Assert.NotNull(cuotaTotal);
        Assert.NotNull(importeTotal);
        
        // Verificar que tienen formato decimal con punto
        Assert.Contains(".", cuotaTotal.InnerText);
        Assert.Contains(".", importeTotal.InnerText);
    }

    [Fact]
    public void XmlValidationService_SePuedeInstanciar()
    {
        // Arrange & Act
        var validationService = new XmlValidationService();

        // Assert
        Assert.NotNull(validationService);
    }

    [Fact]
    public void XmlValidationService_NoFallaSinXsd()
    {
        // Arrange
        var validationService = new XmlValidationService("/ruta/inexistente");
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();
        var xmlDoc = serializer.CrearXmlRegistro(registro);

        // Act
        var resultado = validationService.ValidarContraXsd(xmlDoc);

        // Assert - No debe fallar si no hay XSD, solo advertir
        Assert.True(resultado);
    }

    [Theory]
    [InlineData(TipoFactura.F1, "Factura completa")]
    [InlineData(TipoFactura.F2, "Factura simplificada")]
    [InlineData(TipoFactura.F3, "Factura sustitución")]
    [InlineData(TipoFactura.F4, "Asiento resumen")]
    [InlineData(TipoFactura.R1, "Rectificativa error fundado")]
    [InlineData(TipoFactura.R2, "Rectificativa Art. 80.3")]
    [InlineData(TipoFactura.R3, "Rectificativa Art. 80.4")]
    [InlineData(TipoFactura.R4, "Rectificativa resto")]
    [InlineData(TipoFactura.R5, "Rectificativa simplificada")]
    public void CrearXmlRegistro_TodosTiposFactura_GeneranXmlValido(TipoFactura tipo, string descripcion)
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo() with { TipoFactura = tipo };

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        var ns = NsAeat;
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", ns);

        // Assert
        Assert.NotNull(xmlDoc);
        Assert.NotNull(xmlDoc.DocumentElement);
        
        var tipoFacturaNode = xmlDoc.SelectSingleNode("//sum1:TipoFactura", nsmgr);
        Assert.NotNull(tipoFacturaNode);
        Assert.Equal(tipo.ToString(), tipoFacturaNode.InnerText);
    }

    [Fact]
    public void CrearXmlRegistro_SinEncadenamiento_NoIncluyeElementoEncadenamiento()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo() with { HuellaAnterior = null };
        var ns = NsAeat;

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", ns);

        // Assert
        var encadenamiento = xmlDoc.SelectSingleNode("//sum1:Encadenamiento", nsmgr);
        Assert.Null(encadenamiento);
    }

    [Fact]
    public void CrearXmlRegistro_ConMultipleDesgloses_GeneraTodosLosElementos()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
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
        var ns = NsAeat;

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", ns);

        // Assert
        var detalles = xmlDoc.SelectNodes("//sum1:Desglose/sum1:DetalleDesglose", nsmgr);
        Assert.NotNull(detalles);
        Assert.Equal(3, detalles.Count);
    }

    [Fact]
    public void CrearXmlRegistro_FacturaSimplificadaSinDestinatario_NoIncluyeDestinatarios()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo() with 
        { 
            TipoFactura = TipoFactura.F2,
            Destinatario = null 
        };
        var ns = NsAeat;

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", ns);

        // Assert
        var destinatarios = xmlDoc.SelectSingleNode("//sum1:Destinatarios", nsmgr);
        Assert.Null(destinatarios);
    }

    [Fact]
    public void CrearXmlRegistro_ConDestinatarioSinNIF_GeneraXmlValido()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var destinatarioSinNif = new Receptor(null, "CLIENTE EXTRANJERO");
        var registro = CrearRegistroEjemplo() with { Destinatario = destinatarioSinNif };
        var ns = NsAeat;

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", ns);

        // Assert
        var destinatarios = xmlDoc.SelectSingleNode("//sum1:Destinatarios", nsmgr);
        Assert.NotNull(destinatarios);
        
        var nombreRazon = xmlDoc.SelectSingleNode("//sum1:Destinatarios/sum1:IDDestinatario/sum1:NombreRazon", nsmgr);
        Assert.NotNull(nombreRazon);
        Assert.Equal("CLIENTE EXTRANJERO", nombreRazon.InnerText);
        
        var nif = xmlDoc.SelectSingleNode("//sum1:Destinatarios/sum1:IDDestinatario/sum1:NIF", nsmgr);
        Assert.Null(nif);
    }

    [Fact]
    public void CrearXmlRegistro_ImportesDecimales_FormatoConDosDecimales()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo() with
        {
            CuotaTotal = 123.456m,  // Debe redondearse a 123.46
            ImporteTotal = 654.321m  // Debe redondearse a 654.32
        };
        var ns = NsAeat;

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", ns);

        // Assert
        var cuotaTotal = xmlDoc.SelectSingleNode("//sum1:CuotaTotal", nsmgr);
        var importeTotal = xmlDoc.SelectSingleNode("//sum1:ImporteTotal", nsmgr);
        
        Assert.NotNull(cuotaTotal);
        Assert.NotNull(importeTotal);
        
        // Verificar formato con 2 decimales
        Assert.Equal("123.46", cuotaTotal.InnerText);
        Assert.Equal("654.32", importeTotal.InnerText);
    }

    [Fact]
    public void CrearXmlRegistro_FechaHoraConZonaHoraria_FormatoISO8601()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var fechaHora = new DateTime(2024, 11, 7, 14, 30, 45, DateTimeKind.Local);
        var registro = CrearRegistroEjemplo() with
        {
            FechaHoraHusoGenRegistro = fechaHora
        };
        var ns = NsAeat;

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sum1", ns);

        // Assert
        var fechaHoraNode = xmlDoc.SelectSingleNode("//sum1:FechaHoraHusoGenRegistro", nsmgr);
        Assert.NotNull(fechaHoraNode);
        
        // Debe incluir zona horaria (ej: 2024-11-07T14:30:45+01:00)
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+-]\d{2}:\d{2}$", fechaHoraNode.InnerText);
    }

    [Fact]
    public void CrearXmlRegistro_ElementoRaiz_EsRegistroAlta()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);

        // Assert
        Assert.NotNull(xmlDoc.DocumentElement);
        Assert.Equal("RegistroAlta", xmlDoc.DocumentElement.LocalName);
    }

    [Fact]
    public void CrearXmlRegistro_DeclaracionNamespace_IncluePrefijo()
    {
        // Arrange
        var serializer = new VerifactuSerializer();
        var registro = CrearRegistroEjemplo();

        // Act
        var xmlDoc = serializer.CrearXmlRegistro(registro);

        // Assert
        Assert.NotNull(xmlDoc.DocumentElement);
        var xmlns = xmlDoc.DocumentElement.GetAttribute("xmlns:sum1");
        Assert.Equal(
            NsAeat,
            xmlns);
    }
}
