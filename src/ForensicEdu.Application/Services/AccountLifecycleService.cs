using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Services;

public class AccountLifecycleService : IAccountLifecycleService
{
    private readonly IApplicationDbContext _context;

    public AccountLifecycleService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ExtendValidityOnApprovedPaymentAsync(string userId, DateTime paymentDate, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return;

        // Rule: Each approved payment grants/renews 2 years validity from the payment date
        var newValidUntil = paymentDate.AddYears(2);

        user.RenewFromApprovedPayment(paymentDate);
        await StudentNotifications.CancelValidityMail(_context, user.Id, cancellationToken);
        StudentNotifications.Add(_context, user.Id, "renewal:" + user.Id + ":" + paymentDate.Ticks,
            "CUENTA", "Tu cuenta ha sido renovada", $"Tu cuenta tiene vigencia hasta el {newValidUntil:dd/MM/yyyy}.", "perfil");

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "ACCOUNT_VALIDITY_EXTENDED",
            EntityType = "User",
            EntityId = user.Id,
            Details = $"Cuenta renovada por 2 años hasta {newValidUntil:yyyy-MM-dd HH:mm:ss} UTC por compra aprobada.",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ProcessDailyAccountExpirationsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // One reminder for the current 7/3/1-day window, keyed by the actual deadline.
        // Missed audits do not generate a burst of outdated reminders.
        var activeStudents = await _context.Users.Where(u => u.Role == UserRole.ESTUDIANTE &&
            (u.Status == AccountStatus.ACTIVO || u.Status == AccountStatus.REGISTRADA)).ToListAsync(cancellationToken);
        foreach (var student in activeStudents)
        {
            DateTime? deadline = student.HasApprovedPurchase ? student.ValidUntil : student.RegisteredAt.AddDays(30);
            if (!deadline.HasValue || deadline <= now || deadline > now.AddDays(7)) continue;
            var days = Math.Max(1, (int)Math.Ceiling((deadline.Value - now).TotalDays));
            var window = days <= 1 ? 1 : days <= 3 ? 3 : 7;
            var key = $"validity:{student.Id}:{deadline.Value.Ticks}:{window}";
            if (await _context.StudentNotifications.AnyAsync(n => n.EventKey == key, cancellationToken)) continue;
            var body = student.HasApprovedPurchase
                ? $"Tu cuenta vence el {deadline:dd/MM/yyyy}. Renueva mediante una compra con pago aprobado para continuar con tu formación."
                : $"Tu periodo inicial de 30 días sin compras vence el {deadline:dd/MM/yyyy}. Realiza una compra y acredita el pago para activar la vigencia de dos años.";
            StudentNotifications.Add(_context, student.Id, key, "VIGENCIA", $"Tu cuenta vence en {days} {(days == 1 ? "día" : "días")}", body, "catalogo", deadline);
        }

        // Rule 1: Users registered without any purchase for more than 30 days
        var thirtyDaysAgo = now.AddDays(-30);
        var unpaidUsersToFlag = await _context.Users
            .Where(u => u.Role == ForensicEdu.Domain.Enums.UserRole.ESTUDIANTE && !u.HasApprovedPurchase && (u.Status == AccountStatus.ACTIVO || u.Status == AccountStatus.REGISTRADA) && u.RegisteredAt <= thirtyDaysAgo)
            .ToListAsync(cancellationToken);

        foreach (var user in unpaidUsersToFlag)
        {
            await StudentNotifications.CancelValidityMail(_context, user.Id, cancellationToken);
            StudentNotifications.Add(_context, user.Id, "unpaid-expired:" + user.Id + ":" + user.RegisteredAt.Ticks,
                "CUENTA", "Tu periodo inicial ha finalizado", "Han transcurrido 30 días sin una compra aprobada. Tu cuenta quedó pendiente de eliminación. Contacta a soporte para revisar tu situación.", "mis-consultas");
            user.Status = AccountStatus.PENDIENTE_DE_ELIMINACION;
            user.AccountStatus = user.Status;

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = user.Id,
                Action = "ACCOUNT_MARKED_FOR_DELETION",
                EntityType = "User",
                EntityId = user.Id,
                Details = $"Usuario sin compras durante más de 30 días naturales. Estado actualizado a {AccountStatus.PENDIENTE_DE_ELIMINACION}.",
                CreatedAt = now
            });
        }

        // Rule 2: Active users whose 2-year validity expired
        var expiredUsers = await _context.Users
            .Where(u => u.Role == UserRole.ESTUDIANTE && u.HasApprovedPurchase && u.Status == AccountStatus.ACTIVO && u.ValidUntil.HasValue && u.ValidUntil.Value <= now)
            .ToListAsync(cancellationToken);

        foreach (var user in expiredUsers)
        {
            await StudentNotifications.CancelValidityMail(_context, user.Id, cancellationToken);
            StudentNotifications.Add(_context, user.Id, "validity-expired:" + user.Id + ":" + user.ValidUntil!.Value.Ticks,
                "CUENTA", "La vigencia de tu cuenta ha finalizado", $"Tu cuenta venció el {user.ValidUntil:dd/MM/yyyy} y quedó suspendida temporalmente. Contacta a soporte para revisar la renovación.", "mis-consultas");
            user.Status = AccountStatus.SUSPENDIDO;
            user.AccountStatus = user.Status;

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = user.Id,
                Action = "ACCOUNT_EXPIRED_2_YEARS",
                EntityType = "User",
                EntityId = user.Id,
                Details = $"Vigencia de 2 años expirada ({user.ValidUntil:yyyy-MM-dd}). Cuenta suspendida temporalmente.",
                CreatedAt = now
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
