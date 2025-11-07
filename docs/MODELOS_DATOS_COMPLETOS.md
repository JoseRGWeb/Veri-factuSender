# Modelos de Datos Completos VERI*FACTU

Este documento describe todos los modelos de datos implementados según la especificación oficial de VERI*FACTU de la AEAT.

## Índice

1. [Enumeraciones](#enumeraciones)
2. [Modelos Principales](#modelos-principales)
3. [Modelos Complementarios](#modelos-complementarios)
4. [Ejemplos de Uso](#ejemplos-de-uso)

## Enumeraciones

### TipoFactura

Define los tipos de factura según la especificación VERI*FACTU:

| Valor | Descripción |
|-------|-------------|
| `F1` | Factura completa (con desglose) |
| `F2` | Factura simplificada (sin identificación del destinatario - Art. 6.1.d) RD 1619/2012) |
| `F3` | Factura emitida en sustitución de facturas simplificadas facturadas y declaradas |
| `F4` | Asiento resumen de facturas |
| `R1` | Factura rectificativa por error fundado en derecho y Art. 80 Uno, Dos y Seis LIVA |
| `R2` | Factura rectificativa (Art. 80.3) |
| `R3` | Factura rectificativa (Art. 80.4) |
| `R4` | Factura rectificativa (Resto) |
| `R5` | Factura rectificativa en facturas simplificadas |

### TipoRectificativa

Define el tipo de rectificación en facturas rectificativas:

| Valor | Descripción |
|-------|-------------|
| `S` | Por sustitución |
| `I` | Por diferencias |

### CalificacionOperacion

Define la calificación de la operación respecto al IVA:

| Valor | Descripción |
|-------|-------------|
| `S1` | Operación sujeta y no exenta - sin inversión del sujeto pasivo |
| `S2` | Operación sujeta y no exenta - con inversión del sujeto pasivo |
| `S3` | Operación sujeta y exenta |
| `N1` | Operación no sujeta por reglas de localización |
| `N2` | Operación no sujeta por otras causas |

### ClaveRegimenEspecialOTrascendencia

Define los regímenes especiales o trascendencia según la especificación AEAT:

| Valor | Descripción |
|-------|-------------|
| `RegimenGeneral01` | Régimen general |
| `Exportacion02` | Exportación |
| `BienesUsados03` | Régimen especial de bienes usados, objetos de arte, antigüedades y objetos de colección |
| `OroInversion04` | Régimen especial del oro de inversión |
| `AgenciasViajes05` | Régimen especial de las agencias de viajes |
| `GrupoEntidades06` | Régimen especial grupo de entidades en IVA (Nivel Avanzado) |
| `CriterioCaja07` | Régimen especial del criterio de caja |
| `IPSI_IGIC08` | Operaciones sujetas al IPSI / IGIC |
| `AgenciasViajesMediacion09` | Facturación de prestaciones de servicios de agencias de viaje que actúan como mediadoras |
| `CobrosTerceros10` | Cobros por cuenta de terceros |
| `ArrendamientoLocal11` | Operaciones de arrendamiento de local de negocio sujetas a retención |
| `ArrendamientoLocalNoRetencion12` | Operaciones de arrendamiento de local de negocio no sujetas a retención |
| `ArrendamientoLocalMixto13` | Operaciones de arrendamiento de local de negocio sujetas y no sujetas a retención |
| `CertificacionesObra14` | Factura con IVA pendiente de devengo en certificaciones de obra |
| `TractoSucesivo15` | Factura con IVA pendiente de devengo en operaciones de tracto sucesivo |
| `OSS_IOSS17` | Operaciones acogidas a alguno de los regímenes previstos en el Capítulo XI del Título IX |
| `ExportacionesAsimiladas18` | Exportaciones y operaciones asimiladas a exportaciones |
| `RecargoEquivalencia19` | Operaciones acogidas al régimen especial de recargo de equivalencia |

## Modelos Principales

### Factura

Modelo principal que representa una factura con todos los campos obligatorios y opcionales.

**Campos obligatorios:**
- `Serie`: Serie de la factura
- `Numero`: Número de la factura
- `FechaEmision`: Fecha de emisión de la factura
- `TipoFactura`: Tipo de factura (enum TipoFactura)
- `DescripcionOperacion`: Descripción de la operación
- `Emisor`: Información del emisor

**Campos opcionales comunes:**
- `Receptor`: Información del destinatario (obligatorio para F1, opcional para F2)
- `Lineas`: Líneas de factura (uso interno)
- `Totales`: Totales calculados
- `Desglose`: Desglose de IVA
- `Moneda`: Moneda (por defecto "EUR")
- `Observaciones`: Observaciones adicionales
- `FechaOperacion`: Fecha de la operación (si difiere de fecha de emisión)

**Campos para facturas rectificativas (R1-R5):**
- `TipoRectificativa`: Tipo de rectificación (S o I)
- `FacturasRectificadas`: Lista de facturas que se rectifican
- `ImporteRectificacionSustitutiva`: Importe total de la rectificación (para tipo S)

**Campos adicionales:**
- `ClaveRegimenEspecialOTrascendencia`: Régimen especial
- `Destinatarios`: Lista de destinatarios (para facturas con múltiples destinatarios)
- `FacturacionTerceros`: Información de facturación por cuenta de terceros
- `FacturacionDestinatario`: Información del destinatario en facturación por terceros
- `DesgloseIVA`, `DesgloseIGIC`, `DesgloseIRPF`: Desglose detallado de impuestos
- `Macrodato`: Indicador de macrodato
- `RefExterna`: Referencia externa

### RegistroFacturacion

Modelo que representa el registro completo de facturación que se envía a la AEAT.

**Campos obligatorios:**
- `IDVersion`: Versión del esquema (ej: "1.0")
- `IDEmisorFactura`: NIF del emisor
- `NumSerieFactura`: Número de serie de la factura
- `FechaExpedicionFactura`: Fecha de expedición
- `NombreRazonEmisor`: Nombre o razón social del emisor
- `TipoFactura`: Tipo de factura (enum)
- `DescripcionOperacion`: Descripción de la operación
- `Desglose`: Desglose de impuestos
- `CuotaTotal`: Cuota total de impuestos
- `ImporteTotal`: Importe total de la factura
- `FechaHoraHusoGenRegistro`: Fecha y hora de generación del registro
- `TipoHuella`: Tipo de huella (ej: "01" para SHA-256)
- `Huella`: Huella calculada del registro
- `SistemaInformatico`: Información del sistema informático
- `Factura`: Factura original (uso interno)

**Campos opcionales de encadenamiento:**
- `IDEmisorFacturaAnterior`: NIF del emisor de la factura anterior
- `NumSerieFacturaAnterior`: Número de serie de la factura anterior
- `FechaExpedicionFacturaAnterior`: Fecha de expedición de la factura anterior
- `HuellaAnterior`: Huella del registro anterior

**Campos adicionales:**
- Todos los campos opcionales de Factura
- `Subsanacion`: Indica si es una subsanación
- `RechazoPrevio`: Indica si hubo rechazo previo

## Modelos Complementarios

### FacturaRectificada

Identifica una factura que está siendo rectificada.

```csharp
public record FacturaRectificada(
    string NumSerieFactura,           // Número de serie de la factura rectificada
    DateTime FechaExpedicionFactura   // Fecha de expedición de la factura rectificada
);
```

### IDOtro

Identificación extranjera para destinatarios no residentes.

```csharp
public record IDOtro(
    string CodigoPais,    // Código de país ISO 3166-1 alpha-2 (ej: "FR")
    string IDType,        // Tipo de identificación (02, 03, 04, 05, 06, 07)
    string ID             // Número de identificación
);
```

### DetalleIVA

Desglose detallado de IVA por tipo impositivo.

```csharp
public record DetalleIVA(
    decimal BaseImponible,
    decimal TipoImpositivo,
    decimal CuotaRepercutida,
    decimal? TipoRecargoEquivalencia = null,
    decimal? CuotaRecargoEquivalencia = null
);
```

### DetalleIGIC

Desglose de IGIC (Canarias).

```csharp
public record DetalleIGIC(
    decimal BaseImponible,
    decimal TipoImpositivo,
    decimal CuotaRepercutida,
    decimal? TipoRecargoEquivalencia = null,
    decimal? CuotaRecargoEquivalencia = null
);
```

### DetalleIRPF

Desglose de IRPF (retenciones).

```csharp
public record DetalleIRPF(
    decimal BaseImponible,
    decimal TipoRetencion,
    decimal CuotaRetenida
);
```

### FacturacionTerceros

Información de facturación por cuenta de terceros.

```csharp
public record FacturacionTerceros(
    string NIF,
    string NombreRazon
);
```

### DestinatarioCompleto

Información completa del destinatario con soporte para identificación extranjera.

```csharp
public record DestinatarioCompleto(
    string NombreRazon,
    string? NIF = null,
    IDOtro? IDOtro = null,
    string? CodigoPostal = null,
    string? Direccion = null
);
```

## Ejemplos de Uso

### Factura Completa (F1)

```csharp
var factura = new Factura(
    Serie: "A",
    Numero: "001",
    FechaEmision: new DateTime(2024, 2, 15),
    TipoFactura: TipoFactura.F1,
    DescripcionOperacion: "Venta de productos",
    Emisor: new Emisor("B12345678", "EMPRESA SL"),
    Receptor: new Receptor("12345678Z", "CLIENTE SL"),
    Desglose: new List<DetalleDesglose>
    {
        new DetalleDesglose("01", "S1", 21, 100, 21)
    },
    ClaveRegimenEspecialOTrascendencia: ClaveRegimenEspecialOTrascendencia.RegimenGeneral01
);
```

### Factura Simplificada (F2)

```csharp
var factura = new Factura(
    Serie: "S",
    Numero: "001",
    FechaEmision: new DateTime(2024, 2, 15),
    TipoFactura: TipoFactura.F2,
    DescripcionOperacion: "Venta al público",
    Emisor: new Emisor("B12345678", "EMPRESA SL"),
    Desglose: new List<DetalleDesglose>
    {
        new DetalleDesglose("01", "S1", 21, 50, 10.5m)
    }
    // Sin Receptor en factura simplificada
);
```

### Factura Rectificativa (R1)

```csharp
var factura = new Factura(
    Serie: "AR",
    Numero: "001",
    FechaEmision: new DateTime(2024, 2, 15),
    TipoFactura: TipoFactura.R1,
    DescripcionOperacion: "Rectificación por error fundado en derecho",
    Emisor: new Emisor("B12345678", "EMPRESA SL"),
    Receptor: new Receptor("12345678Z", "CLIENTE SL"),
    TipoRectificativa: TipoRectificativa.S,
    FacturasRectificadas: new List<FacturaRectificada>
    {
        new FacturaRectificada("A/2024/001", new DateTime(2024, 1, 15))
    },
    ImporteRectificacionSustitutiva: 242m
);
```

### Factura con Cliente Extranjero

```csharp
var destinatario = new DestinatarioCompleto(
    NombreRazon: "CLIENTE EXTRANJERO",
    IDOtro: new IDOtro("FR", "02", "123456789"),
    CodigoPostal: "75001",
    Direccion: "Rue de la Paix 1"
);

var factura = new Factura(
    Serie: "E",
    Numero: "001",
    FechaEmision: new DateTime(2024, 2, 15),
    TipoFactura: TipoFactura.F1,
    DescripcionOperacion: "Exportación de servicios",
    Emisor: new Emisor("B12345678", "EMPRESA SL"),
    Destinatarios: new List<DestinatarioCompleto> { destinatario },
    ClaveRegimenEspecialOTrascendencia: ClaveRegimenEspecialOTrascendencia.Exportacion02
);
```

### Factura con Retención IRPF

```csharp
var factura = new Factura(
    Serie: "P",
    Numero: "001",
    FechaEmision: new DateTime(2024, 2, 15),
    TipoFactura: TipoFactura.F1,
    DescripcionOperacion: "Servicios profesionales",
    Emisor: new Emisor("12345678Z", "PROFESIONAL AUTÓNOMO"),
    Receptor: new Receptor("B87654321", "EMPRESA CLIENTE SL"),
    DesgloseIRPF: new List<DetalleIRPF>
    {
        new DetalleIRPF(
            BaseImponible: 1000m,
            TipoRetencion: 15m,
            CuotaRetenida: 150m
        )
    }
);
```

## Referencias

- **Documentación oficial AEAT**: `docs/Veri-Factu_Descripcion_SWeb.md`
- **Guía técnica**: `docs/Verifactu-Guia-Tecnica.md`
- **Tests de validación**: `tests/Verifactu.Client.Tests/`
  - `ModelosComplementariosTests.cs`: Tests de enums y modelos complementarios
  - `FacturasRectificativasTests.cs`: Tests de facturas rectificativas y casos de uso

## Notas Importantes

1. **Campos obligatorios**: `TipoFactura` y `DescripcionOperacion` son obligatorios en todas las facturas.

2. **Facturas rectificativas**: Deben incluir `TipoRectificativa` y `FacturasRectificadas`.

3. **Factura simplificada**: En tipo F2, el `Receptor` es opcional.

4. **Identificación extranjera**: Usar `IDOtro` para clientes no residentes en España.

5. **Desglose de impuestos**: Se puede usar tanto `Desglose` (formato básico) como `DesgloseIVA`, `DesgloseIGIC`, `DesgloseIRPF` (formato detallado).

6. **Encadenamiento**: El sistema debe mantener la huella del registro anterior para el encadenamiento correcto.
