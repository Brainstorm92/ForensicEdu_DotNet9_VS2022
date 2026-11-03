namespace ForensicEdu.Application.Common.Interfaces;
// Serialize checkout/approval for one student and course across API instances.
public interface ICoursePurchaseGate
{
    Task<T> ExecuteAsync<T>(string userId, string courseId, Func<Task<T>> operation, CancellationToken ct);
}
