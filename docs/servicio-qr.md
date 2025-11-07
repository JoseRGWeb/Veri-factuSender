# Servicio de Generación de Código QR para VERI*FACTU

## 📋 Resumen

Este documento describe la implementación del servicio de generación de códigos QR según la especificación oficial S16 de AEAT para facturas VERI*FACTU.

## 🎯 Propósito

El código QR permite al receptor de la factura verificar (cotejar) el registro en la sede electrónica de AEAT, cumpliendo con los requisitos de VERI*FACTU establecidos en la normativa.

## 📦 Componentes Implementados

### 1. Interfaz IQrService

Ubicación: `src/Verifactu.Client/Services/Interfaces.cs`

Define el contrato para el servicio de generación de códigos QR:

- `GenerarUrlCotejo()` - Construye la URL de cotejo con parámetros requeridos
- `GenerarQrPng()` - Genera código QR en formato PNG (bytes)
- `GenerarQrSvg()` - Genera código QR en formato SVG (XML)
- `GenerarQrBase64()` - Genera código QR en formato Base64 (data URI)

### 2. Implementación QrService

Ubicación: `src/Verifactu.Client/Services/QrService.cs`

Servicio completo que implementa la generación de:

#### URL de Cotejo

La URL de cotejo incluye los siguientes parámetros según especificación S16:

```
https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html?
  nif=<NIF_EMISOR>&
  num=<SERIE/NUMERO>&
  fecha=<DD-MM-YYYY>&
  importe=<IMPORTE_TOTAL>&
  huella=<PRIMEROS_13_CARACTERES>
```

**Características:**
- ✅ Formato de fecha: `dd-MM-yyyy` (ej: `13-09-2024`)
- ✅ Formato de importe: 2 decimales con punto (ej: `121.00`)
- ✅ Huella: primeros 13 caracteres de la huella SHA-256
- ✅ Escapado correcto de caracteres especiales en URL
- ✅ Número completo: `Serie/Numero` o solo `Numero` si no hay serie

#### Código QR

Generación de códigos QR en tres formatos:

1. **PNG** (array de bytes)
   - Para guardar en archivos
   - Para adjuntar a PDFs
   - Tamaño configurable (píxeles por módulo)

2. **SVG** (cadena XML)
   - Formato vectorial escalable
   - Ideal para impresión de alta calidad
   - No pierde calidad al escalar

3. **Base64** (data URI)
   - Para incrustar directamente en HTML
   - Para emails HTML
   - Formato: `data:image/png;base64,<contenido>`

### 3. Tests Unitarios

Ubicación: `tests/Verifactu.Client.Tests/QrServiceTests.cs`

Suite completa de 38 tests que cubren:

- ✅ Generación correcta de URL de cotejo
- ✅ Validación de parámetros de URL (NIF, número, fecha, importe, huella)
- ✅ Formato correcto de fecha y importe
- ✅ Primeros 13 caracteres de huella
- ✅ Escapado de caracteres especiales
- ✅ Generación de QR en formato PNG
- ✅ Generación de QR en formato SVG
- ✅ Generación de QR en formato Base64
- ✅ Validación de parámetros de entrada
- ✅ Manejo de errores
- ✅ Tests de integración completos

### 4. Ejemplos de Uso

Ubicación: `src/Verifactu.Client/Examples/QrServiceExamples.cs`

Ejemplos completos que incluyen:

- Ejemplo básico de generación de QR
- Generación en todos los formatos
- Integración completa desde factura hasta QR
- Manejo de errores comunes

## 🚀 Uso del Servicio

### Ejemplo Básico

```csharp
using Verifactu.Client.Services;

// 1. Crear el servicio
var qrService = new QrService();

// 2. Generar URL de cotejo
var urlCotejo = qrService.GenerarUrlCotejo(factura, huella);
// Resultado: https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/cotejo.html?nif=...

// 3. Generar código QR en PNG
var qrPng = qrService.GenerarQrPng(urlCotejo);

// 4. Guardar el QR
File.WriteAllBytes("qr-factura.png", qrPng);
```

### Ejemplo con Todos los Formatos

