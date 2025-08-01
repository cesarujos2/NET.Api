# NET.Api - Hexagonal Architecture

Este proyecto implementa una API REST usando .NET 8 con arquitectura hexagonal (puertos y adaptadores), siguiendo principios de código limpio y patrones de diseño escalables.

## Estructura del Proyecto

```
src/
├── NET.Api.Domain/           # Capa de Dominio (Core Business Logic)
│   ├── Entities/            # Entidades del dominio
│   ├── ValueObjects/        # Objetos de valor
│   ├── Interfaces/          # Contratos del dominio
│   ├── Services/            # Servicios del dominio
│   ├── Events/              # Eventos del dominio
│   └── Exceptions/          # Excepciones del dominio
│
├── NET.Api.Application/      # Capa de Aplicación (Use Cases)
│   ├── Commands/            # Comandos (CQRS)
│   ├── Queries/             # Consultas (CQRS)
│   ├── DTOs/                # Data Transfer Objects
│   ├── Interfaces/          # Contratos de aplicación
│   ├── Behaviors/           # Comportamientos de MediatR
│   ├── Mappings/            # Perfiles de AutoMapper
│   └── Exceptions/          # Excepciones de aplicación
│
├── NET.Api.Infrastructure/   # Capa de Infraestructura (External Concerns)
│   ├── Persistence/         # Configuración de base de datos
│   ├── Repositories/        # Implementación de repositorios
│   ├── ExternalServices/    # Servicios externos
│   └── Configuration/       # Configuración de DI
│
├── NET.Api.Shared/          # Utilidades Compartidas
│   ├── Constants/           # Constantes
│   ├── Extensions/          # Métodos de extensión
│   ├── Utilities/           # Utilidades generales
│   └── Models/              # Modelos compartidos
│
└── NET.Api.WebApi/          # Capa de Presentación (API Entry Point)
    ├── Controllers/         # Controladores de API
    ├── Middleware/          # Middleware personalizado
    └── Configuration/       # Configuración de servicios
```

## Principios de Arquitectura

### Hexagonal Architecture (Ports & Adapters)
- **Domain**: Núcleo de la aplicación, independiente de frameworks externos
- **Application**: Casos de uso y lógica de aplicación
- **Infrastructure**: Implementaciones concretas de puertos (base de datos, servicios externos)
- **WebApi**: Adaptador de entrada (HTTP/REST)

### Patrones Implementados
- **CQRS** (Command Query Responsibility Segregation)
- **Repository Pattern**
- **Unit of Work Pattern**
- **Domain Events**
- **Value Objects**
- **Result Pattern**

### Tecnologías Utilizadas
- **.NET 8**
- **MediatR** - Para CQRS y manejo de comandos/consultas
- **FluentValidation** - Para validación de entrada
- **AutoMapper** - Para mapeo de objetos
- **Entity Framework Core** - Para acceso a datos
- **Swagger/OpenAPI** - Para documentación de API

## Cómo Ejecutar

1. Clonar el repositorio
2. Restaurar paquetes NuGet:
   ```bash
   dotnet restore
   ```
3. Compilar la solución:
   ```bash
   dotnet build
   ```
4. Ejecutar la API:
   ```bash
   dotnet run --project src/NET.Api.WebApi
   ```

## Próximos Pasos

La estructura base está lista para comenzar a implementar:
- Entidades del dominio
- Casos de uso específicos
- Configuración de base de datos
- Autenticación y autorización
- Logging y monitoreo

## Dependencias entre Capas

```
WebApi → Application → Domain
WebApi → Infrastructure → Application → Domain
WebApi → Shared
Application → Shared
Infrastructure → Shared
```

Esta estructura garantiza que el dominio permanezca independiente y que las dependencias fluyan hacia el centro (Domain), siguiendo los principios de la arquitectura hexagonal.
