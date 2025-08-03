# NET.Api - Clean Architecture Project

## 🏗️ Arquitectura del Proyecto

Este proyecto implementa **Clean Architecture** (Arquitectura Limpia) siguiendo los principios de **Robert C. Martin (Uncle Bob)**. La arquitectura está diseñada para ser **mantenible**, **testeable** y **independiente** de frameworks y tecnologías específicas.

## 📁 Estructura del Proyecto

```
NET.Api/
├── src/
│   ├── NET.Api/                    # 🌐 Capa de Presentación (API)
│   ├── NET.Api.Application/         # 🎯 Capa de Aplicación (Casos de Uso)
│   ├── NET.Api.Domain/              # 🏛️ Capa de Dominio (Lógica de Negocio)
│   └── NET.Api.Infrastructure/      # 🔧 Capa de Infraestructura (Implementaciones)
└── tests/
    ├── NET.Api.UnitTests/
    ├── NET.Api.IntegrationTests/
    └── NET.Api.ArchitectureTests/
```

## 🎯 Principios de Clean Architecture

### 1. **Regla de Dependencia**
```
Presentation → Application → Domain ← Infrastructure
```

- **Domain**: No depende de NADA (núcleo independiente)
- **Application**: Solo depende de Domain
- **Infrastructure**: Depende de Domain y Application
- **Presentation**: Depende de Application

### 2. **Inversión de Dependencias**
- Las capas externas implementan interfaces definidas en capas internas
- El dominio define contratos, la infraestructura los implementa

### 3. **Separación de Responsabilidades**
- Cada capa tiene una responsabilidad específica y bien definida

## 🏛️ Capas de la Arquitectura

### 🌐 Presentation Layer (NET.Api)

**Propósito:** Punto de entrada de la aplicación, maneja peticiones HTTP.

**Responsabilidades:**
- ✅ Controladores de API
- ✅ Modelos de Request/Response
- ✅ Configuración de la aplicación
- ✅ Middleware de presentación
- ✅ Filtros y validaciones de entrada
- ✅ Manejo de errores HTTP

**NO debe contener:**
- ❌ Lógica de negocio
- ❌ Acceso a datos
- ❌ Servicios de dominio

**Ejemplo:**
```csharp
[ApiController]
public class UsersController : BaseController
{
    private readonly IUserApplicationService _userService;
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var dto = request.ToDto();
        var user = await _userService.CreateUserAsync(dto);
        return HandleResult(user);
    }
}
```

### 🎯 Application Layer (NET.Api.Application)

**Propósito:** Orquesta casos de uso y coordina operaciones entre dominio e infraestructura.

**Responsabilidades:**
- ✅ Servicios de aplicación (casos de uso)
- ✅ Commands y Queries (CQRS)
- ✅ DTOs (Data Transfer Objects)
- ✅ Mappers (conversión DTOs ↔ Entidades)
- ✅ Validadores de entrada
- ✅ Interfaces de servicios de aplicación

**NO debe contener:**
- ❌ Lógica de negocio (va en Domain)
- ❌ Implementaciones de repositorios
- ❌ Detalles de infraestructura

**Ejemplo:**
```csharp
public class UserApplicationService : IUserApplicationService
{
    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        // 1. Validar entrada
        await _validator.ValidateAsync(dto);
        
        // 2. Convertir a entidad
        var user = dto.ToEntity();
        
        // 3. Aplicar reglas de dominio
        await _userDomainService.ValidateUserCreationAsync(user);
        
        // 4. Persistir
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        
        // 5. Retornar DTO
        return user.ToDto();
    }
}
```

### 🏛️ Domain Layer (NET.Api.Domain)

**Propósito:** Núcleo de la aplicación, contiene la lógica de negocio pura.

**Responsabilidades:**
- ✅ Entidades de dominio
- ✅ Value Objects
- ✅ Servicios de dominio
- ✅ Interfaces de repositorios
- ✅ Eventos de dominio
- ✅ Excepciones de dominio
- ✅ Especificaciones

**NO debe contener:**
- ❌ Implementaciones de repositorios
- ❌ DTOs
- ❌ Dependencias externas
- ❌ Frameworks específicos

