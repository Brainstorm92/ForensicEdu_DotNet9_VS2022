using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Management;

public record CourseInput(string Title, string Slug, string Category, decimal Price, int AcademicHours,
    string Instructor, string ShortDescription, string FullDescription = "", decimal DiscountPrice = 0,
    string ImageUrl = "", string Modalidad = "Virtual asincrónico", string Requisitos = "", int OrderIndex = 0,
    bool IsPublished = false, bool IsActive = true, string ShortCode = "FOR", string InstructorImageUrl = "");
