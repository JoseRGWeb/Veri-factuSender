# Algoritmo de Huella (Hash) Oficial AEAT para VERI*FACTU

> **Versión:** 1.0  
> **Fecha:** 07/11/2025  
> **Estado:** Implementación conforme a especificación oficial AEAT

## 📋 Resumen

Este documento detalla la implementación del algoritmo de cálculo de huella (hash) SHA-256 para registros de facturación VERI*FACTU, conforme a la especificación oficial de la Agencia Estatal de Administración Tributaria (AEAT).

La huella es un componente **crítico** del sistema VERI*FACTU que garantiza:
- **Integridad**: Cualquier modificación del registro invalida la huella
- **Trazabilidad**: Encadenamiento cronológico de registros
- **No repudio**: Evidencia criptográfica del contenido del registro

## 🔐 Especificación del Algoritmo

### Algoritmo Criptográfico

- **Algoritmo**: SHA-256 (Secure Hash Algorithm 256-bit)
- **Codificación**: UTF-8 sin BOM (Byte Order Mark)
- **Formato salida**: Hexadecimal en MAYÚSCULAS
- **Longitud**: 64 caracteres (256 bits / 4 bits por carácter hex)

### Orden Exacto de Campos

Los campos se concatenan **SIN SEPARADORES** en el siguiente orden:

| # | Campo | Formato | Ejemplo | Notas |
|---|-------|---------|---------|-------|
| 1 | `IDVersion` | Texto | `1.0` | Versión del esquema XSD |
| 2 | `IDEmisorFactura` | NIF | `B12345678` | NIF del obligado emisor |
| 3 | `NumSerieFactura` | Texto | `A/2024/001` | Serie+Número completo |
| 4 | `FechaExpedicionFactura` | dd-MM-yyyy | `13-09-2024` | Solo fecha, sin hora |
| 5 | `TipoFactura` | Código | `F1` | Tipo según catálogo AEAT |
| 6 | `CuotaTotal` | Decimal | `21.00` | 2 decimales, punto |
| 7 | `ImporteTotal` | Decimal | `121.00` | 2 decimales, punto |
| 8 | `HuellaAnterior` | Hex | `ABC...789` o vacío | Hash del registro anterior |
| 9 | `FechaHoraHusoGenRegistro` | ISO 8601 | `2024-09-13T19:20:30+01:00` | Con huso horario |

### Reglas de Normalización

#### Decimales (CuotaTotal, ImporteTotal)

```csharp
// Formato: 2 decimales exactos, punto como separador, sin agrupación de miles
decimal importe = 1234.5m;
string normalizado = importe.ToString("F2", CultureInfo.InvariantCulture);
// Resultado: "1234.50"
```

**Reglas**:
- Siempre 2 decimales (incluso si son .00)
- Punto (`.`) como separador decimal (nunca coma)
- Sin separador de miles
- Usar `CultureInfo.InvariantCulture`

#### Fecha de Expedición

```csharp
DateTime fecha = new DateTime(2024, 9, 13);
string normalizada = fecha.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
// Resultado: "13-09-2024"
```

**Reglas**:
- Formato: `dd-MM-yyyy` (día-mes-año)
- Guiones (`-`) como separadores
- Siempre 2 dígitos para día y mes, 4 para año
- **Solo la fecha**, la hora NO se incluye

#### Fecha/Hora de Generación del Registro

```csharp
DateTime fechaHora = new DateTime(2024, 9, 13, 19, 20, 30, DateTimeKind.Local);
string normalizada = fechaHora.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
// Resultado: "2024-09-13T19:20:30+01:00" (si huso = UTC+1)
```

**Reglas**:
- Formato ISO 8601: `yyyy-MM-ddTHH:mm:sszzz`
- Incluye huso horario (`+01:00`, `+00:00`, etc.)
- La `T` separa fecha y hora
- Los dos puntos (`:`) en el huso son obligatorios

#### Huella Anterior

```csharp
// Para el primer registro de la cadena
string huellaAnterior = string.Empty;  // Cadena vacía

// Para registros posteriores
string huellaAnterior = "ABC123...";   // Huella del registro anterior
```

**Reglas**:
- Primer registro: cadena vacía (`""`)
- Registros posteriores: huella completa (64 caracteres hex mayúsculas)
- `null` se trata como cadena vacía

## 📝 Ejemplo Completo

### Datos de Entrada

```csharp
var registro = new RegistroFacturacion
{
    IDVersion = "1.0",
    IDEmisorFactura = "B12345678",
    NumSerieFactura = "A/2024/001",
    FechaExpedicionFactura = new DateTime(2024, 9, 13),
    TipoFactura = "F1",
    CuotaTotal = 21.00m,
    ImporteTotal = 121.00m,
    FechaHoraHusoGenRegistro = new DateTime(2024, 9, 13, 19, 20, 30, DateTimeKind.Local)
    // ... otros campos
};
```

