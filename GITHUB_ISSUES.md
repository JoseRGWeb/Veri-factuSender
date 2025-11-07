# Issues de GitHub para Veri-factuSender

Este documento contiene todas las issues detalladas para implementar la conformidad completa con VERI\*FACTU.

Puedes copiarlas manualmente en GitHub o usar el script `scripts/create-github-issues.ps1` para crearlas automáticamente.

---

## Issue #1: [CRÍTICO] Implementar serialización XML conforme a XSD oficial de AEAT

**Labels**: `critical`, `enhancement`, `compliance`, `phase-1`

### 📋 Descripción

La serialización XML actual en `VerifactuSerializer.cs` usa namespaces y estructura **placeholder** que NO cumplen con los esquemas XSD oficiales de AEAT. Esto es **bloqueante** para cualquier uso en producción o pruebas contra el Portal de Pruebas Externas.

### 🎯 Objetivo

Implementar serialización XML que cumpla 100% con los esquemas XSD oficiales de VERI\*FACTU:

-   `SuministroLR.xsd` - Altas/anulaciones de registros
-   `SuministroInformacion.xsd` - Tipos comunes
-   Otros XSD relacionados según operación

### 📚 Referencias

-   **Documentación oficial**: `docs/Veri-Factu_Descripcion_SWeb.pdf` (sección de especificaciones técnicas)
-   **Guía técnica interna**: `docs/Verifactu-Guia-Tecnica.md` (sección 4.2 Esquemas XSD)
-   **Fuente AEAT**: [Esquemas XSD oficiales](https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/)
-   **WSDL**: [SistemaFacturacion.wsdl](https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl)

### ✅ Criterios de Aceptación

-   [ ] Descargar todos los XSD oficiales y almacenarlos en `docs/xsd/`
-   [ ] Generar clases C# desde XSD (usar `xsd.exe` o implementación manual)
-   [ ] Reemplazar namespace `urn:aeat:verifactu:placeholder` por namespaces reales
-   [ ] Implementar TODOS los campos obligatorios según diseños de registro:
    -   `IDFactura` (serie, número, fecha emisión, NIF emisor)
    -   `Contraparte` con identificación completa
    -   `TipoFactura` (F1, F2, F3, F4, R1-R5, etc.)
    -   `ClaveRegimenEspecialOTrascendencia`
    -   Desglose IVA/IGIC/IRPF con tipos impositivos
    -   `EncadenamientoFacturaAnterior` (huella + campos relacionados)
    -   Campos específicos factura simplificada vs completa
    -   `ImporteExento`, `ImporteNoSujeto`
    -   `DescripcionOperacion`
-   [ ] Añadir validación XML contra XSD antes de envío
-   [ ] Crear tests unitarios que validen XML generado contra XSD
-   [ ] Actualizar `Factura.cs` y `RegistroFacturacion.cs` con campos adicionales necesarios

### 🔧 Archivos a Modificar

-   `src/Verifactu.Client/Services/VerifactuSerializer.cs` - Reescritura completa
-   `src/Verifactu.Client/Models/Factura.cs` - Añadir campos obligatorios
-   `src/Verifactu.Client/Models/RegistroFacturacion.cs` - Ampliar modelo
-   `tests/Verifactu.Client.Tests/` - Nuevos tests de validación XSD

### 💡 Notas de Implementación

**Opción 1 - Generación automática**:

```bash
# Descargar XSD y generar clases
xsd.exe SuministroLR.xsd SuministroInformacion.xsd /c /n:Verifactu.Client.Models.Aeat
```

**Opción 2 - Implementación manual**:

-   Mayor control sobre el modelo
-   Usar atributos `[XmlElement]`, `[XmlAttribute]`, `[XmlNamespace]`
-   Preferible si se necesita mapeo personalizado

**Validación XSD en runtime**:

```csharp
var schemas = new XmlSchemaSet();
schemas.Add("namespace", "path/to/SuministroLR.xsd");
xmlDoc.Schemas = schemas;
xmlDoc.Validate(ValidationEventHandler);
```

### 📌 Prioridad

**CRÍTICA** - Bloqueante para uso en producción

### 🔗 Issues Relacionadas

-   Bloqueado por: Ninguno
-   Bloquea: #2, #3, #4, #5, #6, #7, #8, #9

---

## Issue #2: [CRÍTICO] Implementar algoritmo de huella (hash) oficial según especificación AEAT

**Labels**: `critical`, `enhancement`, `compliance`, `phase-1`

### 📋 Descripción

