using MediatR;

namespace NET.Api.Application.Commands;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
