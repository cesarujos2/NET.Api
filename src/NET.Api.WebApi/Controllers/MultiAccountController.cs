using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Common.Models.UserAccount;
using NET.Api.Shared.Extensions;
using System.Security.Claims;

namespace NET.Api.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MultiAccountController(
    IUserAccountService userAccountService,
    ILogger<MultiAccountController> logger) : ControllerBase
{
    /// <summary>
    /// Get all user accounts for the authenticated user
    /// </summary>
    /// <returns>List of user accounts</returns>
    [HttpGet]
    public async Task<ActionResult<List<UserAccountDto>>> GetUserAccounts()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Usuario no autenticado." });
            }

            var accounts = await userAccountService.GetUserAccountsAsync(userId);
            return Ok(new { success = true, message = "Cuentas obtenidas exitosamente.", data = accounts });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user accounts");
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Get accounts for selection (used during login)
    /// </summary>
    /// <returns>List of accounts for selection</returns>
    [HttpGet("selection")]
    public async Task<ActionResult<List<AccountSelectionDto>>> GetAccountsForSelection()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Usuario no autenticado." });
            }

            var accounts = await userAccountService.GetAccountsForSelectionAsync(userId);
            return Ok(new { success = true, message = "Cuentas para selección obtenidas exitosamente.", data = accounts });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving accounts for selection");
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific user account by ID
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <returns>User account details</returns>
    [HttpGet("{accountId}")]
    public async Task<ActionResult<UserAccountDto>> GetUserAccount(string accountId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Usuario no autenticado." });
            }

            var account = await userAccountService.GetUserAccountByIdAsync(accountId);
            if (account == null)
            {
                return NotFound(new { success = false, message = "Cuenta no encontrada." });
            }

            // Verify account belongs to user
            if (account.ApplicationUserId != userId)
            {
                return Forbid("No tienes permisos para acceder a esta cuenta.");
            }

            return Ok(new { success = true, message = "Cuenta obtenida exitosamente.", data = account });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user account {AccountId}", accountId);
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new user account
    /// </summary>
    /// <param name="request">Account creation request</param>
    /// <returns>Created account</returns>
    [HttpPost]
    public async Task<ActionResult<UserAccountDto>> CreateUserAccount([FromBody] CreateUserAccountRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Usuario no autenticado." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Datos inválidos.", errors = ModelState });
            }

            // Check if user can create more accounts
            if (!await userAccountService.CanCreateMoreAccountsAsync(userId))
            {
                return BadRequest(new { success = false, message = "Has alcanzado el límite máximo de cuentas." });
            }

            var account = await userAccountService.CreateUserAccountAsync(userId, request);
            return CreatedAtAction(nameof(GetUserAccount), new { accountId = account.Id }, 
                new { success = true, message = "Cuenta creada exitosamente.", data = account });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation while creating user account");
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user account");
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing user account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="request">Account update request</param>
    /// <returns>Updated account</returns>
    [HttpPut("{accountId}")]
    public async Task<ActionResult<UserAccountDto>> UpdateUserAccount(string accountId, [FromBody] CreateUserAccountRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Usuario no autenticado." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Datos inválidos.", errors = ModelState });
            }

            // Verify account belongs to user
            var existingAccount = await userAccountService.GetUserAccountByIdAsync(accountId);
            if (existingAccount == null)
            {
                return NotFound(new { success = false, message = "Cuenta no encontrada." });
            }

            if (existingAccount.ApplicationUserId != userId)
            {
                return Forbid("No tienes permisos para modificar esta cuenta.");
            }

            var updatedAccount = await userAccountService.UpdateUserAccountAsync(accountId, request);
            return Ok(new { success = true, message = "Cuenta actualizada exitosamente.", data = updatedAccount });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation while updating user account {AccountId}", accountId);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user account {AccountId}", accountId);
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Set an account as default
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <returns>Updated account</returns>
    [HttpPatch("{accountId}/set-default")]
    public async Task<ActionResult<UserAccountDto>> SetDefaultAccount(string accountId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Usuario no autenticado." });
            }

            // Verify account belongs to user
            var existingAccount = await userAccountService.GetUserAccountByIdAsync(accountId);
            if (existingAccount == null)
            {
                return NotFound(new { success = false, message = "Cuenta no encontrada." });
            }

            if (existingAccount.ApplicationUserId != userId)
            {
                return Forbid("No tienes permisos para modificar esta cuenta.");
            }

            var updatedAccount = await userAccountService.SetDefaultAccountAsync(userId, accountId);
            return Ok(new { success = true, message = "Cuenta establecida como predeterminada exitosamente.", data = updatedAccount });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation while setting default account {AccountId}", accountId);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting default account {AccountId}", accountId);
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate a user account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{accountId}")]
    public async Task<ActionResult> DeactivateUserAccount(string accountId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Usuario no autenticado." });
            }

            // Verify account belongs to user
            var existingAccount = await userAccountService.GetUserAccountByIdAsync(accountId);
            if (existingAccount == null)
            {
                return NotFound(new { success = false, message = "Cuenta no encontrada." });
            }

            if (existingAccount.ApplicationUserId != userId)
            {
                return Forbid("No tienes permisos para eliminar esta cuenta.");
            }

            var success = await userAccountService.DeactivateUserAccountAsync(accountId);
            if (!success)
            {
                return BadRequest(new { success = false, message = "No se pudo desactivar la cuenta." });
            }

            return Ok(new { success = true, message = "Cuenta desactivada exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation while deactivating account {AccountId}", accountId);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating user account {AccountId}", accountId);
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Check if user can create more accounts
    /// </summary>
    /// <returns>Boolean indicating if more accounts can be created</returns>
    [HttpGet("can-create-more")]
    public async Task<ActionResult<bool>> CanCreateMoreAccounts()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Usuario no autenticado." });
            }

            var canCreate = await userAccountService.CanCreateMoreAccountsAsync(userId);
            return Ok(new { success = true, message = "Verificación completada.", data = canCreate });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if user can create more accounts");
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }
}