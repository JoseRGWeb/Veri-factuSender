# Tests de Integración - Verifactu.Integration.Tests

Este proyecto contiene tests de integración contra el Portal de Pruebas Externas (Sandbox) de AEAT para VERI*FACTU.

## ⚠️ Requisitos Importantes

**ESTOS TESTS REQUIEREN CONFIGURACIÓN MANUAL** y no se ejecutarán automáticamente sin un certificado digital válido.

### Requisitos Previos

1. **Certificado digital válido** (formato PFX/PKCS#12 con clave privada)
2. **Acceso a Internet** (puerto 443 para HTTPS)
3. **.NET 9 SDK** o superior

## 📋 Configuración

### Opción 1: User Secrets (Recomendado para Desarrollo)

```bash
# Navegar al proyecto de tests
cd tests/Verifactu.Integration.Tests

# Inicializar user secrets
dotnet user-secrets init

# Configurar rutas del certificado
dotnet user-secrets set "Certificado:PfxPath" "/ruta/completa/al/certificado.pfx"
dotnet user-secrets set "Certificado:PfxPassword" "TuPasswordSegura"

# Verificar configuración
dotnet user-secrets list
```

### Opción 2: Editar appsettings.Sandbox.json

**⚠️ NO VERSIONAR ESTE ARCHIVO CON CREDENCIALES REALES**

```json
{
  "Certificado": {
    "PfxPath": "C:/Certificados/mi-certificado.pfx",
    "PfxPassword": "MiPasswordSegura"
  }
}
```

### Opción 3: Variables de Entorno

```bash
# Linux/macOS
export Certificado__PfxPath="/ruta/al/certificado.pfx"
export Certificado__PfxPassword="PasswordSegura"

# Windows PowerShell
$env:Certificado__PfxPath="C:\Certificados\certificado.pfx"
$env:Certificado__PfxPassword="PasswordSegura"
```

## 🚀 Ejecución

### Ejecutar todos los tests de integración

```bash
# Desde la raíz del repositorio
dotnet test tests/Verifactu.Integration.Tests

# Con verbosidad detallada
dotnet test tests/Verifactu.Integration.Tests --logger "console;verbosity=detailed"
```

### Filtrar por categorías

```bash
# Solo tests de sandbox
dotnet test --filter "Category=Sandbox"

# Solo tests de integración
dotnet test --filter "Category=Integration"
```

### Ejecutar test específico

```bash
dotnet test --filter "FullyQualifiedName~EnviarFacturaCompleta_F1"
```

## 📝 Tests Implementados

### 1. ConexionTLSMutua_ConCertificadoValido_DebeConectar
- **Objetivo**: Validar conexión TLS mutua con certificado
- **Valida**: Configuración de certificado y handshake TLS
- **Resultado esperado**: No debe haber errores de SSL/TLS

### 2. EnviarFacturaCompleta_F1_DebeRecibirRespuestaAEAT
- **Objetivo**: Flujo completo de envío de factura F1
- **Valida**: Generación XML, firma, envío y respuesta
- **Resultado esperado**: Respuesta de AEAT con estado (Correcto/Incorrecto)

### 3. EnviarFacturaSimplificada_F2_DebeRecibirRespuestaAEAT
- **Objetivo**: Envío de factura simplificada F2
- **Valida**: Flujo F2 con requisitos reducidos
- **Resultado esperado**: Respuesta de AEAT

### 4. EnviarFacturaRectificativa_R1_DebeRecibirRespuestaAEAT
- **Objetivo**: Envío de factura rectificativa
- **Valida**: Estructura XML para rectificativas
- **Resultado esperado**: Respuesta de AEAT

### 5. EnviarFacturaConDatosInvalidos_DebeRetornarErrorValidacion
- **Objetivo**: Manejo de errores de validación
- **Valida**: Detección de datos inválidos
- **Resultado esperado**: Estado Incorrecto con código de error

### 6. EnviarFacturasEncadenadas_DebeValidarEncadenamiento
- **Objetivo**: Encadenamiento de huellas entre facturas
- **Valida**: Cálculo y encadenamiento correcto de huellas
- **Resultado esperado**: Ambas facturas enviadas con huellas diferentes

### 7. EnviarFacturaDuplicada_DebeDetectarDuplicado
- **Objetivo**: Detección de registros duplicados
- **Valida**: Idempotencia y detección de duplicados
- **Resultado esperado**: Segunda factura detectada como duplicada

## 🔍 Comportamiento sin Certificado

**Si no se configura certificado**, los tests:
- ✅ **Se ejecutan** pero retornan inmediatamente (pasan)
- 📝 No realizan llamadas reales a AEAT
- ⏱️ Se completan en milisegundos

Esto permite que CI/CD ejecute todos los tests sin requerir certificados.

## 📚 Documentación Completa

Para documentación detallada sobre:
- Configuración avanzada
- Interpretación de resultados
- Códigos de error AEAT
- Troubleshooting
- Migración a producción

**Consultar**: [docs/pruebas-integracion.md](../../docs/pruebas-integracion.md)

## 🔐 Seguridad

### Protección de Certificados

1. **NUNCA** versionar archivos PFX en Git
2. **Usar** user-secrets o variables de entorno
3. **Configurar** permisos restrictivos:
   ```bash
   # Linux/macOS
   chmod 600 /ruta/al/certificado.pfx
   
   # Windows PowerShell (como administrador)
   icacls "C:\Certificados\certificado.pfx" /inheritance:r
   icacls "C:\Certificados\certificado.pfx" /grant:r "%USERNAME%:R"
   ```
4. **En producción**, usar Azure Key Vault, AWS Secrets Manager, etc.

### Archivos Protegidos por .gitignore

Ya están configurados en `.gitignore`:
- `*.pfx`
- `*.p12`
- `*.key`
- `appsettings.*.local.json`
- `secrets.json`

## 🌐 Sandbox vs Producción

### Sandbox (Este proyecto)
- 🌐 Endpoint: `https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion`
- ✅ Datos **SIN validez tributaria**
- 🔄 Datos pueden ser eliminados periódicamente
- 🔓 Validaciones más permisivas

### Producción
- ⚠️ **NO ejecutar estos tests en producción**
- 📝 Crear proyecto separado: `Verifactu.Production.Tests`
- ✅ Usar datos reales y válidos
- 🔒 Certificados de representante válidos
- 📊 Ejecución manual supervisada

## 🆘 Troubleshooting

### "Test omitido: No hay certificado configurado"
- ✅ **Es normal** si no has configurado certificado
- 📝 Los tests pasan pero no ejecutan llamadas reales
- 🔧 Configurar certificado para ejecutar tests reales

### Error de conexión TLS
```
SSL connection could not be established
```
**Solución**: Verificar certificado
```bash
openssl pkcs12 -in certificado.pfx -nokeys -info
```

### Error de validación XML
```
SOAP Fault - Error de validación del XML
```
**Solución**: Revisar logs y comparar con ejemplos oficiales de AEAT

## 📞 Soporte

- **Documentación técnica**: [docs/](../../docs/)
- **Issues**: [GitHub Issues](https://github.com/JoseRGWeb/Veri-factuSender/issues)
- **AEAT FAQs**: [Portal de Pruebas Externas](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/portal-pruebas-externas.html)

---

**Última actualización**: 7 de noviembre de 2025  
**Versión**: 1.0
