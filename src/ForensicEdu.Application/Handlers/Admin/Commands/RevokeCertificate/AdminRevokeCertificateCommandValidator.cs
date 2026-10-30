using FluentValidation;
namespace ForensicEdu.Application.Handlers.Admin.Commands.RevokeCertificate;

public sealed class AdminRevokeCertificateCommandValidator : AbstractValidator<AdminRevokeCertificateCommand>
{
    public AdminRevokeCertificateCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Req).NotNull();
        When(x => x.Req != null, () => { RuleFor(x => x.Req.Reason).NotEmpty().MaximumLength(2000); });
    }
}
