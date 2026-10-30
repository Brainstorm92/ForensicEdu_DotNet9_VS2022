using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateEvent;

public sealed record UpdateEventCommand(string Id, EventInput Input) : IRequest<ApplicationResult>;
