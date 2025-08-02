using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Queries.GetUserProfile;

public class GetUserProfileQuery : IQuery<UserProfileDto>
{
    public string UserId { get; set; } = string.Empty;
}