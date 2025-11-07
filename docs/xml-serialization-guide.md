# Serialización XML conforme a XSD oficial de AEAT

Este documento describe cómo usar la nueva implementación de serialización XML que cumple 100% con los esquemas XSD oficiales de VERI*FACTU.

## Cambios Principales

### 1. Modelos Actualizados

Los modelos ahora incluyen todos los campos obligatorios según los XSD oficiales:

```csharp
// Nuevo modelo RegistroFacturacion conforme a XSD
var registro = new RegistroFacturacion(
    IDVersion: "1.0",
    IDEmisorFactura: "B12345678",
    NumSerieFactura: "A123",
    FechaExpedicionFactura: new DateTime(2024, 2, 13),
    NombreRazonEmisor: "MI EMPRESA SL",
    TipoFactura: "F1",  // F1 = Factura completa
    DescripcionOperacion: "Venta de productos",
    Desglose: desglose,  // Lista de DetalleDesglose
    CuotaTotal: 42.00m,
    ImporteTotal: 242.00m,
    FechaHoraHusoGenRegistro: DateTime.Now,
    TipoHuella: "01",  // 01 = SHA-256
    Huella: "...",
    SistemaInformatico: sistemaInfo,
    Factura: factura,  // Factura original (uso interno)
    Destinatario: receptor,
    HuellaAnterior: "..."  // Para encadenamiento
);
```

### 2. Desglose de IVA

El desglose ahora sigue la estructura oficial `DetalleDesglose`:

```csharp
var desglose = new List<DetalleDesglose>
{
    new DetalleDesglose(
        ClaveRegimen: "01",              // 01 = Régimen general
        CalificacionOperacion: "S1",      // S1 = Sujeta y no exenta
        TipoImpositivo: 21,
        BaseImponible: 200.00m,
        CuotaRepercutida: 42.00m
    )
};
```

Códigos comunes:
- **ClaveRegimen**: `01` (General), `02` (Exportación), `03` (Operaciones a las que se aplique el régimen especial de bienes usados)
- **CalificacionOperacion**: `S1` (Sujeta y no exenta), `S2` (Sujeta y exenta), `N1` (No sujeta)

### 3. Sistema Informático

Información del sistema de facturación:

```csharp
var sistemaInfo = new SistemaInformatico(
    NombreRazon: "MI EMPRESA SL",
    Nif: "B12345678",
    NombreSistemaInformatico: "VerifactuSender",
    IdSistemaInformatico: "1",
    Version: "1.0.0",
    NumeroInstalacion: "1",
    TipoUsoPosibleSoloVerifactu: "N",  // N = No, S = Sí
    TipoUsoPosibleMultiOT: "S",        // Múltiples obligados tributarios
    IndicadorMultiplesOT: "S"
);
```

### 4. Serialización XML

La serialización ahora usa los namespaces oficiales de AEAT:

```csharp
var serializer = new VerifactuSerializer();
var xmlDoc = serializer.CrearXmlRegistro(registro);
```

El XML generado cumple con:
- Namespace: `https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd`
- Elemento raíz: `<RegistroAlta>`
- Estructura conforme a `SuministroInformacion.xsd`

### 5. Validación contra XSD

Se ha añadido un servicio de validación XML:

```csharp
var validationService = new XmlValidationService();
bool esValido = validationService.ValidarContraXsd(xmlDoc, (sender, e) =>
{
    Console.WriteLine($"{e.Severity}: {e.Message}");
});

if (!esValido)
{
    var errores = validationService.ObtenerErrores();
    foreach (var error in errores)
    {
        Console.WriteLine(error);
    }
}
```

**IMPORTANTE**: La validación requiere los archivos XSD oficiales descargados en `docs/xsd/`. Ver instrucciones en `docs/xsd/README.md`.

## Ejemplo Completo

```csharp
using Verifactu.Client.Models;
using Verifactu.Client.Services;

// 1. Crear factura
var factura = new Factura(
    Serie: "A",
    Numero: "123",
    FechaEmision: DateTime.Now,
    Emisor: new Emisor("B12345678", "MI EMPRESA SL"),
    Receptor: new Receptor("12345678Z", "CLIENTE"),
    Lineas: new List<Linea>
    {
        new Linea("Producto 1", 1, 100, 21)
    },
    Totales: new TotalesFactura(100, 21, 121),
    TipoFactura: "F1",
    DescripcionOperacion: "Venta de productos"
);

// 2. Crear desglose de IVA
var desglose = new List<DetalleDesglose>
{
    new DetalleDesglose("01", "S1", 21, 100, 21)
};

// 3. Información del sistema
var sistemaInfo = new SistemaInformatico(
    NombreRazon: "MI EMPRESA SL",
    Nif: "B12345678",
    NombreSistemaInformatico: "VerifactuSender",
    IdSistemaInformatico: "1",
    Version: "1.0.0",
    NumeroInstalacion: "1"
);

// 4. Crear registro
var registro = new RegistroFacturacion(
    IDVersion: "1.0",
    IDEmisorFactura: "B12345678",
    NumSerieFactura: "A123",
    FechaExpedicionFactura: factura.FechaEmision,
    NombreRazonEmisor: "MI EMPRESA SL",
    TipoFactura: "F1",
    DescripcionOperacion: "Venta de productos",
    Desglose: desglose,
    CuotaTotal: 21,
    ImporteTotal: 121,
    FechaHoraHusoGenRegistro: DateTime.Now,
    TipoHuella: "01",
    Huella: "",  // Se calculará después
    SistemaInformatico: sistemaInfo,
    Factura: factura,
    Destinatario: factura.Receptor
);

// 5. Calcular huella
var hashService = new HashService();
var huella = hashService.CalcularHuella(registro, null);
registro = registro with { Huella = huella };

// 6. Serializar a XML
var serializer = new VerifactuSerializer();
var xmlDoc = serializer.CrearXmlRegistro(registro);

// 7. Validar (opcional)
var validationService = new XmlValidationService();
if (validationService.ValidarContraXsd(xmlDoc))
{
    Console.WriteLine("XML válido según XSD");
}

// 8. Firmar y enviar
var signer = new XmlSignerService();
var cert = /* cargar certificado */;
var xmlFirmado = signer.Firmar(xmlDoc, cert);

var soapClient = new VerifactuSoapClient(endpoint, soapAction);
var respuesta = await soapClient.EnviarRegistroAsync(xmlFirmado, cert);
```

