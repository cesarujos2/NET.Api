# Ejemplos Prácticos del Sistema de Gestión de Roles

## Casos de Uso Comunes

### 1. Controlador con Múltiples Niveles de Acceso

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserManagementController : ControllerBase
{
    private readonly IRoleManagementService _roleService;
    
    public UserManagementController(IRoleManagementService roleService)
    {
        _roleService = roleService;
    }
    
    // Solo Owner puede crear administradores
    [HttpPost("create-admin")]
    [Authorize(Policy = Policies.RequireOwnerRole)]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateUserRequest request)
    {
        // Lógica para crear administrador
        return Ok();
    }
    
    // Admin o superior puede gestionar usuarios normales
    [HttpPost("create-user")]
    [Authorize(Policy = Policies.RequireAdminOrAbove)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // Lógica para crear usuario
        return Ok();
    }
    
    // Moderator o superior puede ver lista de usuarios
    [HttpGet("list")]
    [Authorize(Policy = Policies.RequireModeratorOrAbove)]
    public async Task<IActionResult> GetUsers()
    {
        // Lógica para obtener usuarios
        return Ok();
    }
    
    // Support o superior puede ver perfil de usuario
    [HttpGet("{id}")]
    [Authorize(Policy = Policies.CanAccessSupport)]
    public async Task<IActionResult> GetUser(string id)
    {
        // Lógica para obtener usuario específico
        return Ok();
    }
}
```

### 2. Validación Programática de Roles

```csharp
public class DocumentService
{
    private readonly IRoleManagementService _roleService;
    
    public DocumentService(IRoleManagementService roleService)
    {
        _roleService = roleService;
    }
    
    public async Task<bool> CanUserEditDocument(ClaimsPrincipal user, Document document)
    {
        var userRoles = user.FindAll("role").Select(c => c.Value);
        
        // El propietario del documento siempre puede editarlo
        if (document.CreatedBy == user.FindFirst("nameid")?.Value)
            return true;
            
        // Moderator o superior puede editar cualquier documento
        if (_roleService.HasSufficientAuthority(userRoles, RoleConstants.Names.Moderator))
            return true;
            
        // Admin puede editar documentos críticos
        if (document.IsCritical && _roleService.HasSufficientAuthority(userRoles, RoleConstants.Names.Admin))
            return true;
            
        return false;
    }
    
    public async Task<IEnumerable<Document>> GetDocumentsForUser(ClaimsPrincipal user)
    {
        var userRoles = user.FindAll("role").Select(c => c.Value);
        var userId = user.FindFirst("nameid")?.Value;
        
        // Admin o superior puede ver todos los documentos
        if (_roleService.HasSufficientAuthority(userRoles, RoleConstants.Names.Admin))
        {
            return await GetAllDocuments();
        }
        
        // Moderator puede ver documentos públicos y propios
        if (_roleService.HasSufficientAuthority(userRoles, RoleConstants.Names.Moderator))
        {
            return await GetPublicAndOwnDocuments(userId);
        }
        
        // Usuario normal solo ve sus propios documentos
        return await GetUserDocuments(userId);
    }
}
```

### 3. Middleware de Autorización Personalizado

```csharp
public class RoleBasedAccessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRoleManagementService _roleService;
    private readonly ILogger<RoleBasedAccessMiddleware> _logger;
    
    public RoleBasedAccessMiddleware(
        RequestDelegate next, 
        IRoleManagementService roleService,
        ILogger<RoleBasedAccessMiddleware> logger)
    {
        _next = next;
        _roleService = roleService;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Verificar rutas administrativas
        if (context.Request.Path.StartsWithSegments("/admin"))
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = 401;
                return;
            }
            
            var userRoles = context.User.FindAll("role").Select(c => c.Value);
            
            if (!_roleService.HasSufficientAuthority(userRoles, RoleConstants.Names.Admin))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a ruta administrativa sin permisos", 
                    context.User.FindFirst("nameid")?.Value);
                context.Response.StatusCode = 403;
                return;
            }
        }
        
        await _next(context);
    }
}
```

### 4. Filtro de Acción Personalizado

```csharp
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _requiredRole;
    
    public RequireRoleAttribute(string requiredRole)
    {
        _requiredRole = requiredRole;
    }
    
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.User.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        var roleService = context.HttpContext.RequestServices
            .GetRequiredService<IRoleManagementService>();
            
        var userRoles = context.HttpContext.User.FindAll("role").Select(c => c.Value);
        
        if (!roleService.HasSufficientAuthority(userRoles, _requiredRole))
        {
            context.Result = new ForbidResult();
        }
    }
}

