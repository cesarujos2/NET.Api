namespace NET.Api.Application.Common.Exceptions;

/// <summary>
/// Excepción para errores de servicios externos
/// </summary>
public class ExternalServiceException : ApplicationException
{
    public string ServiceName { get; }
    public string? ErrorCode { get; }
    public object? ResponseData { get; }

    public ExternalServiceException(string serviceName, string message, string? errorCode = null, object? responseData = null)
        : base($"Error en el servicio externo '{serviceName}': {message}")
    {
        ServiceName = serviceName;
        ErrorCode = errorCode;
        ResponseData = responseData;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException)
        : base($"Error en el servicio externo '{serviceName}': {message}", innerException)
    {
        ServiceName = serviceName;
        ErrorCode = null;
        ResponseData = null;
    }

    public static ExternalServiceException Unavailable(string serviceName)
    {
        return new ExternalServiceException(
            serviceName,
            "El servicio no está disponible en este momento.",
            "SERVICE_UNAVAILABLE");
    }

    public static ExternalServiceException Timeout(string serviceName)
    {
        return new ExternalServiceException(
            serviceName,
            "El servicio no respondió en el tiempo esperado.",
            "TIMEOUT");
    }
}