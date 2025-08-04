using NET.Api.Domain.Entities;
using AutoFixture.Xunit2;

namespace NET.Api.UnitTests.Domain.Entities;

public class BaseEntityTests
{
    private class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        
        public void UpdateName(string name, string? updatedBy = null)
        {
            Name = name;
            SetUpdatedAt(updatedBy);
        }
        
        public void SetCreator(string createdBy)
        {
            SetCreatedBy(createdBy);
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeWithValidDefaults()
    {
        // Arrange & Act
        var entity = new TestEntity();
        
        // Assert
        entity.Id.Should().NotBeEmpty();
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.UpdatedAt.Should().BeNull();
        entity.CreatedBy.Should().BeNull();
        entity.UpdatedBy.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void SetUpdatedAt_ShouldUpdateTimestampAndUser(string updatedBy)
    {
        // Arrange
        var entity = new TestEntity();
        var originalCreatedAt = entity.CreatedAt;
        
        // Act
        entity.UpdateName("Test", updatedBy);
        
        // Assert
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.UpdatedBy.Should().Be(updatedBy);
        entity.CreatedAt.Should().Be(originalCreatedAt); // Should not change
    }

    [Fact]
    public void SetUpdatedAt_WithoutUser_ShouldUpdateOnlyTimestamp()
    {
        // Arrange
        var entity = new TestEntity();
        
        // Act
        entity.UpdateName("Test");
        
        // Assert
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.UpdatedBy.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void SetCreatedBy_ShouldSetCreatedByUser(string createdBy)
    {
        // Arrange
        var entity = new TestEntity();
        
        // Act
        entity.SetCreator(createdBy);
        
        // Assert
        entity.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void MultipleUpdates_ShouldUpdateTimestampEachTime()
    {
        // Arrange
        var entity = new TestEntity();
        
        // Act
        entity.UpdateName("First Update", "User1");
        var firstUpdate = entity.UpdatedAt;
        
        Thread.Sleep(10); // Ensure different timestamps
        
        entity.UpdateName("Second Update", "User2");
        var secondUpdate = entity.UpdatedAt;
        
        // Assert
        firstUpdate.Should().NotBeNull();
        secondUpdate.Should().NotBeNull();
        secondUpdate.Should().BeAfter(firstUpdate!.Value);
        entity.UpdatedBy.Should().Be("User2");
    }
}