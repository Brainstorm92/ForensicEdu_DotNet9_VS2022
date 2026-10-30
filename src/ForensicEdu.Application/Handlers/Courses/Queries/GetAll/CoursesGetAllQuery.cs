using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Handlers.Courses.Queries.GetAll;
public sealed record CoursesGetAllQuery() : IRequest<ApplicationResult>;
public sealed class CoursesGetAllQueryHandler(IApplicationDbContext _context) : IRequestHandler<CoursesGetAllQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CoursesGetAllQuery message, CancellationToken cancellationToken)
    {


        var courses = await _context.Courses.Where(c=>c.IsActive && c.IsPublished)
            .OrderBy(c => c.OrderIndex)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Slug,
                c.Category,
                c.AcademicHours,
                c.Price,
                c.DiscountPrice,
                c.ShortCode,
                c.ImageUrl,
                c.IsPublished
            })
            .ToListAsync(cancellationToken);

        return ApplicationResult.Success(courses);
    }
}
