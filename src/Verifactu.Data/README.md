# Verifactu.Data

Capa de persistencia y auditoría para Veri-factuSender que implementa Entity Framework Core con soporte para múltiples proveedores de base de datos.

## 📋 Características

- **Soporte multi-base de datos**: SQLite, SQL Server, PostgreSQL
- **Patrón Repository**: Repositorio genérico con operaciones CRUD
- **Unit of Work**: Gestión de transacciones y coordinación de repositorios
- **Auditoría automática**: Seguimiento de fechas de creación y modificación
- **Migraciones**: Control de versiones de esquema de base de datos
- **Índices optimizados**: Para consultas frecuentes

## 🏗️ Estructura

```
Verifactu.Data/
├── Entities/               # Entidades de base de datos
│   ├── EstadoEnvio.cs     # Enum de estados de envío
│   └── RegistroFacturacionEntity.cs
├── Repositories/          # Implementación de repositorios
│   ├── IRepository.cs
│   ├── Repository.cs
│   ├── IRegistroFacturacionRepository.cs
│   └── RegistroFacturacionRepository.cs
├── Migrations/            # Migraciones EF Core
├── Configuration/         # Configuración y extensiones
│   └── ServiceCollectionExtensions.cs
├── VerifactuDbContext.cs  # Contexto de base de datos
├── IUnitOfWork.cs        # Interfaz Unit of Work
└── UnitOfWork.cs         # Implementación Unit of Work
```

## 🚀 Uso

### Configuración con SQLite

```csharp
services.AddVerifactuDataSqlite("Data Source=verifactu.db");
```

### Configuración con SQL Server

```csharp
services.AddVerifactuDataSqlServer("Server=localhost;Database=Verifactu;...");
```

### Configuración con PostgreSQL

```csharp
services.AddVerifactuDataPostgreSQL("Host=localhost;Database=verifactu;...");
```

### Uso del Unit of Work

```csharp
public class FacturacionService
{
    private readonly IUnitOfWork _unitOfWork;

    public FacturacionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task GuardarRegistroAsync(RegistroFacturacionEntity registro)
    {
        await _unitOfWork.RegistrosFacturacion.AddAsync(registro);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<RegistroFacturacionEntity?> ObtenerPorSerieNumeroAsync(string serie, string numero)
    {
        return await _unitOfWork.RegistrosFacturacion.GetBySerieNumeroAsync(serie, numero);
    }
}
```

### Uso con Transacciones

```csharp
public async Task ProcesarLoteAsync(List<RegistroFacturacionEntity> registros)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        foreach (var registro in registros)
        {
            await _unitOfWork.RegistrosFacturacion.AddAsync(registro);
        }
        
        await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

## 📊 Modelo de Datos

### RegistroFacturacionEntity

Tabla principal que almacena los registros de facturación enviados a AEAT.

**Campos principales:**
- `Id` (Guid): Identificador único
- `Serie` / `Numero`: Identificación de la factura
- `FechaHoraExpedicionUTC`: Fecha de expedición
- `Huella`: Hash SHA-256 del registro
- `HuellaAnterior`: Hash del registro anterior (encadenamiento)
- `EstadoEnvio`: Estado actual (Pendiente, Correcto, Rechazado, etc.)
- `XmlFirmado`: XML firmado enviado a AEAT
- `AcuseRecibo`: Respuesta de AEAT
- `CSV`: Código Seguro de Verificación
- `Reintentos`: Número de reintentos realizados

**Índices:**
- Único en (`Serie`, `Numero`)
- `FechaHoraExpedicionUTC`
- `EstadoEnvio`
- `Huella`
- `NifEmisor`

## 🗄️ Migraciones

### Crear una nueva migración

```bash
cd src/Verifactu.Data
dotnet ef migrations add NombreMigracion
```

### Aplicar migraciones

```bash
dotnet ef database update
```

### Eliminar última migración

```bash
dotnet ef migrations remove
```

## 🧪 Tests

El proyecto incluye tests unitarios completos:

```bash
dotnet test tests/Verifactu.Data.Tests/
```

**Tests incluidos:**
- Operaciones CRUD del repositorio
- Consultas específicas por estado, fecha, etc.
- Unit of Work y transacciones
- Auditoría automática

## 📦 Dependencias

- Microsoft.EntityFrameworkCore (9.0.0)
- Microsoft.EntityFrameworkCore.Sqlite (9.0.0)
- Microsoft.EntityFrameworkCore.SqlServer (9.0.0)
- Npgsql.EntityFrameworkCore.PostgreSQL (9.0.0)
- Microsoft.EntityFrameworkCore.Design (9.0.0)

## 📝 Notas

- La auditoría (FechaCreacion, FechaModificacion) se actualiza automáticamente
- Los estados de envío se almacenan como enteros para compatibilidad
- Las transacciones son opcionales y dependen del proveedor de BD
- El diseño soporta extensión para añadir nuevas entidades
