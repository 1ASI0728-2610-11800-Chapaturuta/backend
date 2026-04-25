namespace Frock_backend.Notifications.Domain.Model.Aggregates;

public class Notification
{
    public int Id { get; }
    public int FkIdUser { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info"; // Info, Warning, Success, Error
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    protected Notification() { CreatedAt = DateTime.UtcNow; }

    public Notification(int fkIdUser, string title, string message, string type = "Info")
    {
        FkIdUser = fkIdUser;
        Title = title;
        Message = message;
        Type = type;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }
}
