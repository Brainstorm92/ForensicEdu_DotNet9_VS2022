using System.Security.Claims;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.WebApi.Services;

public static class TokenAccountGuard
{
    public static async Task ValidateAsync(TokenValidatedContext context)
    {
        var id=context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var role=context.Principal?.FindFirstValue(ClaimTypes.Role);
        if(string.IsNullOrWhiteSpace(id)){context.Fail("Identidad inválida.");return;}
        var db=context.HttpContext.RequestServices.GetRequiredService<IApplicationDbContext>();
        var resilience=context.HttpContext.RequestServices.GetRequiredService<IDatabaseResilience>();
        var account=await resilience.ExecuteAsync(ct=>db.Users.AsNoTracking().Where(x=>x.Id==id).Select(x=>new {x.Role,x.Status,x.ValidUntil}).FirstOrDefaultAsync(ct),context.HttpContext.RequestAborted);
        if(account==null || role!=((int)account.Role).ToString(System.Globalization.CultureInfo.InvariantCulture) ||
            account.Status is AccountStatus.SUSPENDIDO or AccountStatus.ELIMINADO or AccountStatus.PENDIENTE_DE_ELIMINACION or AccountStatus.VENCIDA ||
            account.ValidUntil<=DateTime.UtcNow)
            context.Fail("La cuenta no tiene acceso vigente o debe iniciar sesión de nuevo.");
    }
}
