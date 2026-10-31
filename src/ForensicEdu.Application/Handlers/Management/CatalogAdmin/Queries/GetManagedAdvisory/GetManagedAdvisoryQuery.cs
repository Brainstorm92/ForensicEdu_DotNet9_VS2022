using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetManagedAdvisory;

public sealed record GetManagedAdvisoryQuery(string Id) : IRequest<ApplicationResult>;
