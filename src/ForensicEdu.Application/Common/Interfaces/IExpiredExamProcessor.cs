namespace ForensicEdu.Application.Common.Interfaces;

public interface IExpiredExamProcessor
{
    Task<int> ProcessAsync(CancellationToken cancellationToken);
}
