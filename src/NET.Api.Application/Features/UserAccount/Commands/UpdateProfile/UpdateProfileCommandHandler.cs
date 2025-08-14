using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Exceptions;
using NET.Api.Application.Common.Models.User;
using NET.Api.Domain.Entities;
using NET.Api.Domain.Services;

namespace NET.Api.Application.Features.UserAccount.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand, UserDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IProfileCompletionService _profileCompletionService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;

    public UpdateProfileCommandHandler(
        UserManager<ApplicationUser> userManager,
        IProfileCompletionService profileCompletionService,
        IMapper mapper,
        ILogger<UpdateProfileCommandHandler> logger)
    {
        _userManager = userManager;
        _profileCompletionService = profileCompletionService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Actualizando perfil del usuario {UserId}", request.UserId);

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado: {UserId}", request.UserId);
                throw new NotFoundException("Usuario no encontrado.");
            }

            // Verificar si el documento de identidad ya existe
            var existingUserWithDocument = _userManager.Users
                .FirstOrDefault(u => u.IdentityDocument == request.IdentityDocument && u.Id != request.UserId);
            
            if (existingUserWithDocument != null)
            {
                _logger.LogWarning("Documento de identidad ya existe: {IdentityDocument}", request.IdentityDocument);
                throw new ValidationException("IdentityDocument", "El documento de identidad ya está registrado por otro usuario.");
            }

            // Actualizar propiedades del usuario
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.IdentityDocument = request.IdentityDocument;
            user.DateOfBirth = request.DateOfBirth;
            user.Address = request.Address;
            
            // Actualizar estado de completitud del perfil
            _profileCompletionService.UpdateProfileCompletionStatus(user);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Error al actualizar el perfil del usuario {UserId}: {Errors}", request.UserId, errors);
                throw new InvalidOperationException($"Error al actualizar el perfil: {errors}");
            }

            _logger.LogInformation("Perfil actualizado exitosamente para el usuario {UserId}", request.UserId);

            // Obtener roles del usuario
            var roles = await _userManager.GetRolesAsync(user);

            // Mapear a DTO
            var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roles.ToList();

        return userDto;
        }
        catch (Exception ex) when (!(ex is NotFoundException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Error inesperado al actualizar el perfil del usuario {UserId}", request.UserId);
            throw new InvalidOperationException("Ocurrió un error inesperado al actualizar el perfil.");
        }
    }
}