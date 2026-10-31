using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Courses.Queries.GetById;

public sealed class CoursesGetByIdQueryHandler(IApplicationDbContext _context) : IRequestHandler<CoursesGetByIdQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CoursesGetByIdQuery message, CancellationToken cancellationToken)
    {
        var id = message.Id;

        var course = await _context.Courses.FirstOrDefaultAsync(c => (c.Id == id || c.Slug == id) && c.IsActive && c.IsPublished);
        if (course == null) return ApplicationResult.NotFound(new { message = "Curso forense no encontrado." });

        return ApplicationResult.Success(new { course.Id, course.Title, course.Slug, course.Price, course.DiscountPrice,
            course.ShortDescription, course.FullDescription, course.Instructor, course.InstructorImageUrl, course.ImageUrl, course.AcademicHours, course.Requisitos, course.Modalidad });
    }
}
