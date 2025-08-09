using NET.Api.Application.Common.Models.UserAccount;

namespace NET.Api.Application.Common.Models.Authentication;

/// <summary>
/// Response DTO for login that includes account selection when multiple accounts exist
/// </summary>
public class LoginWithAccountSelectionResponseDto
{
    /// <summary>
    /// Indicates if account selection is required
    /// </summary>
    public bool RequiresAccountSelection { get; set; }
    
    /// <summary>
    /// Available accounts for selection (only populated when RequiresAccountSelection is true)
    /// </summary>
    public List<AccountSelectionDto> AvailableAccounts { get; set; } = new();
    
    /// <summary>
    /// Temporary token for account selection (valid for a short time)
    /// </summary>
    public string? SelectionToken { get; set; }
    
    /// <summary>
    /// Authentication response (only populated when account is selected or user has single account)
    /// </summary>
    public AuthResponseDto? AuthResponse { get; set; }
    
    /// <summary>
    /// Selected account information (only when authentication is complete)
    /// </summary>
    public UserAccountDto? SelectedAccount { get; set; }
}