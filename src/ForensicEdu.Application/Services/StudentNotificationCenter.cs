using ForensicEdu.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Domain.Entities;
namespace ForensicEdu.Application.Services;

public sealed class StudentNotificationCenter(IApplicationDbContext db, ICurrentUser current) : IStudentNotificationCenter
{
    public async Task<StudentNoticePage> ListAsync(int page, bool unreadOnly, CancellationToken ct)
    {
        page=Math.Clamp(page,1,100000); const int size=20;
        var mine=db.StudentNotifications.AsNoTracking().Where(n=>n.UserId==current.UserId);
        var unread=await mine.CountAsync(n=>n.ReadAt==null,ct);
        var query=unreadOnly?mine.Where(n=>n.ReadAt==null):mine;
        var total=await query.CountAsync(ct);
        var items=await query.OrderByDescending(n=>n.CreatedAt).ThenByDescending(n=>n.Id).Skip((page-1)*size).Take(size)
            .Select(n=>new StudentNotice(n.Id,n.Kind,n.Title,n.Body,n.ActionTab,n.CreatedAt,n.ReadAt)).ToListAsync(ct);
        return new(items,total,unread,page,size);
    }
    public async Task<bool> ReadAsync(string id, CancellationToken ct)
    {
        var n=await db.StudentNotifications.SingleOrDefaultAsync(n=>n.Id==id && n.UserId==current.UserId,ct);
        if(n==null)return false;
        if(n.ReadAt==null)await SaveReadingAsync([n],ct);
        return true;
    }
    public async Task ReadAllAsync(CancellationToken ct)
    {
        var mine=await db.StudentNotifications.Where(n=>n.UserId==current.UserId && n.ReadAt==null).ToListAsync(ct);
        await SaveReadingAsync(mine,ct);
    }
    private async Task SaveReadingAsync(IReadOnlyList<StudentNotification> notices, CancellationToken ct)
    {
        for(var attempt=0;;attempt++)
        {
            foreach(var n in notices)n.ReadAt ??= DateTime.UtcNow;
            try{await db.SaveChangesAsync(ct);return;}
            catch(DbUpdateConcurrencyException e) when(attempt<2)
            {
                // The email worker may claim a row while its owner marks it read.
                foreach(var entry in e.Entries)await entry.ReloadAsync(ct);
            }
        }
    }
}
