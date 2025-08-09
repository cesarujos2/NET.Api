namespace NET.Api.Application.Common.Models.UserAccount;

/// <summary>
/// DTO for representing user account data
/// </summary>
public class UserAccountDataDto
{
    public string Id { get; set; } = string.Empty;
    public string UserAccountId { get; set; } = string.Empty;
    public string DataKey { get; set; } = string.Empty;
    public string DataValue { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public string? Category { get; set; }
    public string? Description { get; set; }
    public bool IsSensitive { get; set; }
    public bool IsRequired { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}