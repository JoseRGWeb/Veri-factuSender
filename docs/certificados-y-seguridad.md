# Guía de Certificados y Seguridad para VERI*FACTU

Esta guía completa explica cómo gestionar certificados digitales y configurar la seguridad en VerifactuSender para cumplir con los requisitos de VERI*FACTU de la AEAT.

## Tabla de Contenidos

1. [Requisitos de Certificados](#requisitos-de-certificados)
2. [Métodos de Carga de Certificados](#métodos-de-carga-de-certificados)
3. [Configuración Segura](#configuración-segura)
4. [Scripts de Ayuda](#scripts-de-ayuda)
5. [Validación de Certificados](#validación-de-certificados)
6. [Troubleshooting](#troubleshooting)
7. [Mejores Prácticas](#mejores-prácticas)

---

## Requisitos de Certificados

### Requisitos Obligatorios para VERI*FACTU

#### Producción
- **Tipo**: Certificado de representante de persona jurídica
- **Emisor**: Autoridad certificadora reconocida por la AEAT:
  - FNMT-RCM (Fábrica Nacional de Moneda y Timbre)
  - ACCV (Agencia de Certificación de la Comunidad Valenciana)
  - Camerfirma
  - Otras CAs reconocidas por la AEAT
- **NIF**: El NIF del certificado debe coincidir con el NIF del emisor de facturas

#### Sandbox/Pruebas
- Cualquier certificado digital válido y no caducado
- Certificados de prueba son aceptados
- Certificados autofirmados funcionan para desarrollo local

### Requisitos Técnicos

| Requisito | Especificación |
|-----------|---------------|
| **Formato** | PFX/PKCS#12 (debe incluir clave privada) |
| **Algoritmo de firma** | RSA con SHA-256 o superior / ECDSA |
| **Longitud de clave** | RSA mínimo 2048 bits / ECDSA mínimo 256 bits |
| **Estado** | Válido (no caducado, no revocado) |
| **Propósito** | Digital Signature + Client Authentication (OID: 1.3.6.1.5.5.7.3.2) |
| **Clave privada** | Obligatoria (para firma digital y mTLS) |

---

## Métodos de Carga de Certificados

VerifactuSender soporta tres métodos de carga de certificados:

### 1. Desde Archivo PFX (Método Tradicional)

**Ventajas:**
- Simple y directo
- Portátil entre sistemas
- Ideal para desarrollo local

**Desventajas:**
- Requiere gestionar archivos físicos
- Contraseña debe estar en configuración o variables de entorno
- Menos seguro para producción

**Configuración:**

```json
{
  "Certificado": {
    "Tipo": "Archivo",
    "PfxPath": "/ruta/completa/a/certificado.pfx",
    "PfxPassword": ""  // Usar user-secrets o variables de entorno
  }
}
```

**Uso con User Secrets (Desarrollo):**

```bash
# Inicializar user-secrets
cd src/Verifactu.ConsoleDemo
dotnet user-secrets init

# Configurar la contraseña
dotnet user-secrets set "Certificado:PfxPassword" "mi-password-segura"

# Configurar la ruta (opcional)
dotnet user-secrets set "Certificado:PfxPath" "C:\certs\mi-certificado.pfx"
```

**Uso con Variables de Entorno (Producción):**

```bash
# Linux/macOS
export CERTIFICADO__PFXPATH="/opt/certs/certificado.pfx"
export CERTIFICADO__PFXPASSWORD="mi-password-segura"

# Windows PowerShell
$env:CERTIFICADO__PFXPATH = "C:\certs\certificado.pfx"
$env:CERTIFICADO__PFXPASSWORD = "mi-password-segura"

# Windows CMD
set CERTIFICADO__PFXPATH=C:\certs\certificado.pfx
set CERTIFICADO__PFXPASSWORD=mi-password-segura
```

### 2. Desde Almacén de Certificados del Sistema (Recomendado para Producción)

**Ventajas:**
- Más seguro: no hay archivos PFX que proteger
- No requiere contraseña en configuración
- Windows gestiona permisos automáticamente
- Fácil rotación de certificados

**Desventajas:**
- Solo disponible en Windows
- Requiere instalación previa del certificado

**Instalación del Certificado:**

Opción A - Usar el script automatizado:
```powershell
.\scripts\setup-certificates.ps1 -PfxPath "C:\certs\certificado.pfx"
```

Opción B - Instalación manual:
```powershell
# Importar el certificado
$cert = [System.Security.Cryptography.X509Certificates.X509CertificateLoader]::LoadPkcs12FromFile(
    "C:\certs\certificado.pfx",
    "password"
)

# Añadir al almacén
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store("My", "CurrentUser")
$store.Open("ReadWrite")
$store.Add($cert)
$store.Close()

# Obtener el thumbprint
Write-Host "Thumbprint: $($cert.Thumbprint)"
```

Opción C - Interfaz gráfica de Windows:
1. Doble clic en el archivo .pfx
2. Seguir el asistente de importación
3. Seleccionar almacén "Personal" del usuario actual
4. Copiar el thumbprint desde `certmgr.msc`

**Configuración:**

```json
{
  "Certificado": {
    "Tipo": "Almacen",
    "Thumbprint": "3B7E039FDBDA89ABC12345678901234567890ABC",
    "StoreLocation": "CurrentUser",  // o "LocalMachine"
    "StoreName": "My"               // "My" = Personal
  }
}
```

**Valores de StoreLocation:**
- `CurrentUser`: Almacén del usuario actual (no requiere admin)
- `LocalMachine`: Almacén de la máquina (puede requerir admin)

**Valores de StoreName:**
- `My`: Certificados personales (más común)
- `Root`: Autoridades raíz confiables
- `CA`: Autoridades intermedias
- `TrustedPeople`: Personas de confianza

**Comandos útiles PowerShell:**

```powershell
# Listar todos los certificados en "Personal" del usuario
Get-ChildItem Cert:\CurrentUser\My | Format-List Subject, Thumbprint, NotAfter

# Buscar certificado por thumbprint
Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Thumbprint -eq "ABC123..." }

# Ver detalles de un certificado
Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Subject -like "*MiEmpresa*" } | Format-List *
```

### 3. Desde Azure Key Vault (Futuro)

**Estado:** No implementado en esta versión

Esta funcionalidad está planificada para una versión futura y requerirá:
- Dependencias adicionales (Azure SDK)
- Configuración de Azure Key Vault
- Autenticación con Azure (Managed Identity o Service Principal)

---

## Configuración Segura

### Jerarquía de Configuración

VerifactuSender carga la configuración en este orden (último gana):

1. `appsettings.json` (configuración base)
2. `appsettings.{Environment}.json` (específico del entorno)
3. **User Secrets** (solo en desarrollo)
4. **Variables de entorno** (más prioridad)
5. **Argumentos de línea de comandos** (máxima prioridad)

### Configuración por Entorno

#### Desarrollo Local

**Archivo:** `appsettings.Development.json` (en .gitignore)

```json
{
  "Certificado": {
    "Tipo": "Archivo",
    "PfxPath": "C:\\dev\\certs\\test-cert.pfx"
  }
}
```

**User Secrets:**
```bash
dotnet user-secrets set "Certificado:PfxPassword" "password-desarrollo"
```

#### Sandbox/Pruebas

**Archivo:** `appsettings.Sandbox.json`

```json
{
  "Certificado": {
    "Tipo": "Almacen",
    "Thumbprint": "DEV123...",
    "StoreLocation": "CurrentUser"
  }
}
```

#### Producción

**Archivo:** `appsettings.Production.json`

```json
{
  "Certificado": {
    "Tipo": "Almacen",
    "Thumbprint": "",  // Configurar via variables de entorno
    "StoreLocation": "LocalMachine"
  }
}
```

**Variables de Entorno:**
```bash
export CERTIFICADO__THUMBPRINT="PROD456789..."
export CERTIFICADO__STORELOCATION="LocalMachine"
```

### Protección de Secretos

#### ✅ HACER

- Usar `dotnet user-secrets` en desarrollo
- Usar variables de entorno en producción
- Usar almacén de certificados del sistema cuando sea posible
- Rotar certificados antes de su expiración (30 días mínimo)
- Configurar alertas de expiración
- Usar certificados diferentes por entorno

#### ❌ NO HACER

- **NUNCA** versionar contraseñas en Git
- **NUNCA** versionar archivos .pfx en Git
- **NUNCA** dejar contraseñas hardcodeadas en appsettings.json
- **NUNCA** compartir certificados de producción por email/Slack
- **NUNCA** usar el mismo certificado en todos los entornos

---

## Scripts de Ayuda

### setup-certificates.ps1

Script interactivo para instalar certificados en el almacén de Windows.

**Uso básico:**
```powershell
.\scripts\setup-certificates.ps1
```

**Uso avanzado:**
```powershell
# Instalar en almacén del usuario
.\scripts\setup-certificates.ps1 `
    -PfxPath "C:\certs\certificado.pfx" `
    -StoreLocation CurrentUser `
    -StoreName My

# Solo mostrar información sin instalar
.\scripts\setup-certificates.ps1 `
    -PfxPath "C:\certs\certificado.pfx" `
    -InfoOnly
```

**Salida esperada:**
```
Certificado cargado correctamente
Subject: CN=Mi Empresa, O=Mi Organización, C=ES
Thumbprint: 3B7E039FDBDA89ABC...
Válido hasta: 2025-12-31
Estado: Válido (340 días restantes)

Configuración para appsettings.json:
  "Certificado": {
    "Tipo": "Almacen",
    "Thumbprint": "3B7E039FDBDA89ABC...",
    "StoreLocation": "CurrentUser",
    "StoreName": "My"
  }
```

### diagnose-certificates.ps1

Script de diagnóstico para verificar que un certificado cumple los requisitos de VERI*FACTU.

**Uso básico:**
```powershell
# Listar todos los certificados disponibles
.\scripts\diagnose-certificates.ps1 -ListAll

# Diagnosticar certificado desde almacén
.\scripts\diagnose-certificates.ps1 -Thumbprint "ABC123..."

# Diagnosticar archivo PFX
.\scripts\diagnose-certificates.ps1 -PfxPath "C:\certs\certificado.pfx"
```

**Salida esperada:**
```
Verificando requisitos de VERI*FACTU...
✓ Verificando clave privada... OK
✓ Verificando fechas de validez... OK (340 días restantes)
✓ Verificando tipo de clave... OK (RSA 2048 bits)
✓ Verificando algoritmo de firma... OK (sha256RSA)
✓ Verificando propósitos del certificado... OK
✓ Verificando cadena de confianza... OK

RESUMEN DEL DIAGNÓSTICO
✓ El certificado cumple TODOS los requisitos de VERI*FACTU
```

---

## Validación de Certificados

### Validación Automática

El `CertificateLoader` valida automáticamente:

1. **Clave privada**: Debe estar presente
2. **Fechas de validez**: Debe estar dentro del período válido
3. **Tamaño de clave**: RSA ≥ 2048 bits, ECDSA ≥ 256 bits
4. **Algoritmo de firma**: SHA-256 o superior (advertencia)

### Validación Programática

```csharp
using Verifactu.Client.Services;

var certLoader = new CertificateLoader();

// Cargar certificado
var cert = certLoader.CargarDesdePfx("cert.pfx", "password");

// Validar requisitos VERI*FACTU
bool esValido = certLoader.ValidarCertificado(cert);

// Obtener información detallada
var info = certLoader.ObtenerInformacion(cert);
Console.WriteLine($"Subject: {info.Subject}");
Console.WriteLine($"Días hasta expiración: {info.TiempoHastaExpiracion.TotalDays}");
Console.WriteLine($"Tipo de clave: {info.TipoClave} ({info.TamanoClaveBits} bits)");

// Verificar tiempo de expiración
var tiempoRestante = certLoader.TiempoHastaExpiracion(cert);
if (tiempoRestante.TotalDays < 30) {
    Console.WriteLine("¡ADVERTENCIA! El certificado expira pronto");
}
```

---

## Troubleshooting

### Error: "No se encontró el archivo de certificado"

**Problema:** La ruta al archivo PFX es incorrecta o el archivo no existe.

**Solución:**
```bash
# Verificar que el archivo existe
ls -la /ruta/a/certificado.pfx    # Linux/macOS
dir C:\ruta\certificado.pfx       # Windows

# Usar rutas absolutas en configuración
"PfxPath": "C:\\certs\\certificado.pfx"  # Windows (escapar backslashes)
"PfxPath": "/opt/certs/certificado.pfx"  # Linux
```

### Error: "Contraseña incorrecta" (CryptographicException)

**Problema:** La contraseña del PFX es incorrecta.

**Solución:**
```bash
# Verificar que la contraseña es correcta
dotnet user-secrets set "Certificado:PfxPassword" "password-correcta"

# O configurar variable de entorno
export CERTIFICADO__PFXPASSWORD="password-correcta"
```

### Error: "El certificado no contiene clave privada"

**Problema:** El certificado fue exportado sin clave privada.

**Solución:**
```bash
# Al exportar desde certmgr.msc, asegúrate de marcar:
# ☑ "Exportar la clave privada"

# Verificar que el certificado tiene clave privada con PowerShell:
$cert = Get-PfxCertificate -FilePath "certificado.pfx"
$cert.HasPrivateKey  # Debe ser True
```

### Error: "El certificado ha caducado"

**Problema:** El certificado está fuera de su período de validez.

**Solución:**
```powershell
# Verificar fechas con el script de diagnóstico
.\scripts\diagnose-certificates.ps1 -PfxPath "certificado.pfx"

# Renovar el certificado con tu autoridad certificadora
# En producción: planifica renovación 30-60 días antes de expiración
```

### Error: "No se encontró ningún certificado con thumbprint..."

**Problema:** El thumbprint es incorrecto o el certificado no está instalado.

**Solución:**
```powershell
# Listar certificados disponibles
.\scripts\diagnose-certificates.ps1 -ListAll

# Verificar thumbprint correcto
Get-ChildItem Cert:\CurrentUser\My | Format-List Subject, Thumbprint

# Instalar el certificado si no está
.\scripts\setup-certificates.ps1 -PfxPath "certificado.pfx"
```

### Error de permisos en LocalMachine

**Problema:** No tienes permisos para acceder al almacén LocalMachine.

**Solución:**
```json
// Cambiar a CurrentUser en configuración
{
  "Certificado": {
    "StoreLocation": "CurrentUser"  // En lugar de LocalMachine
  }
}
```

---

## Mejores Prácticas

### 🔒 Seguridad

1. **Separación de Entornos**
   - Usar certificados diferentes para desarrollo, sandbox y producción
   - Nunca usar certificados de producción en desarrollo

2. **Protección de Claves Privadas**
   - Usar almacén de certificados del sistema en producción
   - Establecer permisos restrictivos en archivos PFX (chmod 600 en Linux)
   - Nunca versionar certificados en Git

3. **Gestión de Contraseñas**
   - Usar user-secrets en desarrollo
   - Usar variables de entorno o gestores de secretos en producción
   - Rotar contraseñas periódicamente

4. **Monitoreo**
   - Configurar alertas de expiración (30 días antes)
   - Revisar logs de errores de certificados
   - Validar certificados después de renovación

### 📅 Ciclo de Vida de Certificados

1. **Planificación (90 días antes de expiración)**
   - Iniciar proceso de renovación con CA
   - Planificar ventana de mantenimiento

2. **Adquisición (60 días antes)**
   - Solicitar nuevo certificado
   - Validar nuevo certificado en sandbox
   - Documentar thumbprint y fechas

3. **Instalación (30 días antes)**
   - Instalar en entorno de pruebas
   - Ejecutar batería de tests
   - Documentar procedimiento

4. **Despliegue (15 días antes)**
   - Instalar en producción
   - Actualizar configuración
   - Verificar funcionamiento

5. **Limpieza (después de expiración del anterior)**
   - Remover certificado antiguo
   - Actualizar documentación
   - Archivar certificado anterior de forma segura

### 🚀 Automatización

Considera automatizar:

- Alertas de expiración próxima
- Validación periódica de certificados
- Backup de configuración de certificados
- Tests de conectividad con certificados

### 📝 Documentación

Mantener documentado:

- Thumbprints de certificados por entorno
- Fechas de expiración
- Procedimiento de renovación
- Contactos de la CA
- Historia de cambios de certificados

---

## Referencias

- [Documentación oficial .NET 9: X509CertificateLoader](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificateloader)
- [Gestión de certificados en Windows](https://docs.microsoft.com/en-us/windows/win32/seccrypto/certificate-stores)
- [Guía técnica VERI*FACTU](../docs/Verifactu-Guia-Tecnica.md)
- [Protocolos de comunicación](../docs/protocolos-comunicacion.md)

---

## Soporte

Si encuentras problemas:

1. Ejecutar el script de diagnóstico: `.\scripts\diagnose-certificates.ps1`
2. Revisar la sección de [Troubleshooting](#troubleshooting)
3. Consultar logs de la aplicación
4. Abrir un issue en GitHub con la salida del diagnóstico
