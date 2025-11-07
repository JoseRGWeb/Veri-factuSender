# Certificados de Prueba para Tests de Integración

## ⚠️ IMPORTANTE

**NUNCA versionar certificados reales en Git**

Este directorio está destinado a documentación sobre certificados de prueba, NO para almacenar los archivos PFX reales.

## 📋 Certificados Requeridos

Para ejecutar tests de integración contra el sandbox de AEAT, necesitas:

### Certificado Digital Válido

- **Formato**: PFX/PKCS#12 con clave privada
- **Uso**: Firma electrónica y autenticación cliente (mTLS)
- **Emisor**: Autoridad de certificación reconocida por AEAT
- **Tipo**: Puede ser certificado de representante, empleado público, o certificado de entidad

### Tipos de Certificados Válidos

1. **Certificado de Representante** (recomendado para empresas)
   - Emitido por FNMT, Camerfirma, etc.
   - Vinculado a NIF de empresa
   - Permite representar a la entidad

2. **Certificado de Persona Física**
   - DNI electrónico
   - Certificado FNMT de ciudadano
   - Solo para pruebas personales

3. **Certificado de Entidad**
   - Certificado digital de empresa
   - Para sistemas automatizados

## 🔧 Configuración de Certificados

### Opción 1: User Secrets (Recomendado)

```bash
# Navegar al proyecto de tests
cd tests/Verifactu.Integration.Tests

# Configurar ruta del certificado
dotnet user-secrets set "Certificado:PfxPath" "/ruta/absoluta/al/certificado.pfx"
dotnet user-secrets set "Certificado:PfxPassword" "tu_password_segura"
```

### Opción 2: Variables de Entorno

```bash
# Linux/macOS
export Certificado__PfxPath="/ruta/al/certificado.pfx"
export Certificado__PfxPassword="password"

# Windows PowerShell
$env:Certificado__PfxPath="C:\Certificados\certificado.pfx"
$env:Certificado__PfxPassword="password"
```

### Opción 3: Archivo de Configuración Local

Crear `appsettings.Sandbox.local.json` (ignorado por .gitignore):

```json
{
  "Certificado": {
    "PfxPath": "/ruta/absoluta/certificado.pfx",
    "PfxPassword": "password"
  }
}
```

## 🔐 Seguridad de Certificados

### Protección del Archivo PFX

```bash
# Linux/macOS - Permisos restrictivos
chmod 600 /ruta/al/certificado.pfx

# Windows PowerShell (como administrador)
icacls "C:\Certificados\certificado.pfx" /inheritance:r
icacls "C:\Certificados\certificado.pfx" /grant:r "%USERNAME%:R"
```

### Almacenamiento Seguro

1. **Desarrollo Local**
   - Guardar fuera del repositorio
   - Usar user-secrets de .NET
   - Permisos restrictivos en el archivo

2. **CI/CD**
   - GitHub Secrets
   - Azure Key Vault
   - AWS Secrets Manager
   - Evitar variables de entorno en logs

3. **Producción**
   - Hardware Security Module (HSM)
   - Azure Key Vault
   - AWS Certificate Manager
   - Nunca en disco sin cifrar

## 📝 Obtener Certificado de Prueba

### FNMT (España)

1. Visitar https://www.sede.fnmt.gob.es
2. Solicitar certificado de representante o ciudadano
3. Completar proceso de verificación de identidad
4. Descargar certificado en formato PFX

### Camerfirma

1. Visitar https://www.camerfirma.com
2. Solicitar certificado digital
3. Seguir proceso de verificación
4. Descargar en formato PFX

### Para Tests (Sandbox AEAT)

**IMPORTANTE**: El sandbox de AEAT puede requerir:
- Certificado real emitido por CA reconocida
- No acepta certificados auto-firmados
- Verificar compatibilidad con entorno de pruebas

## 🧪 Verificar Certificado

### Usando OpenSSL

```bash
# Ver información del certificado
openssl pkcs12 -in certificado.pfx -nokeys -info

# Verificar que contiene clave privada
openssl pkcs12 -in certificado.pfx -nocerts -info

# Extraer certificado (sin clave privada)
openssl pkcs12 -in certificado.pfx -clcerts -nokeys -out cert.pem
```

### Usando PowerShell (Windows)

```powershell
# Cargar y ver certificado
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2("certificado.pfx", "password")
$cert | Format-List

# Verificar clave privada
$cert.HasPrivateKey
```

### Usando .NET (C#)

```csharp
using System.Security.Cryptography.X509Certificates;

var cert = X509CertificateLoader.LoadPkcs12FromFile("certificado.pfx", "password");
Console.WriteLine($"Subject: {cert.Subject}");
Console.WriteLine($"Valid From: {cert.NotBefore}");
Console.WriteLine($"Valid To: {cert.NotAfter}");
Console.WriteLine($"Has Private Key: {cert.HasPrivateKey}");
```

## 🚫 Archivos Protegidos por .gitignore

Los siguientes archivos están excluidos automáticamente:

- `*.pfx`
- `*.p12`
- `*.key`
- `*.pem` (claves privadas)
- `appsettings.*.local.json`
- `secrets.json`

**NUNCA** eliminar estas exclusiones del `.gitignore`

## 🆘 Troubleshooting

### Error: "The certificate chain was issued by an authority that is not trusted"

- El certificado no es de una CA reconocida
- Instalar certificados intermedios
- Verificar que es un certificado válido para AEAT

### Error: "Cannot load certificate"

- Verificar ruta absoluta del archivo
- Verificar que el password es correcto
- Verificar permisos de lectura del archivo
- Verificar formato PFX válido

### Error: "Certificate does not contain a private key"

- El archivo PFX debe incluir la clave privada
- Re-exportar desde el almacén de certificados incluyendo clave privada

## 📚 Referencias

- [FNMT - Certificados Electrónicos](https://www.sede.fnmt.gob.es)
- [AEAT - Portal VERI*FACTU](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu)
- [Documentación .NET sobre X509Certificate2](https://learn.microsoft.com/es-es/dotnet/api/system.security.cryptography.x509certificates.x509certificate2)

---

**Última actualización**: 7 de noviembre de 2025  
**Versión**: 1.0
