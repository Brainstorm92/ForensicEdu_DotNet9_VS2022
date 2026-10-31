using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetManagedCourse;

public sealed record GetManagedCourseQuery(string Id) : IRequest<ApplicationResult>;
