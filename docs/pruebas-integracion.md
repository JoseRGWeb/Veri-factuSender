# Guía de Pruebas de Integración contra Sandbox AEAT

Esta guía documenta cómo ejecutar y mantener los tests de integración contra el Portal de Pruebas Externas de AEAT para VERI*FACTU.

## Índice

1. [Descripción General](#descripción-general)
2. [Requisitos Previos](#requisitos-previos)
3. [Configuración Inicial](#configuración-inicial)
4. [Ejecución de Tests](#ejecución-de-tests)
5. [Tests Implementados](#tests-implementados)
6. [Interpretación de Resultados](#interpretación-de-resultados)
7. [Troubleshooting](#troubleshooting)
8. [Mantenimiento](#mantenimiento)
9. [Migración a Producción](#migración-a-producción)

---

## Descripción General

Los tests de integración validan el flujo end-to-end completo de envío de registros de facturación al Portal de Pruebas Externas de AEAT:

- ✅ Conexión TLS mutua con certificado digital
- ✅ Generación de XML conforme a XSD oficial
- ✅ Cálculo de huella SHA-256 y encadenamiento
- ✅ Firma electrónica XMLDSig
- ✅ Envío SOAP al sandbox de AEAT
- ✅ Parseo y validación de respuestas
- ✅ Manejo de errores y casos especiales

**IMPORTANTE**: Estos tests se ejecutan contra el **entorno de sandbox** de AEAT. Los datos enviados **NO tienen validez tributaria** y pueden ser eliminados periódicamente por AEAT.

---

## Requisitos Previos

### Software Necesario

- **.NET 9 SDK** o superior
- **Certificado digital válido** (PFX/PKCS#12 con clave privada)
- **Acceso a Internet** (puerto 443 abierto)
- **Visual Studio 2022**, **Visual Studio Code** o **dotnet CLI**

### Conocimientos Recomendados

- Conceptos básicos de SOAP y servicios web
- Familiaridad con certificados digitales y TLS
- Conocimientos de xUnit y testing de integración en .NET
- Entendimiento de VERI*FACTU (consultar `docs/Verifactu-Guia-Tecnica.md`)

### Obtención de Certificado

Los tests requieren un certificado digital válido. Opciones:

1. **Certificado de Representante** (recomendado)
   - Solicitar a FNMT-RCM, ACCV, Camerfirma u otras CA reconocidas
   - Formato: PFX/PKCS#12 con clave privada
   - Debe ser de representante de persona jurídica

2. **Certificado de Persona Física** (alternativo)
   - Con poder de representación
   - Válido para entorno de pruebas

**Consultar**: `docs/entorno-pruebas.md` sección "Configuración de Certificados"

---

## Configuración Inicial

### Opción 1: Usar appsettings.Sandbox.json (Desarrollo Local)

1. **Ubicar el archivo de configuración**:
   ```
   tests/Verifactu.Integration.Tests/appsettings.Sandbox.json
   ```

2. **Editar y configurar rutas del certificado**:
   ```json
   {
     "Certificado": {
       "PfxPath": "C:/Certificados/mi-certificado.pfx",
       "PfxPassword": "MiPasswordSegura123"
     }
   }
   ```

   **⚠️ ADVERTENCIA**: NO versionar este archivo con credenciales reales. Usar `.gitignore` o método 2.

### Opción 2: Usar User Secrets (Recomendado)

Más seguro para desarrollo local:

```bash
# Navegar al proyecto de tests
cd tests/Verifactu.Integration.Tests

# Inicializar user secrets
dotnet user-secrets init

# Configurar certificado
dotnet user-secrets set "Certificado:PfxPath" "/ruta/completa/al/certificado.pfx"
dotnet user-secrets set "Certificado:PfxPassword" "TuPasswordSegura"

# Verificar
dotnet user-secrets list
```

### Opción 3: Variables de Entorno (CI/CD)

Para pipelines de integración continua:

```bash
# Linux/macOS
export Certificado__PfxPath="/ruta/al/certificado.pfx"
export Certificado__PfxPassword="PasswordSegura"

# Windows PowerShell
$env:Certificado__PfxPath="C:\Certificados\certificado.pfx"
$env:Certificado__PfxPassword="PasswordSegura"
```

**Nota**: Usar `__` (doble guion bajo) para niveles jerárquicos en variables de entorno.

### Configuración de Datos de Prueba

Editar `appsettings.Sandbox.json` para personalizar datos del emisor/receptor:

```json
{
  "IntegrationTests": {
    "Emisor": {
      "Nif": "B12345678",
      "Nombre": "TU EMPRESA PRUEBAS SL"
    },
    "Receptor": {
      "Nif": "12345678A",
      "Nombre": "CLIENTE PRUEBAS"
    }
  }
}
```

---

## Ejecución de Tests

### Comando Básico

Ejecutar todos los tests de integración:

```bash
# Desde la raíz del repositorio
dotnet test tests/Verifactu.Integration.Tests

# O con ruta completa
dotnet test /home/usuario/Veri-factuSender/tests/Verifactu.Integration.Tests
```

### Filtrar por Categorías

```bash
# Solo tests de sandbox
dotnet test --filter "Category=Sandbox"

# Solo tests de integración (incluye sandbox)
dotnet test --filter "Category=Integration"

# Ejecutar test específico
dotnet test --filter "FullyQualifiedName~EnviarFacturaCompleta_F1"
```

### Opciones Útiles

```bash
# Con verbosidad detallada
dotnet test --logger "console;verbosity=detailed"

# Con generación de reporte TRX
dotnet test --logger "trx;LogFileName=integration-results.trx"

# Con recolección de cobertura de código
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar en paralelo (cuidado con límites del sandbox)
dotnet test --parallel

# Ejecutar secuencialmente
dotnet test --parallel 1
```

### Ejecución desde Visual Studio

1. Abrir la solución `VerifactuSender.sln`
2. Abrir **Test Explorer** (Ver → Test Explorer)
3. Localizar `Verifactu.Integration.Tests`
4. Clic derecho → **Run** o **Debug**
5. Revisar resultados en Test Explorer

### Ejecución desde Visual Studio Code

1. Instalar extensión **C# Dev Kit**
2. Abrir carpeta del repositorio
3. Ir a panel de **Testing** (icono de matraz)
4. Expandir `Verifactu.Integration.Tests`
5. Ejecutar tests individuales o todos

---

## Tests Implementados

### Test 1: Conexión TLS Mutua

**Objetivo**: Validar que el certificado está correctamente configurado y se puede establecer conexión TLS mutua con el sandbox.

**Archivo**: `SandboxTests.cs`  
**Método**: `ConexionTLSMutua_ConCertificadoValido_DebeConectar()`

**Qué valida**:
- Certificado carga correctamente
- Handshake TLS exitoso
- Servidor AEAT acepta el certificado
- No hay errores de SSL/TLS

**Resultado esperado**: No debe haber errores de certificado o TLS. Errores de validación XML o negocio son aceptables en este test.

### Test 2: Envío Factura Completa (F1)

**Objetivo**: Validar el flujo completo de envío de una factura tipo F1.

**Método**: `EnviarFacturaCompleta_F1_DebeRecibirRespuestaAEAT()`

**Qué valida**:
- Generación de XML conforme a XSD
- Cálculo de huella SHA-256
- Firma XMLDSig correcta
- Envío SOAP exitoso
- Parseo de respuesta AEAT
- Validación de CSV en respuesta exitosa

**Resultado esperado**: 
- Estado de envío: `Correcto`, `ParcialmenteCorrecto` o `Incorrecto`
- Si es `Correcto`: debe tener CSV y estado de registro correcto
- Debe actualizar huella para encadenamiento

### Test 3: Envío Factura Simplificada (F2)

**Objetivo**: Validar envío de factura simplificada.

**Método**: `EnviarFacturaSimplificada_F2_DebeRecibirRespuestaAEAT()`

**Qué valida**:
- Flujo similar a F1 pero con requisitos de F2
- Datos del receptor menos estrictos

**Resultado esperado**: Similar a Test 2

### Test 4: Envío Factura Rectificativa (R1)

**Objetivo**: Validar envío de factura rectificativa.

**Método**: `EnviarFacturaRectificativa_R1_DebeRecibirRespuestaAEAT()`

**Qué valida**:
- Generación de factura rectificativa
- Estructura XML para rectificativas
- Validación AEAT de rectificativas

**Resultado esperado**: Respuesta de AEAT (puede requerir factura original previa)

### Test 5: Manejo de Errores de Validación

**Objetivo**: Verificar que errores se reportan correctamente.

**Método**: `EnviarFacturaConDatosInvalidos_DebeRetornarErrorValidacion()`

**Qué valida**:
- Envío de factura con importe negativo
- AEAT detecta el error
- Respuesta contiene código y descripción de error

**Resultado esperado**: 
- Estado: `Incorrecto`
- Código de error no nulo
- Descripción de error no nula

### Test 6: Encadenamiento de Registros

**Objetivo**: Validar encadenamiento de huellas entre facturas consecutivas.

**Método**: `EnviarFacturasEncadenadas_DebeValidarEncadenamiento()`

**Qué valida**:
- Primera factura sin huella anterior
- Segunda factura con huella de la primera
- Huellas calculadas correctamente
- Encadenamiento aceptado por AEAT

**Resultado esperado**: Ambas facturas enviadas exitosamente, huellas diferentes

### Test 7: Detección de Duplicados

**Objetivo**: Verificar detección de registros duplicados.

**Método**: `EnviarFacturaDuplicada_DebeDetectarDuplicado()`

**Qué valida**:
- Primer envío exitoso
- Segundo envío del mismo registro
- AEAT detecta duplicado (o acepta idempotentemente)

**Resultado esperado**: 
- Primera factura: `Correcto`
- Segunda factura: Puede indicar duplicado o aceptar idempotentemente

---

## Interpretación de Resultados

### Estados de Respuesta AEAT

| Estado | Significado | Acción |
|--------|-------------|--------|
| `Correcto` | Registro aceptado sin errores | ✅ Test exitoso |
| `ParcialmenteCorrecto` | Algunos registros aceptados, otros rechazados | ⚠️ Revisar líneas de respuesta |
| `Incorrecto` | Registro rechazado | ❌ Revisar códigos de error |

### Códigos de Error Comunes

| Código | Descripción | Solución |
|--------|-------------|----------|
| `4001` | NIF del emisor no identificado | Verificar NIF en configuración |
| `3001` | Registro duplicado | Normal en test de duplicados |
| `5001` | Error de validación XML | Revisar estructura XML |
| `6001` | Huella incorrecta | Verificar algoritmo de cálculo |
| `7001` | Firma electrónica inválida | Verificar certificado de firma |

**Consultar**: `docs/gestion-errores-aeat.md` para lista completa de códigos de error.

### Logs y Debugging

Los tests generan logs detallados configurados en `appsettings.Sandbox.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Verifactu": "Trace"
    }
  }
}
```

**Ver logs en tiempo real**:

```bash
# Durante ejecución de tests
dotnet test --logger "console;verbosity=detailed"
```

**Logs incluyen**:
- Peticiones SOAP completas
- Respuestas SOAP completas
- Huellas calculadas
- XML generado y firmado
- Errores detallados

---

## Troubleshooting

### Error: "Test omitido: No hay certificado configurado"

**Síntoma**: Todos los tests se saltan (skipped).

**Causa**: Certificado no configurado o ruta incorrecta.

**Solución**:
1. Verificar ruta del certificado:
   ```bash
   ls -la /ruta/al/certificado.pfx  # Linux/macOS
   dir C:\Certificados\certificado.pfx  # Windows
   ```
2. Verificar configuración:
   ```bash
   dotnet user-secrets list
   ```
3. Verificar que el archivo existe y tiene extensión `.pfx` o `.p12`

### Error: "SSL connection could not be established"

**Síntoma**: Fallo en handshake TLS.

**Causa**: Certificado inválido, expirado o sin clave privada.

**Solución**:
```bash
# Verificar certificado con OpenSSL
openssl pkcs12 -in certificado.pfx -nokeys -info

# Verificar fechas de validez
openssl pkcs12 -in certificado.pfx -nokeys | openssl x509 -noout -dates

# Verificar que tiene clave privada
openssl pkcs12 -in certificado.pfx -nocerts -nodes
```

### Error: "SOAP Fault - Error de validación del XML"

**Síntoma**: Error de validación XML en respuesta AEAT.

**Causa**: XML no conforme a XSD oficial.

**Solución**:
1. Revisar logs de petición SOAP
2. Validar XML contra XSD local:
   ```bash
   xmllint --noout --schema SuministroLR.xsd registro.xml
   ```
3. Comparar con ejemplos oficiales de AEAT
4. Verificar namespaces correctos

### Error: "Timeout durante envío"

**Síntoma**: Tests fallan por timeout.

**Causa**: Sandbox puede ser lento, especialmente en horas pico.

**Solución**:
Aumentar timeout en `appsettings.Sandbox.json`:
```json
{
  "Verifactu": {
    "Timeout": 120
  }
}
```

### Error: "Huella inválida o encadenamiento incorrecto"

**Síntoma**: AEAT rechaza huella calculada.

**Causa**: Algoritmo de hash o campos incorrectos.

**Solución**:
1. Verificar que se usa SHA-256
2. Verificar campos exactos según especificación AEAT
3. Consultar `docs/algoritmo-huella.md`
4. Comparar con ejemplo conocido

### Error: "403 Forbidden - Certificado no autorizado"

**Síntoma**: AEAT rechaza el certificado.

**Causa**: Certificado no es de representante o NIF no coincide.

**Solución**:
1. Verificar que el certificado es de representante
2. Verificar que el NIF del certificado coincide con el emisor
3. Verificar que el certificado no está revocado
4. Usar certificado válido para entorno de pruebas

---

## Mantenimiento

### Actualización de Datos de Prueba

Editar `factura-sandbox-ejemplo.json` para actualizar facturas de prueba:

```json
{
  "Serie": "TEST",
  "Numero": "NUEVA-0001",
  "FechaEmision": "2025-12-01T10:00:00Z",
  ...
}
```

### Añadir Nuevos Tests

1. **Crear método de test en `SandboxTests.cs`**:
   ```csharp
   [Fact]
   [Trait("Category", "Integration")]
   [Trait("Category", "Sandbox")]
   public async Task NuevoTest_Descripcion_DebeValidarAlgo()
   {
       // Arrange
       if (_skipTests) { return; } // Test omitido: No hay certificado configurado
       
       // Act
       // ... código del test
       
       // Assert
       // ... validaciones
   }
   ```

2. **Seguir patrones existentes**:
   - Usar early return `if (_skipTests) { return; }` para omitir sin certificado
   - Generar números de factura únicos con timestamp
   - Actualizar `_ultimaHuella` si el test es exitoso
   - Añadir logs con `System.Diagnostics.Debug.WriteLine`

3. **Documentar el test en esta guía**

### Actualización de Endpoints

Si AEAT cambia endpoints del sandbox, actualizar en `appsettings.Sandbox.json`:

```json
{
  "Verifactu": {
    "EndpointUrl": "https://nuevo-endpoint.aeat.es/...",
    "WsdlUrl": "https://nuevo-endpoint.aeat.es/.../wsdl"
  }
}
```

### Limpieza de Datos

El sandbox puede eliminar datos periódicamente. No es necesario limpiar manualmente.

Si se requiere limpiar huellas de encadenamiento:

```csharp
// En SandboxTests.cs
private static string? _ultimaHuella = null; // Resetear a null
```

---

## Migración a Producción

⚠️ **ADVERTENCIA**: NO ejecutar estos tests directamente contra producción. Crear tests separados para producción.

### Pasos para Crear Tests de Producción

1. **Crear nuevo proyecto**: `Verifactu.Production.Tests`

2. **Duplicar configuración**: 
   ```
   appsettings.Production.json
   ```
   Con endpoints de producción

3. **Adaptar tests**:
   - Usar datos reales y válidos
   - Implementar limpieza y rollback
   - Añadir validaciones exhaustivas
   - Configurar ejecución manual (no CI/CD)

4. **Configurar certificado de producción**:
   - Solo certificados de representante válidos
   - Verificar vigencia y no revocación
   - Almacenar en Azure Key Vault, AWS Secrets Manager, etc.

5. **Ejecutar en entorno controlado**:
   - Horarios de baja carga
   - Con supervisión manual
   - Documentar cada ejecución

**Consultar**: `docs/paso-a-produccion.md` para checklist completo.

---

## Referencias

### Documentación del Proyecto

- [Guía Técnica VERI*FACTU](../Verifactu-Guia-Tecnica.md)
- [Configuración de Entorno de Pruebas](../entorno-pruebas.md)
- [Gestión de Errores AEAT](../gestion-errores-aeat.md)
- [Algoritmo de Huella](../algoritmo-huella.md)
- [Paso a Producción](../paso-a-produccion.md)

### Documentación Oficial AEAT

- [Sede electrónica VERI*FACTU](https://sede.agenciatributaria.gob.es/Sede/iva/sistemas-informaticos-facturacion-verifactu.html)
- [Portal de Pruebas Externas](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/portal-pruebas-externas.html)
- [Información Técnica](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/informacion-tecnica.html)
- [Preguntas Frecuentes](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/preguntas-frecuentes.html)

### Herramientas

- [xUnit Documentation](https://xunit.net/)
- [.NET Testing Documentation](https://learn.microsoft.com/en-us/dotnet/core/testing/)
- [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

---

## Soporte

### Obtener Ayuda

- **Issues del proyecto**: [GitHub Issues](https://github.com/JoseRGWeb/Veri-factuSender/issues)
- **Documentación AEAT**: Consultar siempre la documentación oficial actualizada
- **FAQs AEAT**: Muchas dudas están resueltas en las FAQs oficiales

### Contribuir

Para mejorar los tests o la documentación:

1. Crear un [Issue](https://github.com/JoseRGWeb/Veri-factuSender/issues) describiendo la mejora
2. Enviar un Pull Request con los cambios
3. Consultar la [Guía de Desarrollo](../desarrollo.md)

---

**Última actualización**: 7 de noviembre de 2025  
**Versión del documento**: 1.0  
**Autor**: Equipo de desarrollo Veri-factuSender
