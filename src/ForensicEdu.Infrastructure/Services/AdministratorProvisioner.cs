using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Infrastructure.Services;

// Solo CLI local explícito. No modifica cuentas existentes ni expone una ruta pública.
public static class AdministratorProvisioner
{
    public static async Task CreateAsync(IApplicationDbContext db, IPasswordService passwords, string? username, string? password, CancellationToken ct=default)
    {
        if(string.IsNullOrWhiteSpace(username) || !System.Text.RegularExpressions.Regex.IsMatch(username,"^[A-Za-z0-9_-]{3,50}$") || string.IsNullOrWhiteSpace(password) || password.Length<10 || System.Text.Encoding.UTF8.GetByteCount(password)>72)
            throw new InvalidOperationException("Configure AdminProvision__Username y AdminProvision__Password (10 caracteres mínimo, 72 bytes máximo).");
        var name=username.Trim().ToLowerInvariant();
        var existing=await db.Users.SingleOrDefaultAsync(x=>x.Username==name,ct);
        if(existing!=null) {
            if(existing.Role!=UserRole.SUPERADMIN || !passwords.Verify(password,existing.PasswordHash))throw new InvalidOperationException("El usuario ya existe con otra configuración. No se modificó.");
            Console.WriteLine("El superadministrador ya existe; no se modificaron sus credenciales.");return;
        }
        var email=name+"@accounts.invalid";
        if(await db.Users.AnyAsync(x=>x.Email==email,ct))throw new InvalidOperationException("Existe otra cuenta con el identificador reservado. No se modificó.");
        var user=new User {Username=name,Nombres=username.Trim().ToUpperInvariant(),FullName=username.Trim().ToUpperInvariant(),Email=email,PasswordHash=passwords.Hash(password),Role=UserRole.SUPERADMIN,Status=AccountStatus.ACTIVO,AccountStatus=AccountStatus.ACTIVO,ValidUntil=null};
        db.Users.Add(user);db.AuditLogs.Add(new AuditLog{UserId=user.Id,Action="ADMIN_PROVISIONED",EntityType="User",EntityId=user.Id,Details="Alta explícita mediante consola local."});
        await db.SaveChangesAsync(ct);Console.WriteLine("Superadministrador guardado; contraseña almacenada como hash.");
    }
}
