using Frock_backend.Notifications.Domain.Model.Aggregates;
using Frock_backend.Notifications.Domain.Model.Commands;

namespace Frock_backend.Notifications.Domain.Services;

public interface INotificationCommandService
{
    Task<Notification?> Handle(MarkNotificationReadCommand command);
    Task Handle(DeleteNotificationCommand command);
}
