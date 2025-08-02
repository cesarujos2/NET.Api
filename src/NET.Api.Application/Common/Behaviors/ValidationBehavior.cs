using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using NET.Api.Application.Common.Exceptions;

namespace NET.Api.Application.Common.Behaviors;

/// <summary>
/// Behavior de MediatR para validación automática usando FluentValidation
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;
        _logger.LogDebug("Validando request {RequestName}", requestName);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            _logger.LogWarning("Errores de validación encontrados en {RequestName}: {ErrorCount} errores", 
                requestName, failures.Count);
            
            throw new Exceptions.ValidationException(failures);
        }

        _logger.LogDebug("Validación exitosa para {RequestName}", requestName);
        return await next();
    }
}
