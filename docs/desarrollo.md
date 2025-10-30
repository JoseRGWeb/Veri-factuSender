# Guía de Desarrollo y Contribución

Esta guía está dirigida a desarrolladores que deseen contribuir al proyecto VerifactuSender o entender su funcionamiento interno para adaptarlo a sus necesidades.

## Configuración del Entorno de Desarrollo

### Herramientas Recomendadas

#### IDEs
- **Visual Studio 2022** (Windows) - IDE completo con excelente soporte para .NET
- **Visual Studio Code** (multiplataforma) - Con las siguientes extensiones:
  - C# Dev Kit
  - .NET Install Tool
  - NuGet Package Manager
- **JetBrains Rider** (multiplataforma) - IDE comercial potente para .NET

#### Control de Versiones
```bash
git --version  # Verifica que Git esté instalado
```

#### .NET SDK
```bash
dotnet --version  # Debe ser 9.0.x o superior
dotnet --list-sdks  # Ver todos los SDKs instalados
```

### Clonar y Preparar el Repositorio

```bash
# Clonar el repositorio
git clone https://github.com/JoseRGWeb/Veri-factuSender.git
cd Veri-factuSender

# Crear una rama para tu trabajo
git checkout -b feature/mi-nueva-funcionalidad

# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar tests
dotnet test
```

## Estructura del Código

### Organización de Proyectos

```
Veri-factuSender/
├── src/
│   ├── Verifactu.Client/           # Biblioteca principal
│   │   ├── Models/                 # Modelos de dominio
│   │   ├── Services/               # Lógica de negocio
│   │   └── Soap/                   # Cliente SOAP
│   └── Verifactu.ConsoleDemo/      # Aplicación demo
├── tests/
│   └── Verifactu.Client.Tests/     # Tests unitarios
└── docs/                            # Documentación
```

### Convenciones de Código

#### Nomenclatura
- **Clases e Interfaces**: PascalCase (`RegistroFacturacion`, `IHashService`)
- **Métodos**: PascalCase (`CalcularHuella`, `SerializarRegistro`)
- **Propiedades**: PascalCase (`Uuid`, `FechaExpedicion`)
- **Parámetros y variables locales**: camelCase (`huellaAnterior`, `xmlFirmado`)
- **Constantes**: PascalCase (`DefaultTimeout`)

#### Interfaces
Todas las interfaces deben comenzar con `I`:
```csharp
public interface IHashService { }
```

#### Comentarios en Español
Todos los comentarios de código deben estar en español:
```csharp
// Calcula la huella SHA-256 del registro
public string CalcularHuella(RegistroFacturacion registro)
{
    // Implementación
}
```

## Desarrollo de Nuevas Funcionalidades

### 1. Crear Modelo de Datos

Los modelos se ubican en `src/Verifactu.Client/Models/`.

```csharp
namespace Verifactu.Client.Models
{
    /// <summary>
    /// Representa una línea de detalle de una factura
    /// </summary>
    public class LineaFactura
    {
        /// <summary>
        /// Descripción del producto o servicio
        /// </summary>
        public string Descripcion { get; set; }
        
        /// <summary>
        /// Cantidad de unidades
        /// </summary>
        public decimal Cantidad { get; set; }
        
        /// <summary>
        /// Precio unitario sin IVA
        /// </summary>
        public decimal PrecioUnitario { get; set; }
        
        /// <summary>
        /// Tipo de IVA aplicable (porcentaje)
        /// </summary>
        public int TipoIva { get; set; }
    }
}
```

### 2. Crear Servicio

Los servicios se ubican en `src/Verifactu.Client/Services/`.

#### Definir la Interfaz

```csharp
namespace Verifactu.Client.Services
{
    /// <summary>
    /// Servicio para validación de facturas
    /// </summary>
    public interface IValidadorFacturas
    {
        /// <summary>
        /// Valida que una factura cumpla con las reglas de negocio
        /// </summary>
        /// <param name="factura">Factura a validar</param>
        /// <returns>True si la factura es válida, False en caso contrario</returns>
        bool ValidarFactura(Factura factura);
        
        /// <summary>
        /// Obtiene los errores de validación de una factura
        /// </summary>
        /// <param name="factura">Factura a validar</param>
        /// <returns>Lista de mensajes de error</returns>
        IEnumerable<string> ObtenerErroresValidacion(Factura factura);
    }
}
```

#### Implementar el Servicio

