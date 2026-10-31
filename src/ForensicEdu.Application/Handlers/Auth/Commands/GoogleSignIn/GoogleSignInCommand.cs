using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Auth;
namespace ForensicEdu.Application.Handlers.Auth.Commands.GoogleSignIn;
public sealed record GoogleSignInCommand(GoogleSignInInput Input,string Nonce):IRequest<ApplicationResult>;
