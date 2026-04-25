using Frock_backend.Notifications.Domain.Model.Aggregates;
using Frock_backend.Notifications.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Notifications.Infrastructure.Repositories;

public class NotificationRepository(AppDbContext context) : BaseRepository<Notification>(context), INotificationRepository
{
    public async Task<IEnumerable<Notification>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Notification>().Where(n => n.FkIdUser == userId).OrderByDescending(n => n.CreatedAt).ToListAsync();
    }
}
