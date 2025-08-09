using System.ComponentModel.DataAnnotations;

namespace NET.Api.Domain.Entities;

/// <summary>
/// Represents a specific user account that belongs to an email address.
/// Multiple UserAccounts can be associated with the same email through ApplicationUser.
/// </summary>
public class UserAccount : BaseEntity
{
    /// <summary>
    /// Reference to the main ApplicationUser (email owner)
    /// </summary>
    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property to the main ApplicationUser
    /// </summary>
    public ApplicationUser ApplicationUser { get; set; } = null!;
    
    /// <summary>
    /// Display name for this specific account
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string AccountName { get; set; } = string.Empty;
    
    /// <summary>
    /// Description or purpose of this account
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Profile picture URL or path for this account
    /// </summary>
    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }
    
    /// <summary>
    /// Indicates if this account is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Indicates if this is the default account for the user
    /// </summary>
    public bool IsDefault { get; set; } = false;
    
    /// <summary>
    /// Last time this account was accessed
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }
    
    /// <summary>
    /// Account-specific settings stored as JSON
    /// </summary>
    public string? Settings { get; set; }
    
    /// <summary>
    /// Display order for account selection
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Navigation property for account-specific data
    /// </summary>
    public ICollection<UserAccountData> AccountData { get; set; } = new List<UserAccountData>();
    
    /// <summary>
    /// Updates the last accessed timestamp
    /// </summary>
    public void UpdateLastAccessed()
    {
        LastAccessedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
    
    /// <summary>
    /// Sets this account as default and ensures only one default per user
    /// </summary>
    public void SetAsDefault()
    {
        IsDefault = true;
        SetUpdatedAt();
    }
    
    /// <summary>
    /// Removes default status from this account
    /// </summary>
    public void RemoveDefaultStatus()
    {
        IsDefault = false;
        SetUpdatedAt();
    }
}