El algoritmo actual de cálculo de huella en `HashService.cs` es un **placeholder** que NO cumple con la especificación oficial de AEAT. El orden de campos, normalización y formato son incorrectos.

### 🎯 Objetivo

Implementar el algoritmo SHA-256 de huella exactamente según la especificación oficial S11/S12 de AEAT, incluyendo:

-   Orden exacto de campos
-   Normalización de decimales (2 decimales, punto como separador)
-   Formato ISO 8601 para fechas
-   Manejo correcto de campos opcionales/nulos
-   Encadenamiento con registro anterior

### 📚 Referencias

-   **Documentación oficial**: `docs/Veri-Factu_Descripcion_SWeb.pdf` (algoritmo de hash)
-   **Guía técnica interna**: `docs/Verifactu-Guia-Tecnica.md` (sección 5 - Algoritmo de huella)
-   **Fuente AEAT**: [Algoritmo de cálculo de la huella](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/algoritmo-calculo-huella.html)
-   **FAQ AEAT**: Huella o «hash» (SHA‑256 y encadenado)

### ✅ Criterios de Aceptación

-   [ ] Implementar orden exacto de campos según especificación oficial
-   [ ] Normalización correcta de valores decimales:
    -   2 decimales exactos
    -   Punto (.) como separador decimal
    -   Sin separador de miles
-   [ ] Formato de fechas: ISO 8601 (`yyyy-MM-ddTHH:mm:ssZ`)
-   [ ] Manejo de campos opcionales según reglas AEAT
-   [ ] Implementar encadenamiento correcto con `HuellaAnterior`
-   [ ] Codificación UTF-8 sin BOM
-   [ ] Resultado en mayúsculas hexadecimal
-   [ ] Tests exhaustivos con vectores de prueba oficiales AEAT
-   [ ] Documentar el algoritmo paso a paso en código

### 🔧 Archivos a Modificar

-   `src/Verifactu.Client/Services/HashService.cs` - Reescritura completa
-   `tests/Verifactu.Client.Tests/HashServiceTests.cs` - Tests con vectores oficiales
-   `docs/algoritmo-huella.md` - Documentación detallada (nuevo)

### 💡 Notas de Implementación

**Campos en orden (ejemplo simplificado)**:

1. IDEmisorFactura
2. NumSerieFactura
3. FechaExpedicionFactura
4. TipoFactura
5. CuotaTotal
6. ImporteTotal
7. Huella del registro anterior (si existe)
8. (+ más campos según tipo de factura)

**Normalización decimal**:

```csharp
// Ejemplo: 1234.50 EUR
string normalized = value.ToString("F2", CultureInfo.InvariantCulture);
// Resultado: "1234.50"
```

**Hash final**:

```csharp
var bytes = Encoding.UTF8.GetBytes(concatenatedString);
var hash = SHA256.HashData(bytes);
return Convert.ToHexString(hash); // Mayúsculas
```

### 📌 Prioridad

**CRÍTICA** - Bloqueante para uso en producción

### 🔗 Issues Relacionadas

-   Bloqueado por: #1 (necesita modelo de datos completo)
-   Bloquea: #3, #4

---

## Issue #3: [CRÍTICO] Actualizar cliente SOAP con WSDL oficial y parsear respuestas

**Labels**: `critical`, `enhancement`, `compliance`, `phase-1`

### 📋 Descripción

El cliente SOAP actual en `VerifactuSoapClient.cs` genera un sobre SOAP genérico con namespace placeholder. Necesita alinearse completamente con el WSDL oficial de AEAT.

### 🎯 Objetivo

Implementar cliente SOAP que:

-   Use el WSDL oficial `SistemaFacturacion.wsdl`
-   Implemente operaciones correctas (RegFacturacionAlta, ConsultaLRFacturas, etc.)
-   Parse respuestas según `RespuestaSuministro.xsd` y `RespuestaConsultaLR.xsd`
-   Maneje correctamente headers SOAP y namespaces

### 📚 Referencias

-   **Documentación oficial**: `docs/Veri-Factu_Descripcion_SWeb.pdf` (servicios web)
-   **Guía técnica interna**: `docs/Verifactu-Guia-Tecnica.md` (sección 4.1 WSDL)
-   **WSDL oficial**: [SistemaFacturacion.wsdl](https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl)
-   **XSD Respuesta**: `RespuestaSuministro.xsd`, `RespuestaConsultaLR.xsd`

### ✅ Criterios de Aceptación

