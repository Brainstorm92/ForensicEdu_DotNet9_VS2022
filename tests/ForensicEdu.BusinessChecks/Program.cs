using System.Text.Json;
using ForensicEdu.Application.Services;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

if (args.Length == 2 && args[0] == "--verify-remove-integration-user")
{
    var email = args[1];
    if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^integration-[a-f0-9]{32}@example\.invalid$"))
        throw new InvalidOperationException("Solo se admiten las cuentas temporales generadas por la prueba de integración.");
    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets", "ForensicEdu-Local-Development", "secrets.json");
    using var secrets = JsonDocument.Parse(await File.ReadAllTextAsync(path));
    await using var connection = new Npgsql.NpgsqlConnection(secrets.RootElement.GetProperty("ConnectionStrings:DefaultConnection").GetString());
    await connection.OpenAsync();
    await using var transaction = await connection.BeginTransactionAsync();
    await using var query = new Npgsql.NpgsqlCommand("SELECT \"Id\" FROM \"Users\" WHERE \"Email\"=@email AND \"Nombres\"='Integración' AND \"Apellidos\"='Temporal' AND \"Profesion\"='Prueba de software' AND \"InstitucionEducativa\"='Prueba automatizada'", connection, transaction);
    query.Parameters.AddWithValue("email", email);
    var id = await query.ExecuteScalarAsync() as string ?? throw new Exception("No se encontró el registro esperado en PostgreSQL.");
    Console.WriteLine("OK: registro y datos académicos confirmados directamente en PostgreSQL.");
    await using var delete = new Npgsql.NpgsqlCommand("DELETE FROM \"AuditLogs\" WHERE \"UserId\"=@id; DELETE FROM \"Users\" WHERE \"Id\"=@id AND \"Email\"=@email;", connection, transaction);
    delete.Parameters.AddWithValue("id", id); delete.Parameters.AddWithValue("email", email);
    await delete.ExecuteNonQueryAsync();
    await transaction.CommitAsync();
    Console.WriteLine("OK: eliminada únicamente la cuenta temporal y su auditoría de prueba.");
    return;
}

if (args.Length == 1 && (args[0] == "--inspect-database" || args[0] == "--initialize-empty-database"))
{
    var secretsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets", "ForensicEdu-Local-Development", "secrets.json");
    using var secrets = JsonDocument.Parse(await File.ReadAllTextAsync(secretsPath));
    await using var connection = new Npgsql.NpgsqlConnection(secrets.RootElement.GetProperty("ConnectionStrings:DefaultConnection").GetString());
    try
    {
        await connection.OpenAsync();
        if (args[0] == "--initialize-empty-database")
        {
            await using var transaction = await connection.BeginTransactionAsync();
            await using var mutex = new Npgsql.NpgsqlCommand("SELECT pg_advisory_xact_lock(72026030)", connection, transaction);
            await mutex.ExecuteNonQueryAsync();
            await using var countCommand = new Npgsql.NpgsqlCommand("SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public'", connection, transaction);
            if (Convert.ToInt64(await countCommand.ExecuteScalarAsync()) != 0)
                throw new InvalidOperationException("Inicialización cancelada: la base ya tiene tablas. Use migraciones.");
            using var model = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(connection.ConnectionString).Options);
            await using var create = new Npgsql.NpgsqlCommand(model.Database.GenerateCreateScript(), connection, transaction);
            await create.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            Console.WriteLine("Esquema inicial creado en base vacía mediante transacción.");
        }
        await using var command = new Npgsql.NpgsqlCommand("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name", connection);
        await using var reader = await command.ExecuteReaderAsync();
        Console.WriteLine("Conexión PostgreSQL verificada. Tablas públicas existentes:");
        var count = 0;
        while (await reader.ReadAsync()) { Console.WriteLine(reader.GetString(0)); count++; }
        Console.WriteLine($"Total: {count}.");
    }
    catch (Npgsql.PostgresException ex) { Console.Error.WriteLine($"PostgreSQL rechazó la conexión o consulta. SQLSTATE: {ex.SqlState}"); Environment.ExitCode = 1; }
    catch (Npgsql.NpgsqlException) { Console.Error.WriteLine("No se pudo establecer conexión con el servidor PostgreSQL proporcionado."); Environment.ExitCode = 1; }
    return;
}

