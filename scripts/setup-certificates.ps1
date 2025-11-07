# Script de Configuración de Certificados para VERI*FACTU
# =========================================================
# Este script ayuda a instalar y configurar certificados digitales
# para usar con el sistema VERI*FACTU de la AEAT.
#
# Uso:
#   .\setup-certificates.ps1
#
# Requisitos:
#   - PowerShell 5.1 o superior
#   - Permisos para instalar certificados (CurrentUser no requiere admin)

param(
    [Parameter(HelpMessage="Ruta al archivo PFX del certificado")]
    [string]$PfxPath = "",
    
    [Parameter(HelpMessage="Contraseña del archivo PFX (se solicitará de forma segura si no se proporciona)")]
    [SecureString]$PfxPassword = $null,
    
    [Parameter(HelpMessage="Ubicación del almacén de certificados")]
    [ValidateSet("CurrentUser", "LocalMachine")]
    [string]$StoreLocation = "CurrentUser",
    
    [Parameter(HelpMessage="Nombre del almacén de certificados")]
    [ValidateSet("My", "Root", "CA", "TrustedPeople")]
    [string]$StoreName = "My",
    
    [Parameter(HelpMessage="Solo mostrar información sin instalar")]
    [switch]$InfoOnly
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Setup de Certificados para VERI*FACTU" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Función para solicitar la ruta del PFX si no se proporcionó
function Get-PfxPath {
    if ([string]::IsNullOrWhiteSpace($script:PfxPath)) {
        $script:PfxPath = Read-Host "Ingresa la ruta completa al archivo PFX del certificado"
    }
    
    if (-not (Test-Path $script:PfxPath)) {
        Write-Host "ERROR: No se encontró el archivo: $script:PfxPath" -ForegroundColor Red
        exit 1
    }
}

# Función para solicitar la contraseña del PFX de forma segura
function Get-PfxPassword {
    if ($null -eq $script:PfxPassword) {
        $script:PfxPassword = Read-Host "Ingresa la contraseña del archivo PFX" -AsSecureString
    }
}

# Función para mostrar información del certificado
function Show-CertificateInfo {
    param([System.Security.Cryptography.X509Certificates.X509Certificate2]$Certificate)
    
    Write-Host ""
    Write-Host "Información del Certificado:" -ForegroundColor Green
    Write-Host "----------------------------"
    Write-Host "Subject:     $($Certificate.Subject)"
    Write-Host "Issuer:      $($Certificate.Issuer)"
    Write-Host "Thumbprint:  $($Certificate.Thumbprint)"
    Write-Host "Serial:      $($Certificate.SerialNumber)"
    Write-Host "Válido desde: $($Certificate.NotBefore.ToString('yyyy-MM-dd HH:mm:ss'))"
    Write-Host "Válido hasta: $($Certificate.NotAfter.ToString('yyyy-MM-dd HH:mm:ss'))"
    
    $daysUntilExpiration = ($Certificate.NotAfter - (Get-Date)).Days
    if ($daysUntilExpiration -lt 0) {
        Write-Host "Estado:      CADUCADO hace $([Math]::Abs($daysUntilExpiration)) días" -ForegroundColor Red
    } elseif ($daysUntilExpiration -lt 30) {
        Write-Host "Estado:      Expira en $daysUntilExpiration días (¡RENOVAR PRONTO!)" -ForegroundColor Yellow
    } else {
        Write-Host "Estado:      Válido (expira en $daysUntilExpiration días)" -ForegroundColor Green
    }
    
    Write-Host "Tiene clave privada: $($Certificate.HasPrivateKey)" -ForegroundColor $(if ($Certificate.HasPrivateKey) { "Green" } else { "Red" })
    
    # Mostrar tipo de clave
    if ($Certificate.PublicKey.Key -is [System.Security.Cryptography.RSA]) {
        $keySize = $Certificate.PublicKey.Key.KeySize
        Write-Host "Tipo de clave: RSA ($keySize bits)"
    } elseif ($Certificate.PublicKey.Key -is [System.Security.Cryptography.ECDsa]) {
        $keySize = $Certificate.PublicKey.Key.KeySize
        Write-Host "Tipo de clave: ECDSA ($keySize bits)"
    }
    
    Write-Host ""
}

# Función principal
function Main {
    # Paso 1: Obtener la ruta del PFX
    Get-PfxPath
    
    Write-Host "Archivo PFX: $PfxPath" -ForegroundColor Cyan
    Write-Host ""
    
    # Paso 2: Obtener la contraseña
    Get-PfxPassword
    
    # Paso 3: Cargar el certificado del archivo PFX
    Write-Host "Cargando certificado desde PFX..." -ForegroundColor Cyan
    
    try {
        $pfxBytes = [System.IO.File]::ReadAllBytes($PfxPath)
        $cert = [System.Security.Cryptography.X509Certificates.X509CertificateLoader]::LoadPkcs12(
            $pfxBytes,
            [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
                [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($PfxPassword)
            ),
            [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable
        )
        
        Write-Host "✓ Certificado cargado correctamente" -ForegroundColor Green
        
        # Mostrar información del certificado
        Show-CertificateInfo -Certificate $cert
        
        # Verificar que tiene clave privada
        if (-not $cert.HasPrivateKey) {
            Write-Host "ERROR: El certificado no contiene clave privada." -ForegroundColor Red
            Write-Host "VERI*FACTU requiere certificados con clave privada para firma digital y mTLS." -ForegroundColor Red
            exit 1
        }
        
        # Verificar fechas de validez
        $now = Get-Date
        if ($now -lt $cert.NotBefore) {
            Write-Host "ERROR: El certificado aún no es válido (válido desde $($cert.NotBefore))" -ForegroundColor Red
            exit 1
        }
        
        if ($now -gt $cert.NotAfter) {
            Write-Host "ERROR: El certificado ha caducado (expiró el $($cert.NotAfter))" -ForegroundColor Red
            exit 1
        }
        
        # Si es solo información, terminar aquí
        if ($InfoOnly) {
            Write-Host "Modo información activado. No se instalará el certificado." -ForegroundColor Yellow
            $cert.Dispose()
            exit 0
        }
        
        # Paso 4: Instalar en el almacén de certificados
        Write-Host ""
        Write-Host "¿Deseas instalar este certificado en el almacén $StoreLocation\$StoreName? (S/N)" -ForegroundColor Yellow
        $confirmation = Read-Host
        
        if ($confirmation -ne 'S' -and $confirmation -ne 's') {
            Write-Host "Instalación cancelada." -ForegroundColor Yellow
            $cert.Dispose()
            exit 0
        }
        
        Write-Host ""
        Write-Host "Instalando certificado en $StoreLocation\$StoreName..." -ForegroundColor Cyan
        
        $store = New-Object System.Security.Cryptography.X509Certificates.X509Store(
            $StoreName,
            $StoreLocation
        )
        $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
        $store.Add($cert)
        $store.Close()
        
        Write-Host "✓ Certificado instalado correctamente" -ForegroundColor Green
        Write-Host ""
        
        # Paso 5: Mostrar configuración para appsettings.json
        Write-Host "Configuración para appsettings.json:" -ForegroundColor Green
        Write-Host "-----------------------------------"
        Write-Host "  ""Certificado"": {"
        Write-Host "    ""Tipo"": ""Almacen"","
        Write-Host "    ""Thumbprint"": ""$($cert.Thumbprint)"","
        Write-Host "    ""StoreLocation"": ""$StoreLocation"","
        Write-Host "    ""StoreName"": ""$StoreName"""
        Write-Host "  }"
        Write-Host ""
        
        Write-Host "También puedes configurarlo con variables de entorno:" -ForegroundColor Green
        Write-Host "  `$env:CERTIFICADO__TIPO = 'Almacen'"
        Write-Host "  `$env:CERTIFICADO__THUMBPRINT = '$($cert.Thumbprint)'"
        Write-Host "  `$env:CERTIFICADO__STORELOCATION = '$StoreLocation'"
        Write-Host "  `$env:CERTIFICADO__STORENAME = '$StoreName'"
        Write-Host ""
        
        # Paso 6: Comando para listar certificados
        Write-Host "Para listar todos los certificados en este almacén:" -ForegroundColor Cyan
        Write-Host "  Get-ChildItem Cert:\$StoreLocation\$StoreName | Format-List Subject, Thumbprint, NotAfter"
        Write-Host ""
        
        $cert.Dispose()
        
        Write-Host "✓ Setup completado exitosamente" -ForegroundColor Green
        
    } catch {
        Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# Ejecutar función principal
Main
