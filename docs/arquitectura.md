# Arquitectura del Proyecto VerifactuSender

## Visión General

VerifactuSender es una solución modular en .NET 9 diseñada para facilitar la integración con el sistema VERI\*FACTU de la AEAT. La arquitectura sigue principios de separación de responsabilidades y está organizada en capas claramente definidas.

## Estructura del Proyecto

```
Veri-factuSender/
├── src/
│   ├── Verifactu.Client/          # Biblioteca principal
│   └── Verifactu.ConsoleDemo/      # Aplicación de demostración
├── tests/
│   └── Verifactu.Client.Tests/     # Pruebas unitarias
└── docs/                            # Documentación
```

## Componentes Principales

### 1. Verifactu.Client (Biblioteca Core)

Biblioteca reutilizable que encapsula toda la lógica de integración con VERI\*FACTU.

#### Modelos de Datos (`Models/`)

- **`Factura.cs`**: Representa una factura con todos sus datos fiscales
  - Serie y número
  - Fechas de expedición y operación
  - Datos del emisor y receptor
  - Líneas de detalle
  - Totales e IVA

- **`RegistroFacturacion.cs`**: Registro completo para envío a VERI\*FACTU
  - UUID único del registro
  - Factura asociada
  - Huella (hash) del registro
  - Huella del registro anterior (encadenamiento)
  - Metadatos de envío

- **`Emisor.cs` / `Receptor.cs`**: Datos de las partes de la factura
  - NIF/CIF
  - Nombre o razón social
  - Dirección (opcional)

#### Servicios (`Services/`)

##### IHashService / HashService
**Responsabilidad**: Cálculo de la huella criptográfica (hash) de los registros.

- Implementa el algoritmo SHA-256
- Gestiona el encadenamiento de huellas entre registros consecutivos
- **Nota**: Implementación placeholder que debe adaptarse al algoritmo oficial de AEAT

```csharp
public interface IHashService
{
    string CalcularHuella(RegistroFacturacion registro, string huellaAnterior);
}
```

##### IVerifactuSerializer / VerifactuSerializer
**Responsabilidad**: Serialización de objetos a XML conforme a los esquemas XSD de AEAT.

- Convierte objetos `RegistroFacturacion` a XML
- **Nota**: Implementación placeholder que debe reemplazarse con serialización conforme a XSD oficiales

```csharp
public interface IVerifactuSerializer
{
    string SerializarRegistro(RegistroFacturacion registro);
}
```

##### IXmlSignerService / XmlSignerService
**Responsabilidad**: Firma digital de documentos XML usando XMLDSig.

- Implementa firma enveloped (XMLDSig)
- Utiliza certificados X.509 para la firma
- Aplica transformaciones y canonicalización
- **Nota**: Debe ajustarse a las especificaciones de firma de AEAT

```csharp
public interface IXmlSignerService
{
    string FirmarXml(string xmlSinFirmar, X509Certificate2 certificado);
}
```

##### ICertificateLoader / CertificateLoader
**Responsabilidad**: Carga de certificados digitales desde diferentes fuentes.

- Carga desde archivos PFX
- Soporte para certificados con contraseña
- **Nota**: Actualmente usa constructor obsoleto; debe migrar a `X509CertificateLoader`

```csharp
public interface ICertificateLoader
{
    X509Certificate2 CargarDesdeArchivo(string rutaPfx, string password);
}
```

#### Cliente SOAP (`Soap/`)

##### VerifactuSoapClient
**Responsabilidad**: Comunicación con los servicios web SOAP de AEAT.

- Construcción de sobres SOAP
- Envío de peticiones HTTP con TLS mutuo (certificado cliente)
- Gestión de headers SOAP y SOAPAction
- **Nota**: Implementación placeholder; debe adaptarse al WSDL oficial

```csharp
public class VerifactuSoapClient
{
    public async Task<string> EnviarRegistroAsync(
        string xmlFirmado, 
        string endpointUrl, 
        X509Certificate2 certificadoCliente,
        string soapAction = null
    )
}
```

### 2. Verifactu.ConsoleDemo (Aplicación de Demostración)

Aplicación de consola que demuestra el flujo completo de integración.

#### Funcionalidad

