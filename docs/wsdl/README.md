# WSDL y Esquemas XSD Oficiales de AEAT

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
