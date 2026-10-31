using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Courses.Queries.GetAll;

public sealed class CoursesGetAllQueryHandler(IApplicationDbContext _context) : IRequestHandler<CoursesGetAllQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CoursesGetAllQuery message, CancellationToken cancellationToken)
    {


        var courses = await _context.Courses.Where(c=>c.IsActive && c.IsPublished)
            .OrderBy(c => c.OrderIndex)
            .Select(c=>new {c.Id,c.Title,c.Slug,c.Category,c.AcademicHours,c.Price,c.DiscountPrice,c.ShortCode,c.Code,c.ImageUrl,c.IsPublished,c.IsActive,
                c.ShortDescription,c.FullDescription,c.Instructor,c.InstructorImageUrl,c.InstructorTitle,c.Modalidad,c.Requisitos,
                modules=c.Modules.OrderBy(m=>m.OrderIndex).Select(m=>new {m.Id,m.Title,m.Duration,m.OrderIndex,documents=m.Documents.Select(d=>new {d.Id,d.Title,d.FileType,d.FileSize})})})
            .ToListAsync(cancellationToken);

        return ApplicationResult.Success(courses);
    }
}