-   [ ] Descargar WSDL oficial y almacenar en `docs/wsdl/`
-   [ ] Implementar operación `RegFacturacionAlta`
-   [ ] Implementar operación `ConsultaLRFacturas`
-   [ ] Usar namespaces correctos del WSDL
-   [ ] Construir envelope SOAP conforme al WSDL
-   [ ] Parser de respuestas:
    -   `EstadoEnvio`: Correcto, AceptadoConErrores, Incorrecto
    -   `CodigoError` y `DescripcionError`
    -   CSV de registros presentados
-   [ ] Manejo de timeout y excepciones de red
-   [ ] Tests de integración contra sandbox AEAT
-   [ ] Logging estructurado de requests/responses

### 🔧 Archivos a Modificar

-   `src/Verifactu.Client/Soap/VerifactuSoapClient.cs` - Actualización
-   `src/Verifactu.Client/Models/RespuestaAeat.cs` - Nuevo (modelo de respuesta)
-   `tests/Verifactu.Client.Tests/SoapClientTests.cs` - Nuevos tests

### 💡 Notas de Implementación

**Generación de proxy SOAP**:

```bash
# Opción 1: Usar herramienta de Visual Studio
# Add Service Reference -> WCF Web Service

# Opción 2: Usar dotnet-svcutil
dotnet tool install --global dotnet-svcutil
dotnet-svcutil SistemaFacturacion.wsdl
```

**Estructura SOAP esperada**:

```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:sfe="namespace-del-wsdl">
  <soapenv:Header/>
  <soapenv:Body>
    <sfe:RegFacturacionAlta>
      <Cabecera>
        <IDVersionSii>1.0</IDVersionSii>
        <!-- Más campos según WSDL -->
      </Cabecera>
      <RegistroLRFacturasEmitidas>
        <!-- XML del registro -->
      </RegistroLRFacturasEmitidas>
    </sfe:RegFacturacionAlta>
  </soapenv:Body>
</soapenv:Envelope>
```

### 📌 Prioridad

**CRÍTICA** - Bloqueante para uso en producción

### 🔗 Issues Relacionadas

-   Bloqueado por: #1 (necesita XML serializado correctamente)
-   Bloquea: #5, #8

---

## Issue #4: [ALTA] Completar modelo de datos con todos los campos obligatorios

**Labels**: `enhancement`, `compliance`, `phase-1`

### 📋 Descripción

Los modelos actuales `Factura.cs` y `RegistroFacturacion.cs` son básicos y faltan numerosos campos obligatorios según la especificación VERI\*FACTU.

### 🎯 Objetivo

Ampliar los modelos de datos para incluir TODOS los campos obligatorios y opcionales relevantes según los diseños de registro oficiales de AEAT.

### 📚 Referencias

-   **Documentación oficial**: `docs/Veri-Factu_Descripcion_SWeb.pdf` (diseños de registro)
-   **Guía técnica interna**: `docs/Verifactu-Guia-Tecnica.md` (sección 4.3 Diseños de registro)
-   **Fuente AEAT**: [Diseños de registro](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/disenos-registro.html)

### ✅ Criterios de Aceptación

**Campos obligatorios a añadir**:

-   [ ] `TipoFactura`: F1, F2, F3, F4, R1, R2, R3, R4, R5
-   [ ] `TipoRectificativa`: S, I (si aplica)
-   [ ] `FacturasRectificadas` / `FacturasSustituidas` (referencias)
-   [ ] `ClaveRegimenEspecialOTrascendencia`: 01-17
-   [ ] `ImporteExento`
-   [ ] `ImporteNoSujeto`
-   [ ] `ImporteTransmisionSujetoAIVA`
-   [ ] Desglose IVA/IGIC/IRPF:
    -   `TipoImpositivo`
    -   `BaseImponible`
    -   `CuotaRepercutida`
    -   `TipoRecargoEquivalencia`
    -   `CuotaRecargoEquivalencia`
-   [ ] `DescripcionOperacion`
-   [ ] Identificación completa de contraparte:
    -   `NombreRazon`
    -   `NIF`
    -   `IDOtro` (para extranjeros)
    -   Dirección completa
-   [ ] Campos facturación por terceros:
    -   `NombreRazonEmisor`
    -   `NIFEmisor`
-   [ ] Macrodatos (si >6M EUR):
    -   `Macrodato`
-   [ ] Sistema informático:
    -   `NumeroSerieSistemaInformatico`
    -   `NombreSistemaInformatico`
    -   `NombreRazonSocialDesarrollador`
    -   `NIFDesarrollador`

