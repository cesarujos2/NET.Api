using System.Net;
using System.Text.Json;
using NET.Api.Application.Common.Exceptions;
using NET.Api.Shared.Models;
using ApplicationException = NET.Api.Application.Common.Exceptions.ApplicationException;

namespace NET.Api.Middleware;

/// <summary>
/// Middleware avanzado para manejo centralizado de excepciones
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IWebHostEnvironment environment)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;
    private readonly IWebHostEnvironment _environment = environment;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var userId = context.User?.Identity?.Name;
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        // Log del error con contexto completo
        LogException(exception, traceId, userId, requestPath, requestMethod);

        // Configurar respuesta HTTP
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = GetStatusCode(exception);

        // Crear respuesta de error estructurada
        var errorResponse = CreateErrorResponse(exception, traceId);
        var apiResponse = ApiResponse<object>.ErrorResponse(errorResponse);

        // Serializar y enviar respuesta
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        var jsonResponse = JsonSerializer.Serialize(apiResponse, options);
        await context.Response.WriteAsync(jsonResponse);
    }

    private void LogException(Exception exception, string traceId, string? userId, string requestPath, string requestMethod)
    {
        var logLevel = GetLogLevel(exception);
        var message = "Error procesando solicitud {RequestMethod} {RequestPath} para usuario {UserId}";
        
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = traceId,
            ["UserId"] = userId ?? "Anonymous",
            ["RequestPath"] = requestPath,
            ["RequestMethod"] = requestMethod,
            ["ExceptionType"] = exception.GetType().Name
        });

        _logger.Log(logLevel, exception, message, requestMethod, requestPath, userId ?? "Anonymous");
    }

    private static LogLevel GetLogLevel(Exception exception)
    {
        return exception switch
        {
            ValidationException => LogLevel.Warning,
            NotFoundException => LogLevel.Warning,
            BusinessRuleException => LogLevel.Warning,
            ConflictException => LogLevel.Warning,
            ForbiddenException => LogLevel.Warning,
            UnauthorizedAccessException => LogLevel.Warning,
            ArgumentException => LogLevel.Warning,
            ExternalServiceException => LogLevel.Error,
            ApplicationException => LogLevel.Error,
            _ => LogLevel.Error
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            NotFoundException => (int)HttpStatusCode.NotFound,
            BusinessRuleException => (int)HttpStatusCode.BadRequest,
            ConflictException => (int)HttpStatusCode.Conflict,
            ForbiddenException => (int)HttpStatusCode.Forbidden,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            ExternalServiceException => (int)HttpStatusCode.BadGateway,
            ApplicationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    private ErrorResponse CreateErrorResponse(Exception exception, string traceId)
    {
        return exception switch
        {
            ValidationException validationEx => ErrorResponse.Create(
                errorCode: "VALIDATION_ERROR",
                message: "Se encontraron errores de validación.",
                details: validationEx.Message,
                traceId: traceId,
                validationErrors: validationEx.Errors,
                suggestions: new List<string> { "Revisa los campos marcados como inválidos." }
            ),

            NotFoundException notFoundEx => ErrorResponse.Create(
                errorCode: "RESOURCE_NOT_FOUND",
                message: notFoundEx.Message,
                traceId: traceId,
                context: new { Resource = notFoundEx.ResourceName, Key = notFoundEx.ResourceKey },
                suggestions: new List<string> { "Verifica que el identificador sea correcto." }
            ),

            BusinessRuleException businessEx => ErrorResponse.Create(
                errorCode: "BUSINESS_RULE_VIOLATION",
                message: businessEx.Message,
                traceId: traceId,
                context: new { Rule = businessEx.RuleName, businessEx.Context },
                suggestions: new List<string> { "Revisa las reglas de negocio aplicables." }
            ),

            ConflictException conflictEx => ErrorResponse.Create(
                errorCode: "RESOURCE_CONFLICT",
                message: conflictEx.Message,
                traceId: traceId,
                context: new { Resource = conflictEx.ResourceName, Value = conflictEx.ConflictingValue },
                suggestions: new List<string> { "Usa un valor diferente o actualiza el recurso existente." }
            ),

            ForbiddenException forbiddenEx => ErrorResponse.Create(
                errorCode: "ACCESS_FORBIDDEN",
                message: forbiddenEx.Message,
                traceId: traceId,
                context: new { forbiddenEx.Resource, forbiddenEx.Action },
                suggestions: new List<string> { "Contacta al administrador para obtener los permisos necesarios." }
            ),

            UnauthorizedAccessException => ErrorResponse.Create(
                errorCode: "UNAUTHORIZED_ACCESS",
                message: "Acceso no autorizado. Debes autenticarte para acceder a este recurso.",
                traceId: traceId,
                suggestions: new List<string> { "Inicia sesión con credenciales válidas." }
            ),

            ExternalServiceException externalEx => ErrorResponse.Create(
                errorCode: externalEx.ErrorCode ?? "EXTERNAL_SERVICE_ERROR",
                message: $"Error en servicio externo: {externalEx.ServiceName}",
                details: externalEx.Message,
                traceId: traceId,
                context: new { Service = externalEx.ServiceName, externalEx.ResponseData },
                isRetryable: true,
                suggestions: new List<string> { "Intenta nuevamente en unos momentos." }
            ),

            ArgumentException argEx => ErrorResponse.Create(
                errorCode: "INVALID_ARGUMENT",
                message: "Argumento inválido proporcionado.",
                details: argEx.Message,
                traceId: traceId,
                suggestions: new List<string> { "Verifica los parámetros enviados." }
            ),

            ApplicationException appEx => ErrorResponse.Create(
                errorCode: "APPLICATION_ERROR",
                message: appEx.Message,
                traceId: traceId,
                suggestions: new List<string> { "Contacta al soporte técnico si el problema persiste." }
            ),

            _ => ErrorResponse.Create(
                errorCode: "INTERNAL_SERVER_ERROR",
                message: "Ha ocurrido un error interno del servidor.",
                details: _environment.IsDevelopment() ? exception.Message : null,
                traceId: traceId,
                isRetryable: true,
                suggestions: new List<string> { "Intenta nuevamente. Si el problema persiste, contacta al soporte técnico." },
                stackTrace: _environment.IsDevelopment() ? exception.StackTrace : null
            )
        };
    }
}
