# NET.Api - Clean Architecture API with JWT Authentication

Una API RESTful construida con .NET 8 siguiendo los principios de Clean Architecture, implementando autenticación JWT con tokens de acceso y refresco.

## 🏗️ Arquitectura

Este proyecto sigue los principios de **Clean Architecture** con separación clara de responsabilidades:

### Capas del Proyecto

- **Domain** (`NET.Api.Domain`): Entidades, interfaces de dominio y lógica de negocio
- **Application** (`NET.Api.Application`): Casos de uso, DTOs, comandos, queries y validaciones
- **Infrastructure** (`NET.Api.Infrastructure`): Implementación de persistencia, servicios externos y configuraciones
- **WebApi** (`NET.Api.WebApi`): Controladores, middleware y configuración de la API
- **Shared** (`NET.Api.Shared`): Utilidades y constantes compartidas

## 🚀 Características

### Autenticación y Seguridad
- ✅ **JWT Authentication** con tokens de acceso y refresco
- ✅ **Refresh Token** para renovación automática de sesiones
- ✅ **Identity Framework** para gestión de usuarios
- ✅ **Configuración de expiración** desde `appsettings.json`
- ✅ **Validación de tokens** y revocación de refresh tokens

### Arquitectura y Patrones
- ✅ **Clean Architecture** con separación de capas
- ✅ **CQRS** con MediatR para comandos y queries
- ✅ **Repository Pattern** para acceso a datos
- ✅ **Dependency Injection** nativo de .NET
- ✅ **Principios SOLID** aplicados

### Base de Datos
- ✅ **Entity Framework Core** con Code First
- ✅ **SQLite** para desarrollo (configurable)
- ✅ **Migraciones automáticas** y seeder de datos
- ✅ **Convenciones de nomenclatura** personalizadas

### Validación y Documentación
- ✅ **FluentValidation** para validación de DTOs
- ✅ **AutoMapper** para mapeo de objetos
- ✅ **Swagger/OpenAPI** para documentación
- ✅ **Manejo de errores** centralizado

## 🛠️ Tecnologías

- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core 8.0**
- **SQLite** (configurable a SQL Server/MySQL)
- **JWT Bearer Authentication**
- **MediatR** (CQRS)
- **FluentValidation**
- **AutoMapper**
- **Swagger/OpenAPI**

## 📋 Requisitos

- .NET 8 SDK
- Visual Studio 2022 / VS Code / Rider
- SQLite (incluido)

## 🚀 Instalación y Configuración

### 1. Clonar el repositorio
```bash
git clone <repository-url>
cd NET.Api
```

### 2. Restaurar dependencias
```bash
dotnet restore
```

### 3. Configurar la base de datos
```bash
# Crear migraciones (si es necesario)
dotnet ef migrations add InitialCreate --project src\NET.Api.Infrastructure --startup-project src\NET.Api.WebApi

# Aplicar migraciones
dotnet ef database update --project src\NET.Api.Infrastructure --startup-project src\NET.Api.WebApi
```

### 4. Ejecutar la aplicación
```bash
dotnet run --project src\NET.Api.WebApi
```

La API estará disponible en: `https://localhost:5001` o `http://localhost:5000`

## ⚙️ Configuración

### JWT Settings (`appsettings.json`)
```json
{
  "JwtSettings": {
    "SecretKey": "MySecretKeyForJWTTokenGeneration123456789",
    "Issuer": "NET.Api",
    "Audience": "NET.Api.Users",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Cadena de Conexión
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=NET.Api.db"
  }
}
```

## 📚 Endpoints de la API

### Autenticación

#### Registro de Usuario
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "usuario@ejemplo.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "firstName": "Juan",
  "lastName": "Pérez",
  "identityDocument": "12345678",
  "phoneNumber": "+1234567890"
}
```

#### Inicio de Sesión
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@ejemplo.com",
  "password": "Password123!"
}
```

#### Renovar Token
```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token"
}
```

### Respuesta de Autenticación
```json
{
  "success": true,
  "message": "Inicio de sesión exitoso.",
  "data": {
    "id": "user-id",
    "email": "usuario@ejemplo.com",
    "firstName": "Juan",
    "lastName": "Pérez",
    "fullName": "Juan Pérez",
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token",
    "expiresAt": "2024-01-01T12:15:00Z",
    "roles": ["User"]
  }
}
```

## 🗄️ Estructura de la Base de Datos

### Tablas Principales

- **USERS**: Información de usuarios (Identity)
- **ROLES**: Roles del sistema (Identity)
- **USER_ROLES**: Relación usuarios-roles (Identity)
- **REFRESH_TOKENS**: Tokens de refresco

### Usuario por Defecto

El sistema crea automáticamente un usuario administrador:
- **Email**: `owner@netapi.com`
- **Password**: `Owner123!`
- **Rol**: Owner

## 🔧 Desarrollo

### Agregar Nueva Migración
```bash
dotnet ef migrations add <NombreMigracion> --project src\NET.Api.Infrastructure --startup-project src\NET.Api.WebApi
```

### Revertir Migración
```bash
dotnet ef migrations remove --project src\NET.Api.Infrastructure --startup-project src\NET.Api.WebApi
```

### Generar Script SQL
```bash
dotnet ef migrations script --project src\NET.Api.Infrastructure --startup-project src\NET.Api.WebApi
```

## 🏗️ Principios SOLID Aplicados

### Single Responsibility Principle (SRP)
- Cada servicio tiene una responsabilidad específica
- `AuthService`: Gestión de autenticación
- `JwtTokenService`: Generación y validación de tokens

### Open/Closed Principle (OCP)
- Interfaces para extensibilidad sin modificación
- `IAuthService`, `IJwtTokenService`

### Liskov Substitution Principle (LSP)
- Implementaciones intercambiables a través de interfaces

### Interface Segregation Principle (ISP)
- Interfaces específicas y cohesivas

### Dependency Inversion Principle (DIP)
- Dependencias inyectadas a través de interfaces
- Configuración en `DependencyInjection.cs`

## 🔒 Seguridad

### Tokens JWT
- **Access Token**: Vida corta (15 minutos por defecto)
- **Refresh Token**: Vida larga (7 días por defecto)
- **Revocación**: Los refresh tokens pueden ser revocados
- **Validación**: Verificación de firma, emisor y audiencia

### Configuración de Seguridad
- Contraseñas con requisitos mínimos
- Bloqueo de cuenta tras intentos fallidos
- Tokens únicos y seguros

## 📝 Notas de Desarrollo

### Cambios Recientes
1. **Refactorización de AuthService**: Separación de responsabilidades siguiendo SOLID
2. **Implementación de Refresh Tokens**: Sistema completo de renovación de tokens
3. **Configuración desde appsettings**: Expiración de tokens configurable
4. **Nueva tabla RefreshToken**: Gestión persistente de tokens de refresco
5. **Mejoras en la arquitectura**: Aplicación de principios SOLID

### Próximas Mejoras
- [ ] Implementar logout con revocación de tokens
- [ ] Agregar roles y permisos granulares
- [ ] Implementar rate limiting
- [ ] Agregar logging estructurado
- [ ] Implementar health checks

## 🤝 Contribución

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para más detalles.

## 📞 Contacto

Para preguntas o sugerencias, por favor abre un issue en el repositorio.
