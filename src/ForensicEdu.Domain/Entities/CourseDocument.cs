using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public class CourseDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ModuleId { get; set; } = string.Empty;
    public CourseModule? Module { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileType { get; set; } = "PDF";
    public string FileSize { get; set; } = "3.5 MB";
    public string StorageUrl { get; set; } = string.Empty;
    public bool RequireWatermark { get; set; } = true;
}
