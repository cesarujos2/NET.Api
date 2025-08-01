using NET.Api.Application.Mappings;
using NET.Api.Infrastructure.Configuration;
using FluentValidation;
using MediatR;
using System.Reflection;

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
        // Add MediatR
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(NET.Api.Application.Commands.ICommand).Assembly);
        });

        // Add AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // Add FluentValidation
        services.AddValidatorsFromAssembly(typeof(NET.Api.Application.Commands.ICommand).Assembly);

        return services;
    }
}
