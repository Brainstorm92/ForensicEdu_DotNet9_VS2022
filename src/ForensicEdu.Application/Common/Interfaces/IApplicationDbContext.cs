using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Common.Interfaces;

public interface IApplicationDbContext : IUnitOfWork
{
    DbSet<User> Users { get; }
    DbSet<Course> Courses { get; }
    DbSet<CourseDocument> CourseDocuments { get; }
    DbSet<CourseModule> CourseModules { get; }
    DbSet<LiveEvent> LiveEvents { get; }
    DbSet<AdvisoryService> AdvisoryServices { get; }
    DbSet<Order> Orders { get; }
    DbSet<PaymentEvidence> PaymentEvidences { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<ExamAttempt> ExamAttempts { get; }
    DbSet<DigitalCertificate> DigitalCertificates { get; }
    DbSet<CertificateVerificationLog> CertificateVerificationLogs { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<SupportTicket> SupportTickets { get; }
    DbSet<SystemSocialSetting> SystemSocialSettings { get; }


}