**Ejemplo:**
```csharp
// Entidad de dominio
public class User : BaseEntity
{
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    
    public void ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new DomainException("Email cannot be empty");
            
        Email = newEmail;
        AddDomainEvent(new UserEmailChangedEvent(Id, newEmail));
    }
}

// Servicio de dominio
public class UserDomainService : IDomainService
{
    public async Task<bool> CanUserBePromotedAsync(User user)
    {
        // Reglas de negocio puras
        return user.CreatedAt < DateTime.UtcNow.AddMonths(-6);
    }
}
```

### 🔧 Infrastructure Layer (NET.Api.Infrastructure)

**Propósito:** Implementa todos los detalles técnicos y contratos definidos en capas superiores.

**Responsabilidades:**
- ✅ Implementaciones de repositorios
- ✅ Configuración de Entity Framework
- ✅ Servicios externos (email, SMS, APIs)
- ✅ UnitOfWork implementation
- ✅ Configuraciones de base de datos
- ✅ Middleware de infraestructura

**NO debe contener:**
- ❌ Lógica de negocio
- ❌ Casos de uso
- ❌ Controladores

**Ejemplo:**
```csharp
// Implementación de repositorio
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }
}

// Implementación de UnitOfWork
public class UnitOfWork : IUnitOfWork
{
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
```

## 🔍 Diferencias Clave: Servicios de Dominio vs Aplicación

### 🏛️ Servicios de Dominio

**Ubicación:** `Domain/Services/`

**Características:**
- Contienen **lógica de negocio pura**
- Operan con **entidades de dominio**
- **NO conocen** DTOs, HTTP, o detalles técnicos
- Implementan reglas de negocio complejas
- Son **stateless** (sin estado)

**Cuándo usar:**
- Lógica que involucra múltiples entidades
- Reglas de negocio que no pertenecen a una entidad específica
- Cálculos complejos de dominio

**Ejemplo:**
```csharp
public class UserDomainService : IDomainService
{
    // LÓGICA DE NEGOCIO: ¿Puede un usuario ser promovido?
    public async Task<bool> CanUserBePromotedAsync(User user)
    {
        // Regla de negocio: mínimo 6 meses
        if (user.CreatedAt > DateTime.UtcNow.AddMonths(-6))
            return false;
            
        // Regla de negocio: máximo 1000 usuarios activos
        var activeCount = await _userRepository.CountActiveUsersAsync();
        return activeCount < 1000;
    }
}
```

### 🎯 Servicios de Aplicación

**Ubicación:** `Application/Services/`

**Características:**
- **Orquestan casos de uso**
- Coordinan entre dominio e infraestructura
- Manejan **transacciones**
- Convierten **DTOs ↔ Entidades**
- **NO contienen** lógica de negocio

**Cuándo usar:**
- Implementar casos de uso específicos
- Coordinar múltiples operaciones
- Manejar flujos de trabajo

**Ejemplo:**
```csharp
public class UserApplicationService : IUserApplicationService
{
    // CASO DE USO: Promover usuario
    public async Task<UserDto> PromoteUserAsync(Guid userId)
    {
        // 1. Obtener entidad
        var user = await _userRepository.GetByIdAsync(userId);
        
        // 2. Usar servicio de dominio para validar
        var canBePromoted = await _userDomainService.CanUserBePromotedAsync(user);
        if (!canBePromoted)
            throw new BusinessRuleException("User cannot be promoted");
            
        // 3. Ejecutar operación de dominio
        user.Promote();
        
        // 4. Persistir
        await _unitOfWork.SaveChangesAsync();
        
        // 5. Retornar DTO
        return user.ToDto();
    }
}
```

## 🔄 ¿Por qué las Interfaces están en Diferentes Capas?

### 📍 Interfaces en Domain

**Ejemplos:** `IRepository<T>`, `IUserRepository`, `IEmailService`

**Razón:** Representan **conceptos de negocio** y **capacidades** que el dominio necesita.

```csharp
// Domain/Interfaces/IUserRepository.cs
// ✅ En Domain porque define QUÉ necesita el dominio
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email); // Concepto de negocio
    Task<bool> ExistsWithEmailAsync(string email); // Regla de negocio
}
```

### 📍 Interfaces en Application

**Ejemplos:** `IAuthService`, `IUserApplicationService`

**Razón:** Definen **contratos de casos de uso** específicos de la aplicación.

