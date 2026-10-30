using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.ListManagedCourses;

public sealed record ListManagedCoursesQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
