using Frock_backend.Notifications.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Notifications.Domain.Repositories;

public interface INotificationRepository : IBaseRepository<Notification>
{
    Task<IEnumerable<Notification>> FindByUserIdAsync(int userId);
}
