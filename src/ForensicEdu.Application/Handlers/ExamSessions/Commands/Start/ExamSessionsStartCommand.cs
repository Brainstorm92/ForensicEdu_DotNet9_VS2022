using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.ExamSessions.Commands.Start;

public sealed record ExamSessionsStartCommand(string EnrollmentId) : IRequest<ApplicationResult>;
