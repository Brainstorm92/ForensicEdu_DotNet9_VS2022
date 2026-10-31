namespace ForensicEdu.Application.Common.Interfaces;

public interface ITransientFailureClassifier
{
    bool IsTransient(Exception exception);
}
