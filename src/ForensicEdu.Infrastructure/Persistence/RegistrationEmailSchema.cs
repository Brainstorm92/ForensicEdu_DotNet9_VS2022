using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Infrastructure.Persistence;
public static class RegistrationEmailSchema
{
 public static async Task ApplyAsync(ApplicationDbContext db,CancellationToken ct=default){using var stream=typeof(RegistrationEmailSchema).Assembly.GetManifestResourceStream("ForensicEdu.Infrastructure.Persistence.Schema.20261031_RegistrationEmail.sql")??throw new InvalidOperationException("No se encontró la actualización de registro.");using var reader=new StreamReader(stream);var sql=await reader.ReadToEndAsync(ct);await using var tx=await db.Database.BeginTransactionAsync(ct);await db.Database.ExecuteSqlRawAsync(sql,ct);await tx.CommitAsync(ct);}
}
