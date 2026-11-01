using System.Security.Cryptography;
using System.Text;
using ForensicEdu.Domain.Entities;
namespace ForensicEdu.Application.Common.Management;
public static class PrivateVideos
{
 public const long MaxBytes=2147483648;
 public static bool Key(string? key,bool staging=false){var prefix=staging?"video-uploads/":"videos/";return key!=null&&key.StartsWith(prefix,StringComparison.Ordinal)&&key.EndsWith(".mp4",StringComparison.Ordinal)&&Guid.TryParseExact(key[prefix.Length..^4],"N",out _);}
 public static string Source(CourseModule m)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(m.StreamingId+"\n"+m.VideoStreamingUrl)));
}
