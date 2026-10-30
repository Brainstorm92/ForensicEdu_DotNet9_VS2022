using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.AccountManagement.Commands.ChangePassword;

public sealed record ChangePasswordCommand(PasswordInput Input) : IRequest<ApplicationResult>;
