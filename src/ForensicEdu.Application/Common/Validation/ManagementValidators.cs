using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public static class InputUrls
{
    public static bool HttpsOrEmpty(string? value) => value != null && (value.Length == 0 ||
        (value.Length <= 2048 && Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == "https" && string.IsNullOrEmpty(uri.UserInfo)));
}
public sealed class CourseInputValidator : AbstractValidator<CourseInput>
{
    public CourseInputValidator() {
        RuleFor(x=>x.Title).NotEmpty().MaximumLength(300); RuleFor(x=>x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
        RuleFor(x=>x.Category).NotEmpty().MaximumLength(100); RuleFor(x=>x.Price).InclusiveBetween(0,1000000).PrecisionScale(18,2,true);
        RuleFor(x=>x.DiscountPrice).InclusiveBetween(0,1000000).PrecisionScale(18,2,true).LessThanOrEqualTo(x=>x.Price);
        RuleFor(x=>x.AcademicHours).InclusiveBetween(1,10000); RuleFor(x=>x.Instructor).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.ShortDescription).NotEmpty().MaximumLength(1000); RuleFor(x=>x.FullDescription).NotNull().MaximumLength(50000);
        RuleFor(x=>x.ShortCode).NotEmpty().MaximumLength(10).Matches("^[A-Za-z0-9-]+$");
        RuleFor(x=>x.ImageUrl).Must(InputUrls.HttpsOrEmpty); RuleFor(x=>x.Modalidad).NotEmpty().MaximumLength(100);
        RuleFor(x=>x.Requisitos).NotNull().MaximumLength(2000); RuleFor(x=>x.OrderIndex).InclusiveBetween(0,10000);
    }
}
public sealed class ModuleInputValidator : AbstractValidator<ModuleInput>
{
    public ModuleInputValidator() { RuleFor(x=>x.Title).NotEmpty().MaximumLength(300); RuleFor(x=>x.OrderIndex).InclusiveBetween(0,10000);
        RuleFor(x=>x.Duration).NotNull().MaximumLength(100); RuleFor(x=>x.VideoStreamingUrl).Must(InputUrls.HttpsOrEmpty); }
}
public sealed class DocumentInputValidator : AbstractValidator<DocumentInput>
{
    public DocumentInputValidator() { RuleFor(x=>x.Title).NotEmpty().MaximumLength(300); RuleFor(x=>x.StorageUrl).NotEmpty().Must(InputUrls.HttpsOrEmpty);
        RuleFor(x=>x.FileType).Must(x=>x is "PDF" or "DOCX" or "PPTX" or "XLSX"); RuleFor(x=>x.FileSize).NotNull().MaximumLength(50);
        RuleFor(x=>x.RequireWatermark).Equal(false).WithMessage("La generación de marcas de agua aún no está integrada."); }
}
public sealed class QuestionInputValidator : AbstractValidator<QuestionInput>
{
    public QuestionInputValidator() {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(100); RuleFor(x=>x.Question).NotEmpty().MaximumLength(5000);
        RuleFor(x=>x.Options).NotNull().Must(x=>x is { Length: >=2 and <=8 });
        When(x=>x.Options != null,()=>{ RuleForEach(x=>x.Options).NotEmpty().MaximumLength(2000);
            RuleFor(x=>x.CorrectOptionIndex).GreaterThanOrEqualTo(0).LessThan(x=>x.Options.Length); });
        RuleFor(x=>x.Explanation).NotNull().MaximumLength(5000);
    }
}
public sealed class QuestionBankInputValidator : AbstractValidator<QuestionBankInput>
{
    public QuestionBankInputValidator() {
        RuleFor(x=>x.Questions).NotNull().Must(q=>q is { Length: >0 and <=500 });
        When(x=>x.Questions != null,()=>{ RuleForEach(x=>x.Questions).NotNull().SetValidator(new QuestionInputValidator());
            RuleFor(x=>x.Questions).Must(q=>q.All(x=>x != null) && q.Select(x=>x.Id).Distinct().Count()==q.Length).WithMessage("Los identificadores de preguntas deben ser únicos."); });
    }
}
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
public sealed class AdvisoryInputValidator : AbstractValidator<AdvisoryInput>
{
    public AdvisoryInputValidator() { RuleFor(x=>x.Title).NotEmpty().MaximumLength(300); RuleFor(x=>x.Specialist).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Specialty).NotEmpty().MaximumLength(200); RuleFor(x=>x.Description).NotNull().MaximumLength(20000);
        RuleFor(x=>x.Price).InclusiveBetween(0,1000000).PrecisionScale(18,2,true); RuleFor(x=>x.WhatsappContact).NotEmpty().Matches("^\\+?[0-9]{7,15}$"); }
}
public sealed class ProfileInputValidator : AbstractValidator<ProfileInput>
{
    public ProfileInputValidator() { RuleFor(x=>x.Nombres + " " + x.Apellidos).MaximumLength(200); RuleFor(x=>x.Nombres).NotEmpty().MaximumLength(100); RuleFor(x=>x.Apellidos).NotEmpty().MaximumLength(100);
        RuleFor(x=>x.Phone).NotEmpty().MaximumLength(30); RuleFor(x=>x.PaisResidencia).NotEmpty().MaximumLength(100);
        RuleFor(x=>x.NivelEducativo).Must(v=>v is "Pregrado" or "Bachiller" or "Licenciatura" or "Maestría" or "Doctorado");
        RuleFor(x=>x.InstitucionEducativa).NotEmpty().MaximumLength(300); RuleFor(x=>x.Profesion).NotEmpty().MaximumLength(200); }
}
public sealed class PasswordInputValidator : AbstractValidator<PasswordInput>
{
    public PasswordInputValidator() { RuleFor(x=>x.CurrentPassword).NotEmpty().MaximumLength(72);
        RuleFor(x=>x.NewPassword).NotEmpty().MinimumLength(12).Must(p=>p != null && System.Text.Encoding.UTF8.GetByteCount(p)<=72).WithMessage("La contraseña admite hasta 72 bytes UTF-8."); }
}
public sealed class UserStatusInputValidator : AbstractValidator<UserStatusInput>
{
    public UserStatusInputValidator() { RuleFor(x=>x.Status).Must(s=>s is ForensicEdu.Domain.Enums.AccountStatus.SUSPENDIDO or ForensicEdu.Domain.Enums.AccountStatus.ACTIVO); }
}
public sealed class UserRoleInputValidator : AbstractValidator<UserRoleInput>
{
    public UserRoleInputValidator() { RuleFor(x=>x.Role).IsInEnum(); }
}
public sealed class PaymentReviewInputValidator : AbstractValidator<PaymentReviewInput>
{
    public PaymentReviewInputValidator() { RuleFor(x=>x.Status).Must(s=>s is ForensicEdu.Domain.Enums.PaymentStatus.EN_REVISION or ForensicEdu.Domain.Enums.PaymentStatus.OBSERVADO or ForensicEdu.Domain.Enums.PaymentStatus.RECHAZADO);
        RuleFor(x=>x.Reason).NotEmpty().MaximumLength(2000); }
}
public sealed class TicketStatusInputValidator : AbstractValidator<TicketStatusInput>
{
    public TicketStatusInputValidator() { RuleFor(x=>x.Status).IsInEnum(); }
}
