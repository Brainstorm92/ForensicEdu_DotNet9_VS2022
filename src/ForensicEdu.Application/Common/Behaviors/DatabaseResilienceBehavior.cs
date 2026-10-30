using ForensicEdu.Application.Common.Interfaces;
using MediatR;
namespace ForensicEdu.Application.Common.Behaviors;

public sealed class DatabaseResilienceBehavior<TRequest,TResponse>(IDatabaseResilience resilience)
    : IPipelineBehavior<TRequest,TResponse> where TRequest:notnull
{
    public Task<TResponse> Handle(TRequest request,RequestHandlerDelegate<TResponse> next,CancellationToken cancellationToken)
        => resilience.ExecuteAsync(_=>next(),cancellationToken);
}
