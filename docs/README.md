# Documentación de VerifactuSender

Bienvenido a la documentación completa del proyecto **VerifactuSender**, una solución en .NET 9 para la integración con el sistema VERI\*FACTU de la AEAT.

## Índice de Documentación

### 📚 Documentación General

1. **[Arquitectura del Proyecto](arquitectura.md)** - Descripción de la estructura y componentes del sistema
2. **[Instalación y Configuración](instalacion.md)** - Guía paso a paso para configurar el proyecto
3. **[Guía de Uso](uso.md)** - Ejemplos prácticos y casos de uso
4. **[Desarrollo y Contribución](desarrollo.md)** - Guía para desarrolladores que deseen contribuir

### 🔧 Documentación Técnica

5. **[Guía Técnica de Integración VERI\*FACTU](Verifactu-Guia-Tecnica.md)** - Referencia completa de integración con los servicios de la AEAT
6. **[Protocolos de Comunicación con AEAT](protocolos-comunicacion.md)** - Detalles de TLS, autenticación, formatos de mensajes y control de errores
7. **[Entorno de Pruebas (Sandbox AEAT)](entorno-pruebas.md)** - Configuración del entorno sandbox para pruebas contra el portal de la AEAT
8. **[Pruebas Funcionales End-to-End](pruebas-end-to-end.md)** - 🆕 Guía paso a paso para ejecutar pruebas de funcionamiento completas, incluyendo comandos, ejemplos y manejo de errores
9. **[Hoja de Ruta (Roadmap)](roadmap.md)** - Próximos pasos y mejoras planificadas

### 🔗 Enlaces a Documentación Oficial de la AEAT

#### Recursos Principales
- [Sede electrónica AEAT - Sistemas Informáticos de Facturación (SIF) y VERI\*FACTU](https://sede.agenciatributaria.gob.es/Sede/iva/sistemas-informaticos-facturacion-verifactu.html)
- [Información técnica VERI\*FACTU](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/informacion-tecnica.html)
- [Portal de Pruebas Externas](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/portal-pruebas-externas.html)

#### Documentación Técnica AEAT
- **Servicios Web:**
  - [WSDL de los servicios web](https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl)
  - [Esquemas XSD](https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/)
  
- **Especificaciones Técnicas:**
  - [Diseños de registro](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/disenos-registro.html)
  - [Documento de validaciones y errores](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/validaciones-errores.html)
  - [Algoritmo de cálculo de huella (hash)](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/algoritmo-huella.html)
  - [Especificaciones de firma electrónica](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/firma-electronica.html)
  - [Características del QR y servicio de cotejo](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/qr-cotejo.html)

- **Herramientas:**
  - [Aplicación gratuita de facturación](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/aplicacion-gratuita.html)

#### Normativa y FAQs
- [Preguntas frecuentes (FAQs)](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/preguntas-frecuentes.html)
- [Real Decreto 1007/2023 - Reglamento SIF](https://www.boe.es/buscar/act.php?id=BOE-A-2023-24873)

## Organización de la Documentación

La documentación está organizada para facilitar diferentes niveles de conocimiento:

- **Usuarios nuevos**: Comenzar por [Instalación](instalacion.md) → [Guía de Uso](uso.md)
- **Configurar entorno de pruebas**: Ver [Entorno de Pruebas (Sandbox)](entorno-pruebas.md)
- **Ejecutar pruebas funcionales**: Seguir [Pruebas End-to-End](pruebas-end-to-end.md)
- **Desarrolladores**: Ver [Arquitectura](arquitectura.md) → [Desarrollo](desarrollo.md)
- **Integración técnica**: Consultar [Guía Técnica VERI\*FACTU](Verifactu-Guia-Tecnica.md)
- **Planificación**: Revisar [Roadmap](roadmap.md)

## Contribuciones

Para contribuir a la documentación, consulta la [Guía de Desarrollo](desarrollo.md).

## Notas Importantes

⚠️ **Este proyecto es una plantilla/prototipo** que requiere adaptación a las especificaciones oficiales de la AEAT antes de su uso en producción. Consulta la sección de [próximos pasos](roadmap.md) para detalles sobre las adaptaciones necesarias.

---

**Última actualización:** 30 de octubre de 2025
