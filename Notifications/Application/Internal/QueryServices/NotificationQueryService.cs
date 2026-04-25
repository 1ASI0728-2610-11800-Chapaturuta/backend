using Frock_backend.Notifications.Domain.Model.Aggregates;
using Frock_backend.Notifications.Domain.Model.Queries;
using Frock_backend.Notifications.Domain.Repositories;
using Frock_backend.Notifications.Domain.Services;

namespace Frock_backend.Notifications.Application.Internal.QueryServices;

public class NotificationQueryService(INotificationRepository notificationRepository) : INotificationQueryService
{
    public async Task<IEnumerable<Notification>> Handle(GetNotificationsByUserIdQuery query)
    {
        return await notificationRepository.FindByUserIdAsync(query.UserId);
    }
}
