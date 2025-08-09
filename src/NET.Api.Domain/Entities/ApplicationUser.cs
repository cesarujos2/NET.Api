using Microsoft.AspNetCore.Identity;

namespace NET.Api.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Collection of user accounts associated with this email
    /// </summary>
    public ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();
    
    public string FullName => $"{FirstName} {LastName}";
    
    public void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Gets the default user account for this user
    /// </summary>
    /// <returns>Default UserAccount or null if none exists</returns>
    public UserAccount? GetDefaultAccount()
    {
        return UserAccounts.FirstOrDefault(ua => ua.IsDefault && ua.IsActive);
    }
    
    /// <summary>
    /// Gets all active user accounts for this user
    /// </summary>
    /// <returns>Collection of active UserAccounts</returns>
    public IEnumerable<UserAccount> GetActiveAccounts()
    {
        return UserAccounts.Where(ua => ua.IsActive).OrderBy(ua => ua.DisplayOrder).ThenBy(ua => ua.AccountName);
    }
    
    /// <summary>
    /// Checks if this user has multiple accounts
    /// </summary>
    /// <returns>True if user has more than one active account</returns>
    public bool HasMultipleAccounts()
    {
        return UserAccounts.Count(ua => ua.IsActive) > 1;
    }
}