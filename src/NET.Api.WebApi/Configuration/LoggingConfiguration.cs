using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace NET.Api.WebApi.Configuration;

/// <summary>
/// Configuración avanzada de logging para la aplicación
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configura Serilog con configuración optimizada para desarrollo y producción
    /// </summary>
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        var environment = builder.Environment.EnvironmentName;
        var isDevelopment = builder.Environment.IsDevelopment();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", isDevelopment ? LogEventLevel.Information : LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", environment)
            .Enrich.WithProperty("Application", "NET.Api")
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate: isDevelopment 
                    ? "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
                    : "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/app-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                formatter: new CompactJsonFormatter(),
                restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.File(
                path: "logs/errors-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 90,
                formatter: new CompactJsonFormatter(),
                restrictedToMinimumLevel: LogEventLevel.Error)
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    /// <summary>
    /// Configura el logging nativo de ASP.NET Core como fallback
    /// </summary>
    public static void ConfigureNativeLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        
        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
        }
        
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
        
        // Configurar niveles de log específicos
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
        builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
    }

    /// <summary>
    /// Añade middleware de logging de requests HTTP
    /// </summary>
    public static void UseRequestLogging(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                options.GetLevel = (httpContext, elapsed, ex) => ex != null
                    ? LogEventLevel.Error
                    : httpContext.Response.StatusCode > 499
                        ? LogEventLevel.Error
                        : LogEventLevel.Information;
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
                    diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
                    
                    if (httpContext.User.Identity?.IsAuthenticated == true)
                    {
                        diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? httpContext.User.FindFirst("id")?.Value);
                        diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
                    }
                };
            });
        }
    }
}