**Campos opcionales recomendados**:

-   [ ] `CuotaDeducible`
-   [ ] `BaseRectificada`
-   [ ] `CuotaRectificada`
-   [ ] Datos de cobros:
    -   `FechaCobro`
    -   `ImporteCobro`
    -   `MedioCobro`
-   [ ] `NumeroRegistroAcuerdoFacturacion`
-   [ ] `IdAcuerdoSistemaInformatico`

### 🔧 Archivos a Modificar

-   `src/Verifactu.Client/Models/Factura.cs` - Ampliar record
-   `src/Verifactu.Client/Models/RegistroFacturacion.cs` - Ampliar record
-   `src/Verifactu.Client/Models/TipoFactura.cs` - Nuevo (enum)
-   `src/Verifactu.Client/Models/ClaveRegimen.cs` - Nuevo (enum)
-   `src/Verifactu.Client/Models/DesgloseImpuestos.cs` - Nuevo
-   `src/Verifactu.Client/Models/Contraparte.cs` - Nuevo
-   `tests/Verifactu.Client.Tests/ModelosTests.cs` - Tests de validación

### 💡 Notas de Implementación

**Usar records inmutables**:

```csharp
public record Factura(
    string Serie,
    string Numero,
    DateTime FechaEmision,
    TipoFactura TipoFactura,
    ClaveRegimen ClaveRegimenEspecialOTrascendencia,
    Emisor Emisor,
    Receptor Receptor,
    List<DesgloseImpuesto> DesgloseIVA,
    TotalesFactura Totales,
    // ... más campos
    string? DescripcionOperacion = null,
    string Moneda = "EUR"
);
```

**Validación de datos**:

-   Implementar validaciones básicas en constructores o métodos `Validate()`
-   Usar atributos de validación si se usa FluentValidation

### 📌 Prioridad

**ALTA** - Necesario para serialización y hash correctos

### 🔗 Issues Relacionadas

-   Bloqueado por: Ninguno
-   Bloquea: #1, #2

---

## Issue #5: [ALTA] Implementar gestión de respuestas AEAT y manejo de errores

**Labels**: `enhancement`, `compliance`, `phase-2`

### 📋 Descripción

Actualmente no hay parser de respuestas AEAT ni gestión de códigos de error. Es esencial para manejar correctamente las validaciones, reintentos y estados.

### 🎯 Objetivo

Implementar sistema completo de gestión de respuestas AEAT:

-   Parser de XML de respuesta según `RespuestaSuministro.xsd`
-   Manejo de todos los estados posibles
-   Gestión de códigos de error según documento oficial de validaciones
-   Lógica de reintentos con backoff exponencial

### 📚 Referencias

-   **Documentación oficial**: `docs/Veri-Factu_Descripcion_SWeb.pdf` (validaciones y errores)
-   **Guía técnica interna**: `docs/Verifactu-Guia-Tecnica.md` (sección 4.4 Documento de validaciones)
-   **Fuente AEAT**: [Documento de validaciones y errores](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/validaciones-errores.html)

### ✅ Criterios de Aceptación

-   [ ] Parser de `RespuestaSuministro.xsd`:
    -   `EstadoEnvio`: Correcto, AceptadoConErrores, Incorrecto
    -   `CodigoErrorRegistro`
    -   `DescripcionErrorRegistro`
    -   CSV de registros presentados
-   [ ] Catálogo completo de códigos de error AEAT (>900 validaciones)
-   [ ] Clasificación de errores:
    -   Errores recuperables (reintentar)
    -   Errores de validación (corregir datos)
    -   Errores de configuración (revisar certificado/endpoint)
-   [ ] Lógica de reintentos:
    -   Backoff exponencial (1s, 2s, 4s, 8s, ...)
    -   Máximo 5 reintentos
    -   Solo para errores recuperables (timeout, 5xx)
-   [ ] Logging estructurado de errores con contexto completo
-   [ ] Tests de manejo de cada tipo de error

### 🔧 Archivos a Modificar

-   `src/Verifactu.Client/Models/RespuestaAeat.cs` - Nuevo (modelos de respuesta)
-   `src/Verifactu.Client/Services/RespuestaParser.cs` - Nuevo
-   `src/Verifactu.Client/Services/ErrorHandler.cs` - Nuevo
-   `src/Verifactu.Client/Services/ReintentosService.cs` - Nuevo
-   `src/Verifactu.Client/Models/CodigoError.cs` - Nuevo (enum/catálogo)
-   `tests/Verifactu.Client.Tests/ErrorHandlingTests.cs` - Nuevos tests

