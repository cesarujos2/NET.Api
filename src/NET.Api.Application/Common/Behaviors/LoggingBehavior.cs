using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace NET.Api.Application.Common.Behaviors;

/// <summary>
/// Behavior de MediatR para logging de performance y auditoría
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Iniciando procesamiento de {RequestName} con ID {RequestId}",
            requestName, requestId);

        // Log de request en modo debug
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            _logger.LogDebug(
                "Request {RequestName} ({RequestId}) - Datos: {RequestData}",
                requestName, requestId, requestJson);
        }

        TResponse response;
        try
        {
            response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "Completado {RequestName} ({RequestId}) en {ElapsedMs}ms",
                requestName, requestId, stopwatch.ElapsedMilliseconds);

            // Log de response en modo debug
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                _logger.LogDebug(
                    "Response {RequestName} ({RequestId}) - Datos: {ResponseData}",
                    requestName, requestId, responseJson);
            }

            // Warning para operaciones lentas
            if (stopwatch.ElapsedMilliseconds > 3000)
            {
                _logger.LogWarning(
                    "Operación lenta detectada: {RequestName} ({RequestId}) tomó {ElapsedMs}ms",
                    requestName, requestId, stopwatch.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex,
                "Error procesando {RequestName} ({RequestId}) después de {ElapsedMs}ms",
                requestName, requestId, stopwatch.ElapsedMilliseconds);
            
            throw;
        }

        return response;
    }
}