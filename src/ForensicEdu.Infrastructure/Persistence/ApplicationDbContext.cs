using System.Reflection;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseDocument> CourseDocuments => Set<CourseDocument>();
    public DbSet<CourseModule> CourseModules => Set<CourseModule>();
    public DbSet<LiveEvent> LiveEvents => Set<LiveEvent>();
    public DbSet<AdvisoryService> AdvisoryServices => Set<AdvisoryService>();
    public DbSet<AdvisoryCase> AdvisoryCases => Set<AdvisoryCase>();
    public DbSet<AdvisorySession> AdvisorySessions => Set<AdvisorySession>();
    public DbSet<AdvisoryCareHistory> AdvisoryCareHistory => Set<AdvisoryCareHistory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<PaymentEvidence> PaymentEvidences => Set<PaymentEvidence>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
    public DbSet<DigitalCertificate> DigitalCertificates => Set<DigitalCertificate>();
    public DbSet<CertificateVerificationLog> CertificateVerificationLogs => Set<CertificateVerificationLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<StudentNotification> StudentNotifications => Set<StudentNotification>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SystemSocialSetting> SystemSocialSettings => Set<SystemSocialSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<CourseModule>().ToTable("CourseModule");
        modelBuilder.Entity<CourseModule>().Property(m=>m.StreamingId).IsConcurrencyToken();
        modelBuilder.Entity<CourseModule>().Property(m=>m.VideoStreamingUrl).IsConcurrencyToken();
        modelBuilder.Entity<CourseDocument>().ToTable("CourseDocument");

        modelBuilder.Entity<AdvisoryCase>(e=>{
            e.HasIndex(c=>c.OrderId).IsUnique();e.Property(c=>c.Version).IsConcurrencyToken();
            e.HasOne(c=>c.Order).WithMany().HasForeignKey(c=>c.OrderId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c=>c.Student).WithMany().HasForeignKey(c=>c.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c=>c.Consultant).WithMany().HasForeignKey(c=>c.ConsultantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c=>c.AdvisoryService).WithMany().HasForeignKey(c=>c.AdvisoryServiceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<AdvisorySession>().HasOne(s=>s.AdvisoryCase).WithMany(c=>c.Sessions).HasForeignKey(s=>s.AdvisoryCaseId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AdvisoryCareHistory>().HasOne(h=>h.AdvisoryCase).WithMany(c=>c.History).HasForeignKey(h=>h.AdvisoryCaseId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<StudentNotification>(e => {
            e.HasIndex(n => n.EventKey).IsUnique();
            e.HasIndex(n => new { n.UserId, n.CreatedAt });
            e.HasIndex(n => new { n.EmailStatus, n.NextEmailAttemptAt });
            e.Property(n => n.EventKey).HasMaxLength(300);
            e.Property(n => n.Version).IsConcurrencyToken();
            e.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsConcurrencyToken();
            entity.Property(e => e.BirthDate).HasColumnType("date");
            entity.Property(e => e.EmailConfirmationStamp).IsConcurrencyToken();
            entity.Property(e => e.EmailConfirmationSentAt).IsConcurrencyToken();
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.GoogleSubject).HasMaxLength(255);
            entity.HasIndex(e => e.GoogleSubject).IsUnique();
            entity.HasIndex(e => e.Dni);
            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Dni).HasMaxLength(20).IsRequired();
        });

        // Course
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPrice).HasPrecision(18, 2);
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderCode).IsUnique();
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PaymentEvidence with ANTIFRAUD Fingerprint
        modelBuilder.Entity<PaymentEvidence>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PaymentFingerprint);
            entity.HasIndex(e => e.VoucherFileHash);
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.HasOne(e => e.Order)
                .WithOne(o => o.PaymentEvidence)
                .HasForeignKey<PaymentEvidence>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DigitalCertificate
        modelBuilder.Entity<DigitalCertificate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CertificateCode).IsUnique();
            entity.Property(e => e.FinalGrade).HasPrecision(4, 2);

            entity.HasOne(e => e.Enrollment)
                .WithOne(en => en.DigitalCertificate)
                .HasForeignKey<DigitalCertificate>(c => c.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ExamAttempt
        modelBuilder.Entity<ExamAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EnrollmentId, e.AttemptNumber }).IsUnique();
            entity.Property(e => e.CompletedAt).IsConcurrencyToken();
            entity.Property(e => e.Score).HasPrecision(4, 2);
            entity.Property(e => e.PassingScore).HasPrecision(4, 2);
            entity.Property(e => e.MaxScore).HasPrecision(4, 2);

            entity.HasOne(e => e.Enrollment)
                .WithMany(en => en.ExamAttempts)
                .HasForeignKey(e => e.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
