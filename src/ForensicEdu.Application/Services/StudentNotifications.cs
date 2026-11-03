using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Services;

public static class StudentNotifications
{
    // Caller commits together with payment, support reply or account state.
    public static void Add(IApplicationDbContext db, string userId, string key, string kind,
        string title, string body, string tab, DateTime? expires = null)
        => db.StudentNotifications.Add(new StudentNotification { UserId=userId, EventKey=key,
            Kind=kind, Title=title, Body=body, ActionTab=tab, EmailExpiresAt=expires });

    public static async Task CancelValidityMail(IApplicationDbContext db, string userId, CancellationToken ct)
    {
        var pending=await db.StudentNotifications.Where(n=>n.UserId==userId && n.Kind=="VIGENCIA" &&
            n.EmailStatus!="SENT" && n.EmailStatus!="CANCELLED").ToListAsync(ct);
        foreach(var n in pending) { n.EmailStatus="CANCELLED"; n.Version=Guid.NewGuid().ToString(); }
    }
}
