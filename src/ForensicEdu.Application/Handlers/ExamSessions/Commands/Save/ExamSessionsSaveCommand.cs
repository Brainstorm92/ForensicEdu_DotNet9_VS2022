using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.ExamSessions.Commands.Save;

public sealed record ExamSessionsSaveCommand(string AttemptId, Dictionary<string, int> Answers) : IRequest<ApplicationResult>;
