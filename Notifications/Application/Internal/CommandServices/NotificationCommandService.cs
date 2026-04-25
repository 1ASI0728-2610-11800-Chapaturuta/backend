using Frock_backend.Notifications.Domain.Model.Aggregates;
using Frock_backend.Notifications.Domain.Model.Commands;
using Frock_backend.Notifications.Domain.Repositories;
using Frock_backend.Notifications.Domain.Services;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Notifications.Application.Internal.CommandServices;

public class NotificationCommandService(INotificationRepository notificationRepository, IUnitOfWork unitOfWork) : INotificationCommandService
{
    public async Task<Notification?> Handle(MarkNotificationReadCommand command)
    {
        var notification = await notificationRepository.FindByIdAsync(command.Id);
        if (notification == null) return null;

        notification.IsRead = true;
        try
        {
            notificationRepository.Update(notification);
            await unitOfWork.CompleteAsync();
            return notification;
        }
        catch (Exception e)
        {
            throw new Exception($"Error marking notification as read: {e.Message}");
        }
    }

    public async Task Handle(DeleteNotificationCommand command)
    {
        var notification = await notificationRepository.FindByIdAsync(command.Id);
        if (notification == null) throw new KeyNotFoundException("Notification not found");

        try
        {
            notificationRepository.Remove(notification);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            throw new Exception($"Error deleting notification: {e.Message}");
        }
    }
}
