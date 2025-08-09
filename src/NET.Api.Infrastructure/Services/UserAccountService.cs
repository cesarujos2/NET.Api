using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Common.Models.UserAccount;
using NET.Api.Domain.Entities;
using NET.Api.Infrastructure.Persistence;

namespace NET.Api.Infrastructure.Services;

public class UserAccountService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IMapper mapper,
    ILogger<UserAccountService> logger) : IUserAccountService
{
    private const int MaxAccountsPerUser = 10; // Business rule: max 10 accounts per user

    public async Task<UserAccountDto> CreateUserAccountAsync(string userId, CreateUserAccountRequestDto request)
    {
        try
        {
            // Validate user exists
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("Usuario no encontrado.");
            }

            // Check if user can create more accounts
            if (!await CanCreateMoreAccountsAsync(userId))
            {
                throw new InvalidOperationException($"No puedes crear más de {MaxAccountsPerUser} cuentas.");
            }

            // Check if account name already exists for this user
            var existingAccount = await context.UserAccounts
                .FirstOrDefaultAsync(ua => ua.ApplicationUserId == userId && 
                                          ua.AccountName == request.AccountName && 
                                          ua.IsActive);
            
            if (existingAccount != null)
            {
                throw new InvalidOperationException("Ya existe una cuenta con este nombre.");
            }

            // Create new account
            var userAccount = mapper.Map<UserAccount>(request);
            userAccount.ApplicationUserId = userId;
            // CreatedBy is set automatically by the base entity

            // If this is the first account or explicitly set as default, make it default
            var userAccountsCount = await context.UserAccounts
                .CountAsync(ua => ua.ApplicationUserId == userId && ua.IsActive);
            
            if (userAccountsCount == 0 || request.IsDefault)
            {
                // Remove default from other accounts if setting this as default
                if (request.IsDefault)
                {
                    await RemoveDefaultFromOtherAccountsAsync(userId);
                }
                userAccount.SetAsDefault();
            }

            context.UserAccounts.Add(userAccount);
            await context.SaveChangesAsync();

            logger.LogInformation("User account created successfully for user {UserId} with account name {AccountName}", 
                userId, request.AccountName);

            return mapper.Map<UserAccountDto>(userAccount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user account for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<UserAccountDto>> GetUserAccountsAsync(string userId)
    {
        try
        {
            var userAccounts = await context.UserAccounts
                .Include(ua => ua.AccountData)
                .Where(ua => ua.ApplicationUserId == userId && ua.IsActive)
                .OrderBy(ua => ua.DisplayOrder)
                .ThenBy(ua => ua.AccountName)
                .ToListAsync();

            return mapper.Map<List<UserAccountDto>>(userAccounts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user accounts for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<AccountSelectionDto>> GetAccountsForSelectionAsync(string userId)
    {
        try
        {
            var userAccounts = await context.UserAccounts
                .Where(ua => ua.ApplicationUserId == userId && ua.IsActive)
                .OrderByDescending(ua => ua.IsDefault)
                .ThenBy(ua => ua.DisplayOrder)
                .ThenBy(ua => ua.AccountName)
                .ToListAsync();

            return mapper.Map<List<AccountSelectionDto>>(userAccounts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving accounts for selection for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserAccountDto?> GetUserAccountByIdAsync(string accountId)
    {
        try
        {
            if (!Guid.TryParse(accountId, out var accountGuid))
            {
                return null;
            }

            var userAccount = await context.UserAccounts
                .Include(ua => ua.AccountData)
                .FirstOrDefaultAsync(ua => ua.Id == accountGuid && ua.IsActive);

            return userAccount != null ? mapper.Map<UserAccountDto>(userAccount) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<UserAccountDto> UpdateUserAccountAsync(string accountId, CreateUserAccountRequestDto request)
    {
        try
        {
            if (!Guid.TryParse(accountId, out var accountGuid))
            {
                throw new InvalidOperationException("ID de cuenta inválido.");
            }

            var userAccount = await context.UserAccounts
                .FirstOrDefaultAsync(ua => ua.Id == accountGuid && ua.IsActive);

            if (userAccount == null)
            {
                throw new InvalidOperationException("Cuenta no encontrada.");
            }

            // Check if account name already exists for this user (excluding current account)
            var existingAccount = await context.UserAccounts
                .FirstOrDefaultAsync(ua => ua.ApplicationUserId == userAccount.ApplicationUserId && 
                                          ua.AccountName == request.AccountName && 
                                          ua.IsActive && 
                                          ua.Id != accountGuid);
            
            if (existingAccount != null)
            {
                throw new InvalidOperationException("Ya existe una cuenta con este nombre.");
            }

            // Update account properties
            userAccount.AccountName = request.AccountName;
            userAccount.Description = request.Description;
            userAccount.ProfilePictureUrl = request.ProfilePictureUrl;
            userAccount.DisplayOrder = request.DisplayOrder;
            userAccount.Settings = request.Settings;
            // SetUpdatedAt is called internally by entity methods

            // Handle default status
            if (request.IsDefault && !userAccount.IsDefault)
            {
                await RemoveDefaultFromOtherAccountsAsync(userAccount.ApplicationUserId);
                userAccount.SetAsDefault();
            }
            else if (!request.IsDefault && userAccount.IsDefault)
            {
                // Don't allow removing default if it's the only account
                var accountsCount = await context.UserAccounts
                    .CountAsync(ua => ua.ApplicationUserId == userAccount.ApplicationUserId && ua.IsActive);
                
                if (accountsCount > 1)
                {
                    userAccount.RemoveDefaultStatus();
                }
            }

            await context.SaveChangesAsync();

            logger.LogInformation("User account {AccountId} updated successfully", accountId);

            return mapper.Map<UserAccountDto>(userAccount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<UserAccountDto> SetDefaultAccountAsync(string userId, string accountId)
    {
        try
        {
            if (!Guid.TryParse(accountId, out var accountGuid))
            {
                throw new InvalidOperationException("ID de cuenta inválido.");
            }

            var userAccount = await context.UserAccounts
                .FirstOrDefaultAsync(ua => ua.Id == accountGuid && 
                                          ua.ApplicationUserId == userId && 
                                          ua.IsActive);

            if (userAccount == null)
            {
                throw new InvalidOperationException("Cuenta no encontrada.");
            }

            // Remove default from other accounts
            await RemoveDefaultFromOtherAccountsAsync(userId);

            // Set this account as default
            userAccount.SetAsDefault();
            await context.SaveChangesAsync();

            logger.LogInformation("Account {AccountId} set as default for user {UserId}", accountId, userId);

            return mapper.Map<UserAccountDto>(userAccount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting default account {AccountId} for user {UserId}", accountId, userId);
            throw;
        }
    }

    public async Task<bool> DeactivateUserAccountAsync(string accountId)
    {
        try
        {
            if (!Guid.TryParse(accountId, out var accountGuid))
            {
                return false;
            }

            var userAccount = await context.UserAccounts
                .FirstOrDefaultAsync(ua => ua.Id == accountGuid && ua.IsActive);

            if (userAccount == null)
            {
                return false;
            }

            // Don't allow deactivating the only account
            var activeAccountsCount = await context.UserAccounts
                .CountAsync(ua => ua.ApplicationUserId == userAccount.ApplicationUserId && ua.IsActive);
            
            if (activeAccountsCount <= 1)
            {
                throw new InvalidOperationException("No puedes desactivar la única cuenta activa.");
            }

            userAccount.IsActive = false;
            // SetUpdatedAt is called internally by entity methods

            // If this was the default account, set another one as default
            if (userAccount.IsDefault)
            {
                var nextAccount = await context.UserAccounts
                    .Where(ua => ua.ApplicationUserId == userAccount.ApplicationUserId && 
                                ua.IsActive && 
                                ua.Id != accountGuid)
                    .OrderBy(ua => ua.DisplayOrder)
                    .ThenBy(ua => ua.AccountName)
                    .FirstOrDefaultAsync();

                if (nextAccount != null)
                {
                    nextAccount.SetAsDefault();
                }
            }

            await context.SaveChangesAsync();

            logger.LogInformation("User account {AccountId} deactivated successfully", accountId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating user account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<bool> UpdateLastAccessedAsync(string accountId)
    {
        try
        {
            if (!Guid.TryParse(accountId, out var accountGuid))
            {
                return false;
            }

            var userAccount = await context.UserAccounts
                .FirstOrDefaultAsync(ua => ua.Id == accountGuid && ua.IsActive);

            if (userAccount == null)
            {
                return false;
            }

            userAccount.UpdateLastAccessed();
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating last accessed for account {AccountId}", accountId);
            return false;
        }
    }

    public async Task<UserAccountDto> CreateDefaultAccountAsync(string userId, string accountName)
    {
        try
        {
            var request = new CreateUserAccountRequestDto
            {
                AccountName = accountName,
                Description = "Cuenta principal",
                IsDefault = true,
                DisplayOrder = 0
            };

            return await CreateUserAccountAsync(userId, request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating default account for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> CanCreateMoreAccountsAsync(string userId)
    {
        try
        {
            var currentAccountsCount = await context.UserAccounts
                .CountAsync(ua => ua.ApplicationUserId == userId && ua.IsActive);

            return currentAccountsCount < MaxAccountsPerUser;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if user {UserId} can create more accounts", userId);
            return false;
        }
    }

    private async Task RemoveDefaultFromOtherAccountsAsync(string userId)
    {
        var defaultAccounts = await context.UserAccounts
            .Where(ua => ua.ApplicationUserId == userId && ua.IsDefault && ua.IsActive)
            .ToListAsync();

        foreach (var account in defaultAccounts)
        {
            account.RemoveDefaultStatus();
        }
    }
}