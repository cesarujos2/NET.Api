namespace NET.Api.Application.Common.Models.UserAccount;

/// <summary>
/// DTO for account selection during login
/// </summary>
public class AccountSelectionDto
{
    public string Id { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsDefault { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public int DisplayOrder { get; set; }
}