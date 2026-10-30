using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public class Course
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; } = string.Empty;
    public string ShortCode { get; set; } = "FOR";
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Forense";
    public string ShortDescription { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal DiscountPrice { get; set; }
    public int DurationHours { get; set; }
    public int AcademicHours { get; set; } = 40;
    public int OrderIndex { get; set; }
    public string Instructor { get; set; } = string.Empty;
    public string InstructorImageUrl { get; set; } = "";
    public string InstructorTitle { get; set; } = string.Empty;
    public string Modalidad { get; set; } = "Virtual Asincrónico";
    public string Requisitos { get; set; } = "DNI o CE.";
    public string BannerUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string VideoStreamingUrl { get; set; } = string.Empty;
    public string QuestionsJson { get; set; } = "[]";
    public bool IsActive { get; set; } = true;
    public bool IsPublished { get; set; } = true;

    public ICollection<CourseModule> Modules { get; set; } = new List<CourseModule>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
