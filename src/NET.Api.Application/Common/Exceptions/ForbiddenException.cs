namespace NET.Api.Application.Common.Exceptions;

/// <summary>
/// Excepción para acceso prohibido (usuario autenticado pero sin permisos)
/// </summary>
public class ForbiddenException : ApplicationException
{
    public string? Resource { get; }
    public string? Action { get; }
    public string? UserId { get; }

    public ForbiddenException(string message) : base(message)
    {
        Resource = null;
        Action = null;
        UserId = null;
    }

    public ForbiddenException(string resource, string action, string? userId = null)
        : base($"No tienes permisos para realizar la acción '{action}' en el recurso '{resource}'.")
    {
        Resource = resource;
        Action = action;
        UserId = userId;
    }

    public static ForbiddenException ForResource(string resource, string? userId = null)
    {
        return new ForbiddenException(
            resource,
            "acceder",
            userId);
    }

    public static ForbiddenException ForAction(string action, string? userId = null)
    {
        return new ForbiddenException(
            "recurso",
            action,
            userId);
    }
}