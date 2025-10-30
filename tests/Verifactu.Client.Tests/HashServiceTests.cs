using System;
using Verifactu.Client.Models;
using Verifactu.Client.Services;
using Xunit;

public class HashServiceTests
{
    [Fact]
    public void CalculaHuella_NoNula()
    {
        var svc = new HashService();
        var factura = new Factura(
            Serie: "A",
            Numero: "1",
            FechaEmision: DateTime.UtcNow,
            Emisor: new Emisor("B12345678", "JRWEB, S.L.U."),
            Receptor: new Receptor("12345678Z", "Cliente"),
            Lineas: new(),
            Totales: new TotalesFactura(100, 21, 121)
        );
        var reg = new RegistroFacturacion(Guid.NewGuid().ToString(), DateTime.UtcNow, "A", "1", "", "", factura);
        var h = svc.CalcularHuella(reg, null);
        Assert.False(string.IsNullOrWhiteSpace(h));
    }
}
