<!-- Copilot / AI agent instructions specific to the Veri-factuSender repository -->

# Instrucciones rápidas para agentes de IA (Copilot)

Objetivo corto: ayudar a un agente a entender rápidamente la arquitectura, los flujos críticos y las convenciones del repositorio para que pueda hacer cambios útiles y seguros.

-   Contexto del proyecto

    -   Repositorio: plantilla/prototipo en .NET 9 para construir, firmar y enviar registros de facturación al servicio VERI\*FACTU (AEAT).
    -   Componentes principales:
        -   `src/Verifactu.Client/` — biblioteca con modelos (`Models/`), servicios (`Services/`) y cliente SOAP (`Soap/`).
        -   `src/Verifactu.ConsoleDemo/` — demo de consola que muestra un flujo end-to-end usando `appsettings.json` y `factura-demo.json`.
        -   `tests/Verifactu.Client.Tests/` — tests unitarios de ejemplo (p. ej. `HashServiceTests`).

-   Datos y flujo esencial (ejemplo extraído de `Program.cs`):
    1. Cargar configuración (`appsettings.json` / variables de entorno).

2.  Cargar certificado PFX con `CertificateLoader.CargarDesdePfx`.
3.  Leer `factura-demo.json` y crear `Factura`.
4.  Crear `RegistroFacturacion`, calcular huella con `HashService.CalcularHuella`.
5.  Serializar con `VerifactuSerializer.CrearXmlRegistro` (placeholder).
6.  Firmar con `XmlSignerService.Firmar` (XMLDSig enveloped).
7.  Enviar vía `VerifactuSoapClient.EnviarRegistroAsync` (mTLS + SOAP 1.1).

-   Puntos críticos y advertencias (no cambiar sin pruebas):

    -   `VerifactuSerializer` y `HashService` son implementaciones placeholder. Cambios en XML o huella deben validarse contra XSD/WSDL y las guías AEAT (`docs/Verifactu-Guia-Tecnica.md`).
    -   `CertificateLoader.CargarDesdePfx` usa un constructor obsoleto de `X509Certificate2`. Si migras, usa `X509CertificateLoader.LoadPkcs12()` en .NET 9.
    -   `VerifactuSoapClient.ConstruirSobreSoap` genera un sobre SOAP simplificado; en producción debe alinearse con el WSDL.
    -   Nunca añadir PFX o contraseñas al repositorio; usa `dotnet user-secrets` o variables de entorno.

-   Convenciones del código y patrones observables

    -   API pública definida por interfaces en `Services/Interfaces.cs`. Implementaciones concretas siguen nombres `*Service`.
    -   Uso de `record` para modelos inmutables (`Factura`, `RegistroFacturacion`). Mantén compatibilidad con `record with { }` al modificar propiedades.
    -   Serialización y firma trabajan sobre `System.Xml.XmlDocument` (no usar sólo objetos POCO->JSON).
    -   Dependencias externas: cliente HTTP estándar (`HttpClient`), clases de criptografía y XMLDSig (`System.Security.Cryptography.Xml`).

-   Workflows útiles para desarrolladores (comandos reproducibles)

    -   Build: desde la raíz del repo `dotnet build` o desde la carpeta de la demo `cd src/Verifactu.ConsoleDemo; dotnet run`.
    -   Tests: `dotnet test` en la raíz o `dotnet test tests/Verifactu.Client.Tests`.
    -   Ejecutar demo (PowerShell):
        -   `cd C:\DesarrolloGit\Veri-factuSender\src\Verifactu.ConsoleDemo; dotnet run`

-   Examples concretos a mencionar cuando propongas cambios

    -   Si cambias la huella: actualizar `tests/Verifactu.Client.Tests/HashServiceTests.cs` o añadir nuevos tests que validen la huella contra casos de ejemplo.
    -   Si cambias la serialización XML: añadir validación contra XSD (nuevo test que cargue XSD de `docs/` y valide `XmlDocument`).
    -   Si tocas `CertificateLoader`: asegúrate de documentar la migración a `X509CertificateLoader.LoadPkcs12()` y actualizar los comentarios/remarks.

-   Reglas prácticas al editar

    -   Evita tocar la lógica del cliente SOAP sin un WSDL de referencia y tests; documenta cualquier cambio en `docs/protocolos-comunicacion.md`.
    -   Añade tests unitarios para cualquier cambio en `HashService`, `VerifactuSerializer` o `XmlSignerService`.
    -   Mantén separadas las implementaciones placeholder y las reales: si introduces una implementación que cumple XSD/WSDL, preserva una opción de compatibilidad (ej. implementación alternativa que se seleccione por configuración).

-   Archivos clave a revisar primero
    -   `src/Verifactu.Client/Services/Interfaces.cs`
    -   `src/Verifactu.Client/Services/HashService.cs`
    -   `src/Verifactu.Client/Services/VerifactuSerializer.cs`
    -   `src/Verifactu.Client/Services/XmlSignerService.cs`
    -   `src/Verifactu.Client/Soap/VerifactuSoapClient.cs`
    -   `src/Verifactu.ConsoleDemo/Program.cs` y `appsettings.json`
    -   `docs/Verifactu-Guia-Tecnica.md` y `docs/protocolos-comunicacion.md`

Si una sección queda poco clara o deseas que incluya ejemplos más detallados (por ejemplo, cómo validar XML contra XSD o cómo ejecutar un sandbox AEAT), dime qué prefieres y lo adapto.

---

Fecha: 2025-11-07