```csharp
// Application/Abstractions/Services/IAuthService.cs
// ✅ En Application porque define casos de uso
public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginDto loginDto); // Caso de uso
    Task<AuthResultDto> RegisterAsync(RegisterDto registerDto); // Caso de uso
}
```

### 📍 Interfaces en Infrastructure

**Ejemplos:** `IUnitOfWork`

**Razón:** Son **patrones técnicos** específicos de infraestructura.

```csharp
// Infrastructure/Persistence/IUnitOfWork.cs
// ✅ En Infrastructure porque es patrón técnico
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(); // Detalle técnico de BD
    Task BeginTransactionAsync(); // Específico de BD relacionales
}
```

## 📊 Tabla Comparativa Completa

| Aspecto | Domain | Application | Infrastructure | Presentation |
|---------|--------|-------------|----------------|--------------|
| **Propósito** | Lógica de negocio | Casos de uso | Implementaciones | API HTTP |
| **Dependencias** | Ninguna | Solo Domain | Domain + Application | Application |
| **Contiene** | Entidades, reglas | DTOs, orquestación | Repositorios, BD | Controladores |
| **Interfaces** | Conceptos de negocio | Casos de uso | Patrones técnicos | - |
| **Ejemplo Interface** | `IUserRepository` | `IAuthService` | `IUnitOfWork` | - |
| **Ejemplo Servicio** | `UserDomainService` | `UserApplicationService` | `UserRepository` | `UsersController` |
| **Conoce sobre** | Solo negocio | DTOs + Entidades | BD, APIs externas | HTTP, JSON |
| **Testeable** | ✅ Fácil | ✅ Fácil | ⚠️ Requiere mocks | ⚠️ Requiere setup |

## 🚀 Beneficios de esta Arquitectura

### ✅ Ventajas

1. **Mantenibilidad**: Código organizado y fácil de modificar
2. **Testabilidad**: Cada capa se puede testear independientemente
3. **Flexibilidad**: Fácil cambiar implementaciones sin afectar lógica de negocio
4. **Escalabilidad**: Estructura clara para equipos grandes
5. **Independencia**: Dominio independiente de frameworks y tecnologías

### 🎯 Casos de Uso Ideales

- ✅ Aplicaciones empresariales complejas
- ✅ Sistemas con lógica de negocio rica
- ✅ Proyectos de larga duración
- ✅ Equipos grandes
- ✅ Requisitos cambiantes

### ⚠️ Consideraciones

- Puede ser **overkill** para aplicaciones simples
- Requiere **disciplina** del equipo
- **Curva de aprendizaje** inicial
- Más **código boilerplate**

## 🛠️ Tecnologías Utilizadas

- **.NET 8**: Framework principal
- **Entity Framework Core**: ORM
- **SQL Server**: Base de datos
- **JWT**: Autenticación
- **Swagger**: Documentación de API
- **FluentValidation**: Validaciones
- **MediatR**: Patrón Mediator (opcional)
- **AutoMapper**: Mapeo de objetos (opcional)

## 🚀 Cómo Empezar

1. **Clonar el repositorio**
```bash
git clone <repository-url>
cd NET.Api
```

2. **Restaurar dependencias**
```bash
dotnet restore
```

3. **Configurar base de datos**
```bash
dotnet ef database update --project src/NET.Api.Infrastructure --startup-project src/NET.Api
```

4. **Ejecutar la aplicación**
```bash
dotnet run --project src/NET.Api
```

5. **Acceder a Swagger**
```
https://localhost:7000/swagger
```

## 📚 Recursos Adicionales

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [.NET Application Architecture Guides](https://docs.microsoft.com/en-us/dotnet/architecture/)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)

## 📝 Reglas de Oro

1. **Domain es independiente**: No depende de NADA
2. **Application orquesta**: No contiene lógica de negocio
3. **Infrastructure implementa**: Solo detalles técnicos
4. **Presentation formatea**: Solo maneja HTTP
5. **Interfaces en la capa correcta**: Según su propósito
6. **Seguir la regla de dependencia**: Siempre hacia adentro

---

**¡Bienvenido a Clean Architecture!** 🎉

Esta estructura te ayudará a construir aplicaciones **robustas**, **mantenibles** y **escalables**.
