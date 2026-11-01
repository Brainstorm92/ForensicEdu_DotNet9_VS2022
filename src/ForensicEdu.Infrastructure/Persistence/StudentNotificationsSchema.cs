using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Infrastructure.Persistence;
public static class StudentNotificationsSchema
{
    public static async Task ApplyAsync(ApplicationDbContext db, CancellationToken ct=default)
    {
        using var stream=typeof(StudentNotificationsSchema).Assembly.GetManifestResourceStream("ForensicEdu.Infrastructure.Persistence.Schema.20261101_StudentNotifications.sql") ?? throw new InvalidOperationException("No se encontró el esquema de notificaciones.");
        using var reader=new StreamReader(stream);
        var sql=await reader.ReadToEndAsync(ct);
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlRawAsync(sql,ct);
        await tx.CommitAsync(ct);
    }
}
