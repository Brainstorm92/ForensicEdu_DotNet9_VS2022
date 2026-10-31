using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Admin;
namespace ForensicEdu.Application.Handlers.Admin.Commands.RevokeCertificate;

public sealed record AdminRevokeCertificateCommand(string Code, RevokeRequest Req) : IRequest<ApplicationResult>;
