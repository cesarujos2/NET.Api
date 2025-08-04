using NET.Api.Shared.Constants;

namespace NET.Api.UnitTests.Shared.Constants;

public class RoleConstantsTests
{
    [Fact]
    public void RoleNames_ShouldHaveExpectedValues()
    {
        // Assert
        RoleConstants.Names.Owner.Should().Be("Owner");
        RoleConstants.Names.Admin.Should().Be("Admin");
        RoleConstants.Names.User.Should().Be("User");
        RoleConstants.Names.Moderator.Should().Be("Moderator");
        RoleConstants.Names.Support.Should().Be("Support");
    }

    [Fact]
    public void RoleDescriptions_ShouldNotBeEmpty()
    {
        // Assert
        RoleConstants.Descriptions.Owner.Should().NotBeNullOrEmpty();
        RoleConstants.Descriptions.Admin.Should().NotBeNullOrEmpty();
        RoleConstants.Descriptions.User.Should().NotBeNullOrEmpty();
        RoleConstants.Descriptions.Moderator.Should().NotBeNullOrEmpty();
        RoleConstants.Descriptions.Support.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RoleHierarchy_ShouldHaveCorrectOrder()
    {
        // Assert - Higher values = more authority
        RoleConstants.Hierarchy.Owner.Should().BeGreaterThan(RoleConstants.Hierarchy.Admin);
        RoleConstants.Hierarchy.Admin.Should().BeGreaterThan(RoleConstants.Hierarchy.Moderator);
        RoleConstants.Hierarchy.Moderator.Should().BeGreaterThan(RoleConstants.Hierarchy.Support);
        RoleConstants.Hierarchy.Support.Should().BeGreaterThan(RoleConstants.Hierarchy.User);
    }

    [Fact]
    public void AllRoles_ShouldContainAllDefinedRoles()
    {
        // Arrange
        var expectedRoles = new[]
        {
            RoleConstants.Names.Owner,
            RoleConstants.Names.Admin,
            RoleConstants.Names.Moderator,
            RoleConstants.Names.Support,
            RoleConstants.Names.User
        };
        
        // Assert
        RoleConstants.AllRoles.Should().Contain(expectedRoles);
        RoleConstants.AllRoles.Should().HaveCount(expectedRoles.Length);
    }

    [Fact]
    public void ElevatedRoles_ShouldContainCriticalRoles()
    {
        // Arrange
        var expectedElevatedRoles = new[]
        {
            RoleConstants.Names.Owner,
            RoleConstants.Names.Admin,
            RoleConstants.Names.Moderator
        };
        
        // Assert
        RoleConstants.ElevatedRoles.Should().Contain(expectedElevatedRoles);
    }

    [Fact]
    public void AdminRoles_ShouldContainAdministrativeRoles()
    {
        // Arrange
        var expectedAdminRoles = new[]
        {
            RoleConstants.Names.Owner,
            RoleConstants.Names.Admin
        };
        
        // Assert
        RoleConstants.AdminRoles.Should().Contain(expectedAdminRoles);
    }

    [Theory]
    [InlineData("Owner", true)]
    [InlineData("Admin", true)]
    [InlineData("User", true)]
    [InlineData("InvalidRole", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void IsValidRole_ShouldValidateRoleNames(string roleName, bool expected)
    {
        // Act
        var result = RoleConstants.IsValidRole(roleName);
        
        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Owner", 100)]
    [InlineData("Admin", 80)]
    [InlineData("Moderator", 60)]
    [InlineData("Support", 40)]
    [InlineData("User", 20)]
    [InlineData("InvalidRole", 0)]
    public void GetRoleHierarchy_ShouldReturnCorrectLevels(string roleName, int expected)
    {
        // Act
        var result = RoleConstants.GetRoleHierarchy(roleName);
        
        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Owner", "Admin", true)]
    [InlineData("Admin", "User", true)]
    [InlineData("User", "Admin", false)]
    [InlineData("Admin", "Owner", false)]
    [InlineData("Owner", "Owner", true)] // Same level
    public void HasSufficientAuthority_ShouldCompareHierarchyLevels(string userRole, string targetRole, bool expected)
    {
        // Act
        var result = RoleConstants.HasSufficientAuthority(userRole, targetRole);
        
        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Limits_ShouldHaveValidValues()
    {
        // Assert
        RoleConstants.Limits.MaxOwners.Should().BeGreaterThan(0);
        RoleConstants.Limits.MaxRolesPerUser.Should().BeGreaterThan(0);
        RoleConstants.Limits.MinRoleNameLength.Should().BeGreaterThan(0);
        RoleConstants.Limits.MaxRoleNameLength.Should().BeGreaterThan(RoleConstants.Limits.MinRoleNameLength);
    }

    [Fact]
    public void Limits_MaxOwners_ShouldBeReasonable()
    {
        // Assert - Should be a reasonable number for security
        RoleConstants.Limits.MaxOwners.Should().BeInRange(1, 10);
    }
}