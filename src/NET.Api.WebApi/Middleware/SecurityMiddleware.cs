using NET.Api.Shared.Models;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NET.Api.WebApi.Middleware;

/// <summary>
/// Middleware de seguridad para proteger contra ataques comunes
/// </summary>
public class SecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityMiddleware> _logger;
    private readonly SecurityOptions _options;

    // Patrones para detectar ataques comunes
    private static readonly Regex SqlInjectionPattern = new(
        @"('|(\-\-)|(;)|(\||\|)|(\*|\*)|(%)|(\+)|(\=)|(\<)|(\>)|(\^)|(\()|(\))|(\[)|(\])|(\{)|(\})|(,)|(\.)|(\?)|(\\)|(/)|(\:)|(\!)|(\@)|(\#)|(\$)|(\&)|(\~)|(\`)|(\|)|(\+)|(\=)|(\<)|(\>)|(\^)|(\()|(\))|(\[)|(\])|(\{)|(\})|(,)|(\.)|(\?)|(\\)|(/)|(\:)|(\!)|(\@)|(\#)|(\$)|(\&)|(\~)|(\`)|(select|insert|update|delete|drop|create|alter|exec|execute|union|script|javascript|vbscript|onload|onerror|onclick)\s",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex XssPattern = new(
        @"<script[^>]*>.*?</script>|javascript:|vbscript:|onload=|onerror=|onclick=|onmouseover=|<iframe|<object|<embed|<link|<meta",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PathTraversalPattern = new(
        @"\.\.[\\/]|\.\.%2f|\.\.%5c|%2e%2e[\\/]|%2e%2e%2f|%2e%2e%5c",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public SecurityMiddleware(
        RequestDelegate next,
        ILogger<SecurityMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _options = configuration.GetSection("Security").Get<SecurityOptions>() ?? new SecurityOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Validar headers de seguridad
            if (!ValidateSecurityHeaders(context))
            {
                await HandleSecurityViolation(context, "INVALID_SECURITY_HEADERS", "Headers de seguridad inválidos detectados.");
                return;
            }

            // Validar tamaño del request
            if (!ValidateRequestSize(context))
            {
                await HandleSecurityViolation(context, "REQUEST_TOO_LARGE", "El tamaño de la solicitud excede el límite permitido.");
                return;
            }

            // Validar URL y query parameters
            if (!ValidateUrl(context))
            {
                await HandleSecurityViolation(context, "MALICIOUS_URL_DETECTED", "URL maliciosa detectada.");
                return;
            }

            // Validar User-Agent
            if (!ValidateUserAgent(context))
            {
                await HandleSecurityViolation(context, "SUSPICIOUS_USER_AGENT", "User-Agent sospechoso detectado.");
                return;
            }

            // Validar contenido del body para requests POST/PUT/PATCH
            if (context.Request.Method != "GET" && context.Request.Method != "DELETE")
            {
                if (!await ValidateRequestBody(context))
                {
                    await HandleSecurityViolation(context, "MALICIOUS_CONTENT_DETECTED", "Contenido malicioso detectado en el cuerpo de la solicitud.");
                    return;
                }
            }

            // Añadir headers de seguridad a la respuesta
            AddSecurityHeaders(context);

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en SecurityMiddleware para {RequestPath}", context.Request.Path);
            await HandleSecurityViolation(context, "SECURITY_ERROR", "Error de seguridad interno.");
        }
    }

    private bool ValidateSecurityHeaders(HttpContext context)
    {
        if (!_options.ValidateHeaders) return true;

        // Validar Content-Type para requests con body
        if (context.Request.ContentLength > 0)
        {
            var contentType = context.Request.ContentType;
            if (string.IsNullOrEmpty(contentType))
            {
                _logger.LogWarning("Request sin Content-Type detectado desde {RemoteIP}", 
                    context.Connection.RemoteIpAddress);
                return false;
            }

            var allowedContentTypes = new[] 
            {
                "application/json",
                "application/x-www-form-urlencoded",
                "multipart/form-data",
                "text/plain"
            };

            if (!allowedContentTypes.Any(allowed => contentType.StartsWith(allowed, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Content-Type no permitido: {ContentType} desde {RemoteIP}", 
                    contentType, context.Connection.RemoteIpAddress);
                return false;
            }
        }

        return true;
    }

    private bool ValidateRequestSize(HttpContext context)
    {
        if (!_options.ValidateRequestSize) return true;

        var contentLength = context.Request.ContentLength ?? 0;
        if (contentLength > _options.MaxRequestSizeBytes)
        {
            _logger.LogWarning("Request demasiado grande: {Size} bytes desde {RemoteIP}", 
                contentLength, context.Connection.RemoteIpAddress);
            return false;
        }

        return true;
    }

    private bool ValidateUrl(HttpContext context)
    {
        if (!_options.ValidateUrls) return true;

        var url = context.Request.Path + context.Request.QueryString;
        
        // Verificar path traversal
        if (PathTraversalPattern.IsMatch(url))
        {
            _logger.LogWarning("Path traversal detectado en URL: {Url} desde {RemoteIP}", 
                url, context.Connection.RemoteIpAddress);
            return false;
        }

        // Verificar XSS en query parameters
        if (XssPattern.IsMatch(url))
        {
            _logger.LogWarning("XSS detectado en URL: {Url} desde {RemoteIP}", 
                url, context.Connection.RemoteIpAddress);
            return false;
        }

        // Verificar SQL injection en query parameters
        if (SqlInjectionPattern.IsMatch(url))
        {
            _logger.LogWarning("SQL injection detectado en URL: {Url} desde {RemoteIP}", 
                url, context.Connection.RemoteIpAddress);
            return false;
        }

        return true;
    }

    private bool ValidateUserAgent(HttpContext context)
    {
        if (!_options.ValidateUserAgent) return true;

        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault();
        
        // Bloquear requests sin User-Agent
        if (string.IsNullOrEmpty(userAgent))
        {
            _logger.LogWarning("Request sin User-Agent desde {RemoteIP}", 
                context.Connection.RemoteIpAddress);
            return false;
        }

        // Bloquear User-Agents sospechosos
        var suspiciousUserAgents = new[] 
        {
            "sqlmap", "nikto", "nmap", "masscan", "zap", "burp", "w3af",
            "curl", "wget", "python-requests", "bot", "crawler", "spider"
        };

        if (suspiciousUserAgents.Any(suspicious => 
            userAgent.Contains(suspicious, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("User-Agent sospechoso detectado: {UserAgent} desde {RemoteIP}", 
                userAgent, context.Connection.RemoteIpAddress);
            return false;
        }

        return true;
    }

    private async Task<bool> ValidateRequestBody(HttpContext context)
    {
        if (!_options.ValidateRequestBody) return true;

        if (context.Request.ContentLength == 0) return true;

        // Leer el body de forma que pueda ser reutilizado
        context.Request.EnableBuffering();
        
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        if (string.IsNullOrEmpty(body)) return true;

        // Verificar XSS
        if (XssPattern.IsMatch(body))
        {
            _logger.LogWarning("XSS detectado en body desde {RemoteIP}", 
                context.Connection.RemoteIpAddress);
            return false;
        }

        // Verificar SQL injection
        if (SqlInjectionPattern.IsMatch(body))
        {
            _logger.LogWarning("SQL injection detectado en body desde {RemoteIP}", 
                context.Connection.RemoteIpAddress);
            return false;
        }

        return true;
    }

    private void AddSecurityHeaders(HttpContext context)
    {
        if (!_options.AddSecurityHeaders) return;

        var headers = context.Response.Headers;

        // Prevenir clickjacking
        headers.TryAdd("X-Frame-Options", "DENY");
        
        // Prevenir MIME type sniffing
        headers.TryAdd("X-Content-Type-Options", "nosniff");
        
        // Habilitar XSS protection
        headers.TryAdd("X-XSS-Protection", "1; mode=block");
        
        // Referrer policy
        headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
        
        // Content Security Policy básico
        headers.TryAdd("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-ancestors 'none';");
        
        // Strict Transport Security (solo en HTTPS)
        if (context.Request.IsHttps)
        {
            headers.TryAdd("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }
    }

    private async Task HandleSecurityViolation(HttpContext context, string errorCode, string message)
    {
        _logger.LogWarning(
            "Violación de seguridad: {ErrorCode} - {Message} desde {RemoteIP} para {RequestPath}",
            errorCode, message, context.Connection.RemoteIpAddress, context.Request.Path);

        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        var errorResponse = ErrorResponse.Create(
            errorCode,
            message,
            "La solicitud ha sido bloqueada por razones de seguridad.",
            context.TraceIdentifier,
            suggestions: new List<string> { "Verifica que tu solicitud no contenga contenido malicioso", "Contacta al administrador si crees que esto es un error" });

        var jsonResponse = JsonSerializer.Serialize(ApiResponse<object>.ErrorResponse(errorResponse), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Opciones de configuración para el middleware de seguridad
/// </summary>
public class SecurityOptions
{
    /// <summary>
    /// Validar headers de seguridad
    /// </summary>
    public bool ValidateHeaders { get; set; } = true;

    /// <summary>
    /// Validar tamaño de requests
    /// </summary>
    public bool ValidateRequestSize { get; set; } = true;

    /// <summary>
    /// Tamaño máximo de request en bytes
    /// </summary>
    public long MaxRequestSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Validar URLs contra ataques
    /// </summary>
    public bool ValidateUrls { get; set; } = true;

    /// <summary>
    /// Validar User-Agent
    /// </summary>
    public bool ValidateUserAgent { get; set; } = true;

    /// <summary>
    /// Validar contenido del body
    /// </summary>
    public bool ValidateRequestBody { get; set; } = true;

    /// <summary>
    /// Añadir headers de seguridad a las respuestas
    /// </summary>
    public bool AddSecurityHeaders { get; set; } = true;
}