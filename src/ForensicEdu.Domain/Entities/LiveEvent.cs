using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public class LiveEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = "Congreso";
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime EventDate { get; set; }
    public string TimeSlot { get; set; } = "09:00 - 18:00 (GMT-5)";
    public string Speaker { get; set; } = string.Empty;
    public string SpeakerBio { get; set; } = string.Empty;
    public string AccessStatus { get; set; } = "PROGRAMADO";
    public string RoomUrl { get; set; } = string.Empty;
    public string BannerUrl { get; set; } = string.Empty;
}
