# Sistema de Gestión de Roles

## Descripción General

El sistema de gestión de roles de NET.Api implementa un modelo de autorización basado en jerarquías de roles con políticas personalizadas. Este sistema permite controlar el acceso a recursos y funcionalidades según el nivel de autoridad del usuario.

## Jerarquía de Roles

### Estructura Jerárquica

Los roles están organizados en una jerarquía donde cada rol tiene un nivel numérico que determina su autoridad:

| Rol | Nivel | Descripción |
|-----|-------|-------------|
| **Owner** | 100 | Máximo nivel de autoridad, acceso completo al sistema |
| **Admin** | 80 | Administrador del sistema, gestión de usuarios y configuración |
| **Moderator** | 60 | Moderación de contenido y usuarios |
| **Support** | 40 | Soporte técnico y atención al cliente |
| **User** | 20 | Usuario estándar con permisos básicos |

### Principio de Autoridad

Un usuario con un rol de mayor jerarquía puede realizar todas las acciones de los roles inferiores. Por ejemplo:
- Un **Admin** (80) puede realizar acciones de **Moderator** (60), **Support** (40) y **User** (20)
- Un **User** (20) solo puede realizar acciones específicas de su nivel

## Arquitectura del Sistema

### Componentes Principales

#### 1. Constantes de Roles (`RoleConstants.cs`)

```csharp
public static class RoleConstants
{
    public static class Names
    {
        public const string Owner = "Owner";
        public const string Admin = "Admin";
        public const string Moderator = "Moderator";
        public const string Support = "Support";
        public const string User = "User";
    }

    public static class Hierarchy
    {
        public const int Owner = 100;
        public const int Admin = 80;
        public const int Moderator = 60;
        public const int Support = 40;
        public const int User = 20;
    }
}
```

#### 2. Servicio de Gestión de Roles (`RoleManagementService.cs`)

Implementa la lógica de negocio para:
- Validación de jerarquías
- Verificación de autoridad suficiente
- Gestión de asignación de roles

```csharp
public bool HasSufficientAuthority(IEnumerable<string> userRoles, string requiredRole)
{
    var highestUserRole = GetHighestRole(userRoles);
    return RoleConstants.HasSufficientAuthority(highestUserRole, requiredRole);
}
```

#### 3. Políticas de Autorización (`AuthorizationPolicyProvider.cs`)

Configura las políticas del sistema:

##### Políticas por Rol Específico
- `RequireOwnerRole`: Solo usuarios Owner
- `RequireAdminRole`: Solo usuarios Admin
- `RequireModeratorRole`: Solo usuarios Moderator
- `RequireSupportRole`: Solo usuarios Support
- `RequireUserRole`: Solo usuarios User

##### Políticas por Jerarquía
- `RequireAdminOrAbove`: Admin, Owner
- `RequireModeratorOrAbove`: Moderator, Admin, Owner
- `RequireElevatedRoles`: Moderator, Admin, Owner

##### Políticas por Permisos
- `CanManageUsers`: Gestión de usuarios
- `CanManageRoles`: Gestión de roles
- `CanViewReports`: Visualización de reportes
- `CanModerateContent`: Moderación de contenido
- `CanAccessSupport`: Acceso a soporte

#### 4. Handlers de Autorización

##### RoleHierarchyHandler
Valida si un usuario tiene suficiente autoridad para acceder a un recurso:

```csharp
protected override Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    RoleHierarchyRequirement requirement)
{
    var userRoles = context.User.FindAll("role").Select(c => c.Value);
    
    if (roleManagementService.HasSufficientAuthority(userRoles, requirement.RequiredRole))
    {
        context.Succeed(requirement);
    }
    else
    {
        context.Fail();
    }
    
    return Task.CompletedTask;
}
```

##### MultipleRolesHandler
Permite acceso si el usuario tiene cualquiera de los roles especificados.

##### PermissionHandler
Valida permisos específicos basados en la lógica de negocio.

## Configuración

### Registro de Servicios

En `DependencyInjection.cs`:

```csharp
// Configurar políticas de autorización
services.ConfigureAuthorizationPolicies();
services.RegisterAuthorizationHandlers();

// Registrar servicios de dominio
services.AddScoped<IRoleManagementService, RoleManagementService>();
```

