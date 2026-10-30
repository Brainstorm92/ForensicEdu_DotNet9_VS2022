using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Auth;
namespace ForensicEdu.Application.Handlers.Auth.Commands.Login;

public sealed record AuthLoginCommand(LoginRequest Request) : IRequest<ApplicationResult>;
