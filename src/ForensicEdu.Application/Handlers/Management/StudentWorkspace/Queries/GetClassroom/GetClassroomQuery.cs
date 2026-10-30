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
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetClassroom;

public sealed record GetClassroomQuery(string Id) : IRequest<ApplicationResult>;
public sealed class GetClassroomQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetClassroomQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetClassroomQuery request, CancellationToken ct)
    {
        var enrollment=await db.Enrollments.AsNoTracking().Include(x=>x.Course).ThenInclude(x=>x!.Modules).ThenInclude(x=>x.Documents).FirstOrDefaultAsync(x=>x.Id==request.Id && x.UserId==current.UserId,ct);
        if(enrollment?.Course==null)return ApplicationResult.NotFound();
        if(enrollment.Status!="ACTIVA" && enrollment.Status!="COMPLETADA")return ApplicationResult.Forbidden();
        if(!await db.Orders.AnyAsync(x=>x.Id==enrollment.OrderId && x.UserId==current.UserId && x.Status==OrderStatus.APROBADO,ct))return ApplicationResult.Forbidden();
        return ApplicationResult.Success(new {enrollment.Id,enrollment.CourseId,enrollment.ProgressPercent,enrollment.ExamStatus,enrollment.AttemptsUsed,enrollment.FinalGrade,
            modules=enrollment.Course.Modules.OrderBy(m=>m.OrderIndex).Select(m=>new {m.Id,m.Title,m.Duration,m.OrderIndex,hasVideo=m.VideoStreamingUrl!="",documents=m.Documents.Select(d=>new {d.Id,d.Title,d.FileType,d.FileSize})}) });
    }
}

public sealed class GetClassroomQueryValidator : AbstractValidator<GetClassroomQuery>
{
    public GetClassroomQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
