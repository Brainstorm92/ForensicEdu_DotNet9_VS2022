using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public sealed class EventInputValidator : AbstractValidator<EventInput>
{
    public EventInputValidator() {
        RuleFor(x=>x.Title).NotEmpty().MaximumLength(300); RuleFor(x=>x.Type).NotEmpty().MaximumLength(100);
        RuleFor(x=>x.Description).NotNull().MaximumLength(20000); RuleFor(x=>x.Price).InclusiveBetween(0,1000000).PrecisionScale(18,2,true);
        RuleFor(x=>x.EventDate).Must(d=>d != default && d.Kind==DateTimeKind.Utc).WithMessage("Use una fecha UTC, por ejemplo 2026-12-01T14:00:00Z.");
        RuleFor(x=>x.TimeSlot).NotEmpty().MaximumLength(100); RuleFor(x=>x.Speaker).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.SpeakerBio).NotNull().MaximumLength(5000); RuleFor(x=>x.RoomUrl).Must(InputUrls.HttpsOrEmpty);
        RuleFor(x=>x.BannerUrl).Must(InputUrls.HttpsOrEmpty); RuleFor(x=>x.AccessStatus).Must(s=>s is "PROGRAMADO" or "EN_VIVO" or "FINALIZADO" or "CANCELADO");
    }
}
