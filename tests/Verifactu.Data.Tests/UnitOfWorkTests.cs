using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Verifactu.Data;
using Verifactu.Data.Entities;
using Xunit;

namespace Verifactu.Data.Tests;

/// <summary>
/// Tests para Unit of Work
/// </summary>
public class UnitOfWorkTests : IDisposable
{
    private readonly VerifactuDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<VerifactuDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new VerifactuDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
    }

    [Fact]
    public async Task SaveChangesAsync_DebeGuardarCambiosEnBaseDeDatos()
    {
        // Arrange
        var registro = new RegistroFacturacionEntity
        {
            Id = Guid.NewGuid(),
            Serie = "A",
            Numero = "001",
            FechaHoraExpedicionUTC = DateTime.UtcNow,
            Huella = "test-huella",
            EstadoEnvio = EstadoEnvio.Pendiente,
            XmlFirmado = "<xml>test</xml>",
            NifEmisor = "B12345678",
            NombreEmisor = "Test Emisor",
            ImporteTotal = 100.00m,
            CuotaTotal = 21.00m,
            FechaCreacion = DateTime.UtcNow,
            FechaModificacion = DateTime.UtcNow
        };

        // Act
        await _unitOfWork.RegistrosFacturacion.AddAsync(registro);
        var cambios = await _unitOfWork.SaveChangesAsync();

        // Assert
        Assert.Equal(1, cambios);
        var guardado = await _unitOfWork.RegistrosFacturacion.GetByIdAsync(registro.Id);
        Assert.NotNull(guardado);
    }

    [Fact]
    public async Task BeginTransactionAsync_CommitTransactionAsync_DebeConfirmarTransaccion()
    {
        // Arrange
        var registro1 = CrearRegistro("A", "001");
        var registro2 = CrearRegistro("A", "002");

        // Act
        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.RegistrosFacturacion.AddAsync(registro1);
        await _unitOfWork.RegistrosFacturacion.AddAsync(registro2);
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        var count = await _unitOfWork.RegistrosFacturacion.CountAsync();
        Assert.Equal(2, count);
    }

    [Fact(Skip = "Las transacciones no son soportadas por InMemory database. Este test requiere un provider real.")]
    public async Task RollbackTransactionAsync_DebeRevertirCambios()
    {
        // Arrange
        var registro1 = CrearRegistro("A", "001");
        
        // Primero guardamos un registro fuera de la transacción
        await _unitOfWork.RegistrosFacturacion.AddAsync(registro1);
        await _unitOfWork.SaveChangesAsync();

        var registro2 = CrearRegistro("A", "002");

        // Act
        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.RegistrosFacturacion.AddAsync(registro2);
        await _context.SaveChangesAsync(); // Guardamos pero no confirmamos
        await _unitOfWork.RollbackTransactionAsync();

        // Assert
        var count = await _unitOfWork.RegistrosFacturacion.CountAsync();
        Assert.Equal(1, count); // Solo el primero debe existir
    }

    [Fact]
    public void RegistrosFacturacion_DebeRetornarInstanciaDelRepositorio()
    {
        // Act
        var repository = _unitOfWork.RegistrosFacturacion;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public async Task MultiplesSaveChanges_DebeMantenerConteoCorreto()
    {
        // Arrange
        var registro1 = CrearRegistro("A", "001");
        var registro2 = CrearRegistro("A", "002");
        var registro3 = CrearRegistro("A", "003");

        // Act
        await _unitOfWork.RegistrosFacturacion.AddAsync(registro1);
        await _unitOfWork.SaveChangesAsync();

        await _unitOfWork.RegistrosFacturacion.AddAsync(registro2);
        await _unitOfWork.SaveChangesAsync();

        await _unitOfWork.RegistrosFacturacion.AddAsync(registro3);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var count = await _unitOfWork.RegistrosFacturacion.CountAsync();
        Assert.Equal(3, count);
    }

    private RegistroFacturacionEntity CrearRegistro(string serie, string numero)
    {
        return new RegistroFacturacionEntity
        {
            Id = Guid.NewGuid(),
            Serie = serie,
            Numero = numero,
            FechaHoraExpedicionUTC = DateTime.UtcNow,
            Huella = Guid.NewGuid().ToString("N"),
            EstadoEnvio = EstadoEnvio.Pendiente,
            XmlFirmado = "<xml>test</xml>",
            NifEmisor = "B12345678",
            NombreEmisor = "Test Emisor",
            ImporteTotal = 100.00m,
            CuotaTotal = 21.00m,
            FechaCreacion = DateTime.UtcNow,
            FechaModificacion = DateTime.UtcNow
        };
    }
}
