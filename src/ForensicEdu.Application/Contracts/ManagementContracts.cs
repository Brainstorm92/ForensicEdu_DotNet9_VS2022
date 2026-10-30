using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Management;

public record CourseInput(string Title, string Slug, string Category, decimal Price, int AcademicHours,
    string Instructor, string ShortDescription, string FullDescription = "", decimal DiscountPrice = 0,
    string ImageUrl = "", string Modalidad = "Virtual asincrónico", string Requisitos = "", int OrderIndex = 0,
    bool IsPublished = false, bool IsActive = true, string ShortCode = "FOR");
public record ModuleInput(string Title, int OrderIndex, string Duration = "", string VideoStreamingUrl = "");
public record DocumentInput(string Title, string StorageUrl, string FileType = "PDF", string FileSize = "", bool RequireWatermark = false);
public record QuestionInput(string Id, string Question, string[] Options, int CorrectOptionIndex, string Explanation = "");
public record QuestionBankInput(QuestionInput[] Questions);
public record EventInput(string Title, string Type, string Description, decimal Price, DateTime EventDate,
    string TimeSlot, string Speaker, string SpeakerBio = "", string RoomUrl = "", string BannerUrl = "", string AccessStatus = "PROGRAMADO");
public record AdvisoryInput(string Title, string Specialist, string Specialty, string Description, decimal Price, string WhatsappContact);
public record ProfileInput(string Nombres, string Apellidos, string Phone, string PaisResidencia,
    string NivelEducativo, string InstitucionEducativa, string Profesion);
public record PasswordInput(string CurrentPassword, string NewPassword);
public record UserStatusInput(AccountStatus Status);
public record UserRoleInput(UserRole Role);
public record PaymentReviewInput(PaymentStatus Status, string Reason);
public record TicketStatusInput(TicketStatus Status);
public record PageResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
