using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public static class InputUrls
{
    public static bool ImageOrEmpty(string? value) => HttpsOrEmpty(value) || (value != null && System.Text.RegularExpressions.Regex.IsMatch(value,@"^/assets/[a-zA-Z0-9/_-]+\.(?:png|jpg|jpeg|webp)$")) || (value != null && System.Text.RegularExpressions.Regex.IsMatch(value,@"^/api/media/images/[a-f0-9]{32}$"));
    public static bool HttpsOrEmpty(string? value) => value != null && (value.Length == 0 ||
        (value.Length <= 2048 && Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == "https" && string.IsNullOrEmpty(uri.UserInfo)));
}
