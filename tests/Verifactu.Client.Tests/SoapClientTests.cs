using System;
using System.Xml.Linq;
using Xunit;
using Verifactu.Client.Services;
using Verifactu.Client.Models;

namespace Verifactu.Client.Tests;

/// <summary>
/// Tests para el cliente SOAP de VERI*FACTU
/// </summary>
public class SoapClientTests
{
    [Fact]
    public void ParsearRespuestaSuministro_ConRespuestaCorrecta_DebeRetornarModelo()
    {
        // Arrange: XML de respuesta simulada basada en la estructura oficial de AEAT
        var xmlRespuesta = """
<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
  <soapenv:Body>
    <sfResp:RespuestaRegFactuSistemaFacturacion 
        xmlns:sfResp="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaSuministro.xsd"
        xmlns:sf="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd">
      <sfResp:CSV>ABC123456789WXYZ</sfResp:CSV>
      <sfResp:DatosPresentacion>
        <sf:NIFPresentador>12345678A</sf:NIFPresentador>
        <sf:TimestampPresentacion>2024-11-07T10:30:00+01:00</sf:TimestampPresentacion>
      </sfResp:DatosPresentacion>
      <sfResp:Cabecera>
        <sf:ObligadoEmision>
          <sf:NombreRazon>Empresa Ejemplo SL</sf:NombreRazon>
          <sf:NIF>B12345678</sf:NIF>
        </sf:ObligadoEmision>
      </sfResp:Cabecera>
      <sfResp:TiempoEsperaEnvio>60</sfResp:TiempoEsperaEnvio>
      <sfResp:EstadoEnvio>Correcto</sfResp:EstadoEnvio>
      <sfResp:RespuestaLinea>
        <sfResp:IDFactura>
          <sf:IDEmisorFactura>B12345678</sf:IDEmisorFactura>
          <sf:NumSerieFactura>FAC-2024-001</sf:NumSerieFactura>
          <sf:FechaExpedicionFactura>07-11-2024</sf:FechaExpedicionFactura>
        </sfResp:IDFactura>
        <sfResp:Operacion>
          <sfResp:TipoOperacion>Alta</sfResp:TipoOperacion>
        </sfResp:Operacion>
        <sfResp:EstadoRegistro>Correcto</sfResp:EstadoRegistro>
      </sfResp:RespuestaLinea>
    </sfResp:RespuestaRegFactuSistemaFacturacion>
  </soapenv:Body>
</soapenv:Envelope>
""";

        // Act
        var resultado = VerifactuSoapClient.ParsearRespuestaSuministro(xmlRespuesta);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("ABC123456789WXYZ", resultado.CSV);
        Assert.Equal(60, resultado.TiempoEsperaEnvio);
        Assert.Equal("Correcto", resultado.EstadoEnvio);
        
        Assert.NotNull(resultado.DatosPresentacion);
        Assert.Equal("12345678A", resultado.DatosPresentacion.NIFPresentador);
        
        Assert.NotNull(resultado.Cabecera);
        Assert.NotNull(resultado.Cabecera.ObligadoEmision);
        Assert.Equal("Empresa Ejemplo SL", resultado.Cabecera.ObligadoEmision.NombreRazon);
        Assert.Equal("B12345678", resultado.Cabecera.ObligadoEmision.NIF);
        
        Assert.NotNull(resultado.RespuestasLinea);
        Assert.Single(resultado.RespuestasLinea);
        Assert.Equal("Correcto", resultado.RespuestasLinea[0].EstadoRegistro);
        Assert.Equal("Alta", resultado.RespuestasLinea[0].Operacion?.TipoOperacion);
    }

