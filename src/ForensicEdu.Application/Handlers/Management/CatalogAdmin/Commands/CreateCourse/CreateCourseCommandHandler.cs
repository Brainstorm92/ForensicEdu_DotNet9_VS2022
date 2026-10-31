using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateCourse;

public sealed class CreateCourseCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<CreateCourseCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CreateCourseCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var input=request.Input;
        var item=new Course(); db.Courses.Add(item);
        if(await db.Courses.AnyAsync(x=>x.Slug==input.Slug && x.Id!=item.Id,ct)) return ApplicationResult.Conflict(new {message="El slug ya está registrado."});
        item.Title=input.Title.Trim(); item.Slug=input.Slug; item.Category=input.Category.Trim(); item.Price=input.Price;
        item.DiscountPrice=input.DiscountPrice; item.AcademicHours=input.AcademicHours; item.DurationHours=input.AcademicHours;
        item.Instructor=input.Instructor.Trim(); item.ShortDescription=input.ShortDescription; item.FullDescription=input.FullDescription;
        item.InstructorImageUrl=input.InstructorImageUrl; item.ImageUrl=input.ImageUrl; item.BannerUrl=input.ImageUrl; item.Modalidad=input.Modalidad; item.Requisitos=input.Requisitos;
        item.OrderIndex=input.OrderIndex; item.IsPublished=input.IsPublished; item.IsActive=input.IsActive; item.ShortCode=input.ShortCode.ToUpperInvariant();
        EndpointSupport.Audit(db,current,"COURSE_SAVED","Course",item.Id);
        await db.SaveChangesAsync(ct);
        return ApplicationResult.Success(new {item.Id,item.Title,item.Slug});
    }
}
