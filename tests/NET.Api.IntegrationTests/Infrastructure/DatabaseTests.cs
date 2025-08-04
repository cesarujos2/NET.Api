using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NET.Api.Domain.Entities;
using NET.Api.Infrastructure.Data;
using NET.Api.IntegrationTests.Helpers;
using NET.Api.Shared.Constants;

namespace NET.Api.IntegrationTests.Infrastructure;

public class DatabaseTests : IntegrationTestBase
{
    public DatabaseTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Database_ShouldBeCreatedAndAccessible()
    {
        // Act & Assert
        DbContext.Should().NotBeNull();
        await DbContext.Database.EnsureCreatedAsync();
        
        var canConnect = await DbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task ApplicationUser_ShouldBeCreatedWithCorrectProperties()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var user = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            UserName = "john.doe@test.com",
            IdentityDocument = "12345678",
            IsActive = true
        };

        // Act
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedUser = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        savedUser.Should().NotBeNull();
        savedUser!.FirstName.Should().Be("John");
        savedUser.LastName.Should().Be("Doe");
        savedUser.FullName.Should().Be("John Doe");
        savedUser.IdentityDocument.Should().Be("12345678");
        savedUser.IsActive.Should().BeTrue();
        savedUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ApplicationUser_EmailIndex_ShouldEnforceUniqueness()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var user1 = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "duplicate@test.com",
            UserName = "john.doe@test.com",
            IdentityDocument = "12345678"
        };

        var user2 = new ApplicationUser
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "duplicate@test.com", // Same email
            UserName = "jane.smith@test.com",
            IdentityDocument = "87654321"
        };

        // Act & Assert
        DbContext.Users.Add(user1);
        await DbContext.SaveChangesAsync();

        DbContext.Users.Add(user2);
        
        var act = async () => await DbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ApplicationUser_IdentityDocumentIndex_ShouldEnforceUniqueness()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var user1 = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserName = "john@test.com",
            IdentityDocument = "12345678"
        };

        var user2 = new ApplicationUser
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@test.com",
            UserName = "jane@test.com",
            IdentityDocument = "12345678" // Same identity document
        };

        // Act & Assert
        DbContext.Users.Add(user1);
        await DbContext.SaveChangesAsync();

        DbContext.Users.Add(user2);
        
        var act = async () => await DbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ApplicationRole_ShouldBeCreatedWithCorrectProperties()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var role = new ApplicationRole
        {
            Name = "TestRole",
            Description = "Test role description"
        };

        // Act
        DbContext.Roles.Add(role);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedRole = await DbContext.Roles.FirstOrDefaultAsync(r => r.Name == "TestRole");
        savedRole.Should().NotBeNull();
        savedRole!.Name.Should().Be("TestRole");
        savedRole.Description.Should().Be("Test role description");
        savedRole.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UserManager_ShouldCreateUserSuccessfully()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            UserName = "john.doe@test.com",
            IdentityDocument = "12345678"
        };

        // Act
        var result = await userManager.CreateAsync(user, "Password123!");

        // Assert
        result.Succeeded.Should().BeTrue();
        
        var createdUser = await userManager.FindByEmailAsync(user.Email);
        createdUser.Should().NotBeNull();
        createdUser!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RoleManager_ShouldCreateRoleSuccessfully()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var roleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var role = new ApplicationRole
        {
            Name = "TestRole",
            Description = "Test role for integration testing"
        };

        // Act
        var result = await roleManager.CreateAsync(role);

        // Assert
        result.Succeeded.Should().BeTrue();
        
        var createdRole = await roleManager.FindByNameAsync("TestRole");
        createdRole.Should().NotBeNull();
        createdRole!.Description.Should().Be("Test role for integration testing");
    }

    [Fact]
    public async Task UserRoleAssignment_ShouldWorkCorrectly()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        
        // Create role
        var role = new ApplicationRole { Name = "TestRole", Description = "Test role" };
        await roleManager.CreateAsync(role);
        
        // Create user
        var user = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            UserName = "john.doe@test.com",
            IdentityDocument = "12345678"
        };
        await userManager.CreateAsync(user, "Password123!");

        // Act
        var addToRoleResult = await userManager.AddToRoleAsync(user, "TestRole");

        // Assert
        addToRoleResult.Succeeded.Should().BeTrue();
        
        var userRoles = await userManager.GetRolesAsync(user);
        userRoles.Should().Contain("TestRole");
        
        var isInRole = await userManager.IsInRoleAsync(user, "TestRole");
        isInRole.Should().BeTrue();
    }

    [Fact]
    public async Task DatabaseConstraints_ShouldEnforceMaxLengths()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var user = new ApplicationUser
        {
            FirstName = new string('A', 101), // Exceeds max length of 100
            LastName = "Doe",
            Email = "john.doe@test.com",
            UserName = "john.doe@test.com",
            IdentityDocument = "12345678"
        };

        // Act & Assert
        DbContext.Users.Add(user);
        
        var act = async () => await DbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task BaseEntity_ShouldSetTimestampsAutomatically()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var role = new ApplicationRole
        {
            Name = "TimestampTest",
            Description = "Testing automatic timestamps"
        };

        var beforeCreate = DateTime.UtcNow;

        // Act - Create
        DbContext.Roles.Add(role);
        await DbContext.SaveChangesAsync();
        
        var afterCreate = DateTime.UtcNow;

        // Assert - Creation timestamps
        role.CreatedAt.Should().BeAfter(beforeCreate.AddSeconds(-1));
        role.CreatedAt.Should().BeBefore(afterCreate.AddSeconds(1));
        role.UpdatedAt.Should().BeAfter(beforeCreate.AddSeconds(-1));
        role.UpdatedAt.Should().BeBefore(afterCreate.AddSeconds(1));

        // Act - Update
        await Task.Delay(100); // Ensure different timestamp
        var beforeUpdate = DateTime.UtcNow;
        
        role.Description = "Updated description";
        await DbContext.SaveChangesAsync();
        
        var afterUpdate = DateTime.UtcNow;

        // Assert - Update timestamp changed, creation timestamp unchanged
        role.UpdatedAt.Should().BeAfter(beforeUpdate.AddSeconds(-1));
        role.UpdatedAt.Should().BeBefore(afterUpdate.AddSeconds(1));
        role.UpdatedAt.Should().BeAfter(role.CreatedAt);
    }
}