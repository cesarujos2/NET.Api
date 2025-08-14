using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NET.Api.Application.Common.Models.User;
using NET.Api.Application.Features.UserAccount.Commands.ChangeEmail;
using NET.Api.Application.Features.UserAccount.Commands.ChangePassword;
using NET.Api.Application.Features.UserAccount.Commands.ConfirmEmailChange;
using NET.Api.Application.Features.UserAccount.Commands.UpdateProfile;
using NET.Api.Application.Features.UserAccount.Queries.GetProfile;
using NET.Api.Application.Features.UserAccount.Queries.GetProfileStatus;
using NET.Api.WebApi.Controllers;
using System.Security.Claims;

namespace NET.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserAccountController(IMediator mediator) : BaseApiController
{

    /// <summary>
    /// Get current user profile (requires authentication)
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile()
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

    /// <summary>
    /// Actualizar perfil del usuario
    /// </summary>
    /// <param name="request">Datos del perfil a actualizar</param>
    /// <returns>Perfil actualizado</returns>
    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateUserRequestDto request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
        {
            return Unauthorized(new { success = false, message = "Usuario no autenticado." });
        }

        var command = new UpdateProfileCommand
        {
            UserId = CurrentUserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IdentityDocument = request.IdentityDocument,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address
        };

        var result = await mediator.Send(command);
        return Ok(new { success = true, message = "Perfil actualizado exitosamente.", data = result });
    }

    /// <summary>
    /// Cambiar email del usuario
    /// </summary>
    /// <param name="request">Datos para cambio de email</param>
    /// <returns>Resultado del cambio de email</returns>
    [HttpPost("change-email")]
    public async Task<ActionResult<UserOperationResponseDto>> ChangeEmail([FromBody] ChangeUserEmailRequestDto request)
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

    /// <summary>
    /// Confirmar cambio de email
    /// </summary>
    /// <param name="request">Datos para confirmar cambio de email</param>
    /// <returns>Resultado de la confirmación</returns>
    [HttpPost("confirm-email-change")]
    [AllowAnonymous]
    public async Task<ActionResult<UserOperationResponseDto>> ConfirmEmailChange([FromBody] ConfirmUserEmailChangeRequestDto request)
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

    /// <summary>
    /// Cambiar contraseña del usuario
    /// </summary>
    /// <param name="request">Datos para cambio de contraseña</param>
    /// <returns>Resultado del cambio de contraseña</returns>
    [HttpPost("change-password")]
    public async Task<ActionResult<UserOperationResponseDto>> ChangePassword([FromBody] ChangeUserPasswordRequestDto request)
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

    /// <summary>
    /// Obtiene el estado de completitud del perfil del usuario autenticado
    /// </summary>
    /// <returns>Estado del perfil del usuario</returns>
    [HttpGet("profile/status")]
    public async Task<ActionResult<UserStatusDto>> GetProfileStatus()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { success = false, message = "Usuario no autenticado." });
        }

        var query = new GetProfileStatusQuery { UserId = userId };
        var result = await mediator.Send(query);

        return Ok(new { success = true, message = "Estado del perfil obtenido exitosamente.", data = result });
    }
}