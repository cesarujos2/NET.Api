using Microsoft.EntityFrameworkCore;
using NET.Api.Infrastructure.Persistence;
using NET.Api.WebApi.Configuration;
using NET.Api.WebApi.Middleware;
using NET.Api.Shared.Constants;
using Serilog;
using NET.Api.Middleware;

// Configurar Serilog temprano para capturar logs de inicio
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando aplicación NET.Api...");
    
    var builder = WebApplication.CreateBuilder(args);

    // Configurar logging avanzado
    builder.ConfigureSerilog();

    // Añadir configuración adicional de error handling
    builder.Configuration.AddJsonFile("appsettings.ErrorHandling.json", optional: true, reloadOnChange: true);

    // Add services using dependency injection configuration
    builder.Services.AddWebApiServices(builder.Configuration);
    
    // Añadir servicios de memoria cache para rate limiting
    builder.Services.AddMemoryCache();

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
        
        logger.LogInformation("Seeding database...");
        await DataSeeder.SeedAsync(scope.ServiceProvider);
        logger.LogInformation("Database seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}

    // Configure the HTTP request pipeline
    Log.Information("Configurando pipeline de middleware...");
    
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Middleware de logging de requests (debe ir temprano)
    app.UseRequestLogging();

    // Middleware de seguridad (debe ir antes que otros middlewares)
    app.UseMiddleware<SecurityMiddleware>();

    // Middleware de rate limiting
    app.UseMiddleware<RateLimitingMiddleware>();

    // Middleware de manejo de excepciones (debe ir después de seguridad pero antes de otros)
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseHttpsRedirection();

    // Add CORS
    app.UseCors(ApiConstants.Policies.DefaultCors);

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Aplicación NET.Api iniciada correctamente en {Environment}", app.Environment.EnvironmentName);
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error fatal al iniciar la aplicación NET.Api");
    throw;
}
finally
{
    Log.Information("Cerrando aplicación NET.Api...");
    Log.CloseAndFlush();
}
