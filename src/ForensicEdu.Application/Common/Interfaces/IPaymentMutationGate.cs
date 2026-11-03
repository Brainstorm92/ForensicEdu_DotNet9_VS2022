namespace ForensicEdu.Application.Common.Interfaces;
// All bank evidence mutations share this gate, across students, products and API replicas.
public interface IPaymentMutationGate
{
 Task<T> ExecuteAsync<T>(Func<Task<T>> operation,CancellationToken ct);
}
