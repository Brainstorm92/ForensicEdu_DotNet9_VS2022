using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.ListPublicEvents;

public sealed record ListPublicEventsQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
