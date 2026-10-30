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
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateCourse;

public sealed record UpdateCourseCommand(string Id, CourseInput Input) : IRequest<ApplicationResult>;
public sealed class UpdateCourseCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<UpdateCourseCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(UpdateCourseCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var input=request.Input;
        var item=await db.Courses.FirstOrDefaultAsync(x=>x.Id==request.Id,ct); if(item==null)return ApplicationResult.NotFound();
        if(await db.Courses.AnyAsync(x=>x.Slug==input.Slug && x.Id!=item.Id,ct)) return ApplicationResult.Conflict(new {message="El slug ya está registrado."});
        item.Title=input.Title.Trim(); item.Slug=input.Slug; item.Category=input.Category.Trim(); item.Price=input.Price;
        item.DiscountPrice=input.DiscountPrice; item.AcademicHours=input.AcademicHours; item.DurationHours=input.AcademicHours;
        item.Instructor=input.Instructor.Trim(); item.ShortDescription=input.ShortDescription; item.FullDescription=input.FullDescription;
        item.ImageUrl=input.ImageUrl; item.BannerUrl=input.ImageUrl; item.Modalidad=input.Modalidad; item.Requisitos=input.Requisitos;
        item.OrderIndex=input.OrderIndex; item.IsPublished=input.IsPublished; item.IsActive=input.IsActive; item.ShortCode=input.ShortCode.ToUpperInvariant();
        EndpointSupport.Audit(db,current,"COURSE_SAVED","Course",item.Id);
        await db.SaveChangesAsync(ct);
        return ApplicationResult.Success(new {item.Id,item.Title,item.Slug});
    }
}

public sealed class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new CourseInputValidator());
    }
}
