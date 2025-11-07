using System;
using System.Collections.Generic;
using System.Globalization;
using Verifactu.Client.Models;
using Verifactu.Client.Services;
using Xunit;

public class HashServiceTests
{
    private readonly HashService _hashService;

    public HashServiceTests()
    {
        _hashService = new HashService();
    }

    /// <summary>
    /// Test básico: verifica que la huella no sea nula o vacía
    /// </summary>
    [Fact]
    public void CalculaHuella_NoNula()
    {
        var registro = CrearRegistroPrueba();
        var huella = _hashService.CalcularHuella(registro, null);
        
        Assert.False(string.IsNullOrWhiteSpace(huella));
    }

    /// <summary>
    /// Test: verifica que la huella esté en formato hexadecimal mayúsculas
    /// SHA-256 genera 64 caracteres hexadecimales (256 bits / 4 bits por char hex)
    /// </summary>
    [Fact]
    public void CalculaHuella_FormatoHexadecimalMayusculas()
    {
        var registro = CrearRegistroPrueba();
        var huella = _hashService.CalcularHuella(registro, null);
        
        // SHA-256 debe generar exactamente 64 caracteres hexadecimales
        Assert.Equal(64, huella.Length);
        
        // Debe estar en mayúsculas
        Assert.Equal(huella, huella.ToUpperInvariant());
        
        // Debe contener solo caracteres hexadecimales (0-9, A-F)
        Assert.Matches("^[0-9A-F]+$", huella);
    }

    /// <summary>
    /// Test: verifica que registros idénticos generen la misma huella (determinismo)
    /// </summary>
    [Fact]
    public void CalculaHuella_Determinista()
    {
        var registro1 = CrearRegistroPrueba();
        var registro2 = CrearRegistroPrueba();
        
        var huella1 = _hashService.CalcularHuella(registro1, null);
        var huella2 = _hashService.CalcularHuella(registro2, null);
        
        Assert.Equal(huella1, huella2);
    }

    /// <summary>
    /// Test: verifica que pequeños cambios generen huellas completamente diferentes
    /// </summary>
    [Fact]
    public void CalculaHuella_CambiosPequeñosGeneranHuellasDiferentes()
    {
        var registro1 = CrearRegistroPrueba();
        var registro2 = CrearRegistroPrueba() with { ImporteTotal = 121.01m }; // Cambio de 1 céntimo
        
        var huella1 = _hashService.CalcularHuella(registro1, null);
        var huella2 = _hashService.CalcularHuella(registro2, null);
        
        Assert.NotEqual(huella1, huella2);
    }

    /// <summary>
    /// Test: verifica normalización correcta de decimales (2 decimales, punto)
    /// </summary>
    [Fact]
    public void CalculaHuella_NormalizacionDecimales()
    {
        // Casos de prueba con diferentes valores decimales
        var registro1 = CrearRegistroPrueba() with { ImporteTotal = 100.00m };
        var registro2 = CrearRegistroPrueba() with { ImporteTotal = 100m }; // Sin decimales explícitos
        
        var huella1 = _hashService.CalcularHuella(registro1, null);
        var huella2 = _hashService.CalcularHuella(registro2, null);
        
        // Deben generar la misma huella porque 100.00 == 100
        Assert.Equal(huella1, huella2);
    }

    /// <summary>
    /// Test: verifica que decimales con más de 2 posiciones se normalicen correctamente
    /// </summary>
    [Fact]
    public void CalculaHuella_DecimalesConMasPrecision()
    {
        var registro1 = CrearRegistroPrueba() with { ImporteTotal = 121.456m };
        var registro2 = CrearRegistroPrueba() with { ImporteTotal = 121.46m }; // Redondeado a 2 decimales
        
        var huella1 = _hashService.CalcularHuella(registro1, null);
        var huella2 = _hashService.CalcularHuella(registro2, null);
        
        // Deben generar la misma huella (redondeo a 2 decimales)
        Assert.Equal(huella1, huella2);
    }