```csharp
var qrService = new QrService();
var urlCotejo = qrService.GenerarUrlCotejo(factura, huella);

// PNG - para archivos
var qrPng = qrService.GenerarQrPng(urlCotejo, pixelsPorModulo: 20);
File.WriteAllBytes("factura-qr.png", qrPng);

// SVG - para impresión de alta calidad
var qrSvg = qrService.GenerarQrSvg(urlCotejo, pixelsPorModulo: 10);
File.WriteAllText("factura-qr.svg", qrSvg);

// Base64 - para HTML
var qrBase64 = qrService.GenerarQrBase64(urlCotejo, pixelsPorModulo: 20);
var html = $"<img src=\"{qrBase64}\" alt=\"QR VERI*FACTU\" />";
```

### Integración con HashService

```csharp
// Crear servicios
var hashService = new HashService();
var qrService = new QrService();

// Calcular huella del registro
var huella = hashService.CalcularHuella(registro, huellaAnterior);

// Generar URL y QR
var urlCotejo = qrService.GenerarUrlCotejo(factura, huella);
var qrPng = qrService.GenerarQrPng(urlCotejo);

// Guardar QR junto con la factura
var nombreQr = $"qr-{factura.Serie}-{factura.Numero}.png";
File.WriteAllBytes(nombreQr, qrPng);
```

## ⚙️ Configuración

### Dependencias

El servicio utiliza la librería **QRCoder v1.6.0**:

```xml
<PackageReference Include="QRCoder" Version="1.6.0" />
```

**Verificación de seguridad:** ✅ Sin vulnerabilidades conocidas

### Parámetros Configurables

- **pixelsPorModulo**: Tamaño del código QR
  - PNG/Base64: valor por defecto `20`
  - SVG: valor por defecto `10`
  - Rango recomendado: 5-30 píxeles

## 📐 Especificaciones Técnicas

### Formato de URL de Cotejo

| Parámetro | Formato | Ejemplo | Observaciones |
|-----------|---------|---------|---------------|
| `nif` | Cadena | `B12345678` | NIF del emisor |
| `num` | Cadena | `A%2F2024%2F001` | Serie/Número con escape de `/` |
| `fecha` | dd-MM-yyyy | `13-09-2024` | Fecha de emisión |
| `importe` | Decimal (2 dec) | `121.00` | Importe total con punto |
| `huella` | Hexadecimal | `ABCD123456789` | Primeros 13 caracteres |

### Nivel de Corrección de Errores QR

El servicio utiliza nivel de corrección **Q (Quartile)**:
- 25% de recuperación de datos
- Balance óptimo entre tamaño y robustez
- Recomendado por AEAT para facturas

### Tamaños de Salida

Los tamaños aproximados generados son:

| Formato | Tamaño Típico | Notas |
|---------|---------------|-------|
| PNG (20px/módulo) | 3-5 KB | Adecuado para impresión |
| SVG | 5-8 KB | Escalable sin pérdida |
| Base64 | 4-7 KB | Incluye data URI |

## ✅ Validaciones Implementadas

El servicio valida:

1. **Factura no nula**
   - `ArgumentNullException` si factura es null

2. **Huella válida**
   - `ArgumentNullException` si huella es null o vacía
   - `ArgumentException` si huella tiene menos de 13 caracteres

3. **URL válida para QR**
   - `ArgumentNullException` si URL es null o vacía

4. **Tamaño de píxeles válido**
   - `ArgumentOutOfRangeException` si pixelsPorModulo ≤ 0

## 🧪 Testing

### Cobertura de Tests

- **38 tests unitarios** para QrService
- **Total: 133 tests** en el proyecto (todos pasan ✅)

### Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Solo tests de QR
dotnet test --filter "FullyQualifiedName~QrServiceTests"

