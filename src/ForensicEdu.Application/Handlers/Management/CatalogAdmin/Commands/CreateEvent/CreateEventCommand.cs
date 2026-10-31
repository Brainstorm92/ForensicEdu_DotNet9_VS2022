using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateEvent;

public sealed record CreateEventCommand(EventInput Input) : IRequest<ApplicationResult>;
