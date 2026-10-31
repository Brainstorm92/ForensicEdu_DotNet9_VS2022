using ForensicEdu.Application.Common.Interfaces;
using Npgsql;
namespace ForensicEdu.Infrastructure.Resilience;

public sealed class NpgsqlTransientFailureClassifier : ITransientFailureClassifier
{
    public bool IsTransient(Exception exception)
    {
        // Cancellation and business errors must not open the database circuit.
        if(exception is OperationCanceledException)return false;
        for(Exception? current=exception;current!=null;current=current.InnerException)
            if(current is NpgsqlException postgres)return postgres.IsTransient;
        return false;
    }
}