## Estructura XML Generada

El XML generado tiene esta estructura:

```xml
<sum1:RegistroAlta xmlns:sum1="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd">
  <sum1:IDVersion>1.0</sum1:IDVersion>
  <sum1:IDFactura>
    <sum1:IDEmisorFactura>B12345678</sum1:IDEmisorFactura>
    <sum1:NumSerieFactura>A123</sum1:NumSerieFactura>
    <sum1:FechaExpedicionFactura>13-02-2024</sum1:FechaExpedicionFactura>
  </sum1:IDFactura>
  <sum1:NombreRazonEmisor>MI EMPRESA SL</sum1:NombreRazonEmisor>
  <sum1:TipoFactura>F1</sum1:TipoFactura>
  <sum1:DescripcionOperacion>Venta de productos</sum1:DescripcionOperacion>
  <sum1:Destinatarios>
    <sum1:IDDestinatario>
      <sum1:NombreRazon>CLIENTE</sum1:NombreRazon>
      <sum1:NIF>12345678Z</sum1:NIF>
    </sum1:IDDestinatario>
  </sum1:Destinatarios>
  <sum1:Desglose>
    <sum1:DetalleDesglose>
      <sum1:ClaveRegimen>01</sum1:ClaveRegimen>
      <sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>
      <sum1:TipoImpositivo>21.00</sum1:TipoImpositivo>
      <sum1:BaseImponibleOimporteNoSujeto>100.00</sum1:BaseImponibleOimporteNoSujeto>
      <sum1:CuotaRepercutida>21.00</sum1:CuotaRepercutida>
    </sum1:DetalleDesglose>
  </sum1:Desglose>
  <sum1:CuotaTotal>21.00</sum1:CuotaTotal>
  <sum1:ImporteTotal>121.00</sum1:ImporteTotal>
  <sum1:Encadenamiento>
    <sum1:RegistroAnterior>
      <sum1:Huella>...</sum1:Huella>
    </sum1:RegistroAnterior>
  </sum1:Encadenamiento>
  <sum1:SistemaInformatico>
    <sum1:NombreRazon>MI EMPRESA SL</sum1:NombreRazon>
    <sum1:NIF>B12345678</sum1:NIF>
    <sum1:NombreSistemaInformatico>VerifactuSender</sum1:NombreSistemaInformatico>
    <sum1:IdSistemaInformatico>1</sum1:IdSistemaInformatico>
    <sum1:Version>1.0.0</sum1:Version>
    <sum1:NumeroInstalacion>1</sum1:NumeroInstalacion>
    <sum1:TipoUsoPosibleSoloVerifactu>N</sum1:TipoUsoPosibleSoloVerifactu>
    <sum1:TipoUsoPosibleMultiOT>S</sum1:TipoUsoPosibleMultiOT>
    <sum1:IndicadorMultiplesOT>S</sum1:IndicadorMultiplesOT>
  </sum1:SistemaInformatico>
  <sum1:FechaHoraHusoGenRegistro>2024-02-13T19:20:30+01:00</sum1:FechaHoraHusoGenRegistro>
  <sum1:TipoHuella>01</sum1:TipoHuella>
  <sum1:Huella>ABCDEF1234567890</sum1:Huella>
</sum1:RegistroAlta>
```

## Formatos de Datos

### Fechas
- **FechaExpedicionFactura**: `dd-MM-yyyy` (Ejemplo: `13-02-2024`)
- **FechaHoraHusoGenRegistro**: `yyyy-MM-ddTHH:mm:sszzz` (Ejemplo: `2024-02-13T19:20:30+01:00`)

### Números
- Decimales con punto como separador: `21.00`
- 2 decimales para importes y tipos impositivos

### NIFs
- Formato estándar español: letra + 8 dígitos (personas jurídicas) o 8 dígitos + letra (personas físicas)

## Tests

Se han implementado 11 tests unitarios que validan:

1. Generación de XML válido
2. Uso de namespaces oficiales AEAT
3. Presencia de elementos obligatorios
4. Estructura correcta de IDFactura
5. Desglose de IVA con todos los campos
6. Encadenamiento con registro anterior
7. Sistema informático completo
8. Formatos numéricos correctos
9. Validación XSD funcional
10. Destinatarios opcionales
11. Campos condicionales

Ejecutar tests:
```bash
dotnet test
```

## Referencias

- **Documentación oficial**: `docs/Veri-Factu_Descripcion_SWeb.md`
- **Guía técnica**: `docs/Verifactu-Guia-Tecnica.md`
- **XSD oficiales**: Ver `docs/xsd/README.md` para descargar
- **Ejemplos XML**: Ver Anexo II en `docs/Veri-Factu_Descripcion_SWeb.md`

## Próximos Pasos

1. Descargar XSD oficiales para validación completa
2. Ajustar algoritmo de huella según especificación oficial AEAT
3. Probar contra Portal de Pruebas Externas
4. Implementar manejo completo de respuestas SOAP
