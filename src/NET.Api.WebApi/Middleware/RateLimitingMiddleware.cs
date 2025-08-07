using Microsoft.Extensions.Caching.Memory;
using NET.Api.Shared.Models;
using System.Net;
using System.Text.Json;

namespace NET.Api.WebApi.Middleware;

/// <summary>
/// Middleware para implementar rate limiting y throttling
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<RateLimitingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        _options = configuration.GetSection("RateLimit").Get<RateLimitOptions>() ?? new RateLimitOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Obtener identificador del cliente (IP + User ID si está autenticado)
        var clientId = GetClientIdentifier(context);
        var endpoint = GetEndpointIdentifier(context);
        var cacheKey = $"rate_limit_{clientId}_{endpoint}";

        // Verificar si el endpoint requiere rate limiting
        if (!ShouldApplyRateLimit(context))
        {
            await _next(context);
            return;
        }

        // Obtener o crear el contador de requests
        var requestCount = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.WindowSizeInMinutes);
            return new RateLimitCounter
            {
                Count = 0,
                WindowStart = DateTime.UtcNow
            };
        });

        // Verificar si se ha excedido el límite
        if (requestCount!.Count >= _options.MaxRequests)
        {
            _logger.LogWarning(
                "Rate limit exceeded for client {ClientId} on endpoint {Endpoint}. Count: {Count}, Limit: {Limit}",
                clientId, endpoint, requestCount.Count, _options.MaxRequests);

            await HandleRateLimitExceeded(context, requestCount);
            return;
        }

        // Incrementar contador
        requestCount.Count++;
        _cache.Set(cacheKey, requestCount, TimeSpan.FromMinutes(_options.WindowSizeInMinutes));

        // Añadir headers de rate limit
        AddRateLimitHeaders(context, requestCount);

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Priorizar User ID si está autenticado
        var userId = context.User?.FindFirst("sub")?.Value ?? context.User?.FindFirst("id")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user_{userId}";
        }

        // Usar IP como fallback
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Considerar headers de proxy para obtener la IP real
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            ip = forwardedFor.Split(',')[0].Trim();
        }

        return $"ip_{ip}";
    }

    private string GetEndpointIdentifier(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";
        var method = context.Request.Method;
        
        // Normalizar rutas con parámetros (ej: /api/users/123 -> /api/users/{id})
        var normalizedPath = NormalizePath(path);
        
        return $"{method}_{normalizedPath}";
    }

    private string NormalizePath(string path)
    {
        // Implementación básica - en producción se podría usar un sistema más sofisticado
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var normalizedSegments = new List<string>();

        foreach (var segment in segments)
        {
            // Si el segmento es un GUID o número, reemplazarlo con un placeholder
            if (Guid.TryParse(segment, out _) || int.TryParse(segment, out _))
            {
                normalizedSegments.Add("{id}");
            }
            else
            {
                normalizedSegments.Add(segment);
            }
        }

        return "/" + string.Join("/", normalizedSegments);
    }

    private bool ShouldApplyRateLimit(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        
        // No aplicar rate limiting a endpoints de salud y métricas
        var excludedPaths = new[] { "/health", "/metrics", "/swagger" };
        
        return !excludedPaths.Any(excluded => path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase));
    }

    private async Task HandleRateLimitExceeded(HttpContext context, RateLimitCounter counter)
    {
        var timeUntilReset = counter.WindowStart.AddMinutes(_options.WindowSizeInMinutes) - DateTime.UtcNow;
        var retryAfterSeconds = (int)Math.Ceiling(timeUntilReset.TotalSeconds);

        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();
        context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = "0";
        context.Response.Headers["X-RateLimit-Reset"] = ((DateTimeOffset)counter.WindowStart.AddMinutes(_options.WindowSizeInMinutes)).ToUnixTimeSeconds().ToString();
        context.Response.ContentType = "application/json";

        var errorResponse = ErrorResponse.Create(
            "RATE_LIMIT_EXCEEDED",
            "Se ha excedido el límite de solicitudes permitidas.",
            $"Límite: {_options.MaxRequests} solicitudes por {_options.WindowSizeInMinutes} minutos. Intenta nuevamente en {retryAfterSeconds} segundos.",
            context.TraceIdentifier,
            isRetryable: true,
            suggestions: new List<string> { $"Espera {retryAfterSeconds} segundos antes de realizar otra solicitud", "Considera implementar un mecanismo de retry con backoff exponencial" });

        var jsonResponse = JsonSerializer.Serialize(ApiResponse<object>.ErrorResponse(errorResponse), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private void AddRateLimitHeaders(HttpContext context, RateLimitCounter counter)
    {
        var remaining = Math.Max(0, _options.MaxRequests - counter.Count);
        var resetTime = ((DateTimeOffset)counter.WindowStart.AddMinutes(_options.WindowSizeInMinutes)).ToUnixTimeSeconds();

        context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = resetTime.ToString();
    }
}

/// <summary>
/// Opciones de configuración para rate limiting
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// Número máximo de requests permitidos en la ventana de tiempo
    /// </summary>
    public int MaxRequests { get; set; } = 100;

    /// <summary>
    /// Tamaño de la ventana de tiempo en minutos
    /// </summary>
    public int WindowSizeInMinutes { get; set; } = 1;
}

/// <summary>
/// Contador interno para tracking de requests
/// </summary>
internal class RateLimitCounter
{
    public int Count { get; set; }
    public DateTime WindowStart { get; set; }
}