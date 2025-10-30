# Guía de Configuración del Entorno de Pruebas (Sandbox AEAT)

Esta guía proporciona instrucciones detalladas para configurar y utilizar el entorno de pruebas (sandbox) de VERI*FACTU de la AEAT.

## Índice

1. [Visión General](#visión-general)
2. [Requisitos Previos](#requisitos-previos)
3. [Acceso al Portal de Pruebas Externas](#acceso-al-portal-de-pruebas-externas)
4. [Configuración de Certificados](#configuración-de-certificados)
5. [Endpoints del Sandbox](#endpoints-del-sandbox)
6. [Configuración de Variables de Entorno](#configuración-de-variables-de-entorno)
7. [Archivos de Configuración](#archivos-de-configuración)
8. [Pruebas y Validación](#pruebas-y-validación)
9. [Solución de Problemas](#solución-de-problemas)
10. [Diferencias entre Sandbox y Producción](#diferencias-entre-sandbox-y-producción)

---

## Visión General

El **Portal de Pruebas Externas** de la AEAT proporciona un entorno completo para realizar pruebas de integración con VERI*FACTU sin trascendencia tributaria. Este entorno permite:

- Probar el envío de registros de facturación
- Validar la firma electrónica XMLDSig
- Verificar el cálculo de huellas (hash) y encadenado
- Probar la generación y validación de códigos QR
- Consultar registros enviados
- Validar la estructura XML contra los XSD oficiales

**Importante**: Los datos enviados al sandbox **NO tienen validez tributaria** y pueden ser eliminados periódicamente.

---

## Requisitos Previos

### Software Necesario

- **.NET 9 SDK** o superior
- **Certificado digital** válido (ver sección de certificados)
- **Acceso a Internet** (puerto 443 abierto para HTTPS)
- **Editor de texto** o IDE para editar archivos de configuración

### Conocimientos Recomendados

- Conceptos básicos de SOAP y servicios web
- Familiaridad con certificados digitales y TLS
- Conocimientos de XML y firma electrónica (XMLDSig)
- Entendimiento de SHA-256 y operaciones hash

---

## Acceso al Portal de Pruebas Externas

### URL del Portal

**Portal de Pruebas Externas**: https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/portal-pruebas-externas.html

### Proceso de Acceso

1. **Requisito**: Debes tener un **certificado electrónico válido** instalado en tu navegador
   - Certificado de representante de persona jurídica
   - Certificado de persona física con poder de representación
   - Certificado de entidad sin personalidad jurídica

2. **Acceso al portal**:
   - Navega a la URL del portal de pruebas
   - El navegador solicitará que selecciones un certificado
   - Selecciona tu certificado digital
   - Acepta y continúa

3. **Funcionalidades disponibles**:
   - Acceso a WSDL de pruebas
   - Descarga de esquemas XSD de preproducción
   - Documentación técnica actualizada
   - Ejemplos de peticiones y respuestas
   - Herramientas de validación

### Obtención de Artefactos Técnicos

Una vez dentro del portal, puedes descargar:

- **WSDL de pruebas**: `SistemaFacturacion.wsdl`
- **Esquemas XSD**: 
  - `SuministroLR.xsd`
  - `RespuestaSuministro.xsd`
  - `ConsultaLR.xsd`
  - `RespuestaConsultaLR.xsd`
  - `SuministroInformacion.xsd`
  - `EventosSIF.xsd`
  - `RespuestaValRegistNoVeriFactu.xsd`

**Ubicación típica de descarga**: https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/

---

## Configuración de Certificados

### Tipos de Certificados Aceptados

Para el entorno de pruebas, necesitas un certificado digital válido:

#### Certificados Válidos

1. **Certificado de Representante** (recomendado)
   - Emitido por la FNMT-RCM
   - Emitido por autoridades certificadoras reconocidas (ACCV, Camerfirma, etc.)
   - Debe incluir la clave privada

2. **Certificado de Persona Física**
   - Con poder de representación de la empresa
   - Válido para pruebas

3. **Certificado de Pruebas**
   - Algunos proveedores ofrecen certificados específicos para entornos de pruebas
   - Consulta con tu autoridad certificadora

#### Requisitos del Certificado

- Formato: **PFX/P12** (con clave privada incluida)
- Algoritmo: RSA 2048 bits o superior
- Estado: **Válido** (no caducado, no revocado)
- Propósito: Firma digital y autenticación de cliente

### Obtención del Certificado PFX

#### Si tienes el certificado en el navegador

**Windows (exportar desde el navegador)**:
```powershell
# 1. Abrir el navegador y acceder a Configuración > Privacidad y seguridad > Certificados
# 2. Exportar el certificado con clave privada
# 3. Elegir formato PFX/PKCS#12
# 4. Establecer una contraseña segura
# 5. Guardar en ubicación segura (ej: C:\Certificados\mi-certificado.pfx)
```

**Linux (exportar desde navegador)**:
```bash
# Firefox: Preferencias > Privacidad y Seguridad > Ver Certificados > Sus Certificados
# Exportar con formato PKCS#12 (.p12)
# Establecer contraseña
# Guardar en: ~/certificados/mi-certificado.pfx
```

#### Si tienes certificado y clave separados

```bash
# Combinar certificado (.cer/.crt) y clave privada (.key) en PFX
openssl pkcs12 -export \
  -in certificado.cer \
  -inkey clave-privada.key \
  -out mi-certificado.pfx \
  -name "Certificado AEAT Pruebas"
# Se solicitará una contraseña para proteger el PFX
```

### Seguridad del Certificado

⚠️ **Medidas de seguridad obligatorias**:

1. **Nunca versiones el archivo PFX** en Git
2. **Protege con contraseña fuerte** el archivo PFX
3. **Limita permisos del archivo**:
   ```bash
   # Linux/macOS
   chmod 600 /ruta/al/certificado.pfx
   
   # Windows PowerShell (como administrador)
   icacls "C:\Certificados\certificado.pfx" /inheritance:r
   icacls "C:\Certificados\certificado.pfx" /grant:r "%USERNAME%:R"
   ```
4. **No compartas** el certificado ni la contraseña
5. **Usa almacén seguro** en producción (Azure Key Vault, AWS Secrets Manager, etc.)

### Verificación del Certificado

```bash
# Verificar contenido del certificado PFX
openssl pkcs12 -in mi-certificado.pfx -nokeys -info

# Verificar fechas de validez
openssl pkcs12 -in mi-certificado.pfx -nokeys | openssl x509 -noout -dates

# Verificar que contiene clave privada
openssl pkcs12 -in mi-certificado.pfx -nocerts -nodes
```

---

## Endpoints del Sandbox

### Endpoints Principales

Los endpoints exactos del sandbox se obtienen del **WSDL de pruebas**. A continuación, se muestran las URL típicas (verificar con el WSDL actual):

#### Servicio de Envío de Registros

```
Endpoint: https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion
WSDL: https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl
```

**Operaciones disponibles**:
- `RegFacturacionAlta` - Alta de registro de facturación
- `RegFacturacionAnulacion` - Anulación de registro
- `ConsultaRegistros` - Consulta de registros enviados
- `ValidarQR` - Validación de código QR

#### Servicio de Validación QR

```
Endpoint: https://prewww1.aeat.es/wlpl/TIKE-CONT/ValidarQR
```

### Configuración de Red

#### Puertos Requeridos

- **Puerto 443 (HTTPS)**: Para comunicación con servicios AEAT
- **TLS 1.2 o superior**: Versión mínima de protocolo TLS

#### Configuración de Proxy (si aplica)

Si tu red utiliza proxy, configúralo:

```bash
# Variables de entorno (Linux/macOS)
export HTTPS_PROXY=http://proxy.empresa.com:8080
export NO_PROXY=localhost,127.0.0.1

# Windows PowerShell
$env:HTTPS_PROXY="http://proxy.empresa.com:8080"
```

O en código .NET:
```csharp
var handler = new HttpClientHandler
{
    Proxy = new WebProxy("http://proxy.empresa.com:8080"),
    UseProxy = true
};
var client = new HttpClient(handler);
```

---

## Configuración de Variables de Entorno

### Variables Requeridas

Para el entorno de pruebas, configura las siguientes variables:

#### Variables de Certificado

```bash
# Linux/macOS
export VERIFACTU_CERT_PATH="/home/usuario/certificados/certificado-pruebas.pfx"
export VERIFACTU_CERT_PASSWORD="TuPasswordSegura"

# Windows PowerShell
$env:VERIFACTU_CERT_PATH="C:\Certificados\certificado-pruebas.pfx"
$env:VERIFACTU_CERT_PASSWORD="TuPasswordSegura"
```

#### Variables de Endpoint

```bash
# Linux/macOS
export VERIFACTU_ENDPOINT_URL="https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion"
export VERIFACTU_SOAP_ACTION="RegFacturacionAlta"
export VERIFACTU_ENV="sandbox"

# Windows PowerShell
$env:VERIFACTU_ENDPOINT_URL="https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion"
$env:VERIFACTU_SOAP_ACTION="RegFacturacionAlta"
$env:VERIFACTU_ENV="sandbox"
```

#### Variables Opcionales

```bash
# Configuración de logging
export VERIFACTU_LOG_LEVEL="Debug"
export VERIFACTU_LOG_PATH="/var/log/verifactu"

# Configuración de reintentos
export VERIFACTU_MAX_RETRIES="3"
export VERIFACTU_RETRY_DELAY_MS="1000"

# Huella anterior (para encadenado)
export VERIFACTU_HUELLA_ANTERIOR=""
```

### Persistencia de Variables

#### Linux/macOS (bash)

Añade al archivo `~/.bashrc` o `~/.bash_profile`:

```bash
# Añadir al final del archivo
export VERIFACTU_CERT_PATH="/home/usuario/certificados/certificado-pruebas.pfx"
export VERIFACTU_CERT_PASSWORD="TuPasswordSegura"
export VERIFACTU_ENDPOINT_URL="https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion"
export VERIFACTU_ENV="sandbox"
```

Luego recarga:
```bash
source ~/.bashrc
```

#### Windows

**Método 1: PowerShell Profile**

Edita `$PROFILE` (crear si no existe):
```powershell
# Abrir/crear profile
notepad $PROFILE

# Añadir variables
$env:VERIFACTU_CERT_PATH="C:\Certificados\certificado-pruebas.pfx"
$env:VERIFACTU_CERT_PASSWORD="TuPasswordSegura"
$env:VERIFACTU_ENDPOINT_URL="https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion"
$env:VERIFACTU_ENV="sandbox"
```

**Método 2: Variables de Sistema**

```powershell
# Establecer variable de usuario permanente
[System.Environment]::SetEnvironmentVariable('VERIFACTU_ENDPOINT_URL', 'https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion', 'User')
[System.Environment]::SetEnvironmentVariable('VERIFACTU_ENV', 'sandbox', 'User')
```

⚠️ **No almacenes contraseñas en variables de sistema permanentes**. Usa User Secrets o gestores de secretos.

---

## Archivos de Configuración

### appsettings.Sandbox.json

Crea un archivo de configuración específico para el sandbox:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Certificado": {
    "PfxPath": "",
    "PfxPassword": "",
    "StoreLocation": "CurrentUser",
    "StoreName": "My",
    "Thumbprint": ""
  },
  "Verifactu": {
    "Environment": "Sandbox",
    "EndpointUrl": "https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion",
    "WsdlUrl": "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl",
    "SoapAction": "RegFacturacionAlta",
    "Timeout": 30,
    "MaxRetries": 3,
    "RetryDelayMs": 1000,
    "HuellaAnterior": "",
    "ValidarXmlContraXsd": true,
    "XsdBasePath": "./xsd"
  },
  "Features": {
    "EnableDetailedErrors": true,
    "EnableRequestLogging": true,
    "EnableResponseLogging": true,
    "ValidateBeforeSend": true
  }
}
```

### Uso de User Secrets (Desarrollo)

Para proteger las credenciales en desarrollo:

```bash
# Inicializar user secrets
cd src/Verifactu.ConsoleDemo
dotnet user-secrets init

# Establecer secretos
dotnet user-secrets set "Certificado:PfxPath" "/ruta/certificado.pfx"
dotnet user-secrets set "Certificado:PfxPassword" "TuPasswordSegura"
dotnet user-secrets set "Verifactu:EndpointUrl" "https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion"

# Listar secretos configurados
dotnet user-secrets list

# Eliminar un secreto
dotnet user-secrets remove "Certificado:PfxPassword"

# Limpiar todos los secretos
dotnet user-secrets clear
```

### factura-demo-sandbox.json

Crea un archivo de factura de ejemplo para el sandbox:

```json
{
  "Serie": "TEST",
  "Numero": "SANDBOX-0001",
  "FechaExpedicion": "2025-10-30T10:00:00Z",
  "Emisor": {
    "Nif": "B12345678",
    "Nombre": "EMPRESA PRUEBAS SANDBOX S.L."
  },
  "Receptor": {
    "Nif": "12345678A",
    "Nombre": "CLIENTE PRUEBAS SANDBOX"
  },
  "Lineas": [
    {
      "Descripcion": "Producto de prueba sandbox",
      "Cantidad": 1,
      "PrecioUnitario": 100.00,
      "TipoIva": 21,
      "ImporteLinea": 100.00
    },
    {
      "Descripcion": "Servicio de prueba sandbox",
      "Cantidad": 2,
      "PrecioUnitario": 50.00,
      "TipoIva": 21,
      "ImporteLinea": 100.00
    }
  ],
  "TotalSinIva": 200.00,
  "TotalIva": 42.00,
  "TotalFactura": 242.00,
  "FormaPago": "Transferencia",
  "Observaciones": "Factura de prueba para entorno sandbox AEAT"
}
```

### Variables en appsettings por Entorno

Estructura recomendada de archivos:

```
src/Verifactu.ConsoleDemo/
├── appsettings.json                  # Configuración base
├── appsettings.Development.json      # Desarrollo local
├── appsettings.Sandbox.json          # Sandbox/Pruebas
├── appsettings.Production.json       # Producción
├── factura-demo.json                 # Factura de ejemplo
└── factura-demo-sandbox.json         # Factura sandbox
```

Seleccionar entorno al ejecutar:

```bash
# Linux/macOS
export ASPNETCORE_ENVIRONMENT=Sandbox
dotnet run

# Windows PowerShell
$env:ASPNETCORE_ENVIRONMENT="Sandbox"
dotnet run

# O directamente en el comando
dotnet run --environment Sandbox
```

---

## Pruebas y Validación

### Verificación de Conectividad

Antes de enviar registros, verifica la conectividad:

```bash
# Verificar acceso al endpoint (debe responder con error SOAP, no timeout)
curl -v https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion

# Verificar descarga de WSDL (requiere certificado en el navegador)
curl -v https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl
```

### Ejecución de Pruebas

#### Prueba Básica

```bash
cd src/Verifactu.ConsoleDemo

# Configurar entorno
export ASPNETCORE_ENVIRONMENT=Sandbox

# Ejecutar con factura de prueba
dotnet run

# O especificar archivo de factura
dotnet run -- --factura factura-demo-sandbox.json
```

#### Validación de XML contra XSD

Antes de enviar, valida el XML generado:

```bash
# Descargar XSD del portal de pruebas
mkdir -p xsd
cd xsd
# Descargar desde portal de pruebas (requiere autenticación)

# Validar XML generado (puede usar herramientas como xmllint)
xmllint --noout --schema SuministroLR.xsd registro-generado.xml
```

#### Prueba de Firma

Verifica que la firma XML sea válida:

```csharp
// Código de ejemplo para verificar firma
var doc = new XmlDocument();
doc.PreserveWhitespace = true;
doc.LoadXml(xmlFirmado);

var signedXml = new SignedXml(doc);
var signatureNode = doc.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl)[0];
signedXml.LoadXml((XmlElement)signatureNode);

bool isValid = signedXml.CheckSignature();
Console.WriteLine($"Firma válida: {isValid}");
```

### Casos de Prueba Recomendados

Ejecuta estos casos de prueba en el sandbox:

1. **Envío de registro básico**
   - Una factura con datos mínimos requeridos
   - Validar respuesta exitosa de AEAT

2. **Encadenamiento de registros**
   - Enviar primer registro (sin huella anterior)
   - Enviar segundo registro (con huella del primero)
   - Verificar encadenamiento correcto

3. **Manejo de errores**
   - Enviar registro con datos inválidos
   - Verificar códigos de error devueltos
   - Comprobar mensajes de error descriptivos

4. **Duplicados**
   - Enviar mismo UUID dos veces
   - Verificar detección de duplicados

5. **Consulta de registros**
   - Enviar registro
   - Consultar por UUID
   - Verificar datos devueltos

6. **Generación y validación de QR**
   - Generar código QR del registro
   - Validar QR contra servicio de cotejo
   - Verificar datos en respuesta

### Logs y Depuración

Habilita logging detallado en sandbox:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Verifactu": "Trace",
      "System.Net.Http": "Information"
    }
  }
}
```

Revisar logs generados:

```bash
# Ver logs en tiempo real (si se configuró archivo)
tail -f logs/verifactu-sandbox.log

# Filtrar errores
grep "ERROR" logs/verifactu-sandbox.log

# Filtrar peticiones SOAP
grep "SOAP Request" logs/verifactu-sandbox.log
```

---

## Solución de Problemas

### Problemas Comunes y Soluciones

#### Error: "No se puede establecer conexión TLS"

**Síntomas**:
```
System.Net.Http.HttpRequestException: The SSL connection could not be established
```

**Causas**:
- Certificado no configurado correctamente
- TLS mutual authentication no funciona
- Certificado expirado o inválido

**Soluciones**:
```csharp
// Verificar que el certificado se carga correctamente
var cert = new X509Certificate2(pfxPath, password);
Console.WriteLine($"Certificado: {cert.Subject}");
Console.WriteLine($"Válido desde: {cert.NotBefore}");
Console.WriteLine($"Válido hasta: {cert.NotAfter}");
Console.WriteLine($"Tiene clave privada: {cert.HasPrivateKey}");

// Verificar que se incluye en la petición HTTP
var handler = new HttpClientHandler();
handler.ClientCertificates.Add(cert);
```

#### Error: "SOAP Fault - Validación XML"

**Síntomas**:
```xml
<faultstring>Error de validación del XML</faultstring>
```

**Causas**:
- XML no conforme a XSD
- Namespaces incorrectos
- Campos obligatorios faltantes

**Soluciones**:
1. Validar contra XSD local antes de enviar
2. Comparar con ejemplos del portal de pruebas
3. Verificar namespaces:
   ```xml
   xmlns:sfe="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SuministroLR.xsd"
   ```

#### Error: "UUID duplicado"

**Síntomas**:
```
Código error: 1001 - Registro duplicado
```

**Causa**:
- Reenvío del mismo UUID

**Solución**:
```csharp
// Generar nuevo UUID para cada registro
var uuid = Guid.NewGuid().ToString();

// O usar formato específico si lo requiere AEAT
var uuid = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}";
```

#### Error: "Huella inválida o encadenamiento incorrecto"

**Síntomas**:
```
Error de validación: huella no coincide
```

**Causas**:
- Algoritmo de hash incorrecto
- Campos usados para calcular hash incorrectos
- Huella anterior incorrecta

**Soluciones**:
1. Verificar algoritmo (debe ser SHA-256)
2. Verificar campos exactos usados en el cálculo
3. Normalización de textos (encoding, mayúsculas, espacios)
4. Verificar que la huella anterior es la del último registro enviado

#### Error de Timeout

**Síntomas**:
```
System.Threading.Tasks.TaskCanceledException: The request was canceled due to the configured HttpClient.Timeout
```

**Solución**:
```csharp
// Aumentar timeout para sandbox (puede ser más lento)
var client = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(60)
};
```

#### Error: "Certificado no autorizado"

**Síntomas**:
```
403 Forbidden - Certificado no válido para esta operación
```

**Causas**:
- Certificado no es de representante
- Certificado expirado
- NIF del certificado no coincide con el emisor

**Soluciones**:
1. Verificar que el certificado es de representante de la empresa
2. Verificar fechas de validez
3. Verificar que el NIF del certificado corresponde al emisor de la factura

### Herramientas de Diagnóstico

#### Inspeccionar Tráfico SOAP

Usa Fiddler o Wireshark para inspeccionar peticiones:

```bash
# En código, habilitar logging de HTTP
var handler = new HttpClientHandler();
var client = new HttpClient(handler);

// .NET 5+ incluye logging automático si se configura
services.AddHttpClient<VerifactuSoapClient>()
    .ConfigureHttpClient(client => 
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<LoggingHandler>(); // Handler personalizado
```

#### Validar XML Offline

```bash
# Instalar xmllint (Linux/macOS)
sudo apt-get install libxml2-utils  # Debian/Ubuntu
brew install libxml2  # macOS

# Validar
xmllint --noout --schema SuministroLR.xsd mi-registro.xml
```

#### Verificar Conectividad AEAT

```bash
# Verificar resolución DNS
nslookup prewww1.aeat.es

# Verificar conectividad TLS
openssl s_client -connect prewww1.aeat.es:443 -servername prewww1.aeat.es

# Verificar con certificado cliente
openssl s_client -connect prewww1.aeat.es:443 \
  -cert certificado.pem \
  -key clave-privada.key \
  -servername prewww1.aeat.es
```

---

## Diferencias entre Sandbox y Producción

### Diferencias Técnicas

| Aspecto | Sandbox | Producción |
|---------|---------|------------|
| **Endpoint** | `prewww1.aeat.es` | `www1.aeat.es` o según WSDL |
| **Datos** | Sin validez tributaria | Validez tributaria plena |
| **Validaciones** | Pueden ser más permisivas | Estrictas y completas |
| **Persistencia** | Datos pueden eliminarse | Datos persistentes |
| **Rate Limiting** | Más permisivo | Estricto |
| **Certificados** | Cualquier certificado válido | Solo certificados de representante activos |
| **Soporte** | Documentación y FAQs | Soporte oficial AEAT |

### Consideraciones para Migrar a Producción

1. **Actualizar endpoints**:
   ```json
   {
     "Verifactu": {
       "Environment": "Production",
       "EndpointUrl": "https://www1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion"
     }
   }
   ```

2. **Verificar certificado de producción**:
   - Debe ser certificado de representante válido
   - Vigente y no revocado
   - Asociado al NIF correcto

3. **Endurecer validaciones**:
   - Validar todos los campos contra especificaciones
   - Implementar validaciones de negocio
   - No asumir que sandbox comportamiento = producción

4. **Aumentar observabilidad**:
   - Logging completo de todas las operaciones
   - Métricas de envíos exitosos/fallidos
   - Alertas para errores críticos

5. **Implementar gestión de errores robusta**:
   - Reintentos con exponential backoff
   - Cola de dead letters para fallos persistentes
   - Reconciliación manual si es necesario

6. **Backup de datos**:
   - Almacenar huellas de todos los registros
   - Backup de XML firmados
   - Acuses de recibo de AEAT

### Checklist de Paso a Producción

**Para una guía completa y detallada**, consulta el **[Checklist de Paso a Producción](paso-a-produccion.md)** que incluye:
- Validaciones previas exhaustivas
- Configuración técnica completa
- Gestión de certificados de producción
- Cambio de endpoints
- Configuración de seguridad
- Monitorización y logging
- Plan de contingencia y rollback
- Procedimiento de migración paso a paso

**Checklist resumido**:

- [ ] Todas las pruebas pasadas en sandbox
- [ ] Validación XML contra XSD oficial
- [ ] Certificado de producción obtenido y validado
- [ ] Endpoints de producción configurados
- [ ] Variables de entorno de producción configuradas
- [ ] Secrets almacenados de forma segura
- [ ] Logging y monitorización configurados
- [ ] Procedimientos de backup implementados
- [ ] Documentación operativa completa
- [ ] Plan de rollback definido
- [ ] Equipo entrenado en operación
- [ ] Contactos de soporte AEAT identificados

👉 **[Ver checklist completo de paso a producción](paso-a-produccion.md)**

---

## Referencias y Enlaces

### Documentación Oficial AEAT

- [Sede electrónica VERI*FACTU](https://sede.agenciatributaria.gob.es/Sede/iva/sistemas-informaticos-facturacion-verifactu.html)
- [Portal de Pruebas Externas](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/portal-pruebas-externas.html)
- [Información Técnica](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/informacion-tecnica.html)
- [Preguntas Frecuentes](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/preguntas-frecuentes.html)

### Documentación del Proyecto

- [Guía de Instalación](instalacion.md)
- [Guía Técnica VERI*FACTU](Verifactu-Guia-Tecnica.md)
- [Guía de Uso](uso.md)
- [Arquitectura](arquitectura.md)
- [Guía de Desarrollo](desarrollo.md)

### Herramientas Útiles

- [FNMT - Certificados](https://www.sede.fnmt.gob.es/certificados)
- [OpenSSL](https://www.openssl.org/)
- [Fiddler](https://www.telerik.com/fiddler) - Debugging proxy
- [SoapUI](https://www.soapui.org/) - Testing de servicios SOAP

---

## Soporte y Contribuciones

### Obtener Ayuda

- **Issues del proyecto**: [GitHub Issues](https://github.com/JoseRGWeb/Veri-factuSender/issues)
- **Documentación AEAT**: Consulta siempre la documentación oficial actualizada
- **FAQs AEAT**: Muchas dudas comunes están resueltas en las FAQs oficiales

### Contribuir

Si encuentras errores en esta documentación o deseas mejorarla:

1. Abre un [Issue](https://github.com/JoseRGWeb/Veri-factuSender/issues) describiendo el problema
2. O envía un Pull Request con las correcciones
3. Consulta la [Guía de Desarrollo](desarrollo.md) para más información

---

**Última actualización**: 30 de octubre de 2025  
**Versión del documento**: 1.0
