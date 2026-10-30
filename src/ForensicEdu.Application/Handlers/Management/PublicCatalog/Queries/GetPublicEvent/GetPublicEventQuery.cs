using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.GetPublicEvent;

public sealed record GetPublicEventQuery(string Id) : IRequest<ApplicationResult>;
