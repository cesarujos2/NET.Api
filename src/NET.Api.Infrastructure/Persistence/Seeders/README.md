# Seeders de Datos - Inicialización de Base de Datos

Esta carpeta contiene los seeders responsables de poblar la base de datos con datos iniciales necesarios para el funcionamiento de la aplicación.

## Archivos

### EmailTemplateSeeder.cs
**Propósito**: Seeder para plantillas de email del sistema.
- **Responsabilidades**:
  - Crear plantillas de email predefinidas para diferentes tipos de notificaciones
  - Configurar contenido HTML y texto plano para emails
  - Establecer plantillas para confirmación de email, recuperación de contraseña, etc.
- **Funcionalidades**:
  - Verifica existencia antes de crear para evitar duplicados
  - Configura plantillas con variables dinámicas (placeholders)
  - Establece configuraciones de formato y estilo
- **Plantillas típicas**:
  - Confirmación de registro de usuario
  - Recuperación de contraseña
  - Notificaciones de cambios de rol
  - Bienvenida a nuevos usuarios

### RoleSeeder.cs
**Propósito**: Seeder para roles del sistema y jerarquía inicial.
- **Responsabilidades**:
  - Crear roles fundamentales del sistema (SuperAdmin, Admin, Manager, User)
  - Establecer jerarquía inicial de roles
  - Configurar permisos y restricciones básicas
- **Funcionalidades**:
  - Crea roles con niveles jerárquicos apropiados
  - Establece roles del sistema que no pueden ser eliminados
  - Configura descripciones y metadatos de roles
- **Roles típicos creados**:
  - **SuperAdmin** (Nivel 1): Acceso completo al sistema
  - **Admin** (Nivel 2): Administración general
  - **Manager** (Nivel 3): Gestión de equipos y usuarios
  - **User** (Nivel 4): Usuario estándar

## Arquitectura de Seeders

### Patrón de Diseño

1. **Idempotencia**:
   - Los seeders pueden ejecutarse múltiples veces sin crear duplicados
   - Verifican existencia antes de crear nuevos registros
   - Actualizan registros existentes si es necesario

2. **Orden de Ejecución**:
   - Los seeders se ejecutan en orden específico para respetar dependencias
   - `RoleSeeder` se ejecuta antes que seeders que dependen de roles
   - `EmailTemplateSeeder` es independiente y puede ejecutarse en cualquier momento

3. **Configuración Flexible**:
   - Datos de seed configurables a través de archivos de configuración
   - Soporte para diferentes entornos (Development, Staging, Production)
   - Capacidad de deshabilitar seeders específicos

### Integración con Entity Framework

```csharp
// Ejemplo de uso en DataSeeder.cs
public async Task SeedAsync()
{
    await RoleSeeder.SeedAsync(_context);
    await EmailTemplateSeeder.SeedAsync(_context);
    // Otros seeders...
}
```

### Ejecución

Los seeders se ejecutan automáticamente:

1. **Durante el inicio de la aplicación** (en Development)
2. **En migraciones de base de datos**
3. **Manualmente a través de comandos CLI**

### Beneficios

1. **Consistencia**:
   - Garantiza que todos los entornos tengan los datos básicos necesarios
   - Establece configuración estándar para nuevas instalaciones
   - Mantiene coherencia entre diferentes despliegues

2. **Automatización**:
   - Reduce configuración manual en nuevos entornos
   - Facilita setup de entornos de desarrollo y testing
   - Acelera procesos de CI/CD

3. **Mantenibilidad**:
   - Centraliza la lógica de inicialización de datos
   - Facilita actualizaciones de configuración base
   - Permite versionado de cambios en datos iniciales

## Mejores Prácticas

### Para RoleSeeder

1. **Jerarquía Clara**: Mantener niveles jerárquicos consistentes
2. **Roles del Sistema**: Marcar roles críticos como no eliminables
3. **Nombres Estándar**: Usar convenciones de nomenclatura consistentes
4. **Metadatos Completos**: Incluir descripciones y configuraciones apropiadas

### Para EmailTemplateSeeder

1. **Plantillas Responsivas**: Crear templates que funcionen en diferentes dispositivos
2. **Variables Dinámicas**: Usar placeholders para contenido personalizable
3. **Fallbacks**: Proporcionar versiones de texto plano para todos los templates
4. **Localización**: Considerar soporte para múltiples idiomas

### Consideraciones de Seguridad

1. **No incluir datos sensibles** en seeders (contraseñas, tokens, etc.)
2. **Usar configuración externa** para datos específicos del entorno
3. **Validar datos** antes de insertar en base de datos
4. **Logging apropiado** para auditoría de cambios

## Extensión

Para agregar nuevos seeders:

1. Crear clase que implemente patrón de seeder existente
2. Agregar verificaciones de idempotencia
3. Registrar en `DataSeeder.cs`
4. Considerar dependencias y orden de ejecución
5. Incluir logging y manejo de errores apropiado

Este sistema de seeders proporciona una base sólida para la inicialización automática y consistente de datos en todos los entornos de la aplicación.