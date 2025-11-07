using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Verifactu.Data;
using Verifactu.Data.Entities;
using Verifactu.Data.Repositories;
using Xunit;

namespace Verifactu.Data.Tests;

/// <summary>
/// Tests para el repositorio de registros de facturación
/// </summary>
public class RegistroFacturacionRepositoryTests : IDisposable
{
    private readonly VerifactuDbContext _context;
    private readonly IRegistroFacturacionRepository _repository;

    public RegistroFacturacionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<VerifactuDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new VerifactuDbContext(options);
        _repository = new RegistroFacturacionRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task AddAsync_DebeCrearRegistroCorrectamente()
    {
        // Arrange
        var registro = CrearRegistroFacturacion("A", "001");

        // Act
        await _repository.AddAsync(registro);
        await _context.SaveChangesAsync();

        // Assert
        var registroGuardado = await _repository.GetByIdAsync(registro.Id);
        Assert.NotNull(registroGuardado);
        Assert.Equal("A", registroGuardado.Serie);
        Assert.Equal("001", registroGuardado.Numero);
    }

    [Fact]
    public async Task GetBySerieNumeroAsync_DebeEncontrarRegistroExistente()
    {
        // Arrange
        var registro = CrearRegistroFacturacion("B", "002");
        await _repository.AddAsync(registro);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.GetBySerieNumeroAsync("B", "002");

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(registro.Id, resultado.Id);
    }

    [Fact]
    public async Task GetByEstadoAsync_DebeRetornarSoloRegistrosConEstadoEspecifico()
    {
        // Arrange
        await _repository.AddAsync(CrearRegistroFacturacion("A", "001", EstadoEnvio.Pendiente));
        await _repository.AddAsync(CrearRegistroFacturacion("A", "002", EstadoEnvio.Correcto));
        await _repository.AddAsync(CrearRegistroFacturacion("A", "003", EstadoEnvio.Pendiente));
        await _context.SaveChangesAsync();

        // Act
        var pendientes = await _repository.GetByEstadoAsync(EstadoEnvio.Pendiente);

        // Assert
        Assert.Equal(2, pendientes.Count());
        Assert.All(pendientes, r => Assert.Equal(EstadoEnvio.Pendiente, r.EstadoEnvio));
    }

    [Fact]
    public async Task GetPendientesEnvioAsync_DebeRetornarPendientesYErroresTemporales()
    {
        // Arrange
        await _repository.AddAsync(CrearRegistroFacturacion("A", "001", EstadoEnvio.Pendiente));
        await _repository.AddAsync(CrearRegistroFacturacion("A", "002", EstadoEnvio.ErrorTemporal));
        await _repository.AddAsync(CrearRegistroFacturacion("A", "003", EstadoEnvio.Correcto));
        await _repository.AddAsync(CrearRegistroFacturacion("A", "004", EstadoEnvio.Rechazado));
        await _context.SaveChangesAsync();

        // Act
        var pendientes = await _repository.GetPendientesEnvioAsync();

        // Assert
        Assert.Equal(2, pendientes.Count());
    }

    [Fact]
    public async Task GetUltimoRegistroCorretoAsync_DebeRetornarRegistroMasReciente()
    {
        // Arrange
        var fecha1 = DateTime.UtcNow.AddDays(-2);
        var fecha2 = DateTime.UtcNow.AddDays(-1);
        var fecha3 = DateTime.UtcNow;

        await _repository.AddAsync(CrearRegistroFacturacion("A", "001", EstadoEnvio.Correcto, fecha1));
        await _repository.AddAsync(CrearRegistroFacturacion("A", "002", EstadoEnvio.Correcto, fecha2));
        var registroReciente = CrearRegistroFacturacion("A", "003", EstadoEnvio.Correcto, fecha3);
        await _repository.AddAsync(registroReciente);
        await _context.SaveChangesAsync();

        // Act
        var ultimo = await _repository.GetUltimoRegistroCorretoAsync();

        // Assert
        Assert.NotNull(ultimo);
        Assert.Equal(registroReciente.Id, ultimo.Id);
        Assert.Equal("003", ultimo.Numero);
    }

