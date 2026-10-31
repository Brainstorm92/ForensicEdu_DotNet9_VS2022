namespace ForensicEdu.Application.Common.Interfaces;

public interface IDatabaseResilience
{
    Task<T> ExecuteAsync<T>(Func<CancellationToken,Task<T>> operation,CancellationToken cancellationToken=default);
}
