# VerifactuSender (.NET 9)

## Descripción

VerifactuSender es una plantilla/prototipo en .NET 9 para construir, firmar y enviar registros de facturación al servicio VERI\*FACTU (AEAT). Incluye:

-   Biblioteca `Verifactu.Client` con modelos de factura, servicios para cálculo de huella (hash), serialización a XML, firma XML (XMLDSig) y cliente SOAP con soporte para TLS mutuo.
-   Aplicación de consola `Verifactu.ConsoleDemo` que muestra un flujo de ejemplo: leer JSON de factura, construir un registro, calcular huella, serializar, firmar y enviar por SOAP (placeholder).
-   Tests unitarios mínimos (ej.: `HashServiceTests`) como punto de partida.

⚠️ **IMPORTANTE**: El proyecto contiene implementaciones "placeholder" para la serialización XML, el algoritmo de huella y el endpoint SOAP. Antes de usarlo en producción debes ajustar la estructura al XSD/WSDL oficiales de la AEAT y verificar las políticas de firma y comunicación.

## 📚 Documentación Completa

Este proyecto incluye documentación exhaustiva en el directorio [`docs/`](docs/):

- **[Índice de Documentación](docs/README.md)** - Punto de entrada a toda la documentación
- **[Guía de Instalación](docs/instalacion.md)** - Configuración paso a paso del proyecto
- **[Guía de Uso](docs/uso.md)** - Ejemplos prácticos y casos de uso
- **[Arquitectura](docs/arquitectura.md)** - Estructura y componentes del sistema
- **[Guía de Desarrollo](docs/desarrollo.md)** - Para contribuidores y desarrolladores
- **[Guía Técnica VERI\*FACTU](docs/Verifactu-Guia-Tecnica.md)** - Integración con servicios AEAT
- **[Roadmap](docs/roadmap.md)** - Próximos pasos y mejoras planificadas

### 🚀 Inicio Rápido

1. **Nuevos usuarios**: Comienza con la [Guía de Instalación](docs/instalacion.md)
2. **Ver ejemplos**: Consulta la [Guía de Uso](docs/uso.md)
3. **Entender el código**: Lee la [Arquitectura](docs/arquitectura.md)
4. **Contribuir**: Revisa la [Guía de Desarrollo](docs/desarrollo.md)

## Estado

-   Plantilla / ejemplo: requiere adaptación a especificaciones AEAT (XSD/WSDL, reglas de huella, endpoints).

## Requisitos

-   .NET 9 SDK (SDK compatible con los proyectos en `src/`)
-   Certificado de representante en formato PFX (con contraseña) o certificado en almacén del sistema
-   Endpoint (WSDL/URL) de VERI\*FACTU para pruebas/producción
-   Acceso seguro a secretos (no dejar contraseñas en appsettings.json en producción)

## Estructura principal del repositorio

-   `src/Verifactu.Client/`
    -   `Models/` — Modelos como `Factura`, `RegistroFacturacion`, `Emisor`, `Receptor`.
    -   `Services/` — Implementaciones y contratos: `HashService`, `VerifactuSerializer`, `XmlSignerService`, `CertificateLoader`, `Interfaces.cs`.
    -   `Soap/VerifactuSoapClient.cs` — Cliente SOAP que envuelve el XML en un sobre SOAP y lo envía usando `HttpClient` con certificado cliente para TLS mutuo.
-   `src/Verifactu.ConsoleDemo/` — Demo de consola con `appsettings.json` y `factura-demo.json`.
-   `tests/Verifactu.Client.Tests/` — Tests unitarios (ej.: `HashServiceTests.cs`).

## Configuración (ejemplo)

La demo usa `appsettings.json` para los datos sensibles y de configuración. Ejemplo de claves relevantes:

-   `Certificado:PfxPath` — ruta al fichero `.pfx` con la clave privada.
-   `Certificado:PfxPassword` — contraseña del PFX.
-   `Verifactu:EndpointUrl` — URL del servicio SOAP (WSDL/endpoint).
-   `Verifactu:SoapAction` — acción SOAP si aplica.
-   `Verifactu:HuellaAnterior` — (opcional) huella anterior para el encadenado.

Ejemplo mínimo (el archivo `src/Verifactu.ConsoleDemo/appsettings.json` contiene un ejemplo):

