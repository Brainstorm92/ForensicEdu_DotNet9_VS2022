using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetModuleVideo;

public sealed record GetModuleVideoQuery(string Id, string ModuleId) : IRequest<ApplicationResult>;
