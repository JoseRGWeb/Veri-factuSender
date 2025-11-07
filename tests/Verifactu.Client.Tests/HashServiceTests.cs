using System;
using System.Collections.Generic;
using Verifactu.Client.Models;
using Verifactu.Client.Services;
using Xunit;

public class HashServiceTests
{
    [Fact]
    public void CalculaHuella_NoNula()
    {
        var svc = new HashService();
        
        // Crear datos de prueba conforme al modelo XSD
        var factura = new Factura(
            Serie: "A",
            Numero: "1",
            FechaEmision: DateTime.UtcNow,
            Emisor: new Emisor("B12345678", "JRWEB, S.L.U."),
            Receptor: new Receptor("12345678Z", "Cliente"),
            Lineas: new(),
            Totales: new TotalesFactura(100, 21, 121),
            TipoFactura: "F1",
            DescripcionOperacion: "Test"
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
            NombreRazon: "Test SL",
            Nif: "B12345678",
            NombreSistemaInformatico: "VerifactuSender",
            IdSistemaInformatico: "1",
            Version: "1.0",
            NumeroInstalacion: "1"
        );
        
        var reg = new RegistroFacturacion(
            IDVersion: "1.0",
            IDEmisorFactura: "B12345678",
            NumSerieFactura: "A1",
            FechaExpedicionFactura: DateTime.UtcNow,
            NombreRazonEmisor: "JRWEB, S.L.U.",
            TipoFactura: "F1",
            DescripcionOperacion: "Test",
            Desglose: desglose,
            CuotaTotal: 21,
            ImporteTotal: 121,
            FechaHoraHusoGenRegistro: DateTime.Now,
            TipoHuella: "01",
            Huella: "",
            SistemaInformatico: sistemaInfo,
            Factura: factura
        );
        
        var h = svc.CalcularHuella(reg, null);
        Assert.False(string.IsNullOrWhiteSpace(h));
    }
}
