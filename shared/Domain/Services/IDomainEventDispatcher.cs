using MediatR;

namespace Frock_backend.shared.Domain.Services;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<INotification> domainEvents, CancellationToken cancellationToken = default);
}
