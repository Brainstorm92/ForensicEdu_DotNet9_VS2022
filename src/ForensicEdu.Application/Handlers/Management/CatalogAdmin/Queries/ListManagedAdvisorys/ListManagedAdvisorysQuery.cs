using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.ListManagedAdvisorys;

public sealed record ListManagedAdvisorysQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
