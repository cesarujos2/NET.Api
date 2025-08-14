using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.User;

namespace NET.Api.Application.Features.UserAccount.Queries.GetProfileStatus;

public class GetProfileStatusQuery : IQuery<UserStatusDto>
{
    public string UserId { get; set; } = string.Empty;
}