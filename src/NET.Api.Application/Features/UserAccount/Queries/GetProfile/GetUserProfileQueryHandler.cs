using NET.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AutoMapper;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.UserAccount.Queries.GetProfile;

public class GetUserProfileQueryHandler(
    UserManager<ApplicationUser> userManager,
    IMapper mapper,
    ILogger<GetUserProfileQueryHandler> logger) : IQueryHandler<GetUserProfileQuery, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            
            if (user == null)
            {
                logger.LogWarning("User with ID {UserId} not found", request.UserId);
                throw new InvalidOperationException("Usuario no encontrado.");
            }

            var userRoles = await userManager.GetRolesAsync(user);
            
            var userProfile = mapper.Map<UserProfileDto>(user);
            userProfile.Roles = [.. userRoles];
            
            logger.LogInformation("User profile retrieved successfully for user {UserId}", request.UserId);
            
            return userProfile;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user profile for user {UserId}", request.UserId);
            throw;
        }
    }
}