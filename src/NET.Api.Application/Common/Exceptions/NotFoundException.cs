namespace NET.Api.Application.Common.Exceptions;

/// <summary>
/// Excepción para recursos no encontrados
/// </summary>
public class NotFoundException : ApplicationException
{
    public string ResourceName { get; }
    public object ResourceKey { get; }

    public NotFoundException(string resourceName, object resourceKey)
        : base($"El recurso '{resourceName}' con identificador '{resourceKey}' no fue encontrado.")
    {
        ResourceName = resourceName;
        ResourceKey = resourceKey;
    }

    public NotFoundException(string message) : base(message)
    {
        ResourceName = string.Empty;
        ResourceKey = string.Empty;
    }
}