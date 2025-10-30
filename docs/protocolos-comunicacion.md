# Protocolos de Comunicación con AEAT

Esta guía detalla los protocolos de comunicación técnicos requeridos para la integración con los servicios web de VERI*FACTU de la AEAT, incluyendo especificaciones de TLS, autenticación, formatos de mensajes y control de errores.

## Índice

1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Protocolos de Seguridad y Transporte](#protocolos-de-seguridad-y-transporte)
3. [Autenticación y Certificados](#autenticación-y-certificados)
4. [Formatos de Mensajes](#formatos-de-mensajes)
5. [Control de Errores y Respuestas](#control-de-errores-y-respuestas)
6. [Diferencias entre Entornos](#diferencias-entre-entornos)
7. [Implementación en el Proyecto](#implementación-en-el-proyecto)
8. [Ejemplos Prácticos](#ejemplos-prácticos)
9. [Resolución de Problemas](#resolución-de-problemas)
10. [Referencias](#referencias)

---

## Resumen Ejecutivo

La comunicación con los servicios VERI*FACTU de la AEAT se realiza mediante:

- **Protocolo de transporte**: HTTPS sobre TLS 1.2 o superior
- **Arquitectura**: Servicios web SOAP 1.1
- **Autenticación**: TLS mutuo (mutual TLS) con certificados digitales X.509
- **Formato de datos**: XML conforme a esquemas XSD publicados por AEAT
- **Puerto**: 443 (HTTPS estándar)
- **Encoding**: UTF-8

---

## Protocolos de Seguridad y Transporte

### 1. TLS (Transport Layer Security)

#### Versiones Soportadas

| Versión TLS | Estado | Notas |
|-------------|--------|-------|
| **TLS 1.3** | ✅ Recomendado | Máxima seguridad y rendimiento |
| **TLS 1.2** | ✅ Soportado | Versión mínima requerida |
| **TLS 1.1** | ❌ No soportado | Obsoleto y inseguro |
| **TLS 1.0** | ❌ No soportado | Obsoleto y inseguro |
| **SSL 3.0 y anteriores** | ❌ No soportado | Vulnerables |

#### Configuración de TLS

**Requisitos obligatorios:**

1. **Versión mínima**: TLS 1.2
2. **Cipher suites**: Solo algoritmos fuertes (AES-GCM, ChaCha20)
3. **Perfect Forward Secrecy (PFS)**: Recomendado
4. **Validación de certificados**: Obligatoria para el servidor AEAT

**Cipher suites recomendados** (en orden de preferencia):

```
TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384
TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256
TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256
TLS_DHE_RSA_WITH_AES_256_GCM_SHA384
TLS_DHE_RSA_WITH_AES_128_GCM_SHA256
```

**Cipher suites a evitar:**
- Cualquiera que use RC4, DES, 3DES
- Algoritmos de cifrado de flujo débiles
- Modos CBC sin mitigación de ataques
- Algoritmos con claves de menos de 128 bits

#### Configuración en .NET

```csharp
// .NET 9 utiliza TLS 1.2+ por defecto
// Para configuración explícita:

using System.Net;
using System.Net.Security;

// Forzar TLS 1.2 o superior
ServicePointManager.SecurityProtocol = 
    SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

// Validación de certificados del servidor (recomendado en producción)
ServicePointManager.ServerCertificateValidationCallback = 
    (sender, cert, chain, sslPolicyErrors) =>
    {
        // En producción, validar correctamente el certificado
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;
            
        // Log del error para diagnóstico
        Console.WriteLine($"Error de certificado SSL: {sslPolicyErrors}");
        
        // En producción, retornar false si hay errores
        // En desarrollo/sandbox, puede ser más permisivo
        return false; // Cambiar según entorno
    };
```

#### HttpClient con TLS configurado

```csharp
var handler = new HttpClientHandler
{
    // TLS 1.2+ se utiliza automáticamente en .NET 9
    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
    
    // Validar certificados del servidor
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
    {
        // Implementar validación según políticas de seguridad
        return errors == SslPolicyErrors.None;
    }
};

var httpClient = new HttpClient(handler);
```

### 2. Autenticación Mutua TLS (mTLS)

La AEAT requiere **autenticación mutua** (mutual TLS), donde:

- **Cliente autentica al servidor**: Verificando el certificado SSL/TLS del servidor AEAT
- **Servidor autentica al cliente**: Mediante el certificado digital del representante

#### Flujo de autenticación mTLS

```
1. Cliente inicia conexión TLS → Servidor AEAT
2. Servidor envía su certificado → Cliente
3. Cliente valida certificado del servidor
4. Servidor solicita certificado de cliente
5. Cliente envía certificado de representante → Servidor
6. Servidor valida certificado del cliente
7. Si ambos certificados son válidos → Conexión establecida
8. Intercambio de mensajes SOAP sobre canal seguro
```

**Ventajas de mTLS:**
- Autenticación bidireccional fuerte
- No requiere usuario/contraseña adicional
- Protección contra man-in-the-middle
- Trazabilidad mediante certificados

---

## Autenticación y Certificados

### 1. Certificados Digitales X.509

#### Tipos de Certificados Aceptados

**Para Producción:**
- ✅ Certificado de representante de persona jurídica
- ✅ Certificado de entidad sin personalidad jurídica
- ✅ Certificado de persona física con poder de representación
- ❌ Certificados de empleado público (no válidos para este uso)
- ❌ Certificados de persona física sin representación

**Para Sandbox/Pruebas:**
- ✅ Cualquier certificado digital válido y no caducado
- ✅ Certificados de prueba emitidos por CA reconocidas
- ⚠️ Certificados autofirmados (pueden funcionar, no recomendado)

#### Requisitos del Certificado

**Algoritmo de firma:**
- RSA con SHA-256 o superior
- ECDSA con curvas P-256, P-384 o P-521

**Longitud de clave:**
- RSA: Mínimo 2048 bits (recomendado 4096 bits)
- ECDSA: Mínimo 256 bits

**Campos obligatorios del certificado:**
```
Subject: CN=Nombre o Razón Social, SERIALNUMBER=NIF, ...
Key Usage: Digital Signature, Key Encipherment
Extended Key Usage: Client Authentication (1.3.6.1.5.5.7.3.2)
```

**Validaciones que realiza AEAT:**
1. Certificado no caducado
2. Certificado no revocado (verificación CRL/OCSP)
3. Cadena de confianza válida hasta CA raíz reconocida
4. Propósitos de uso correctos (Client Authentication)
5. NIF del certificado coincide con NIF del emisor de facturas

### 2. Formato PFX/PKCS#12

Los certificados deben estar en formato **PFX (PKCS#12)** que incluye:
- Certificado público X.509
- Clave privada
- Cadena de certificados (opcional pero recomendado)

**Estructura de un archivo PFX:**
```
PFX Container
├── Private Key (clave privada RSA/ECDSA)
├── Certificate (certificado del titular)
└── CA Certificates (opcional: certificados de la CA)
```

#### Generación y conversión de certificados

**Generar PFX desde certificado y clave privada:**
```bash
# Combinar certificado (.cer) y clave privada (.key)
openssl pkcs12 -export \
  -in certificado.cer \
  -inkey clave-privada.key \
  -out certificado.pfx \
  -name "Certificado AEAT" \
  -passout pass:MiPasswordSegura
```

**Exportar desde almacén de Windows:**
```powershell
# Listar certificados del usuario actual
Get-ChildItem -Path Cert:\CurrentUser\My

# Exportar certificado con clave privada
$cert = Get-ChildItem -Path Cert:\CurrentUser\My\<Thumbprint>
$password = ConvertTo-SecureString -String "MiPassword" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "certificado.pfx" -Password $password
```

**Verificar contenido del PFX:**
```bash
# Ver información del certificado
openssl pkcs12 -in certificado.pfx -nokeys -info

# Verificar que contiene clave privada
openssl pkcs12 -in certificado.pfx -nocerts -nodes

# Extraer certificado en formato PEM
openssl pkcs12 -in certificado.pfx -clcerts -nokeys -out cert.pem
```

### 3. Carga de Certificados en .NET

#### Método Recomendado (.NET 9)

```csharp
using System.Security.Cryptography.X509Certificates;

// Usar X509CertificateLoader (nuevo en .NET 9)
public class CertificateLoader
{
    public static X509Certificate2 CargarCertificado(string rutaPfx, string password)
    {
        // Método recomendado en .NET 9+
        byte[] pfxBytes = File.ReadAllBytes(rutaPfx);
        
        return X509CertificateLoader.LoadPkcs12(
            pfxBytes, 
            password,
            X509KeyStorageFlags.MachineKeySet | 
            X509KeyStorageFlags.Exportable |
            X509KeyStorageFlags.PersistKeySet
        );
    }
    
    // Cargar desde almacén del sistema
    public static X509Certificate2 CargarDesdeAlmacen(string thumbprint)
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        
        var certs = store.Certificates.Find(
            X509FindType.FindByThumbprint, 
            thumbprint, 
            validOnly: true
        );
        
        if (certs.Count == 0)
            throw new Exception($"Certificado con thumbprint {thumbprint} no encontrado");
            
        return certs[0];
    }
}
```

#### Validaciones del Certificado

```csharp
public static void ValidarCertificado(X509Certificate2 certificado)
{
    // 1. Verificar que tiene clave privada
    if (!certificado.HasPrivateKey)
        throw new Exception("El certificado no contiene clave privada");
    
    // 2. Verificar fechas de validez
    DateTime ahora = DateTime.Now;
    if (ahora < certificado.NotBefore)
        throw new Exception("El certificado aún no es válido");
    if (ahora > certificado.NotAfter)
        throw new Exception("El certificado ha caducado");
    
    // 3. Verificar cadena de confianza
    var chain = new X509Chain();
    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
    
    if (!chain.Build(certificado))
    {
        var errores = string.Join(", ", 
            chain.ChainStatus.Select(s => s.StatusInformation));
        throw new Exception($"Cadena de certificados inválida: {errores}");
    }
    
    // 4. Verificar propósito de uso
    bool tieneClientAuth = certificado.Extensions
        .OfType<X509EnhancedKeyUsageExtension>()
        .Any(ext => ext.EnhancedKeyUsages
            .Cast<Oid>()
            .Any(oid => oid.Value == "1.3.6.1.5.5.7.3.2")); // Client Authentication
    
    if (!tieneClientAuth)
        Console.WriteLine("Advertencia: El certificado puede no tener Client Authentication");
}
```

### 4. Uso del Certificado en HttpClient

```csharp
public class VerifactuHttpClient
{
    private readonly HttpClient _httpClient;
    
    public VerifactuHttpClient(X509Certificate2 certificado)
    {
        // Validar certificado antes de usar
        ValidarCertificado(certificado);
        
        // Crear handler con certificado cliente
        var handler = new HttpClientHandler();
        
        // Agregar certificado para autenticación mutua TLS
        handler.ClientCertificates.Add(certificado);
        
        // Configuración de seguridad
        handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
        handler.CheckCertificateRevocationList = true;
        
        // Crear cliente HTTP
        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        // Configurar headers por defecto
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "VerifactuSender/1.0");
    }
    
    public async Task<string> PostAsync(string url, string contenido)
    {
        var content = new StringContent(contenido, Encoding.UTF8, "text/xml");
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

---

## Formatos de Mensajes

### 1. Arquitectura SOAP 1.1

Los servicios VERI*FACTU utilizan **SOAP 1.1** sobre HTTPS.

#### Características de SOAP 1.1

- **Namespace**: `http://schemas.xmlsoap.org/soap/envelope/`
- **Encoding**: UTF-8
- **Content-Type**: `text/xml; charset=utf-8`
- **SOAPAction**: Header HTTP obligatorio (valor definido en WSDL)

### 2. Estructura de Mensajes SOAP

#### Anatomía de una Petición SOAP

```xml
<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope 
    xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
    xmlns:sfe="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SuministroLR.xsd">
    
    <!-- Cabecera SOAP (opcional en VERI*FACTU) -->
    <soapenv:Header>
        <!-- Headers de seguridad WS-Security si se requieren -->
    </soapenv:Header>
    
    <!-- Cuerpo SOAP (obligatorio) -->
    <soapenv:Body>
        <!-- Operación específica según WSDL -->
        <sfe:SuministroLRFacturasEmitidas>
            <sfe:Cabecera>
                <sfe:IDVersion>1.0</sfe:IDVersion>
                <sfe:Titular>
                    <sfe:NIF>B12345678</sfe:NIF>
                    <sfe:NombreRazon>EMPRESA EJEMPLO SL</sfe:NombreRazon>
                </sfe:Titular>
            </sfe:Cabecera>
            <sfe:RegistroLRFacturasEmitidas>
                <!-- Datos del registro de facturación -->
            </sfe:RegistroLRFacturasEmitidas>
        </sfe:SuministroLRFacturasEmitidas>
    </soapenv:Body>
</soapenv:Envelope>
```

#### Headers HTTP Requeridos

```http
POST /wlpl/TIKE-CONT/SistemaFacturacion HTTP/1.1
Host: www1.aeat.es (o prewww1.aeat.es para sandbox)
Content-Type: text/xml; charset=utf-8
Content-Length: [longitud del mensaje]
SOAPAction: "RegFacturacionAlta"
User-Agent: VerifactuSender/1.0
Accept: text/xml, application/xml
Connection: keep-alive
```

### 3. Operaciones SOAP Disponibles

Según el WSDL de VERI*FACTU, las operaciones principales son:

#### 3.1. RegFacturacionAlta

**Propósito**: Alta de un nuevo registro de facturación

**SOAPAction**: `RegFacturacionAlta`

**Estructura de petición:**
```xml
<soapenv:Body>
    <sfe:SuministroLRFacturasEmitidas>
        <sfe:Cabecera>
            <sfe:IDVersion>1.0</sfe:IDVersion>
            <sfe:Titular>
                <sfe:NIF>B12345678</sfe:NIF>
                <sfe:NombreRazon>EMPRESA SL</sfe:NombreRazon>
            </sfe:Titular>
        </sfe:Cabecera>
        <sfe:RegistroLRFacturasEmitidas>
            <sfe:IDFactura>
                <sfe:IDEmisorFactura>B12345678</sfe:IDEmisorFactura>
                <sfe:NumSerieFactura>A/001</sfe:NumSerieFactura>
                <sfe:FechaExpedicionFactura>30-10-2025</sfe:FechaExpedicionFactura>
            </sfe:IDFactura>
            <sfe:Huella>SHA256_HASH_AQUI</sfe:Huella>
            <!-- Más campos según XSD -->
        </sfe:RegistroLRFacturasEmitidas>
    </sfe:SuministroLRFacturasEmitidas>
</soapenv:Body>
```

**Respuesta exitosa:**
```xml
<soapenv:Body>
    <sfe:RespuestaLRFacturasEmitidas>
        <sfe:Cabecera>
            <sfe:IDVersion>1.0</sfe:IDVersion>
        </sfe:Cabecera>
        <sfe:RespuestaLinea>
            <sfe:IDFactura>
                <sfe:IDEmisorFactura>B12345678</sfe:IDEmisorFactura>
                <sfe:NumSerieFactura>A/001</sfe:NumSerieFactura>
                <sfe:FechaExpedicionFactura>30-10-2025</sfe:FechaExpedicionFactura>
            </sfe:IDFactura>
            <sfe:EstadoRegistro>Aceptado</sfe:EstadoRegistro>
            <sfe:CodigoSeguro>ABC123XYZ</sfe:CodigoSeguro>
            <sfe:CSV>1234567890</sfe:CSV>
        </sfe:RespuestaLinea>
    </sfe:RespuestaLRFacturasEmitidas>
</soapenv:Body>
```

#### 3.2. RegFacturacionAnulacion

**Propósito**: Anulación de un registro previamente enviado

**SOAPAction**: `RegFacturacionAnulacion`

#### 3.3. ConsultaLRFacturas

**Propósito**: Consulta de registros de facturación enviados

**SOAPAction**: `ConsultaLRFacturas`

**Parámetros de consulta:**
- Por rango de fechas
- Por número de factura
- Por NIF emisor/receptor
- Por estado del registro

### 4. Esquemas XSD

Los mensajes XML deben cumplir estrictamente con los esquemas XSD publicados por AEAT.

#### Esquemas Principales

| Esquema XSD | Propósito |
|-------------|-----------|
| `SuministroLR.xsd` | Estructura de registros de facturación (altas/anulaciones) |
| `RespuestaSuministro.xsd` | Estructura de respuestas a envíos |
| `ConsultaLR.xsd` | Estructura de peticiones de consulta |
| `RespuestaConsultaLR.xsd` | Estructura de respuestas a consultas |
| `SuministroInformacion.xsd` | Tipos comunes y definiciones compartidas |
| `EventosSIF.xsd` | Registro de eventos (sistemas no VERI*FACTU) |

#### Namespaces Importantes

```xml
xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
xmlns:sfe="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SuministroLR.xsd"
xmlns:si="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SuministroInformacion.xsd"
```

### 5. Validación de XML

#### Validación Local antes del Envío

```csharp
using System.Xml;
using System.Xml.Schema;

public class XmlValidator
{
    public static bool ValidarContraXsd(string xmlContent, string xsdPath)
    {
        var settings = new XmlReaderSettings();
        settings.Schemas.Add(null, xsdPath);
        settings.ValidationType = ValidationType.Schema;
        
        bool esValido = true;
        settings.ValidationEventHandler += (sender, args) =>
        {
            esValido = false;
            Console.WriteLine($"Error de validación: {args.Message}");
        };
        
        using var reader = XmlReader.Create(
            new StringReader(xmlContent), 
            settings
        );
        
        while (reader.Read()) { }
        
        return esValido;
    }
}
```

### 6. Construcción de Mensajes SOAP en .NET

```csharp
public class SoapMessageBuilder
{
    public static string ConstruirMensajeAlta(
        string registroXml, 
        string nifTitular, 
        string nombreTitular)
    {
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope 
    xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
    xmlns:sfe=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SuministroLR.xsd"">
    <soapenv:Header/>
    <soapenv:Body>
        <sfe:SuministroLRFacturasEmitidas>
            <sfe:Cabecera>
                <sfe:IDVersion>1.0</sfe:IDVersion>
                <sfe:Titular>
                    <sfe:NIF>{nifTitular}</sfe:NIF>
                    <sfe:NombreRazon>{SecurityElement.Escape(nombreTitular)}</sfe:NombreRazon>
                </sfe:Titular>
            </sfe:Cabecera>
            {registroXml}
        </sfe:SuministroLRFacturasEmitidas>
    </soapenv:Body>
</soapenv:Envelope>";
    }
}
```

---

## Control de Errores y Respuestas

### 1. Códigos de Estado HTTP

| Código HTTP | Significado | Acción Recomendada |
|-------------|-------------|-------------------|
| **200 OK** | Petición procesada correctamente | Procesar respuesta SOAP |
| **400 Bad Request** | XML malformado o inválido | Revisar estructura XML |
| **401 Unauthorized** | Autenticación fallida | Verificar certificado |
| **403 Forbidden** | Certificado no autorizado | Verificar permisos del certificado |
| **404 Not Found** | Endpoint incorrecto | Verificar URL del servicio |
| **500 Internal Server Error** | Error del servidor AEAT | Reintentar más tarde |
| **502 Bad Gateway** | Proxy/Gateway error | Problema de red, reintentar |
| **503 Service Unavailable** | Servicio no disponible temporalmente | Reintentar con backoff |
| **504 Gateway Timeout** | Timeout en servidor | Aumentar timeout, reintentar |

### 2. SOAP Faults

Cuando hay errores en el procesamiento, AEAT devuelve un **SOAP Fault**:

```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
    <soapenv:Body>
        <soapenv:Fault>
            <faultcode>soapenv:Client</faultcode>
            <faultstring>Error de validación del registro</faultstring>
            <detail>
                <sfe:ErrorDetalle>
                    <sfe:CodigoError>1001</sfe:CodigoError>
                    <sfe:DescripcionError>El NIF del emisor no es válido</sfe:DescripcionError>
                    <sfe:CampoError>IDEmisorFactura</sfe:CampoError>
                </sfe:ErrorDetalle>
            </detail>
        </soapenv:Fault>
    </soapenv:Body>
</soapenv:Envelope>
```

**Tipos de faultcode:**
- `soapenv:Client` - Error en la petición del cliente (XML inválido, datos incorrectos)
- `soapenv:Server` - Error en el servidor AEAT (error interno, servicio no disponible)

### 3. Códigos de Error de AEAT

#### Errores de Validación Comunes

| Código | Descripción | Solución |
|--------|-------------|----------|
| **1001** | NIF inválido | Verificar formato y validez del NIF |
| **1002** | Fecha de expedición inválida | Formato: DD-MM-YYYY |
| **1003** | Serie/Número duplicado | Verificar unicidad del registro |
| **1004** | Huella (hash) inválida | Recalcular según algoritmo oficial |
| **1005** | Encadenamiento incorrecto | Verificar huella anterior |
| **1006** | Firma electrónica inválida | Regenerar firma XMLDSig |
| **1007** | Certificado no autorizado | Usar certificado de representante válido |
| **2001** | XML no conforme a XSD | Validar contra esquemas XSD |
| **2002** | Namespace incorrecto | Verificar xmlns en XML |
| **3001** | Campo obligatorio faltante | Completar todos los campos requeridos |
| **4001** | Registro no encontrado | Verificar UUID del registro |
| **5001** | Servicio temporalmente no disponible | Reintentar más tarde |

### 4. Estados de Registro

Tras el envío, cada registro puede tener uno de estos estados:

| Estado | Descripción | Siguiente Acción |
|--------|-------------|-----------------|
| **Aceptado** | Registro correctamente procesado y almacenado | Guardar CSV y código seguro |
| **AceptadoConErrores** | Registro aceptado pero con advertencias | Revisar warnings |
| **Rechazado** | Registro rechazado por errores de validación | Corregir errores y reenviar |
| **EnProceso** | Registro siendo procesado (asíncrono) | Consultar estado más tarde |

### 5. Manejo de Errores en Código

```csharp
public class SoapErrorHandler
{
    public static async Task<RespuestaAeat> EnviarConManejodeErrores(
        HttpClient client,
        string url,
        string soapMessage,
        string soapAction)
    {
        var maxReintentos = 3;
        var delayBase = TimeSpan.FromSeconds(2);
        
        for (int intento = 0; intento < maxReintentos; intento++)
        {
            try
            {
                var content = new StringContent(
                    soapMessage, 
                    Encoding.UTF8, 
                    "text/xml"
                );
                content.Headers.Add("SOAPAction", soapAction);
                
                var response = await client.PostAsync(url, content);
                
                // Leer respuesta
                var responseXml = await response.Content.ReadAsStringAsync();
                
                // Verificar si es un SOAP Fault
                if (EsSoapFault(responseXml))
                {
                    var error = ParsearSoapFault(responseXml);
                    
                    // Errores de cliente (4xx) no se reintentan
                    if (error.EsErrorDeCliente)
                        throw new SoapFaultException(error);
                    
                    // Errores de servidor (5xx) se pueden reintentar
                    if (intento < maxReintentos - 1)
                    {
                        await Task.Delay(delayBase * Math.Pow(2, intento));
                        continue;
                    }
                    
                    throw new SoapFaultException(error);
                }
                
                // Respuesta exitosa
                response.EnsureSuccessStatusCode();
                return ParsearRespuesta(responseXml);
            }
            catch (HttpRequestException ex) when (
                ex.StatusCode == HttpStatusCode.ServiceUnavailable ||
                ex.StatusCode == HttpStatusCode.GatewayTimeout)
            {
                // Reintentar en caso de errores transitorios
                if (intento < maxReintentos - 1)
                {
                    await Task.Delay(delayBase * Math.Pow(2, intento));
                    continue;
                }
                throw;
            }
        }
        
        throw new Exception("Máximo de reintentos alcanzado");
    }
    
    private static bool EsSoapFault(string xml)
    {
        return xml.Contains("<soapenv:Fault>") || 
               xml.Contains("<soap:Fault>");
    }
    
    private static ErrorAeat ParsearSoapFault(string xml)
    {
        var doc = XDocument.Parse(xml);
        var ns = doc.Root.GetDefaultNamespace();
        
        var fault = doc.Descendants(ns + "Fault").FirstOrDefault();
        if (fault == null)
            throw new Exception("No se encontró elemento Fault en respuesta");
        
        return new ErrorAeat
        {
            FaultCode = fault.Element(ns + "faultcode")?.Value,
            FaultString = fault.Element(ns + "faultstring")?.Value,
            EsErrorDeCliente = fault.Element(ns + "faultcode")?.Value
                .Contains("Client") ?? false
        };
    }
}

public class SoapFaultException : Exception
{
    public ErrorAeat Error { get; }
    
    public SoapFaultException(ErrorAeat error) 
        : base(error.FaultString)
    {
        Error = error;
    }
}

public class ErrorAeat
{
    public string FaultCode { get; set; }
    public string FaultString { get; set; }
    public string CodigoError { get; set; }
    public string DescripcionError { get; set; }
    public bool EsErrorDeCliente { get; set; }
}
```

### 6. Logging y Trazabilidad

```csharp
public class SoapLogger
{
    private readonly ILogger _logger;
    
    public async Task<string> EnviarConLogging(
        HttpClient client,
        string url,
        string soapRequest,
        string soapAction)
    {
        var requestId = Guid.NewGuid().ToString();
        
        _logger.LogInformation(
            "Enviando petición SOAP. RequestId: {RequestId}, " +
            "SOAPAction: {SOAPAction}, URL: {URL}",
            requestId, soapAction, url
        );
        
        // Log del request (sanitizar datos sensibles)
        _logger.LogDebug(
            "SOAP Request: {RequestId}\n{Request}",
            requestId, 
            SanitizarXml(soapRequest)
        );
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var content = new StringContent(
                soapRequest, 
                Encoding.UTF8, 
                "text/xml"
            );
            content.Headers.Add("SOAPAction", soapAction);
            
            var response = await client.PostAsync(url, content);
            var responseXml = await response.Content.ReadAsStringAsync();
            
            stopwatch.Stop();
            
            _logger.LogInformation(
                "Respuesta SOAP recibida. RequestId: {RequestId}, " +
                "StatusCode: {StatusCode}, Duración: {Duration}ms",
                requestId, 
                (int)response.StatusCode, 
                stopwatch.ElapsedMilliseconds
            );
            
            _logger.LogDebug(
                "SOAP Response: {RequestId}\n{Response}",
                requestId, 
                SanitizarXml(responseXml)
            );
            
            return responseXml;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(
                ex,
                "Error en petición SOAP. RequestId: {RequestId}, " +
                "Duración: {Duration}ms",
                requestId, 
                stopwatch.ElapsedMilliseconds
            );
            
            throw;
        }
    }
    
    private string SanitizarXml(string xml)
    {
        // Eliminar datos sensibles antes de loggear
        // (NIFs parciales, eliminar passwords, etc.)
        return xml; // Implementar sanitización según necesidades
    }
}
```

---

## Diferencias entre Entornos

### Comparativa Completa: Sandbox vs Producción

| Aspecto | Sandbox/Pruebas | Producción |
|---------|-----------------|------------|
| **URL Base** | `https://prewww1.aeat.es` | `https://www1.aeat.es` |
| **Endpoint SOAP** | `/wlpl/TIKE-CONT/SistemaFacturacion` | `/wlpl/TIKE-CONT/SistemaFacturacion` |
| **WSDL** | Acceso con certificado en portal de pruebas | URL de producción (documentada) |
| **Certificados** | Cualquier certificado válido | Solo certificados de representante activos |
| **TLS** | TLS 1.2+ | TLS 1.2+ (mismo requisito) |
| **Validaciones** | Pueden ser más permisivas | Estrictas y completas |
| **NIF** | Acepta NIFs de prueba | Solo NIFs reales y válidos |
| **Persistencia** | Datos pueden eliminarse periódicamente | Datos persistentes e inmutables |
| **Validez Tributaria** | **Ninguna** (solo pruebas) | **Plena validez tributaria** |
| **Rate Limiting** | Más permisivo | Límites estrictos (no documentados públicamente) |
| **Timeout** | Puede ser más lento | Optimizado para producción |
| **Consultas** | Disponibles | Disponibles |
| **Errores** | Pueden incluir información detallada | Mensajes de error estándar |
| **Monitorización** | No garantizada | Monitorización 24/7 |
| **Disponibilidad** | Puede tener mantenimientos | Alta disponibilidad (SLA) |

### 1. Configuración por Entorno

#### appsettings.Sandbox.json

```json
{
  "Verifactu": {
    "Environment": "Sandbox",
    "BaseUrl": "https://prewww1.aeat.es",
    "EndpointPath": "/wlpl/TIKE-CONT/SistemaFacturacion",
    "WsdlUrl": "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl",
    "TlsMinVersion": "Tls12",
    "Timeout": 60,
    "MaxRetries": 3,
    "EnableDetailedLogging": true,
    "ValidarCertificadoServidor": false,
    "ValidarXmlContraXsd": true
  },
  "Certificado": {
    "ValidationMode": "Permissive"
  }
}
```

#### appsettings.Production.json

```json
{
  "Verifactu": {
    "Environment": "Production",
    "BaseUrl": "https://www1.aeat.es",
    "EndpointPath": "/wlpl/TIKE-CONT/SistemaFacturacion",
    "WsdlUrl": "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl",
    "TlsMinVersion": "Tls12",
    "Timeout": 30,
    "MaxRetries": 5,
    "EnableDetailedLogging": false,
    "ValidarCertificadoServidor": true,
    "ValidarXmlContraXsd": true
  },
  "Certificado": {
    "ValidationMode": "Strict",
    "RequireRepresentativeCertificate": true,
    "CheckRevocation": true
  }
}
```

### 2. Código Adaptable a Entorno

```csharp
public class VerifactuClientFactory
{
    public static VerifactuSoapClient Create(
        IConfiguration configuration,
        X509Certificate2 certificado)
    {
        var env = configuration["Verifactu:Environment"];
        var baseUrl = configuration["Verifactu:BaseUrl"];
        var endpointPath = configuration["Verifactu:EndpointPath"];
        var endpoint = $"{baseUrl}{endpointPath}";
        
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(certificado);
        
        // Configuración específica por entorno
        if (env == "Production")
        {
            // Producción: validación estricta
            handler.ServerCertificateCustomValidationCallback = null; // Usar validación por defecto
            handler.CheckCertificateRevocationList = true;
        }
        else
        {
            // Sandbox: más permisivo para pruebas
            handler.ServerCertificateCustomValidationCallback = 
                (message, cert, chain, errors) =>
                {
                    // Permitir certificados auto-firmados en sandbox
                    return true; // Solo para desarrollo/sandbox
                };
        }
        
        var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(
                configuration.GetValue<int>("Verifactu:Timeout")
            )
        };
        
        return new VerifactuSoapClient(
            httpClient,
            endpoint,
            configuration["Verifactu:SOAPAction"]
        );
    }
}
```

### 3. Migración de Sandbox a Producción

#### Checklist de Migración

```markdown
## Checklist: Sandbox → Producción

### Configuración
- [ ] Actualizar endpoints a URLs de producción
- [ ] Configurar certificado de representante válido
- [ ] Verificar que el certificado no está próximo a caducar
- [ ] Configurar secrets en sistema seguro (Azure Key Vault, AWS Secrets Manager)
- [ ] Actualizar timeouts y reintentos para producción
- [ ] Activar validación estricta de certificados de servidor

### Validaciones
- [ ] Todos los tests pasan en sandbox
- [ ] XML validado contra XSD oficiales
- [ ] Huella (hash) calculada correctamente
- [ ] Firma XMLDSig generada y validada
- [ ] Encadenamiento de registros probado

### Seguridad
- [ ] TLS 1.2+ configurado
- [ ] Verificación de revocación de certificados activada
- [ ] Logging configurado sin exponer datos sensibles
- [ ] Certificados almacenados de forma segura
- [ ] Secrets rotados y protegidos

### Operaciones
- [ ] Monitorización configurada
- [ ] Alertas para errores críticos
- [ ] Procedimientos de backup implementados
- [ ] Plan de rollback documentado
- [ ] Documentación operativa completa
- [ ] Equipo capacitado

### Cumplimiento
- [ ] Verificar que NIF del certificado = NIF del emisor
- [ ] Confirmar poderes de representación vigentes
- [ ] Revisar normativa actualizada
- [ ] Contactos de soporte AEAT identificados
```

### 4. Variables de Entorno Recomendadas

```bash
# Sandbox
export VERIFACTU_ENV=Sandbox
export VERIFACTU_BASE_URL=https://prewww1.aeat.es
export VERIFACTU_CERT_VALIDATION=Permissive
export VERIFACTU_LOG_LEVEL=Debug

# Producción
export VERIFACTU_ENV=Production
export VERIFACTU_BASE_URL=https://www1.aeat.es
export VERIFACTU_CERT_VALIDATION=Strict
export VERIFACTU_LOG_LEVEL=Information
```

---

## Implementación en el Proyecto

### 1. Estructura del Cliente SOAP

El proyecto implementa la comunicación SOAP en:

```
src/Verifactu.Client/
├── Soap/
│   └── VerifactuSoapClient.cs    # Cliente SOAP con TLS mutuo
├── Services/
│   ├── CertificateLoader.cs      # Carga de certificados
│   ├── HashService.cs            # Cálculo de huellas
│   ├── XmlSignerService.cs       # Firma XML
│   └── VerifactuSerializer.cs    # Serialización a XML
└── Models/
    └── (modelos de datos)
```

### 2. Flujo de Comunicación Implementado

```
1. Cargar certificado (CertificateLoader)
   ↓
2. Construir registro de facturación (Models)
   ↓
3. Calcular huella SHA-256 (HashService)
   ↓
4. Serializar a XML (VerifactuSerializer)
   ↓
5. Firmar XML (XmlSignerService)
   ↓
6. Construir sobre SOAP (VerifactuSoapClient)
   ↓
7. Enviar con TLS mutuo (HttpClient + certificado)
   ↓
8. Procesar respuesta
```

### 3. Ejemplo de Uso Completo

```csharp
// Configuración
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .Build();

// 1. Cargar certificado
var certLoader = new CertificateLoader();
var certificado = certLoader.CargarDesdePfx(
    config["Certificado:PfxPath"],
    config["Certificado:PfxPassword"]
);

// 2. Crear registro de facturación
var factura = new Factura
{
    Serie = "A",
    Numero = "2025-0001",
    FechaExpedicion = DateTime.UtcNow,
    Emisor = new Emisor { Nif = "B12345678", Nombre = "MI EMPRESA SL" },
    Receptor = new Receptor { Nif = "12345678A", Nombre = "CLIENTE" },
    TotalFactura = 242.00m
};

var registro = new RegistroFacturacion
{
    Uuid = Guid.NewGuid().ToString(),
    Factura = factura
};

// 3. Calcular huella
var hashService = new HashService();
var huellaAnterior = config["Verifactu:HuellaAnterior"];
registro.Huella = hashService.CalcularHuella(registro, huellaAnterior);

// 4. Serializar a XML
var serializer = new VerifactuSerializer();
var xmlRegistro = serializer.SerializarRegistro(registro);

// 5. Firmar XML
var signer = new XmlSignerService();
var xmlFirmado = signer.FirmarXml(xmlRegistro, certificado);

// 6. Enviar por SOAP
var soapClient = new VerifactuSoapClient(
    config["Verifactu:EndpointUrl"],
    config["Verifactu:SoapAction"]
);

var respuesta = await soapClient.EnviarRegistroAsync(
    xmlFirmado,
    certificado,
    CancellationToken.None
);

Console.WriteLine($"Respuesta AEAT: {respuesta}");
```

---

## Ejemplos Prácticos

### 1. Prueba de Conectividad

```csharp
public class ConnectivityTest
{
    public static async Task TestConexion(string endpoint, X509Certificate2 cert)
    {
        try
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(cert);
            
            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(10);
            
            var response = await client.GetAsync(endpoint);
            
            Console.WriteLine($"✓ Conectividad OK");
            Console.WriteLine($"  Status: {response.StatusCode}");
            Console.WriteLine($"  TLS: {handler.SslProtocols}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error de conectividad: {ex.Message}");
        }
    }
}
```

### 2. Validación de Certificado

```csharp
public static void ValidarYMostrarInfoCertificado(X509Certificate2 cert)
{
    Console.WriteLine("=== INFORMACIÓN DEL CERTIFICADO ===");
    Console.WriteLine($"Subject: {cert.Subject}");
    Console.WriteLine($"Issuer: {cert.Issuer}");
    Console.WriteLine($"Serial Number: {cert.SerialNumber}");
    Console.WriteLine($"Thumbprint: {cert.Thumbprint}");
    Console.WriteLine($"Válido desde: {cert.NotBefore:dd/MM/yyyy}");
    Console.WriteLine($"Válido hasta: {cert.NotAfter:dd/MM/yyyy}");
    Console.WriteLine($"Tiene clave privada: {cert.HasPrivateKey}");
    Console.WriteLine($"Algoritmo: {cert.SignatureAlgorithm.FriendlyName}");
    
    // Verificar validez temporal
    var ahora = DateTime.Now;
    if (ahora < cert.NotBefore)
        Console.WriteLine("⚠ ADVERTENCIA: El certificado aún no es válido");
    else if (ahora > cert.NotAfter)
        Console.WriteLine("✗ ERROR: El certificado ha caducado");
    else
    {
        var diasRestantes = (cert.NotAfter - ahora).Days;
        Console.WriteLine($"✓ Certificado válido ({diasRestantes} días restantes)");
        
        if (diasRestantes < 30)
            Console.WriteLine("⚠ ADVERTENCIA: El certificado caduca pronto");
    }
    
    // Verificar cadena de confianza
    var chain = new X509Chain();
    var chainBuilt = chain.Build(cert);
    Console.WriteLine($"Cadena de confianza: {(chainBuilt ? "✓ Válida" : "✗ Inválida")}");
    
    if (!chainBuilt)
    {
        Console.WriteLine("Errores en la cadena:");
        foreach (var status in chain.ChainStatus)
        {
            Console.WriteLine($"  - {status.StatusInformation}");
        }
    }
}
```

### 3. Test End-to-End

```csharp
public class E2ETest
{
    public static async Task TestEnvioCompleto()
    {
        Console.WriteLine("=== TEST END-TO-END VERIFACTU ===\n");
        
        // 1. Cargar certificado
        Console.WriteLine("1. Cargando certificado...");
        var cert = new X509Certificate2("certificado.pfx", "password");
        ValidarYMostrarInfoCertificado(cert);
        
        // 2. Preparar datos
        Console.WriteLine("\n2. Preparando registro de facturación...");
        var registro = CrearRegistroPrueba();
        Console.WriteLine($"   UUID: {registro.Uuid}");
        Console.WriteLine($"   Factura: {registro.Factura.Serie}/{registro.Factura.Numero}");
        
        // 3. Calcular huella
        Console.WriteLine("\n3. Calculando huella...");
        var hashService = new HashService();
        registro.Huella = hashService.CalcularHuella(registro, "");
        Console.WriteLine($"   Huella: {registro.Huella}");
        
        // 4. Serializar
        Console.WriteLine("\n4. Serializando a XML...");
        var serializer = new VerifactuSerializer();
        var xml = serializer.SerializarRegistro(registro);
        Console.WriteLine($"   Longitud XML: {xml.Length} caracteres");
        
        // 5. Firmar
        Console.WriteLine("\n5. Firmando XML...");
        var signer = new XmlSignerService();
        var xmlFirmado = signer.FirmarXml(xml, cert);
        Console.WriteLine("   ✓ XML firmado correctamente");
        
        // 6. Enviar
        Console.WriteLine("\n6. Enviando a AEAT...");
        var client = new VerifactuSoapClient(
            "https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion",
            "RegFacturacionAlta"
        );
        
        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlFirmado);
            
            var respuesta = await client.EnviarRegistroAsync(
                xmlDoc, 
                cert, 
                CancellationToken.None
            );
            
            Console.WriteLine("   ✓ Respuesta recibida");
            Console.WriteLine($"\n7. Respuesta AEAT:\n{respuesta}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ Error: {ex.Message}");
            throw;
        }
    }
    
    private static RegistroFacturacion CrearRegistroPrueba()
    {
        return new RegistroFacturacion
        {
            Uuid = Guid.NewGuid().ToString(),
            Factura = new Factura
            {
                Serie = "TEST",
                Numero = $"SANDBOX-{DateTime.Now:yyyyMMddHHmmss}",
                FechaExpedicion = DateTime.UtcNow,
                Emisor = new Emisor 
                { 
                    Nif = "B12345678", 
                    Nombre = "EMPRESA PRUEBAS SL" 
                },
                Receptor = new Receptor 
                { 
                    Nif = "12345678A", 
                    Nombre = "CLIENTE PRUEBAS" 
                },
                TotalSinIva = 200.00m,
                TotalIva = 42.00m,
                TotalFactura = 242.00m
            }
        };
    }
}
```

---

## Resolución de Problemas

### 1. Problemas de TLS/SSL

#### Error: "The SSL connection could not be established"

**Diagnóstico:**
```bash
# Verificar conectividad TLS
openssl s_client -connect prewww1.aeat.es:443 -tls1_2

# Con certificado cliente
openssl s_client -connect prewww1.aeat.es:443 \
  -cert cert.pem -key key.pem -tls1_2
```

**Soluciones:**
1. Verificar que TLS 1.2+ está habilitado
2. Comprobar que el certificado cliente se incluye en la petición
3. Verificar fechas de validez del certificado

### 2. Problemas de Certificados

#### Error: "Certificate validation failed"

**Diagnóstico en .NET:**
```csharp
var handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback = 
    (message, cert, chain, errors) =>
    {
        Console.WriteLine($"Errores SSL: {errors}");
        if (errors != SslPolicyErrors.None)
        {
            Console.WriteLine("Detalles del certificado:");
            Console.WriteLine($"  Subject: {cert.Subject}");
            Console.WriteLine($"  Issuer: {cert.Issuer}");
            Console.WriteLine($"  Valid: {cert.NotBefore} - {cert.NotAfter}");
        }
        return errors == SslPolicyErrors.None;
    };
```

### 3. Problemas de SOAP

#### Error: "SOAP Fault - XML inválido"

**Verificación:**
```csharp
// Validar XML localmente antes de enviar
var xsd = XmlSchema.Read(
    File.OpenRead("SuministroLR.xsd"), 
    null
);

var settings = new XmlReaderSettings();
settings.Schemas.Add(xsd);
settings.ValidationType = ValidationType.Schema;

try
{
    using var reader = XmlReader.Create(
        new StringReader(xmlContent), 
        settings
    );
    while (reader.Read()) { }
    Console.WriteLine("✓ XML válido");
}
catch (XmlSchemaValidationException ex)
{
    Console.WriteLine($"✗ XML inválido: {ex.Message}");
}
```

### 4. Debugging de Peticiones HTTP

```csharp
// Handler de logging para todas las peticiones
public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        Console.WriteLine("=== REQUEST ===");
        Console.WriteLine($"{request.Method} {request.RequestUri}");
        Console.WriteLine("Headers:");
        foreach (var header in request.Headers)
        {
            Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        }
        
        if (request.Content != null)
        {
            var content = await request.Content.ReadAsStringAsync();
            Console.WriteLine($"Body:\n{content}");
        }
        
        var response = await base.SendAsync(request, cancellationToken);
        
        Console.WriteLine("\n=== RESPONSE ===");
        Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
        Console.WriteLine("Headers:");
        foreach (var header in response.Headers)
        {
            Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Body:\n{responseContent}");
        
        return response;
    }
}

// Uso:
var handler = new LoggingHandler { InnerHandler = new HttpClientHandler() };
var client = new HttpClient(handler);
```

---

## Referencias

### Documentación Oficial AEAT

- [Sede electrónica VERI*FACTU](https://sede.agenciatributaria.gob.es/Sede/iva/sistemas-informaticos-facturacion-verifactu.html)
- [Información técnica completa](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/informacion-tecnica.html)
- [Portal de Pruebas Externas](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/portal-pruebas-externas.html)
- [WSDL Servicios Web](https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl)
- [Esquemas XSD](https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/)

### Estándares y Especificaciones

- [RFC 8446 - TLS 1.3](https://datatracker.ietf.org/doc/html/rfc8446)
- [RFC 5246 - TLS 1.2](https://datatracker.ietf.org/doc/html/rfc5246)
- [SOAP 1.1 Specification](https://www.w3.org/TR/2000/NOTE-SOAP-20000508/)
- [XML Digital Signature (XMLDSig)](https://www.w3.org/TR/xmldsig-core/)
- [X.509 Certificate Standard](https://www.itu.int/rec/T-REC-X.509)

### Documentación del Proyecto

- [README Principal](../README.md)
- [Guía Técnica VERI*FACTU](Verifactu-Guia-Tecnica.md)
- [Entorno de Pruebas](entorno-pruebas.md)
- [Arquitectura](arquitectura.md)
- [Guía de Uso](uso.md)

### Herramientas Útiles

- [OpenSSL](https://www.openssl.org/) - Herramientas criptográficas y TLS
- [SoapUI](https://www.soapui.org/) - Testing de servicios SOAP
- [Fiddler](https://www.telerik.com/fiddler) - Proxy de debugging HTTP/HTTPS
- [Postman](https://www.postman.com/) - Testing de APIs

---

**Última actualización:** 30 de octubre de 2025  
**Versión del documento:** 1.0  
**Mantenedor:** Equipo VerifactuSender
