# Implementaciones de Servicios de Roles - Infraestructura

Esta carpeta contiene las implementaciones concretas de todos los servicios relacionados con la gestión de roles.

## Servicios Actuales (Arquitectura Refactorizada)

### RoleAuthorizationService.cs
**Propósito**: Implementación del servicio de autorización de roles.
- **Interfaz implementada**: `IRoleAuthorizationService`
- **Responsabilidades**:
  - Evaluar permisos de asignación y remoción de roles
  - Verificar autoridad jerárquica entre usuarios
  - Aplicar reglas de negocio de autorización
- **Métodos principales**:
  - `CanAssignRoleAsync`: Verifica si un usuario puede asignar un rol específico
  - `CanRemoveRoleAsync`: Verifica si un usuario puede remover un rol específico
  - `HasSufficientAuthorityAsync`: Evalúa si un usuario tiene autoridad sobre otro
- **Dependencias**: Utiliza `IRoleHierarchyService` para evaluaciones jerárquicas

### RoleHierarchyService.cs
**Propósito**: Implementación del servicio de jerarquía de roles.
- **Interfaz implementada**: `IRoleHierarchyService`
- **Responsabilidades**:
  - Gestionar niveles jerárquicos de roles
  - Comparar autoridad entre diferentes roles
  - Mantener integridad de la estructura jerárquica
- **Métodos principales**:
  - `GetRoleHierarchyLevelAsync`: Obtiene el nivel jerárquico de un rol
  - `IsHigherThanAsync`: Compara jerarquía entre dos roles
  - `GetHighestRoleAsync`: Obtiene el rol de mayor jerarquía de un usuario
  - `IsValidRoleAsync`: Valida existencia y estado de un rol
- **Lógica**: Implementa sistema de niveles numéricos (menor número = mayor autoridad)

### RoleValidationService.cs
**Propósito**: Implementación del servicio de validación de roles.
- **Interfaz implementada**: `IRoleValidationService`
- **Responsabilidades**:
  - Validar reglas de negocio para operaciones de roles
  - Verificar restricciones de creación y eliminación
  - Aplicar validaciones de formato y unicidad
- **Métodos principales**:
  - `CanCreateRoleAsync`: Valida si se puede crear un rol
  - `CanDeleteRoleAsync`: Valida si se puede eliminar un rol
  - `IsValidRoleNameAsync`: Verifica formato y unicidad de nombres
  - `ValidateRoleConstraintsAsync`: Aplica restricciones específicas del dominio
- **Validaciones**: Nombres únicos, formatos válidos, restricciones de sistema

### RoleManagementService.cs
**Propósito**: Implementación del servicio de aplicación para gestión de roles.
- **Interfaz implementada**: `IRoleManagementApplicationService`
- **Responsabilidades**:
  - Orquestar operaciones complejas de gestión de roles
  - Coordinar servicios de dominio para operaciones CRUD
  - Manejar transacciones y consistencia de datos
- **Métodos principales**:
  - `CreateRoleAsync`: Crea nuevos roles con validaciones completas
  - `UpdateRoleAsync`: Actualiza roles existentes
  - `DeleteRoleAsync`: Elimina roles con verificaciones de seguridad
  - `AssignRoleToUserAsync`: Asigna roles a usuarios
  - `RemoveRoleFromUserAsync`: Remueve roles de usuarios
- **Patrón**: Implementa patrón de Servicio de Aplicación (Application Service)

### RoleQueryService.cs
**Propósito**: Implementación del servicio de consultas de roles.
- **Interfaz implementada**: `IRoleQueryService`
- **Responsabilidades**:
  - Proporcionar consultas optimizadas de solo lectura
  - Implementar patrones de consulta específicos
  - Manejar proyecciones y transformaciones de datos
- **Métodos principales**:
  - `GetAssignableRolesAsync`: Obtiene roles que un usuario puede asignar
  - `GetUserRolesAsync`: Obtiene roles de un usuario específico
  - `GetActiveRolesAsync`: Obtiene todos los roles activos
  - `GetUsersWithMultipleRolesAsync`: Consulta usuarios con múltiples roles
- **Patrón**: Implementa CQRS (Command Query Responsibility Segregation)



## Arquitectura y Patrones

### Principios de Diseño

1. **Single Responsibility Principle (SRP)**:
   - Cada servicio tiene una responsabilidad específica y bien definida
   - Separación clara entre autorización, jerarquía, validación y gestión

2. **Dependency Inversion Principle (DIP)**:
   - Servicios dependen de abstracciones (interfaces) no de implementaciones
   - Facilita testing con mocks y cambio de implementaciones

3. **Open/Closed Principle (OCP)**:
   - Servicios abiertos para extensión, cerrados para modificación
   - Nuevas funcionalidades se agregan sin modificar código existente

### Patrones Implementados

1. **Domain Services**: Encapsulan lógica de negocio que no pertenece a entidades
2. **Application Services**: Orquestan operaciones y coordinan servicios de dominio
3. **CQRS**: Separación entre comandos (escritura) y consultas (lectura)
4. **Repository Pattern**: Acceso a datos abstraído a través de repositorios

### Beneficios de la Refactorización

1. **Mantenibilidad**: Código más limpio y fácil de mantener
2. **Testabilidad**: Servicios pequeños y enfocados, fáciles de probar
3. **Escalabilidad**: Arquitectura que soporta crecimiento y cambios
4. **Reutilización**: Servicios especializados reutilizables en diferentes contextos

## Migración y Transición

### Estado Actual
- ✅ Servicios nuevos implementados y funcionando
- ✅ Migración completada exitosamente
- ✅ Arquitectura limpia completamente implementada

### Próximos Pasos
1. Continuar mantenimiento y mejoras de los servicios actuales
2. Implementar nuevas funcionalidades siguiendo los patrones establecidos
3. Optimizar rendimiento basado en métricas de uso
4. Expandir cobertura de tests según sea necesario

## Testing

Cada servicio debe tener:
- Tests unitarios para lógica de negocio
- Tests de integración para interacciones con base de datos
- Mocks apropiados para dependencias externas
- Cobertura de casos edge y manejo de errores

## Performance

Consideraciones de rendimiento:
- Cacheo de consultas frecuentes de jerarquía
- Optimización de consultas de base de datos
- Lazy loading para operaciones costosas
- Paginación en consultas que retornan muchos resultados

Esta arquitectura proporciona una base sólida, mantenible y escalable para la gestión de roles en la aplicación.