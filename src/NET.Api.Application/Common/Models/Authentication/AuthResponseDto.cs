using NET.Api.Application.Common.Models.UserAccount;

namespace NET.Api.Application.Common.Models.Authentication;

public class AuthResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool RequiresEmailConfirmation { get; set; } = false;
    
    /// <summary>
    /// Information about the selected user account
    /// </summary>
    public UserAccountDto? SelectedAccount { get; set; }
    
    /// <summary>
    /// Indicates if the user has multiple accounts available
    /// </summary>
    public bool HasMultipleAccounts { get; set; } = false;
}