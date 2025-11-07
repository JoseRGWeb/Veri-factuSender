# Guía de Troubleshooting - Cliente SOAP VERI*FACTU

## 📋 Índice

1. [Errores de Conexión y TLS](#errores-de-conexión-y-tls)
2. [Errores de Certificado](#errores-de-certificado)
3. [Errores SOAP](#errores-soap)
4. [Errores de Validación](#errores-de-validación)
5. [Timeouts](#timeouts)
6. [Códigos de Error AEAT](#códigos-de-error-aeat)
7. [Problemas de Rendimiento](#problemas-de-rendimiento)
8. [Diagnóstico Avanzado](#diagnóstico-avanzado)

---

## Errores de Conexión y TLS

### Error: "Could not establish SSL connection"

**Síntomas:**
```
HttpRequestException: The SSL connection could not be established
```

**Causas posibles:**
1. Certificado cliente inválido o caducado
2. Protocolo TLS incompatible
3. Firewall bloqueando conexión
4. Endpoint incorrecto

**Soluciones:**

#### 1. Verificar certificado
```csharp
// Verificar que el certificado está cargado correctamente
var cert = CertificateLoader.CargarDesdePfx("certificado.pfx", "password");
Console.WriteLine($"Certificado válido hasta: {cert.NotAfter}");
Console.WriteLine($"Emisor: {cert.Issuer}");
Console.WriteLine($"Asunto: {cert.Subject}");

// Verificar que tiene clave privada
if (!cert.HasPrivateKey)
{
    throw new InvalidOperationException("El certificado no tiene clave privada");
}
```

#### 2. Verificar endpoint
```csharp
// Sandbox
const string SANDBOX_URL = "https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion";

// Producción
const string PROD_URL = "https://www1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion";

// NO usar HTTP (debe ser HTTPS)
```

#### 3. Habilitar logs de TLS (solo en desarrollo)
```csharp
// Agregar al inicio del programa para debugging
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.AllowAutoRedirect", false);

// Habilitar logging detallado de SSL
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_NET_SECURITY_NATIVE_LOG", "1");
```

### Error: "The remote certificate is invalid"

**Síntomas:**
```
AuthenticationException: The remote certificate is invalid according to the validation procedure
```

**Causa:** El certificado del servidor AEAT no es confiado por el sistema operativo.

**Solución:**

En desarrollo/sandbox, se puede omitir validación (NO USAR EN PRODUCCIÓN):
```csharp
var handler = new HttpClientHandler();
handler.ClientCertificates.Add(cert);

// SOLO PARA DESARROLLO - NO USAR EN PRODUCCIÓN
handler.ServerCertificateCustomValidationCallback = 
    (message, cert, chain, errors) => true;

using var http = new HttpClient(handler);
```

En producción, asegurar que los certificados raíz de AEAT estén instalados:
```bash
# Linux/Ubuntu - Actualizar certificados del sistema
sudo update-ca-certificates

# Windows - Los certificados de AEAT deberían estar instalados por defecto
```

---

## Errores de Certificado

### Error: "NIF del certificado no coincide"

**Síntomas:**
- Error SOAP Fault con código relacionado a NIF
- Registro rechazado con error de autenticación

**Causa:** El NIF en el certificado digital no coincide con el NIF del emisor de facturas.

**Solución:**
```csharp
// Verificar NIF en el certificado
var cert = CertificateLoader.CargarDesdePfx("certificado.pfx", "password");
var nifCertificado = ExtraerNifDeCertificado(cert);
Console.WriteLine($"NIF en certificado: {nifCertificado}");

// Verificar que coincide con NIF del emisor
var nifEmisor = "B12345678";
if (nifCertificado != nifEmisor)
{
    throw new InvalidOperationException(
        $"NIF del certificado ({nifCertificado}) no coincide con NIF emisor ({nifEmisor})");
}

// Método auxiliar para extraer NIF
string ExtraerNifDeCertificado(X509Certificate2 cert)
{
    // El NIF suele estar en el Subject del certificado
    // Formato: "CN=NOMBRE, SERIALNUMBER=IDCES-12345678A, ..."
    var subject = cert.Subject;
    var match = System.Text.RegularExpressions.Regex.Match(
        subject, @"SERIALNUMBER=IDCES-([A-Z0-9]+)");
    
    if (match.Success)
        return match.Groups[1].Value;
    
    throw new InvalidOperationException("No se pudo extraer NIF del certificado");
}
```

### Error: "Certificado caducado"

**Síntomas:**
```
CryptographicException: X509Certificate2 is expired
```

**Solución:**
```csharp
// Verificar validez del certificado
var cert = CertificateLoader.CargarDesdePfx("certificado.pfx", "password");
var ahora = DateTime.UtcNow;

if (ahora < cert.NotBefore)
{
    throw new InvalidOperationException(
        $"El certificado aún no es válido. Válido desde: {cert.NotBefore}");
}

if (ahora > cert.NotAfter)
{
    throw new InvalidOperationException(
        $"El certificado ha caducado. Válido hasta: {cert.NotAfter}");
}

// Advertir si el certificado caduca pronto (60 días)
var diasRestantes = (cert.NotAfter - ahora).Days;
if (diasRestantes < 60)
{
    Console.WriteLine($"⚠️ ADVERTENCIA: El certificado caduca en {diasRestantes} días");
}
```

### Error: "Certificado revocado"

**Síntomas:**
- Error SSL durante handshake
- Error SOAP relacionado con certificado

**Solución:**

Verificar estado de revocación del certificado:
```csharp
using System.Security.Cryptography.X509Certificates;

bool VerificarCertificadoNoRevocado(X509Certificate2 cert)
{
    var chain = new X509Chain();
    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
    
    bool isValid = chain.Build(cert);
    
    if (!isValid)
    {
        foreach (var element in chain.ChainElements)
        {
            foreach (var status in element.ChainElementStatus)
            {
                Console.WriteLine($"Error: {status.Status} - {status.StatusInformation}");
                
                if (status.Status == X509ChainStatusFlags.Revoked)
                {
                    throw new InvalidOperationException(
                        "El certificado ha sido revocado. Debe obtener un nuevo certificado.");
                }
            }
        }
    }
    
    return isValid;
}
```

---

## Errores SOAP

### Error: "SOAP Fault received"

**Síntomas:**
```xml
<soap:Fault>
  <faultcode>soap:Client</faultcode>
  <faultstring>Error en validación</faultstring>
  <detail>...</detail>
</soap:Fault>
```

**Causa:** El XML enviado no cumple con el esquema XSD o tiene errores de validación.

**Solución:**

1. **Validar XML antes de enviar:**
```csharp
// Habilitar validación en appsettings.json
{
  "Verifactu": {
    "ValidarXmlContraXsd": true,
    "XsdBasePath": "./xsd"
  }
}
```

2. **Capturar y analizar SOAP Fault:**
```csharp
try
{
    var respuestaXml = await soapClient.EnviarRegistroAsync(xmlFirmado, cert);
}
catch (HttpRequestException ex)
{
    // Verificar si es SOAP Fault
    var response = ex.Message;
    if (response.Contains("<soap:Fault>"))
    {
        var doc = XDocument.Parse(response);
        XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
        
        var fault = doc.Descendants(soap + "Fault").FirstOrDefault();
        if (fault != null)
        {
            var faultCode = fault.Element(soap + "faultcode")?.Value;
            var faultString = fault.Element(soap + "faultstring")?.Value;
            var detail = fault.Element("detail")?.ToString();
            
            Console.WriteLine($"SOAP Fault:");
            Console.WriteLine($"  Código: {faultCode}");
            Console.WriteLine($"  Mensaje: {faultString}");
            Console.WriteLine($"  Detalle: {detail}");
        }
    }
    throw;
}
```

### Error: "Invalid namespace"

**Síntomas:**
- SOAP Fault indicando namespace incorrecto
- Error de validación XSD

**Causa:** Namespaces XML no coinciden con los del WSDL oficial.

**Solución:**

Verificar que los namespaces sean exactamente:
```csharp
// Namespaces correctos (NO modificar)
const string NS_SOAP = "http://schemas.xmlsoap.org/soap/envelope/";
const string NS_SUMINISTRO_LR = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroLR.xsd";
const string NS_SUMINISTRO_INFO = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd";

// ❌ INCORRECTO - namespace antiguo
// const string NS_OLD = "http://www.aeat.es/...";
```

---

## Errores de Validación

### Error de Código AEAT: 4001

**Mensaje:** "NIF del emisor no identificado"

**Causa:** El NIF del emisor no está registrado en AEAT o es inválido.

**Solución:**
1. Verificar que el NIF es correcto y tiene formato válido
2. Verificar que el obligado está dado de alta en VERI*FACTU
3. En sandbox, usar NIF de pruebas proporcionados por AEAT

### Error de Código AEAT: 3001

**Mensaje:** "Registro duplicado"

**Causa:** Se está intentando enviar un registro que ya fue aceptado previamente.

**Solución:**
```csharp
// Verificar si el error indica duplicado
var respuesta = await client.EnviarRegFacturacionAltaAsync(xmlFirmado, cert);

foreach (var linea in respuesta.RespuestasLinea)
{
    if (linea.RegistroDuplicado != null)
    {
        Console.WriteLine("⚠️ Registro duplicado");
        Console.WriteLine($"  ID petición previa: {linea.RegistroDuplicado.IdPeticionRegistroDuplicado}");
        Console.WriteLine($"  Estado previo: {linea.RegistroDuplicado.EstadoRegistroDuplicado}");
        
        // No reintentar - el registro ya fue aceptado
        // Usar el CSV original si se necesita
    }
}
```

### Error de Código AEAT: 5002

**Mensaje:** "Fecha inválida"

**Causa:** La fecha de la factura está fuera del rango permitido.

**Solución:**
```csharp
// Validar fecha antes de enviar
var fechaFactura = factura.FechaEmision;
var ahora = DateTime.UtcNow;

// No permitir facturas futuras
if (fechaFactura > ahora)
{
    throw new ValidationException("La fecha de factura no puede ser futura");
}

// No permitir facturas muy antiguas (ej: más de 4 años)
var fechaMinima = ahora.AddYears(-4);
if (fechaFactura < fechaMinima)
{
    throw new ValidationException($"La fecha de factura es demasiado antigua (mínimo: {fechaMinima:yyyy-MM-dd})");
}

// Usar formato ISO 8601
var fechaFormateada = fechaFactura.ToString("yyyy-MM-dd");
```

---

## Timeouts

### Error: "Request timeout after 120 seconds"

**Síntomas:**
```
TaskCanceledException: The operation was canceled
TimeoutException: Timeout al enviar registro a AEAT
```

**Causas posibles:**
1. Red lenta o inestable
2. Servidor AEAT sobrecargado
3. Payload XML muy grande
4. Timeout configurado demasiado bajo

**Soluciones:**

#### 1. Aumentar timeout
```csharp
// En VerifactuSoapClient constructor o HttpClient
var handler = new HttpClientHandler();
handler.ClientCertificates.Add(cert);

using var http = new HttpClient(handler);
http.Timeout = TimeSpan.FromSeconds(300); // 5 minutos en lugar de 120s
```

#### 2. Implementar lógica de reintento
```csharp
public async Task<string> EnviarConReintentos(
    XmlDocument xmlFirmado, 
    X509Certificate2 cert,
    int maxReintentos = 3)
{
    int intento = 0;
    while (true)
    {
        try
        {
            return await soapClient.EnviarRegistroAsync(xmlFirmado, cert);
        }
        catch (TimeoutException) when (intento < maxReintentos)
        {
            intento++;
            var delay = TimeSpan.FromSeconds(Math.Pow(2, intento)); // Backoff exponencial
            Console.WriteLine($"Timeout. Reintentando en {delay.TotalSeconds}s (intento {intento}/{maxReintentos})...");
            await Task.Delay(delay);
        }
    }
}
```

#### 3. Reducir tamaño de envíos
```csharp
// En lugar de enviar 1000 registros, dividir en lotes más pequeños
const int TAMAÑO_LOTE = 100;

for (int i = 0; i < registros.Count; i += TAMAÑO_LOTE)
{
    var lote = registros.Skip(i).Take(TAMAÑO_LOTE).ToList();
    await EnviarLote(lote);
    
    // Respetar TiempoEsperaEnvio entre lotes
    await Task.Delay(TimeSpan.FromSeconds(tiempoEspera));
}
```

---

## Códigos de Error AEAT

### Tabla de Códigos Comunes

| Código | Descripción | Acción Recomendada |
|--------|-------------|-------------------|
| 3001 | Registro duplicado | No reintentar, usar CSV original |
| 4001 | NIF emisor no válido | Verificar NIF y registro en AEAT |
| 4002 | NIF receptor no válido | Verificar formato NIF receptor |
| 5001 | Fecha expedición inválida | Verificar formato fecha (YYYY-MM-DD) |
| 5002 | Fecha fuera de rango | Ajustar fecha al rango permitido |
| 6001 | Importe inválido | Verificar formato decimal (punto como separador) |
| 6002 | Tipo de factura no válido | Usar valores permitidos (F1, F2, R1-R5) |
| 7001 | Huella inválida | Recalcular huella según algoritmo oficial |
| 7002 | Encadenamiento incorrecto | Verificar huella anterior correcta |
| 8001 | Firma digital inválida | Verificar certificado y algoritmo de firma |
| 9001 | Error interno AEAT | Reintentar después de unos minutos |

### Manejo de Errores por Tipo

```csharp
void ManejarErrorAeat(RespuestaLinea linea)
{
    if (linea.EstadoRegistro == "Incorrecto" && linea.CodigoErrorRegistro != null)
    {
        var codigo = linea.CodigoErrorRegistro;
        var descripcion = linea.DescripcionErrorRegistro;
        
        switch (codigo)
        {
            case "3001": // Duplicado
                Console.WriteLine("Registro duplicado - no es necesario reintentar");
                break;
                
            case "4001": // NIF inválido
            case "4002":
                throw new ValidationException($"Error de NIF: {descripcion}");
                
            case "5001": // Fecha inválida
            case "5002":
                throw new ValidationException($"Error de fecha: {descripcion}");
                
            case "7001": // Huella inválida
            case "7002":
                throw new ValidationException($"Error de huella/encadenamiento: {descripcion}");
                
            case "8001": // Firma inválida
                throw new ValidationException($"Error de firma digital: {descripcion}");
                
            case "9001": // Error interno AEAT
                // Reintentar con backoff
                Console.WriteLine("Error temporal de AEAT - reintentar");
                break;
                
            default:
                throw new Exception($"Error AEAT {codigo}: {descripcion}");
        }
    }
}
```

---

## Problemas de Rendimiento

### Problema: Envíos muy lentos

**Síntomas:**
- Cada envío tarda más de 5 segundos
- Alta latencia en las respuestas

**Soluciones:**

#### 1. Reusar conexiones HTTP
```csharp
// NO crear nuevo HttpClient en cada petición
// ❌ INCORRECTO
foreach (var registro in registros)
{
    using var client = new HttpClient(); // Crea nueva conexión TCP cada vez
    await client.PostAsync(...);
}

// ✅ CORRECTO - Reusar cliente
private static readonly HttpClient SharedClient = new HttpClient();

foreach (var registro in registros)
{
    await SharedClient.PostAsync(...); // Reutiliza conexión TCP
}
```

#### 2. Habilitar HTTP/2 (si AEAT lo soporta)
```csharp
var handler = new HttpClientHandler();
handler.ClientCertificates.Add(cert);

var client = new HttpClient(handler);
client.DefaultRequestVersion = new Version(2, 0);
```

#### 3. Envíos en paralelo (con cuidado)
```csharp
// Respetar límites de rate limiting de AEAT
var semaphore = new SemaphoreSlim(5); // Máximo 5 peticiones concurrentes

var tareas = registros.Select(async registro =>
{
    await semaphore.WaitAsync();
    try
    {
        return await EnviarRegistro(registro);
    }
    finally
    {
        semaphore.Release();
    }
});

var resultados = await Task.WhenAll(tareas);
```

---

## Diagnóstico Avanzado

### Capturar tráfico HTTP/SOAP

Para debugging avanzado, capturar el tráfico SOAP completo:

```csharp
public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Log request
        Console.WriteLine("=== REQUEST ===");
        Console.WriteLine($"{request.Method} {request.RequestUri}");
        Console.WriteLine($"Headers: {request.Headers}");
        
        if (request.Content != null)
        {
            var content = await request.Content.ReadAsStringAsync();
            Console.WriteLine($"Body:\n{content}");
        }
        
        // Enviar petición
        var response = await base.SendAsync(request, cancellationToken);
        
        // Log response
        Console.WriteLine("\n=== RESPONSE ===");
        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Headers: {response.Headers}");
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Body:\n{responseContent}");
        
        return response;
    }
}

// Usar el handler
var handler = new HttpClientHandler();
handler.ClientCertificates.Add(cert);

var loggingHandler = new LoggingHandler { InnerHandler = handler };
var client = new HttpClient(loggingHandler);
```

### Validar XML contra XSD offline

```csharp
using System.Xml;
using System.Xml.Schema;

void ValidarXmlContraXsd(XmlDocument xmlDoc, string xsdPath)
{
    var schemas = new XmlSchemaSet();
    schemas.Add(null, xsdPath);
    
    xmlDoc.Schemas = schemas;
    
    var errores = new List<string>();
    
    xmlDoc.Validate((sender, e) =>
    {
        errores.Add($"{e.Severity}: {e.Message}");
    });
    
    if (errores.Any())
    {
        throw new ValidationException(
            $"XML no válido:\n{string.Join("\n", errores)}");
    }
}
```

### Verificar conectividad con AEAT

```bash
# Verificar conectividad básica
ping prewww1.aeat.es

# Verificar puerto HTTPS abierto
telnet prewww1.aeat.es 443

# Verificar certificado del servidor
openssl s_client -connect prewww1.aeat.es:443 -showcerts

# Verificar con curl
curl -v https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion
```

---

## Checklist de Diagnóstico

Cuando tengas un problema, verificar en orden:

- [ ] **Certificado válido:** No caducado, no revocado, con clave privada
- [ ] **NIF coincidente:** NIF del certificado = NIF del emisor
- [ ] **Endpoint correcto:** Sandbox o producción según entorno
- [ ] **Conectividad:** Acceso a Internet, firewall permite HTTPS
- [ ] **XML válido:** Conforme a XSD, namespaces correctos
- [ ] **Firma válida:** XMLDSig correcto con algoritmo apropiado
- [ ] **Huella correcta:** SHA-256 calculado según especificación
- [ ] **TiempoEsperaEnvio:** Respetar delay entre envíos
- [ ] **Logs habilitados:** Revisar logs detallados para más info

---

## Recursos Adicionales

- **Documentación oficial:** `docs/Verifactu-Guia-Tecnica.md`
- **Guía de uso:** `docs/uso-cliente-soap.md`
- **Análisis WSDL:** `docs/wsdl/WSDL-ANALYSIS.md`
- **Tests de ejemplo:** `tests/Verifactu.Integration.Tests/SandboxTests.cs`
- **Portal AEAT:** https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/

---

**Última actualización:** 2025-11-07
