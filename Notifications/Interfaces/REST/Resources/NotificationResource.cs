namespace Frock_backend.Notifications.Interfaces.REST.Resources;

public record NotificationResource(int Id, int FkIdUser, string Title, string Message, string Type, bool IsRead, DateTime CreatedAt);