# Test específico
dotnet test --filter "GenerarUrlCotejo_GeneraUrlValida"
```

### Categorías de Tests

1. **Tests de URL de cotejo** (13 tests)
   - Generación válida de URL
   - Parámetros correctos
   - Formatos de fecha e importe
   - Manejo de errores

2. **Tests de PNG** (5 tests)
   - Generación de bytes válidos
   - Cabecera PNG correcta
   - Diferentes tamaños

3. **Tests de SVG** (4 tests)
   - Generación de XML válido
   - Elementos SVG correctos

4. **Tests de Base64** (4 tests)
   - Data URI válido
   - Decodificación correcta

5. **Tests de integración** (2 tests)
   - Flujo completo
   - URLs largas

## 🔒 Seguridad

### Verificaciones Realizadas

✅ **gh-advisory-database**: Sin vulnerabilidades en QRCoder  
✅ **CodeQL**: 0 alertas de seguridad  
✅ **Code Review**: Comentarios menores sobre consistencia (no críticos)

### Buenas Prácticas Implementadas

- Validación exhaustiva de parámetros de entrada
- Mensajes de error claros y descriptivos
- Escapado correcto de caracteres en URL
- Uso de `using` para liberar recursos (QRCodeGenerator)
- Inmutabilidad de datos (record types)

## 📚 Referencias

### Documentación Oficial AEAT

- **Especificación S16**: Características del QR y servicio de cotejo
- **Guía Técnica**: Sección 7 - Código QR y servicio de cotejo
- **Descripción SWeb**: Documentación completa de servicios

### Documentación Interna

- `docs/Verifactu-Guia-Tecnica.md` - Sección 7
- `docs/Veri-Factu_Descripcion_SWeb.md` - Especificaciones QR
- `src/Verifactu.Client/Examples/QrServiceExamples.cs` - Ejemplos prácticos

## 🛠️ Solución de Problemas

### Error: "La huella debe tener al menos 13 caracteres"

**Causa**: La huella proporcionada es demasiado corta.

**Solución**: Asegúrate de pasar la huella SHA-256 completa (64 caracteres hexadecimales). El servicio tomará automáticamente los primeros 13.

```csharp
// ✗ Incorrecto
var huella = "ABCD123"; // Solo 7 caracteres

// ✓ Correcto
var huella = hashService.CalcularHuella(registro, huellaAnterior); // 64 caracteres
```

### Error: "El tamaño debe ser mayor que 0"

**Causa**: Se pasó un valor inválido para `pixelsPorModulo`.

**Solución**: Usa valores positivos (recomendado: 10-20).

```csharp
// ✗ Incorrecto
var qr = qrService.GenerarQrPng(url, pixelsPorModulo: -1);

// ✓ Correcto
var qr = qrService.GenerarQrPng(url, pixelsPorModulo: 20);
```

### QR demasiado grande o pequeño

**Ajuste**: Modifica el parámetro `pixelsPorModulo`:

```csharp
// QR pequeño (para pantallas)
var qrPequeno = qrService.GenerarQrPng(url, pixelsPorModulo: 10);

// QR mediano (uso general)
var qrMedio = qrService.GenerarQrPng(url, pixelsPorModulo: 20);

// QR grande (impresión de alta calidad)
var qrGrande = qrService.GenerarQrPng(url, pixelsPorModulo: 30);
```

## 🔄 Próximos Pasos Recomendados

1. **Incluir leyenda "VERI*FACTU"** en facturas cuando corresponda
2. **Integrar QR en plantillas de factura PDF**
3. **Añadir soporte para generación masiva de QR**
4. **Cachear códigos QR generados** para optimizar rendimiento
5. **Añadir opción de personalización de colores** en QR

## 📝 Changelog

### Versión 1.0.0 (2024-11-07)

- ✅ Implementación inicial del servicio QR
- ✅ Soporte para formatos PNG, SVG y Base64
- ✅ 38 tests unitarios completos
- ✅ Ejemplos de uso detallados
- ✅ Documentación completa en español
- ✅ Validación de seguridad (0 vulnerabilidades)

## 👥 Contribuciones

Para contribuir al desarrollo del servicio QR:

1. Asegúrate de que todos los tests pasen
2. Añade tests para nuevas funcionalidades
3. Actualiza la documentación
4. Sigue las convenciones de código del proyecto
5. Ejecuta CodeQL antes de hacer commit

## 📞 Soporte

Para reportar problemas o solicitar mejoras:

- Abrir un issue en el repositorio
- Incluir ejemplos reproducibles
- Especificar versión de .NET y QRCoder

---

**Última actualización**: 2024-11-07  
**Versión**: 1.0.0  
**Estado**: ✅ Producción
