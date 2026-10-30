using System.Security.Claims;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.WebApi.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController(IApplicationDbContext db) : ControllerBase
{
    private static readonly HashSet<string> SocialKeys = new(StringComparer.Ordinal)
        { "Facebook", "Instagram", "YouTube", "TikTok", "LinkedIn", "X", "WhatsApp" };

    [HttpGet("social")]
    public async Task<IActionResult> Social() => Ok(await db.SystemSocialSettings.Where(s => SocialKeys.Contains(s.Key))
        .Select(s => new { s.Key, s.Value }).ToListAsync());

    public record SettingRequest(string Value);

    [Authorize(Roles = "1,2")]
    [HttpPut("{key}")]
    public async Task<IActionResult> Set(string key, SettingRequest request)
    {
        if (key == "ExamRetakeWaitHours")
        {
            if (!int.TryParse(request.Value, out var hours) || hours < 0 || hours > 168) return BadRequest();
        }
        else if (!SocialKeys.Contains(key) || (!string.IsNullOrEmpty(request.Value) &&
            (!Uri.TryCreate(request.Value, UriKind.Absolute, out var uri) || uri.Scheme != "https")))
            return BadRequest(new { message = "Use una red admitida y una URL HTTPS; vacío desactiva el enlace." });
        var setting = await db.SystemSocialSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) { setting = new SystemSocialSetting { Key = key }; db.SystemSocialSettings.Add(setting); }
        setting.Value = request.Value;
        setting.UpdatedAt = DateTime.UtcNow;
        db.AuditLogs.Add(new AuditLog { UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!, Action = "SETTING_CHANGED", EntityType = "SystemSocialSetting", EntityId = setting.Id, Details = key });
        await db.SaveChangesAsync();
        return NoContent();
    }
}
