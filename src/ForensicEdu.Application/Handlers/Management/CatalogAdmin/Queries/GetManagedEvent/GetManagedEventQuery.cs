using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetManagedEvent;

public sealed record GetManagedEventQuery(string Id) : IRequest<ApplicationResult>;