// Uso del filtro personalizado
[RequireRole(RoleConstants.Names.Admin)]
public class AdminController : ControllerBase
{
    // Todos los métodos requieren rol Admin o superior
}
```

### 5. Servicio de Auditoría de Roles

```csharp
public class RoleAuditService
{
    private readonly IRoleManagementService _roleService;
    private readonly ILogger<RoleAuditService> _logger;
    
    public RoleAuditService(
        IRoleManagementService roleService,
        ILogger<RoleAuditService> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }
    
    public async Task LogRoleAssignment(string adminUserId, string targetUserId, string newRole)
    {
        var adminRoles = await GetUserRoles(adminUserId);
        
        _logger.LogInformation(
            "Usuario {AdminId} con roles [{AdminRoles}] asignó rol {NewRole} a usuario {TargetId}",
            adminUserId,
            string.Join(", ", adminRoles),
            newRole,
            targetUserId);
            
        // Verificar si la asignación es válida
        if (!_roleService.CanAssignRole(adminRoles, newRole))
        {
            _logger.LogWarning(
                "Intento de asignación de rol inválida: {AdminId} intentó asignar {NewRole}",
                adminUserId,
                newRole);
        }
    }
    
    public async Task LogAccessAttempt(string userId, string resource, bool success)
    {
        var userRoles = await GetUserRoles(userId);
        
        _logger.LogInformation(
            "Usuario {UserId} con roles [{UserRoles}] {Result} acceso a {Resource}",
            userId,
            string.Join(", ", userRoles),
            success ? "obtuvo" : "fue denegado",
            resource);
    }
}
```

## Patrones de Implementación

### 1. Patrón Repository con Filtrado por Roles

```csharp
public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IRoleManagementService _roleService;
    
    public async Task<IEnumerable<Document>> GetDocumentsAsync(
        ClaimsPrincipal user, 
        DocumentFilter filter = null)
    {
        var query = _context.Documents.AsQueryable();
        var userRoles = user.FindAll("role").Select(c => c.Value);
        var userId = user.FindFirst("nameid")?.Value;
        
        // Filtrar por nivel de acceso
        if (!_roleService.HasSufficientAuthority(userRoles, RoleConstants.Names.Admin))
        {
            // No admin: solo documentos propios o públicos
            query = query.Where(d => d.CreatedBy == userId || d.IsPublic);
        }
        
        if (!_roleService.HasSufficientAuthority(userRoles, RoleConstants.Names.Moderator))
        {
            // No moderator: excluir documentos en revisión
            query = query.Where(d => d.Status != DocumentStatus.UnderReview);
        }
        
        // Aplicar filtros adicionales
        if (filter != null)
        {
            query = ApplyFilters(query, filter);
        }
        
        return await query.ToListAsync();
    }
}
```

### 2. Patrón Command con Validación de Roles

```csharp
public class CreateUserCommand : IRequest<CreateUserResponse>
{
    public string Email { get; set; }
    public string Role { get; set; }
    public ClaimsPrincipal RequestingUser { get; set; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IRoleManagementService _roleService;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public async Task<CreateUserResponse> Handle(
        CreateUserCommand request, 
        CancellationToken cancellationToken)
    {
        // Validar que el usuario puede asignar el rol solicitado
        var requestingUserRoles = request.RequestingUser.FindAll("role").Select(c => c.Value);
        
        if (!_roleService.CanAssignRole(requestingUserRoles, request.Role))
        {
            throw new UnauthorizedAccessException(
                $"No tiene permisos para asignar el rol {request.Role}");
        }
        
        // Crear usuario
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email
        };
        
        var result = await _userManager.CreateAsync(user);
        
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, request.Role);
        }
        
        return new CreateUserResponse { Success = result.Succeeded };
    }
}
```

### 3. Patrón Specification para Reglas de Negocio

```csharp
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
}

public class CanEditDocumentSpecification : ISpecification<(ClaimsPrincipal User, Document Document)>
{
    private readonly IRoleManagementService _roleService;
    
    public CanEditDocumentSpecification(IRoleManagementService roleService)
    {
        _roleService = roleService;
    }
    
    public bool IsSatisfiedBy((ClaimsPrincipal User, Document Document) input)
    {
        var (user, document) = input;
        var userRoles = user.FindAll("role").Select(c => c.Value);
        var userId = user.FindFirst("nameid")?.Value;
        
        // Regla 1: El propietario puede editar
        if (document.CreatedBy == userId)
            return true;
            
        // Regla 2: Admin puede editar cualquier documento
        if (_roleService.HasSufficientAuthority(userRoles, RoleConstants.Names.Admin))
            return true;
            
        // Regla 3: Moderator puede editar documentos no críticos
        if (_roleService.HasSufficientAuthority(userRoles, RoleConstants.Names.Moderator) 
            && !document.IsCritical)
            return true;
            
        return false;
    }
}
```

## Casos de Prueba

### 1. Pruebas Unitarias para RoleManagementService

```csharp
[TestClass]
public class RoleManagementServiceTests
{
    private IRoleManagementService _roleService;
    
