using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NET.Api.Domain.Entities;
using NET.Api.Infrastructure.Persistence.Seeders;
using NET.Api.Shared.Constants;

namespace NET.Api.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await RoleSeeder.SeedAsync(roleManager, logger);
            await SeedOwnerUserAsync(userManager, logger);
            await SeedEmailTemplatesAsync(serviceProvider, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }



    private static async Task SeedOwnerUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        const string ownerEmail = "owner@netapi.com";
        const string ownerPassword = "Owner123!";

        var existingUser = await userManager.FindByEmailAsync(ownerEmail);
        if (existingUser == null)
        {
            var ownerUser = new ApplicationUser
            {
                UserName = ownerEmail,
                Email = ownerEmail,
                FirstName = "System",
                LastName = "Owner",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(ownerUser, ownerPassword);
            if (result.Succeeded)
            {
                // Assign Owner role
                await userManager.AddToRoleAsync(ownerUser, RoleConstants.Names.Owner);
                logger.LogInformation($"Owner user created successfully with email: {ownerEmail}");
                logger.LogInformation($"Owner user password: {ownerPassword}");
            }
            else
            {
                logger.LogError($"Failed to create owner user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            logger.LogInformation("Owner user already exists.");
            
            // Ensure owner has the Owner role
            if (!await userManager.IsInRoleAsync(existingUser, RoleConstants.Names.Owner))
            {
                await userManager.AddToRoleAsync(existingUser, RoleConstants.Names.Owner);
                logger.LogInformation("Owner role assigned to existing owner user.");
            }
        }
    }
    
    private static async Task SeedEmailTemplatesAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await EmailTemplateSeeder.SeedAsync(context);
            logger.LogInformation("Email templates seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding email templates.");
            throw;
        }
    }
}