using Microsoft.AspNetCore.RateLimiting;
namespace ForensicEdu.WebApi.Common;
public static class EmailRateLimitExtensions
{
    public static IServiceCollection AddEmailRateLimits(this IServiceCollection services)
    {
        services.AddRateLimiter(options => {
    options.RejectionStatusCode = 429;
    options.OnRejected = async (context, ct) => { context.HttpContext.Response.Headers.RetryAfter = "600"; await context.HttpContext.Response.WriteAsJsonAsync(new { message = "Demasiadas solicitudes. Espere unos minutos antes de volver a intentar." }, ct); };
    options.AddPolicy("password-recovery", context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions { PermitLimit = 8, Window = TimeSpan.FromMinutes(10), QueueLimit = 0 }));
    options.AddPolicy("email-verification", context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(10), QueueLimit = 0 }));
    options.AddPolicy("email-test", context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions { PermitLimit = 3, Window = TimeSpan.FromMinutes(10), QueueLimit = 0 }));
});
        return services;
    }
}