    /// <summary>
    /// Test: verifica formato de fecha de expedición (dd-MM-yyyy)
    /// </summary>
    [Fact]
    public void CalculaHuella_FormatoFechaExpedicion()
    {
        var fecha1 = new DateTime(2024, 9, 13, 10, 30, 0, DateTimeKind.Utc);
        var fecha2 = new DateTime(2024, 9, 13, 15, 45, 30, DateTimeKind.Utc); // Misma fecha, diferente hora
        
        var registro1 = CrearRegistroPrueba() with { FechaExpedicionFactura = fecha1 };
        var registro2 = CrearRegistroPrueba() with { FechaExpedicionFactura = fecha2 };
        
        var huella1 = _hashService.CalcularHuella(registro1, null);
        var huella2 = _hashService.CalcularHuella(registro2, null);
        
        // Deben generar la misma huella (solo se usa dd-MM-yyyy, no la hora)
        Assert.Equal(huella1, huella2);
    }

    /// <summary>
    /// Test: verifica encadenamiento con registro anterior
    /// </summary>
    [Fact]
    public void CalculaHuella_EncadenamientoConHuellaAnterior()
    {
        var registro = CrearRegistroPrueba();
        
        var huellaSinAnterior = _hashService.CalcularHuella(registro, null);
        var huellaConAnterior = _hashService.CalcularHuella(registro, "HUELLA_ANTERIOR_EJEMPLO");
        
        // Deben ser diferentes (la huella anterior afecta al cálculo)
        Assert.NotEqual(huellaSinAnterior, huellaConAnterior);
    }

    /// <summary>
    /// Test: verifica que huella anterior null y cadena vacía generen el mismo resultado
    /// </summary>
    [Fact]
    public void CalculaHuella_HuellaAnteriorNullYVacia()
    {
        var registro = CrearRegistroPrueba();
        
        var huellaConNull = _hashService.CalcularHuella(registro, null);
        var huellaConVacia = _hashService.CalcularHuella(registro, string.Empty);
        
        // Deben generar la misma huella
        Assert.Equal(huellaConNull, huellaConVacia);
    }

    /// <summary>
    /// Test: verifica formato ISO 8601 de fecha/hora generación con huso horario
    /// </summary>
    [Fact]
    public void CalculaHuella_FormatoFechaHoraHusoGenRegistro()
    {
        // Crear dos registros con la misma fecha/hora pero en diferentes husos horarios
        var fechaUtc = new DateTime(2024, 9, 13, 18, 20, 30, DateTimeKind.Utc);
        var fechaLocal = new DateTimeOffset(2024, 9, 13, 19, 20, 30, TimeSpan.FromHours(1)); // UTC+1
        
        var registro1 = CrearRegistroPrueba() with 
        { 
            FechaHoraHusoGenRegistro = fechaUtc 
        };
        
        var registro2 = CrearRegistroPrueba() with 
        { 
            FechaHoraHusoGenRegistro = fechaLocal.DateTime 
        };
        
        var huella1 = _hashService.CalcularHuella(registro1, null);
        var huella2 = _hashService.CalcularHuella(registro2, null);
        
        // Las huellas deben ser diferentes porque aunque representan el mismo instante,
        // el huso horario se incluye en la cadena a hashear
        Assert.NotEqual(huella1, huella2);
    }

