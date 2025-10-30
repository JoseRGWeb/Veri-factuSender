# Guía de Pruebas Funcionales End-to-End

Esta guía proporciona instrucciones detalladas paso a paso para ejecutar pruebas de funcionamiento completas contra el portal de pruebas de la AEAT, incluyendo comandos, ejemplos de payloads y manejo de errores.

## Índice

1. [Introducción](#introducción)
2. [Prerequisitos](#prerequisitos)
3. [Configuración Inicial](#configuración-inicial)
4. [Escenarios de Prueba](#escenarios-de-prueba)
5. [Comandos y Ejemplos](#comandos-y-ejemplos)
6. [Payloads de Ejemplo](#payloads-de-ejemplo)
7. [Verificación de Resultados](#verificación-de-resultados)
8. [Manejo de Errores](#manejo-de-errores)
9. [Automatización de Pruebas](#automatización-de-pruebas)
10. [Resolución de Problemas](#resolución-de-problemas)

---

## Introducción

### Objetivo de las Pruebas End-to-End

Las pruebas end-to-end (E2E) verifican el flujo completo de envío de registros de facturación a VERI*FACTU, desde la creación de una factura hasta la recepción de la confirmación de la AEAT.

### Alcance

Estas pruebas cubren:
- ✅ Creación y validación de facturas
- ✅ Cálculo de huellas y encadenamiento
- ✅ Serialización a XML conforme a XSD
- ✅ Firma electrónica con certificado digital
- ✅ Envío por SOAP al portal de pruebas AEAT
- ✅ Procesamiento de respuestas y errores
- ✅ Consulta de registros enviados
- ✅ Validación de códigos QR

### Entorno de Pruebas

⚠️ **Importante**: Estas pruebas se ejecutan contra el **entorno sandbox** de la AEAT, que no tiene validez tributaria.

**Endpoint de pruebas**: `https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion`

---

## Prerequisitos

### Software Necesario

Antes de comenzar, asegúrate de tener instalado:

```bash
# Verificar instalación de .NET 9 SDK
dotnet --version
# Debe mostrar: 9.0.x o superior

# Verificar Git
git --version

# Verificar OpenSSL (para gestión de certificados)
openssl version
```

### Certificado Digital

Necesitas un **certificado digital válido** en formato PFX:

```bash
# Verificar que tienes el certificado
ls -lh /ruta/a/tu-certificado.pfx

# Verificar contenido del certificado
openssl pkcs12 -in /ruta/a/tu-certificado.pfx -nokeys -info
# Introduce la contraseña cuando se solicite
```

**Requisitos del certificado**:
- ✅ Formato PFX/P12 con clave privada
- ✅ Válido (no caducado ni revocado)
- ✅ Emitido por autoridad certificadora reconocida
- ✅ Protegido con contraseña segura

### Acceso al Portal de Pruebas

Verifica acceso al portal:

```bash
# Probar conectividad al endpoint
curl -v https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion 2>&1 | grep "Connected"

# Probar acceso al WSDL
curl -I https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/SistemaFacturacion.wsdl
```

### Clonar el Repositorio

```bash
# Clonar el proyecto
git clone https://github.com/JoseRGWeb/Veri-factuSender.git
cd Veri-factuSender

# Verificar estructura
ls -la
```

---

## Configuración Inicial

### Paso 1: Compilar el Proyecto

```bash
# Desde la raíz del repositorio
dotnet build

# Verificar que no hay errores
# Debe mostrar: Build succeeded
```

### Paso 2: Ejecutar Tests Unitarios

```bash
# Ejecutar todos los tests
dotnet test

# Resultado esperado:
# Test summary: total: X, failed: 0, succeeded: X, skipped: 0
```

### Paso 3: Configurar Variables de Entorno

#### Linux/macOS

```bash
# Crear archivo de variables de entorno
cat > ~/.verifactu-test.env << 'EOF'
export VERIFACTU_ENV="Sandbox"
export VERIFACTU_CERT_PATH="/home/usuario/certificados/certificado-pruebas.pfx"
export VERIFACTU_CERT_PASSWORD="TuPasswordSegura"
export VERIFACTU_ENDPOINT_URL="https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion"
export VERIFACTU_SOAP_ACTION="RegFacturacionAlta"
export VERIFACTU_LOG_LEVEL="Debug"
export VERIFACTU_HUELLA_ANTERIOR=""
EOF

# Cargar variables
source ~/.verifactu-test.env

# Verificar
echo $VERIFACTU_ENDPOINT_URL
```

#### Windows PowerShell

```powershell
# Crear script de configuración
@"
`$env:VERIFACTU_ENV="Sandbox"
`$env:VERIFACTU_CERT_PATH="C:\Certificados\certificado-pruebas.pfx"
`$env:VERIFACTU_CERT_PASSWORD="TuPasswordSegura"
`$env:VERIFACTU_ENDPOINT_URL="https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion"
`$env:VERIFACTU_SOAP_ACTION="RegFacturacionAlta"
`$env:VERIFACTU_LOG_LEVEL="Debug"
`$env:VERIFACTU_HUELLA_ANTERIOR=""
"@ | Out-File -FilePath $env:USERPROFILE\.verifactu-test.ps1

# Cargar variables
. $env:USERPROFILE\.verifactu-test.ps1

# Verificar
Write-Host $env:VERIFACTU_ENDPOINT_URL
```

### Paso 4: Configurar User Secrets (Recomendado)

```bash
# Navegar al proyecto de consola
cd src/Verifactu.ConsoleDemo

# Inicializar user secrets
dotnet user-secrets init

# Configurar certificado
dotnet user-secrets set "Certificado:PfxPath" "/ruta/a/tu-certificado.pfx"
dotnet user-secrets set "Certificado:PfxPassword" "TuPasswordSegura"

# Configurar endpoint
dotnet user-secrets set "Verifactu:EndpointUrl" "https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion"
dotnet user-secrets set "Verifactu:SoapAction" "RegFacturacionAlta"
dotnet user-secrets set "Verifactu:HuellaAnterior" ""

# Listar secretos configurados
dotnet user-secrets list
```

### Paso 5: Verificar Configuración

```bash
# Desde src/Verifactu.ConsoleDemo
dotnet run --help

# Verificar que el certificado se carga correctamente
# (El programa debe arrancar sin errores de certificado)
```

---

## Escenarios de Prueba

### Matriz de Escenarios

| ID | Escenario | Descripción | Resultado Esperado |
|----|-----------|-------------|-------------------|
| E2E-001 | Envío de factura simple | Una factura básica con una línea | Aceptada por AEAT |
| E2E-002 | Envío de factura múltiples líneas | Factura con 3+ líneas diferentes | Aceptada por AEAT |
| E2E-003 | Encadenamiento de registros | Envío secuencial de 3 facturas | Huellas encadenadas correctamente |
| E2E-004 | Factura con datos inválidos | NIF incorrecto | Rechazada con código de error |
| E2E-005 | Registro duplicado | Mismo UUID enviado dos veces | Detectado como duplicado |
| E2E-006 | Timeout de conexión | Timeout configurado muy corto | Error de timeout controlado |
| E2E-007 | Certificado inválido | Certificado expirado | Error de autenticación |
| E2E-008 | XML mal formado | XML que no cumple XSD | Error de validación |
| E2E-009 | Consulta de registro | Consultar registro previamente enviado | Registro encontrado |
| E2E-010 | Validación de QR | Generar y validar código QR | QR válido |

---

## Comandos y Ejemplos

### E2E-001: Envío de Factura Simple

**Objetivo**: Enviar una factura básica de prueba al sandbox de AEAT.

#### Paso 1: Preparar la Factura

```bash
cd src/Verifactu.ConsoleDemo

# Crear factura de prueba simple
cat > factura-e2e-001.json << 'EOF'
{
  "Serie": "TEST-E2E",
  "Numero": "001",
  "FechaEmision": "2025-10-30T10:00:00",
  "Emisor": {
    "Nif": "B12345678",
    "Nombre": "EMPRESA PRUEBAS E2E S.L.",
    "Direccion": "C/ Pruebas 1",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Receptor": {
    "Nif": "12345678Z",
    "Nombre": "CLIENTE PRUEBA E2E",
    "Direccion": "Av. Test 2",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Lineas": [
    {
      "Descripcion": "Servicio de prueba E2E",
      "Cantidad": 1,
      "PrecioUnitario": 100.0,
      "TipoImpositivo": 21.0
    }
  ],
  "Totales": {
    "BaseImponible": 100.0,
    "CuotaImpuestos": 21.0,
    "ImporteTotal": 121.0
  },
  "Moneda": "EUR",
  "Observaciones": "Factura de prueba E2E-001"
}
EOF
```

#### Paso 2: Ejecutar el Envío

```bash
# Configurar entorno
export ASPNETCORE_ENVIRONMENT=Sandbox

# Ejecutar con la factura de prueba
dotnet run -- --factura factura-e2e-001.json

# O si usas PowerShell en Windows:
# $env:ASPNETCORE_ENVIRONMENT="Sandbox"
# dotnet run -- --factura factura-e2e-001.json
```

#### Paso 3: Verificar el Resultado

```bash
# El output debe mostrar algo similar a:
# ========================================
# VerifactuSender - Demo de envío
# ========================================
# [INFO] Cargando configuración...
# [INFO] Cargando certificado...
# [INFO] Leyendo factura: factura-e2e-001.json
# [INFO] Calculando huella...
# [INFO] Serializando a XML...
# [INFO] Firmando XML...
# [INFO] Enviando por SOAP...
# [SUCCESS] Registro enviado correctamente
# [INFO] UUID: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
# [INFO] Huella: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

**Resultado Esperado**: 
- ✅ Código de respuesta: 200 OK
- ✅ Estado: Aceptado
- ✅ UUID asignado
- ✅ Huella calculada

### E2E-002: Envío de Factura con Múltiples Líneas

**Objetivo**: Verificar el procesamiento correcto de facturas complejas.

#### Payload

```bash
cat > factura-e2e-002.json << 'EOF'
{
  "Serie": "TEST-E2E",
  "Numero": "002",
  "FechaEmision": "2025-10-30T11:00:00",
  "Emisor": {
    "Nif": "B12345678",
    "Nombre": "EMPRESA PRUEBAS E2E S.L.",
    "Direccion": "C/ Pruebas 1",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Receptor": {
    "Nif": "87654321B",
    "Nombre": "CLIENTE EMPRESARIAL",
    "Direccion": "Plaza Empresa 10",
    "Provincia": "Barcelona",
    "Municipio": "Barcelona",
    "Pais": "ES"
  },
  "Lineas": [
    {
      "Descripcion": "Consultoría técnica",
      "Cantidad": 10,
      "PrecioUnitario": 75.0,
      "TipoImpositivo": 21.0
    },
    {
      "Descripcion": "Desarrollo software",
      "Cantidad": 20,
      "PrecioUnitario": 50.0,
      "TipoImpositivo": 21.0
    },
    {
      "Descripcion": "Soporte técnico",
      "Cantidad": 5,
      "PrecioUnitario": 40.0,
      "TipoImpositivo": 21.0
    },
    {
      "Descripcion": "Material didáctico",
      "Cantidad": 15,
      "PrecioUnitario": 10.0,
      "TipoImpositivo": 21.0
    }
  ],
  "Totales": {
    "BaseImponible": 2100.0,
    "CuotaImpuestos": 441.0,
    "ImporteTotal": 2541.0
  },
  "Moneda": "EUR",
  "Observaciones": "Factura de prueba E2E-002 con múltiples líneas"
}
EOF
```

#### Comando

```bash
dotnet run -- --factura factura-e2e-002.json
```

**Resultado Esperado**: 
- ✅ Todas las líneas procesadas correctamente
- ✅ Totales calculados correctamente
- ✅ Aceptado por AEAT

---

### E2E-003: Encadenamiento de Registros

**Objetivo**: Verificar el encadenamiento correcto de huellas entre registros sucesivos.

#### Script de Prueba

```bash
# Crear script de encadenamiento
cat > test-encadenamiento.sh << 'EOFSCRIPT'
#!/bin/bash

echo "=== PRUEBA E2E-003: Encadenamiento de Registros ==="

# Variables
HUELLA_ANTERIOR=""

# Función para enviar factura
enviar_factura() {
    local numero=$1
    local huella_ant=$2
    
    echo ""
    echo "--- Enviando factura $numero ---"
    
    # Actualizar huella anterior en configuración
    dotnet user-secrets set "Verifactu:HuellaAnterior" "$huella_ant"
    
    # Enviar factura
    dotnet run -- --factura factura-e2e-003-$numero.json > resultado-$numero.txt
    
    # Extraer nueva huella (adaptar según formato de salida)
    NUEVA_HUELLA=$(grep "Huella:" resultado-$numero.txt | cut -d' ' -f2)
    
    echo "Huella calculada: $NUEVA_HUELLA"
    
    echo $NUEVA_HUELLA
}

# Crear facturas
for i in 1 2 3; do
    cat > factura-e2e-003-$i.json << EOF
{
  "Serie": "CHAIN",
  "Numero": "00$i",
  "FechaEmision": "2025-10-30T12:0${i}:00",
  "Emisor": {
    "Nif": "B12345678",
    "Nombre": "EMPRESA PRUEBAS E2E S.L.",
    "Direccion": "C/ Pruebas 1",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Receptor": {
    "Nif": "12345678Z",
    "Nombre": "CLIENTE PRUEBA E2E",
    "Direccion": "Av. Test 2",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Lineas": [
    {
      "Descripcion": "Servicio encadenado $i",
      "Cantidad": 1,
      "PrecioUnitario": $(($i * 100)).0,
      "TipoImpositivo": 21.0
    }
  ],
  "Totales": {
    "BaseImponible": $(($i * 100)).0,
    "CuotaImpuestos": $(($i * 21)).0,
    "ImporteTotal": $(($i * 121)).0
  },
  "Moneda": "EUR",
  "Observaciones": "Factura encadenada $i de 3"
}
EOF
done

# Enviar primera factura (sin huella anterior)
HUELLA_1=$(enviar_factura 1 "")
sleep 2

# Enviar segunda factura (con huella de la primera)
HUELLA_2=$(enviar_factura 2 "$HUELLA_1")
sleep 2

# Enviar tercera factura (con huella de la segunda)
HUELLA_3=$(enviar_factura 3 "$HUELLA_2")

echo ""
echo "=== RESUMEN DE ENCADENAMIENTO ==="
echo "Huella 1: $HUELLA_1"
echo "Huella 2: $HUELLA_2 (encadenada con 1)"
echo "Huella 3: $HUELLA_3 (encadenada con 2)"
echo ""
echo "✅ Prueba completada. Verificar que las 3 facturas fueron aceptadas."
EOFSCRIPT

chmod +x test-encadenamiento.sh
```

#### Ejecutar

```bash
# Ejecutar el script de encadenamiento
./test-encadenamiento.sh
```

**Resultado Esperado**:
- ✅ Las 3 facturas son aceptadas
- ✅ Cada huella es diferente
- ✅ El sistema AEAT confirma el encadenamiento correcto

---

### E2E-004: Manejo de Datos Inválidos

**Objetivo**: Verificar que los errores de validación se manejan correctamente.

#### Factura con NIF Inválido

```bash
cat > factura-e2e-004-nif-invalido.json << 'EOF'
{
  "Serie": "ERR",
  "Numero": "001",
  "FechaEmision": "2025-10-30T13:00:00",
  "Emisor": {
    "Nif": "INVALIDO",
    "Nombre": "EMPRESA PRUEBAS E2E S.L.",
    "Direccion": "C/ Pruebas 1",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Receptor": {
    "Nif": "12345678Z",
    "Nombre": "CLIENTE PRUEBA",
    "Direccion": "Av. Test 2",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Lineas": [
    {
      "Descripcion": "Servicio de prueba",
      "Cantidad": 1,
      "PrecioUnitario": 100.0,
      "TipoImpositivo": 21.0
    }
  ],
  "Totales": {
    "BaseImponible": 100.0,
    "CuotaImpuestos": 21.0,
    "ImporteTotal": 121.0
  },
  "Moneda": "EUR"
}
EOF
```

#### Ejecutar y Capturar Error

```bash
dotnet run -- --factura factura-e2e-004-nif-invalido.json 2>&1 | tee error-e2e-004.log

# Verificar código de error
grep "ERROR" error-e2e-004.log
```

**Resultado Esperado**:
- ❌ Factura rechazada
- ✅ Código de error devuelto por AEAT
- ✅ Mensaje de error descriptivo
- ✅ Aplicación no se cuelga

#### Factura con Totales Incorrectos

```bash
cat > factura-e2e-004-totales-incorrectos.json << 'EOF'
{
  "Serie": "ERR",
  "Numero": "002",
  "FechaEmision": "2025-10-30T13:15:00",
  "Emisor": {
    "Nif": "B12345678",
    "Nombre": "EMPRESA PRUEBAS E2E S.L.",
    "Direccion": "C/ Pruebas 1",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Receptor": {
    "Nif": "12345678Z",
    "Nombre": "CLIENTE PRUEBA",
    "Direccion": "Av. Test 2",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Lineas": [
    {
      "Descripcion": "Servicio de prueba",
      "Cantidad": 1,
      "PrecioUnitario": 100.0,
      "TipoImpositivo": 21.0
    }
  ],
  "Totales": {
    "BaseImponible": 100.0,
    "CuotaImpuestos": 21.0,
    "ImporteTotal": 999.99
  },
  "Moneda": "EUR"
}
EOF

dotnet run -- --factura factura-e2e-004-totales-incorrectos.json 2>&1 | tee error-e2e-004-totales.log
```

**Resultado Esperado**:
- ❌ Error de validación detectado
- ✅ Mensaje indicando discrepancia en totales

---

### E2E-005: Detección de Duplicados

**Objetivo**: Verificar que el sistema detecta registros duplicados.

```bash
# Crear factura
cat > factura-e2e-005.json << 'EOF'
{
  "Serie": "DUP",
  "Numero": "001",
  "FechaEmision": "2025-10-30T14:00:00",
  "Emisor": {
    "Nif": "B12345678",
    "Nombre": "EMPRESA PRUEBAS E2E S.L.",
    "Direccion": "C/ Pruebas 1",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Receptor": {
    "Nif": "12345678Z",
    "Nombre": "CLIENTE PRUEBA",
    "Direccion": "Av. Test 2",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Lineas": [
    {
      "Descripcion": "Servicio único",
      "Cantidad": 1,
      "PrecioUnitario": 100.0,
      "TipoImpositivo": 21.0
    }
  ],
  "Totales": {
    "BaseImponible": 100.0,
    "CuotaImpuestos": 21.0,
    "ImporteTotal": 121.0
  },
  "Moneda": "EUR"
}
EOF

# Primer envío (debe ser aceptado)
echo "=== Primer envío ==="
dotnet run -- --factura factura-e2e-005.json > envio1.log 2>&1
cat envio1.log

# Extraer UUID del primer envío
UUID=$(grep "UUID:" envio1.log | cut -d' ' -f2)
echo "UUID del primer envío: $UUID"

# Esperar un momento
sleep 3

# Segundo envío con el mismo UUID (debe ser rechazado)
echo ""
echo "=== Segundo envío (duplicado) ==="
# Nota: Necesitarías modificar el código para reutilizar el mismo UUID
# Este es un ejemplo conceptual
dotnet run -- --factura factura-e2e-005.json --uuid $UUID > envio2.log 2>&1
cat envio2.log

# Verificar error de duplicado
grep -i "duplicado\|duplicate" envio2.log
```

**Resultado Esperado**:
- ✅ Primer envío: Aceptado
- ❌ Segundo envío: Rechazado
- ✅ Código de error: Registro duplicado

---

## Payloads de Ejemplo

### Factura Básica con IVA 21%

```json
{
  "Serie": "A",
  "Numero": "2025-001",
  "FechaEmision": "2025-10-30T10:00:00",
  "Emisor": {
    "Nif": "B12345678",
    "Nombre": "MI EMPRESA S.L.",
    "Direccion": "C/ Principal 1",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Receptor": {
    "Nif": "12345678Z",
    "Nombre": "Juan Pérez",
    "Direccion": "Av. Cliente 10",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Lineas": [
    {
      "Descripcion": "Servicio de consultoría",
      "Cantidad": 10,
      "PrecioUnitario": 100.0,
      "TipoImpositivo": 21.0
    }
  ],
  "Totales": {
    "BaseImponible": 1000.0,
    "CuotaImpuestos": 210.0,
    "ImporteTotal": 1210.0
  },
  "Moneda": "EUR"
}
```

### Factura con Múltiples Tipos de IVA

```json
{
  "Serie": "B",
  "Numero": "2025-002",
  "FechaEmision": "2025-10-30T11:00:00",
  "Emisor": {
    "Nif": "B87654321",
    "Nombre": "COMERCIO MIXTO S.L.",
    "Direccion": "C/ Comercio 5",
    "Provincia": "Barcelona",
    "Municipio": "Barcelona",
    "Pais": "ES"
  },
  "Receptor": {
    "Nif": "87654321B",
    "Nombre": "Cliente Comercial S.A.",
    "Direccion": "Polígono Industrial 20",
    "Provincia": "Barcelona",
    "Municipio": "Barcelona",
    "Pais": "ES"
  },
  "Lineas": [
    {
      "Descripcion": "Productos alimentarios básicos",
      "Cantidad": 50,
      "PrecioUnitario": 2.0,
      "TipoImpositivo": 4.0
    },
    {
      "Descripcion": "Productos alimentarios generales",
      "Cantidad": 30,
      "PrecioUnitario": 5.0,
      "TipoImpositivo": 10.0
    },
    {
      "Descripcion": "Productos no alimentarios",
      "Cantidad": 20,
      "PrecioUnitario": 10.0,
      "TipoImpositivo": 21.0
    }
  ],
  "Totales": {
    "BaseImponible": 450.0,
    "CuotaImpuestos": 61.0,
    "ImporteTotal": 511.0
  },
  "Moneda": "EUR"
}
```

### Factura Rectificativa

```json
{
  "Serie": "R",
  "Numero": "2025-R001",
  "FechaEmision": "2025-10-30T12:00:00",
  "TipoFactura": "Rectificativa",
  "FacturaRectificada": {
    "Serie": "A",
    "Numero": "2025-001",
    "Fecha": "2025-10-15"
  },
  "Emisor": {
    "Nif": "B12345678",
    "Nombre": "MI EMPRESA S.L.",
    "Direccion": "C/ Principal 1",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Receptor": {
    "Nif": "12345678Z",
    "Nombre": "Juan Pérez",
    "Direccion": "Av. Cliente 10",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Lineas": [
    {
      "Descripcion": "Rectificación: Servicio de consultoría",
      "Cantidad": -2,
      "PrecioUnitario": 100.0,
      "TipoImpositivo": 21.0
    }
  ],
  "Totales": {
    "BaseImponible": -200.0,
    "CuotaImpuestos": -42.0,
    "ImporteTotal": -242.0
  },
  "Moneda": "EUR",
  "Observaciones": "Rectificación por error en cantidad facturada"
}
```

### Factura con Retención IRPF

```json
{
  "Serie": "C",
  "Numero": "2025-003",
  "FechaEmision": "2025-10-30T13:00:00",
  "Emisor": {
    "Nif": "12345678A",
    "Nombre": "Juan Profesional Autónomo",
    "Direccion": "C/ Autónomo 15",
    "Provincia": "Valencia",
    "Municipio": "Valencia",
    "Pais": "ES"
  },
  "Receptor": {
    "Nif": "B99887766",
    "Nombre": "EMPRESA CONTRATANTE S.L.",
    "Direccion": "Av. Empresarial 100",
    "Provincia": "Valencia",
    "Municipio": "Valencia",
    "Pais": "ES"
  },
  "Lineas": [
    {
      "Descripcion": "Servicios profesionales de diseño",
      "Cantidad": 40,
      "PrecioUnitario": 50.0,
      "TipoImpositivo": 21.0
    }
  ],
  "Totales": {
    "BaseImponible": 2000.0,
    "CuotaImpuestos": 420.0,
    "RetencionIRPF": 300.0,
    "ImporteTotal": 2120.0
  },
  "ImporteAPagar": 1820.0,
  "Moneda": "EUR"
}
```

### Factura Simplificada (Ticket)

```json
{
  "Serie": "T",
  "Numero": "2025-T0123",
  "FechaEmision": "2025-10-30T14:30:00",
  "TipoFactura": "Simplificada",
  "Emisor": {
    "Nif": "B55443322",
    "Nombre": "COMERCIO MINORISTA S.L.",
    "Direccion": "C/ Tienda 8",
    "Provincia": "Sevilla",
    "Municipio": "Sevilla",
    "Pais": "ES"
  },
  "Lineas": [
    {
      "Descripcion": "Varios artículos",
      "Cantidad": 1,
      "PrecioUnitario": 45.50,
      "TipoImpositivo": 21.0
    }
  ],
  "Totales": {
    "BaseImponible": 45.50,
    "CuotaImpuestos": 9.56,
    "ImporteTotal": 55.06
  },
  "Moneda": "EUR"
}
```

---

## Verificación de Resultados

### Verificar Respuesta SOAP

La respuesta del servidor AEAT debe incluir:

```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <RespuestaRegistro xmlns="...">
      <Estado>Aceptado</Estado>
      <CodigoRespuesta>0</CodigoRespuesta>
      <Mensaje>Registro aceptado correctamente</Mensaje>
      <CSV>CODIGO_SEGURO_VERIFICACION</CSV>
      <FechaRecepcion>2025-10-30T15:00:00Z</FechaRecepcion>
    </RespuestaRegistro>
  </soap:Body>
</soap:Envelope>
```

### Código de Análisis de Respuesta

```csharp
public class RespuestaValidator
{
    public void ValidarRespuesta(string xmlRespuesta)
    {
        var doc = XDocument.Parse(xmlRespuesta);
        var ns = doc.Root.GetDefaultNamespace();
        
        var estado = doc.Descendants(ns + "Estado").FirstOrDefault()?.Value;
        var codigo = doc.Descendants(ns + "CodigoRespuesta").FirstOrDefault()?.Value;
        var mensaje = doc.Descendants(ns + "Mensaje").FirstOrDefault()?.Value;
        var csv = doc.Descendants(ns + "CSV").FirstOrDefault()?.Value;
        
        Console.WriteLine($"Estado: {estado}");
        Console.WriteLine($"Código: {codigo}");
        Console.WriteLine($"Mensaje: {mensaje}");
        Console.WriteLine($"CSV: {csv}");
        
        if (estado == "Aceptado" && codigo == "0")
        {
            Console.WriteLine("✅ Registro aceptado exitosamente");
        }
        else
        {
            Console.WriteLine($"❌ Registro rechazado: {mensaje}");
        }
    }
}
```

### Verificar Huella Calculada

```bash
# Script para verificar huella
cat > verificar-huella.sh << 'EOFSCRIPT'
#!/bin/bash

FACTURA=$1
HUELLA_ESPERADA=$2

echo "Verificando huella de: $FACTURA"

# Ejecutar cálculo
dotnet run -- --factura $FACTURA --solo-calcular-huella > huella-calculada.txt

# Extraer huella
HUELLA=$(grep "Huella:" huella-calculada.txt | cut -d' ' -f2)

echo "Huella calculada: $HUELLA"
echo "Huella esperada:  $HUELLA_ESPERADA"

if [ "$HUELLA" == "$HUELLA_ESPERADA" ]; then
    echo "✅ Huella correcta"
    exit 0
else
    echo "❌ Huella incorrecta"
    exit 1
fi
EOFSCRIPT

chmod +x verificar-huella.sh
```

### Logs a Revisar

Después de cada prueba, verifica estos logs:

```bash
# Ver log de aplicación
cat logs/verifactu-$(date +%Y%m%d).log

# Filtrar solo errores
grep "ERROR" logs/verifactu-$(date +%Y%m%d).log

# Ver peticiones SOAP enviadas
grep "SOAP Request" logs/verifactu-$(date +%Y%m%d).log

# Ver respuestas SOAP recibidas
grep "SOAP Response" logs/verifactu-$(date +%Y%m%d).log
```

---

## Manejo de Errores

### Tabla de Códigos de Error Comunes

| Código | Descripción | Solución |
|--------|-------------|----------|
| 1001 | Registro duplicado | Verificar UUID, usar uno nuevo |
| 2001 | Error de validación XML | Revisar XML contra XSD, verificar estructura |
| 2002 | NIF inválido | Verificar formato de NIF (letra y números) |
| 2003 | Datos obligatorios faltantes | Completar todos los campos requeridos |
| 3001 | Error de firma electrónica | Verificar certificado, revisar algoritmo de firma |
| 3002 | Certificado no válido | Renovar certificado, verificar caducidad |
| 3003 | Certificado no autorizado | Usar certificado de representante correcto |
| 4001 | Huella inválida | Revisar algoritmo de cálculo de huella |
| 4002 | Encadenamiento incorrecto | Verificar huella anterior |
| 5001 | Error de comunicación | Revisar conectividad, endpoint correcto |
| 5002 | Timeout | Aumentar timeout, reintentar |
| 9999 | Error interno del servidor | Reintentar más tarde, contactar soporte AEAT |

### Manejo de Error: Timeout

```bash
# Configurar timeout más alto
dotnet user-secrets set "Verifactu:Timeout" "60"

# Implementar reintentos
cat > enviar-con-reintentos.sh << 'EOFSCRIPT'
#!/bin/bash

FACTURA=$1
MAX_INTENTOS=3
INTENTO=1

while [ $INTENTO -le $MAX_INTENTOS ]; do
    echo "Intento $INTENTO de $MAX_INTENTOS"
    
    if dotnet run -- --factura $FACTURA; then
        echo "✅ Envío exitoso en intento $INTENTO"
        exit 0
    else
        echo "❌ Fallo en intento $INTENTO"
        INTENTO=$((INTENTO + 1))
        
        if [ $INTENTO -le $MAX_INTENTOS ]; then
            DELAY=$((2 ** (INTENTO - 1)))
            echo "Esperando $DELAY segundos antes del siguiente intento..."
            sleep $DELAY
        fi
    fi
done

echo "❌ Todos los intentos fallaron"
exit 1
EOFSCRIPT

chmod +x enviar-con-reintentos.sh

# Usar
./enviar-con-reintentos.sh factura-e2e-001.json
```

### Manejo de Error: Certificado Inválido

```bash
# Verificar certificado antes de enviar
cat > verificar-certificado.sh << 'EOFSCRIPT'
#!/bin/bash

CERT_PATH=$1
CERT_PASSWORD=$2

echo "Verificando certificado: $CERT_PATH"

# Verificar que existe
if [ ! -f "$CERT_PATH" ]; then
    echo "❌ Archivo de certificado no encontrado"
    exit 1
fi

# Verificar contenido (requiere password)
if ! openssl pkcs12 -in "$CERT_PATH" -nokeys -passin pass:"$CERT_PASSWORD" -info > /dev/null 2>&1; then
    echo "❌ No se puede leer el certificado (contraseña incorrecta o archivo corrupto)"
    exit 1
fi

# Verificar fechas de validez
echo "Verificando fechas de validez..."
openssl pkcs12 -in "$CERT_PATH" -nokeys -passin pass:"$CERT_PASSWORD" | \
    openssl x509 -noout -dates

# Verificar que tiene clave privada
if ! openssl pkcs12 -in "$CERT_PATH" -nocerts -passin pass:"$CERT_PASSWORD" -nodes > /dev/null 2>&1; then
    echo "❌ El certificado no contiene clave privada"
    exit 1
fi

echo "✅ Certificado válido"
exit 0
EOFSCRIPT

chmod +x verificar-certificado.sh

# Usar
./verificar-certificado.sh /ruta/certificado.pfx "password"
```

### Manejo de Error: XML Inválido

```bash
# Validar XML contra XSD antes de enviar
cat > validar-xml.sh << 'EOFSCRIPT'
#!/bin/bash

XML_FILE=$1
XSD_FILE=$2

echo "Validando XML contra XSD..."

if ! command -v xmllint &> /dev/null; then
    echo "xmllint no está instalado"
    echo "Instalar: sudo apt-get install libxml2-utils  # Debian/Ubuntu"
    echo "         brew install libxml2  # macOS"
    exit 1
fi

if xmllint --noout --schema "$XSD_FILE" "$XML_FILE" 2>&1; then
    echo "✅ XML válido"
    exit 0
else
    echo "❌ XML inválido"
    exit 1
fi
EOFSCRIPT

chmod +x validar-xml.sh

# Descargar XSD del portal de pruebas AEAT
mkdir -p xsd
cd xsd
# curl -O https://www2.agenciatributaria.gob.es/static_files/.../SuministroLR.xsd

# Validar
./validar-xml.sh mi-registro.xml xsd/SuministroLR.xsd
```


### Captura y Análisis de Errores

```bash
# Script completo de análisis de errores
cat > analizar-error.sh << 'EOFSCRIPT'
#!/bin/bash

LOG_FILE=$1

echo "=== ANÁLISIS DE ERRORES ==="
echo ""

# Extraer código de error
CODIGO_ERROR=$(grep -oP "CodigoError: \K\d+" $LOG_FILE)
if [ -n "$CODIGO_ERROR" ]; then
    echo "Código de error encontrado: $CODIGO_ERROR"
    
    # Buscar descripción del error
    case $CODIGO_ERROR in
        1001) echo "Descripción: Registro duplicado" ;;
        2001) echo "Descripción: Error de validación XML" ;;
        2002) echo "Descripción: NIF inválido" ;;
        2003) echo "Descripción: Datos obligatorios faltantes" ;;
        3001) echo "Descripción: Error de firma electrónica" ;;
        3002) echo "Descripción: Certificado no válido" ;;
        3003) echo "Descripción: Certificado no autorizado" ;;
        4001) echo "Descripción: Huella inválida" ;;
        4002) echo "Descripción: Encadenamiento incorrecto" ;;
        5001) echo "Descripción: Error de comunicación" ;;
        5002) echo "Descripción: Timeout" ;;
        *) echo "Descripción: Error desconocido" ;;
    esac
fi

# Extraer mensaje de error
MENSAJE=$(grep -oP "Mensaje: \K.*" $LOG_FILE)
if [ -n "$MENSAJE" ]; then
    echo ""
    echo "Mensaje de error:"
    echo "$MENSAJE"
fi

# Extraer stack trace si existe
if grep -q "StackTrace:" $LOG_FILE; then
    echo ""
    echo "Stack trace:"
    sed -n '/StackTrace:/,/^$/p' $LOG_FILE
fi

echo ""
echo "=== FIN DEL ANÁLISIS ==="
EOFSCRIPT

chmod +x analizar-error.sh
```

---

## Automatización de Pruebas

### Suite Completa de Pruebas

```bash
# Script maestro para ejecutar todas las pruebas E2E
cat > ejecutar-suite-e2e.sh << 'EOFSCRIPT'
#!/bin/bash

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Contadores
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Función para ejecutar test
run_test() {
    local test_id=$1
    local test_name=$2
    local test_command=$3
    
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    
    echo ""
    echo "========================================="
    echo "Test: $test_id - $test_name"
    echo "========================================="
    
    if eval $test_command; then
        echo -e "${GREEN}✅ PASSED${NC}: $test_id"
        PASSED_TESTS=$((PASSED_TESTS + 1))
        return 0
    else
        echo -e "${RED}❌ FAILED${NC}: $test_id"
        FAILED_TESTS=$((FAILED_TESTS + 1))
        return 1
    fi
}

# Banner inicial
echo "╔════════════════════════════════════════════════╗"
echo "║   SUITE DE PRUEBAS END-TO-END VERI*FACTU      ║"
echo "║   Entorno: Sandbox AEAT                        ║"
echo "╚════════════════════════════════════════════════╝"
echo ""
echo "Fecha: $(date '+%Y-%m-%d %H:%M:%S')"
echo ""

# Verificar prerequisitos
echo "Verificando prerequisitos..."

if ! dotnet --version > /dev/null 2>&1; then
    echo -e "${RED}❌ .NET SDK no está instalado${NC}"
    exit 1
fi

if [ -z "$VERIFACTU_CERT_PATH" ] || [ ! -f "$VERIFACTU_CERT_PATH" ]; then
    echo -e "${YELLOW}⚠️  Certificado no configurado o no encontrado${NC}"
    echo "Configura VERIFACTU_CERT_PATH"
fi

echo -e "${GREEN}✅ Prerequisitos verificados${NC}"

# Navegar al directorio del proyecto
cd src/Verifactu.ConsoleDemo

# Ejecutar tests
run_test "E2E-001" "Envío de factura simple" \
    "dotnet run -- --factura factura-e2e-001.json > /tmp/e2e-001.log 2>&1"

run_test "E2E-002" "Envío de factura con múltiples líneas" \
    "dotnet run -- --factura factura-e2e-002.json > /tmp/e2e-002.log 2>&1"

run_test "E2E-003" "Encadenamiento de registros" \
    "./test-encadenamiento.sh > /tmp/e2e-003.log 2>&1"

run_test "E2E-004" "Manejo de datos inválidos (debe fallar)" \
    "! dotnet run -- --factura factura-e2e-004-nif-invalido.json > /tmp/e2e-004.log 2>&1"

run_test "E2E-005" "Detección de duplicados" \
    "dotnet run -- --factura factura-e2e-005.json > /tmp/e2e-005.log 2>&1"

# Resumen final
echo ""
echo "╔════════════════════════════════════════════════╗"
echo "║              RESUMEN DE PRUEBAS                ║"
echo "╚════════════════════════════════════════════════╝"
echo ""
echo "Total de pruebas:    $TOTAL_TESTS"
echo -e "${GREEN}Pruebas exitosas:    $PASSED_TESTS${NC}"
echo -e "${RED}Pruebas fallidas:    $FAILED_TESTS${NC}"
echo ""

# Calcular porcentaje
if [ $TOTAL_TESTS -gt 0 ]; then
    SUCCESS_RATE=$((PASSED_TESTS * 100 / TOTAL_TESTS))
    echo "Tasa de éxito: $SUCCESS_RATE%"
fi

echo ""
echo "Logs guardados en /tmp/e2e-*.log"
echo ""

# Exit code
if [ $FAILED_TESTS -eq 0 ]; then
    echo -e "${GREEN}✅ Todas las pruebas pasaron exitosamente${NC}"
    exit 0
else
    echo -e "${RED}❌ Algunas pruebas fallaron. Revisa los logs.${NC}"
    exit 1
fi
EOFSCRIPT

chmod +x ejecutar-suite-e2e.sh
```

### Integración con CI/CD

#### GitHub Actions

```yaml
# .github/workflows/e2e-tests.yml
name: Pruebas E2E Sandbox AEAT

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]
  schedule:
    # Ejecutar diariamente a las 2 AM UTC
    - cron: '0 2 * * *'

jobs:
  e2e-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Configurar certificado de prueba
      env:
        CERT_BASE64: ${{ secrets.VERIFACTU_TEST_CERT_BASE64 }}
        CERT_PASSWORD: ${{ secrets.VERIFACTU_TEST_CERT_PASSWORD }}
      run: |
        echo "$CERT_BASE64" | base64 -d > test-cert.pfx
        echo "VERIFACTU_CERT_PATH=$(pwd)/test-cert.pfx" >> $GITHUB_ENV
        echo "VERIFACTU_CERT_PASSWORD=$CERT_PASSWORD" >> $GITHUB_ENV
    
    - name: Configurar variables de entorno
      run: |
        echo "VERIFACTU_ENDPOINT_URL=https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion" >> $GITHUB_ENV
        echo "VERIFACTU_ENV=Sandbox" >> $GITHUB_ENV
    
    - name: Ejecutar suite E2E
      run: |
        cd docs
        bash ejecutar-suite-e2e.sh
    
    - name: Subir logs de pruebas
      if: always()
      uses: actions/upload-artifact@v3
      with:
        name: e2e-test-logs
        path: /tmp/e2e-*.log
    
    - name: Limpiar certificado
      if: always()
      run: rm -f test-cert.pfx
```

### Pruebas de Carga

```bash
# Script de pruebas de carga - enviar múltiples facturas
cat > prueba-carga.sh << 'EOFSCRIPT'
#!/bin/bash

NUM_FACTURAS=${1:-10}
DELAY_ENTRE_ENVIOS=${2:-2}

echo "=== PRUEBA DE CARGA ==="
echo "Facturas a enviar: $NUM_FACTURAS"
echo "Delay entre envíos: $DELAY_ENTRE_ENVIOS segundos"
echo ""

SUCCESS=0
FAILED=0

for i in $(seq 1 $NUM_FACTURAS); do
    echo "Enviando factura $i de $NUM_FACTURAS..."
    
    # Crear factura única
    cat > factura-carga-$i.json << EOF
{
  "Serie": "LOAD",
  "Numero": "$(printf '%05d' $i)",
  "FechaEmision": "$(date -u +%Y-%m-%dT%H:%M:%S)",
  "Emisor": {
    "Nif": "B12345678",
    "Nombre": "EMPRESA PRUEBAS CARGA S.L.",
    "Direccion": "C/ Pruebas 1",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Receptor": {
    "Nif": "12345678Z",
    "Nombre": "CLIENTE PRUEBA CARGA",
    "Direccion": "Av. Test 2",
    "Provincia": "Madrid",
    "Municipio": "Madrid",
    "Pais": "ES"
  },
  "Lineas": [
    {
      "Descripcion": "Servicio de carga $i",
      "Cantidad": 1,
      "PrecioUnitario": $(($i * 10)).0,
      "TipoImpositivo": 21.0
    }
  ],
  "Totales": {
    "BaseImponible": $(($i * 10)).0,
    "CuotaImpuestos": $(echo "scale=2; $i * 10 * 0.21" | bc),
    "ImporteTotal": $(echo "scale=2; $i * 10 * 1.21" | bc)
  },
  "Moneda": "EUR"
}
EOF
    
    # Enviar
    if dotnet run -- --factura factura-carga-$i.json > /tmp/carga-$i.log 2>&1; then
        echo "✅ Factura $i enviada exitosamente"
        SUCCESS=$((SUCCESS + 1))
    else
        echo "❌ Error al enviar factura $i"
        FAILED=$((FAILED + 1))
    fi
    
    # Delay
    if [ $i -lt $NUM_FACTURAS ]; then
        sleep $DELAY_ENTRE_ENVIOS
    fi
done

echo ""
echo "=== RESUMEN ==="
echo "Total: $NUM_FACTURAS"
echo "Exitosas: $SUCCESS"
echo "Fallidas: $FAILED"
echo "Tasa de éxito: $((SUCCESS * 100 / NUM_FACTURAS))%"
EOFSCRIPT

chmod +x prueba-carga.sh

# Ejecutar prueba de carga con 10 facturas
./prueba-carga.sh 10 2
```

---

## Resolución de Problemas

### Checklist de Diagnóstico

Cuando encuentres problemas, sigue esta lista:

#### 1. Verificar Conectividad

```bash
# Ping al servidor (puede no responder a ping)
ping prewww1.aeat.es

# Verificar resolución DNS
nslookup prewww1.aeat.es

# Probar conexión HTTPS
curl -I https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion

# Verificar que el puerto 443 está abierto
nc -zv prewww1.aeat.es 443
```

#### 2. Verificar Certificado

```bash
# Ver información del certificado
openssl pkcs12 -in $VERIFACTU_CERT_PATH -nokeys -passin pass:$VERIFACTU_CERT_PASSWORD -info

# Verificar fechas
openssl pkcs12 -in $VERIFACTU_CERT_PATH -nokeys -passin pass:$VERIFACTU_CERT_PASSWORD | \
    openssl x509 -noout -dates

# Verificar que tiene clave privada
openssl pkcs12 -in $VERIFACTU_CERT_PATH -nocerts -passin pass:$VERIFACTU_CERT_PASSWORD -nodes | head -n 5
```

#### 3. Verificar Configuración

```bash
# Ver secretos configurados
cd src/Verifactu.ConsoleDemo
dotnet user-secrets list

# Ver variables de entorno
env | grep VERIFACTU

# Verificar appsettings
cat appsettings.Sandbox.json | jq .
```

#### 4. Verificar Logs

```bash
# Ver últimas líneas del log
tail -n 50 logs/verifactu-$(date +%Y%m%d).log

# Buscar errores
grep -i "error\|exception\|fail" logs/verifactu-$(date +%Y%m%d).log

# Ver peticiones SOAP
grep -A 20 "SOAP Request" logs/verifactu-$(date +%Y%m%d).log | head -n 50
```

### Problemas Específicos y Soluciones

#### Problema: "No se puede establecer conexión SSL/TLS"

**Síntomas**:
```
System.Net.Http.HttpRequestException: The SSL connection could not be established
```

**Diagnóstico**:
```bash
# Verificar versión TLS soportada
openssl s_client -connect prewww1.aeat.es:443 -tls1_2

# Verificar con certificado cliente
openssl s_client -connect prewww1.aeat.es:443 \
    -cert cert.pem -key key.pem -tls1_2
```

**Solución**:
1. Asegurar que TLS 1.2 o superior está habilitado
2. Verificar que el certificado cliente se está enviando correctamente
3. Revisar configuración de proxy si aplica

#### Problema: "Timeout al enviar"

**Síntomas**:
```
System.Threading.Tasks.TaskCanceledException: The request was canceled
```

**Solución**:
```bash
# Aumentar timeout en configuración
dotnet user-secrets set "Verifactu:Timeout" "60"

# O en appsettings.Sandbox.json
{
  "Verifactu": {
    "Timeout": 60
  }
}
```

#### Problema: "XML no válido"

**Síntomas**:
```
SOAP Fault: Error de validación del XML
```

**Diagnóstico**:
```bash
# Guardar XML generado
dotnet run -- --factura test.json --save-xml > generated.xml

# Validar contra XSD
xmllint --noout --schema SuministroLR.xsd generated.xml
```

**Solución**:
1. Revisar namespaces en el XML
2. Verificar que todos los campos obligatorios están presentes
3. Comparar con ejemplos del portal de pruebas AEAT

#### Problema: "Huella incorrecta"

**Síntomas**:
```
Error 4001: Huella inválida
```

**Diagnóstico**:
```bash
# Verificar algoritmo de hash
echo -n "datos_de_prueba" | openssl dgst -sha256

# Comparar con huella esperada
```

**Solución**:
1. Revisar el algoritmo de cálculo (debe ser SHA-256)
2. Verificar encoding de caracteres (UTF-8)
3. Asegurar que se incluyen todos los campos requeridos
4. Verificar orden de los campos

### Herramientas de Depuración

#### Habilitar Logging Detallado

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Verifactu": "Trace",
      "System.Net.Http": "Trace",
      "System.Net.Http.HttpClient": "Trace"
    }
  }
}
```

#### Capturar Tráfico con Fiddler

En Windows:

```powershell
# Configurar proxy para .NET
$env:HTTP_PROXY="http://localhost:8888"
$env:HTTPS_PROXY="http://localhost:8888"

# Ejecutar aplicación
dotnet run
```

#### Usar SoapUI para Pruebas Manuales

1. Descargar SoapUI
2. Importar WSDL de AEAT
3. Configurar certificado cliente
4. Enviar peticiones de prueba manualmente

---

## Métricas y Reportes

### Generar Reporte de Pruebas

```bash
cat > generar-reporte.sh << 'EOFSCRIPT'
#!/bin/bash

FECHA=$(date +%Y%m%d_%H%M%S)
REPORTE="reporte-e2e-$FECHA.html"

cat > $REPORTE << 'EOHTML'
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <title>Reporte Pruebas E2E - VERI*FACTU</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        h1 { color: #003366; }
        table { border-collapse: collapse; width: 100%; margin: 20px 0; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #003366; color: white; }
        .pass { background-color: #d4edda; }
        .fail { background-color: #f8d7da; }
        .summary { background-color: #e7f3ff; padding: 15px; margin: 20px 0; }
    </style>
</head>
<body>
    <h1>Reporte de Pruebas End-to-End VERI*FACTU</h1>
    <div class="summary">
        <h2>Resumen</h2>
        <p><strong>Fecha:</strong> $(date '+%Y-%m-%d %H:%M:%S')</p>
        <p><strong>Entorno:</strong> Sandbox AEAT</p>
        <p><strong>Endpoint:</strong> https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion</p>
    </div>
    
    <h2>Resultados de Pruebas</h2>
    <table>
        <tr>
            <th>ID</th>
            <th>Descripción</th>
            <th>Resultado</th>
            <th>Tiempo (s)</th>
            <th>Detalles</th>
        </tr>
EOHTML

# Leer resultados de logs
for log in /tmp/e2e-*.log; do
    if [ -f "$log" ]; then
        TEST_ID=$(basename $log .log | cut -d- -f2-)
        
        if grep -q "SUCCESS\|✅" $log; then
            RESULTADO="PASS"
            CLASS="pass"
        else
            RESULTADO="FAIL"
            CLASS="fail"
        fi
        
        DESCRIPCION=$(grep -m1 "Test:" $log | cut -d: -f2- || echo "N/A")
        TIEMPO=$(grep -oP "Elapsed: \K[0-9.]+" $log || echo "N/A")
        
        cat >> $REPORTE << EOHTML
        <tr class="$CLASS">
            <td>$TEST_ID</td>
            <td>$DESCRIPCION</td>
            <td>$RESULTADO</td>
            <td>$TIEMPO</td>
            <td><a href="$log">Ver log</a></td>
        </tr>
EOHTML
    fi
done

cat >> $REPORTE << 'EOHTML'
    </table>
    
    <h2>Logs Completos</h2>
    <p>Logs disponibles en: /tmp/e2e-*.log</p>
</body>
</html>
EOHTML

echo "Reporte generado: $REPORTE"
open $REPORTE 2>/dev/null || xdg-open $REPORTE 2>/dev/null || echo "Abre el archivo manualmente: $REPORTE"
EOFSCRIPT

chmod +x generar-reporte.sh
```

---

## Mejores Prácticas

### 1. Gestión de Certificados

- ✅ Usar certificados de prueba específicos para sandbox
- ✅ Nunca versionar certificados en Git
- ✅ Rotar certificados antes de que expiren
- ✅ Usar Azure Key Vault o similar en producción

### 2. Gestión de Datos de Prueba

- ✅ Mantener un conjunto de facturas de prueba reutilizables
- ✅ Usar NIFs de prueba (no reales)
- ✅ Documentar cada caso de prueba
- ✅ Versionar archivos de prueba en Git

### 3. Ejecución de Pruebas

- ✅ Ejecutar suite completa antes de cada release
- ✅ Automatizar pruebas en CI/CD
- ✅ Mantener logs de todas las ejecuciones
- ✅ Revisar fallos inmediatamente

### 4. Monitorización

- ✅ Registrar todas las peticiones y respuestas
- ✅ Monitorizar tasa de éxito/fallo
- ✅ Alertar sobre fallos consecutivos
- ✅ Revisar logs regularmente

---

## Referencias

### Documentación del Proyecto

- [Guía de Instalación](instalacion.md)
- [Guía de Uso](uso.md)
- [Entorno de Pruebas](entorno-pruebas.md)
- [Arquitectura](arquitectura.md)
- [Protocolos de Comunicación](protocolos-comunicacion.md)

### Documentación Oficial AEAT

- [Portal de Pruebas Externas](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/portal-pruebas-externas.html)
- [Información Técnica VERI*FACTU](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/informacion-tecnica.html)
- [Preguntas Frecuentes](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/preguntas-frecuentes.html)
- [Documentación de Servicios Web](https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/)

### Herramientas

- [.NET 9 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [SoapUI](https://www.soapui.org/)
- [Fiddler](https://www.telerik.com/fiddler)
- [Postman](https://www.postman.com/)

---

## Anexos

### Anexo A: Variables de Entorno Completas

```bash
# Certificado
export VERIFACTU_CERT_PATH="/ruta/certificado.pfx"
export VERIFACTU_CERT_PASSWORD="password"

# Endpoint
export VERIFACTU_ENDPOINT_URL="https://prewww1.aeat.es/wlpl/TIKE-CONT/SistemaFacturacion"
export VERIFACTU_WSDL_URL="https://www2.agenciatributaria.gob.es/.../SistemaFacturacion.wsdl"
export VERIFACTU_SOAP_ACTION="RegFacturacionAlta"

# Configuración
export VERIFACTU_ENV="Sandbox"
export VERIFACTU_TIMEOUT="30"
export VERIFACTU_MAX_RETRIES="3"
export VERIFACTU_RETRY_DELAY_MS="1000"

# Logging
export VERIFACTU_LOG_LEVEL="Debug"
export VERIFACTU_LOG_PATH="./logs"

# Otros
export VERIFACTU_HUELLA_ANTERIOR=""
export ASPNETCORE_ENVIRONMENT="Sandbox"
```

### Anexo B: Ejemplo de Respuesta SOAP Completa

```xml
<?xml version="1.0" encoding="UTF-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
               xmlns:sfe="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/">
  <soap:Body>
    <sfe:RespuestaRegFacturacionAlta>
      <sfe:Cabecera>
        <sfe:IDVersion>1.0</sfe:IDVersion>
        <sfe:FechaHoraRespuesta>2025-10-30T15:30:45Z</sfe:FechaHoraRespuesta>
      </sfe:Cabecera>
      <sfe:RespuestaLinea>
        <sfe:IDFactura>
          <sfe:IDEmisorFactura>B12345678</sfe:IDEmisorFactura>
          <sfe:NumSerieFactura>TEST-E2E</sfe:NumSerieFactura>
          <sfe:FechaExpedicionFactura>30-10-2025</sfe:FechaExpedicionFactura>
        </sfe:IDFactura>
        <sfe:EstadoRegistro>Aceptado</sfe:EstadoRegistro>
        <sfe:CodigoErrorRegistro>0</sfe:CodigoErrorRegistro>
        <sfe:DescripcionErrorRegistro>Registro aceptado correctamente</sfe:DescripcionErrorRegistro>
        <sfe:CSV>ABC123XYZ789DEF456</sfe:CSV>
      </sfe:RespuestaLinea>
    </sfe:RespuestaRegFacturacionAlta>
  </soap:Body>
</soap:Envelope>
```

### Anexo C: Checklist Pre-Producción

Antes de pasar de sandbox a producción, verificar:

- [ ] Todas las pruebas E2E pasan exitosamente en sandbox
- [ ] Certificado de producción obtenido y validado
- [ ] Endpoints de producción configurados
- [ ] Validación XML contra XSD oficial verificada
- [ ] Algoritmo de huella implementado según especificación AEAT
- [ ] Firma electrónica cumple con políticas AEAT
- [ ] Sistema de logging y monitorización configurado
- [ ] Backups automáticos de registros configurados
- [ ] Procedimientos de rollback documentados
- [ ] Equipo entrenado en operación y soporte
- [ ] Plan de contingencia definido
- [ ] Contacto con soporte AEAT establecido
- [ ] Documentación operativa completa
- [ ] Pruebas de carga realizadas
- [ ] Pruebas de recuperación ante desastres realizadas

---

## Soporte

Para preguntas o problemas:

1. Revisa la [documentación del proyecto](README.md)
2. Consulta las [FAQs de AEAT](https://sede.agenciatributaria.gob.es/Sede/iva/verifactu/preguntas-frecuentes.html)
3. Abre un [Issue en GitHub](https://github.com/JoseRGWeb/Veri-factuSender/issues)
4. Consulta los logs de depuración

---

**Última actualización**: 30 de octubre de 2025  
**Versión**: 1.0  
**Autor**: Equipo VerifactuSender

