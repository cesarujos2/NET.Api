using NET.Api.Infrastructure.Configuration;
using FluentValidation;
using MediatR;
using System.Reflection;
using NET.Api.Application.Common.Behaviors;
using NET.Api.Application.Common.Mappings;

namespace NET.Api.WebApi.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add controllers
        services.AddControllers();
        
        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCorsPolicy", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // Add Swagger/OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Add Application layer services
        services.AddApplicationServices();
        
        // Add Infrastructure layer services
        services.AddInfrastructure(configuration);

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Get Application assembly reference
        var applicationAssembly = typeof(MappingProfile).Assembly;
        
        // Add MediatR - Registers all handlers from Application assembly
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
        });

        // Add MediatR Behaviors (order matters - they execute in registration order)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Add AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // Add FluentValidation - Registers all validators from Application assembly
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
