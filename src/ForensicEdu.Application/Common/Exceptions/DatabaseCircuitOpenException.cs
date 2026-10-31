namespace ForensicEdu.Application.Common.Exceptions;

public sealed class DatabaseCircuitOpenException(TimeSpan retryAfter,Exception innerException)
    : Exception("PostgreSQL no está disponible temporalmente. El circuito de protección está abierto.",innerException)
{
    public TimeSpan RetryAfter { get; } = retryAfter;
}
