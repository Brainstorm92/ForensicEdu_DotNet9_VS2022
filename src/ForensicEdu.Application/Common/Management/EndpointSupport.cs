using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Common.Management;

public static class EndpointSupport
{
    public static bool HasRole(ICurrentUser user, params string[] roles) => roles.Any(user.IsInRole);
    public static async Task<PageResult<T>> PageAsync<T>(IQueryable<T> query, int page, int pageSize, CancellationToken ct)
        => new(await query.Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct), await query.CountAsync(ct), page, pageSize);
    public static void Audit(IApplicationDbContext db, ICurrentUser user, string action, string entity, string id, string details="")
        => db.AuditLogs.Add(new AuditLog { UserId=user.UserId, Action=action, EntityType=entity, EntityId=id, Details=details });
    public static readonly System.Text.Json.JsonSerializerOptions Json = new(System.Text.Json.JsonSerializerDefaults.Web);
}
