using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Application.Common.Validation;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetModuleVideo;

public sealed record GetModuleVideoQuery(string Id, string ModuleId) : IRequest<ApplicationResult>;
public sealed class GetModuleVideoQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetModuleVideoQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetModuleVideoQuery request, CancellationToken ct)
    {
        var enrollment=await db.Enrollments.AsNoTracking().Include(x=>x.Course).ThenInclude(x=>x!.Modules).ThenInclude(x=>x.Documents).FirstOrDefaultAsync(x=>x.Id==request.Id && x.UserId==current.UserId,ct);
        if(enrollment?.Course==null)return ApplicationResult.NotFound();
        if(enrollment.Status!="ACTIVA" && enrollment.Status!="COMPLETADA")return ApplicationResult.Forbidden();
        if(!await db.Orders.AnyAsync(x=>x.Id==enrollment.OrderId && x.UserId==current.UserId && x.Status==OrderStatus.APROBADO,ct))return ApplicationResult.Forbidden();
        var module=enrollment.Course.Modules.FirstOrDefault(x=>x.Id==request.ModuleId);if(module==null)return ApplicationResult.NotFound();
        if(string.IsNullOrWhiteSpace(module.VideoStreamingUrl))return ApplicationResult.Unavailable(new {message="El video está pendiente de configuración."});
        return ApplicationResult.Success(new {url=module.VideoStreamingUrl, message="Recurso configurado. La emisión de enlaces temporales depende del proveedor de streaming."});
    }
}

public sealed class GetModuleVideoQueryValidator : AbstractValidator<GetModuleVideoQuery>
{
    public GetModuleVideoQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.ModuleId).NotEmpty().MaximumLength(200);
    }
}
