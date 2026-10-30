using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.GetPublicAdvisory;

public sealed record GetPublicAdvisoryQuery(string Id) : IRequest<ApplicationResult>;
