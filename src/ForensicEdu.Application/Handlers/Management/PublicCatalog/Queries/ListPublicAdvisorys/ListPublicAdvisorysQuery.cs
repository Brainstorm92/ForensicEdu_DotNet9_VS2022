using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.ListPublicAdvisorys;

public sealed record ListPublicAdvisorysQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
