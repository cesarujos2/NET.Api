using MediatR;

namespace NET.Api.Application.Queries;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
