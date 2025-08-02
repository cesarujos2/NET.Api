using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NET.Api.Domain.Entities;

namespace NET.Api.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await SeedRolesAsync(roleManager, logger);
            await SeedOwnerUserAsync(userManager, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        var roles = new[] { "Owner", "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var identityRole = new IdentityRole(role);
                var result = await roleManager.CreateAsync(identityRole);
                
                if (result.Succeeded)
                {
                    logger.LogInformation($"Role '{role}' created successfully.");
                }
                else
                {
                    logger.LogError($"Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }

    private static async Task SeedOwnerUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        const string ownerEmail = "owner@netapi.com";
        const string ownerPassword = "Owner123!";
        const string ownerDocument = "OWNER001";

        var existingUser = await userManager.FindByEmailAsync(ownerEmail);
        if (existingUser == null)
        {
            var ownerUser = new ApplicationUser
            {
                UserName = ownerEmail,
                Email = ownerEmail,
                FirstName = "System",
                LastName = "Owner",
                IdentityDocument = ownerDocument,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(ownerUser, ownerPassword);
            if (result.Succeeded)
            {
                // Assign Owner role
                await userManager.AddToRoleAsync(ownerUser, "Owner");
                logger.LogInformation($"Owner user created successfully with email: {ownerEmail}");
                logger.LogInformation($"Owner user password: {ownerPassword}");
                logger.LogInformation($"Owner user document: {ownerDocument}");
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
            if (!await userManager.IsInRoleAsync(existingUser, "Owner"))
            {
                await userManager.AddToRoleAsync(existingUser, "Owner");
                logger.LogInformation("Owner role assigned to existing owner user.");
            }
        }
    }
}