### 💡 Notas de Implementación

**Estados de respuesta**:

```csharp
public enum EstadoEnvio
{
    Correcto,              // Todo OK
    AceptadoConErrores,    // Aceptado pero con warnings
    Incorrecto             // Rechazado
}
```

**Lógica de reintentos con Polly**:

```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .WaitAndRetryAsync(
        retryCount: 5,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (exception, timeSpan, attempt, context) =>
        {
            logger.LogWarning($"Reintento {attempt} después de {timeSpan.TotalSeconds}s");
        });
```

**Clasificación de errores**:

-   **1xxx**: Errores de estructura XML
-   **2xxx**: Errores de validación de datos
-   **3xxx**: Errores de negocio
-   **4xxx**: Errores de certificado/autenticación
-   **5xxx**: Errores de servidor AEAT

### 📌 Prioridad

**ALTA** - Esencial para robustez en producción

### 🔗 Issues Relacionadas

-   Bloqueado por: #3 (necesita parser de SOAP)
-   Bloquea: #8

---

## Issue #6: [MEDIA] Implementar generación de código QR y servicio de cotejo

**Labels**: `enhancement`, `compliance`, `phase-2`

### 📋 Descripción

Las facturas VERI\*FACTU deben incluir un código QR que permita al receptor cotejar el registro en la sede electrónica de AEAT.

### 🎯 Objetivo

Implementar generación de código QR según especificación oficial S16 de AEAT:

-   URL de cotejo con parámetros correctos
-   Generación de imagen QR
-   Inclusión de leyenda "VERI\*FACTU" cuando corresponde

### 📚 Referencias

-   **Documentación oficial**: `docs/Veri-Factu_Descripcion_SWeb.pdf` (QR y cotejo)
-   **Guía técnica interna**: `docs/Verifactu-Guia-Tecnica.md` (sección 7 - Código QR)
-   **Fuente AEAT**: [Características del QR y servicio de cotejo](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/caracteristicas-qr.html)

### ✅ Criterios de Aceptación

-   [ ] Construir URL de cotejo con parámetros:
    -   NIF emisor
    -   Número de factura
    -   Fecha de expedición
    -   Importe total
    -   Huella del registro
-   [ ] Generar código QR (usar librería QRCoder o similar)
-   [ ] Formatos de salida:
    -   Imagen PNG/SVG
    -   Base64 para embedding en HTML/PDF
-   [ ] Añadir leyenda "VERI\*FACTU" en factura cuando aplique
-   [ ] Validar que el QR es escaneable y redirige correctamente
-   [ ] Tests de generación de QR

### 🔧 Archivos a Modificar

-   `src/Verifactu.Client/Services/QrService.cs` - Nuevo
-   `src/Verifactu.Client/Services/Interfaces.cs` - Añadir `IQrService`
-   `Verifactu.Client.csproj` - Añadir dependencia QRCoder
-   `tests/Verifactu.Client.Tests/QrServiceTests.cs` - Nuevos tests

### 💡 Notas de Implementación

**URL de cotejo (ejemplo)**:

```
https://sede.agenciatributaria.gob.es/verifactu/cotejo
  ?nif=B12345678
  &numserie=FA-2025-001
  &fecha=30-10-2025
  &importe=1234.50
  &huella=ABCD1234...
```

**Generación de QR con QRCoder**:

```csharp
using QRCoder;

var qrGenerator = new QRCodeGenerator();
var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
var qrCode = new PngByteQRCode(qrCodeData);
byte[] qrCodeImage = qrCode.GetGraphic(20);
```

**Dependencia NuGet**:

```xml
<PackageReference Include="QRCoder" Version="1.6.0" />
```

### 📌 Prioridad

**MEDIA** - Necesario para conformidad visual de facturas

### 🔗 Issues Relacionadas

-   Bloqueado por: #2 (necesita huella calculada)
-   Bloquea: Ninguna

---

## Issue #7: [ALTA] Implementar capa de persistencia y auditoría

**Labels**: `enhancement`, `infrastructure`, `phase-2`

### 📋 Descripción

Actualmente no hay persistencia de registros enviados, respuestas AEAT ni auditoría. Es crítico para trazabilidad y recuperación ante fallos.

### 🎯 Objetivo

Implementar capa de persistencia completa con:

-   Almacenamiento de registros de facturación
-   Historial de envíos y respuestas AEAT
-   Sistema de auditoría de cambios
-   Soporte para múltiples backends (SQL Server, PostgreSQL, SQLite)

### 📚 Referencias

-   **Documentación oficial**: `docs/Veri-Factu_Descripcion_SWeb.pdf`
-   **Guía técnica interna**: `docs/Verifactu-Guia-Tecnica.md` (sección 9 - Modelo de datos sugerido)

### ✅ Criterios de Aceptación

**Tabla `RegistrosFacturacion`**:

-   [ ] `Id` (GUID/PK)
-   [ ] `Uuid` (GUID único del registro)
-   [ ] `Serie`, `Numero`
-   [ ] `FechaHoraExpedicionUTC`
-   [ ] `Huella`, `HuellaAnterior`
-   [ ] `EstadoEnvio` (Pendiente, Enviado, Aceptado, Rechazado, Error)
-   [ ] `CodigoErrorAEAT`, `DescripcionErrorAEAT`
-   [ ] `IdFactura` (FK a tabla Facturas)
-   [ ] `XmlFirmado` (XML completo enviado)
-   [ ] `AcuseRecibo` (respuesta AEAT)
-   [ ] `FechaEnvio`, `FechaRespuesta`
-   [ ] `Reintentos` (contador)
-   [ ] `FechaCreacion`, `FechaModificacion`

**Tabla `Facturas`**:

-   [ ] Modelo normalizado completo
-   [ ] Relación con `RegistrosFacturacion`

**Tabla `AuditoriaRegistros`**:

-   [ ] `Id`, `RegistroId`
-   [ ] `Accion` (Creado, Enviado, Reintentado, Aceptado, Rechazado)
-   [ ] `Usuario`, `Timestamp`
-   [ ] `DetallesJson` (información adicional)

**Índices necesarios**:

-   [ ] `IX_RegistrosFacturacion_FechaHoraExpedicion`
-   [ ] `IX_RegistrosFacturacion_SerieNumero`
-   [ ] `IX_RegistrosFacturacion_EstadoEnvio`
-   [ ] `IX_RegistrosFacturacion_Huella`
-   [ ] `UX_RegistrosFacturacion_Uuid` (único)

**Funcionalidades**:

-   [ ] Repositorio genérico con patrón Unit of Work
-   [ ] Migraciones de base de datos (Entity Framework Core)
-   [ ] Soporte para SQLite (desarrollo), SQL Server/PostgreSQL (producción)
-   [ ] Queries optimizados para búsquedas frecuentes
-   [ ] Soft delete para registros históricos

### 🔧 Archivos a Crear

-   `src/Verifactu.Data/` - Nuevo proyecto
-   `src/Verifactu.Data/Entities/RegistroFacturacionEntity.cs`
-   `src/Verifactu.Data/Entities/FacturaEntity.cs`
-   `src/Verifactu.Data/Entities/AuditoriaRegistroEntity.cs`
-   `src/Verifactu.Data/VerifactuDbContext.cs`
-   `src/Verifactu.Data/Repositories/IRegistroRepository.cs`
-   `src/Verifactu.Data/Repositories/RegistroRepository.cs`
-   `src/Verifactu.Data/Migrations/`
-   `tests/Verifactu.Data.Tests/` - Tests de repositorio

### 💡 Notas de Implementación

**Entity Framework Core**:

```csharp
public class VerifactuDbContext : DbContext
{
    public DbSet<RegistroFacturacionEntity> RegistrosFacturacion { get; set; }
    public DbSet<FacturaEntity> Facturas { get; set; }
    public DbSet<AuditoriaRegistroEntity> Auditoria { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegistroFacturacionEntity>()
            .HasIndex(r => r.Uuid)
            .IsUnique();

        modelBuilder.Entity<RegistroFacturacionEntity>()
            .HasIndex(r => new { r.Serie, r.Numero });
    }
}
```

**Repositorio**:

```csharp
public interface IRegistroRepository
{
    Task<RegistroFacturacionEntity?> GetByUuidAsync(Guid uuid);
    Task<RegistroFacturacionEntity?> GetBySerieNumeroAsync(string serie, string numero);
    Task<IEnumerable<RegistroFacturacionEntity>> GetPendientesEnvioAsync();
    Task AddAsync(RegistroFacturacionEntity registro);
    Task UpdateAsync(RegistroFacturacionEntity registro);
}
```