### Cadena a Hashear (sin huella anterior)

```
1.0B12345678A/2024/00113-09-2024F121.00121.002024-09-13T19:20:30+01:00
```

**Desglose**:
- `1.0` - IDVersion
- `B12345678` - IDEmisorFactura
- `A/2024/001` - NumSerieFactura
- `13-09-2024` - FechaExpedicionFactura
- `F1` - TipoFactura
- `21.00` - CuotaTotal
- `121.00` - ImporteTotal
- *(vacío)* - HuellaAnterior
- `2024-09-13T19:20:30+01:00` - FechaHoraHusoGenRegistro

### Cálculo SHA-256

```csharp
var hashService = new HashService();
string huella = hashService.CalcularHuella(registro, null);
// Resultado (ejemplo): "A1B2C3D4E5F6...789" (64 caracteres hex)
```

## 🔗 Encadenamiento de Registros

El encadenamiento garantiza la trazabilidad cronológica:

```
Registro 1 (sin anterior)
  ↓ calcula huella → HUELLA_1

Registro 2 (incluye HUELLA_1)
  ↓ calcula huella → HUELLA_2

Registro 3 (incluye HUELLA_2)
  ↓ calcula huella → HUELLA_3
```

### Ejemplo de Encadenamiento

```csharp
// Registro 1
var reg1 = CrearRegistro("A/001");
string huella1 = hashService.CalcularHuella(reg1, null);

// Registro 2 (encadenado con el 1)
var reg2 = CrearRegistro("A/002");
string huella2 = hashService.CalcularHuella(reg2, huella1);

// Registro 3 (encadenado con el 2)
var reg3 = CrearRegistro("A/003");
string huella3 = hashService.CalcularHuella(reg3, huella2);
```

## ⚠️ Consideraciones Importantes

### Criticidad del Orden

**⚠️ ADVERTENCIA**: El orden de los campos es CRÍTICO. Cambiar el orden invalida completamente la huella y hace que el registro sea rechazado por AEAT.

### Inmutabilidad

Una vez calculada y enviada a AEAT, la huella NO puede modificarse. Si hay errores:
- **Subsanación**: Se genera un nuevo registro con flag `Subsanacion=S`
- **Anulación**: Se genera un registro de anulación

### Huso Horario

El huso horario afecta al cálculo de la huella:
- Usar siempre el huso horario local del sistema de facturación
- Documentar el huso horario usado
- En España peninsular: UTC+1 (invierno) / UTC+2 (verano)

### Precisión Decimal

Importante para evitar discrepancias:
- Siempre redondear a 2 decimales antes del cálculo
- Usar `decimal` en C# (nunca `float` o `double`)
- El formato `F2` garantiza exactamente 2 decimales

## 🧪 Tests y Validación

### Suite de Tests

El proyecto incluye tests exhaustivos en `HashServiceTests.cs`:

1. **Formato básico**: Longitud, formato hexadecimal, mayúsculas
2. **Determinismo**: Mismos datos → misma huella
3. **Sensibilidad**: Pequeños cambios → huella diferente
4. **Normalización**: Decimales, fechas, campos opcionales
5. **Encadenamiento**: Secuencias de registros

### Ejecución de Tests

```bash
cd /ruta/al/proyecto
dotnet test --filter "FullyQualifiedName~HashServiceTests"
```

### Validación Manual

Para verificar manualmente una huella:

```csharp
// 1. Construir la cadena exacta
string cadena = "1.0B12345678A/2024/00113-09-2024F121.00121.002024-09-13T19:20:30+01:00";

// 2. Calcular SHA-256
var bytes = System.Text.Encoding.UTF8.GetBytes(cadena);
var hash = System.Security.Cryptography.SHA256.HashData(bytes);
var huella = Convert.ToHexString(hash);

// 3. Verificar formato
Console.WriteLine($"Huella: {huella}");
Console.WriteLine($"Longitud: {huella.Length}"); // Debe ser 64
```

## 📚 Referencias

- **Documentación oficial AEAT**: `docs/Veri-Factu_Descripcion_SWeb.md`
- **Guía técnica interna**: `docs/Verifactu-Guia-Tecnica.md` (Sección 5)
- **Implementación**: `src/Verifactu.Client/Services/HashService.cs`
- **Tests**: `tests/Verifactu.Client.Tests/HashServiceTests.cs`

## 📞 Soporte

Para dudas sobre el algoritmo de huella:
1. Consultar este documento
2. Revisar tests en `HashServiceTests.cs`
3. Consultar documentación oficial AEAT (enlaces en Guía Técnica)

---

**Última actualización**: 07/11/2025  
**Versión del documento**: 1.0  
**Compatibilidad**: VERI*FACTU v1.0 (AEAT)
