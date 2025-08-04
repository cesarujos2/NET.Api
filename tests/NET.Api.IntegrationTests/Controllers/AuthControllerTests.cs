using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NET.Api.Application.Features.Auth.Commands.Register;
using NET.Api.Domain.Entities;
using NET.Api.IntegrationTests.Helpers;
using NET.Api.Shared.DTOs.Responses;
using System.Net;
using System.Net.Http.Json;

namespace NET.Api.IntegrationTests.Controllers;

public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var registerCommand = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            IdentityDocument = "12345678",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await DeserializeResponseAsync<ApiResponse<object>>(response);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Message.Should().Contain("successfully");

        // Verify user was created in database
        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(registerCommand.Email);
        user.Should().NotBeNull();
        user!.FirstName.Should().Be(registerCommand.FirstName);
        user.LastName.Should().Be(registerCommand.LastName);
        user.IdentityDocument.Should().Be(registerCommand.IdentityDocument);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var existingUser = new ApplicationUser
        {
            FirstName = "Existing",
            LastName = "User",
            Email = "existing@test.com",
            UserName = "existing@test.com",
            IdentityDocument = "87654321"
        };
        await userManager.CreateAsync(existingUser, "Password123!");

        var registerCommand = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@test.com", // Duplicate email
            IdentityDocument = "12345678",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var apiResponse = await DeserializeResponseAsync<ApiResponse<object>>(response);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("email");
    }

    [Fact]
    public async Task Register_WithDuplicateIdentityDocument_ShouldReturnBadRequest()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var existingUser = new ApplicationUser
        {
            FirstName = "Existing",
            LastName = "User",
            Email = "existing@test.com",
            UserName = "existing@test.com",
            IdentityDocument = "12345678"
        };
        await userManager.CreateAsync(existingUser, "Password123!");

        var registerCommand = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            IdentityDocument = "12345678", // Duplicate identity document
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var apiResponse = await DeserializeResponseAsync<ApiResponse<object>>(response);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("identity document");
    }

    [Theory]
    [InlineData("", "Doe", "john@test.com", "12345678", "Password123!", "Password123!")]
    [InlineData("John", "", "john@test.com", "12345678", "Password123!", "Password123!")]
    [InlineData("John", "Doe", "invalid-email", "12345678", "Password123!", "Password123!")]
    [InlineData("John", "Doe", "john@test.com", "", "Password123!", "Password123!")]
    [InlineData("John", "Doe", "john@test.com", "12345678", "weak", "weak")]
    [InlineData("John", "Doe", "john@test.com", "12345678", "Password123!", "DifferentPassword123!")]
    public async Task Register_WithInvalidData_ShouldReturnBadRequest(
        string firstName, string lastName, string email, string identityDocument, 
        string password, string confirmPassword)
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var registerCommand = new RegisterCommand
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            IdentityDocument = identityDocument,
            Password = password,
            ConfirmPassword = confirmPassword
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var apiResponse = await DeserializeResponseAsync<ApiResponse<object>>(response);
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Register_ShouldAssignUserRole()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        // Ensure User role exists
        var roleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = "User", Description = "Standard user role" });
        }

        var registerCommand = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            IdentityDocument = "12345678",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify user has User role
        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(registerCommand.Email);
        user.Should().NotBeNull();
        
        var roles = await userManager.GetRolesAsync(user!);
        roles.Should().Contain("User");
    }

    protected override async Task SeedDatabaseAsync()
    {
        await base.SeedDatabaseAsync();
        
        // Seed required roles
        var roleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        
        var roles = new[]
        {
            new ApplicationRole { Name = "Owner", Description = "System owner with full access" },
            new ApplicationRole { Name = "Admin", Description = "Administrator with elevated privileges" },
            new ApplicationRole { Name = "User", Description = "Standard user role" },
            new ApplicationRole { Name = "Moderator", Description = "Content moderator" },
            new ApplicationRole { Name = "Support", Description = "Customer support representative" }
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                await roleManager.CreateAsync(role);
            }
        }
    }
}