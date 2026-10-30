using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.SetUserRole;

public sealed record SetUserRoleCommand(string Id, UserRoleInput Input) : IRequest<ApplicationResult>;
