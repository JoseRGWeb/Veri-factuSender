using System.Xml;
using System.Xml.Schema;

namespace Verifactu.Client.Services;

/// <summary>
/// Servicio para validar documentos XML contra esquemas XSD oficiales de AEAT.
/// NOTA: Requiere los archivos XSD oficiales en docs/xsd/
/// </summary>
public class XmlValidationService : IXmlValidationService
{
    private readonly string _xsdBasePath;
    private XmlSchemaSet? _schemas;
    private bool _isValid;
    private readonly List<string> _validationErrors;

    public XmlValidationService(string? xsdBasePath = null)
    {
        // Por defecto busca XSD en docs/xsd/ relativo al proyecto
        _xsdBasePath = xsdBasePath ?? Path.Combine(
            Directory.GetCurrentDirectory(), 
            "..", "..", "..", "..", "docs", "xsd"
        );
        _validationErrors = new List<string>();
    }

    /// <summary>
    /// Valida un documento XML contra los esquemas XSD.
    /// NOTA: Esta implementación requiere que los archivos XSD estén disponibles localmente.
    /// Los XSD oficiales deben descargarse desde AEAT y colocarse en docs/xsd/
    /// </summary>
    public bool ValidarContraXsd(XmlDocument xmlDoc, ValidationEventHandler? validationEventHandler = null)
    {
        try
        {
            // Intentar cargar esquemas si no están cargados
            if (_schemas == null)
            {
                _schemas = CargarEsquemasXsd();
            }

            // Si no hay esquemas disponibles, reportar warning pero no fallar
            if (_schemas == null || _schemas.Count == 0)
            {
                Console.WriteLine("ADVERTENCIA: No se encontraron esquemas XSD para validación.");
                Console.WriteLine($"Buscar XSD en: {_xsdBasePath}");
                Console.WriteLine("Los archivos XSD deben descargarse manualmente desde AEAT.");
                Console.WriteLine("Ver docs/xsd/README.md para instrucciones.");
                return true; // No fallar si no hay XSD disponibles
            }

            _isValid = true;
            _validationErrors.Clear();

            // Configurar validación
            xmlDoc.Schemas = _schemas;

            // Validar con handler personalizado o el predeterminado
            xmlDoc.Validate(validationEventHandler ?? ValidationCallback);

            return _isValid;
        }
        catch (Exception ex)
        {
            _validationErrors.Add($"Error durante validación: {ex.Message}");
            // Solo invocar el handler si existe, no crear ValidationEventArgs manualmente
            if (validationEventHandler != null)
            {
                Console.WriteLine($"Error de validación: {ex.Message}");
            }
            return false;
        }
    }

    /// <summary>
    /// Callback predeterminado para eventos de validación
    /// </summary>
    private void ValidationCallback(object? sender, ValidationEventArgs e)
    {
        if (e.Severity == XmlSeverityType.Error)
        {
            _isValid = false;
            _validationErrors.Add($"ERROR: {e.Message}");
        }
        else if (e.Severity == XmlSeverityType.Warning)
        {
            _validationErrors.Add($"WARNING: {e.Message}");
        }
    }

    /// <summary>
    /// Carga los esquemas XSD desde el directorio especificado
    /// </summary>
    private XmlSchemaSet? CargarEsquemasXsd()
    {
        if (!Directory.Exists(_xsdBasePath))
        {
            Console.WriteLine($"Directorio XSD no encontrado: {_xsdBasePath}");
            return null;
        }

        var schemas = new XmlSchemaSet();
        var xsdFiles = Directory.GetFiles(_xsdBasePath, "*.xsd");

        if (xsdFiles.Length == 0)
        {
            Console.WriteLine($"No se encontraron archivos XSD en: {_xsdBasePath}");
            return null;
        }

        foreach (var xsdFile in xsdFiles)
        {
            try
            {
                // Leer el XSD para extraer el namespace target
                using var reader = XmlReader.Create(xsdFile);
                var schema = XmlSchema.Read(reader, (sender, e) =>
                {
                    Console.WriteLine($"Error al leer XSD {Path.GetFileName(xsdFile)}: {e.Message}");
                });

                if (schema != null && !string.IsNullOrEmpty(schema.TargetNamespace))
                {
                    schemas.Add(schema);
                    Console.WriteLine($"XSD cargado: {Path.GetFileName(xsdFile)} (namespace: {schema.TargetNamespace})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar XSD {Path.GetFileName(xsdFile)}: {ex.Message}");
            }
        }

        schemas.Compile();
        return schemas.Count > 0 ? schemas : null;
    }

    /// <summary>
    /// Obtiene los errores de validación acumulados
    /// </summary>
    public IReadOnlyList<string> ObtenerErrores() => _validationErrors.AsReadOnly();
}