1. **Carga de configuración** desde `appsettings.json`
2. **Carga de certificado** usando `CertificateLoader`
3. **Lectura de factura** desde `factura-demo.json`
4. **Construcción de registro** de facturación
5. **Cálculo de huella** y encadenamiento
6. **Serialización a XML**
7. **Firma del XML**
8. **Envío por SOAP** al endpoint configurado

#### Archivos de Configuración

**appsettings.json**
```json
{
  "Certificado": {
    "PfxPath": "ruta/al/certificado.pfx",
    "PfxPassword": "contraseña"
  },
  "Verifactu": {
    "EndpointUrl": "https://...",
    "SoapAction": "...",
    "HuellaAnterior": ""
  }
}
```

**factura-demo.json**
- Ejemplo de factura en formato JSON
- Facilita pruebas sin necesidad de base de datos

### 3. Verifactu.Client.Tests (Pruebas)

Pruebas unitarias usando xUnit.

#### Tests Actuales

- **HashServiceTests**: Verificación del cálculo de huellas
  - Tests básicos de funcionalidad
  - Verificación de formato SHA-256

#### Tests Necesarios (pendientes)

- Serialización XML vs XSD
- Firma XML y validación
- Encadenamiento de huellas
- Integración SOAP (mocks)

## Flujo de Datos

```
┌─────────────────┐
│  Factura JSON   │
└────────┬────────┘
         │
         v
┌─────────────────────────────┐
│  Construcción de Registro   │
│  (RegistroFacturacion)      │
└────────┬────────────────────┘
         │
         v
┌─────────────────────────────┐
│  Cálculo de Huella          │
│  (HashService)              │
└────────┬────────────────────┘
         │
         v
┌─────────────────────────────┐
│  Serialización a XML        │
│  (VerifactuSerializer)      │
└────────┬────────────────────┘
         │
         v
┌─────────────────────────────┐
│  Firma XML                  │
│  (XmlSignerService)         │
└────────┬────────────────────┘
         │
         v
┌─────────────────────────────┐
│  Envío SOAP                 │
│  (VerifactuSoapClient)      │
└────────┬────────────────────┘
         │
         v
┌─────────────────────────────┐
│  Respuesta AEAT             │
└─────────────────────────────┘
```

## Principios de Diseño

### 1. Separación de Responsabilidades
Cada componente tiene una responsabilidad única y bien definida.

### 2. Inversión de Dependencias
Uso de interfaces para permitir testing y diferentes implementaciones.

### 3. Configuración Externalizada
Datos sensibles y parámetros de configuración fuera del código.

### 4. Extensibilidad
Diseño modular que permite reemplazar componentes individuales.

## Dependencias Principales

### .NET 9
- `System.Security.Cryptography.X509Certificates` - Gestión de certificados
- `System.Security.Cryptography.Xml` - Firma XML
- `System.Net.Http` - Cliente HTTP para SOAP
- `Microsoft.Extensions.Configuration` - Configuración
- `System.Text.Json` - Serialización JSON

### Testing
- `xUnit` - Framework de pruebas
- `Microsoft.NET.Test.Sdk` - SDK de testing

## Consideraciones de Seguridad

### Certificados
- Los certificados PFX deben protegerse con permisos restrictivos
- Considerar almacenamiento en el almacén de certificados del sistema
- No versionar certificados ni contraseñas

### Datos Sensibles
- Usar `dotnet user-secrets` en desarrollo
- Variables de entorno o Azure Key Vault en producción
- No incluir `appsettings.json` con datos reales en el repositorio

### Comunicación
- TLS mutuo (certificado cliente) obligatorio
- Validación de certificados del servidor
- Timeouts y políticas de reintento apropiadas

## Puntos de Extensión

### Almacenamiento
El proyecto actual no incluye persistencia. Puntos de extensión recomendados:

- **Repository pattern** para almacenar registros de facturación
- **Queue** para gestión asíncrona de envíos
- **Cache** para huellas y estados

### Logging
- Integración con `Microsoft.Extensions.Logging`
- Logging estructurado para auditoría
- Métricas de envío y errores

### Validación
- Validación de facturas antes de procesamiento
- Validación XML contra XSD
- Reglas de negocio personalizadas

## Próximos Pasos

Consulta el [Roadmap](roadmap.md) para detalles sobre:
- Implementación conforme a XSD/WSDL oficiales
- Algoritmo de huella oficial
- Mejoras de seguridad
- Tests de integración

---

**Última actualización:** 30 de octubre de 2025
