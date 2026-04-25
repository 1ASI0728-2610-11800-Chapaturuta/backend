using Frock_backend.shared.Domain.Services;
using MediatR;

namespace Frock_backend.shared.Infrastructure.Services;

public class DomainEventDispatcher(IMediator mediator) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<INotification> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
