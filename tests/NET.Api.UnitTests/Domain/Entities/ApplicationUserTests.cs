using NET.Api.Domain.Entities;
using AutoFixture.Xunit2;

namespace NET.Api.UnitTests.Domain.Entities;

public class ApplicationUserTests
{
    [Theory]
    [AutoData]
    public void Constructor_ShouldInitializeWithDefaults(string firstName, string lastName, string identityDocument)
    {
        // Arrange & Act
        var user = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            IdentityDocument = identityDocument
        };
        
        // Assert
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.IdentityDocument.Should().Be(identityDocument);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeNull();
        user.IsActive.Should().BeTrue();
    }

    [Theory]
    [AutoData]
    public void FullName_ShouldConcatenateFirstAndLastName(string firstName, string lastName)
    {
        // Arrange
        var user = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName
        };
        
        // Act
        var fullName = user.FullName;
        
        // Assert
        fullName.Should().Be($"{firstName} {lastName}");
    }

    [Fact]
    public void FullName_WithEmptyNames_ShouldReturnSpaceOnly()
    {
        // Arrange
        var user = new ApplicationUser
        {
            FirstName = string.Empty,
            LastName = string.Empty
        };
        
        // Act
        var fullName = user.FullName;
        
        // Assert
        fullName.Should().Be(" ");
    }

    [Fact]
    public void SetUpdatedAt_ShouldUpdateTimestamp()
    {
        // Arrange
        var user = new ApplicationUser();
        var originalUpdatedAt = user.UpdatedAt;
        
        // Act
        user.SetUpdatedAt();
        
        // Assert
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }

    [Fact]
    public void SetUpdatedAt_CalledMultipleTimes_ShouldUpdateEachTime()
    {
        // Arrange
        var user = new ApplicationUser();
        
        // Act
        user.SetUpdatedAt();
        var firstUpdate = user.UpdatedAt;
        
        Thread.Sleep(10); // Ensure different timestamps
        
        user.SetUpdatedAt();
        var secondUpdate = user.UpdatedAt;
        
        // Assert
        firstUpdate.Should().NotBeNull();
        secondUpdate.Should().NotBeNull();
        secondUpdate.Should().BeAfter(firstUpdate!.Value);
    }

    [Theory]
    [InlineData("john.doe@example.com", "john.doe@example.com")]
    [InlineData("ADMIN@COMPANY.COM", "ADMIN@COMPANY.COM")]
    public void Email_ShouldAcceptValidEmailFormats(string email, string expected)
    {
        // Arrange
        var user = new ApplicationUser();
        
        // Act
        user.Email = email;
        
        // Assert
        user.Email.Should().Be(expected);
    }

    [Theory]
    [AutoData]
    public void IsActive_ShouldBeConfigurable(bool isActive)
    {
        // Arrange
        var user = new ApplicationUser();
        
        // Act
        user.IsActive = isActive;
        
        // Assert
        user.IsActive.Should().Be(isActive);
    }
}