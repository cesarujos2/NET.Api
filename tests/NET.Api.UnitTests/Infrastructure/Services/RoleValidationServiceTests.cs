using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NET.Api.Domain.Entities;
using NET.Api.Infrastructure.Services.RoleService;
using NET.Api.Shared.Constants;
using AutoFixture.Xunit2;
using System.Linq.Expressions;

namespace NET.Api.UnitTests.Infrastructure.Services;

public class RoleValidationServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly RoleValidationService _service;

    public RoleValidationServiceTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _roleManagerMock = CreateRoleManagerMock();
        
        _service = new RoleValidationService(
            _roleManagerMock.Object,
            _userManagerMock.Object);
    }

    [Theory]
    [InlineData("ValidRole", true)]
    [InlineData("a", false)] // Too short
    [InlineData("", false)] // Empty
    [InlineData("ThisRoleNameIsWayTooLongAndExceedsTheMaximumAllowedLengthForRoleNames", false)] // Too long
    public async Task IsValidRoleNameAsync_ShouldValidateRoleNameLength(string roleName, bool expected)
    {
        // Act
        var result = await _service.IsValidRoleNameAsync(roleName);
        
        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CanCreateRoleAsync_WithValidRole_ShouldReturnTrue()
    {
        // Arrange
        var role = new ApplicationRole("TestRole");
        _roleManagerMock.Setup(x => x.FindByNameAsync(role.Name!))
            .ReturnsAsync((ApplicationRole?)null);
        
        // Act
        var result = await _service.CanCreateRoleAsync(role);
        
        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanCreateRoleAsync_WithExistingRole_ShouldReturnFalse()
    {
        // Arrange
        var role = new ApplicationRole("ExistingRole");
        var existingRole = new ApplicationRole("ExistingRole");
        
        _roleManagerMock.Setup(x => x.FindByNameAsync(role.Name!))
            .ReturnsAsync(existingRole);
        
        // Act
        var result = await _service.CanCreateRoleAsync(role);
        
        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(RoleConstants.Names.Owner, false)] // System role
    [InlineData(RoleConstants.Names.Admin, false)] // System role
    [InlineData("CustomRole", true)] // Non-system role
    public async Task CanDeleteRoleAsync_ShouldValidateSystemRoles(string roleName, bool expected)
    {
        // Arrange
        var role = new ApplicationRole 
        { 
            Name = roleName,
            IsSystemRole = RoleConstants.IsStaticRole(roleName)
        };
        var users = new List<ApplicationUser>();
        
        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(roleName))
            .ReturnsAsync(users);
        
        // Act
        var result = await _service.CanDeleteRoleAsync(role);
        
        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [AutoData]
    public async Task CanDeleteRoleAsync_WithAssignedUsers_ShouldReturnFalse(string roleName)
    {
        // Arrange
        var role = new ApplicationRole { Name = roleName };
        var users = new List<ApplicationUser> { new ApplicationUser() };
        
        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(roleName))
            .ReturnsAsync(users);
        
        // Act
        var result = await _service.CanDeleteRoleAsync(role);
        
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAssignOwnerRoleAsync_WithinLimit_ShouldReturnTrue()
    {
        // Arrange
        var currentOwners = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user1" },
            new ApplicationUser { Id = "user2" }
        };
        
        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(RoleConstants.Names.Owner))
            .ReturnsAsync(currentOwners);
        
        // Act
        var result = await _service.CanAssignOwnerRoleAsync();
        
        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAssignOwnerRoleAsync_AtLimit_ShouldReturnFalse()
    {
        // Arrange
        var currentOwners = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user1" },
            new ApplicationUser { Id = "user2" },
            new ApplicationUser { Id = "user3" }
        };
        
        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(RoleConstants.Names.Owner))
            .ReturnsAsync(currentOwners);
        
        // Act
        var result = await _service.CanAssignOwnerRoleAsync();
        
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAssignOwnerRoleAsync_WithExcludedUser_ShouldReturnTrue()
    {
        // Arrange
        var excludedUserId = "user2";
        var currentOwners = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user1" },
            new ApplicationUser { Id = excludedUserId },
            new ApplicationUser { Id = "user3" }
        };
        
        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(RoleConstants.Names.Owner))
            .ReturnsAsync(currentOwners);
        
        // Act
        var result = await _service.CanAssignOwnerRoleAsync(excludedUserId);
        
        // Assert
        result.Should().BeTrue();
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
    }

    private static Mock<RoleManager<ApplicationRole>> CreateRoleManagerMock()
    {
        var store = new Mock<IRoleStore<ApplicationRole>>();
        return new Mock<RoleManager<ApplicationRole>>(store.Object, null, null, null, null);
    }
}