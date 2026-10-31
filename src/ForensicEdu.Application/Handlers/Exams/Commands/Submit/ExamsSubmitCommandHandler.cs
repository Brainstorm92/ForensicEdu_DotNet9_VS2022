using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Exams.Commands.Submit;

public sealed class ExamsSubmitCommandHandler(IEvaluationService _evaluationService, ICurrentUser current) : IRequestHandler<ExamsSubmitCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ExamsSubmitCommand message, CancellationToken cancellationToken)
    {
        var submission = message.Submission;

        if (submission.UserId != current.UserId) return ApplicationResult.Forbidden();
        try
        {
            var result = await _evaluationService.SubmitExamAttemptAsync(submission, cancellationToken);
            return ApplicationResult.Success(result);
        }
        catch (System.InvalidOperationException ex)
        {
            return ApplicationResult.Invalid(new { message = ex.Message });
        }
    }
}
