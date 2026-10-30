using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Auth;
namespace ForensicEdu.Application.Handlers.Auth.Commands.Register;

public sealed record AuthRegisterCommand(RegisterRequest Request) : IRequest<ApplicationResult>;
