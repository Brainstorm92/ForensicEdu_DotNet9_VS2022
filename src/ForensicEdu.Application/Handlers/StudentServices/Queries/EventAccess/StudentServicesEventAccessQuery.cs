using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.StudentServices;

namespace ForensicEdu.Application.Handlers.StudentServices.Queries.EventAccess;
public sealed record StudentServicesEventAccessQuery(string Id) : IRequest<ApplicationResult>;
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
        if (string.IsNullOrWhiteSpace(item.RoomUrl)) return ApplicationResult.Unavailable( new { message = "La sala está pendiente de configuración." });
        return ApplicationResult.Success(new { url = item.RoomUrl });
    }
}

public sealed class StudentServicesEventAccessQueryValidator : AbstractValidator<StudentServicesEventAccessQuery>
{
    public StudentServicesEventAccessQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().MaximumLength(200);
    }
}
