using NET.Api.Application.Common.Models.Authentication;
using NET.Api.Application.Common.Models.UserAccount;

namespace NET.Api.Application.Abstractions.Services;

/// <summary>
/// Service interface for managing user accounts (multi-account functionality)
/// </summary>
public interface IUserAccountService : IApplicationService
{
    /// <summary>
    /// Creates a new user account for the specified user
    /// </summary>
    /// <param name="userId">The ApplicationUser ID</param>
    /// <param name="request">Account creation request</param>
    /// <returns>Created user account</returns>
    Task<UserAccountDto> CreateUserAccountAsync(string userId, CreateUserAccountRequestDto request);
    
    /// <summary>
    /// Gets all user accounts for the specified user
    /// </summary>
    /// <param name="userId">The ApplicationUser ID</param>
    /// <returns>List of user accounts</returns>
    Task<List<UserAccountDto>> GetUserAccountsAsync(string userId);
    
    /// <summary>
    /// Gets all active user accounts for selection during login
    /// </summary>
    /// <param name="userId">The ApplicationUser ID</param>
    /// <returns>List of accounts available for selection</returns>
    Task<List<AccountSelectionDto>> GetAccountsForSelectionAsync(string userId);
    
    /// <summary>
    /// Gets a specific user account by ID
    /// </summary>
    /// <param name="accountId">The UserAccount ID</param>
    /// <returns>User account or null if not found</returns>
    Task<UserAccountDto?> GetUserAccountByIdAsync(string accountId);
    
    /// <summary>
    /// Updates an existing user account
    /// </summary>
    /// <param name="accountId">The UserAccount ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated user account</returns>
    Task<UserAccountDto> UpdateUserAccountAsync(string accountId, CreateUserAccountRequestDto request);
    
    /// <summary>
    /// Sets a user account as the default for the user
    /// </summary>
    /// <param name="userId">The ApplicationUser ID</param>
    /// <param name="accountId">The UserAccount ID to set as default</param>
    /// <returns>Updated user account</returns>
    Task<UserAccountDto> SetDefaultAccountAsync(string userId, string accountId);
    
    /// <summary>
    /// Deactivates a user account
    /// </summary>
    /// <param name="accountId">The UserAccount ID</param>
    /// <returns>True if successful</returns>
    Task<bool> DeactivateUserAccountAsync(string accountId);
    
    /// <summary>
    /// Updates the last accessed timestamp for an account
    /// </summary>
    /// <param name="accountId">The UserAccount ID</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateLastAccessedAsync(string accountId);
    
    /// <summary>
    /// Creates a default account for a new user during registration
    /// </summary>
    /// <param name="userId">The ApplicationUser ID</param>
    /// <param name="accountName">Default account name (usually user's full name)</param>
    /// <returns>Created default account</returns>
    Task<UserAccountDto> CreateDefaultAccountAsync(string userId, string accountName);
    
    /// <summary>
    /// Validates if a user can create more accounts (business rules)
    /// </summary>
    /// <param name="userId">The ApplicationUser ID</param>
    /// <returns>True if user can create more accounts</returns>
    Task<bool> CanCreateMoreAccountsAsync(string userId);
}