**Dependencias NuGet**:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
```

### 📌 Prioridad

**ALTA** - Esencial para trazabilidad y recuperación

### 🔗 Issues Relacionadas

-   Bloqueado por: #4 (necesita modelo de datos completo)
-   Bloquea: #8

---

## Issue #8: [ALTA] Implementar tests de integración contra Portal de Pruebas Externas

**Labels**: `testing`, `compliance`, `phase-2`

### 📋 Descripción

Se necesitan tests de integración reales contra el Portal de Pruebas Externas de AEAT para validar el flujo end-to-end completo.

### 🎯 Objetivo

Crear suite completa de tests de integración que validen:

-   Conexión con certificado al sandbox AEAT
-   Envío de registros de facturación
-   Recepción y parseo de respuestas
-   Consulta de registros enviados
-   Manejo de errores comunes

### 📚 Referencias

-   **Documentación oficial**: `docs/Veri-Factu_Descripcion_SWeb.pdf`
-   **Guía técnica interna**: `docs/Verifactu-Guia-Tecnica.md` (sección 3 - Entornos)
-   **Portal de Pruebas**: [Acceso con certificado](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/portal-pruebas-externas.html)

### ✅ Criterios de Aceptación

-   [ ] Tests de conexión TLS mutua con certificado
-   [ ] Test de envío de factura completa (tipo F1)
-   [ ] Test de envío de factura simplificada (tipo F2)
-   [ ] Test de factura rectificativa (tipo R1-R5)
-   [ ] Test de consulta de registro enviado
-   [ ] Test de manejo de error de validación
-   [ ] Test de timeout y reintento
-   [ ] Test de validación de huella y encadenamiento
-   [ ] Configuración de endpoint de sandbox en `appsettings.Sandbox.json`
-   [ ] Documentación de cómo ejecutar tests de integración

### 🔧 Archivos a Crear/Modificar

-   `tests/Verifactu.Integration.Tests/` - Nuevo proyecto
-   `tests/Verifactu.Integration.Tests/SandboxTests.cs`
-   `tests/Verifactu.Integration.Tests/appsettings.Test.json`
-   `tests/Verifactu.Integration.Tests/TestFixtures/`
-   `docs/pruebas-integracion.md` - Nueva documentación

### 💡 Notas de Implementación

**Configuración de sandbox**:

```json
{
    "Verifactu": {
        "EndpointUrl": "https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion",
        "SoapAction": "RegFacturacionAlta"
    },
    "Certificado": {
        "PfxPath": "path/to/test-cert.pfx",
        "PfxPassword": "test-password"
    }
}
```

**Test de ejemplo**:

```csharp
[Fact]
public async Task EnviarFacturaCompleta_DeberiaAceptarse()
{
    // Arrange
    var factura = CrearFacturaCompleta();
    var registro = CrearRegistroFacturacion(factura);

    // Act
    var respuesta = await _soapClient.EnviarRegistroAsync(registro);

    // Assert
    Assert.Equal(EstadoEnvio.Correcto, respuesta.Estado);
    Assert.NotNull(respuesta.CSV);
}
```

**Marcado de tests**:

```csharp
[Trait("Category", "Integration")]
[Trait("Requires", "Certificate")]
public class SandboxTests { }
```

**Ejecución selectiva**:

```bash
# Solo tests de integración
dotnet test --filter "Category=Integration"

