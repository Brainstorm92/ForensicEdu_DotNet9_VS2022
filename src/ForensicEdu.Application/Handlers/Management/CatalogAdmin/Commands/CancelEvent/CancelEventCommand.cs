using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CancelEvent;

public sealed record CancelEventCommand(string Id) : IRequest<ApplicationResult>;
