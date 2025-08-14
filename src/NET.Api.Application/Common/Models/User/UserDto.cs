namespace NET.Api.Application.Common.Models.User;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string PhoneNumber { get; set; } = string.Empty;
    public string IdentityDocument { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public bool IsProfileComplete { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
}