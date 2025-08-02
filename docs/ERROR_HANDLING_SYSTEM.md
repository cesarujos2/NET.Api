# Sistema de Manejo de Errores - NET.Api

## Descripción General

Este documento describe el sistema completo de manejo de errores implementado en NET.Api, que incluye middleware personalizado, excepciones tipadas, logging avanzado, seguridad y rate limiting.

## Arquitectura del Sistema

### 1. Excepciones Personalizadas

El sistema incluye excepciones específicas para diferentes tipos de errores:

- **`ValidationException`**: Errores de validación de FluentValidation
- **`NotFoundException`**: Recursos no encontrados
- **`BusinessRuleException`**: Violaciones de reglas de negocio
- **`ConflictException`**: Conflictos de recursos (duplicados)
- **`ForbiddenException`**: Acceso prohibido (usuario autenticado sin permisos)
- **`ExternalServiceException`**: Errores de servicios externos
- **`ApplicationException`**: Clase base para excepciones de aplicación

### 2. Middleware de Manejo de Errores

#### ExceptionHandlingMiddleware
- Captura todas las excepciones no manejadas
- Determina automáticamente el código de estado HTTP
- Crea respuestas estructuradas con `ErrorResponse`
- Registra errores con contexto completo
- Incluye stack trace solo en desarrollo

#### SecurityMiddleware
- Protege contra ataques comunes (XSS, SQL Injection, Path Traversal)
- Valida headers de seguridad
- Controla tamaño de requests
- Bloquea User-Agents sospechosos
- Añade headers de seguridad a las respuestas

#### RateLimitingMiddleware
- Implementa rate limiting por IP/Usuario
- Configurable por endpoint
- Headers informativos de límites
- Respuestas estructuradas para límites excedidos

### 3. Behaviors de MediatR

#### LoggingBehavior
- Registra todas las operaciones de MediatR
- Mide tiempo de ejecución
- Detecta operaciones lentas
- Logging de requests/responses en debug

#### ValidationBehavior
- Valida automáticamente con FluentValidation
- Lanza `ValidationException` con errores estructurados
- Logging de errores de validación

### 4. Controlador Base Mejorado

#### BaseApiController
- Métodos helper para manejo de resultados
- Acceso a información del usuario autenticado
- Manejo automático de excepciones con `ExecuteSafelyAsync`
- Logging integrado
- Validación de autenticación

### 5. Sistema de Logging Avanzado

#### Configuración con Serilog
- Logging estructurado en JSON
- Separación de logs por nivel (app, errores)
- Rotación automática de archivos
- Enriquecimiento con contexto (máquina, thread, usuario)
- Configuración diferente para desarrollo/producción

## Configuración

### appsettings.ErrorHandling.json

```json
{
  "RateLimit": {
    "MaxRequests": 100,
    "WindowSizeInMinutes": 1
  },
  "Security": {
    "ValidateHeaders": true,
    "ValidateRequestSize": true,
    "MaxRequestSizeBytes": 10485760,
    "ValidateUrls": true,
    "ValidateUserAgent": true,
    "ValidateRequestBody": true,
    "AddSecurityHeaders": true
  },
  "ErrorHandling": {
    "IncludeStackTrace": false,
    "EnableDetailedErrors": false,
    "SlowRequestThresholdMs": 5000
  }
}
```

## Uso en Controladores

### Ejemplo Básico

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : BaseApiController
{
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var query = new GetUserQuery { Id = id };
            var user = await Mediator.Send(query);
            return user;
        }, "Usuario obtenido exitosamente");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser(CreateUserCommand command)
    {
        // Verificar autenticación
        var authCheck = RequireAuthentication<UserDto>();
        if (authCheck != null) return authCheck;

        return await ExecuteSafelyAsync(async () =>
        {
            var result = await Mediator.Send(command);
            return result;
        }, "Usuario creado exitosamente");
    }
}
```

### Manejo Manual de Errores

```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetUsers()
{
    try
    {
        var users = await Mediator.Send(new GetUsersQuery());
        return HandleResult(users, "Usuarios obtenidos exitosamente");
    }
    catch (ValidationException ex)
    {
        return HandleValidationError<List<UserDto>>(ex.Errors);
    }
    catch (Exception)
    {
        return HandleError<List<UserDto>>(
            "INTERNAL_ERROR",
            "Error interno del servidor",
            500);
    }
}
```

## Uso en Handlers

### Lanzar Excepciones Específicas

```csharp
public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id);
        
        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.Id.ToString());
        }

        if (!user.IsActive)
        {
            throw new BusinessRuleException(
                "UserMustBeActive",
                "El usuario debe estar activo para ser consultado");
        }

        return _mapper.Map<UserDto>(user);
    }
}
```

### Validación con FluentValidation

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El formato del email es inválido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres");
    }
}
```

