namespace ForensicEdu.Domain.Entities;

// Inbox and durable email queue are committed with the event that created the notice.
public sealed class StudentNotification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = "";
    public User? User { get; set; }
    public string EventKey { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string ActionTab { get; set; } = "notificaciones";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public DateTime? EmailExpiresAt { get; set; }
    public DateTime? EmailSentAt { get; set; }
    public DateTime? NextEmailAttemptAt { get; set; }
    public int EmailAttempts { get; set; }
    public string EmailStatus { get; set; } = "PENDING";
    public string Version { get; set; } = Guid.NewGuid().ToString();
}
