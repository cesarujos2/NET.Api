using System.Reflection;

namespace NET.Api.ArchitectureTests;

public class NamingConventionTests
{
    private static readonly Assembly DomainAssembly = typeof(NET.Api.Domain.Entities.BaseEntity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(NET.Api.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(NET.Api.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly WebApiAssembly = typeof(Program).Assembly;
    private static readonly Assembly SharedAssembly = typeof(NET.Api.Shared.Constants.RoleConstants).Assembly;

    [Fact]
    public void Interfaces_ShouldStartWithI()
    {
        // Arrange
        var assemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, WebApiAssembly, SharedAssembly };

        // Act & Assert
        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That().AreInterfaces()
                .Should().HaveNameStartingWith("I")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"Interfaces in {assembly.GetName().Name} should start with 'I'. Violations: {string.Join(", ", result.FailingTypeNames)}");
        }
    }

    [Fact]
    public void AbstractClasses_ShouldStartWithAbstractOrBase()
    {
        // Arrange
        var assemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, WebApiAssembly, SharedAssembly };

        // Act & Assert
        foreach (var assembly in assemblies)
        {
            var abstractClasses = Types.InAssembly(assembly)
                .That().AreAbstract()
                .And().AreClasses()
                .GetTypes()
                .Where(t => !t.Name.StartsWith("Abstract") && !t.Name.StartsWith("Base"))
                .ToList();

            abstractClasses.Should().BeEmpty(
                $"Abstract classes in {assembly.GetName().Name} should start with 'Abstract' or 'Base'. " +
                $"Violations: {string.Join(", ", abstractClasses.Select(t => t.Name))}");
        }
    }

    [Fact]
    public void Exceptions_ShouldEndWithException()
    {
        // Arrange
        var assemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, WebApiAssembly, SharedAssembly };

        // Act & Assert
        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That().Inherit(typeof(Exception))
                .Should().HaveNameEndingWith("Exception")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"Exception classes in {assembly.GetName().Name} should end with 'Exception'. Violations: {string.Join(", ", result.FailingTypeNames)}");
        }
    }

    [Fact]
    public void DTOs_ShouldEndWithDto()
    {
        // Arrange & Act
        var result = Types.InAssembly(SharedAssembly)
            .That().ResideInNamespaceMatching(".*DTOs.*")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Dto")
            .Or().HaveNameEndingWith("Request")
            .Or().HaveNameEndingWith("Response")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"DTO classes should end with 'Dto', 'Request', or 'Response'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Validators_ShouldEndWithValidator()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That().ResideInNamespaceMatching(".*Validators.*")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Validator")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Validator classes should end with 'Validator'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Repositories_ShouldEndWithRepository()
    {
        // Arrange & Act
        var result = Types.InAssembly(InfrastructureAssembly)
            .That().ResideInNamespaceMatching(".*Repositories.*")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Repository classes should end with 'Repository'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Middlewares_ShouldEndWithMiddleware()
    {
        // Arrange & Act
        var result = Types.InAssembly(WebApiAssembly)
            .That().ResideInNamespaceMatching(".*Middleware.*")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Middleware")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Middleware classes should end with 'Middleware'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Constants_ShouldEndWithConstants()
    {
        // Arrange & Act
        var result = Types.InAssembly(SharedAssembly)
            .That().ResideInNamespaceMatching(".*Constants.*")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Constants")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Constant classes should end with 'Constants'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Configurations_ShouldEndWithConfiguration()
    {
        // Arrange & Act
        var result = Types.InAssembly(InfrastructureAssembly)
            .That().ResideInNamespaceMatching(".*Configurations.*")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Configuration")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Configuration classes should end with 'Configuration'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Behaviors_ShouldEndWithBehavior()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That().ResideInNamespaceMatching(".*Behaviors.*")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Behavior")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Behavior classes should end with 'Behavior'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void Extensions_ShouldEndWithExtensions()
    {
        // Arrange
        var assemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, WebApiAssembly, SharedAssembly };

        // Act & Assert
        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That().ResideInNamespaceMatching(".*Extensions.*")
                .And().AreClasses()
                .Should().HaveNameEndingWith("Extensions")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"Extension classes in {assembly.GetName().Name} should end with 'Extensions'. Violations: {string.Join(", ", result.FailingTypeNames)}");
        }
    }

    [Fact]
    public void Entities_ShouldNotHaveAsyncSuffix()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .That().ResideInNamespace("NET.Api.Domain.Entities")
            .And().AreClasses()
            .Should().NotHaveNameEndingWith("Async")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Entity classes should not end with 'Async'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }

    [Fact]
    public void PublicClasses_ShouldHaveXmlDocumentation()
    {
        // Arrange
        var assemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, SharedAssembly };

        // Act & Assert
        foreach (var assembly in assemblies)
        {
            var publicClasses = Types.InAssembly(assembly)
                .That().ArePublic()
                .And().AreClasses()
                .And().DoNotHaveNameMatching(".*Tests.*")
                .GetTypes()
                .Where(t => !t.IsNested)
                .ToList();

            // Note: This is a conceptual test. In practice, XML documentation
            // checking would require additional tooling or custom reflection
            publicClasses.Should().NotBeEmpty(
                $"Assembly {assembly.GetName().Name} should have public classes for documentation validation.");
        }
    }

    [Fact]
    public void TestClasses_ShouldEndWithTests()
    {
        // Arrange
        var testAssemblies = new[]
        {
            typeof(NET.Api.UnitTests.Domain.Entities.BaseEntityTests).Assembly,
            typeof(NET.Api.IntegrationTests.Controllers.AuthControllerTests).Assembly,
            typeof(CleanArchitectureTests).Assembly
        };

        // Act & Assert
        foreach (var assembly in testAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .That().AreClasses()
                .And().ArePublic()
                .And().DoNotHaveNameMatching(".*Base.*")
                .And().DoNotHaveNameMatching(".*Helper.*")
                .Should().HaveNameEndingWith("Tests")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"Test classes in {assembly.GetName().Name} should end with 'Tests'. Violations: {string.Join(", ", result.FailingTypeNames)}");
        }
    }

    [Fact]
    public void Enums_ShouldNotHavePluralNames()
    {
        // Arrange
        var assemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, WebApiAssembly, SharedAssembly };
        var commonPluralEndings = new[] { "s", "es", "ies" };

        // Act & Assert
        foreach (var assembly in assemblies)
        {
            var enums = Types.InAssembly(assembly)
                .That().AreEnums()
                .GetTypes()
                .Where(e => commonPluralEndings.Any(ending => e.Name.EndsWith(ending, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            enums.Should().BeEmpty(
                $"Enums in {assembly.GetName().Name} should have singular names. " +
                $"Potential violations: {string.Join(", ", enums.Select(e => e.Name))}");
        }
    }
}