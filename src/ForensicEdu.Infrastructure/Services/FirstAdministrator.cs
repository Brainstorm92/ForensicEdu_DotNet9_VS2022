using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Infrastructure.Services;

// Invoked only by an explicit local CLI command, never an unauthenticated endpoint.
public static class FirstAdministrator
{
    public static async Task CreateAsync(IApplicationDbContext db,IPasswordService passwords,string? email,string? password,CancellationToken ct=default)
    {
        if(!System.Net.Mail.MailAddress.TryCreate(email,out _) || string.IsNullOrWhiteSpace(password) || password.Length<12 || System.Text.Encoding.UTF8.GetByteCount(password)>72)
            throw new InvalidOperationException("Configure BootstrapAdmin__Email y BootstrapAdmin__Password (12 caracteres como mínimo y hasta 72 bytes UTF-8).");
        if(await db.Users.AnyAsync(x=>x.Role==UserRole.SUPERADMIN,ct))throw new InvalidOperationException("Ya existe un superadministrador. No se modificó ninguna cuenta.");
        var normalized=email!.Trim().ToLowerInvariant();
        if(await db.Users.AnyAsync(x=>x.Email.ToLower()==normalized,ct))throw new InvalidOperationException("El correo ya pertenece a una cuenta. No se promovió ni modificó esa cuenta.");
        var user=new User {Nombres="Administrador",Apellidos="Principal",FullName="Administrador Principal",Email=normalized,
            PasswordHash=passwords.Hash(password),Role=UserRole.SUPERADMIN,Status=AccountStatus.ACTIVO,AccountStatus=AccountStatus.ACTIVO,ValidUntil=null};
        db.Users.Add(user);db.AuditLogs.Add(new AuditLog {UserId=user.Id,Action="FIRST_ADMIN_CREATED",EntityType="User",EntityId=user.Id,Details="Creación explícita mediante comando local."});
        await db.SaveChangesAsync(ct);
    }
}
