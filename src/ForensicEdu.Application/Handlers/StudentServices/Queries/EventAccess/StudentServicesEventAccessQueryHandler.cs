using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.StudentServices.Queries.EventAccess;

public sealed class StudentServicesEventAccessQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<StudentServicesEventAccessQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(StudentServicesEventAccessQuery message, CancellationToken cancellationToken)
    {
        var id = message.Id;

        if (!await db.Orders.AnyAsync(o => o.UserId == current.UserId && o.ProductId == id && o.ProductType == ProductType.EVENTO && o.Status == OrderStatus.APROBADO)) return ApplicationResult.Forbidden();
        var item = await db.LiveEvents.FirstOrDefaultAsync(e => e.Id == id);
        if (item == null) return ApplicationResult.NotFound();
        if (item.AccessStatus == "CANCELADO" || item.AccessStatus == "FINALIZADO" || DateTime.UtcNow < item.EventDate.AddMinutes(-15))
            return ApplicationResult.Conflict(new { message = "La sala todavía no está habilitada o el evento ha finalizado." });
        if (string.IsNullOrWhiteSpace(item.RoomUrl)) return ApplicationResult.Unavailable(new { message = "El evento aún no tiene un enlace de acceso configurado. Contacta a soporte o inténtalo más tarde." });
        if (!Uri.TryCreate(item.RoomUrl.Trim(), UriKind.Absolute, out var room) || room.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(room.UserInfo))
            return ApplicationResult.Unavailable(new { message = "El enlace del evento no es válido. Contacta a soporte." });
        return ApplicationResult.Success(new { url = item.RoomUrl.Trim() });
    }
}
