using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.ListManagedEvents;

public sealed record ListManagedEventsQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
