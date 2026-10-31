using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.CreateUser;
public sealed record CreateUserCommand(CreateUserInput Input):IRequest<ApplicationResult>;
