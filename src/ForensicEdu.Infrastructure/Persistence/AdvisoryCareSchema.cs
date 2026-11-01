using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Infrastructure.Persistence;
public static class AdvisoryCareSchema
{
 public static async Task ApplyAsync(ApplicationDbContext db,CancellationToken ct=default){
  using var stream=typeof(AdvisoryCareSchema).Assembly.GetManifestResourceStream("ForensicEdu.Infrastructure.Persistence.Schema.20261031_AdvisoryCare.sql")??throw new InvalidOperationException("No se encontró el script de actualización.");
  using var reader=new StreamReader(stream);var sql=await reader.ReadToEndAsync(ct);
  await using var tx=await db.Database.BeginTransactionAsync(ct);await db.Database.ExecuteSqlRawAsync(sql,ct);await tx.CommitAsync(ct);
 }
}