    [Fact]
    public void ParsearRespuestaSuministro_ConErrores_DebeRetornarCodigosError()
    {
        // Arrange: Respuesta con error
        var xmlRespuesta = """
<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
  <soapenv:Body>
    <sfResp:RespuestaRegFactuSistemaFacturacion 
        xmlns:sfResp="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaSuministro.xsd"
        xmlns:sf="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd">
      <sfResp:TiempoEsperaEnvio>60</sfResp:TiempoEsperaEnvio>
      <sfResp:EstadoEnvio>Incorrecto</sfResp:EstadoEnvio>
      <sfResp:RespuestaLinea>
        <sfResp:IDFactura>
          <sf:IDEmisorFactura>B12345678</sf:IDEmisorFactura>
          <sf:NumSerieFactura>FAC-2024-001</sf:NumSerieFactura>
          <sf:FechaExpedicionFactura>07-11-2024</sf:FechaExpedicionFactura>
        </sfResp:IDFactura>
        <sfResp:EstadoRegistro>Incorrecto</sfResp:EstadoRegistro>
        <sfResp:CodigoErrorRegistro>4001</sfResp:CodigoErrorRegistro>
        <sfResp:DescripcionErrorRegistro>El NIF del emisor no está identificado</sfResp:DescripcionErrorRegistro>
      </sfResp:RespuestaLinea>
    </sfResp:RespuestaRegFactuSistemaFacturacion>
  </soapenv:Body>
</soapenv:Envelope>
""";

        // Act
        var resultado = VerifactuSoapClient.ParsearRespuestaSuministro(xmlRespuesta);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Incorrecto", resultado.EstadoEnvio);
        Assert.Null(resultado.CSV); // No se genera CSV si todo falla
        
        Assert.NotNull(resultado.RespuestasLinea);
        Assert.Single(resultado.RespuestasLinea);
        Assert.Equal("Incorrecto", resultado.RespuestasLinea[0].EstadoRegistro);
        Assert.Equal("4001", resultado.RespuestasLinea[0].CodigoErrorRegistro);
        Assert.Equal("El NIF del emisor no está identificado", resultado.RespuestasLinea[0].DescripcionErrorRegistro);
    }

    [Fact]
    public void ParsearRespuestaSuministro_ConRegistroDuplicado_DebeRetornarDatosDuplicado()
    {
        // Arrange: Respuesta con registro duplicado
        var xmlRespuesta = """
<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
  <soapenv:Body>
    <sfResp:RespuestaRegFactuSistemaFacturacion 
        xmlns:sfResp="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaSuministro.xsd"
        xmlns:sf="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd">
      <sfResp:EstadoEnvio>Incorrecto</sfResp:EstadoEnvio>
      <sfResp:RespuestaLinea>
        <sfResp:IDFactura>
          <sf:IDEmisorFactura>B12345678</sf:IDEmisorFactura>
          <sf:NumSerieFactura>FAC-2024-001</sf:NumSerieFactura>
          <sf:FechaExpedicionFactura>07-11-2024</sf:FechaExpedicionFactura>
        </sfResp:IDFactura>
        <sfResp:EstadoRegistro>Incorrecto</sfResp:EstadoRegistro>
        <sfResp:CodigoErrorRegistro>3001</sfResp:CodigoErrorRegistro>
        <sfResp:DescripcionErrorRegistro>Registro duplicado</sfResp:DescripcionErrorRegistro>
        <sfResp:RegistroDuplicado>
          <sfResp:IdPeticionRegistroDuplicado>20241107103000123456</sfResp:IdPeticionRegistroDuplicado>
          <sfResp:EstadoRegistroDuplicado>Correcta</sfResp:EstadoRegistroDuplicado>
        </sfResp:RegistroDuplicado>
      </sfResp:RespuestaLinea>
    </sfResp:RespuestaRegFactuSistemaFacturacion>
  </soapenv:Body>
</soapenv:Envelope>
""";

        // Act
        var resultado = VerifactuSoapClient.ParsearRespuestaSuministro(xmlRespuesta);

        // Assert
        Assert.NotNull(resultado);
        Assert.NotNull(resultado.RespuestasLinea);
        Assert.Single(resultado.RespuestasLinea);
        
        var linea = resultado.RespuestasLinea[0];
        Assert.NotNull(linea.RegistroDuplicado);
        Assert.Equal("20241107103000123456", linea.RegistroDuplicado.IdPeticionRegistroDuplicado);
        Assert.Equal("Correcta", linea.RegistroDuplicado.EstadoRegistroDuplicado);
    }

