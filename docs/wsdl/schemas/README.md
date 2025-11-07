# Esquemas XSD Oficiales de AEAT para VERI*FACTU

## Estado Actual

⚠️ **IMPORTANTE**: Los archivos XSD oficiales NO están incluidos en este repositorio debido a restricciones de red durante el desarrollo automatizado.

**Los esquemas XSD deben descargarse manualmente** desde las URLs oficiales de AEAT indicadas a continuación.

## Descarga de Esquemas XSD Oficiales

### Entorno de Producción (RECOMENDADO)

Descargar los siguientes archivos desde las URLs oficiales de AEAT:

```bash
# Crear el directorio si no existe
mkdir -p docs/wsdl/schemas

# Descargar esquemas XSD oficiales
cd docs/wsdl/schemas

# 1. SuministroInformacion.xsd - Tipos comunes
curl -o SuministroInformacion.xsd \
  "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SuministroInformacion.xsd"

# 2. SuministroLR.xsd - Alta/anulación de registros
curl -o SuministroLR.xsd \
  "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SuministroLR.xsd"

# 3. RespuestaSuministro.xsd - Respuestas de operaciones
curl -o RespuestaSuministro.xsd \
  "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/RespuestaSuministro.xsd"

# 4. ConsultaLR.xsd - Consulta de registros
curl -o ConsultaLR.xsd \
  "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/ConsultaLR.xsd"

# 5. RespuestaConsultaLR.xsd - Respuesta de consultas
curl -o RespuestaConsultaLR.xsd \
  "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/RespuestaConsultaLR.xsd"
```

### Entorno de Pruebas (Sandbox)

Si necesitas los esquemas del entorno de pruebas:

```bash
# Reemplazar www2 por prewww2 en las URLs anteriores
curl -o SuministroInformacion.xsd \
  "https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SuministroInformacion.xsd"

# ... (repetir para los demás archivos)
```

## Verificación de Descarga

Después de descargar, verifica que los archivos estén presentes:

```bash
ls -lh docs/wsdl/schemas/
```

Deberías ver al menos los siguientes archivos:
- `SuministroInformacion.xsd` (tipos comunes)
- `SuministroLR.xsd` (altas y anulaciones)
- `RespuestaSuministro.xsd` (respuestas)
- `ConsultaLR.xsd` (opcional - para consultas)
- `RespuestaConsultaLR.xsd` (opcional - para respuestas de consultas)

## Información de los Esquemas

### SuministroInformacion.xsd
**Namespace**: `https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd`

Define los tipos comunes utilizados por todos los demás esquemas:
- `RegistroAlta` - Estructura principal del registro de facturación
- `IDFactura` - Identificación de facturas
- `Desglose` y `DetalleDesglose` - Información de IVA
- `SistemaInformatico` - Información del sistema de facturación
- `Encadenamiento` - Estructura para el encadenado de huellas
- Destinatarios, importes, fechas, etc.

### SuministroLR.xsd
**Namespace**: `https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroLR.xsd`

Define las operaciones de suministro:
- `RegFactuSistemaFacturacion` - Mensaje de alta/anulación
- `Cabecera` - Información del obligado emisor
- `RegistroFactura` - Contenedor de registros

### RespuestaSuministro.xsd
**Namespace**: `https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaSuministro.xsd`

Define la estructura de respuestas del servicio:
- Estados de aceptación/rechazo
- Códigos de error
- Información de registro

## Validación XML contra XSD

Una vez descargados los esquemas, puedes validar XML generado:

### Con xmllint (Linux/Mac)

```bash
xmllint --noout --schema docs/wsdl/schemas/SuministroInformacion.xsd mi-registro.xml
```

### Con C# (mediante XmlValidationService)

```csharp
var validationService = new XmlValidationService("/ruta/a/docs/wsdl/schemas");
var xmlDoc = /* tu XmlDocument */;

var esValido = validationService.ValidarContraXsd(xmlDoc, (sender, e) =>
{
    Console.WriteLine($"{e.Severity}: {e.Message}");
});

if (!esValido)
{
    var errores = validationService.ObtenerErrores();
    foreach (var error in errores)
    {
        Console.WriteLine(error);
    }
}
```

## Tests Automatizados

Los tests en `XmlSerializationTests.cs` incluyen:

1. **Tests sin XSD**: Verifican estructura básica sin esquemas (siempre pasan)
2. **Tests con XSD**: Se ejecutan solo si los archivos XSD están disponibles

Para habilitar tests con validación XSD completa:
1. Descargar los esquemas según instrucciones arriba
2. Ejecutar `dotnet test` - los tests detectarán automáticamente los esquemas

## Documentación de Referencia

- **Guía Técnica**: `docs/Verifactu-Guia-Tecnica.md` (Sección 4.2)
- **Descripción Servicios Web**: `docs/Veri-Factu_Descripcion_SWeb.md`
- **Guía Serialización**: `docs/xml-serialization-guide.md`
- **WSDL y Endpoints**: `docs/wsdl/README.md`

## Versiones y Actualización

**Última versión conocida**: v1.0.3 (28/07/2025)

Los esquemas XSD pueden actualizarse. Consulta regularmente:
- Sede electrónica AEAT: https://sede.agenciatributaria.gob.es
- Portal de Pruebas: https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/

## Notas Importantes

1. **Sin XSD, la validación salta con advertencia**: Si los archivos no están presentes, `XmlValidationService` emite advertencias pero NO falla, permitiendo desarrollo sin esquemas.

2. **Producción requiere validación**: Antes de pasar a producción, **DEBES** validar contra los XSD oficiales.

3. **Namespace correcto**: El código ya usa el namespace oficial correcto:
   ```
   https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd
   ```

4. **WSDL completo**: Para el WSDL completo del servicio, consulta `docs/wsdl/README.md`

## Checklist Pre-Producción

Antes de usar en producción, verifica:

- [ ] Esquemas XSD descargados y en `docs/wsdl/schemas/`
- [ ] XML generado válido contra XSD (`XmlValidationService`)
- [ ] Tests de validación pasando con XSD
- [ ] Namespace correcto en XML generado
- [ ] Estructura conforme a ejemplos oficiales (Anexo II documentación)
- [ ] Pruebas exitosas contra sandbox AEAT
