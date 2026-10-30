using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Certificates.Commands.Verify;

public sealed record CertificatesVerifyCommand(string Code, string Ip, string UserAgent) : IRequest<ApplicationResult>;
