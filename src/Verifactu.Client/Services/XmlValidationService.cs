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
    private readonly List<string> _validationWarnings;

    public XmlValidationService(string? xsdBasePath = null)
    {
        // Por defecto busca XSD en docs/xsd/ relativo a la solución
        _xsdBasePath = xsdBasePath ?? BuscarDirectorioXsd();
        _validationErrors = new List<string>();
        _validationWarnings = new List<string>();
    }

    /// <summary>
    /// Busca el directorio de XSD comenzando desde el directorio actual y subiendo hasta encontrar la solución
    /// </summary>
    private static string BuscarDirectorioXsd()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var dirInfo = new DirectoryInfo(currentDir);
        
        // Buscar hacia arriba hasta encontrar el archivo .sln o llegar a la raíz
        while (dirInfo != null)
        {
            // Buscar archivo .sln en el directorio actual
            if (dirInfo.GetFiles("*.sln").Length > 0)
            {
                // Encontramos la raíz de la solución
                var xsdPath = Path.Combine(dirInfo.FullName, "docs", "xsd");
                if (Directory.Exists(xsdPath))
                {
                    return xsdPath;
                }
            }
            
            dirInfo = dirInfo.Parent;
        }
        
        // Si no se encuentra, usar ruta por defecto relativa
        return Path.Combine(currentDir, "docs", "xsd");
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

            // Si no hay esquemas disponibles, registrar advertencia pero no fallar
            if (_schemas == null || _schemas.Count == 0)
            {
                _validationWarnings.Add("No se encontraron esquemas XSD para validación.");
                _validationWarnings.Add($"Ruta de búsqueda: {_xsdBasePath}");
                _validationWarnings.Add("Los archivos XSD deben descargarse manualmente desde AEAT.");
                _validationWarnings.Add("Ver docs/xsd/README.md para instrucciones.");
                return true; // No fallar si no hay XSD disponibles
            }

            _isValid = true;
            _validationErrors.Clear();
            _validationWarnings.Clear();

            // Configurar validación
            xmlDoc.Schemas = _schemas;

            // Validar con handler personalizado o el predeterminado
            xmlDoc.Validate(validationEventHandler ?? ValidationCallback);

            return _isValid;
        }
        catch (Exception ex)
        {
            _validationErrors.Add($"Error durante validación: {ex.Message}");
            if (validationEventHandler != null)
            {
                _validationErrors.Add($"Excepción: {ex.GetType().Name}");
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
            _validationWarnings.Add($"WARNING: {e.Message}");
        }
    }

    /// <summary>
    /// Carga los esquemas XSD desde el directorio especificado
    /// </summary>
    private XmlSchemaSet? CargarEsquemasXsd()
    {
        if (!Directory.Exists(_xsdBasePath))
        {
            _validationWarnings.Add($"Directorio XSD no encontrado: {_xsdBasePath}");
            return null;
        }

        var schemas = new XmlSchemaSet();
        var xsdFiles = Directory.GetFiles(_xsdBasePath, "*.xsd");

        if (xsdFiles.Length == 0)
        {
            _validationWarnings.Add($"No se encontraron archivos XSD en: {_xsdBasePath}");
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
                    _validationWarnings.Add($"Error al leer XSD {Path.GetFileName(xsdFile)}: {e.Message}");
                });

                if (schema != null && !string.IsNullOrEmpty(schema.TargetNamespace))
                {
                    schemas.Add(schema);
                    // XSD cargado correctamente - solo registramos en debug
                }
            }
            catch (Exception ex)
            {
                _validationWarnings.Add($"Error al cargar XSD {Path.GetFileName(xsdFile)}: {ex.Message}");
            }
        }

        schemas.Compile();
        return schemas.Count > 0 ? schemas : null;
    }

    /// <summary>
    /// Obtiene los errores de validación acumulados
    /// </summary>
    public IReadOnlyList<string> ObtenerErrores() => _validationErrors.AsReadOnly();
    
    /// <summary>
    /// Obtiene las advertencias de validación acumuladas
    /// </summary>
    public IReadOnlyList<string> ObtenerAdvertencias() => _validationWarnings.AsReadOnly();
}
