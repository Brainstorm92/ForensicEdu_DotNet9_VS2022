using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public class CourseModule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CourseId { get; set; } = string.Empty;
    public Course? Course { get; set; }
    public int OrderIndex { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Duration { get; set; } = "2h 00m";
    public string VideoStreamingUrl { get; set; } = string.Empty;
    public string StreamingId { get; set; } = string.Empty;

    public ICollection<CourseDocument> Documents { get; set; } = new List<CourseDocument>();
}
