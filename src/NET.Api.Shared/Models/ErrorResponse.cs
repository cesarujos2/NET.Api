using System.Text.Json.Serialization;

namespace NET.Api.Shared.Models;

/// <summary>
/// Modelo de respuesta de error estructurado
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Código de error único para identificación
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje principal del error
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detalles adicionales del error
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Timestamp del error
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID único para rastrear el error
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Errores de validación específicos por campo
    /// </summary>
    public IDictionary<string, string[]>? ValidationErrors { get; set; }

    /// <summary>
    /// Información adicional del contexto del error
    /// </summary>
    public object? Context { get; set; }

    /// <summary>
    /// Indica si el error es recuperable
    /// </summary>
    public bool IsRetryable { get; set; } = false;

    /// <summary>
    /// Sugerencias para resolver el error
    /// </summary>
    public List<string>? Suggestions { get; set; }

    /// <summary>
    /// Enlaces a documentación relacionada
    /// </summary>
    public List<string>? HelpLinks { get; set; }

    /// <summary>
    /// Información del stack trace (solo en desarrollo)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StackTrace { get; set; }

    public static ErrorResponse Create(
        string errorCode,
        string message,
        string? details = null,
        string? traceId = null,
        IDictionary<string, string[]>? validationErrors = null,
        object? context = null,
        bool isRetryable = false,
        List<string>? suggestions = null,
        List<string>? helpLinks = null,
        string? stackTrace = null)
    {
        return new ErrorResponse
        {
            ErrorCode = errorCode,
            Message = message,
            Details = details,
            TraceId = traceId ?? Guid.NewGuid().ToString(),
            ValidationErrors = validationErrors,
            Context = context,
            IsRetryable = isRetryable,
            Suggestions = suggestions,
            HelpLinks = helpLinks,
            StackTrace = stackTrace
        };
    }
}