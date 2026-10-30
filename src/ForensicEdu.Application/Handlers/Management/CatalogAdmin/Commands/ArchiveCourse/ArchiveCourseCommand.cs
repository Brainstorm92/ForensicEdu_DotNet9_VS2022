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
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.ArchiveCourse;

public sealed record ArchiveCourseCommand(string Id) : IRequest<ApplicationResult>;
public sealed class ArchiveCourseCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ArchiveCourseCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ArchiveCourseCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var item=await db.Courses.FirstOrDefaultAsync(x=>x.Id==request.Id,ct); if(item==null)return ApplicationResult.NotFound();
        item.IsActive=false;item.IsPublished=false; EndpointSupport.Audit(db,current,"COURSE_ARCHIVED","Course",item.Id);
        await db.SaveChangesAsync(ct); return ApplicationResult.NoContent();
    }
}

public sealed class ArchiveCourseCommandValidator : AbstractValidator<ArchiveCourseCommand>
{
    public ArchiveCourseCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
