# Scripts de Utilidad

Este directorio contiene scripts de ayuda para la configuración y validación del entorno de VerifactuSender.

## verify-sandbox-setup.sh

Script de verificación de la configuración del entorno sandbox para pruebas con AEAT.

### Uso

```bash
# Desde la raíz del proyecto
./scripts/verify-sandbox-setup.sh
```

### Qué verifica

El script verifica los siguientes aspectos:

1. **✓ .NET SDK** - Verifica que .NET 9 SDK esté instalado
2. **✓ Estructura del proyecto** - Verifica que todos los directorios necesarios existan
3. **✓ Archivos de configuración** - Verifica la presencia de appsettings.json y archivos de configuración
4. **✓ Validez JSON** - Valida la sintaxis de los archivos JSON de configuración
5. **✓ Variables de entorno** - Verifica si las variables de entorno necesarias están configuradas
6. **✓ Conectividad AEAT** - Prueba la conectividad con el endpoint sandbox de AEAT
7. **✓ Compilación** - Verifica que el proyecto compila sin errores
8. **✓ Documentación** - Verifica que la documentación necesaria esté disponible
9. **✓ Seguridad** - Verifica que .gitignore proteja archivos sensibles

### Salida

El script muestra un resumen con:
- **Pasadas** (✓): Verificaciones exitosas
- **Advertencias** (⚠): Configuraciones opcionales o problemas no críticos
- **Fallidas** (✗): Problemas que deben resolverse

### Código de salida

- `0`: Todo correcto (0 fallos)
- `1`: Hay problemas que requieren atención

### Requisitos

- Bash shell (Linux/macOS/WSL)
- Comandos opcionales para verificaciones completas:
  - `dotnet` - Para verificar SDK y compilación
  - `python3` - Para validar sintaxis JSON
  - `curl` - Para verificar conectividad

### Ejemplo de salida

```
==================================================
  Verificación de Entorno Sandbox VERI*FACTU
==================================================

1. Verificando .NET SDK...
✓ .NET SDK instalado: v9.0.305

2. Verificando estructura del proyecto...
✓ Archivo de solución encontrado
✓ Directorio de demo encontrado
✓ Directorio de librería cliente encontrado

...

==================================================
                    RESUMEN
==================================================
Pasadas: 14
Advertencias: 4
Fallidas: 0

✓ El entorno está configurado correctamente
```

## Uso en Windows

Para ejecutar en Windows, usa Git Bash o WSL:

```powershell
# Git Bash
bash scripts/verify-sandbox-setup.sh

# WSL
wsl ./scripts/verify-sandbox-setup.sh
```

## Contribuir

Si deseas añadir más scripts de utilidad, asegúrate de:

1. Documentarlos en este README
2. Hacerlos ejecutables: `chmod +x script.sh`
3. Incluir comentarios descriptivos en el código
4. Manejar errores apropiadamente
5. Proporcionar salida clara y útil

---

**Última actualización**: 30 de octubre de 2025
