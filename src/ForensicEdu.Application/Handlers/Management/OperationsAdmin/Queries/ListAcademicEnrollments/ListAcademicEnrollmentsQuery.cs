using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListAcademicEnrollments;

public sealed record ListAcademicEnrollmentsQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
