using ForensicEdu.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Infrastructure.Persistence;

public sealed class CoursePurchaseGate(ApplicationDbContext db) : ICoursePurchaseGate
{
    // Bounded locks are only for providers without relational transactions (isolated tests).
    private static readonly SemaphoreSlim[] TestLocks = Enumerable.Range(0,64).Select(_=>new SemaphoreSlim(1,1)).ToArray();
    public async Task<T> ExecuteAsync<T>(string userId, string courseId, Func<Task<T>> operation, CancellationToken ct)
    {
        var key="course-purchase:"+userId+":"+courseId;
        if(!db.Database.IsRelational())
        {
            var gate=TestLocks[(uint)StringComparer.Ordinal.GetHashCode(key)%TestLocks.Length];
            await gate.WaitAsync(ct);try{return await operation();}finally{gate.Release();}
        }
        await using var transaction=await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({key},0))",ct);
        var result=await operation();
        await transaction.CommitAsync(ct);
        return result;
    }
}
