# Guía Rápida: Configuración Sandbox AEAT

Esta es una guía de referencia rápida para configurar el entorno de pruebas. Para detalles completos, consulta [entorno-pruebas.md](entorno-pruebas.md).

## ⚡ Setup Rápido (5 minutos)

### 1. Prerequisitos
- ✅ .NET 9 SDK instalado
- ✅ Certificado digital válido en formato PFX
- ✅ Acceso al Portal de Pruebas Externas AEAT

### 2. Configurar User Secrets

```bash
cd src/Verifactu.ConsoleDemo
dotnet user-secrets init
dotnet user-secrets set "Certificado:PfxPath" "/ruta/a/certificado.pfx"
dotnet user-secrets set "Certificado:PfxPassword" "TuPassword"
```

### 3. Configurar Entorno

```bash
# Linux/macOS
export ASPNETCORE_ENVIRONMENT=Sandbox
export VERIFACTU_ENV=sandbox

# Windows PowerShell
$env:ASPNETCORE_ENVIRONMENT="Sandbox"
$env:VERIFACTU_ENV="sandbox"
```

### 4. Ejecutar Demo

```bash
dotnet run
```

## 📋 Endpoints del Sandbox

| Servicio | URL |
|----------|-----|
| **SOAP Endpoint** | `https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion` |
| **WSDL** | `https://www2.agenciatributaria.gob.es/.../SistemaFacturacion.wsdl` |
| **Portal Pruebas** | `https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/portal-pruebas-externas.html` |
| **XSD** | `https://www2.agenciatributaria.gob.es/.../ws/` |

## 🔑 Variables de Entorno Clave

```bash
# Certificado
VERIFACTU_CERT_PATH="/ruta/certificado.pfx"
VERIFACTU_CERT_PASSWORD="password"

# Endpoint
VERIFACTU_ENDPOINT_URL="https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion"
VERIFACTU_ENV="sandbox"

# Configuración
VERIFACTU_LOG_LEVEL="Debug"
VERIFACTU_MAX_RETRIES="3"
```

## 🧪 Casos de Prueba Básicos

### 1. Envío Simple
```bash
# Usar factura de ejemplo sandbox
dotnet run -- --factura factura-demo-sandbox.json
```

### 2. Encadenamiento
```bash
# Primer registro (sin huella anterior)
dotnet run

# Segundo registro (con huella del primero)
# Actualizar VERIFACTU_HUELLA_ANTERIOR con el hash del primero
export VERIFACTU_HUELLA_ANTERIOR="hash_del_registro_anterior"
dotnet run
```

### 3. Validación XML
```bash
# Descargar XSD del portal de pruebas
mkdir xsd && cd xsd
# Descargar manualmente desde portal (requiere certificado)

# Validar con xmllint
xmllint --noout --schema xsd/SuministroLR.xsd output/registro.xml
```

## 🔍 Verificación Rápida

### Comprobar Certificado
```bash
# Ver información del certificado
openssl pkcs12 -in certificado.pfx -nokeys -info

# Verificar fechas
openssl pkcs12 -in certificado.pfx -nokeys | openssl x509 -noout -dates
```

### Comprobar Conectividad
```bash
# Ping al endpoint (debe responder, aunque con error SOAP)
curl -v https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion
```

## ⚠️ Errores Comunes

### "No se puede establecer conexión TLS"
**Solución**: Verificar que el certificado tiene clave privada y está en formato PFX correcto.

### "UUID duplicado"
**Solución**: Generar nuevo UUID para cada registro:
```csharp
var uuid = Guid.NewGuid().ToString();
```

### "Error de validación XML"
**Solución**: Validar XML localmente contra XSD antes de enviar.

### "Timeout"
**Solución**: Aumentar timeout en appsettings.Sandbox.json:
```json
{
  "Verifactu": {
    "Timeout": 60
  }
}
```

## 📁 Estructura de Archivos

```
src/Verifactu.ConsoleDemo/
├── appsettings.json              # Configuración base
├── appsettings.Sandbox.json      # Configuración sandbox ✅
├── factura-demo.json             # Factura ejemplo
└── factura-demo-sandbox.json     # Factura sandbox ✅
```

## 🔐 Seguridad

### ❌ NO HACER
- Nunca versionar archivos .pfx
- Nunca poner contraseñas en appsettings.json
- Nunca compartir certificados

### ✅ HACER
- Usar dotnet user-secrets en desarrollo
- Limitar permisos del archivo PFX: `chmod 600 certificado.pfx`
- Usar variables de entorno para secretos
- Añadir *.pfx al .gitignore

## 📚 Recursos Adicionales

- **[Guía Completa de Sandbox](entorno-pruebas.md)** - Documentación detallada
- **[Guía Técnica VERI*FACTU](Verifactu-Guia-Tecnica.md)** - Especificaciones técnicas
- **[Portal AEAT](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/portal-pruebas-externas.html)** - Portal oficial de pruebas

## 🆘 Soporte

- **Issues**: [GitHub Issues](https://github.com/JoseRGWeb/Veri-factuSender/issues)
- **FAQs AEAT**: [Preguntas Frecuentes](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/preguntas-frecuentes.html)
- **Documentación**: [docs/](../docs/)

---

**Última actualización**: 30 de octubre de 2025