    [Fact]
    public async Task GetByFechaRangoAsync_DebeRetornarRegistrosEnRango()
    {
        // Arrange
        var desde = DateTime.UtcNow.AddDays(-5);
        var hasta = DateTime.UtcNow.AddDays(-1);

        await _repository.AddAsync(CrearRegistroFacturacion("A", "001", EstadoEnvio.Pendiente, DateTime.UtcNow.AddDays(-10)));
        await _repository.AddAsync(CrearRegistroFacturacion("A", "002", EstadoEnvio.Pendiente, DateTime.UtcNow.AddDays(-3)));
        await _repository.AddAsync(CrearRegistroFacturacion("A", "003", EstadoEnvio.Pendiente, DateTime.UtcNow.AddDays(-2)));
        await _repository.AddAsync(CrearRegistroFacturacion("A", "004", EstadoEnvio.Pendiente, DateTime.UtcNow));
        await _context.SaveChangesAsync();

        // Act
        var enRango = await _repository.GetByFechaRangoAsync(desde, hasta);

        // Assert
        Assert.Equal(2, enRango.Count());
    }

    [Fact]
    public async Task GetParaReintentoAsync_DebeRetornarSoloErroresTemporalesConReintentosBajoLimite()
    {
        // Arrange
        var r1 = CrearRegistroFacturacion("A", "001", EstadoEnvio.ErrorTemporal);
        r1.Reintentos = 1;
        
        var r2 = CrearRegistroFacturacion("A", "002", EstadoEnvio.ErrorTemporal);
        r2.Reintentos = 2;
        
        var r3 = CrearRegistroFacturacion("A", "003", EstadoEnvio.ErrorTemporal);
        r3.Reintentos = 3;
        
        var r4 = CrearRegistroFacturacion("A", "004", EstadoEnvio.Pendiente);
        r4.Reintentos = 0;

        await _repository.AddAsync(r1);
        await _repository.AddAsync(r2);
        await _repository.AddAsync(r3);
        await _repository.AddAsync(r4);
        await _context.SaveChangesAsync();

        // Act
        var paraReintento = await _repository.GetParaReintentoAsync(maxReintentos: 3);

        // Assert
        Assert.Equal(2, paraReintento.Count());
        Assert.All(paraReintento, r => Assert.True(r.Reintentos < 3));
    }

    [Fact]
    public async Task Update_DebeActualizarRegistroCorrectamente()
    {
        // Arrange
        var registro = CrearRegistroFacturacion("A", "001", EstadoEnvio.Pendiente);
        await _repository.AddAsync(registro);
        await _context.SaveChangesAsync();

        // Act
        registro.EstadoEnvio = EstadoEnvio.Correcto;
        registro.CSV = "CSV123456";
        _repository.Update(registro);
        await _context.SaveChangesAsync();

        // Assert
        var actualizado = await _repository.GetByIdAsync(registro.Id);
        Assert.NotNull(actualizado);
        Assert.Equal(EstadoEnvio.Correcto, actualizado.EstadoEnvio);
        Assert.Equal("CSV123456", actualizado.CSV);
    }

    [Fact]
    public async Task CountAsync_DebeDevolverConteoCorreto()
    {
        // Arrange
        await _repository.AddAsync(CrearRegistroFacturacion("A", "001", EstadoEnvio.Pendiente));
        await _repository.AddAsync(CrearRegistroFacturacion("A", "002", EstadoEnvio.Correcto));
        await _repository.AddAsync(CrearRegistroFacturacion("A", "003", EstadoEnvio.Pendiente));
        await _context.SaveChangesAsync();

        // Act
        var total = await _repository.CountAsync();
        var pendientes = await _repository.CountAsync(r => r.EstadoEnvio == EstadoEnvio.Pendiente);

        // Assert
        Assert.Equal(3, total);
        Assert.Equal(2, pendientes);
    }

    private RegistroFacturacionEntity CrearRegistroFacturacion(
        string serie, 
        string numero, 
        EstadoEnvio estado = EstadoEnvio.Pendiente,
        DateTime? fecha = null)
    {
        return new RegistroFacturacionEntity
        {
            Id = Guid.NewGuid(),
            Serie = serie,
            Numero = numero,
            FechaHoraExpedicionUTC = fecha ?? DateTime.UtcNow,
            Huella = Guid.NewGuid().ToString("N"),
            EstadoEnvio = estado,
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
