namespace NET.Api.Application.Common.Exceptions;

/// <summary>
/// Excepción para violaciones de reglas de negocio
/// </summary>
public class BusinessRuleException : ApplicationException
{
    public string RuleName { get; }
    public object? Context { get; }

    public BusinessRuleException(string ruleName, string message, object? context = null)
        : base(message)
    {
        RuleName = ruleName;
        Context = context;
    }

    public BusinessRuleException(string message) : base(message)
    {
        RuleName = string.Empty;
        Context = null;
    }
}