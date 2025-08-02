namespace NET.Api.Application.Common.Exceptions;

/// <summary>
/// Excepción para conflictos de recursos (ej: duplicados)
/// </summary>
public class ConflictException : ApplicationException
{
    public string ResourceName { get; }
    public object? ConflictingValue { get; }

    public ConflictException(string resourceName, object conflictingValue, string message)
        : base(message)
    {
        ResourceName = resourceName;
        ConflictingValue = conflictingValue;
    }

    public ConflictException(string message) : base(message)
    {
        ResourceName = string.Empty;
        ConflictingValue = null;
    }

    public static ConflictException ForDuplicate(string resourceName, object value)
    {
        return new ConflictException(
            resourceName,
            value,
            $"Ya existe un '{resourceName}' con el valor '{value}'.");
    }
}