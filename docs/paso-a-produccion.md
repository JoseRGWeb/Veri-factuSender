# Checklist de Paso de Pruebas a Producción

Esta guía proporciona un checklist completo y detallado para migrar el sistema VerifactuSender del entorno de pruebas (sandbox) al entorno de producción de VERI*FACTU de la AEAT.

## Índice

1. [Visión General](#visión-general)
2. [Requisitos Previos](#requisitos-previos)
3. [Checklist de Validaciones Previas](#checklist-de-validaciones-previas)
4. [Checklist de Configuración Técnica](#checklist-de-configuración-técnica)
5. [Checklist de Certificados](#checklist-de-certificados)
6. [Checklist de Endpoints](#checklist-de-endpoints)
7. [Checklist de Seguridad](#checklist-de-seguridad)
8. [Checklist de Monitorización y Logging](#checklist-de-monitorización-y-logging)
9. [Checklist de Plan de Contingencia](#checklist-de-plan-de-contingencia)
10. [Checklist de Validaciones Post-Migración](#checklist-de-validaciones-post-migración)
11. [Diferencias Clave entre Entornos](#diferencias-clave-entre-entornos)
12. [Procedimiento de Migración Paso a Paso](#procedimiento-de-migración-paso-a-paso)
13. [Rollback y Recuperación](#rollback-y-recuperación)
14. [Anexos y Referencias](#anexos-y-referencias)

---

## Visión General

### Objetivo

Este checklist asegura una migración controlada y segura del sistema VerifactuSender de pruebas a producción, minimizando riesgos y garantizando el cumplimiento de todas las validaciones requeridas por la AEAT.

### Alcance

La migración incluye:
- ✅ Cambio de endpoints de sandbox a producción
- ✅ Actualización de certificados digitales
- ✅ Configuración de variables de entorno
- ✅ Validaciones técnicas y funcionales
- ✅ Implementación de monitorización
- ✅ Establecimiento de procedimientos de contingencia

### Duración Estimada

- **Preparación**: 1-2 semanas
- **Ejecución de migración**: 1 día
- **Validación post-migración**: 1-3 días
- **Monitorización inicial**: 1 semana

---

## Requisitos Previos

Antes de comenzar el proceso de migración, asegúrate de cumplir con:

### Organizativos

- [ ] **Autorización formal** de la dirección para el paso a producción
- [ ] **Equipo técnico disponible** durante la migración
- [ ] **Ventana de mantenimiento** acordada (si aplica)
- [ ] **Plan de comunicación** a usuarios afectados
- [ ] **Contactos de soporte AEAT** identificados y disponibles

### Técnicos

- [ ] **Todas las pruebas en sandbox** completadas exitosamente
- [ ] **Documentación técnica** completa y actualizada
- [ ] **Certificado de producción** obtenido y validado
- [ ] **Acceso a entorno de producción** configurado
- [ ] **Backup de configuraciones actuales** realizado
- [ ] **Variables de producción** definidas y documentadas

### Legales y Normativos

- [ ] **Alta en AEAT** como sistema de facturación (si aplica)
- [ ] **Declaración responsable** del fabricante presentada (si aplica)
- [ ] **Revisión legal** de procesos completada
- [ ] **Cumplimiento RGPD** verificado
- [ ] **Normativa de facturación electrónica** revisada

---

## Checklist de Validaciones Previas

### Pruebas en Sandbox

#### Funcionales

- [ ] **Envío de registros básicos** - Al menos 20 registros exitosos
- [ ] **Encadenamiento correcto** - Verificar hash y huella anterior
- [ ] **Anulación de registros** - Probar flujo completo de anulación
- [ ] **Consulta de registros** - Validar recuperación de datos
- [ ] **Generación de QR** - Verificar creación y validez
- [ ] **Validación de QR** - Comprobar servicio de cotejo
- [ ] **Factura rectificativa** - Probar diferentes tipos
- [ ] **Diferentes tipos de operación** - Validar variedad de casos

#### Técnicas

- [ ] **Validación XML contra XSD** - 100% de éxito
- [ ] **Firma electrónica XMLDSig** - Verificar validez de firmas
- [ ] **Cálculo de huella (hash)** - Algoritmo SHA-256 correcto
- [ ] **Serialización XML** - Conforme a especificaciones AEAT
- [ ] **TLS mutuo (mTLS)** - Autenticación con certificado cliente
- [ ] **Manejo de errores SOAP** - Códigos de error correctamente procesados
- [ ] **Timeouts y reintentos** - Políticas funcionando correctamente
- [ ] **Codificación de caracteres** - UTF-8 correctamente implementado

#### Rendimiento

- [ ] **Tiempo de respuesta** - Menos de 5 segundos promedio
- [ ] **Manejo de volumen** - Probado con carga esperada
- [ ] **Uso de memoria** - Sin fugas detectadas
- [ ] **Conexiones concurrentes** - Probado límite de conexiones

#### Seguridad

- [ ] **Certificados validados** - No caducados, no revocados
- [ ] **Contraseñas seguras** - No hardcodeadas en código
- [ ] **Permisos de archivos** - Certificados con permisos restrictivos
- [ ] **Logs sin datos sensibles** - No exponer información crítica
- [ ] **Vulnerabilidades conocidas** - Ninguna crítica sin resolver

### Documentación

- [ ] **Manual de usuario** - Actualizado y revisado
- [ ] **Documentación técnica** - Completa y validada
- [ ] **Procedimientos operativos** - Documentados y probados
- [ ] **Plan de contingencia** - Definido y comunicado
- [ ] **Runbooks de incidencias** - Preparados y accesibles

---

## Checklist de Configuración Técnica

### Archivos de Configuración

#### appsettings.Production.json

- [ ] **Crear archivo** `appsettings.Production.json`
- [ ] **Configurar logging** con nivel apropiado (Warning/Error)
- [ ] **Endpoint de producción** correctamente configurado
- [ ] **Timeout adecuado** (ej: 30 segundos)
- [ ] **Reintentos configurados** (ej: 3 intentos con backoff)
- [ ] **Desactivar características de debug**
  - [ ] EnableDetailedErrors = false
  - [ ] EnableRequestLogging = false (o con filtrado)
  - [ ] EnableResponseLogging = false (o con filtrado)
  - [ ] SaveGeneratedXml = false (o en ruta segura)
- [ ] **Validaciones activadas**
  - [ ] ValidateBeforeSend = true
  - [ ] ValidarXmlContraXsd = true

Ejemplo de estructura:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Verifactu": "Information"
    }
  },
  "Certificado": {
    "PfxPath": "",
    "PfxPassword": "",
    "Comment": "USAR SECRETS MANAGER en producción"
  },
  "Verifactu": {
    "Environment": "Production",
    "EndpointUrl": "https://www1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion",
    "WsdlUrl": "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl",
    "SoapAction": "RegFacturacionAlta",
    "Timeout": 30,
    "MaxRetries": 3,
    "RetryDelayMs": 2000,
    "HuellaAnterior": "",
    "ValidarXmlContraXsd": true,
    "XsdBasePath": "./xsd"
  },
  "Features": {
    "EnableDetailedErrors": false,
    "EnableRequestLogging": false,
    "EnableResponseLogging": false,
    "ValidateBeforeSend": true,
    "SaveGeneratedXml": false
  }
}
```

#### Variables de Entorno

- [ ] **ASPNETCORE_ENVIRONMENT** = "Production"
- [ ] **VERIFACTU_ENV** = "production"
- [ ] **Certificado**:
  - [ ] VERIFACTU_CERT_PATH (no hardcodear en appsettings)
  - [ ] VERIFACTU_CERT_PASSWORD (usar secrets manager)
- [ ] **Logging**:
  - [ ] VERIFACTU_LOG_LEVEL = "Information" o "Warning"
  - [ ] VERIFACTU_LOG_PATH configurado

#### Gestión de Secretos

- [ ] **Nunca** guardar contraseñas en archivos de configuración
- [ ] **Usar Azure Key Vault, AWS Secrets Manager o similar**
- [ ] **Variables de entorno** solo para desarrollo local
- [ ] **Certificados** almacenados en almacén seguro
- [ ] **Rotación de secretos** planificada y documentada

### Dependencias y Versiones

- [ ] **.NET Runtime** - Versión 9.0 o superior instalada
- [ ] **Librerías actualizadas** - Sin vulnerabilidades conocidas
- [ ] **Versiones de XSD** - Última versión oficial de AEAT
- [ ] **Certificados raíz** - Cadena de confianza actualizada

---

## Checklist de Certificados

### Obtención del Certificado de Producción

- [ ] **Solicitud de certificado** a autoridad certificadora reconocida
  - [ ] FNMT-RCM
  - [ ] Camerfirma
  - [ ] ACCV
  - [ ] Otra CA reconocida por AEAT

- [ ] **Tipo de certificado adecuado**:
  - [ ] Certificado de representante de persona jurídica (recomendado)
  - [ ] Certificado de persona física con poder de representación
  - [ ] Asociado al NIF correcto de la empresa

- [ ] **Características técnicas**:
  - [ ] Formato PFX/P12 con clave privada
  - [ ] Algoritmo RSA 2048 bits o superior
  - [ ] Válido (no caducado)
  - [ ] No revocado (verificar CRL/OCSP)
  - [ ] Propósito: firma digital y autenticación de cliente

### Validación del Certificado

- [ ] **Verificar datos del certificado**:
  ```bash
  openssl pkcs12 -in certificado-produccion.pfx -nokeys -info
  ```

- [ ] **Verificar fechas de validez**:
  ```bash
  openssl pkcs12 -in certificado-produccion.pfx -nokeys | openssl x509 -noout -dates
  ```
  - [ ] NotBefore anterior a fecha de migración
  - [ ] NotAfter con al menos 6 meses de vigencia

- [ ] **Verificar que contiene clave privada**:
  ```bash
  openssl pkcs12 -in certificado-produccion.pfx -nocerts -nodes
  ```

- [ ] **Verificar cadena de certificación**:
  ```bash
  openssl pkcs12 -in certificado-produccion.pfx -nokeys | openssl x509 -noout -text
  ```
  - [ ] Emisor correcto
  - [ ] Subject con NIF correcto
  - [ ] Key Usage incluye digitalSignature
  - [ ] Extended Key Usage incluye clientAuth

### Instalación y Seguridad

- [ ] **Ubicación segura del certificado**:
  - [ ] Permisos de archivo restrictivos (600 en Linux, NTFS en Windows)
  - [ ] Almacenado en ruta segura y con backup
  - [ ] No incluido en repositorio Git (.gitignore actualizado)

- [ ] **Contraseña del PFX**:
  - [ ] Contraseña fuerte (mínimo 16 caracteres)
  - [ ] Almacenada en gestor de secretos (no en archivos)
  - [ ] Conocida solo por personal autorizado

- [ ] **Backup del certificado**:
  - [ ] Copia de seguridad en ubicación segura
  - [ ] Procedimiento de recuperación documentado
  - [ ] Acceso de emergencia definido

- [ ] **Plan de renovación**:
  - [ ] Alerta 60 días antes de caducidad
  - [ ] Procedimiento de renovación documentado
  - [ ] Responsable de renovación asignado

### Pruebas con Certificado de Producción

- [ ] **Prueba de carga** del certificado en la aplicación
- [ ] **Prueba de autenticación TLS** contra endpoint de producción
- [ ] **Verificación de permisos** de la entidad asociada al certificado
- [ ] **Prueba en sandbox** con certificado de producción (si permite AEAT)

---

## Checklist de Endpoints

### Identificación de Endpoints de Producción

- [ ] **Obtener URLs oficiales** del WSDL de producción de AEAT
- [ ] **Verificar endpoints actualizados** en documentación oficial
  - URL típica: `https://www1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion`
  - Confirmar con WSDL oficial más reciente

### Diferencias entre Sandbox y Producción

| Configuración | Sandbox | Producción |
|---------------|---------|------------|
| **URL Base** | `https://prewww1.aeat.es` | `https://www1.aeat.es` |
| **WSDL** | WSDL de pruebas | WSDL de producción |
| **Validaciones** | Más permisivas | Estrictas |
| **Rate Limiting** | Permisivo | Estricto según normativa |
| **Persistencia** | Temporal (pueden borrarse) | Permanente |
| **Validez tributaria** | Ninguna | Plena |

### Actualización de Configuración

- [ ] **Actualizar EndpointUrl** en `appsettings.Production.json`
- [ ] **Actualizar WsdlUrl** si ha cambiado
- [ ] **Verificar SoapAction** - Puede ser diferente en producción
- [ ] **Ajustar timeouts** según experiencia en sandbox
- [ ] **Configurar políticas de reintento** apropiadas para producción

### Validación de Conectividad

- [ ] **Test de conectividad** al endpoint de producción:
  ```bash
  curl -v https://www1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion
  ```

- [ ] **Verificar puerto 443 abierto** en firewall
- [ ] **Verificar proxy** (si aplica) configurado correctamente
- [ ] **TLS 1.2 o superior** soportado
- [ ] **DNS resuelve correctamente** www1.aeat.es

### SoapActions Disponibles

Verificar y documentar las operaciones disponibles:

- [ ] **RegFacturacionAlta** - Alta de registro de facturación
- [ ] **RegFacturacionAnulacion** - Anulación de registro
- [ ] **ConsultaRegistros** - Consulta de registros enviados
- [ ] **ValidarQR** - Validación de código QR
- [ ] Otras operaciones según WSDL oficial

---

## Checklist de Seguridad

### Protección de Datos Sensibles

- [ ] **Secretos fuera de código fuente**:
  - [ ] Contraseñas de certificados
  - [ ] Rutas a certificados
  - [ ] Claves de API (si aplica)
  - [ ] Strings de conexión a BD

- [ ] **Gestor de secretos configurado**:
  - [ ] Azure Key Vault, AWS Secrets Manager, HashiCorp Vault, etc.
  - [ ] Permisos de acceso configurados (principio de mínimo privilegio)
  - [ ] Auditoría de accesos habilitada
  - [ ] Rotación automática de secretos configurada

- [ ] **.gitignore actualizado**:
  ```gitignore
  # Certificados
  *.pfx
  *.p12
  *.pem
  *.key
  
  # Configuración con secretos
  appsettings.Production.json
  appsettings.*.local.json
  
  # User secrets
  secrets.json
  
  # Logs con datos sensibles
  logs/
  *.log
  ```

### Permisos y Accesos

- [ ] **Principio de mínimo privilegio**:
  - [ ] Aplicación ejecuta con usuario no privilegiado
  - [ ] Acceso a certificados limitado a proceso de la aplicación
  - [ ] Archivos de configuración accesibles solo por el servicio

- [ ] **Permisos de archivos** (Linux/Unix):
  ```bash
  chmod 600 /ruta/certificado-produccion.pfx
  chmod 640 /ruta/appsettings.Production.json
  ```

- [ ] **ACLs en Windows**:
  ```powershell
  icacls "C:\Certificados\produccion.pfx" /inheritance:r
  icacls "C:\Certificados\produccion.pfx" /grant:r "NT AUTHORITY\NetworkService:R"
  ```

### Seguridad de Comunicaciones

- [ ] **TLS 1.2 o superior** obligatorio
- [ ] **Certificados raíz** actualizados en el sistema
- [ ] **Validación de certificado servidor** habilitada
- [ ] **Mutual TLS (mTLS)** configurado correctamente
- [ ] **Certificate pinning** considerado (opcional, avanzado)

### Logging Seguro

- [ ] **No registrar datos sensibles**:
  - [ ] Contraseñas
  - [ ] Contenido completo de certificados
  - [ ] Datos personales (RGPD)
  - [ ] Datos tributarios completos

- [ ] **Ofuscación de datos en logs**:
  - [ ] NIFs parcialmente ocultos (ej: "B12***678")
  - [ ] Números de factura parciales
  - [ ] Importes enmascarados en debug

- [ ] **Almacenamiento seguro de logs**:
  - [ ] Permisos restrictivos en directorio de logs
  - [ ] Rotación de logs configurada
  - [ ] Retención según normativa (mínimo 4 años para facturación)

### Auditoría

- [ ] **Registro de eventos críticos**:
  - [ ] Envíos de registros de facturación
  - [ ] Anulaciones
  - [ ] Errores de validación
  - [ ] Fallos de autenticación
  - [ ] Accesos al sistema

- [ ] **Trazabilidad completa**:
  - [ ] UUID de cada registro
  - [ ] Timestamp de operaciones
  - [ ] Usuario/sistema que ejecuta la operación
  - [ ] Resultado de la operación (éxito/fallo)

### Vulnerabilidades y Parches

- [ ] **Análisis de vulnerabilidades** ejecutado:
  ```bash
  dotnet list package --vulnerable
  ```

- [ ] **Dependencias actualizadas** a últimas versiones seguras
- [ ] **Parches de seguridad** del SO aplicados
- [ ] **Procedimiento de actualización** definido
- [ ] **Monitorización de CVEs** configurada

---

## Checklist de Monitorización y Logging

### Configuración de Logging

- [ ] **Nivel de log apropiado** para producción:
  - [ ] Default: Warning o Information
  - [ ] Microsoft: Warning
  - [ ] Verifactu: Information
  - [ ] System.Net.Http: Information (para troubleshooting de conectividad)

- [ ] **Destinos de logs configurados**:
  - [ ] Archivo en disco (con rotación)
  - [ ] Sistema centralizado (Elasticsearch, Splunk, Azure Monitor, etc.)
  - [ ] Alerta en tiempo real para errores críticos

- [ ] **Estructura de logs**:
  - [ ] Formato JSON estructurado (facilita parsing)
  - [ ] Timestamp en UTC
  - [ ] Nivel de severidad
  - [ ] Contexto (clase, método)
  - [ ] Mensaje descriptivo
  - [ ] Datos adicionales (UUID, correlationId)

Ejemplo de configuración en `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "Verifactu": "Information",
      "System.Net.Http": "Warning"
    },
    "File": {
      "Path": "/var/log/verifactu/app-.log",
      "FileSizeLimitBytes": 52428800,
      "RetainedFileCountLimit": 30,
      "RollingInterval": "Day"
    }
  }
}
```

### Métricas y KPIs

- [ ] **Métricas de negocio**:
  - [ ] Número de registros enviados por día
  - [ ] Tasa de éxito vs. fallos
  - [ ] Tiempo promedio de respuesta
  - [ ] Número de reintentos

- [ ] **Métricas técnicas**:
  - [ ] Uso de CPU
  - [ ] Uso de memoria
  - [ ] Número de conexiones activas
  - [ ] Latencia de red

- [ ] **Métricas de disponibilidad**:
  - [ ] Uptime del servicio
  - [ ] Disponibilidad de endpoint AEAT
  - [ ] Tasa de timeouts

### Alertas

- [ ] **Alertas críticas configuradas**:
  - [ ] Fallo de conexión con AEAT
  - [ ] Certificado próximo a caducar (60 días)
  - [ ] Tasa de error superior al 5%
  - [ ] Servicio caído o sin respuesta
  - [ ] Uso de disco superior al 90%

- [ ] **Canales de notificación**:
  - [ ] Email a equipo técnico
  - [ ] SMS para alertas críticas (opcional)
  - [ ] Integración con sistema de tickets
  - [ ] Slack/Teams (opcional)

- [ ] **Escalado de alertas**:
  - [ ] Nivel 1: Equipo de operaciones
  - [ ] Nivel 2: Responsable técnico (si no se resuelve en 30 min)
  - [ ] Nivel 3: Dirección TI (si no se resuelve en 2 horas)

### Dashboards

- [ ] **Dashboard de operaciones** con:
  - [ ] Estado del servicio (activo/inactivo)
  - [ ] Últimos 10 envíos (éxito/fallo)
  - [ ] Gráfica de envíos por hora/día
  - [ ] Tasa de éxito en las últimas 24h
  - [ ] Tiempo de respuesta promedio
  - [ ] Estado del certificado

- [ ] **Dashboard de auditoría** con:
  - [ ] Historial completo de registros enviados
  - [ ] Registros anulados
  - [ ] Filtros por fecha, estado, tipo
  - [ ] Exportación a CSV/Excel

### Persistencia de Datos

- [ ] **Almacenamiento de registros enviados**:
  - [ ] UUID de cada registro
  - [ ] Huella (hash) calculada
  - [ ] XML firmado (opcional, pero recomendado)
  - [ ] Acuse de recibo de AEAT
  - [ ] Timestamp de envío
  - [ ] Estado (enviado/fallido/anulado)

- [ ] **Base de datos o almacenamiento**:
  - [ ] SQL Server, PostgreSQL, MySQL, MongoDB, etc.
  - [ ] Backup automático configurado
  - [ ] Retención mínima de 4 años (normativa fiscal)
  - [ ] Índices para búsquedas rápidas

- [ ] **Estructura de tabla ejemplo**:
  ```sql
  CREATE TABLE RegistrosFacturacion (
      Id BIGINT PRIMARY KEY IDENTITY,
      UUID NVARCHAR(100) UNIQUE NOT NULL,
      Serie NVARCHAR(50),
      Numero NVARCHAR(50),
      FechaExpedicion DATETIME2,
      EmisorNif NVARCHAR(20),
      HuellaRegistro NVARCHAR(64), -- SHA-256 hex
      HuellaAnterior NVARCHAR(64),
      XmlFirmado NVARCHAR(MAX),
      RespuestaAeat NVARCHAR(MAX),
      Estado NVARCHAR(20), -- Enviado, Fallido, Anulado
      FechaEnvio DATETIME2 DEFAULT GETUTCDATE(),
      IntentosEnvio INT DEFAULT 1,
      UltimoError NVARCHAR(MAX),
      UsuarioCreacion NVARCHAR(100)
  );
  ```

---

## Checklist de Plan de Contingencia

### Escenarios de Fallo

#### Fallo de Conectividad con AEAT

- [ ] **Detección**:
  - [ ] Timeout en petición SOAP
  - [ ] Error de red (DNS, TCP, TLS)

- [ ] **Acciones**:
  - [ ] Registrar error detallado en log
  - [ ] Almacenar registro en cola de reintentos
  - [ ] Reintentar según política (ej: 3 intentos, backoff exponencial)
  - [ ] Alertar si fallan todos los reintentos
  - [ ] Procesar manualmente si persiste el fallo

- [ ] **Procedimiento documentado** y accesible

#### Certificado Caducado o Revocado

- [ ] **Detección**:
  - [ ] Error de validación de certificado
  - [ ] Error TLS handshake

- [ ] **Acciones inmediatas**:
  - [ ] Suspender envíos automáticos
  - [ ] Alertar a equipo técnico (crítico)
  - [ ] Verificar estado del certificado
  - [ ] Instalar certificado renovado
  - [ ] Reanudar servicio
  - [ ] Procesar cola pendiente

- [ ] **Contacto de autoridad certificadora** disponible

#### Validación de XML Fallida

- [ ] **Detección**:
  - [ ] Error de validación contra XSD
  - [ ] SOAP Fault de AEAT indicando XML inválido

- [ ] **Acciones**:
  - [ ] Registrar XML problemático (para análisis)
  - [ ] No reenviar automáticamente
  - [ ] Alertar para revisión manual
  - [ ] Corregir datos origen si aplica
  - [ ] Actualizar validaciones para prevenir recurrencia

#### Error de Huella o Encadenamiento

- [ ] **Detección**:
  - [ ] AEAT rechaza por huella inválida
  - [ ] Encadenamiento roto

- [ ] **Acciones**:
  - [ ] Detener envíos inmediatamente
  - [ ] Identificar último registro válido
  - [ ] Consultar AEAT para obtener última huella válida
  - [ ] Recalcular huella siguiente
  - [ ] Reanudar con huella correcta
  - [ ] Revisar logs para identificar causa raíz

#### Sistema AEAT No Disponible

- [ ] **Detección**:
  - [ ] Error 503 Service Unavailable
  - [ ] Timeout prolongado
  - [ ] Comunicado oficial de AEAT

- [ ] **Acciones**:
  - [ ] Acumular registros en cola local
  - [ ] No perder datos
  - [ ] Monitorizar disponibilidad de AEAT
  - [ ] Reanudar envíos cuando servicio esté disponible
  - [ ] Procesar cola acumulada gradualmente (respetando rate limits)

### Cola de Reintentos

- [ ] **Implementación de cola persistente**:
  - [ ] Base de datos, Redis, RabbitMQ, Azure Queue, etc.
  - [ ] Registros no enviados no se pierden

- [ ] **Política de reintentos**:
  ```csharp
  // Ejemplo de política
  - Intento 1: Inmediato
  - Intento 2: Después de 30 segundos
  - Intento 3: Después de 2 minutos
  - Intento 4: Después de 10 minutos
  - Intento 5+: Manual o después de 1 hora
  ```

- [ ] **Dead Letter Queue (DLQ)** para fallos permanentes

### Rollback

- [ ] **Procedimiento de rollback definido**:
  1. Detener servicio en producción
  2. Restaurar configuración de sandbox
  3. Verificar conectividad con sandbox
  4. Reiniciar servicio en modo sandbox
  5. Investigar causa del fallo
  6. Planificar nuevo intento de migración

- [ ] **Backup de configuración anterior** disponible
- [ ] **Scripts de rollback** preparados y probados
- [ ] **Tiempo estimado de rollback** < 15 minutos

### Contactos de Emergencia

- [ ] **Equipo técnico interno**:
  - [ ] Desarrollador principal: [Nombre, teléfono, email]
  - [ ] Administrador de sistemas: [Nombre, teléfono, email]
  - [ ] Responsable de TI: [Nombre, teléfono, email]

- [ ] **Soporte AEAT**:
  - [ ] Teléfono de soporte técnico AEAT
  - [ ] Email de soporte
  - [ ] Horario de atención

- [ ] **Proveedor de certificados**:
  - [ ] Contacto FNMT/Camerfirma/ACCV
  - [ ] Procedimiento de soporte urgente

---

## Checklist de Validaciones Post-Migración

### Primeras 24 Horas

#### Validación Inmediata (primeras 2 horas)

- [ ] **Envío de registro de prueba**:
  - [ ] Preparar factura de prueba real (no ficticia)
  - [ ] Enviar a producción
  - [ ] Verificar respuesta exitosa de AEAT
  - [ ] Confirmar que el registro aparece en consulta AEAT

- [ ] **Verificar encadenamiento**:
  - [ ] Enviar segundo registro
  - [ ] Verificar huella anterior correcta
  - [ ] Confirmar aceptación por AEAT

- [ ] **Prueba de consulta**:
  - [ ] Consultar registros enviados
  - [ ] Verificar datos devueltos correctos

- [ ] **Validación de QR**:
  - [ ] Generar código QR del registro
  - [ ] Validar contra servicio de cotejo de AEAT
  - [ ] Verificar datos en respuesta

- [ ] **Logs y monitorización**:
  - [ ] Verificar que los logs se generan correctamente
  - [ ] Comprobar que las métricas se recopilan
  - [ ] Validar que las alertas funcionan (provocar alerta de test)

#### Validación Continua (primeras 24 horas)

- [ ] **Monitorización activa**:
  - [ ] Revisar logs cada 2 horas
  - [ ] Verificar dashboard de operaciones
  - [ ] Comprobar que no hay alertas críticas

- [ ] **Envíos reales**:
  - [ ] Procesar facturas reales (volumen limitado inicial)
  - [ ] Verificar tasa de éxito 100% o > 99%
  - [ ] Documentar cualquier error encontrado

- [ ] **Rendimiento**:
  - [ ] Tiempo de respuesta < 5 segundos promedio
  - [ ] Sin timeouts
  - [ ] Uso de recursos (CPU, memoria) normal

### Primera Semana

- [ ] **Incremento gradual de volumen**:
  - Día 1: 10-20% del volumen esperado
  - Día 2-3: 50% del volumen esperado
  - Día 4-7: 100% del volumen esperado

- [ ] **Validaciones diarias**:
  - [ ] Revisar logs de errores
  - [ ] Verificar métricas de éxito
  - [ ] Comprobar encadenamiento correcto
  - [ ] Validar que todas las facturas se envían

- [ ] **Pruebas funcionales adicionales**:
  - [ ] Anulación de registro real
  - [ ] Factura rectificativa
  - [ ] Diferentes tipos de operación
  - [ ] Volumen máximo esperado

- [ ] **Backup y auditoría**:
  - [ ] Verificar que todos los registros se almacenan
  - [ ] Comprobar backups automáticos
  - [ ] Validar que se pueden recuperar datos

### Primer Mes

- [ ] **Revisión semanal**:
  - [ ] Análisis de logs y métricas
  - [ ] Identificar patrones de error
  - [ ] Optimizar configuración si es necesario

- [ ] **Validación con AEAT** (si aplica):
  - [ ] Confirmar que todos los registros están en AEAT
  - [ ] Verificar integridad de datos
  - [ ] Resolver discrepancias si existen

- [ ] **Revisión de capacidad**:
  - [ ] Evaluar si los recursos son suficientes
  - [ ] Planificar escalado si es necesario

- [ ] **Actualización de documentación**:
  - [ ] Documentar lecciones aprendidas
  - [ ] Actualizar procedimientos operativos
  - [ ] Mejorar runbooks basándose en experiencia

---

## Diferencias Clave entre Entornos

### Tabla Comparativa Detallada

| Aspecto | Sandbox (Pruebas) | Producción |
|---------|-------------------|------------|
| **URL Base** | `https://prewww1.aeat.es` | `https://www1.aeat.es` |
| **Endpoint SOAP** | `/wlpl/TIKE-CONT/SistemaFacturacion` | `/wlpl/TIKE-CONT/SistemaFacturacion` |
| **WSDL** | WSDL de pruebas (preproducción) | WSDL de producción oficial |
| **Certificados** | Cualquier certificado válido | Solo certificados de representante activos |
| **Validación NIF** | Puede ser permisiva | Estricta (NIF debe existir y ser válido) |
| **Validaciones XML** | Pueden ser más permisivas | Totalmente estrictas según XSD |
| **Persistencia datos** | Temporal (pueden eliminarse sin aviso) | Permanente (validez tributaria) |
| **Validez tributaria** | Ninguna | Plena (obligaciones fiscales reales) |
| **Rate Limiting** | Más permisivo (para facilitar pruebas) | Estricto según normativa |
| **Horario servicio** | 24/7 (con posibles mantenimientos) | 24/7 (alta disponibilidad) |
| **Soporte** | Documentación y FAQs | Soporte oficial AEAT |
| **Logs AEAT** | Pueden no conservarse indefinidamente | Conservados según normativa |
| **Anulaciones** | Posibles sin restricciones | Según normativa fiscal |
| **Datos de prueba** | Permitidos y recomendados | Solo datos reales |

### Consideraciones Importantes

#### En Sandbox NO tienes:

- ❌ Validez tributaria de los registros
- ❌ Garantía de persistencia de datos
- ❌ Validaciones completas de negocio (puede ser permisivo)
- ❌ Soporte oficial AEAT para incidencias
- ❌ SLA de disponibilidad

#### En Producción SÍ tienes:

- ✅ Validez tributaria plena
- ✅ Obligaciones fiscales reales
- ✅ Persistencia garantizada de datos
- ✅ Validaciones completas y estrictas
- ✅ Soporte oficial AEAT
- ✅ SLA de alta disponibilidad
- ✅ Responsabilidad legal de los datos

---

## Procedimiento de Migración Paso a Paso

### Fase 1: Preparación (1-2 semanas antes)

#### Semana -2

- [ ] **Día 1-2**: Revisar este checklist completo
- [ ] **Día 3-4**: Completar todas las pruebas en sandbox
- [ ] **Día 5**: Solicitar certificado de producción (si no se tiene)
- [ ] **Día 6-7**: Preparar documentación y procedimientos

#### Semana -1

- [ ] **Día 8**: Crear `appsettings.Production.json`
- [ ] **Día 9**: Configurar gestor de secretos (Key Vault, etc.)
- [ ] **Día 10**: Configurar monitorización y alertas
- [ ] **Día 11**: Configurar logging de producción
- [ ] **Día 12**: Preparar base de datos de producción
- [ ] **Día 13**: Ejecutar pruebas de integración completas
- [ ] **Día 14**: Reunión de validación con equipo

### Fase 2: Validación Final (día anterior)

- [ ] **09:00** - Verificar certificado de producción instalado
- [ ] **09:30** - Test de conectividad con endpoint de producción
- [ ] **10:00** - Validar configuración completa
- [ ] **11:00** - Backup de configuración actual
- [ ] **12:00** - Reunión de go/no-go con stakeholders
- [ ] **14:00** - Comunicar a usuarios afectados
- [ ] **15:00** - Preparar entorno de producción
- [ ] **16:00** - Validación final de sandbox
- [ ] **17:00** - Confirmar disponibilidad de equipo para día siguiente

### Fase 3: Migración (Día D)

#### Horario Recomendado: Comenzar a primera hora (ej: 08:00)

**08:00 - 08:30** - Preparación inicial

- [ ] Conectar con equipo técnico
- [ ] Verificar que no hay mantenimientos programados en AEAT
- [ ] Hacer backup final de configuración sandbox
- [ ] Verificar certificado de producción válido

**08:30 - 09:00** - Configuración

- [ ] Establecer `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Cargar secretos de producción desde gestor
- [ ] Activar configuración de producción
- [ ] Verificar variables de entorno

**09:00 - 09:30** - Validación técnica

- [ ] Build de la aplicación
- [ ] Test de carga de certificado
- [ ] Test de conectividad con AEAT producción
- [ ] Verificar logs se generan correctamente

**09:30 - 10:00** - Primer envío

- [ ] Preparar factura de prueba real (no ficticia)
- [ ] Enviar primer registro a producción
- [ ] Verificar respuesta exitosa
- [ ] Consultar registro en AEAT
- [ ] Validar QR generado

**10:00 - 10:30** - Validación de encadenamiento

- [ ] Enviar segundo registro
- [ ] Verificar encadenamiento correcto
- [ ] Confirmar aceptación por AEAT

**10:30 - 12:00** - Procesamiento gradual

- [ ] Procesar lote pequeño de facturas reales (10-20)
- [ ] Monitorizar cada envío
- [ ] Verificar tasa de éxito 100%
- [ ] Resolver cualquier incidencia inmediatamente

**12:00 - 13:00** - Almuerzo y monitorización

- [ ] Mantener monitorización activa
- [ ] Equipo disponible para incidencias

**13:00 - 17:00** - Incremento de volumen

- [ ] Incrementar gradualmente el volumen
- [ ] Monitorizar continuamente
- [ ] Documentar cualquier anomalía
- [ ] Verificar rendimiento

**17:00 - 18:00** - Validación de día completo

- [ ] Revisar logs completos del día
- [ ] Verificar todas las métricas
- [ ] Confirmar todos los registros enviados
- [ ] Reunión de status con stakeholders

**18:00** - Cierre del día

- [ ] Documentar lecciones del día
- [ ] Planificar monitorización para los siguientes días
- [ ] Confirmar disponibilidad de equipo on-call

### Fase 4: Estabilización (días 2-7)

**Días 2-3**: Monitorización intensiva

- [ ] Revisar logs cada 2 horas
- [ ] Incrementar volumen al 50%
- [ ] Resolver incidencias rápidamente
- [ ] Documentar problemas y soluciones

**Días 4-7**: Operación normal

- [ ] Volumen al 100%
- [ ] Monitorización normal (horario laboral)
- [ ] On-call configurado para fuera de horario
- [ ] Reunión diaria de status

### Fase 5: Revisión (semana 2)

- [ ] Reunión de retrospectiva
- [ ] Análisis de métricas de la semana
- [ ] Identificar mejoras
- [ ] Actualizar documentación
- [ ] Optimizar configuración basándose en experiencia

---

## Rollback y Recuperación

### Cuándo Hacer Rollback

Considera hacer rollback si:

- ❌ Tasa de error superior al 10% de forma sostenida
- ❌ Imposibilidad de conectar con AEAT por más de 2 horas
- ❌ Problemas críticos de certificado sin solución inmediata
- ❌ Errores de encadenamiento que no se pueden resolver
- ❌ Pérdida de datos o inconsistencias graves
- ❌ Decisión de stakeholders por motivos de negocio

### Procedimiento de Rollback

**Tiempo estimado**: 15-30 minutos

1. **Detener servicio de producción** (inmediato):
   ```bash
   # Linux/macOS
   sudo systemctl stop verifactu-service
   
   # Windows
   Stop-Service -Name "VerifactuService"
   ```

2. **Restaurar configuración de sandbox** (5 minutos):
   ```bash
   # Cambiar variable de entorno
   export ASPNETCORE_ENVIRONMENT=Sandbox
   
   # O restaurar appsettings anterior
   cp appsettings.Sandbox.json.backup appsettings.json
   ```

3. **Verificar conectividad con sandbox** (5 minutos):
   ```bash
   curl -v https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion
   ```

4. **Reiniciar servicio en modo sandbox** (5 minutos):
   ```bash
   # Linux/macOS
   sudo systemctl start verifactu-service
   
   # Windows
   Start-Service -Name "VerifactuService"
   ```

5. **Validar funcionamiento** (10 minutos):
   - Enviar registro de prueba a sandbox
   - Verificar respuesta exitosa
   - Confirmar que el servicio está operativo

6. **Comunicar rollback** (inmediato):
   - Notificar a stakeholders
   - Informar a usuarios afectados
   - Documentar razón del rollback

### Post-Rollback

- [ ] **Análisis de causa raíz**:
  - [ ] Revisar logs completos
  - [ ] Identificar punto de fallo exacto
  - [ ] Documentar problema detalladamente

- [ ] **Plan de acción correctiva**:
  - [ ] Definir pasos para resolver el problema
  - [ ] Asignar responsables
  - [ ] Establecer plazos

- [ ] **Nuevo intento de migración**:
  - [ ] Planificar nueva fecha (mínimo 1 semana después)
  - [ ] Incorporar lecciones aprendidas
  - [ ] Reforzar plan de pruebas
  - [ ] Validar solución en sandbox antes de reintentar

### Recuperación de Datos

Si hubo pérdida de datos o inconsistencias:

1. **Identificar registros afectados**:
   ```sql
   SELECT * FROM RegistrosFacturacion 
   WHERE FechaEnvio BETWEEN '2025-01-15 08:00:00' AND '2025-01-15 18:00:00'
   AND Estado = 'Fallido';
   ```

2. **Consultar AEAT** para verificar qué se recibió realmente

3. **Reenviar registros perdidos** con huella correcta

4. **Reconciliación**:
   - Comparar base de datos local con registros en AEAT
   - Identificar discrepancias
   - Resolver una por una

---

## Anexos y Referencias

### A. Checklist Resumido (Quick Reference)

Antes de migrar, asegúrate de haber completado:

**Validaciones Previas**:
- ✅ Todas las pruebas en sandbox exitosas
- ✅ Certificado de producción obtenido y validado
- ✅ Documentación completa

**Configuración Técnica**:
- ✅ appsettings.Production.json creado
- ✅ Variables de entorno configuradas
- ✅ Secretos en gestor seguro

**Endpoints y Certificados**:
- ✅ URLs de producción configuradas
- ✅ Certificado instalado y con permisos correctos
- ✅ Conectividad verificada

**Seguridad**:
- ✅ Datos sensibles protegidos
- ✅ Permisos configurados
- ✅ Logging seguro

**Monitorización**:
- ✅ Logs configurados
- ✅ Alertas configuradas
- ✅ Dashboard operativo

**Plan de Contingencia**:
- ✅ Procedimientos de rollback preparados
- ✅ Contactos de emergencia disponibles
- ✅ Cola de reintentos implementada

### B. Comandos Útiles

#### Verificación de Certificado

```bash
# Ver información del certificado
openssl pkcs12 -in certificado-produccion.pfx -nokeys -info

# Verificar fechas
openssl pkcs12 -in certificado-produccion.pfx -nokeys | openssl x509 -noout -dates

# Verificar subject y emisor
openssl pkcs12 -in certificado-produccion.pfx -nokeys | openssl x509 -noout -subject -issuer

# Verificar que contiene clave privada
openssl pkcs12 -in certificado-produccion.pfx -nocerts -nodes | head -20
```

#### Verificación de Conectividad

```bash
# Test de conectividad básico
curl -v https://www1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion

# Verificar DNS
nslookup www1.aeat.es

# Verificar TLS
openssl s_client -connect www1.aeat.es:443 -servername www1.aeat.es

# Test con certificado cliente
curl -v --cert certificado.pem --key clave.key https://www1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion
```

#### Gestión de Servicio

```bash
# Linux systemd
sudo systemctl status verifactu-service
sudo systemctl start verifactu-service
sudo systemctl stop verifactu-service
sudo systemctl restart verifactu-service

# Windows
Get-Service -Name "VerifactuService"
Start-Service -Name "VerifactuService"
Stop-Service -Name "VerifactuService"
Restart-Service -Name "VerifactuService"
```

#### Logs

```bash
# Ver logs en tiempo real
tail -f /var/log/verifactu/app.log

# Buscar errores
grep "ERROR" /var/log/verifactu/app.log

# Últimos 100 errores
grep "ERROR" /var/log/verifactu/app.log | tail -100

# Logs de hoy
grep "2025-01-15" /var/log/verifactu/app.log
```

### C. Plantilla de Comunicación

#### Email de Notificación de Migración

```
Asunto: Migración VerifactuSender a Producción - [Fecha]

Estimados,

Informamos que se procederá a la migración del sistema VerifactuSender del entorno de pruebas al entorno de producción de VERI*FACTU de la AEAT.

Fecha: [Fecha de migración]
Hora: [Hora de inicio] - [Hora estimada de fin]
Responsable: [Nombre del responsable]

Durante la migración:
- El servicio estará disponible de forma limitada
- Se procesarán facturas de prueba inicialmente
- El volumen se incrementará gradualmente

Hemos completado todas las pruebas en el entorno sandbox con resultados exitosos.

El equipo técnico estará disponible durante toda la migración para resolver cualquier incidencia.

En caso de preguntas o problemas, contactar a:
- [Nombre]: [Email] / [Teléfono]

Saludos,
[Equipo técnico]
```

### D. Enlaces y Referencias

#### Documentación AEAT Oficial

- [Sede electrónica VERI*FACTU](https://sede.agenciatributaria.gob.es/Sede/iva/sistemas-informaticos-facturacion-verifactu.html)
- [Portal de Pruebas Externas](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/portal-pruebas-externas.html)
- [Información Técnica](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/informacion-tecnica.html)
- [Preguntas Frecuentes](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/preguntas-frecuentes.html)
- [WSDL de Servicios](https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl)

#### Documentación del Proyecto

- [README principal](../README.md)
- [Guía de Instalación](instalacion.md)
- [Entorno de Pruebas (Sandbox)](entorno-pruebas.md)
- [Guía Técnica VERI*FACTU](Verifactu-Guia-Tecnica.md)
- [Protocolos de Comunicación](protocolos-comunicacion.md)
- [Arquitectura](arquitectura.md)

#### Normativa

- [Real Decreto 1007/2023](https://www.boe.es/buscar/act.php?id=BOE-A-2023-24873) - Reglamento SIF

#### Autoridades de Certificación

- [FNMT-RCM](https://www.sede.fnmt.gob.es/certificados)
- [Camerfirma](https://www.camerfirma.com/)
- [ACCV - Generalitat Valenciana](https://www.accv.es/)

### E. Glosario

- **AEAT**: Agencia Estatal de Administración Tributaria
- **mTLS**: Mutual TLS - Autenticación mutua con certificados cliente y servidor
- **PFX/P12**: Formato de certificado que incluye clave privada
- **QR**: Código QR de factura para cotejo
- **Sandbox**: Entorno de pruebas sin validez tributaria
- **SIF**: Sistema Informático de Facturación
- **SOAP**: Simple Object Access Protocol - Protocolo de servicios web
- **TLS**: Transport Layer Security - Protocolo de seguridad
- **UUID**: Identificador único universal de registro
- **VERI*FACTU**: Sistema de verificación de facturación de la AEAT
- **WSDL**: Web Services Description Language
- **XSD**: XML Schema Definition - Esquema de validación XML
- **XMLDSig**: XML Digital Signature - Firma digital XML

---

## Historial de Cambios

| Versión | Fecha | Autor | Cambios |
|---------|-------|-------|---------|
| 1.0 | 2025-10-30 | Equipo VerifactuSender | Versión inicial del checklist |

---

## Aprobaciones

| Rol | Nombre | Firma | Fecha |
|-----|--------|-------|-------|
| Responsable Técnico | | | |
| Responsable de Seguridad | | | |
| Director de TI | | | |

---

**Última actualización**: 30 de octubre de 2025  
**Versión del documento**: 1.0  
**Próxima revisión**: Después de la primera migración a producción

---

## Notas Finales

Este checklist es una guía exhaustiva pero debe adaptarse a las necesidades específicas de tu organización. Algunas secciones pueden no aplicar o pueden requerir pasos adicionales según tu infraestructura y requisitos.

**Recomendaciones finales**:

1. ✅ **No te apresures**: Es mejor retrasar la migración unos días que hacerla con prisas
2. ✅ **Documenta todo**: Cada paso, cada decisión, cada problema encontrado
3. ✅ **Comunica proactivamente**: Mantén informados a todos los stakeholders
4. ✅ **Ten un plan B**: El rollback no es un fracaso, es una medida de seguridad
5. ✅ **Aprende de la experiencia**: Cada migración mejora tus procesos

**En caso de duda**:
- Consulta la documentación oficial de AEAT
- Revisa este checklist completo
- Contacta con el equipo técnico
- No arriesgues si no estás seguro

¡Éxito en tu migración a producción! 🚀
