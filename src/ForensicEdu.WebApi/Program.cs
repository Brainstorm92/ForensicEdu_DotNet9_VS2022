using ForensicEdu.Application;
using ForensicEdu.Infrastructure;
using ForensicEdu.Infrastructure.BackgroundJobs;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddCommandLine(args);
}

// Add services to the container
builder.Services.AddControllers();
// Use a per-process key locally; production must supply JwtSettings__Secret.
if (builder.Environment.IsDevelopment())
    builder.Configuration["JwtSettings:Secret"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    throw new InvalidOperationException("Configure JwtSettings__Secret con al menos 32 caracteres.");
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"], ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.FromSeconds(30)
    });
builder.Services.AddAuthorization();
builder.Services.AddHostedService<ForensicEdu.WebApi.ExpiredExamsWorker>();
builder.Services.AddOpenApi();

// Add Clean Architecture Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Hangfire for background scheduled jobs (Account expiration checks at midnight)
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage());
builder.Services.AddHangfireServer();

// Add CORS to communicate directly with React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();
// Return a recoverable service error instead of confusing registration failures.
app.Use(async (context, next) =>
{
    try { await next(); }
    catch (Exception ex) when (!context.Response.HasStarted && DatabaseUnavailable(ex))
    {
        app.Logger.LogWarning("PostgreSQL no disponible. Tipo de fallo: {ErrorType}", ex.GetBaseException().GetType().Name);
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.Headers.RetryAfter = "5";
        await context.Response.WriteAsJsonAsync(new { message = "No se pudo conectar con PostgreSQL. Espere unos segundos e inténtelo de nuevo. No se confirmó el registro; si ya existe, inicie sesión." });
    }
});

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    var dbConnection = new Npgsql.NpgsqlConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
    app.Logger.LogInformation("Destino PostgreSQL: {Host}:{Port}/{Database}", dbConnection.Host, dbConnection.Port, dbConnection.Database);
    app.MapGet("/api/health/database", async (ForensicEdu.Infrastructure.Persistence.ApplicationDbContext db) =>
    {
        try
        {
            var connected = await db.Database.CanConnectAsync();
            return Results.Json(new { connected, dbConnection.Host, dbConnection.Port, dbConnection.Database }, statusCode: connected ? 200 : 503);
        }
        catch { return Results.Json(new { connected = false, dbConnection.Host, dbConnection.Port, dbConnection.Database }, statusCode: 503); }
    });
    app.MapOpenApi();
    app.MapScalarApiReference(); // Modern Scalar UI for .NET 9
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowFrontendApp");
app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard (Admin only in production)
app.MapHangfireDashboard("/hangfire").RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = "1" });

// Register recurring jobs: Daily Account Lifecycle check at 00:00 UTC
RecurringJob.AddOrUpdate<AccountLifecycleHangfireJob>(
    "daily-account-lifecycle-audit",
    job => job.ExecuteDailyLifecycleAuditAsync(),
    Cron.Daily(0, 0)
);

app.MapControllers();
app.MapFallbackToFile("{*path:nonfile}", "index.html");

app.Run();

static bool DatabaseUnavailable(Exception error)
{
    for (Exception? current = error; current != null; current = current.InnerException)
        if (current is Npgsql.NpgsqlException pg && (pg.IsTransient || pg is not Npgsql.PostgresException)) return true;
    return false;
}