```csharp
namespace Verifactu.Client.Services
{
    /// <summary>
    /// Implementación del validador de facturas
    /// </summary>
    public class ValidadorFacturas : IValidadorFacturas
    {
        public bool ValidarFactura(Factura factura)
        {
            return !ObtenerErroresValidacion(factura).Any();
        }
        
        public IEnumerable<string> ObtenerErroresValidacion(Factura factura)
        {
            var errores = new List<string>();
            
            if (string.IsNullOrWhiteSpace(factura.Serie))
                errores.Add("La serie de la factura es obligatoria");
            
            if (string.IsNullOrWhiteSpace(factura.Numero))
                errores.Add("El número de la factura es obligatorio");
            
            if (factura.TotalFactura <= 0)
                errores.Add("El total de la factura debe ser mayor que cero");
            
            // Validar que el total coincida con suma de líneas + IVA
            var totalCalculado = factura.TotalSinIva + factura.TotalIva;
            if (Math.Abs(factura.TotalFactura - totalCalculado) > 0.01m)
                errores.Add("El total de la factura no coincide con la suma de base + IVA");
            
            return errores;
        }
    }
}
```

### 3. Escribir Tests

Los tests se ubican en `tests/Verifactu.Client.Tests/`.

```csharp
using Xunit;
using Verifactu.Client.Models;
using Verifactu.Client.Services;

namespace Verifactu.Client.Tests
{
    public class ValidadorFacturasTests
    {
        private readonly IValidadorFacturas _validador;
        
        public ValidadorFacturasTests()
        {
            _validador = new ValidadorFacturas();
        }
        
        [Fact]
        public void ValidarFactura_FacturaValida_RetornaTrue()
        {
            // Arrange
            var factura = new Factura
            {
                Serie = "A",
                Numero = "2025-0001",
                TotalSinIva = 100.00m,
                TotalIva = 21.00m,
                TotalFactura = 121.00m,
                Emisor = new Emisor { Nif = "B12345678", Nombre = "Empresa SL" },
                Receptor = new Receptor { Nif = "12345678A", Nombre = "Cliente" }
            };
            
            // Act
            var resultado = _validador.ValidarFactura(factura);
            
            // Assert
            Assert.True(resultado);
        }
        
        [Fact]
        public void ValidarFactura_SerieFaltante_RetornaFalse()
        {
            // Arrange
            var factura = new Factura
            {
                Serie = "", // Serie vacía
                Numero = "2025-0001",
                TotalFactura = 121.00m
            };
            
            // Act
            var resultado = _validador.ValidarFactura(factura);
            
            // Assert
            Assert.False(resultado);
        }
        
        [Fact]
        public void ObtenerErroresValidacion_TotalIncorrecto_RetornaMensajeError()
        {
            // Arrange
            var factura = new Factura
            {
                Serie = "A",
                Numero = "2025-0001",
                TotalSinIva = 100.00m,
                TotalIva = 21.00m,
                TotalFactura = 100.00m // Total incorrecto
            };
            
            // Act
            var errores = _validador.ObtenerErroresValidacion(factura);
            
            // Assert
            Assert.Contains(errores, e => e.Contains("total de la factura no coincide"));
        }
    }
}
```

### 4. Ejecutar Tests

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests de un proyecto específico
dotnet test tests/Verifactu.Client.Tests/

# Ejecutar tests con cobertura
dotnet test /p:CollectCoverage=true

# Ejecutar un test específico
dotnet test --filter "FullyQualifiedName~ValidadorFacturasTests.ValidarFactura_FacturaValida_RetornaTrue"
```

## Debugging

### Visual Studio / Rider

1. Establecer breakpoints haciendo clic en el margen izquierdo
2. Presionar F5 para iniciar debugging
3. Usar F10 (Step Over) y F11 (Step Into) para navegar

### Visual Studio Code

1. Crear `.vscode/launch.json`:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (console)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/src/Verifactu.ConsoleDemo/bin/Debug/net9.0/Verifactu.ConsoleDemo.dll",
            "args": [],
            "cwd": "${workspaceFolder}/src/Verifactu.ConsoleDemo",
            "console": "internalConsole",
            "stopAtEntry": false
        }
    ]
}
```

2. Establecer breakpoints y presionar F5

## Trabajar con Certificados en Desarrollo

### Crear Certificado de Prueba

```bash
# Generar certificado autofirmado (solo para pruebas locales)
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes

# Convertir a PFX
openssl pkcs12 -export -out certificado-prueba.pfx -inkey key.pem -in cert.pem
```

**⚠️ Advertencia**: Los certificados autofirmados NO son válidos para AEAT. Solo úsalos para desarrollo local.

## Contribuir al Proyecto

### Proceso de Contribución

1. **Fork del repositorio** en GitHub

2. **Clonar tu fork**:
```bash
git clone https://github.com/TU_USUARIO/Veri-factuSender.git
cd Veri-factuSender
```

3. **Crear una rama** para tu funcionalidad:
```bash
git checkout -b feature/descripcion-breve
```

4. **Hacer cambios** siguiendo las convenciones del proyecto

5. **Escribir tests** para tu funcionalidad

6. **Ejecutar tests** y asegurar que pasen:
```bash
dotnet test
```

