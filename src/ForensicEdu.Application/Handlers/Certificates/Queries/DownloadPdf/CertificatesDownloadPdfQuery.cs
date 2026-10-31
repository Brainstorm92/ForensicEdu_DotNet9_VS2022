using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Certificates.Queries.DownloadPdf;

public sealed record CertificatesDownloadPdfQuery(string Code) : IRequest<ApplicationResult>;
