using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using NET.Api.Application.Common.Models.Authentication;
using NET.Api.Application.Common.Models.UserAccount;
using NET.Api.Application.Features.UserAccount.Commands.ChangeEmail;
using NET.Api.Application.Features.UserAccount.Commands.ChangePassword;
using NET.Api.Application.Features.UserAccount.Commands.ConfirmEmailChange;
using NET.Api.Application.Features.UserAccount.Commands.UpdateProfile;
using NET.Api.Application.Features.UserAccount.Queries.GetProfile;
using System.Security.Claims;

namespace NET.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserAccountController(IMediator mediator, ILogger<UserAccountController> logger) : ControllerBase
{

    /// <summary>
    /// Get current user profile (requires authentication)
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Usuario no autenticado." });
            }

            var query = new GetUserProfileQuery
            {
                UserId = userId
            };

            var result = await mediator.Send(query);
            return Ok(new { success = true, message = "Perfil obtenido exitosamente.", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar perfil del usuario
    /// </summary>
    /// <param name="request">Datos del perfil a actualizar</param>
    /// <returns>Perfil actualizado</returns>
    [HttpPut("profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateUserProfileRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Usuario no autenticado." });
            }

            var command = new UpdateProfileCommand
            {
                UserId = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber
            };

            var result = await mediator.Send(command);
            return Ok(new { success = true, message = "Perfil actualizado exitosamente.", data = result });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Cambiar email del usuario
    /// </summary>
    /// <param name="request">Datos para cambio de email</param>
    /// <returns>Resultado del cambio de email</returns>
    [HttpPost("change-email")]
    public async Task<ActionResult<UserAccountResponseDto>> ChangeEmail([FromBody] ChangeEmailRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Usuario no autenticado." });
            }

            var command = new ChangeEmailCommand
            {
                UserId = userId,
                NewEmail = request.NewEmail,
                CurrentPassword = request.CurrentPassword,
                BaseUrl = request.BaseUrl
            };

            var result = await mediator.Send(command);
            return Ok(new { success = result.Success, message = result.Message, data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error changing user email");
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Confirmar cambio de email
    /// </summary>
    /// <param name="request">Datos para confirmar cambio de email</param>
    /// <returns>Resultado de la confirmación</returns>
    [HttpPost("confirm-email-change")]
    [AllowAnonymous]
    public async Task<ActionResult<UserAccountResponseDto>> ConfirmEmailChange([FromBody] ConfirmEmailChangeRequestDto request)
    {
        try
        {
            var command = new ConfirmEmailChangeCommand
            {
                UserId = request.UserId,
                NewEmail = request.NewEmail,
                Token = request.Token
            };

            var result = await mediator.Send(command);
            return Ok(new { success = result.Success, message = result.Message, data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming email change");
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Cambiar contraseña del usuario
    /// </summary>
    /// <param name="request">Datos para cambio de contraseña</param>
    /// <returns>Resultado del cambio de contraseña</returns>
    [HttpPost("change-password")]
    public async Task<ActionResult<UserAccountResponseDto>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Usuario no autenticado." });
            }

            var command = new ChangePasswordCommand
            {
                UserId = userId,
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword,
                ConfirmPassword = request.ConfirmPassword
            };

            var result = await mediator.Send(command);
            return Ok(new { success = result.Success, message = result.Message, data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error changing user password");
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }
}