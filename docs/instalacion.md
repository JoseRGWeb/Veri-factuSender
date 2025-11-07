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

Necesitarás un **certificado digital** para autenticación con VERI*FACTU:

**Requisitos:**
- Certificado válido emitido por una autoridad certificadora reconocida
- Con clave privada incluida
- RSA mínimo 2048 bits o ECDSA mínimo 256 bits
- Algoritmo SHA-256 o superior

**Opciones de Configuración:**
1. **Archivo PFX**: Para desarrollo local (requiere contraseña)
2. **Almacén de Certificados de Windows**: Para producción (recomendado)
3. **Variables de Entorno**: Para CI/CD y contenedores

> 📚 **Guía completa**: Ver [Certificados y Seguridad](./certificados-y-seguridad.md) para instrucciones detalladas sobre gestión de certificados, validación, y mejores prácticas de seguridad.

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

#### 1. Configurar Certificado

VerifactuSender soporta múltiples métodos de carga de certificados. Elige el más apropiado para tu entorno:

##### Opción A: Archivo PFX (Desarrollo)

Edita `appsettings.json`:

```json
{
  "Certificado": {
    "Tipo": "Archivo",
    "PfxPath": "/ruta/completa/a/certificado.pfx",
    "PfxPassword": ""  // No guardar aquí, usar user-secrets
  }
}
```

**Configurar contraseña con User Secrets:**
```bash
cd src/Verifactu.ConsoleDemo
dotnet user-secrets init
dotnet user-secrets set "Certificado:PfxPassword" "tu-password-segura"
dotnet user-secrets set "Certificado:PfxPath" "/ruta/a/certificado.pfx"
```

##### Opción B: Almacén de Certificados (Producción - Recomendado)

**Paso 1**: Instalar el certificado en Windows:
```powershell
# Usar el script de ayuda
.\scripts\setup-certificates.ps1 -PfxPath "C:\certs\certificado.pfx"

# O instalar manualmente usando certmgr.msc
```

**Paso 2**: Configurar en `appsettings.json`:
```json
{
  "Certificado": {
    "Tipo": "Almacen",
    "Thumbprint": "3B7E039FDBDA89ABC...",  // Obtenido del script o certmgr.msc
    "StoreLocation": "CurrentUser",
    "StoreName": "My"
  }
}
```

##### Opción C: Variables de Entorno (CI/CD)

```bash
# Linux/macOS
export CERTIFICADO__TIPO="Archivo"
export CERTIFICADO__PFXPATH="/opt/certs/certificado.pfx"
export CERTIFICADO__PFXPASSWORD="password-segura"

# Windows PowerShell
$env:CERTIFICADO__TIPO = "Almacen"
$env:CERTIFICADO__THUMBPRINT = "ABC123..."
$env:CERTIFICADO__STORELOCATION = "CurrentUser"
```

##### Validar Certificado

Usa el script de diagnóstico para verificar que tu certificado cumple los requisitos:

```powershell
# Validar archivo PFX
.\scripts\diagnose-certificates.ps1 -PfxPath "certificado.pfx"

# Validar certificado instalado
.\scripts\diagnose-certificates.ps1 -Thumbprint "ABC123..."

# Listar todos los certificados disponibles
.\scripts\diagnose-certificates.ps1 -ListAll
```

> 📚 **Más información**: Consulta la [Guía de Certificados y Seguridad](./certificados-y-seguridad.md) para:
> - Requisitos detallados de VERI*FACTU
> - Troubleshooting de certificados
> - Mejores prácticas de seguridad
> - Gestión del ciclo de vida

#### 2. Configurar Endpoint de VERI*FACTU

Actualiza la configuración del servicio VERI*FACTU en el archivo apropiado:

```json
{
  "Verifactu": {
    "EndpointUrl": "https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion",
    "SoapAction": "RegFacturacionAlta",
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
