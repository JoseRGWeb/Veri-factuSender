#!/bin/bash
# Script de verificación de configuración del entorno sandbox AEAT
# Para uso con VerifactuSender

echo "=================================================="
echo "  Verificación de Entorno Sandbox VERI*FACTU"
echo "=================================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Counters
PASS=0
FAIL=0
WARN=0

# Function to print results
print_check() {
    local status=$1
    local message=$2
    
    if [ "$status" = "PASS" ]; then
        echo -e "${GREEN}✓${NC} $message"
        ((PASS++))
    elif [ "$status" = "FAIL" ]; then
        echo -e "${RED}✗${NC} $message"
        ((FAIL++))
    elif [ "$status" = "WARN" ]; then
        echo -e "${YELLOW}⚠${NC} $message"
        ((WARN++))
    fi
}

# 1. Verificar .NET SDK
echo "1. Verificando .NET SDK..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    if [[ "$DOTNET_VERSION" == 9.* ]]; then
        print_check "PASS" ".NET SDK instalado: v$DOTNET_VERSION"
    else
        print_check "WARN" ".NET SDK instalado pero versión < 9.0: v$DOTNET_VERSION"
    fi
else
    print_check "FAIL" ".NET SDK no está instalado"
fi
echo ""

# 2. Verificar estructura del proyecto
echo "2. Verificando estructura del proyecto..."
if [ -f "VerifactuSender.sln" ]; then
    print_check "PASS" "Archivo de solución encontrado"
else
    print_check "FAIL" "Archivo de solución no encontrado"
fi

if [ -d "src/Verifactu.ConsoleDemo" ]; then
    print_check "PASS" "Directorio de demo encontrado"
else
    print_check "FAIL" "Directorio de demo no encontrado"
fi

if [ -d "src/Verifactu.Client" ]; then
    print_check "PASS" "Directorio de librería cliente encontrado"
else
    print_check "FAIL" "Directorio de librería cliente no encontrado"
fi
echo ""

# 3. Verificar archivos de configuración
echo "3. Verificando archivos de configuración..."
if [ -f "src/Verifactu.ConsoleDemo/appsettings.json" ]; then
    print_check "PASS" "appsettings.json existe"
else
    print_check "FAIL" "appsettings.json no encontrado"
fi

if [ -f "src/Verifactu.ConsoleDemo/appsettings.Sandbox.json" ]; then
    print_check "PASS" "appsettings.Sandbox.json existe"
else
    print_check "WARN" "appsettings.Sandbox.json no encontrado (opcional)"
fi

if [ -f "src/Verifactu.ConsoleDemo/factura-demo-sandbox.json" ]; then
    print_check "PASS" "factura-demo-sandbox.json existe"
else
    print_check "WARN" "factura-demo-sandbox.json no encontrado (opcional)"
fi
echo ""

# 4. Verificar validez de JSON
echo "4. Verificando validez de archivos JSON..."
if command -v python3 &> /dev/null; then
    if [ -f "src/Verifactu.ConsoleDemo/appsettings.json" ]; then
        if python3 -m json.tool src/Verifactu.ConsoleDemo/appsettings.json > /dev/null 2>&1; then
            print_check "PASS" "appsettings.json es JSON válido"
        else
            print_check "FAIL" "appsettings.json tiene errores de sintaxis"
        fi
    fi
    
    if [ -f "src/Verifactu.ConsoleDemo/appsettings.Sandbox.json" ]; then
        if python3 -m json.tool src/Verifactu.ConsoleDemo/appsettings.Sandbox.json > /dev/null 2>&1; then
            print_check "PASS" "appsettings.Sandbox.json es JSON válido"
        else
            print_check "FAIL" "appsettings.Sandbox.json tiene errores de sintaxis"
        fi
    fi
else
    print_check "WARN" "Python3 no disponible, omitiendo validación JSON"
fi
echo ""

# 5. Verificar variables de entorno
echo "5. Verificando variables de entorno..."
if [ -n "$VERIFACTU_CERT_PATH" ]; then
    print_check "PASS" "VERIFACTU_CERT_PATH está configurada"
    if [ -f "$VERIFACTU_CERT_PATH" ]; then
        print_check "PASS" "Archivo de certificado existe: $VERIFACTU_CERT_PATH"
    else
        print_check "FAIL" "Archivo de certificado no encontrado: $VERIFACTU_CERT_PATH"
    fi
