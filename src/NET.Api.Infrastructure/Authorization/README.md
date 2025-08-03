# Sistema de Autorización - Infraestructura

Esta carpeta contiene la implementación del sistema de autorización basado en roles y jerarquías para la aplicación.

## Archivos

### AuthorizationPolicyProvider.cs
**Propósito**: Proveedor personalizado de políticas de autorización.
- **Responsabilidades**:
  - Crear políticas de autorización dinámicamente
  - Gestionar políticas basadas en jerarquía de roles
  - Integrar con el sistema de autorización de ASP.NET Core
- **Funcionalidades**:
  - Generación automática de políticas para roles específicos
  - Soporte para políticas jerárquicas (rol X o superior)
  - Cacheo de políticas para mejor rendimiento
- **Integración**: Se registra en el contenedor de DI y se usa automáticamente por ASP.NET Core

## Subcarpetas

### Handlers/
Contiene los manejadores de autorización que implementan la lógica específica de evaluación.

#### RoleHierarchyHandler.cs
**Propósito**: Manejador de autorización para requisitos de jerarquía de roles.
- **Responsabilidades**:
  - Evaluar si un usuario cumple con requisitos jerárquicos de roles
  - Verificar autoridad suficiente basada en jerarquía
  - Integrar con servicios de dominio para validaciones
- **Funcionamiento**:
  - Recibe contexto de autorización y requisitos
  - Consulta servicios de jerarquía de roles
  - Determina si el usuario tiene autorización suficiente
- **Casos de uso**:
  - Proteger endpoints que requieren roles específicos o superiores
  - Validar operaciones que requieren autoridad jerárquica
  - Controlar acceso basado en nivel de rol

### Requirements/
Contiene las definiciones de requisitos de autorización personalizados.

#### RoleHierarchyRequirement.cs
**Propósito**: Define requisitos de autorización basados en jerarquía de roles.
- **Responsabilidades**:
  - Especificar el rol mínimo requerido para una operación
  - Definir parámetros para evaluación jerárquica
  - Proporcionar metadatos para el sistema de autorización
- **Propiedades**:
  - `RequiredRole`: Rol mínimo necesario
  - `AllowHigherRoles`: Si se permiten roles superiores
  - `StrictMatch`: Si requiere coincidencia exacta
- **Uso**: Se utiliza en atributos `[Authorize]` y políticas personalizadas

## Arquitectura del Sistema de Autorización

### Flujo de Autorización

1. **Solicitud HTTP** → Middleware de autorización
2. **AuthorizationPolicyProvider** → Crea/obtiene política apropiada
3. **RoleHierarchyHandler** → Evalúa requisitos específicos
4. **Servicios de Dominio** → Consulta jerarquía y validaciones
5. **Decisión** → Autoriza o deniega acceso

### Integración con ASP.NET Core

```csharp
// Ejemplo de uso en controladores
[Authorize(Policy = "RequireManagerOrHigher")]
public async Task<IActionResult> ManageUsers()
{
    // Solo usuarios con rol Manager o superior pueden acceder
}
```

### Beneficios del Diseño

1. **Flexibilidad**:
   - Políticas dinámicas basadas en configuración
   - Soporte para jerarquías complejas
   - Extensible para nuevos tipos de requisitos

2. **Performance**:
   - Cacheo de políticas frecuentemente usadas
   - Evaluación eficiente de jerarquías
   - Minimiza consultas a base de datos

3. **Mantenibilidad**:
   - Separación clara entre requisitos y manejadores
   - Lógica de autorización centralizada
   - Fácil testing y debugging

4. **Seguridad**:
   - Evaluación consistente de permisos
   - Prevención de escalación de privilegios
   - Auditoría completa de decisiones de autorización

## Configuración

El sistema se configura en `DependencyInjection.cs`:

```csharp
services.AddAuthorization(options =>
{
    // Configuración de políticas base
});

services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
services.AddScoped<IAuthorizationHandler, RoleHierarchyHandler>();
```

## Extensibilidad

Para agregar nuevos tipos de autorización:

1. Crear nuevo `IAuthorizationRequirement`
2. Implementar `AuthorizationHandler<T>` correspondiente
3. Registrar en el contenedor de DI
4. Usar en políticas y atributos de autorización

Este diseño permite un sistema de autorización robusto, flexible y mantenible que se integra perfectamente con la arquitectura de la aplicación.