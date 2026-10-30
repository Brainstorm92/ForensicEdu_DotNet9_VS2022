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

        // Rule 1: Users registered without any purchase for more than 30 days
        var thirtyDaysAgo = now.AddDays(-30);
        var unpaidUsersToFlag = await _context.Users
            .Where(u => u.Role == ForensicEdu.Domain.Enums.UserRole.ESTUDIANTE && !u.HasApprovedPurchase && (u.Status == AccountStatus.ACTIVO || u.Status == AccountStatus.REGISTRADA) && u.RegisteredAt <= thirtyDaysAgo)
            .ToListAsync(cancellationToken);

        foreach (var user in unpaidUsersToFlag)
        {
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
            .Where(u => u.Status == AccountStatus.ACTIVO && u.ValidUntil.HasValue && u.ValidUntil.Value <= now)
            .ToListAsync(cancellationToken);

        foreach (var user in expiredUsers)
        {
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