else
    print_check "WARN" "VERIFACTU_CERT_PATH no está configurada (puede usar appsettings)"
fi

if [ -n "$VERIFACTU_CERT_PASSWORD" ]; then
    print_check "PASS" "VERIFACTU_CERT_PASSWORD está configurada"
else
    print_check "WARN" "VERIFACTU_CERT_PASSWORD no está configurada (puede usar user-secrets)"
fi

if [ -n "$ASPNETCORE_ENVIRONMENT" ]; then
    print_check "PASS" "ASPNETCORE_ENVIRONMENT = $ASPNETCORE_ENVIRONMENT"
else
    print_check "WARN" "ASPNETCORE_ENVIRONMENT no está configurada (usará Production por defecto)"
fi
echo ""

# 6. Verificar conectividad a endpoints de sandbox
echo "6. Verificando conectividad a AEAT..."
if command -v curl &> /dev/null; then
    # Solo verificar que el dominio es alcanzable
    if curl -s --connect-timeout 5 --max-time 10 -I https://prewww1.aeat.es > /dev/null 2>&1; then
        print_check "PASS" "Conexión a prewww1.aeat.es exitosa"
    else
        print_check "WARN" "No se pudo conectar a prewww1.aeat.es (puede ser firewall o red)"
    fi
else
    print_check "WARN" "curl no disponible, omitiendo verificación de conectividad"
fi
echo ""

# 7. Verificar que el proyecto compila
echo "7. Verificando compilación del proyecto..."
if command -v dotnet &> /dev/null; then
    if dotnet build --no-restore > /dev/null 2>&1; then
        print_check "PASS" "Proyecto compila correctamente"
    else
        print_check "FAIL" "Proyecto no compila (ejecutar 'dotnet build' para ver errores)"
    fi
else
    print_check "WARN" ".NET no disponible, omitiendo verificación de compilación"
fi
echo ""

# 8. Verificar documentación
echo "8. Verificando documentación..."
if [ -f "docs/entorno-pruebas.md" ]; then
    print_check "PASS" "Guía de entorno de pruebas disponible"
else
    print_check "WARN" "Guía de entorno de pruebas no encontrada"
fi

if [ -f "docs/SANDBOX-QUICKSTART.md" ]; then
    print_check "PASS" "Guía rápida de sandbox disponible"
else
    print_check "WARN" "Guía rápida de sandbox no encontrada"
fi

if [ -f "README.md" ]; then
    print_check "PASS" "README.md disponible"
else
    print_check "FAIL" "README.md no encontrado"
fi
echo ""

# 9. Verificar .gitignore para seguridad
echo "9. Verificando configuración de seguridad..."
if [ -f ".gitignore" ]; then
    if grep -q "*.pfx" .gitignore && grep -q "*.p12" .gitignore; then
        print_check "PASS" ".gitignore protege archivos de certificados"
    else
        print_check "WARN" ".gitignore no incluye protección para *.pfx y *.p12"
    fi
else
    print_check "FAIL" ".gitignore no encontrado"
fi
echo ""

# Resumen
echo "=================================================="
echo "                    RESUMEN"
echo "=================================================="
echo -e "${GREEN}Pasadas:${NC} $PASS"
echo -e "${YELLOW}Advertencias:${NC} $WARN"
echo -e "${RED}Fallidas:${NC} $FAIL"
echo ""

if [ $FAIL -eq 0 ]; then
    echo -e "${GREEN}✓ El entorno está configurado correctamente${NC}"
    echo ""
    echo "Próximos pasos:"
    echo "  1. Configura el certificado en appsettings.Sandbox.json o variables de entorno"
    echo "  2. Ejecuta: export ASPNETCORE_ENVIRONMENT=Sandbox"
    echo "  3. Ejecuta: cd src/Verifactu.ConsoleDemo && dotnet run"
    echo ""
    echo "Consulta docs/SANDBOX-QUICKSTART.md para más información"
    exit 0
else
    echo -e "${RED}✗ Hay problemas que deben resolverse${NC}"
    echo ""
    echo "Revisa los errores anteriores y consulta:"
    echo "  - docs/entorno-pruebas.md para configuración detallada"
    echo "  - docs/instalacion.md para requisitos del sistema"
    exit 1
fi
