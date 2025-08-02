using MediatR;
using Microsoft.AspNetCore.Mvc;
using NET.Api.Application.Common.Exceptions;
using NET.Api.Shared.Models;
using System.Security.Claims;

namespace NET.Api.WebApi.Controllers;

/// <summary>
/// Controlador base con funcionalidades comunes y manejo de errores mejorado
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    private ILogger? _logger;

    /// <summary>
    /// Instancia de MediatR para envío de comandos y queries
    /// </summary>
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Logger específico del controlador
    /// </summary>
    protected ILogger Logger => _logger ??= HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
        .CreateLogger(GetType());

    /// <summary>
    /// ID del usuario autenticado actual
    /// </summary>
    protected string? CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// Email del usuario autenticado actual
    /// </summary>
    protected string? CurrentUserEmail => User.FindFirst(ClaimTypes.Email)?.Value;

    /// <summary>
    /// Nombre del usuario autenticado actual
    /// </summary>
    protected string? CurrentUserName => User.FindFirst(ClaimTypes.Name)?.Value;

    /// <summary>
    /// Verifica si el usuario está autenticado
    /// </summary>
    protected bool IsAuthenticated => User.Identity?.IsAuthenticated == true;

    /// <summary>
    /// Maneja el resultado de una operación exitosa
    /// </summary>
    protected ActionResult<ApiResponse<T>> HandleResult<T>(T result, string? message = null)
    {
        if (result == null)
        {
            Logger.LogWarning("Resultado nulo devuelto en {ControllerName}", GetType().Name);
            return NotFound(ApiResponse<T>.ErrorResponse(
                "RESOURCE_NOT_FOUND",
                "El recurso solicitado no fue encontrado."));
        }

        return Ok(ApiResponse<T>.SuccessResponse(
            result, 
            message ?? "Operación completada exitosamente."));
    }

    /// <summary>
    /// Maneja el resultado de una operación exitosa con metadatos
    /// </summary>
    protected ActionResult<ApiResponse<T>> HandleResult<T>(T result, string message, object metadata)
    {
        if (result == null)
        {
            Logger.LogWarning("Resultado nulo devuelto en {ControllerName}", GetType().Name);
            return NotFound(ApiResponse<T>.ErrorResponse(
                "RESOURCE_NOT_FOUND",
                "El recurso solicitado no fue encontrado."));
        }

        return Ok(ApiResponse<T>.SuccessResponse(result, message, metadata));
    }

    /// <summary>
    /// Maneja errores personalizados
    /// </summary>
    protected ActionResult<ApiResponse<T>> HandleError<T>(
        string errorCode,
        string message,
        int statusCode = 400,
        string? details = null,
        IDictionary<string, string[]>? validationErrors = null)
    {
        Logger.LogWarning(
            "Error manejado en {ControllerName}: {ErrorCode} - {Message}",
            GetType().Name, errorCode, message);

        var response = ApiResponse<T>.ErrorResponse(
            errorCode,
            message,
            details,
            validationErrors);

        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Maneja errores de validación
    /// </summary>
    protected ActionResult<ApiResponse<T>> HandleValidationError<T>(IDictionary<string, string[]> validationErrors)
    {
        Logger.LogWarning(
            "Errores de validación en {ControllerName}: {ErrorCount} errores",
            GetType().Name, validationErrors.Count);

        return BadRequest(ApiResponse<T>.ErrorResponse(
            "VALIDATION_ERROR",
            "Se encontraron errores de validación.",
            validationErrors: validationErrors));
    }

    /// <summary>
    /// Verifica que el usuario esté autenticado
    /// </summary>
    protected ActionResult<ApiResponse<T>>? RequireAuthentication<T>()
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(CurrentUserId))
        {
            Logger.LogWarning(
                "Acceso no autorizado en {ControllerName}",
                GetType().Name);

            return Unauthorized(ApiResponse<T>.ErrorResponse(
                "UNAUTHORIZED_ACCESS",
                "Debes autenticarte para acceder a este recurso."));
        }

        return null;
    }

    /// <summary>
    /// Ejecuta una operación de forma segura con manejo de errores automático
    /// </summary>
    protected async Task<ActionResult<ApiResponse<T>>> ExecuteSafelyAsync<T>(
        Func<Task<T>> operation,
        string? successMessage = null)
    {
        try
        {
            var result = await operation();
            return HandleResult(result, successMessage);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<T>.ErrorResponse(
                "RESOURCE_NOT_FOUND",
                ex.Message));
        }
        catch (ValidationException ex)
        {
            return HandleValidationError<T>(ex.Errors);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ApiResponse<T>.ErrorResponse(
                "BUSINESS_RULE_VIOLATION",
                ex.Message));
        }
        catch (ConflictException ex)
        {
            return Conflict(ApiResponse<T>.ErrorResponse(
                "RESOURCE_CONFLICT",
                ex.Message));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, ApiResponse<T>.ErrorResponse(
                "ACCESS_FORBIDDEN",
                ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse<T>.ErrorResponse(
                "UNAUTHORIZED_ACCESS",
                "No tienes permisos para realizar esta operación."));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, 
                "Error no controlado en {ControllerName}",
                GetType().Name);
            
            return StatusCode(500, ApiResponse<T>.ErrorResponse(
                "INTERNAL_SERVER_ERROR",
                "Ha ocurrido un error interno del servidor."));
        }
    }
}