# Excluir tests que requieren certificado
dotnet test --filter "Requires!=Certificate"
```

### 📌 Prioridad

**ALTA** - Crítico para validación antes de producción

### 🔗 Issues Relacionadas

-   Bloqueado por: #1, #2, #3, #5
-   Bloquea: Paso a producción

---

## Issue #9: [MEDIA] Migrar CertificateLoader a API no obsoleta de .NET 9

**Labels**: `enhancement`, `modernization`, `phase-3`

### 📋 Descripción

El método `CertificateLoader.CargarDesdePfx` usa el constructor obsoleto de `X509Certificate2` que genera warning SYSLIB0057 en .NET 9.

### 🎯 Objetivo

Migrar a la nueva API `X509CertificateLoader.LoadPkcs12()` recomendada por .NET 9, manteniendo la misma funcionalidad.

### 📚 Referencias

-   **Warning SYSLIB0057**: [Documentación Microsoft](https://learn.microsoft.com/en-us/dotnet/fundamentals/syslib-diagnostics/syslib0057)
-   **.NET 9 Breaking Changes**: X509Certificate constructors

### ✅ Criterios de Aceptación

-   [ ] Reemplazar constructor obsoleto por `X509CertificateLoader.LoadPkcs12()`
-   [ ] Mantener compatibilidad con flags existentes:
    -   `X509KeyStorageFlags.MachineKeySet`
    -   `X509KeyStorageFlags.Exportable`
    -   `X509KeyStorageFlags.PersistKeySet`
-   [ ] Añadir validaciones recomendadas:
    -   Verificar `HasPrivateKey == true`
    -   Validar fechas de vigencia
    -   Verificar cadena de confianza
    -   Comprobar propósito Client Authentication
-   [ ] Actualizar tests existentes
-   [ ] Actualizar documentación y comentarios

### 🔧 Archivos a Modificar

-   `src/Verifactu.Client/Services/CertificateLoader.cs`
-   `tests/Verifactu.Client.Tests/CertificateLoaderTests.cs`

### 💡 Notas de Implementación

**Nueva implementación**:

```csharp
public X509Certificate2 CargarDesdePfx(string rutaPfx, string password)
{
    byte[] pfxBytes = File.ReadAllBytes(rutaPfx);

    var cert = X509CertificateLoader.LoadPkcs12(
        pfxBytes,
        password,
        X509KeyStorageFlags.MachineKeySet |
        X509KeyStorageFlags.Exportable |
        X509KeyStorageFlags.PersistKeySet
    );

    // Validaciones recomendadas
    if (!cert.HasPrivateKey)
        throw new InvalidOperationException("El certificado no contiene clave privada");

    if (cert.NotBefore > DateTime.Now || cert.NotAfter < DateTime.Now)
        throw new InvalidOperationException("El certificado no está vigente");

    // Verificar propósito Client Authentication (1.3.6.1.5.5.7.3.2)
    var extensions = cert.Extensions.OfType<X509EnhancedKeyUsageExtension>().FirstOrDefault();
    if (extensions != null)
    {
        bool hasClientAuth = extensions.EnhancedKeyUsages
            .Cast<Oid>()
            .Any(oid => oid.Value == "1.3.6.1.5.5.7.3.2");

        if (!hasClientAuth)
            throw new InvalidOperationException("El certificado no tiene propósito Client Authentication");
    }

    return cert;
}
```

**Validación de cadena de confianza**:

```csharp
var chain = new X509Chain();
chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;

if (!chain.Build(cert))
{
    var errors = chain.ChainStatus.Select(s => s.StatusInformation);
    throw new InvalidOperationException($"Cadena de certificado inválida: {string.Join(", ", errors)}");
}
```

### 📌 Prioridad

**MEDIA** - No bloqueante pero recomendable

### 🔗 Issues Relacionadas

-   Bloqueado por: Ninguno
-   Bloquea: Ninguna

---

## 📝 Instrucciones para Crear las Issues en GitHub

### Opción 1: Manual (Copiar y Pegar)

1. Ve a tu repositorio en GitHub: `https://github.com/JoseRGWeb/Veri-factuSender/issues`
2. Haz clic en "New Issue"
3. Copia el **título** y el **cuerpo** de cada issue de este documento
4. Añade las **labels** sugeridas
5. Asigna a ti mismo si corresponde
6. Haz clic en "Submit new issue"

### Opción 2: Automatizada (Script PowerShell)

Usa el script `scripts/create-github-issues.ps1` que generaré a continuación.

### Opción 3: CLI de GitHub

```bash
# Instalar GitHub CLI si no lo tienes
# https://cli.github.com/

# Autenticarte
gh auth login

# Crear cada issue (ejemplo)
gh issue create \
  --repo JoseRGWeb/Veri-factuSender \
  --title "[CRÍTICO] Implementar serialización XML conforme a XSD oficial de AEAT" \
  --body "$(cat issue-1-body.md)" \
  --label "critical,enhancement"
```

---

## 📊 Roadmap Sugerido

### Fase 1 - CRÍTICO (Sprint 1-2)

-   Issue #1: Serialización XML ⚡
-   Issue #2: Algoritmo de huella ⚡
-   Issue #4: Modelo de datos completo ⚡

### Fase 2 - ESENCIAL (Sprint 3-4)

-   Issue #3: Cliente SOAP actualizado
-   Issue #5: Gestión de respuestas y errores
-   Issue #7: Capa de persistencia

### Fase 3 - COMPLETO (Sprint 5-6)

-   Issue #6: Código QR
-   Issue #8: Tests de integración
-   Issue #9: Migración .NET 9

---

**Fecha de creación**: 7 de noviembre de 2025  
**Autor**: Análisis de conformidad automatizado  
**Versión**: 1.0
