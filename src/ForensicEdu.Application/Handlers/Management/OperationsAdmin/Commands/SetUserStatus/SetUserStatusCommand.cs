using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.SetUserStatus;

public sealed record SetUserStatusCommand(string Id, UserStatusInput Input) : IRequest<ApplicationResult>;