    [Fact]
    public void ParsearRespuestaSuministro_ConMultiplesRegistros_DebeRetornarTodos()
    {
        // Arrange: Respuesta con múltiples registros (aceptación parcial)
        var xmlRespuesta = """
<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
  <soapenv:Body>
    <sfResp:RespuestaRegFactuSistemaFacturacion 
        xmlns:sfResp="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaSuministro.xsd"
        xmlns:sf="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd">
      <sfResp:CSV>XYZ987654321ABCD</sfResp:CSV>
      <sfResp:TiempoEsperaEnvio>60</sfResp:TiempoEsperaEnvio>
      <sfResp:EstadoEnvio>ParcialmenteCorrecto</sfResp:EstadoEnvio>
      <sfResp:RespuestaLinea>
        <sfResp:IDFactura>
          <sf:IDEmisorFactura>B12345678</sf:IDEmisorFactura>
          <sf:NumSerieFactura>FAC-001</sf:NumSerieFactura>
          <sf:FechaExpedicionFactura>07-11-2024</sf:FechaExpedicionFactura>
        </sfResp:IDFactura>
        <sfResp:EstadoRegistro>Correcto</sfResp:EstadoRegistro>
      </sfResp:RespuestaLinea>
      <sfResp:RespuestaLinea>
        <sfResp:IDFactura>
          <sf:IDEmisorFactura>B12345678</sf:IDEmisorFactura>
          <sf:NumSerieFactura>FAC-002</sf:NumSerieFactura>
          <sf:FechaExpedicionFactura>07-11-2024</sf:FechaExpedicionFactura>
        </sfResp:IDFactura>
        <sfResp:EstadoRegistro>Incorrecto</sfResp:EstadoRegistro>
        <sfResp:CodigoErrorRegistro>5002</sfResp:CodigoErrorRegistro>
        <sfResp:DescripcionErrorRegistro>Fecha inválida</sfResp:DescripcionErrorRegistro>
      </sfResp:RespuestaLinea>
    </sfResp:RespuestaRegFactuSistemaFacturacion>
  </soapenv:Body>
</soapenv:Envelope>
""";

        // Act
        var resultado = VerifactuSoapClient.ParsearRespuestaSuministro(xmlRespuesta);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("ParcialmenteCorrecto", resultado.EstadoEnvio);
        Assert.Equal("XYZ987654321ABCD", resultado.CSV);
        
        Assert.NotNull(resultado.RespuestasLinea);
        Assert.Equal(2, resultado.RespuestasLinea.Count);
        
        // Primer registro: correcto
        Assert.Equal("Correcto", resultado.RespuestasLinea[0].EstadoRegistro);
        Assert.Null(resultado.RespuestasLinea[0].CodigoErrorRegistro);
        
        // Segundo registro: incorrecto
        Assert.Equal("Incorrecto", resultado.RespuestasLinea[1].EstadoRegistro);
        Assert.Equal("5002", resultado.RespuestasLinea[1].CodigoErrorRegistro);
    }

