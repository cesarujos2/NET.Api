using System.Text.Json.Serialization;

namespace NET.Api.Shared.Models;

/// <summary>
/// Respuesta estándar de la API con soporte para errores estructurados
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje principal de la respuesta
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Datos de la respuesta (solo en caso de éxito)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    /// <summary>
    /// Información detallada del error (solo en caso de fallo)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ErrorResponse? Error { get; set; }

    /// <summary>
    /// Lista simple de errores (mantenida por compatibilidad)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Metadatos adicionales de la respuesta
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Metadata { get; set; }

    /// <summary>
    /// Timestamp de la respuesta
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Crea una respuesta exitosa
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operación exitosa", object? metadata = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Crea una respuesta de error simple
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }

    /// <summary>
    /// Crea una respuesta de error estructurada
    /// </summary>
    public static ApiResponse<T> ErrorResponse(ErrorResponse error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = error.Message,
            Error = error
        };
    }

    /// <summary>
    /// Crea una respuesta de error con código específico
    /// </summary>
    public static ApiResponse<T> ErrorResponse(
        string errorCode,
        string message,
        string? details = null,
        IDictionary<string, string[]>? validationErrors = null,
        object? context = null,
        bool isRetryable = false,
        List<string>? suggestions = null)
    {
        var error = Models.ErrorResponse.Create(
            errorCode,
            message,
            details,
            traceId: Guid.NewGuid().ToString(),
            validationErrors: validationErrors,
            context: context,
            isRetryable: isRetryable,
            suggestions: suggestions);

        return new ApiResponse<T>
        {
            Success = false,
            Error = error
        };
    }
}
