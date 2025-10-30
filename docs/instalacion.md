# Guía de Instalación y Configuración

Esta guía te ayudará a configurar VerifactuSender en tu entorno de desarrollo.

## Requisitos Previos

### Software Necesario

1. **[.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)** (versión 9.0 o superior)
   ```bash
   dotnet --version
   # Debe mostrar 9.0.x o superior
   ```

2. **Editor de Código** (recomendado)
   - [Visual Studio 2022](https://visualstudio.microsoft.com/) (Windows)
   - [Visual Studio Code](https://code.visualstudio.com/) (multiplataforma)
   - [JetBrains Rider](https://www.jetbrains.com/rider/) (multiplataforma)

3. **Git** para control de versiones
   ```bash
   git --version
   ```

### Certificado Digital

Necesitarás un **certificado de representante** en formato PFX:

- Certificado válido emitido por una autoridad certificadora reconocida
- Con clave privada incluida
- Contraseña del certificado
- Permisos de lectura sobre el archivo PFX

## Instalación

### 1. Clonar el Repositorio

```bash
git clone https://github.com/JoseRGWeb/Veri-factuSender.git
cd Veri-factuSender
```

### 2. Restaurar Dependencias

Desde la raíz del proyecto:

```bash
dotnet restore
```

Este comando descargará todas las dependencias NuGet necesarias.

### 3. Compilar el Proyecto

```bash
dotnet build
```

Deberías ver una salida similar a:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 4. Ejecutar Tests

Verifica que todo funciona correctamente:

```bash
dotnet test
```

Todos los tests deberían pasar exitosamente.

## Configuración

### Configuración de la Aplicación de Demo

#### 1. Configurar appsettings.json

Navega a la aplicación de demostración:

```bash
cd src/Verifactu.ConsoleDemo
```

Edita el archivo `appsettings.json`:

```json
{
  "Certificado": {
    "PfxPath": "C:/ruta/completa/a/tu-certificado.pfx",
    "PfxPassword": "TU_CONTRASEÑA_SEGURA"
  },
  "Verifactu": {
    "EndpointUrl": "https://prewww1.aeat.es/wlpl/TIKE-CONT/ValidarQR",
    "SoapAction": "urn:EnviarRegistroFacturacion",
    "HuellaAnterior": ""
  }
}
```

**⚠️ Importante:**
- Reemplaza `C:/ruta/completa/a/tu-certificado.pfx` con la ruta real a tu certificado
- Reemplaza `TU_CONTRASEÑA_SEGURA` con la contraseña de tu certificado
- El `EndpointUrl` es un placeholder; usa el endpoint oficial de AEAT cuando esté disponible

#### 2. Proteger Datos Sensibles (Recomendado)

En lugar de guardar la contraseña en `appsettings.json`, usa **User Secrets**:

```bash
# Desde src/Verifactu.ConsoleDemo/
dotnet user-secrets init
dotnet user-secrets set "Certificado:PfxPassword" "TU_CONTRASEÑA_SEGURA"
```

Luego actualiza `appsettings.json` para no incluir la contraseña:

```json
{
  "Certificado": {
    "PfxPath": "C:/ruta/completa/a/tu-certificado.pfx"
  },
  "Verifactu": {
    "EndpointUrl": "https://...",
    "SoapAction": "urn:EnviarRegistroFacturacion",
    "HuellaAnterior": ""
  }
}
```

#### 3. Configurar Datos de Prueba

Edita `factura-demo.json` con datos de prueba realistas:

```json
{
  "Serie": "A",
  "Numero": "2025-0001",
  "FechaExpedicion": "2025-10-30T10:00:00Z",
  "Emisor": {
    "Nif": "B12345678",
    "Nombre": "Empresa Demo S.L."
  },
  "Receptor": {
    "Nif": "12345678A",
    "Nombre": "Cliente Demo"
  },
  "Lineas": [
    {
      "Descripcion": "Producto de prueba",
      "Cantidad": 1,
      "PrecioUnitario": 100.00,
      "TipoIva": 21
    }
  ],
  "TotalSinIva": 100.00,
  "TotalIva": 21.00,
  "TotalFactura": 121.00
}
```

### Configuración para Biblioteca (Integración en tu Proyecto)

Si deseas integrar `Verifactu.Client` en tu propia aplicación:

#### 1. Agregar Referencia al Proyecto

En tu archivo `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="ruta/a/Verifactu.Client/Verifactu.Client.csproj" />
</ItemGroup>
```

O copiar el DLL compilado y referenciar directamente.

#### 2. Configurar Servicios (Dependency Injection)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Verifactu.Client.Services;

var services = new ServiceCollection();

// Registrar servicios
services.AddSingleton<IHashService, HashService>();
services.AddSingleton<IVerifactuSerializer, VerifactuSerializer>();
services.AddSingleton<IXmlSignerService, XmlSignerService>();
services.AddSingleton<ICertificateLoader, CertificateLoader>();
services.AddSingleton<VerifactuSoapClient>();

var serviceProvider = services.BuildServiceProvider();
```

#### 3. Usar los Servicios

```csharp
// Cargar certificado
var certLoader = serviceProvider.GetRequiredService<ICertificateLoader>();
var certificado = certLoader.CargarDesdeArchivo("ruta.pfx", "password");

// Crear registro
var registro = new RegistroFacturacion
{
    Uuid = Guid.NewGuid().ToString(),
    Factura = new Factura { /* ... */ }
};

// Calcular huella
var hashService = serviceProvider.GetRequiredService<IHashService>();
registro.Huella = hashService.CalcularHuella(registro, huellaAnterior);

// Serializar
var serializer = serviceProvider.GetRequiredService<IVerifactuSerializer>();
var xml = serializer.SerializarRegistro(registro);

// Firmar
var signer = serviceProvider.GetRequiredService<IXmlSignerService>();
var xmlFirmado = signer.FirmarXml(xml, certificado);

// Enviar
var soapClient = serviceProvider.GetRequiredService<VerifactuSoapClient>();
var respuesta = await soapClient.EnviarRegistroAsync(
    xmlFirmado, 
    endpointUrl, 
    certificado
);
```

## Configuración de Entornos

### Desarrollo

Use `appsettings.Development.json` para configuración específica de desarrollo:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "Verifactu": {
    "EndpointUrl": "https://endpoint-pruebas.aeat.es/..."
  }
}
```

### Producción

**Variables de entorno** (recomendado para producción):

```bash
# Linux/macOS
export Certificado__PfxPath="/ruta/segura/certificado.pfx"
export Certificado__PfxPassword="PASSWORD_SEGURO"
export Verifactu__EndpointUrl="https://endpoint-produccion.aeat.es/..."

# Windows PowerShell
$env:Certificado__PfxPath="C:\ruta\segura\certificado.pfx"
$env:Certificado__PfxPassword="PASSWORD_SEGURO"
$env:Verifactu__EndpointUrl="https://endpoint-produccion.aeat.es/..."
```

O usar un gestor de secretos como:
- **Azure Key Vault**
- **AWS Secrets Manager**
- **HashiCorp Vault**

## Verificación de la Instalación

### Ejecutar la Aplicación Demo

```bash
cd src/Verifactu.ConsoleDemo
dotnet run
```

Deberías ver una salida similar a:

```
VerifactuSender - Demo de envío de facturas a VERI*FACTU
===========================================================

[1] Cargando configuración...
[2] Cargando certificado desde: C:/ruta/certificado.pfx
[3] Certificado cargado: CN=Empresa Demo
[4] Leyendo factura demo...
[5] Construyendo registro de facturación...
[6] Calculando huella encadenada...
[7] Serializando registro a XML...
[8] Firmando XML...
[9] Enviando por SOAP a: https://...
[10] Respuesta recibida:
     Status: 200
     [Contenido de la respuesta]

Proceso completado.
```

## Solución de Problemas Comunes

### Error: "No se puede cargar el certificado"

**Causa**: Ruta incorrecta o contraseña incorrecta.

**Solución**:
- Verifica que la ruta al PFX sea absoluta y correcta
- Verifica la contraseña del certificado
- Comprueba que tienes permisos de lectura sobre el archivo

### Error: "SYSLIB0057 obsolete warning"

**Causa**: Uso de constructor obsoleto de `X509Certificate2`.

**Solución**: Este es un warning conocido que se resolverá en futuras versiones. No afecta la funcionalidad.

### Error de compilación

**Causa**: SDK de .NET 9 no instalado.

**Solución**: 
```bash
dotnet --version  # Verifica la versión
# Si no es 9.x, descarga e instala .NET 9 SDK
```

### Tests fallan

**Causa**: Dependencias no restauradas o problema de configuración.

**Solución**:
```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
```

## Próximos Pasos

Ahora que has instalado y configurado VerifactuSender:

1. Lee la [Guía de Uso](uso.md) para aprender a usar la biblioteca
2. Consulta la [Arquitectura](arquitectura.md) para entender la estructura
3. Revisa el [Roadmap](roadmap.md) para conocer las limitaciones actuales

## Seguridad

⚠️ **Recordatorios Importantes de Seguridad:**

1. **Nunca** versiones el archivo `appsettings.json` con contraseñas reales
2. **Nunca** versiones archivos PFX
3. Añade estos archivos a `.gitignore`:
   ```
   appsettings.json
   appsettings.*.json
   *.pfx
   *.p12
   ```
4. Usa `user-secrets` en desarrollo y gestores de secretos en producción
5. Restringe permisos del archivo PFX (solo lectura para el usuario de la aplicación)

---

**Última actualización:** 30 de octubre de 2025