    [TestInitialize]
    public void Setup()
    {
        var logger = Mock.Of<ILogger<RoleManagementService>>();
        _roleService = new RoleManagementService(logger);
    }
    
    [TestMethod]
    public void HasSufficientAuthority_AdminCanAccessModeratorResource_ReturnsTrue()
    {
        // Arrange
        var userRoles = new[] { RoleConstants.Names.Admin };
        var requiredRole = RoleConstants.Names.Moderator;
        
        // Act
        var result = _roleService.HasSufficientAuthority(userRoles, requiredRole);
        
        // Assert
        Assert.IsTrue(result);
    }
    
    [TestMethod]
    public void HasSufficientAuthority_UserCannotAccessAdminResource_ReturnsFalse()
    {
        // Arrange
        var userRoles = new[] { RoleConstants.Names.User };
        var requiredRole = RoleConstants.Names.Admin;
        
        // Act
        var result = _roleService.HasSufficientAuthority(userRoles, requiredRole);
        
        // Assert
        Assert.IsFalse(result);
    }
    
    [TestMethod]
    public void HasSufficientAuthority_MultipleRoles_UsesHighestRole()
    {
        // Arrange
        var userRoles = new[] { RoleConstants.Names.User, RoleConstants.Names.Admin };
        var requiredRole = RoleConstants.Names.Moderator;
        
        // Act
        var result = _roleService.HasSufficientAuthority(userRoles, requiredRole);
        
        // Assert
        Assert.IsTrue(result);
    }
}
```

### 2. Pruebas de Integración para Autorización

```csharp
[TestClass]
public class AuthorizationIntegrationTests : TestBase
{
    [TestMethod]
    public async Task EmailTemplatesController_UserRole_ShouldReturnForbidden()
    {
        // Arrange
        var client = CreateClientWithRole(RoleConstants.Names.User);
        
        // Act
        var response = await client.GetAsync("/api/EmailTemplates");
        
        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [TestMethod]
    public async Task EmailTemplatesController_AdminRole_ShouldReturnOk()
    {
        // Arrange
        var client = CreateClientWithRole(RoleConstants.Names.Admin);
        
        // Act
        var response = await client.GetAsync("/api/EmailTemplates");
        
        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
```

## Mejores Prácticas

### 1. Separación de Responsabilidades

```csharp
// ❌ Malo: Lógica de autorización mezclada con lógica de negocio
public class DocumentService
{
    public async Task<Document> GetDocument(int id, ClaimsPrincipal user)
    {
        var document = await _repository.GetByIdAsync(id);
        
        // Lógica de autorización mezclada
        var userRoles = user.FindAll("role").Select(c => c.Value);
        if (!_roleService.HasSufficientAuthority(userRoles, "Moderator"))
        {
            throw new UnauthorizedAccessException();
        }
        
        return document;
    }
}

// ✅ Bueno: Separación clara de responsabilidades
[Authorize(Policy = Policies.RequireModeratorOrAbove)]
public class DocumentController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<Document> GetDocument(int id)
    {
        // Solo lógica de negocio
        return await _documentService.GetDocumentAsync(id);
    }
}
```

### 2. Uso Consistente de Políticas

```csharp
// ❌ Malo: Verificación manual inconsistente
[HttpGet]
public async Task<IActionResult> GetUsers()
{
    var userRoles = User.FindAll("role").Select(c => c.Value);
    if (!userRoles.Contains("Admin") && !userRoles.Contains("Owner"))
    {
        return Forbid();
    }
    // ...
}

// ✅ Bueno: Uso de políticas definidas
[HttpGet]
[Authorize(Policy = Policies.RequireAdminOrAbove)]
public async Task<IActionResult> GetUsers()
{
    // ...
}
```

### 3. Logging y Auditoría

```csharp
public class AuditableController : ControllerBase
{
    protected void LogAccess(string action, object data = null)
    {
        var userId = User.FindFirst("nameid")?.Value;
        var userRoles = User.FindAll("role").Select(c => c.Value);
        
        _logger.LogInformation(
            "Usuario {UserId} con roles [{Roles}] ejecutó {Action}. Datos: {@Data}",
            userId,
            string.Join(", ", userRoles),
            action,
            data);
    }
}
```

Esta documentación proporciona ejemplos prácticos y patrones recomendados para implementar el sistema de gestión de roles de manera efectiva y mantenible.