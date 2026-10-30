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
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetDocumentAccess;

public sealed record GetDocumentAccessQuery(string Id, string DocumentId) : IRequest<ApplicationResult>;
public sealed class GetDocumentAccessQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetDocumentAccessQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetDocumentAccessQuery request, CancellationToken ct)
    {
        var enrollment=await db.Enrollments.AsNoTracking().Include(x=>x.Course).ThenInclude(x=>x!.Modules).ThenInclude(x=>x.Documents).FirstOrDefaultAsync(x=>x.Id==request.Id && x.UserId==current.UserId,ct);
        if(enrollment?.Course==null)return ApplicationResult.NotFound();
        if(enrollment.Status!="ACTIVA" && enrollment.Status!="COMPLETADA")return ApplicationResult.Forbidden();
        if(!await db.Orders.AnyAsync(x=>x.Id==enrollment.OrderId && x.UserId==current.UserId && x.Status==OrderStatus.APROBADO,ct))return ApplicationResult.Forbidden();
        var document=enrollment.Course.Modules.SelectMany(x=>x.Documents).FirstOrDefault(x=>x.Id==request.DocumentId);if(document==null)return ApplicationResult.NotFound();
        if(document.RequireWatermark)return ApplicationResult.Unavailable(new {message="Este documento requiere un servicio de marca de agua todavía no configurado."});
        EndpointSupport.Audit(db,current,"DOCUMENT_ACCESS_GRANTED","CourseDocument",document.Id);await db.SaveChangesAsync(ct);
        return ApplicationResult.Success(new {url=document.StorageUrl,document.Title,document.FileType});
    }
}

public sealed class GetDocumentAccessQueryValidator : AbstractValidator<GetDocumentAccessQuery>
{
    public GetDocumentAccessQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.DocumentId).NotEmpty().MaximumLength(200);
    }
}
