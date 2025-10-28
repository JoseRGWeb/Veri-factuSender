## Próximos pasos recomendados

Este archivo enumera los pasos recomendados para llevar la plantilla VerifactuSender desde un prototipo/ejemplo a una solución lista para pruebas e integración con los servicios VERI\*FACTU (AEAT).

Cada ítem incluye una breve descripción, prioridad recomendada y criterios de aceptación mínimos.

---

### 1) Implementar serialización XML conforme a los XSD oficiales

-   Descripción: Reemplazar `VerifactuSerializer` por una implementación que genere XML exactamente conforme a los XSD que provea AEAT (namespaces, atributos, orden de elementos, tipos y validación).
-   Prioridad: Alta
-   Criterios de aceptación:
    -   Validación automática del XML generado contra los XSD de AEAT (usando `XmlSchemaSet` o herramientas CI).
    -   Ejemplos de XML de prueba que coincidan con los escenarios (facturas con varias líneas, exenciones, etc.).

### 2) Implementar algoritmo de huella/encadenado oficial

-   Descripción: Sustituir el `HashService` placeholder por la implementación exacta del algoritmo de huella/encadenado que requiere AEAT, incluyendo normalización de textos y tratamiento de decimales/fechas.
-   Prioridad: Alta
-   Criterios de aceptación:
    -   Tests unitarios que verifiquen huellas conocidas (vector de prueba proporcionado por AEAT o generado por referencia).
    -   Persistencia e importación de la `HuellaAnterior` para mantener el encadenado entre envíos.

### 3) Revisar y ajustar la firma XML (XMLDSig)

-   Descripción: Confirmar transformaciones y canonicalización exigidas por AEAT (por ejemplo C14N), referencias incluidas y elementos que deben protegerse. Asegurar que la firma sea compatible con validadores de AEAT.
-   Prioridad: Alta
-   Criterios de aceptación:
    -   Pruebas de interoperabilidad con un validador/sandbox de AEAT (o WSDL/XSD de pruebas).
    -   Tests que verifiquen que la firma se valida externamente.

### 4) Integración con WSDL/SOAP real y ajuste de sobre SOAP

-   Descripción: Adaptar `VerifactuSoapClient` al WSDL real: nombres de operaciones, namespaces, encabezados SOAP (headers) y `SOAPAction` si aplica. Manejar attachments si el servicio lo requiere.
-   Prioridad: Alta
-   Criterios de aceptación:
    -   Cliente capaz de consumir WSDL (generación opcional de proxy con `Connected Services` o `dotnet-svcutil`).
    -   Test de integración que envía a endpoint de pruebas (sandbox) y procesa respuesta.

### 5) Manejo de errores, reintentos y idempotencia

-   Descripción: Implementar políticas de reintento (exponencial con jitter), timeouts, circuit-breaker para llamadas SOAP y lógica para detectar duplicados/ idempotencia (reintentos seguros).
-   Prioridad: Alta
-   Criterios de aceptación:
    -   Reintentos visibles en logs y controlados via configuración.
    -   Pruebas que simulan errores transitorios y verifican reintento correcto.

### 6) Seguridad: manejo de certificados y secretos

-   Descripción: No usar PFX con contraseña en texto plano. Preferir cargar certificados desde el almacén del sistema o usar APIs recomendadas de .NET para carga segura (evitar el constructor obsoleto). Gestionar secrets con `dotnet user-secrets` en desarrollo y un secret manager (Azure Key Vault, AWS Secrets Manager, etc.) en producción.
-   Prioridad: Alta
-   Criterios de aceptación:
    -   Documentación y ejemplos para cargar certificados desde almacén de Windows/Linux.
    -   No hay secrets en repo; verificado por escaneo rápido antes de PR.

### 7) Validación y pruebas automáticas (unit + integración)

-   Descripción: Añadir tests unitarios adicionales y tests de integración que cubran: serialización vs XSD, cálculo de huella, firma XML y envío/recepción SOAP (mock o sandbox).
-   Prioridad: Alta
-   Criterios de aceptación:
    -   Cobertura básica para servicios críticos (huella, serializer, signer, soap client).
    -   Pruebas de integración opcionales que se ejecuten en CI solo con secrets/config seguros.

### 8) CI/CD: build, tests y checks de seguridad

-   Descripción: Configurar pipeline (GitHub Actions o similar) que haga: build, restore, tests, validación XML contra XSD y escaneo de secrets/credenciales accidentales.
-   Prioridad: Media
-   Criterios de aceptación:
    -   Workflow que falla si los tests unitarios fallan o si se detectan secrets en el commit.

### 9) Documentación técnica y documentación de despliegue

-   Descripción: Completar `README.md` con ejemplos reales, y añadir `DEPLOYMENT.md` o `OPERATIONS.md` con pasos para desplegar, permisos de PFX, backups y recovery.
-   Prioridad: Media
-   Criterios de aceptación:
    -   Documentación con pasos reproducibles para: obtener certificado, configurar appsettings/variables, ejecutar demo y pruebas.

### 10) Logging, métricas y monitorización

-   Descripción: Integrar logging estructurado (Microsoft.Extensions.Logging) con niveles configurables, y exponer métricas básicas (envíos, errores, latencia) para observabilidad.
-   Prioridad: Media
-   Criterios de aceptación:
    -   Logs suficientes para depurar fallos de firma/envío.
    -   Métricas básicas exportables a Prometheus/Aplicación de APM (opcional).

### 11) Gestión de estados y persistencia

-   Descripción: Definir cómo persistir: registros enviados, huellas, estados (pendiente, enviado, fallido), y diseñar modelos para reintentos o conciliación.
-   Prioridad: Media
-   Criterios de aceptación:
    -   Esquema propuesto (ej: tabla simple SQL o documento) y un repositorio de ejemplo para almacenar huella anterior y estado.

### 12) Pruebas de interoperabilidad y certificado legal

-   Descripción: Realizar pruebas contra el entorno de pruebas de AEAT; confirmar requisitos legales y de certificado (p. ej. representante, roles autorizados).
-   Prioridad: Alta
-   Criterios de aceptación:
    -   Validación por el entorno de pruebas/sandbox de AEAT con al menos un envío exitoso.

### 13) Checklist de puesta en producción

-   Descripción: Crear una checklist que incluya: XSD/WSDL aprobados, certificados correctos, backups del PFX (si aplica), permisos de acceso, monitorización, plan de rollback, pruebas finales y notificación a stakeholders.
-   Prioridad: Alta
-   Criterios de aceptación:
    -   Checklist revisada y aprobada antes del despliegue.

### 14) Mejoras opcionales

-   Generación automática de proxies SOAP desde WSDL (si se desea), o migración a cliente gRPC/REST si AEAT ofrece alternativas.
-   Soporte para múltiples emisores (multi-tenant) si la arquitectura lo requiere.

---

Notas finales

-   Para cada tarea de prioridad Alta, crea una rama/PR independiente con su propio conjunto de tests y documentación.
-   Puedo ayudarte a implementar cualquiera de los puntos anteriores; dime cuál quieres que haga a continuación y comienzo con la implementación, tests y CI necesarios.

Fecha de generación: 28 de octubre de 2025
