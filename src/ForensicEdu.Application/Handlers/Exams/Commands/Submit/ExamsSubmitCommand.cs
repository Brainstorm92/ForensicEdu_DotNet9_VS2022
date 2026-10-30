using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Handlers.Exams.Commands.Submit;
public sealed record ExamsSubmitCommand(ExamSubmissionDto Submission) : IRequest<ApplicationResult>;
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

public sealed class ExamsSubmitCommandValidator : AbstractValidator<ExamsSubmitCommand>
{
    public ExamsSubmitCommandValidator()
    {
        RuleFor(x => x.Submission).NotNull();
        When(x => x.Submission != null, () => {
        RuleFor(x => x.Submission.EnrollmentId).NotEmpty();
        RuleFor(x => x.Submission.UserId).NotEmpty();
        RuleFor(x => x.Submission.SelectedAnswers).NotNull();
        });
    }
}