## Respuestas de Error Estructuradas

### Formato de Respuesta

```json
{
  "success": false,
  "data": null,
  "errorResponse": {
    "errorCode": "VALIDATION_ERROR",
    "message": "Se encontraron errores de validación",
    "details": "Los datos proporcionados no son válidos",
    "timestamp": "2024-01-15T10:30:00Z",
    "traceId": "0HN7GKQJM5K4H:00000001",
    "validationErrors": {
      "Email": ["El email es requerido"],
      "Password": ["La contraseña debe tener al menos 8 caracteres"]
    },
    "context": {
      "endpoint": "POST /api/v1/users",
      "userId": "user123"
    },
    "isRetryable": false,
    "suggestions": [
      "Verifica que todos los campos requeridos estén completos",
      "Asegúrate de que el formato de los datos sea correcto"
    ]
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## Logging y Monitoreo

### Estructura de Logs

```json
{
  "@t": "2024-01-15T10:30:00.123Z",
  "@l": "Error",
  "@m": "Excepción no manejada en ExceptionHandlingMiddleware",
  "@x": "System.ArgumentException: Invalid user ID...",
  "SourceContext": "NET.Api.WebApi.Middleware.ExceptionHandlingMiddleware",
  "RequestPath": "/api/v1/users/invalid",
  "RequestMethod": "GET",
  "UserId": "user123",
  "TraceId": "0HN7GKQJM5K4H:00000001",
  "ExceptionType": "ArgumentException",
  "Environment": "Production",
  "Application": "NET.Api",
  "MachineName": "WEB-SERVER-01",
  "ThreadId": 15
}
```

### Métricas de Performance

- Tiempo de ejecución de requests
- Detección de operaciones lentas (>5s por defecto)
- Conteo de errores por tipo
- Rate limiting por endpoint

## Seguridad

### Headers de Seguridad Añadidos

- `X-Frame-Options: DENY`
- `X-Content-Type-Options: nosniff`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Content-Security-Policy: default-src 'self'...`
- `Strict-Transport-Security: max-age=31536000` (solo HTTPS)

### Protecciones Implementadas

- **XSS**: Detección de scripts maliciosos
- **SQL Injection**: Patrones de inyección SQL
- **Path Traversal**: Intentos de acceso a archivos
- **User-Agent Filtering**: Bloqueo de herramientas de hacking
- **Request Size Limiting**: Prevención de ataques DoS

## Rate Limiting

### Configuración por Defecto

- 100 requests por minuto por IP/Usuario
- Ventana deslizante de 1 minuto
- Headers informativos en respuestas
- Exclusión de endpoints de salud

### Headers de Rate Limit

- `X-RateLimit-Limit`: Límite máximo
- `X-RateLimit-Remaining`: Requests restantes
- `X-RateLimit-Reset`: Timestamp de reset
- `Retry-After`: Segundos hasta poder reintentar

## Mejores Prácticas

1. **Usar ExecuteSafelyAsync** en controladores para manejo automático
2. **Lanzar excepciones específicas** en lugar de genéricas
3. **Incluir contexto útil** en mensajes de error
4. **Validar con FluentValidation** en lugar de validación manual
5. **No exponer información sensible** en mensajes de error
6. **Usar logging estructurado** para facilitar búsquedas
7. **Configurar rate limits apropiados** según el tipo de endpoint

## Extensibilidad

El sistema está diseñado para ser fácilmente extensible:

- Añadir nuevos tipos de excepciones heredando de `ApplicationException`
- Crear middlewares adicionales siguiendo el patrón establecido
- Configurar diferentes políticas de rate limiting por endpoint
- Añadir nuevas validaciones de seguridad en `SecurityMiddleware`
- Extender el logging con nuevos enrichers de Serilog

## Troubleshooting

### Problemas Comunes

1. **Logs no aparecen**: Verificar configuración de Serilog
2. **Rate limiting muy restrictivo**: Ajustar configuración en appsettings
3. **Errores de seguridad falsos positivos**: Revisar patrones en SecurityMiddleware
4. **Performance degradada**: Verificar configuración de logging en producción

### Debugging

- Usar TraceId para seguir requests específicos
- Revisar logs estructurados en archivos JSON
- Verificar headers de respuesta para información de rate limiting
- Usar logging de desarrollo para requests/responses detallados