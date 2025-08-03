# Servicios de Dominio - Gestión de Roles

Esta carpeta contiene las interfaces de servicios de dominio que encapsulan la lógica de negocio relacionada con la gestión de roles.

## Archivos

### IDomainService.cs
**Propósito**: Interfaz base para todos los servicios de dominio.
- Define el contrato común para servicios de dominio
- Establece patrones y convenciones para implementaciones
- Proporciona estructura base para servicios especializados

### IRoleAuthorizationService.cs
**Propósito**: Servicio de dominio para lógica de autorización de roles.
- **Responsabilidades**:
  - Validar si un usuario puede asignar roles específicos
  - Verificar autoridad suficiente para operaciones de roles
  - Evaluar permisos basados en jerarquía de roles
- **Métodos principales**:
  - `CanAssignRoleAsync`: Verifica si se puede asignar un rol
  - `CanRemoveRoleAsync`: Verifica si se puede remover un rol
  - `HasSufficientAuthorityAsync`: Evalúa autoridad del usuario
- **Principios**: Encapsula reglas de negocio de autorización

### IRoleHierarchyService.cs
**Propósito**: Servicio de dominio para gestión de jerarquía de roles.
- **Responsabilidades**:
  - Gestionar niveles jerárquicos de roles
  - Comparar autoridad entre diferentes roles
  - Validar estructura jerárquica del sistema
- **Métodos principales**:
  - `GetRoleHierarchyLevelAsync`: Obtiene nivel jerárquico de un rol
  - `IsHigherThanAsync`: Compara jerarquía entre roles
  - `GetHighestRoleAsync`: Obtiene el rol de mayor jerarquía
  - `IsValidRoleAsync`: Valida existencia y validez de roles
- **Principios**: Mantiene integridad de la jerarquía de roles

### IRoleValidationService.cs
**Propósito**: Servicio de dominio para validaciones de reglas de negocio de roles.
- **Responsabilidades**:
  - Validar reglas de creación y eliminación de roles
  - Verificar unicidad y formato de nombres de roles
  - Aplicar restricciones de negocio específicas
- **Métodos principales**:
  - `CanCreateRoleAsync`: Valida si se puede crear un rol
  - `CanDeleteRoleAsync`: Valida si se puede eliminar un rol
  - `IsValidRoleNameAsync`: Verifica formato y unicidad de nombres
  - `ValidateRoleConstraintsAsync`: Aplica restricciones específicas
- **Principios**: Garantiza integridad y consistencia de datos

## Arquitectura de Dominio

### Principios de Diseño

1. **Domain-Driven Design (DDD)**:
   - Servicios encapsulan lógica de negocio compleja
   - No pertenecen naturalmente a una entidad específica
   - Mantienen el modelo de dominio rico y expresivo

2. **Separación de Responsabilidades**:
   - Cada servicio tiene una responsabilidad específica y bien definida
   - Autorización, jerarquía y validación están separadas
   - Facilita mantenimiento y testing independiente

3. **Inversión de Dependencias**:
   - Interfaces definidas en la capa de dominio
   - Implementaciones en la capa de infraestructura
   - Permite testing con mocks y cambio de implementaciones

### Interacciones

- **IRoleAuthorizationService** utiliza **IRoleHierarchyService** para evaluar autoridad
- **IRoleValidationService** puede consultar **IRoleHierarchyService** para validaciones
- Los servicios de aplicación orquestan estos servicios de dominio

### Beneficios

1. **Testabilidad**: Cada servicio puede ser probado independientemente
2. **Reutilización**: Lógica de dominio reutilizable en diferentes contextos
3. **Mantenibilidad**: Cambios en reglas de negocio localizados en servicios específicos
4. **Expresividad**: El código refleja claramente las reglas de negocio del dominio

## Implementación

Las implementaciones concretas de estas interfaces se encuentran en:
- `NET.Api.Infrastructure/Services/RoleService/`

Cada implementación debe:
- Seguir los principios de dominio establecidos
- Manejar casos edge y validaciones apropiadas
- Proporcionar logging y manejo de errores consistente
- Mantener performance óptimo para operaciones frecuentes