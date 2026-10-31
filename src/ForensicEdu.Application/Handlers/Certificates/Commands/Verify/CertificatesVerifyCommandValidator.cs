using FluentValidation;
namespace ForensicEdu.Application.Handlers.Certificates.Commands.Verify;

public sealed class CertificatesVerifyCommandValidator : AbstractValidator<CertificatesVerifyCommand>
{
    public CertificatesVerifyCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(200);
    }
}
