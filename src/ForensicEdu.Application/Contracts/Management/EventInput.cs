using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Management;

public record EventInput(string Title, string Type, string Description, decimal Price, DateTime EventDate,
    string TimeSlot, string Speaker, string SpeakerBio = "", string RoomUrl = "", string BannerUrl = "", string AccessStatus = "PROGRAMADO");
