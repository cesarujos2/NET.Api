using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NET.Api.Infrastructure.Data;
using System.Text;
using System.Text.Json;

namespace NET.Api.IntegrationTests.Helpers;

public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;
    protected readonly ApplicationDbContext DbContext;

    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
                });

                // Ensure the database is created
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
            });

            builder.UseEnvironment("Testing");
        });

        Client = Factory.CreateClient();
        Scope = Factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    protected StringContent CreateJsonContent(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    protected async Task SeedDatabaseAsync()
    {
        // Clear existing data
        DbContext.Users.RemoveRange(DbContext.Users);
        DbContext.Roles.RemoveRange(DbContext.Roles);
        await DbContext.SaveChangesAsync();

        // Seed test data if needed
        // This can be overridden in derived classes
    }

    protected async Task CleanDatabaseAsync()
    {
        DbContext.Users.RemoveRange(DbContext.Users);
        DbContext.Roles.RemoveRange(DbContext.Roles);
        await DbContext.SaveChangesAsync();
    }

    public virtual void Dispose()
    {
        Scope?.Dispose();
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}