using Frock_backend.Notifications.Domain.Model.Aggregates;
using Frock_backend.Notifications.Domain.Model.Queries;

namespace Frock_backend.Notifications.Domain.Services;

public interface INotificationQueryService
{
    Task<IEnumerable<Notification>> Handle(GetNotificationsByUserIdQuery query);
}