    /// <summary>
    /// Test: vector de prueba con valores conocidos
    /// Este test documenta un ejemplo completo de cálculo de huella
    /// </summary>
    [Fact]
    public void CalculaHuella_VectorPruebaConocido()
    {
        // Crear un registro con valores específicos y conocidos
        var fechaExpedicion = new DateTime(2024, 9, 13, 0, 0, 0, DateTimeKind.Utc);
        var fechaGenRegistro = new DateTime(2024, 9, 13, 19, 20, 30, DateTimeKind.Local);
        
        var registro = CrearRegistroPrueba() with
        {
            IDVersion = "1.0",
            IDEmisorFactura = "B12345678",
            NumSerieFactura = "A/2024/001",
            FechaExpedicionFactura = fechaExpedicion,
            TipoFactura = TipoFactura.F1,
            CuotaTotal = 21.00m,
            ImporteTotal = 121.00m,
            FechaHoraHusoGenRegistro = fechaGenRegistro
        };
        
        var huella = _hashService.CalcularHuella(registro, null);
        
        // Verificar que la huella se genera correctamente
        Assert.NotNull(huella);
        Assert.Equal(64, huella.Length);
        Assert.Matches("^[0-9A-F]+$", huella);
        
        // Nota: El valor exacto de la huella dependerá del huso horario local
        // al ejecutar el test, por lo que no podemos validar un hash específico
        // sin mockear el huso horario del sistema
    }

    /// <summary>
    /// Test: verifica el encadenamiento secuencial de múltiples registros
    /// </summary>
    [Fact]
    public void CalculaHuella_EncadenamientoSecuencial()
    {
        var registro1 = CrearRegistroPrueba() with { NumSerieFactura = "A/001" };
        var registro2 = CrearRegistroPrueba() with { NumSerieFactura = "A/002" };
        var registro3 = CrearRegistroPrueba() with { NumSerieFactura = "A/003" };
        
        // Calcular huella del primer registro (sin huella anterior)
        var huella1 = _hashService.CalcularHuella(registro1, null);
        
        // Calcular huella del segundo registro (con huella del primero)
        var huella2 = _hashService.CalcularHuella(registro2, huella1);
        
        // Calcular huella del tercer registro (con huella del segundo)
        var huella3 = _hashService.CalcularHuella(registro3, huella2);
        
        // Todas las huellas deben ser diferentes
        Assert.NotEqual(huella1, huella2);
        Assert.NotEqual(huella2, huella3);
        Assert.NotEqual(huella1, huella3);
        
        // Recalcular las huellas debe dar el mismo resultado (determinismo)
        Assert.Equal(huella1, _hashService.CalcularHuella(registro1, null));
        Assert.Equal(huella2, _hashService.CalcularHuella(registro2, huella1));
        Assert.Equal(huella3, _hashService.CalcularHuella(registro3, huella2));
    }

    /// <summary>
    /// Método auxiliar para crear un registro de prueba con valores por defecto
    /// </summary>
    private static RegistroFacturacion CrearRegistroPrueba()
    {
        var factura = new Factura(
            Serie: "A",
            Numero: "1",
            FechaEmision: new DateTime(2024, 9, 13, 0, 0, 0, DateTimeKind.Utc),
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Operación de prueba",
            Emisor: new Emisor("B12345678", "EMPRESA TEST SL"),
            Receptor: new Receptor("12345678Z", "CLIENTE TEST"),
            Lineas: new(),
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
            NombreRazon: "EMPRESA TEST SL",
            Nif: "B12345678",
            NombreSistemaInformatico: "VerifactuSender",
            IdSistemaInformatico: "1",
            Version: "1.0",
            NumeroInstalacion: "1"
        );
        
        return new RegistroFacturacion(
            IDVersion: "1.0",
            IDEmisorFactura: "B12345678",
            NumSerieFactura: "A/2024/001",
            FechaExpedicionFactura: new DateTime(2024, 9, 13, 0, 0, 0, DateTimeKind.Utc),
            NombreRazonEmisor: "EMPRESA TEST SL",
            TipoFactura: TipoFactura.F1,
            DescripcionOperacion: "Operación de prueba",
            Desglose: desglose,
            CuotaTotal: 21.00m,
            ImporteTotal: 121.00m,
            FechaHoraHusoGenRegistro: new DateTime(2024, 9, 13, 19, 20, 30, DateTimeKind.Local),
            TipoHuella: "01",
            Huella: "",
            SistemaInformatico: sistemaInfo,
            Factura: factura
        );
    }
}