    [Fact]
    public void ParsearRespuestaConsultaLR_ConDatos_DebeRetornarModelo()
    {
        // Arrange: XML de respuesta de consulta
        var xmlRespuesta = """
<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
  <soapenv:Body>
    <conResp:RespuestaConsultaFactuSistemaFacturacion
        xmlns:conResp="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaConsultaLR.xsd"
        xmlns:sf="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd">
      <conResp:Cabecera>
        <sf:IDVersion>1.0</sf:IDVersion>
      </conResp:Cabecera>
      <conResp:PeriodoImputacion>
        <conResp:Ejercicio>2024</conResp:Ejercicio>
        <conResp:Periodo>11</conResp:Periodo>
      </conResp:PeriodoImputacion>
      <conResp:IndicadorPaginacion>N</conResp:IndicadorPaginacion>
      <conResp:ResultadoConsulta>ConDatos</conResp:ResultadoConsulta>
      <conResp:RegistroRespuestaConsultaFactuSistemaFacturacion>
        <conResp:IDFactura>
          <sf:IDEmisorFactura>B12345678</sf:IDEmisorFactura>
          <sf:NumSerieFactura>FAC-2024-100</sf:NumSerieFactura>
          <sf:FechaExpedicionFactura>05-11-2024</sf:FechaExpedicionFactura>
        </conResp:IDFactura>
        <conResp:DatosRegistroFacturacion>
          <conResp:TipoFactura>F1</conResp:TipoFactura>
          <conResp:DescripcionOperacion>Venta de productos</conResp:DescripcionOperacion>
          <conResp:CuotaTotal>210.50</conResp:CuotaTotal>
          <conResp:ImporteTotal>1210.50</conResp:ImporteTotal>
          <conResp:Huella>ABC123DEF456789</conResp:Huella>
          <conResp:FechaHoraHusoGenRegistro>2024-11-05T14:30:00+01:00</conResp:FechaHoraHusoGenRegistro>
        </conResp:DatosRegistroFacturacion>
      </conResp:RegistroRespuestaConsultaFactuSistemaFacturacion>
    </conResp:RespuestaConsultaFactuSistemaFacturacion>
  </soapenv:Body>
</soapenv:Envelope>
""";

        // Act
        var resultado = VerifactuSoapClient.ParsearRespuestaConsultaLR(xmlRespuesta);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("ConDatos", resultado.ResultadoConsulta);
        Assert.Equal("N", resultado.IndicadorPaginacion);
        
        Assert.NotNull(resultado.PeriodoImputacion);
        Assert.Equal(2024, resultado.PeriodoImputacion.Ejercicio);
        Assert.Equal("11", resultado.PeriodoImputacion.Periodo);
        
        Assert.NotNull(resultado.RegistrosRespuesta);
        Assert.Single(resultado.RegistrosRespuesta);
        
        var registro = resultado.RegistrosRespuesta[0];
        Assert.NotNull(registro.IDFactura);
        Assert.Equal("B12345678", registro.IDFactura.IDEmisorFactura);
        Assert.Equal("FAC-2024-100", registro.IDFactura.NumSerieFactura);
        
        Assert.NotNull(registro.DatosRegistroFacturacion);
        Assert.Equal("F1", registro.DatosRegistroFacturacion.TipoFactura);
        Assert.Equal(210.50m, registro.DatosRegistroFacturacion.CuotaTotal);
        Assert.Equal(1210.50m, registro.DatosRegistroFacturacion.ImporteTotal);
        Assert.Equal("ABC123DEF456789", registro.DatosRegistroFacturacion.Huella);
    }

    [Fact]
    public void ParsearRespuestaConsultaLR_SinDatos_DebeRetornarResultadoVacio()
    {
        // Arrange: Respuesta sin datos
        var xmlRespuesta = """
<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
  <soapenv:Body>
    <conResp:RespuestaConsultaFactuSistemaFacturacion
        xmlns:conResp="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaConsultaLR.xsd"
        xmlns:sf="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd">
      <conResp:Cabecera>
        <sf:IDVersion>1.0</sf:IDVersion>
      </conResp:Cabecera>
      <conResp:PeriodoImputacion>
        <conResp:Ejercicio>2024</conResp:Ejercicio>
        <conResp:Periodo>10</conResp:Periodo>
      </conResp:PeriodoImputacion>
      <conResp:IndicadorPaginacion>N</conResp:IndicadorPaginacion>
      <conResp:ResultadoConsulta>SinDatos</conResp:ResultadoConsulta>
    </conResp:RespuestaConsultaFactuSistemaFacturacion>
  </soapenv:Body>
</soapenv:Envelope>
""";

        // Act
        var resultado = VerifactuSoapClient.ParsearRespuestaConsultaLR(xmlRespuesta);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("SinDatos", resultado.ResultadoConsulta);
        Assert.Equal("N", resultado.IndicadorPaginacion);
        Assert.Empty(resultado.RegistrosRespuesta);
    }

    [Fact]
    public void ParsearRespuestaSuministro_ConXmlInvalido_DebeLanzarExcepcion()
    {
        // Arrange
        var xmlInvalido = "<invalid>xml</invalid>";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            VerifactuSoapClient.ParsearRespuestaSuministro(xmlInvalido));
    }

    [Fact]
    public void ParsearRespuestaConsultaLR_ConXmlInvalido_DebeLanzarExcepcion()
    {
        // Arrange
        var xmlInvalido = "<invalid>xml</invalid>";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            VerifactuSoapClient.ParsearRespuestaConsultaLR(xmlInvalido));
    }
}
