using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace ForensicEdu.Infrastructure.Services;

public sealed class NotificationEmailDispatcher(ApplicationDbContext db, IPlatformEmail email, ILogger<NotificationEmailDispatcher> logger)
{
    public async Task DispatchAsync(CancellationToken ct=default)
    {
        var config=await email.GetConfigurationAsync(ct);
        if(!config.Enabled || !config.HasPassword || string.IsNullOrWhiteSpace(config.FrontendUrl))return;
        var now=DateTime.UtcNow;
        var ids=await db.StudentNotifications.AsNoTracking().Where(n=>
            (n.EmailStatus=="PENDING" || n.EmailStatus=="SENDING") &&
            (n.NextEmailAttemptAt==null || n.NextEmailAttemptAt<=now))
            .OrderBy(n=>n.CreatedAt).Select(n=>n.Id).Take(50).ToListAsync(ct);
        foreach(var id in ids)
        {
            ct.ThrowIfCancellationRequested();
            var n=await db.StudentNotifications.Include(n=>n.User).SingleAsync(n=>n.Id==id,ct);
            if(n.EmailStatus!="PENDING" && n.EmailStatus!="SENDING")continue;
            if(n.NextEmailAttemptAt>DateTime.UtcNow)continue;
            if(n.EmailExpiresAt<=DateTime.UtcNow || n.User==null || n.User.Status==AccountStatus.ELIMINADO)
            { n.EmailStatus="CANCELLED";n.Version=Guid.NewGuid().ToString();await Save(ct);continue; }
            // Do not send educational notices to an address that has not been verified yet.
            if(n.User.EmailConfirmationRequired && n.User.EmailConfirmedAt==null)
            { n.NextEmailAttemptAt=DateTime.UtcNow.AddHours(1);await Save(ct);continue; }
            // A versioned lease prevents two workers from sending the same row concurrently.
            n.EmailStatus="SENDING";n.NextEmailAttemptAt=DateTime.UtcNow.AddMinutes(10);
            n.Version=Guid.NewGuid().ToString();
            if(!await Save(ct))continue;
            try
            {
                var tab=n.ActionTab is "mis-cursos" or "mis-eventos" or "asesoria" or "mis-consultas" or "perfil" or "catalogo" ? n.ActionTab : "notificaciones";
                var url=config.FrontendUrl.TrimEnd('/') + "/#section=" + Uri.EscapeDataString(tab);
                await email.SendMessageAsync(n.User.Email,n.Title+" — Academia Forense",
                    new PlatformEmailMessage(n.Title,n.Body,"Ir a la plataforma",url,Category:n.Kind switch {
                        "MATRICULA"=>"COMPRA Y MATRÍCULA", "SOPORTE"=>"ATENCIÓN Y SOPORTE",_=>"VIGENCIA DE CUENTA"}),ct);
                n.EmailStatus="SENT";n.EmailSentAt=DateTime.UtcNow;n.NextEmailAttemptAt=null;
            }
            catch(Exception e) when(e is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                n.EmailAttempts++;
                n.EmailStatus=n.EmailAttempts>=5?"FAILED":"PENDING";
                n.NextEmailAttemptAt=DateTime.UtcNow.AddMinutes(Math.Min(240,5*Math.Pow(2,n.EmailAttempts)));
                logger.LogWarning("Aviso de correo pendiente; tipo de fallo {Type}. No se registran destinatarios ni contenido.",e.GetType().Name);
            }
            n.Version=Guid.NewGuid().ToString();await Save(ct);
        }
    }
    private async Task<bool> Save(CancellationToken ct)
    {
        try{await db.SaveChangesAsync(ct);return true;}
        catch(DbUpdateConcurrencyException){db.ChangeTracker.Clear();return false;}
    }
}
