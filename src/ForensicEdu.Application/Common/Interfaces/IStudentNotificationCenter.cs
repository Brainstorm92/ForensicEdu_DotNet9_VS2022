namespace ForensicEdu.Application.Common.Interfaces;
public sealed record StudentNotice(string Id, string Kind, string Title, string Body, string ActionTab, DateTime CreatedAt, DateTime? ReadAt);
public sealed record StudentNoticePage(IReadOnlyList<StudentNotice> Items, int Total, int UnreadCount, int Page, int PageSize);
public interface IStudentNotificationCenter
{
    Task<StudentNoticePage> ListAsync(int page, bool unreadOnly, CancellationToken ct);
    Task<bool> ReadAsync(string id, CancellationToken ct);
    Task ReadAllAsync(CancellationToken ct);
}