### Configuración JWT

Para que los roles funcionen correctamente con JWT:

```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    // Mapear claims de JWT a ClaimTypes estándar
    NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
};
```

### Deshabilitación del Mapeo Automático

En `Program.cs`:

```csharp
// Deshabilitar el mapeo automático de claims inbound de JWT
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
```

## Uso en Controladores

### Aplicación de Políticas

```csharp
[Authorize(Policy = Policies.RequireAdminOrAbove)]
public class EmailTemplatesController : ControllerBase
{
    // Solo usuarios Admin o Owner pueden acceder
}

[HttpGet("admin-only")]
[Authorize(Policy = Policies.RequireAdminRole)]
public IActionResult AdminOnlyEndpoint()
{
    // Solo usuarios Admin específicamente
}

[HttpGet("elevated-access")]
[Authorize(Policy = Policies.RequireElevatedRoles)]
public IActionResult ElevatedAccessEndpoint()
{
    // Moderator, Admin o Owner
}
```

### Verificación Programática

```csharp
public class SomeService
{
    private readonly IRoleManagementService _roleService;
    
    public bool CanUserPerformAction(IEnumerable<string> userRoles, string requiredRole)
    {
        return _roleService.HasSufficientAuthority(userRoles, requiredRole);
    }
}
```

## Ejemplos de Uso

### Escenario 1: Gestión de Plantillas de Email

```csharp
[Authorize(Policy = Policies.RequireAdminOrAbove)]
public class EmailTemplatesController : ControllerBase
{
    // Solo Admin y Owner pueden gestionar plantillas
}
```

### Escenario 2: Moderación de Contenido

```csharp
[Authorize(Policy = Policies.CanModerateContent)]
public IActionResult ModerateContent()
{
    // Moderator, Admin y Owner pueden moderar
}
```

### Escenario 3: Gestión de Usuarios

```csharp
[Authorize(Policy = Policies.CanManageUsers)]
public IActionResult ManageUsers()
{
    // Solo Admin y Owner pueden gestionar usuarios
}
```

## Seguridad y Mejores Prácticas

### 1. Principio de Menor Privilegio
- Asignar el rol mínimo necesario para cada usuario
- Revisar periódicamente los permisos asignados

### 2. Validación de Tokens
- Los roles se extraen del claim "role" en el JWT
- Se valida la firma y expiración del token

### 3. Logging y Auditoría
- Todas las decisiones de autorización se registran
- Se mantiene un historial de cambios de roles

### 4. Configuración Segura
- Claims types configurados explícitamente
- Mapeo automático de JWT deshabilitado

## Troubleshooting

### Problema: Usuario no puede acceder a recursos

1. **Verificar el token JWT**:
   - Decodificar el token y verificar el claim "role"
   - Verificar que el token no haya expirado

2. **Verificar la política aplicada**:
   - Confirmar que la política del controlador/acción es correcta
   - Verificar que el rol del usuario tiene suficiente autoridad

3. **Revisar logs de autorización**:
   - Buscar logs del `RoleHierarchyHandler`
   - Verificar logs del `RoleManagementService`

### Problema: Autorización no funciona correctamente

1. **Verificar registro de handlers**:
   ```csharp
   services.RegisterAuthorizationHandlers();
   ```

2. **Verificar configuración de JWT**:
   ```csharp
   RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
   ```

3. **Verificar mapeo de claims**:
   ```csharp
   JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
   ```

## Extensibilidad

### Agregar Nuevos Roles

1. Actualizar `RoleConstants.cs`:
   ```csharp
   public const string NewRole = "NewRole";
   public const int NewRole = 50; // En Hierarchy
   ```

2. Actualizar `GetRoleHierarchy()` en `RoleConstants.cs`

3. Crear nuevas políticas si es necesario

### Agregar Nuevos Permisos

1. Definir nueva política en `AuthorizationPolicyProvider.cs`
2. Actualizar `PermissionHandler.cs` con la nueva lógica
3. Aplicar la política en controladores

## Conclusión

El sistema de gestión de roles proporciona un framework robusto y flexible para controlar el acceso a recursos en la aplicación. Su diseño basado en jerarquías y políticas permite una gestión granular de permisos mientras mantiene la simplicidad en el uso.