using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Settings;
public record SettingRequest(string Value);
public static class SettingsKeys { public static readonly HashSet<string> SocialKeys = new(StringComparer.Ordinal) { "Facebook", "Instagram", "YouTube", "TikTok", "LinkedIn", "X", "WhatsApp" }; }
