using System.Linq;
using System.Threading.Tasks;
using ForensicEdu.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public CoursesController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await _context.Courses
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
            .ToListAsync();

        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id || c.Slug == id);
        if (course == null) return NotFound(new { message = "Curso forense no encontrado." });

        return Ok(new { course.Id, course.Title, course.Slug, course.Price, course.DiscountPrice,
            course.ShortDescription, course.FullDescription, course.Instructor, course.AcademicHours, course.Requisitos, course.Modalidad });
    }
}
