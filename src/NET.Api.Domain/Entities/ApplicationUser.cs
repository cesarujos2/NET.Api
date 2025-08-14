using Microsoft.AspNetCore.Identity;

namespace NET.Api.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    //Basic Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Compeleted Information
    public bool IsProfileComplete { get; set; } = false;
    public string? Address { get; set; }
    public string? IdentityDocument { get; set; }
    public DateTime? DateOfBirth { get; set; }

    // Computed Property
    public string FullName => $"{FirstName} {LastName}";
    

    public void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateProfileCompletionStatus()
    {
        IsProfileComplete = !string.IsNullOrWhiteSpace(FirstName) &&
                           !string.IsNullOrWhiteSpace(LastName) &&
                           !string.IsNullOrWhiteSpace(IdentityDocument) &&
                           DateOfBirth.HasValue &&
                           !string.IsNullOrWhiteSpace(Address);
    }
}