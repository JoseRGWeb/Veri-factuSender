# Script de Diagnóstico de Certificados para VERI*FACTU
# =======================================================
# Este script diagnostica problemas comunes con certificados digitales
# para el sistema VERI*FACTU de la AEAT.
#
# Uso:
#   .\diagnose-certificates.ps1
#   .\diagnose-certificates.ps1 -Thumbprint "ABC123..."
#   .\diagnose-certificates.ps1 -PfxPath "C:\certs\certificado.pfx"
#
# Requisitos:
#   - PowerShell 5.1 o superior

param(
    [Parameter(HelpMessage="Thumbprint del certificado a diagnosticar")]
    [string]$Thumbprint = "",
    
    [Parameter(HelpMessage="Ruta a archivo PFX para diagnosticar")]
    [string]$PfxPath = "",
    
    [Parameter(HelpMessage="Ubicación del almacén de certificados")]
    [ValidateSet("CurrentUser", "LocalMachine", "Both")]
    [string]$StoreLocation = "Both",
    
    [Parameter(HelpMessage="Mostrar todos los certificados disponibles")]
    [switch]$ListAll
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Diagnóstico de Certificados VERI*FACTU" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Función para verificar requisitos de VERI*FACTU
function Test-VerifactuRequirements {
    param([System.Security.Cryptography.X509Certificates.X509Certificate2]$Certificate)
    
    $passed = $true
    $warnings = @()
    $errors = @()
    
    Write-Host "Verificando requisitos de VERI*FACTU..." -ForegroundColor Cyan
    Write-Host ""
    
    # 1. Verificar clave privada
    Write-Host "✓ Verificando clave privada..." -NoNewline
    if ($Certificate.HasPrivateKey) {
        Write-Host " OK" -ForegroundColor Green
    } else {
        Write-Host " FALLO" -ForegroundColor Red
        $errors += "El certificado NO tiene clave privada. VERI*FACTU requiere clave privada para firma y mTLS."
        $passed = $false
    }
    
    # 2. Verificar fechas de validez
    $now = Get-Date
    Write-Host "✓ Verificando fechas de validez..." -NoNewline
    if ($now -lt $Certificate.NotBefore) {
        Write-Host " FALLO" -ForegroundColor Red
        $errors += "El certificado aún no es válido. Será válido desde: $($Certificate.NotBefore)"
        $passed = $false
    } elseif ($now -gt $Certificate.NotAfter) {
        Write-Host " FALLO" -ForegroundColor Red
        $errors += "El certificado ha CADUCADO. Expiró el: $($Certificate.NotAfter)"
        $passed = $false
    } else {
        $daysLeft = ($Certificate.NotAfter - $now).Days
        if ($daysLeft -lt 30) {
            Write-Host " ADVERTENCIA" -ForegroundColor Yellow
            $warnings += "El certificado expira pronto: $daysLeft días restantes. Considera renovarlo."
        } else {
            Write-Host " OK ($daysLeft días restantes)" -ForegroundColor Green
        }
    }
    
    # 3. Verificar tipo y tamaño de clave
    Write-Host "✓ Verificando tipo de clave..." -NoNewline
    if ($Certificate.PublicKey.Key -is [System.Security.Cryptography.RSA]) {
        $keySize = $Certificate.PublicKey.Key.KeySize
        if ($keySize -lt 2048) {
            Write-Host " FALLO" -ForegroundColor Red
            $errors += "La clave RSA es de $keySize bits. VERI*FACTU requiere mínimo 2048 bits."
            $passed = $false
        } else {
            Write-Host " OK (RSA $keySize bits)" -ForegroundColor Green
        }
    } elseif ($Certificate.PublicKey.Key -is [System.Security.Cryptography.ECDsa]) {
        $keySize = $Certificate.PublicKey.Key.KeySize
        if ($keySize -lt 256) {
            Write-Host " FALLO" -ForegroundColor Red
            $errors += "La clave ECDSA es de $keySize bits. VERI*FACTU requiere mínimo 256 bits."
            $passed = $false
        } else {
            Write-Host " OK (ECDSA $keySize bits)" -ForegroundColor Green
        }
    } else {
        Write-Host " DESCONOCIDO" -ForegroundColor Yellow
        $warnings += "Tipo de clave desconocido. Verifica compatibilidad con VERI*FACTU."
    }
    
    # 4. Verificar algoritmo de firma
    Write-Host "✓ Verificando algoritmo de firma..." -NoNewline
    $sigAlg = $Certificate.SignatureAlgorithm.FriendlyName
    if ($sigAlg -match "sha256|sha384|sha512") {
        Write-Host " OK ($sigAlg)" -ForegroundColor Green
    } else {
        Write-Host " ADVERTENCIA" -ForegroundColor Yellow
        $warnings += "El algoritmo de firma '$sigAlg' puede no ser aceptado. Se recomienda SHA-256 o superior."
    }
    
    # 5. Verificar Enhanced Key Usage (si existe)
    Write-Host "✓ Verificando propósitos del certificado..." -NoNewline
    $hasClientAuth = $false
    $purposes = @()
    
    foreach ($ext in $Certificate.Extensions) {
        if ($ext -is [System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension]) {
            foreach ($oid in $ext.EnhancedKeyUsages) {
                $purposes += "$($oid.FriendlyName) ($($oid.Value))"
                if ($oid.Value -eq "1.3.6.1.5.5.7.3.2") {
                    $hasClientAuth = $true
                }
            }
        }
    }
    
    if ($purposes.Count -eq 0) {
        Write-Host " OK (sin restricciones)" -ForegroundColor Green
    } elseif ($hasClientAuth) {
        Write-Host " OK (Client Authentication presente)" -ForegroundColor Green
    } else {
        Write-Host " ADVERTENCIA" -ForegroundColor Yellow
        $warnings += "No se encontró propósito 'Client Authentication'. Verifica compatibilidad para mTLS."
    }
    
    # 6. Verificar cadena de confianza
    Write-Host "✓ Verificando cadena de confianza..." -NoNewline
    $chain = New-Object System.Security.Cryptography.X509Certificates.X509Chain
    $chain.ChainPolicy.RevocationMode = [System.Security.Cryptography.X509Certificates.X509RevocationMode]::NoCheck
    
    if ($chain.Build($Certificate)) {
        Write-Host " OK" -ForegroundColor Green
    } else {
        Write-Host " ADVERTENCIA" -ForegroundColor Yellow
        $warnings += "Problemas con la cadena de confianza. En sandbox/pruebas esto es aceptable."
        foreach ($status in $chain.ChainStatus) {
            $warnings += "  - $($status.StatusInformation)"
        }
    }
    
    # Resumen
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "RESUMEN DEL DIAGNÓSTICO" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    if ($passed -and $warnings.Count -eq 0) {
        Write-Host "✓ El certificado cumple TODOS los requisitos de VERI*FACTU" -ForegroundColor Green
    } elseif ($passed) {
        Write-Host "✓ El certificado cumple los requisitos básicos" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Advertencias ($($warnings.Count)):" -ForegroundColor Yellow
        foreach ($warning in $warnings) {
            Write-Host "  ⚠ $warning" -ForegroundColor Yellow
        }
    } else {
        Write-Host "✗ El certificado NO cumple los requisitos de VERI*FACTU" -ForegroundColor Red
        Write-Host ""
        Write-Host "Errores ($($errors.Count)):" -ForegroundColor Red
        foreach ($error in $errors) {
            Write-Host "  ✗ $error" -ForegroundColor Red
        }
        
        if ($warnings.Count -gt 0) {
            Write-Host ""
            Write-Host "Advertencias ($($warnings.Count)):" -ForegroundColor Yellow
            foreach ($warning in $warnings) {
                Write-Host "  ⚠ $warning" -ForegroundColor Yellow
            }
        }
    }
    
    Write-Host ""
}

# Función para mostrar información detallada
function Show-DetailedCertificateInfo {
    param([System.Security.Cryptography.X509Certificates.X509Certificate2]$Certificate)
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "INFORMACIÓN DETALLADA DEL CERTIFICADO" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "Subject:        $($Certificate.Subject)"
    Write-Host "Issuer:         $($Certificate.Issuer)"
    Write-Host "Thumbprint:     $($Certificate.Thumbprint)"
    Write-Host "Serial Number:  $($Certificate.SerialNumber)"
    Write-Host "Version:        $($Certificate.Version)"
    Write-Host ""
    
    Write-Host "Válido desde:   $($Certificate.NotBefore.ToString('yyyy-MM-dd HH:mm:ss'))"
    Write-Host "Válido hasta:   $($Certificate.NotAfter.ToString('yyyy-MM-dd HH:mm:ss'))"
    
    $daysLeft = ($Certificate.NotAfter - (Get-Date)).Days
    Write-Host "Días restantes: $daysLeft"
    Write-Host ""
    
    Write-Host "Tiene clave privada: $($Certificate.HasPrivateKey)"
    Write-Host "Algoritmo de firma:  $($Certificate.SignatureAlgorithm.FriendlyName)"
    
    if ($Certificate.PublicKey.Key -is [System.Security.Cryptography.RSA]) {
        Write-Host "Tipo de clave:       RSA ($($Certificate.PublicKey.Key.KeySize) bits)"
    } elseif ($Certificate.PublicKey.Key -is [System.Security.Cryptography.ECDsa]) {
        Write-Host "Tipo de clave:       ECDSA ($($Certificate.PublicKey.Key.KeySize) bits)"
    }
    
    Write-Host ""
    Write-Host "Propósitos (Enhanced Key Usage):"
    $foundEKU = $false
    foreach ($ext in $Certificate.Extensions) {
        if ($ext -is [System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension]) {
            $foundEKU = $true
            foreach ($oid in $ext.EnhancedKeyUsages) {
                Write-Host "  - $($oid.FriendlyName) ($($oid.Value))"
            }
        }
    }
    if (-not $foundEKU) {
        Write-Host "  (sin restricciones de uso)"
    }
    
    Write-Host ""
}

# Función para listar certificados en almacén
function List-StoreCertificates {
    param([string]$Location)
    
    Write-Host ""
    Write-Host "Certificados en $Location\My:" -ForegroundColor Cyan
    Write-Host "----------------------------"
    
    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("My", $Location)
    $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadOnly)
    
    $certs = $store.Certificates
    
    if ($certs.Count -eq 0) {
        Write-Host "  (no hay certificados)" -ForegroundColor Gray
    } else {
        foreach ($cert in $certs) {
            $daysLeft = ($cert.NotAfter - (Get-Date)).Days
            $status = if ($daysLeft -lt 0) { "CADUCADO" } elseif ($daysLeft -lt 30) { "Expira pronto" } else { "Válido" }
            $statusColor = if ($daysLeft -lt 0) { "Red" } elseif ($daysLeft -lt 30) { "Yellow" } else { "Green" }
            
            Write-Host ""
            Write-Host "Subject:    $($cert.Subject)"
            Write-Host "Thumbprint: $($cert.Thumbprint)"
            Write-Host "Expira:     $($cert.NotAfter.ToString('yyyy-MM-dd')) ($status)" -ForegroundColor $statusColor
            Write-Host "Clave priv: $($cert.HasPrivateKey)"
        }
    }
    
    $store.Close()
    Write-Host ""
}

