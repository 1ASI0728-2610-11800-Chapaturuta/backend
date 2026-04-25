using Frock_backend.Notifications.Domain.Model.Aggregates;
using Frock_backend.Notifications.Interfaces.REST.Resources;

namespace Frock_backend.Notifications.Interfaces.REST.Transform;

public static class NotificationResourceFromEntityAssembler
{
    public static NotificationResource ToResourceFromEntity(Notification entity) =>
        new NotificationResource(entity.Id, entity.FkIdUser, entity.Title, entity.Message, entity.Type, entity.IsRead, entity.CreatedAt);
}
