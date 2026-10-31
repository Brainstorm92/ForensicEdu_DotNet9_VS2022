using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetMyExamHistory;

public sealed record GetMyExamHistoryQuery(string Id) : IRequest<ApplicationResult>;
