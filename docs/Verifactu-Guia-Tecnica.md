# Guía técnica de integración con VERI*FACTU (AEAT)

> **Fecha:** 30/10/2025 — **Ámbito:** Integración backend (servicios web) para remisión voluntaria de *registros de facturación* VERI*FACTU y funcionalidades relacionadas (consulta, QR, validaciones y firma).  
> **Nota:** Esta guía resume y organiza la documentación pública de la AEAT. Revisa siempre los originales por si hubiera cambios.

---

## Índice
1. [Resumen ejecutivo](#resumen-ejecutivo)
2. [Panorama general y componentes](#panorama-general-y-componentes)
3. [Entornos y autenticación](#entornos-y-autenticación)
4. [Artefactos técnicos](#artefactos-técnicos)
   - 4.1 [WSDL (servicios web)](#41-wsdl-servicios-web)
   - 4.2 [Esquemas XSD](#42-esquemas-xsd)
   - 4.3 [Diseños de registro](#43-diseños-de-registro)
   - 4.4 [Documento de validaciones y errores](#44-documento-de-validaciones-y-errores)
5. [Algoritmo de huella («hash») y encadenado](#algoritmo-de-huella-hash-y-encadenado)
6. [Firma electrónica (XMLDSig) de registros](#firma-electrónica-xmldsig-de-registros)
7. [Código QR y servicio de cotejo](#código-qr-y-servicio-de-cotejo)
8. [Flujos de integración recomendados](#flujos-de-integración-recomendados)
9. [Modelo de datos sugerido (persistencia)](#modelo-de-datos-sugerido-persistencia)
10. [Buenas prácticas y seguridad](#buenas-prácticas-y-seguridad)
11. [Checklist de paso a producción](#checklist-de-paso-a-producción)
12. [Anexos](#anexos)
    - A. [Ejemplo de estructura SOAP (placeholder)](#a-ejemplo-de-estructura-soap-placeholder)
    - B. [Ejemplo de XML de registro (placeholder)](#b-ejemplo-de-xml-de-registro-placeholder)
13. [Fuentes oficiales](#fuentes-oficiales)

---

## Resumen ejecutivo
VERI*FACTU es el marco de **sistemas informáticos de facturación (SIF)** que permite **remitir voluntariamente** a la AEAT los **registros de facturación** en el momento o con inmediatez suficiente, garantizando trazabilidad mediante **huella SHA‑256 encadenada**, y habilitando **cotejo** por QR. La AEAT facilita **servicios web (SOAP)**, **WSDL/XSD**, **documento de validaciones**, **especificación de hash** y **firma electrónica**, además de un **Portal de Pruebas Externas** y una **aplicación gratuita** en sede para casos manuales. [S1, S2, S3, S4, S5]

## Panorama general y componentes
- **Sede electrónica VERI*FACTU**: portal con información general, gestiones, app gratuita, vídeos y FAQs. [S1]
- **Información técnica** (índice único): acceso a **Diseños de registro**, **WSDL**, **Esquemas (XSD)**, **Validaciones/Errores**, **Especificaciones de servicios**, **Algoritmo de huella (hash)**, **Firma**, **Características del QR**, **Ejemplos de declaraciones responsables**. [S2]
- **Portal de Pruebas Externas**: entorno de pruebas de la AEAT (sin trascendencia tributaria). Requiere **certificado electrónico**. [S3, S6]
- **Aplicación gratuita de facturación**: para contribuyentes con pocas facturas; genera factura imprimible con QR y conserva el registro (alta VERI*FACTU). [S7, S1]

## Entornos y autenticación
- **Pruebas**: *Portal de Pruebas Externas* (autenticación **con certificado**). Permite pruebas de presentación/consulta y acceso a artefactos (WSDL/XSD) de preproducción. [S3, S6, S8, S9]
- **Producción**: sede electrónica (servicios web).  
- **Autenticación/seguridad**: los accesos a cliente de servicio web y consultas se indican **“Con certificado”** (mutua autenticación TLS). Los endpoints y operaciones se definen en el **WSDL**. [S8, S9]
- **No usar Cl@ve para procesos automáticos**: Cl@ve es para interacción humana en sede; las integraciones servidor-a-servidor emplean certificado.

## Artefactos técnicos
### 4.1 WSDL (servicios web)
- Página oficial WSDL (desarrolladores AEAT). [S8]  
- **WSDL de pruebas** (preproducción) accesible con certificado: `SistemaFacturacion.wsdl`. [S9]

### 4.2 Esquemas XSD
Listado publicado (preproducción, con certificado): [S10]  
- `SuministroLR.xsd` — altas/anulaciones VERI*FACTU y no VERI*FACTU.  
- `RespuestaSuministro.xsd` — respuesta de operaciones.  
- `ConsultaLR.xsd` — consulta de registros VERI*FACTU.  
- `RespuestaConsultaLR.xsd` — respuesta de consulta.  
- `SuministroInformacion.xsd` — tipos comunes.  
- `EventosSIF.xsd` — registro de eventos (sistemas no VERI*FACTU).  
- `RespuestaValRegistNoVeriFactu.xsd` — respuesta de validación de registros no VERI*FACTU.

### 4.3 Diseños de registro
Referencia centralizada en sede (incluye definiciones de campos de los registros). [S4]

### 4.4 Documento de validaciones y errores
Punto de entrada en sede (índice técnico). Contiene reglas, códigos y descripciones de validaciones. [S5]  
> Nota: además existe documentación complementaria de errores/estados en descripciones de servicio web. [S13]

## Algoritmo de huella («hash») y encadenado
- **Hash**: **SHA‑256** sobre los datos definidos del registro. [S12]  
- **Encadenamiento**: la huella del registro **N** se incluye en el registro **N+1**; implementa trazabilidad y orden cronológico. [S12]  
- **Especificación detallada**: ver la página **“Algoritmo de cálculo de codificación de la huella o ‘hash’”** (incluye detalles y ejemplos). [S11]

## Firma electrónica (XMLDSig) de registros
- Página técnica: **“Especificaciones técnicas de la firma electrónica de los registros de facturación y de evento”**. [S14]  
- **Obligatoriedad**: los **sistemas no VERI*FACTU** deben firmar todos los registros (RF y eventos). Los **VERI*FACTU** disponen de requisitos propios (cotejo por QR, hash encadenado, etc.). Revisa el detalle normativo en la página de firma y FAQs. [S15]

## Código QR y servicio de cotejo
- Página técnica: **“Características del QR y especificaciones del servicio de cotejo o remisión por parte del receptor”**. [S16]  
- **Factura verificable**: inclusión de **código QR** y, en su caso, mención **VERI*FACTU** en facturas completas/simplificadas emitidas con SIF. [S16, S18]  
- El **receptor** puede **cotejar** la constancia del registro en la sede a partir del QR. [S17]

## Flujos de integración recomendados
1. **Generación de factura** en tu sistema → construir **Registro de facturación** (diseños de registro). [S4]
2. **Cálculo de huella SHA‑256** y **encadenado** (incluir `hash` previo en el siguiente registro). [S11, S12]
3. **Serialización XML** conforme a **XSD** (Suministro/Tipos). [S10]
4. **Firma** (cuando aplique según especificaciones) y **envío SOAP** al endpoint del **WSDL** (TLS mutuo con certificado). [S8, S9, S14]
5. **Gestión de respuesta**: estados de aceptación, errores (consultar **validaciones y errores**); registro en tu BD. [S5, S13]
6. **Generación del QR** y **leyendas** en la factura impresa/visualizable. [S16, S18]
7. **Consulta** de registros (operación de consulta). [S10]

## Modelo de datos sugerido (persistencia)
- **Tabla RegistrosFacturacion**: `Uuid`, `Serie`, `Numero`, `FechaHoraExpedicionUTC`, `Huella`, `HuellaAnterior`, `EstadoEnvio`, `CodigoErrorAEAT`, `DescripcionErrorAEAT`, `IdFactura`, `XmlFirmado`, `AcuseRecibo`, `FechaEnvio`, `Reintentos`.
- **Tabla Facturas**: datos fiscal/IVA, receptor, totales, líneas (normalizado).
- **Índices**: por `FechaHoraExpedicionUTC`, `Serie+Numero`, `EstadoEnvio`.
- **Bitácora**: cambios relevantes y **registro de eventos** (en especial si operas modalidades no VERI*FACTU).

## Buenas prácticas y seguridad
- **Certificados**: limitar permisos PFX; considerar almacén de certificados. Rotar con antelación.
- **Idempotencia**: usar `Uuid` y controles de duplicado (hay validación AEAT). [S13]
- **Observabilidad**: trazabilidad por `Uuid`, `Huella`, y acuses; *dead‑letter queue* de envíos fallidos.
- **Aislamiento**: separar procesos de firma/hashing de la capa web pública.
- **Versionado**: registrar versiones de XSD/WSDL/documentos usados.
- **Ensayos**: automatizar pruebas de integración contra **Portal de Pruebas Externas** antes de cambios. [S3]

## Checklist de paso a producción
- [ ] Certificado de representante instalado y probado (cliente TLS). [S3, S8]
- [ ] XML 100% conforme a **XSD** (validación local + pruebas). [S10]
- [ ] **Hash** y **encadenado** verificados con ejemplos oficiales. [S11, S12]
- [ ] **Firma** implementada según especificación (cuando aplique). [S14]
- [ ] Manejo completo de **validaciones/errores**. [S5, S13]
- [ ] QR y leyendas en factura (visual/impresa). [S16, S18]
- [ ] Procedimientos de reintento e idempotencia.
- [ ] Registro y auditoría operativa.

## Anexos

### A) Ejemplo de estructura SOAP (placeholder)
```xml
<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
  <soapenv:Header/>
  <soapenv:Body>
    <veri:EnviarRegistroRequest xmlns:veri="urn:aeat:verifactu:placeholder">
      <veri:RegistroXml>
        <!-- Aquí va el XML de registro conforme a XSD de AEAT -->
      </veri:RegistroXml>
    </veri:EnviarRegistroRequest>
  </soapenv:Body>
</soapenv:Envelope>
```

### B) Ejemplo de XML de registro (placeholder)
> **Sustituir** por las etiquetas reales de los **XSD** (“SuministroLR.xsd”, “SuministroInformacion.xsd”, etc.).
```xml
<veri:RegistroFacturacion xmlns:veri="urn:aeat:verifactu:placeholder">
  <veri:Cabecera>
    <veri:Uuid>...</veri:Uuid>
    <veri:FechaHoraExpedicionUtc>2025-10-30T10:00:00Z</veri:FechaHoraExpedicionUtc>
    <veri:Serie>A</veri:Serie>
    <veri:Numero>2025-0001</veri:Numero>
    <veri:HashPrevio>...</veri:HashPrevio>
    <veri:Huella>...</veri:Huella>
  </veri:Cabecera>
  <veri:Factura>
    <!-- Emisor, Receptor, Líneas, Totales, etc. -->
  </veri:Factura>
</veri:RegistroFacturacion>
```

---

## Fuentes oficiales
- **S1.** Sede AEAT — “Sistemas Informáticos de Facturación (SIF) y VERI*FACTU” (página general).
- **S2.** Sede AEAT — “Información técnica” (índice VERI*FACTU).
- **S3.** Sede AEAT — “Portal de Pruebas Externas” (ficha).
- **S4.** Sede AEAT — “Diseños de registro” (índice técnico).
- **S5.** Sede AEAT — “Documento de validaciones y errores” (índice técnico).
- **S6.** Portal de Pruebas Externas (preportal) — acceso con certificado.
- **S7.** Manual PDF — “Guía de la aplicación gratuita de facturación de la AEAT”.
- **S8.** Desarrolladores AEAT — “WSDL de los servicios web”.
- **S9.** WSDL de pruebas — “SistemaFacturacion.wsdl” (preproducción, certificado).
- **S10.** Desarrolladores AEAT — “Esquemas de los servicios web (XSD)”.
- **S11.** Sede AEAT — “Algoritmo de cálculo de la huella o ‘hash’”.
- **S12.** Sede AEAT — “FAQ: Huella o «hash» (SHA‑256 y encadenado)”.
- **S13.** Desarrolladores AEAT — “Descripción del servicio web (remisión voluntaria)” (PDF).
- **S14.** Sede AEAT — “Especificaciones técnicas de la firma electrónica”.
- **S15.** Sede AEAT — “FAQ: Firma” (alcance por modalidad).
- **S16.** Sede AEAT — “Características del QR y servicio de cotejo”.
- **S17.** Sede AEAT — “FAQ: Sistemas VERI*FACTU” (cotejo por QR).
- **S18.** Sede AEAT — “Cuestiones generales” (QR y leyendas).

---

## Enlaces Directos a Documentación Oficial

Para facilitar el acceso, aquí se proporcionan enlaces directos a los recursos más importantes de la AEAT:

### Documentación Esencial
- [Sede electrónica VERI*FACTU](https://sede.agenciatributaria.gob.es/Sede/iva/sistemas-informaticos-facturacion-verifactu.html)
- [Información técnica completa](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/informacion-tecnica.html)
- [Portal de Pruebas Externas](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/portal-pruebas-externas.html)

### Archivos Técnicos
- [WSDL de servicios web](https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl)
- [Esquemas XSD](https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/)

### Normativa
- [Real Decreto 1007/2023 - Reglamento SIF](https://www.boe.es/buscar/act.php?id=BOE-A-2023-24873)
- [Preguntas frecuentes](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/preguntas-frecuentes.html)

**Nota**: Los enlaces pueden cambiar. Verificar siempre desde la [sede electrónica oficial](https://sede.agenciatributaria.gob.es).

**Última actualización de enlaces**: 30 de octubre de 2025
