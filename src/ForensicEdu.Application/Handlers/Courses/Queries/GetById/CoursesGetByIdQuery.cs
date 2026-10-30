using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Handlers.Courses.Queries.GetById;
public sealed record CoursesGetByIdQuery(string Id) : IRequest<ApplicationResult>;
public sealed class CoursesGetByIdQueryHandler(IApplicationDbContext _context) : IRequestHandler<CoursesGetByIdQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CoursesGetByIdQuery message, CancellationToken cancellationToken)
    {
        var id = message.Id;

        var course = await _context.Courses.FirstOrDefaultAsync(c => (c.Id == id || c.Slug == id) && c.IsActive && c.IsPublished);
        if (course == null) return ApplicationResult.NotFound(new { message = "Curso forense no encontrado." });

        return ApplicationResult.Success(new { course.Id, course.Title, course.Slug, course.Price, course.DiscountPrice,
            course.ShortDescription, course.FullDescription, course.Instructor, course.AcademicHours, course.Requisitos, course.Modalidad });
    }
}

public sealed class CoursesGetByIdQueryValidator : AbstractValidator<CoursesGetByIdQuery>
{
    public CoursesGetByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().MaximumLength(200);
    }
}
