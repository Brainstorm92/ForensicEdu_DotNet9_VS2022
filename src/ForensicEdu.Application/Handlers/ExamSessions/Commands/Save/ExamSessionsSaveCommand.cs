using MediatR;using System.Text.Json;using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.ExamSessions.Commands.Save;
public sealed record ExamSessionsSaveCommand(string AttemptId,Dictionary<string,JsonElement> Answers,string? Revision=null):IRequest<ApplicationResult>;
