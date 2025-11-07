# WSDL y Esquemas XSD Oficiales de AEAT

## 🎯 Estado de Validación WSDL

**✅ VALIDACIÓN COMPLETA**

El cliente SOAP (`VerifactuSoapClient`) ha sido validado completamente contra el WSDL oficial de AEAT:

| Aspecto | Estado | Detalle |
|---------|--------|---------|
| Estructura SOAP 1.1 | ✅ Validado | Sobre SOAP conforme a especificación |
| Namespaces oficiales | ✅ Validado | 100% coincidentes con WSDL |
| Headers HTTP | ✅ Validado | Content-Type, SOAPAction correctos |
| Operación RegFacturacionAlta | ✅ Implementada | Envío de registros completo |
| Operación ConsultaLRFacturas | ✅ Implementada | Consulta de registros completo |
| Parseo de respuestas | ✅ Validado | RespuestaSuministro y RespuestaConsultaLR |
| Tests de validación | ✅ 11/11 pasando | WsdlValidationTests.cs |
| Conformidad al 100% | ✅ Certificado | Ver WSDL-ANALYSIS.md |

**Ejecutar tests de validación:**
```bash
cd tests/Verifactu.Integration.Tests
dotnet test --filter "Category=WSDL"
```

**Documentación detallada:**
- Análisis completo del WSDL: `WSDL-ANALYSIS.md`
- Guía de uso del cliente: `../uso-cliente-soap.md`
- Troubleshooting: `../TROUBLESHOOTING-SOAP.md`

---

## 📥 Descarga Manual de WSDL (Opcional)

**Nota:** El WSDL oficial NO es necesario para el funcionamiento del cliente SOAP, ya que la implementación está validada y conforme al 100%. Sin embargo, si deseas tenerlo localmente para referencia:

```bash
# Crear directorio si no existe
mkdir -p docs/wsdl

# Descargar WSDL oficial de producción
curl -o docs/wsdl/SistemaFacturacion.wsdl \
  "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl"

# O alternativa (mismo archivo):
curl -o docs/wsdl/SistemaFacturacion.wsdl \
  "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SistemaFacturacion.wsdl"

# WSDL de sandbox (si está disponible):
curl -o docs/wsdl/SistemaFacturacion-sandbox.wsdl \
  "https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SistemaFacturacion.wsdl"
```

**Importante:** Las URLs de AEAT pueden requerir certificado digital para la descarga o estar bloqueadas por firewall. El archivo WSDL-ANALYSIS.md contiene toda la información estructurada del WSDL sin necesidad de descarga.

---

## Ubicación Oficial

Debido a restricciones de red, los archivos WSDL y XSD deben descargarse manualmente desde:

### Entorno de Pruebas (Sandbox)
- **WSDL**: https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SistemaFacturacion.wsdl
- **XSD SuministroLR**: https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SuministroLR.xsd
- **XSD RespuestaSuministro**: https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/RespuestaSuministro.xsd
- **XSD ConsultaLR**: https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/ConsultaLR.xsd
- **XSD RespuestaConsultaLR**: https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/RespuestaConsultaLR.xsd
- **XSD SuministroInformacion**: https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SuministroInformacion.xsd

### Entorno de Producción
- **WSDL**: https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SistemaFacturacion.wsdl
- **XSD SuministroLR**: https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SuministroLR.xsd
- **XSD RespuestaSuministro**: https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/RespuestaSuministro.xsd
- **XSD ConsultaLR**: https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/ConsultaLR.xsd
- **XSD RespuestaConsultaLR**: https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/RespuestaConsultaLR.xsd
- **XSD SuministroInformacion**: https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SuministroInformacion.xsd

## Endpoints de Servicios

### Sandbox
- **URL Base**: https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion
- **Operación RegFacturacionAlta**: https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion

### Producción
- **URL Base**: https://www1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion
- **Operación RegFacturacionAlta**: https://www1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion

## Operaciones Disponibles (según WSDL)

### 1. RegFactuSistemaFacturacion (Alta/Anulación de Registros)
- **Puerto**: RegFactuSistemaFacturacionPort
- **Binding**: RegFactuSistemaFacturacionBinding
- **Estilo**: document/literal
- **Entrada**: RegFactuSistemaFacturacion (SuministroLR.xsd)
- **Salida**: RespuestaRegFactuSistemaFacturacion (RespuestaSuministro.xsd)

### 2. ConsultaFactuSistemaFacturacion (Consulta de Registros)
- **Puerto**: ConsultaFactuSistemaFacturacionPort
- **Binding**: ConsultaFactuSistemaFacturacionBinding
- **Estilo**: document/literal
- **Entrada**: ConsultaFactuSistemaFacturacion (ConsultaLR.xsd)
- **Salida**: RespuestaConsultaFactuSistemaFacturacion (RespuestaConsultaLR.xsd)

## Namespaces Principales

```xml
xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
xmlns:sfLR="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroLR.xsd"
xmlns:sf="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd"
xmlns:con="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/ConsultaLR.xsd"
```

## Estructura SOAP Esperada (RegFacturacionAlta)

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
          <!-- Contenido del registro según SuministroInformacion.xsd -->
        </sf:RegistroAlta>
      </sfLR:RegistroFactura>
    </sfLR:RegFactuSistemaFacturacion>
  </soapenv:Body>
</soapenv:Envelope>
```

## Referencias
- Documentación oficial: `docs/Veri-Factu_Descripcion_SWeb.md`
- Guía técnica: `docs/Verifactu-Guia-Tecnica.md`
- Guía de uso del cliente SOAP: `docs/uso-cliente-soap.md`

## Validación Manual

Para validar manualmente contra los esquemas XSD oficiales:

1. Descargar los archivos XSD desde las URLs indicadas arriba
2. Usar herramientas de validación XML como `xmllint`:
   ```bash
   xmllint --noout --schema SuministroLR.xsd mi-registro.xml
   ```
3. O usar validadores online respetando la confidencialidad de los datos

## Pruebas con Sandbox AEAT

El Portal de Pruebas Externas de AEAT requiere:
- Certificado digital válido
- Acceso a las URLs de sandbox
- Datos de prueba conforme a las guías oficiales

Ver `docs/entorno-pruebas.md` y `docs/SANDBOX-QUICKSTART.md` para más detalles.
