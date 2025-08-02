using MediatR;

namespace NET.Api.Application.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

public interface IQuery : IRequest
{
}