# Función principal
function Main {
    # Listar todos los certificados si se solicitó
    if ($ListAll) {
        if ($StoreLocation -eq "Both") {
            List-StoreCertificates -Location "CurrentUser"
            List-StoreCertificates -Location "LocalMachine"
        } else {
            List-StoreCertificates -Location $StoreLocation
        }
        return
    }
    
    # Diagnosticar certificado específico desde PFX
    if (-not [string]::IsNullOrWhiteSpace($PfxPath)) {
        if (-not (Test-Path $PfxPath)) {
            Write-Host "ERROR: No se encontró el archivo: $PfxPath" -ForegroundColor Red
            exit 1
        }
        
        $password = Read-Host "Ingresa la contraseña del archivo PFX" -AsSecureString
        
        try {
            $pfxBytes = [System.IO.File]::ReadAllBytes($PfxPath)
            $cert = [System.Security.Cryptography.X509Certificates.X509CertificateLoader]::LoadPkcs12(
                $pfxBytes,
                [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
                    [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
                ),
                [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable
            )
            
            Show-DetailedCertificateInfo -Certificate $cert
            Test-VerifactuRequirements -Certificate $cert
            
            $cert.Dispose()
        } catch {
            Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
            exit 1
        }
        
        return
    }
    
    # Diagnosticar certificado desde almacén por thumbprint
    if (-not [string]::IsNullOrWhiteSpace($Thumbprint)) {
        $found = $false
        
        $locations = if ($StoreLocation -eq "Both") { @("CurrentUser", "LocalMachine") } else { @($StoreLocation) }
        
        foreach ($loc in $locations) {
            $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("My", $loc)
            $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadOnly)
            
            $certs = $store.Certificates.Find(
                [System.Security.Cryptography.X509Certificates.X509FindType]::FindByThumbprint,
                $Thumbprint,
                $false
            )
            
            if ($certs.Count -gt 0) {
                $found = $true
                Write-Host "Certificado encontrado en $loc\My" -ForegroundColor Green
                
                Show-DetailedCertificateInfo -Certificate $certs[0]
                Test-VerifactuRequirements -Certificate $certs[0]
                
                break
            }
            
            $store.Close()
        }
        
        if (-not $found) {
            Write-Host "ERROR: No se encontró ningún certificado con thumbprint: $Thumbprint" -ForegroundColor Red
            Write-Host ""
            Write-Host "Usa el parámetro -ListAll para ver todos los certificados disponibles." -ForegroundColor Yellow
            exit 1
        }
        
        return
    }
    
    # Si no se especificó nada, mostrar ayuda
    Write-Host "Uso:" -ForegroundColor Yellow
    Write-Host "  .\diagnose-certificates.ps1 -ListAll                    # Listar todos los certificados"
    Write-Host "  .\diagnose-certificates.ps1 -Thumbprint 'ABC123...'     # Diagnosticar por thumbprint"
    Write-Host "  .\diagnose-certificates.ps1 -PfxPath 'C:\cert.pfx'      # Diagnosticar archivo PFX"
    Write-Host ""
    Write-Host "Ejemplo de configuración válida para VERI*FACTU:" -ForegroundColor Cyan
    Write-Host "  - Certificado con clave privada"
    Write-Host "  - RSA ≥ 2048 bits o ECDSA ≥ 256 bits"
    Write-Host "  - SHA-256 o superior"
    Write-Host "  - Dentro del período de validez"
    Write-Host ""
}

# Ejecutar función principal
Main