7. **Commit de cambios**:
```bash
git add .
git commit -m "Descripción clara del cambio"
```

8. **Push a tu fork**:
```bash
git push origin feature/descripcion-breve
```

9. **Crear Pull Request** en GitHub

### Convenciones de Commits

Usar mensajes descriptivos en español:

```bash
# Buenos ejemplos
git commit -m "Añadir validación de NIF en emisor y receptor"
git commit -m "Corregir cálculo de IVA en facturas con descuento"
git commit -m "Actualizar documentación de instalación"

# Evitar
git commit -m "fix"
git commit -m "cambios"
git commit -m "WIP"
```

### Checklist antes de Pull Request

- [ ] El código compila sin errores
- [ ] Todos los tests pasan
- [ ] Se han añadido tests para las nuevas funcionalidades
- [ ] La documentación está actualizada
- [ ] El código sigue las convenciones del proyecto
- [ ] No se incluyen secretos ni certificados
- [ ] Los comentarios están en español

## Directrices de Código

### Manejo de Errores

```csharp
// ✅ Correcto: Excepciones específicas con mensajes claros
public void CargarCertificado(string ruta)
{
    if (string.IsNullOrEmpty(ruta))
        throw new ArgumentNullException(nameof(ruta), "La ruta del certificado no puede estar vacía");
    
    if (!File.Exists(ruta))
        throw new FileNotFoundException($"No se encontró el certificado en: {ruta}");
    
    try
    {
        // Cargar certificado
    }
    catch (CryptographicException ex)
    {
        throw new InvalidOperationException("Error al cargar el certificado. Verifique la contraseña.", ex);
    }
}

// ❌ Incorrecto: Excepciones genéricas sin contexto
public void CargarCertificado(string ruta)
{
    try
    {
        // Cargar certificado
    }
    catch (Exception ex)
    {
        throw ex; // Pierde el stack trace
    }
}
```

### Uso de Async/Await

```csharp
// ✅ Correcto: Usar async/await apropiadamente
public async Task<string> EnviarRegistroAsync(string xml)
{
    using var client = new HttpClient();
    var response = await client.PostAsync(url, content);
    return await response.Content.ReadAsStringAsync();
}

// ❌ Incorrecto: Bloquear operaciones asíncronas
public string EnviarRegistro(string xml)
{
    var task = EnviarRegistroAsync(xml);
    return task.Result; // Puede causar deadlocks
}
```

### Inyección de Dependencias

```csharp
// ✅ Correcto: Usar interfaces e inyección
public class ServicioFacturacion
{
    private readonly IHashService _hashService;
    private readonly IVerifactuSerializer _serializer;
    
    public ServicioFacturacion(IHashService hashService, IVerifactuSerializer serializer)
    {
        _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }
}

// ❌ Incorrecto: Crear dependencias directamente
public class ServicioFacturacion
{
    private readonly HashService _hashService = new HashService(); // Acoplamiento fuerte
}
```

## Recursos Útiles

### Documentación .NET
- [Guía de C#](https://docs.microsoft.com/es-es/dotnet/csharp/)
- [API de .NET](https://docs.microsoft.com/es-es/dotnet/api/)
- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore/cmdline)

### Documentación VERI*FACTU
- [Documentación oficial AEAT](https://sede.agenciatributaria.gob.es/Sede/iva/sistemas-informaticos-facturacion-verifactu.html)
- [Guía Técnica del proyecto](Verifactu-Guia-Tecnica.md)

### Herramientas
- [XML Schema Validator](https://www.freeformatter.com/xml-validator-xsd.html)
- [XMLDSig Validator](https://www.aleksey.com/xmlsec/xmldsig-verifier.html)
- [Hash Calculator](https://emn178.github.io/online-tools/sha256.html)

## Problemas Comunes

### "Restauración de paquetes NuGet falló"

```bash
# Limpiar caché de NuGet
dotnet nuget locals all --clear

# Restaurar de nuevo
dotnet restore
```

### "Tests no se descubren en Visual Studio"

1. Rebuild de la solución
2. Test Explorer → Refresh
3. Verificar que el proyecto de tests tiene el SDK correcto:
```xml
<Project Sdk="Microsoft.NET.Sdk">
```

### "Error CS0246: No se encuentra el tipo"

```bash
# Limpiar y rebuild
dotnet clean
dotnet build
```

## Contacto y Soporte

- **Issues**: [GitHub Issues](https://github.com/JoseRGWeb/Veri-factuSender/issues)
- **Discusiones**: [GitHub Discussions](https://github.com/JoseRGWeb/Veri-factuSender/discussions)

## Licencia

Consulta el archivo [LICENSE](../LICENSE) en la raíz del repositorio para información sobre la licencia del proyecto.

---

**Última actualización:** 30 de octubre de 2025
