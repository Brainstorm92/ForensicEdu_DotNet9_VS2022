using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Services;

public static class PaymentAntifraud
{
 public static string Bank(string value)=>value.Trim().ToUpperInvariant();
 public static string Operation(string value)=>value.Trim().ToUpperInvariant().Replace(" ","").Replace("-","").Replace("/","");
 private static string Holder(string value)=>string.Join(" ",value.Trim().ToUpperInvariant().Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries));
 public sealed record Match(string PaymentId,string OrderCode,string Reason,PaymentStatus Status);
 public sealed record Assessment(List<Match> ExactDuplicates,List<Match> PartialMatches,string ReviewToken);
 public static async Task<Assessment> AssessAsync(IApplicationDbContext db,PaymentEvidence payment,CancellationToken ct)
 {
  var bank=Bank(payment.BankName);var operation=Operation(payment.OperationNumber);var day=payment.OperationDate.Date;var next=day.AddDays(1);var hash=payment.VoucherFileHash.Trim().ToLowerInvariant();
  // Restrict candidates in SQL; no file content or personal details are returned.
  var candidates=await db.PaymentEvidences.AsNoTracking().Include(p=>p.Order).Where(p=>p.Id!=payment.Id &&
   (p.Status==PaymentStatus.APROBADO||p.Status==PaymentStatus.PENDIENTE||p.Status==PaymentStatus.EN_REVISION||p.Status==PaymentStatus.OBSERVADO) &&
   (p.OperationNumber.Trim().ToUpper().Replace(" ","").Replace("-","").Replace("/","")==operation ||
    (hash!=""&&p.VoucherFileHash.ToLower()==hash) ||
    (p.PaymentFingerprint!=""&&p.PaymentFingerprint==payment.PaymentFingerprint) ||
    (p.BankName.Trim().ToUpper()==bank&&p.OperationDate>=day&&p.OperationDate<next&&p.Amount==payment.Amount)))
   .ToListAsync(ct);
  var exact=new List<Match>();var partial=new List<Match>();
  foreach(var other in candidates.OrderBy(p=>p.Id,StringComparer.Ordinal)){
   var sameOp=Operation(other.OperationNumber)==operation&&operation!="";var sameBank=Bank(other.BankName)==bank;
   var sameHash=hash!=""&&other.VoucherFileHash.Equals(hash,StringComparison.OrdinalIgnoreCase);
   var sameFingerprint=payment.PaymentFingerprint!=""&&other.PaymentFingerprint==payment.PaymentFingerprint;
   if(other.Status==PaymentStatus.APROBADO&&(sameBank&&sameOp||sameHash||sameFingerprint)){
    exact.Add(new(other.Id,other.Order?.OrderCode??other.OrderId,sameHash?"Comprobante ya utilizado":"Operación bancaria ya aprobada",other.Status));continue;
   }
   string? reason=sameHash?"Mismo comprobante en otro pago pendiente":sameBank&&sameOp?"Misma operación en otro pago pendiente":sameOp&&operation.Length>=4?"Mismo número de operación en otro banco":
    sameBank&&other.OperationDate.Date==day&&other.Amount==payment.Amount&&Holder(other.AccountHolderName)==Holder(payment.AccountHolderName)&&Holder(payment.AccountHolderName)!=""?"Coinciden banco, fecha, monto y titular; cambia la operación":null;
   if(reason!=null)partial.Add(new(other.Id,other.Order?.OrderCode??other.OrderId,reason,other.Status));
  }
  // Binds manual acknowledgement to current evidence and candidate set; replacements invalidate it.
  var snapshot=JsonSerializer.Serialize(new{payment.Id,bank,operation,payment.OperationDate,payment.Amount,holder=Holder(payment.AccountHolderName),hash,payment.VoucherFileUrl,exact,partial,evidence=candidates.OrderBy(p=>p.Id,StringComparer.Ordinal).Select(p=>new{p.Id,p.BankName,p.OperationNumber,p.OperationDate,p.Amount,p.AccountHolderName,p.VoucherFileHash,p.VoucherFileUrl,p.Status})});
  return new(exact,partial,Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(snapshot))));
 }
}
