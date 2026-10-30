using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Exams.Commands.Submit;

public sealed record ExamsSubmitCommand(ExamSubmissionDto Submission) : IRequest<ApplicationResult>;
