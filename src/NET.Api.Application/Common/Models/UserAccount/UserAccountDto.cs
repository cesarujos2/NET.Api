namespace NET.Api.Application.Common.Models.UserAccount;

/// <summary>
/// DTO for representing a user account
/// </summary>
public class UserAccountDto
{
    public string Id { get; set; } = string.Empty;
    public string ApplicationUserId { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public string? Settings { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<UserAccountDataDto> AccountData { get; set; } = new();
}