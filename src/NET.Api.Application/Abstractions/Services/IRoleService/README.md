# Interfaces de Servicios de Roles - Capa de Aplicación

Esta carpeta contiene las interfaces que definen los contratos para los servicios de gestión de roles en la capa de aplicación.

## Archivos



### IRoleManagementService.cs
**Propósito**: Interfaz principal para la gestión de roles a nivel de aplicación.
- Define operaciones CRUD para roles: crear, actualizar, eliminar
- Incluye métodos para asignación y remoción de roles a usuarios
- Maneja la lógica de aplicación y coordinación entre servicios de dominio
- **Arquitectura**: Capa de Aplicación - Orquesta servicios de dominio

### IRoleQueryService.cs
**Propósito**: Interfaz especializada para consultas de roles.
- Define métodos de solo lectura para obtener información de roles
- Incluye consultas como `GetAssignableRolesAsync`, `GetUserRolesAsync`, `GetActiveRolesAsync`
- Implementa el patrón CQRS (Command Query Responsibility Segregation)
- **Arquitectura**: Capa de Aplicación - Consultas especializadas

## Arquitectura

### Servicios de Roles
- `IRoleManagementApplicationService`: Gestión completa de roles (CRUD y asignaciones)
- `IRoleQueryService`: Consultas especializadas de roles (solo lectura)

## Principios de Diseño

1. **Separación de Responsabilidades**: Cada interfaz tiene una responsabilidad específica
2. **CQRS**: Separación entre comandos (gestión) y consultas
3. **Clean Architecture**: Interfaces en la capa de aplicación, implementaciones en infraestructura
4. **Arquitectura Limpia**: Implementación completa de Clean Architecture y principios SOLID