```json
{
    "Certificado": {
        "PfxPath": "C:/path/a/tu-certificado.pfx",
        "PfxPassword": "CAMBIA_ESTA_PASSWORD"
    },
    "Verifactu": {
        "EndpointUrl": "https://example.com/verifactu/ws",
        "SoapAction": "urn:EnviarRegistroFacturacion",
        "HuellaAnterior": ""
    }
}
```

Notas de seguridad: usa `dotnet user-secrets` o variables de entorno para no guardar contraseñas en texto claro. Protege el fichero PFX y limita permisos de lectura.

## Uso rápido (demo)

Desde PowerShell en Windows (ruta del repo):

```powershell
cd C:\DesarrolloGit\Veri-factuSender\src\Verifactu.ConsoleDemo
dotnet run
```

La consola hace lo siguiente (flujo de ejemplo):

1. Carga configuración de `appsettings.json` / variables de entorno.
2. Carga el certificado PFX usando `CertificateLoader`.
3. Lee `factura-demo.json` y crea un `Factura`.
4. Construye un `RegistroFacturacion` y calcula la huella encadenada con `HashService` (implementación de ejemplo).
5. Serializa el registro a XML con `VerifactuSerializer` (placeholder).
6. Firma el XML con `XmlSignerService` (XMLDSig enveloped).
7. Envía el XML firmado con `VerifactuSoapClient` mediante una petición SOAP (sobre/Envelope simplificado).

## Qué hay que adaptar antes de producción

-   **Serialización a XML**: `VerifactuSerializer` actualmente genera un XML placeholder. Reemplaza por una serialización que cumpla exactamente los XSD exigidos por AEAT.
-   **Algoritmo de huella**: `HashService` emplea un ejemplo con SHA-256. Sustituye por la lógica oficial y las reglas de encadenado.
-   **Firma XML**: Revisa parámetros de canonicalización (C14N), transformaciones y políticas indicadas por AEAT.
-   **SOAP/WSDL**: `VerifactuSoapClient` necesita la URL/WSDL reales, nombres de operaciones, y la correcta cabecera/soapAction.
-   **Manejo de errores y reintentos**: Implementa políticas de reintento, logging y persistencia de huellas/registros según tus requisitos.

Para más detalles sobre las mejoras necesarias, consulta el [Roadmap completo](docs/roadmap.md).

## 🔗 Enlaces a Documentación Oficial AEAT

- [Sede electrónica VERI\*FACTU](https://sede.agenciatributaria.gob.es/Sede/iva/sistemas-informaticos-facturacion-verifactu.html)
- [Información técnica](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/informacion-tecnica.html)
- [Portal de Pruebas Externas](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/portal-pruebas-externas.html)
- [Documentación técnica detallada](docs/Verifactu-Guia-Tecnica.md) - Incluye enlaces a WSDL, XSD y especificaciones

## Tests

Hay tests de ejemplo en `tests/Verifactu.Client.Tests/`.

Para ejecutar los tests desde la raíz del repo:

```powershell
cd C:\DesarrolloGit\Veri-factuSender
dotnet test
```

## Desarrollo y buenas prácticas

-   Mantén datos sensibles fuera del repositorio (usa secrets o variables de entorno).
-   Versiona y documenta cualquier cambio en la estructura XML que implementes.
-   Añade validaciones y pruebas unitarias para: serialización XML vs XSD, cálculo de huella, firma y envío SOAP.

## Contribuir

Para contribuir al proyecto:

1. Lee la [Guía de Desarrollo](docs/desarrollo.md) para entender el proceso
2. Haz fork del repositorio
3. Crea una rama para tu funcionalidad
4. Acompaña cambios con tests y documentación clara
5. Abre un Pull Request

⚠️ **Importante**: No incluyas claves, certificados PFX o contraseñas en el repositorio.

## Licencia

Este repositorio incluye un fichero `LICENSE` en la raíz. Revisa la licencia para usos, distribución y contribuciones.

---

## 📖 Recursos Adicionales

- **[Documentación completa del proyecto](docs/)** - Toda la documentación en un solo lugar
- **[Preguntas frecuentes AEAT](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/preguntas-frecuentes.html)** - FAQs oficiales
- **[Real Decreto 1007/2023](https://www.boe.es/buscar/act.php?id=BOE-A-2023-24873)** - Normativa sobre SIF

Para consultas técnicas o reportar problemas, usa [GitHub Issues](https://github.com/JoseRGWeb/Veri-factuSender/issues).
