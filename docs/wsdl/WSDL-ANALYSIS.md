# Análisis del WSDL Oficial de AEAT para VERI*FACTU

## 📋 Resumen Ejecutivo

Este documento contiene el análisis detallado del WSDL oficial de AEAT para el sistema VERI*FACTU, comparándolo con la implementación actual del cliente SOAP.

## 🌐 URLs Oficiales del WSDL

### Producción
```
https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl
```

**Nota alternativa (según documentación):**
```
https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SistemaFacturacion.wsdl
```

### Sandbox (Pruebas)
```
https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SistemaFacturacion.wsdl
```

## 📍 Endpoints de Servicio

### Producción
- **URL Base**: `https://www1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion`
- **Protocolo**: HTTPS (TLS 1.2+)
- **Puerto**: 443 (estándar HTTPS)
- **Autenticación**: mTLS (Mutual TLS) con certificado digital

### Sandbox
- **URL Base**: `https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion`
- **Protocolo**: HTTPS (TLS 1.2+)
- **Puerto**: 443 (estándar HTTPS)
- **Autenticación**: mTLS (Mutual TLS) con certificado digital de pruebas

## 🔧 Operaciones del Servicio

### 1. RegFactuSistemaFacturacion

**Descripción**: Alta o anulación de registros de facturación

**Binding**: `RegFactuSistemaFacturacionBinding`
- Estilo: `document/literal`
- Transporte: `http://schemas.xmlsoap.org/soap/http`

**Puerto**: `RegFactuSistemaFacturacionPort`

**Mensaje de Entrada**: `RegFactuSistemaFacturacion`
- Namespace: `https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroLR.xsd`
- Elemento raíz: `<sfLR:RegFactuSistemaFacturacion>`

**Mensaje de Salida**: `RespuestaRegFactuSistemaFacturacion`
- Namespace: `https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaSuministro.xsd`
- Elemento raíz: `<sfResp:RespuestaRegFactuSistemaFacturacion>`

**SOAPAction**: `""` (vacío según WSDL oficial)

**Estructura del Mensaje SOAP**:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:sfLR="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroLR.xsd"
                  xmlns:sf="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd">
  <soapenv:Header/>
  <soapenv:Body>
    <sfLR:RegFactuSistemaFacturacion>
      <sfLR:Cabecera>
        <sf:ObligadoEmision>
          <sf:NombreRazon>...</sf:NombreRazon>
          <sf:NIF>...</sf:NIF>
        </sf:ObligadoEmision>
      </sfLR:Cabecera>
      <sfLR:RegistroFactura>
        <sf:RegistroAlta>
          <!-- Contenido según SuministroInformacion.xsd -->
        </sf:RegistroAlta>
      </sfLR:RegistroFactura>
    </sfLR:RegFactuSistemaFacturacion>
  </soapenv:Body>
</soapenv:Envelope>
```

### 2. ConsultaFactuSistemaFacturacion

**Descripción**: Consulta de registros de facturación previamente enviados

**Binding**: `ConsultaFactuSistemaFacturacionBinding`
- Estilo: `document/literal`
- Transporte: `http://schemas.xmlsoap.org/soap/http`

**Puerto**: `ConsultaFactuSistemaFacturacionPort`

**Mensaje de Entrada**: `ConsultaFactuSistemaFacturacion`
- Namespace: `https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/ConsultaLR.xsd`
- Elemento raíz: `<con:ConsultaFactuSistemaFacturacion>`

**Mensaje de Salida**: `RespuestaConsultaFactuSistemaFacturacion`
- Namespace: `https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaConsultaLR.xsd`
- Elemento raíz: `<conResp:RespuestaConsultaFactuSistemaFacturacion>`

**SOAPAction**: `""` (vacío según WSDL oficial)

## 📦 Namespaces Oficiales

### SOAP
```xml
xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
```

### Servicios VERI*FACTU
```xml
xmlns:sfLR="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroLR.xsd"
xmlns:sf="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd"
xmlns:sfResp="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaSuministro.xsd"
xmlns:con="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/ConsultaLR.xsd"
xmlns:conResp="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaConsultaLR.xsd"
```

## ✅ Validación de Implementación Actual

### Estado del Cliente SOAP (`VerifactuSoapClient.cs`)

| Aspecto | Estado | Notas |
|---------|--------|-------|
| Namespaces SOAP | ✅ Correcto | `http://schemas.xmlsoap.org/soap/envelope/` |
| Namespace SuministroLR | ✅ Correcto | Implementado correctamente |
| Namespace SuministroInformacion | ✅ Correcto | Implementado correctamente |
| Namespace RespuestaSuministro | ✅ Correcto | Usado en parseo de respuestas |
| Namespace ConsultaLR | ✅ Correcto | Usado en operación de consulta |
| Namespace RespuestaConsultaLR | ✅ Correcto | Usado en parseo de consultas |
| Estructura sobre SOAP | ✅ Correcto | `ConstruirSobreSoap()` conforme |
| SOAPAction header | ✅ Correcto | Configurable, vacío por defecto |
| Endpoint URL | ✅ Correcto | Configurable para sandbox/producción |
| mTLS authentication | ✅ Correcto | `HttpClientHandler.ClientCertificates` |
| Timeout | ✅ Correcto | 120 segundos configurable |
| Encoding | ✅ Correcto | UTF-8 |
| Content-Type | ✅ Correcto | `text/xml; charset=utf-8` |
| Operación RegFacturacionAlta | ✅ Implementada | `EnviarRegFacturacionAltaAsync()` |
| Operación ConsultaLRFacturas | ✅ Implementada | `ConsultarLRFacturasAsync()` |
| Parseo de respuestas | ✅ Correcto | Métodos `ParsearRespuesta*()` |

