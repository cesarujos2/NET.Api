using System.Reflection;

namespace NET.Api.ArchitectureTests;

public class CleanArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(NET.Api.Domain.Entities.BaseEntity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(NET.Api.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(NET.Api.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly WebApiAssembly = typeof(Program).Assembly;
    private static readonly Assembly SharedAssembly = typeof(NET.Api.Shared.Constants.RoleConstants).Assembly;

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnOtherLayers()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("NET.Api.Application")
            .And().NotHaveDependencyOn("NET.Api.Infrastructure")
            .And().NotHaveDependencyOn("NET.Api.WebApi")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not depend on other layers. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Domain_ShouldOnlyDependOnSharedAndSystemLibraries()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .OnlyHaveDependenciesOn(
                "NET.Api.Domain",
                "NET.Api.Shared",
                "System",
                "Microsoft.Extensions.Identity.Stores",
                "Microsoft.AspNetCore.Identity",
                "netstandard",
                "mscorlib")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain should only depend on allowed assemblies. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Application_ShouldNotHaveDependencyOnInfrastructureOrWebApi()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("NET.Api.Infrastructure")
            .And().NotHaveDependencyOn("NET.Api.WebApi")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Application layer should not depend on Infrastructure or WebApi layers. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Application_ShouldDependOnDomainAndShared()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .HaveDependencyOn("NET.Api.Domain")
            .And().HaveDependencyOn("NET.Api.Shared")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should depend on Domain and Shared layers.");
    }

    [Fact]
    public void Infrastructure_ShouldDependOnApplicationAndDomain()
    {
        // Arrange & Act
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .HaveDependencyOn("NET.Api.Application")
            .And().HaveDependencyOn("NET.Api.Domain")
            .And().HaveDependencyOn("NET.Api.Shared")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Infrastructure layer should depend on Application, Domain and Shared layers.");
    }

    [Fact]
    public void Infrastructure_ShouldNotHaveDependencyOnWebApi()
    {
        // Arrange & Act
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("NET.Api.WebApi")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Infrastructure layer should not depend on WebApi layer. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void WebApi_ShouldDependOnAllOtherLayers()
    {
        // Arrange & Act
        var result = Types.InAssembly(WebApiAssembly)
            .Should()
            .HaveDependencyOn("NET.Api.Application")
            .And().HaveDependencyOn("NET.Api.Infrastructure")
            .And().HaveDependencyOn("NET.Api.Domain")
            .And().HaveDependencyOn("NET.Api.Shared")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "WebApi layer should depend on all other layers.");
    }

    [Fact]
    public void Shared_ShouldNotHaveDependencyOnOtherLayers()
    {
        // Arrange & Act
        var result = Types.InAssembly(SharedAssembly)
            .Should()
            .NotHaveDependencyOn("NET.Api.Domain")
            .And().NotHaveDependencyOn("NET.Api.Application")
            .And().NotHaveDependencyOn("NET.Api.Infrastructure")
            .And().NotHaveDependencyOn("NET.Api.WebApi")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Shared layer should not depend on other layers. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Domain_Entities_ShouldBePublic()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .That().ResideInNamespace("NET.Api.Domain.Entities")
            .Should().BePublic()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain entities should be public. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Domain_Entities_ShouldNotHavePublicSetters()
    {
        // Arrange
        var entityTypes = Types.InAssembly(DomainAssembly)
            .That().ResideInNamespace("NET.Api.Domain.Entities")
            .And().AreClasses()
            .GetTypes();

        // Act & Assert
        foreach (var entityType in entityTypes)
        {
            var publicSetters = entityType.GetProperties()
                .Where(p => p.SetMethod?.IsPublic == true && p.Name != "Id")
                .ToList();

            // Allow some exceptions for Identity properties and specific cases
            var allowedPublicSetters = new[] { "Email", "UserName", "NormalizedEmail", "NormalizedUserName", 
                "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", 
                "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount", "EmailConfirmed",
                "Name", "NormalizedName", "Description" };

            var violatingSetters = publicSetters
                .Where(p => !allowedPublicSetters.Contains(p.Name))
                .ToList();

            violatingSetters.Should().BeEmpty(
                $"Entity {entityType.Name} should not have public setters except for allowed properties. " +
                $"Violating properties: {string.Join(", ", violatingSetters.Select(p => p.Name))}");
        }
    }

    [Fact]
    public void Application_Commands_ShouldEndWithCommand()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That().ResideInNamespaceMatching(".*Commands.*")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Command")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Command classes should end with 'Command'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Application_Queries_ShouldEndWithQuery()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That().ResideInNamespaceMatching(".*Queries.*")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Query")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Query classes should end with 'Query'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Application_Handlers_ShouldEndWithHandler()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That().ResideInNamespaceMatching(".*Handlers.*")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Handler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Handler classes should end with 'Handler'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void WebApi_Controllers_ShouldEndWithController()
    {
        // Arrange & Act
        var result = Types.InAssembly(WebApiAssembly)
            .That().ResideInNamespace("NET.Api.Controllers")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Controller")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Controller classes should end with 'Controller'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void WebApi_Controllers_ShouldInheritFromBaseApiController()
    {
        // Arrange & Act
        var result = Types.InAssembly(WebApiAssembly)
            .That().ResideInNamespace("NET.Api.Controllers")
            .And().AreClasses()
            .And().DoNotHaveName("BaseApiController")
            .Should().Inherit(Types.InAssembly(WebApiAssembly).That().HaveName("BaseApiController").GetTypes().First())
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Controllers should inherit from BaseApiController. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Infrastructure_Services_ShouldEndWithService()
    {
        // Arrange & Act
        var result = Types.InAssembly(InfrastructureAssembly)
            .That().ResideInNamespaceMatching(".*Services.*")
            .And().AreClasses()
            .And().AreNotAbstract()
            .Should().HaveNameEndingWith("Service")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Service classes should end with 'Service'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Shared_Constants_ShouldBePublicAndStatic()
    {
        // Arrange & Act
        var result = Types.InAssembly(SharedAssembly)
            .That().ResideInNamespaceMatching(".*Constants.*")
            .And().AreClasses()
            .Should().BePublic()
            .And().BeStatic()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Constant classes should be public and static. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }
}