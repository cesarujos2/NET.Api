# Modelos de Gestión de Roles - DTOs

Esta carpeta contiene los Data Transfer Objects (DTOs) utilizados para la gestión de roles en la capa de aplicación.

## Archivos

### AssignRoleRequestDto.cs
**Propósito**: DTO para solicitudes de asignación de roles a usuarios.
- Contiene información necesaria para asignar un rol específico a un usuario
- Incluye validaciones de entrada para garantizar datos correctos
- Utilizado en endpoints de asignación de roles

### CreateRoleRequestDto.cs
**Propósito**: DTO para solicitudes de creación de nuevos roles.
- Define la estructura de datos requerida para crear un rol
- Incluye propiedades como nombre, descripción y configuraciones del rol
- Contiene validaciones para nombres únicos y formatos válidos

### RemoveRoleRequestDto.cs
**Propósito**: DTO para solicitudes de remoción de roles de usuarios.
- Estructura de datos para remover roles específicos de usuarios
- Incluye validaciones para garantizar operaciones seguras
- Utilizado en endpoints de remoción de roles

### RoleDto.cs
**Propósito**: DTO principal para representar información de roles.
- Modelo de datos completo para roles en respuestas de API
- Incluye propiedades como ID, nombre, descripción, fecha de creación
- Utilizado en consultas y respuestas de información de roles

### UpdateRoleRequestDto.cs
**Propósito**: DTO para solicitudes de actualización de roles existentes.
- Define la estructura para modificar propiedades de roles
- Incluye validaciones para cambios permitidos
- Utilizado en endpoints de actualización de roles

### UserRoleDto.cs
**Propósito**: DTO para representar la relación entre usuarios y roles.
- Modelo que combina información de usuario y rol
- Utilizado en consultas que requieren datos de ambas entidades
- Incluye información como ID de usuario, nombre de usuario, rol asignado

## Características

### Validaciones
- Todos los DTOs incluyen atributos de validación apropiados
- Validaciones de formato, longitud y reglas de negocio
- Mensajes de error descriptivos para mejor experiencia de usuario

### Mapeo
- Los DTOs se mapean automáticamente a entidades de dominio usando AutoMapper
- Configuración de mapeo definida en `MappingProfile.cs`
- Transformaciones bidireccionales entre DTOs y entidades

### Uso en API
- Utilizados como modelos de entrada en controladores
- Serializados/deserializados automáticamente por ASP.NET Core
- Documentados automáticamente en Swagger/OpenAPI

## Principios de Diseño

1. **Separación de Capas**: Los DTOs mantienen la capa de aplicación independiente del dominio
2. **Validación Temprana**: Validaciones en el punto de entrada de la API
3. **Inmutabilidad**: DTOs diseñados como objetos inmutables cuando es posible
4. **Claridad**: Nombres descriptivos que reflejan su propósito específico