### ✅ Conclusión: NO se requieren ajustes

La implementación actual del cliente SOAP está **100% conforme al WSDL oficial**:
- Todos los namespaces coinciden con el WSDL
- La estructura SOAP es correcta
- Los headers HTTP son apropiados
- La autenticación mTLS está bien implementada
- Las operaciones están correctamente definidas

## 📊 Headers HTTP

### Headers enviados por el cliente

```http
POST /wlpl/TIKE-CONT/SistemaFacturacion HTTP/1.1
Host: prewww1.aeat.es (o www1.aeat.es en producción)
Content-Type: text/xml; charset=utf-8
SOAPAction: ""
Content-Length: [calculado]
User-Agent: [implícito por HttpClient]
```

### Headers de autenticación TLS

La autenticación se realiza a nivel TLS durante el handshake, no en headers HTTP.

## 🔒 Requisitos de Seguridad

### Certificado Digital

**Requisitos**:
- Tipo: Certificado de representante de persona jurídica
- Formato: PFX/PKCS#12 con clave privada
- Algoritmo: RSA 2048+ bits o ECDSA 256+ bits
- Estado: Válido, no caducado, no revocado
- NIF: Debe coincidir con el NIF del emisor de facturas

### TLS

**Protocolo**: TLS 1.2 o superior (TLS 1.3 recomendado)
**Cipher Suites**: Según política de seguridad de AEAT
**.NET 9**: Usa automáticamente TLS 1.2+ por defecto

## ⚙️ Configuración Recomendada

### appsettings.Sandbox.json

```json
{
  "Verifactu": {
    "Environment": "Sandbox",
    "EndpointUrl": "https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion",
    "SoapAction": "",
    "Timeout": 120,
    "MaxRetries": 3,
    "RetryDelayMs": 2000
  }
}
```

### appsettings.Production.json

```json
{
  "Verifactu": {
    "Environment": "Production",
    "EndpointUrl": "https://www1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion",
    "SoapAction": "",
    "Timeout": 120,
    "MaxRetries": 3,
    "RetryDelayMs": 2000
  }
}
```

## 🔍 Validación de Respuestas

### Estados de Envío (EstadoEnvio)

| Valor | Significado |
|-------|-------------|
| `Correcto` | Todos los registros aceptados |
| `ParcialmenteCorrecto` | Algunos registros aceptados, otros rechazados |
| `Incorrecto` | Todos los registros rechazados |

### Estados de Registro (EstadoRegistro)

| Valor | Significado |
|-------|-------------|
| `Correcto` | Registro aceptado sin errores |
| `AceptadoConErrores` | Registro aceptado con errores admisibles |
| `Incorrecto` | Registro rechazado |

## 📝 Diferencias Sandbox vs Producción

| Aspecto | Sandbox | Producción |
|---------|---------|------------|
| Endpoint | `prewww1.aeat.es` | `www1.aeat.es` |
| Certificado | Pruebas | Real válido |
| Validaciones | Más permisivas | Estrictas |
| Datos | Sin validez tributaria | Vinculantes legalmente |
| Disponibilidad | Puede tener interrupciones | SLA garantizado |
| Rate limiting | Más permisivo | Más restrictivo |

## 🚨 Errores Comunes

### Error de Certificado TLS

**Síntoma**: `SSL connection error`, `certificate not trusted`

**Causa**: Certificado inválido, revocado o NIF no coincidente

**Solución**: 
- Verificar validez del certificado
- Confirmar que NIF del certificado = NIF del emisor
- Actualizar certificado si está caducado

### Timeout

**Síntoma**: `TaskCanceledException`, `Timeout`

**Causa**: Red lenta, servidor sobrecargado, payload muy grande

**Solución**:
- Aumentar timeout si es necesario
- Verificar conectividad
- Reducir número de registros por envío

### Error SOAP Fault

**Síntoma**: Respuesta con `<soap:Fault>`

**Causa**: XML malformado, validación XSD fallida, datos incorrectos

**Solución**:
- Validar XML contra XSD local antes de enviar
- Revisar logs detallados del error
- Consultar códigos de error en documentación AEAT

## 📚 Referencias

- **Documentación WSDL**: `docs/wsdl/README.md`
- **Guía de uso**: `docs/uso-cliente-soap.md`
- **Descripción servicios web**: `docs/Veri-Factu_Descripcion_SWeb.md`
- **Guía técnica**: `docs/Verifactu-Guia-Tecnica.md`
- **Tests de integración**: `tests/Verifactu.Integration.Tests/SandboxTests.cs`

## 📅 Historial de Cambios

| Fecha | Versión | Cambios |
|-------|---------|---------|
| 2025-11-07 | 1.0 | Análisis inicial del WSDL oficial |

---

**Estado**: ✅ VALIDADO - Cliente SOAP conforme al 100% con WSDL oficial de AEAT
