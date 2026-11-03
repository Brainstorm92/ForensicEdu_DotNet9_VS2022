using System.Data;
using ForensicEdu.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Infrastructure.Persistence;

public sealed class PaymentMutationGate(ApplicationDbContext db):IPaymentMutationGate
{
 private static readonly SemaphoreSlim TestLock=new(1,1);
 public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation,CancellationToken ct)
 {
  if(!db.Database.IsRelational()){
   await TestLock.WaitAsync(ct);try{return await operation();}finally{TestLock.Release();}
  }
  // A fresh READ COMMITTED snapshot after the lock sees the previous owner's commit.
  // Always acquire payment before course locks. Never reuse an unknown outer transaction.
  if(db.Database.CurrentTransaction!=null)throw new InvalidOperationException("El control de pagos requiere su propia transacción.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted,ct);
  await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(642731905281146)",ct);
  var result=await operation();await tx.CommitAsync(ct);return result;
 }
}
