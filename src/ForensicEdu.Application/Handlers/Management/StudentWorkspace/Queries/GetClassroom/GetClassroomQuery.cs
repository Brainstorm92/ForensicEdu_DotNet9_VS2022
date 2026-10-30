using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetClassroom;

public sealed record GetClassroomQuery(string Id) : IRequest<ApplicationResult>;
