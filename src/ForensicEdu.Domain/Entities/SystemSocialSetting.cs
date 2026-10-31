using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public class SystemSocialSetting
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
