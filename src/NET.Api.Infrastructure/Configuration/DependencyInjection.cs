using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NET.Api.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add infrastructure services here
        // TODO: Add database context when needed
        // TODO: Add repository implementations when needed
        // TODO: Add external service implementations when needed
        
        return services;
    }
}
