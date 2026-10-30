using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public partial class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = "DNI";
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string PaisResidencia { get; set; } = "Perú";
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string NivelEducativo { get; set; } = "Licenciatura";
    public string InstitucionEducativa { get; set; } = string.Empty;
    public string Profesion { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.ESTUDIANTE;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public AccountStatus Status { get; set; } = AccountStatus.ACTIVO;
    public AccountStatus AccountStatus { get; set; } = AccountStatus.ACTIVO;
    public DateTime? ValidUntil { get; set; } = DateTime.UtcNow.AddDays(30);
    public bool HasApprovedPurchase { get; set; } = false;
    public DateTime? LastPurchaseDate { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<DigitalCertificate> DigitalCertificates { get; set; } = new List<DigitalCertificate>();
}

public class Course
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; } = string.Empty;
    public string ShortCode { get; set; } = "FOR";
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Forense";
    public string ShortDescription { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal DiscountPrice { get; set; }
    public int DurationHours { get; set; }
    public int AcademicHours { get; set; } = 40;
    public int OrderIndex { get; set; }
    public string Instructor { get; set; } = string.Empty;
    public string InstructorTitle { get; set; } = string.Empty;
    public string Modalidad { get; set; } = "Virtual Asincrónico";
    public string Requisitos { get; set; } = "DNI o CE.";
    public string BannerUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string VideoStreamingUrl { get; set; } = string.Empty;
    public string QuestionsJson { get; set; } = "[]";
    public bool IsActive { get; set; } = true;
    public bool IsPublished { get; set; } = true;

    public ICollection<CourseModule> Modules { get; set; } = new List<CourseModule>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}

public class CourseModule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CourseId { get; set; } = string.Empty;
    public Course? Course { get; set; }
    public int OrderIndex { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Duration { get; set; } = "2h 00m";
    public string VideoStreamingUrl { get; set; } = string.Empty;
    public string StreamingId { get; set; } = string.Empty;

    public ICollection<CourseDocument> Documents { get; set; } = new List<CourseDocument>();
}

public class CourseDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ModuleId { get; set; } = string.Empty;
    public CourseModule? Module { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileType { get; set; } = "PDF";
    public string FileSize { get; set; } = "3.5 MB";
    public string StorageUrl { get; set; } = string.Empty;
    public bool RequireWatermark { get; set; } = true;
}

public class LiveEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = "Congreso";
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime EventDate { get; set; }
    public string TimeSlot { get; set; } = "09:00 - 18:00 (GMT-5)";
    public string Speaker { get; set; } = string.Empty;
    public string SpeakerBio { get; set; } = string.Empty;
    public string AccessStatus { get; set; } = "PROGRAMADO";
    public string RoomUrl { get; set; } = string.Empty;
    public string BannerUrl { get; set; } = string.Empty;
}

public class AdvisoryService
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "Asesoría Personalizada Pericial y Consultoría en Juicio Oral";
    public string Specialist { get; set; } = "Cmdte. (R) Manuel Sotomayor Vega";
    public string Specialty { get; set; } = "Perito Oficial Balístico & Criminalista Forense";
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; } = 250.00m;
    public string WhatsappContact { get; set; } = "+51 944 678 901";
}

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderCode { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public ProductType ProductType { get; set; } = ProductType.CURSO;
    public string ProductId { get; set; } = string.Empty;
    public string ProductTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.PENDIENTE_VALIDACION;

    public PaymentEvidence? PaymentEvidence { get; set; }
}

public class PaymentEvidence
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; } = string.Empty;
    public Order? Order { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string OperationNumber { get; set; } = string.Empty;
    public DateTime OperationDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string AccountHolderName { get; set; } = string.Empty;
    public string PaymentFingerprint { get; set; } = string.Empty;
    public string VoucherFileHash { get; set; } = string.Empty;
    public string VoucherSha256Hash { get; set; } = string.Empty;
    public string VoucherFileUrl { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.PENDIENTE;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ValidatedAt { get; set; }
    public string? ValidatedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUserId { get; set; }
}

public class Enrollment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public string CourseId { get; set; } = string.Empty;
    public Course? Course { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int ProgressPercent { get; set; } = 0;
    public string CompletedModulesJson { get; set; } = "[]";
    public string Status { get; set; } = "ACTIVA";
    public ExamStatus ExamStatus { get; set; } = ExamStatus.NO_INICIADA;
    public decimal? FinalGrade { get; set; }
    public decimal? FirstAttemptScore { get; set; }
    public decimal? SecondAttemptScore { get; set; }
    public int AttemptsUsed { get; set; } = 0;
    public DateTime? SecondAttemptAvailableAt { get; set; }
    public string? CertificateId { get; set; }

    public DigitalCertificate? DigitalCertificate { get; set; }
    public ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
}

public class Question
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CourseId { get; set; } = string.Empty;
    public Course? Course { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string OptionsJson { get; set; } = "[]";
    public int CorrectAnswerIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

public class ExamAttempt
{
    public string QuestionsSnapshotJson { get; set; } = "[]";
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EnrollmentId { get; set; } = string.Empty;
    public Enrollment? Enrollment { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public int AttemptNumber { get; set; } = 1;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; } = 20.0m;
    public decimal PassingScore { get; set; } = 13.0m;
    public bool Approved { get; set; }
    public ExamStatus Status { get; set; } = ExamStatus.NO_INICIADA;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? NextAttemptAllowedAt { get; set; }
    public int DurationSeconds { get; set; } = 600;
    public string AnswersJson { get; set; } = "{}";
}

public class DigitalCertificate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EnrollmentId { get; set; } = string.Empty;
    public Enrollment? Enrollment { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public string CourseId { get; set; } = string.Empty;
    public Course? Course { get; set; }
    public string CertificateCode { get; set; } = string.Empty;
    public string QrValidationUrl { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public string StudentFullName { get; set; } = string.Empty;
    public string StudentDni { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public int AcademicHours { get; set; } = 40;
    public decimal FinalGrade { get; set; }
    public CertificateStatus Status { get; set; } = CertificateStatus.VALIDO;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }
}

public class CertificateVerificationLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CertificateCode { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;
    public bool IsValid { get; set; } = true;
}

public class SupportTicket
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TicketCode { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Message { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.ABIERTO;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ResponsesJson { get; set; } = "[]";
}

public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Details { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
}

public class SystemSocialSetting
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