void Check(bool ok, string message) { if (!ok) throw new Exception(message); Console.WriteLine("OK: " + message); }
ApplicationDbContext NewDb() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
async Task MustReject(Func<Task> action, string message)
{
    try { await action(); } catch (InvalidOperationException) { Console.WriteLine("OK: " + message); return; }
    throw new Exception(message);
}
using (var db = NewDb())
{
    var user = new User { ValidUntil = DateTime.UtcNow.AddYears(5) }; db.Users.Add(user); await db.SaveChangesAsync();
    var approval = DateTime.UtcNow;
    await new AccountLifecycleService(db).ExtendValidityOnApprovedPaymentAsync(user.Id, approval);
    Check(user.ValidUntil == approval.AddYears(2), "Renueva desde aprobación, sin acumular años");
    var old = new User { RegisteredAt = DateTime.UtcNow.AddDays(-31), Status = AccountStatus.REGISTRADA };
    db.Users.Add(old); await db.SaveChangesAsync();
    await new AccountLifecycleService(db).ProcessDailyAccountExpirationsAsync();
    Check(old.Status == AccountStatus.PENDIENTE_DE_ELIMINACION, "30 días sin compras: pendiente de eliminación");
    db.PaymentEvidences.Add(new PaymentEvidence { Order = new Order { UserId = user.Id, User = user }, BankName = "BCP", OperationNumber = "123", Status = PaymentStatus.APROBADO,
        PaymentFingerprint = "original", Amount = 20 }); await db.SaveChangesAsync();
    var duplicate = await new AntifraudService(db).EvaluatePaymentAsync("bcp", "123", DateTime.UtcNow, 999, "Otro titular");
    Check(duplicate.IsDuplicate, "Bloquea operación aprobada aunque cambie monto o titular");
}
foreach (var correct in new[] { 1299, 1300 })
{
    using var db = NewDb();
    var course = new Course(); var user = new User();
    var enrollment = new Enrollment { Course = course, CourseId = course.Id, User = user, UserId = user.Id, ProgressPercent = 100 };
    var snapshot = JsonSerializer.Serialize(Enumerable.Range(0, 2000).Select(i => new { id = i.ToString(), correctOptionIndex = 1 }));
    var attempt = new ExamAttempt { Enrollment = enrollment, EnrollmentId = enrollment.Id, UserId = user.Id,
        QuestionsSnapshotJson = snapshot, StartedAt = DateTime.UtcNow };
    db.AddRange(course, user, enrollment, attempt); await db.SaveChangesAsync();
    var service = new EvaluationService(db);
    var answers = Enumerable.Range(0, correct).ToDictionary(i => i.ToString(), _ => 1);
    var result = await service.SubmitExamAttemptAsync(new(enrollment.Id, user.Id, answers, 0));
    Check(result.Score == (correct == 1299 ? 12.99m : 13m), "Límite exacto de nota " + result.Score);
    Check(result.Approved == (correct == 1300), "Certifica solamente desde 13");
    await MustReject(() => service.SubmitExamAttemptAsync(new(enrollment.Id, user.Id, answers, 0)),
        correct == 1299 ? "Bloquea reintento antes de 24 horas" : "Bloquea reintento después de aprobar");
}
using (var db = NewDb())
{
    var course = new Course(); var user = new User();
    var enrollment = new Enrollment { Course = course, CourseId = course.Id, User = user, UserId = user.Id };
    db.AddRange(course, user, enrollment);
    db.ExamAttempts.Add(new ExamAttempt { Enrollment = enrollment, UserId = user.Id, StartedAt = DateTime.UtcNow.AddMinutes(-11),
        QuestionsSnapshotJson = "[{\"id\":\"a\",\"correctOptionIndex\":1}]", AnswersJson = "{\"a\":0}" });
    await db.SaveChangesAsync();
    var result = await new EvaluationService(db).SubmitExamAttemptAsync(new(enrollment.Id, user.Id, new() { ["a"] = 1 }, 0));
    Check(result.Score == 0, "Ignora respuestas tardías y duración enviada por navegador");
}
if (args.Length == 2 && args[0] == "--schema")
{
    using var pg = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql("Host=localhost;Database=ForensicEduDb").Options);
    await File.WriteAllTextAsync(args[1], pg.Database.GenerateCreateScript());
    Console.WriteLine("Esquema PostgreSQL generado, sin conectar ni modificar una base existente.");
}
