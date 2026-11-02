using ForensicEdu.Application.Contracts.AdvisoryCare;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Interfaces;
public interface IAdvisoryCare
{
 Task<PageResult<CareSummary>> ListAsync(int page,int pageSize,string? search,string? status,CancellationToken ct);
 Task<CareDetail> GetAsync(string id,CancellationToken ct);
 Task<List<ConsultantOption>> ConsultantsAsync(CancellationToken ct);
 Task AssignAsync(string id,CareAssignment input,CancellationToken ct);
 Task UpdateAsync(string id,CareUpdate input,CancellationToken ct);
 Task SaveSessionAsync(string id,string? sessionId,CareSessionInput input,CancellationToken ct);
}
public enum CareFailure { Invalid,NotFound,Forbidden,Conflict }
public sealed class AdvisoryCareException(CareFailure failure,string message):Exception(message){public CareFailure Failure=>failure;}
