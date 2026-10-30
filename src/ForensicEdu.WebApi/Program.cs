using ForensicEdu.WebApi.Common;
using ForensicEdu.WebApi.Services;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application;
using ForensicEdu.Infrastructure;
using ForensicEdu.Infrastructure.BackgroundJobs;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var startupArgs=args.Where(x=>x!="--bootstrap-admin").ToArray();
var builder = WebApplication.CreateBuilder(startupArgs);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddCommandLine(startupArgs);
}

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
// Use a per-process key locally; production must supply JwtSettings__Secret.
if (builder.Environment.IsDevelopment())
    builder.Configuration["JwtSettings:Secret"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    throw new InvalidOperationException("Configure JwtSettings__Secret con al menos 32 caracteres.");
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"], ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
    options.Events=new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents {OnTokenValidated=TokenAccountGuard.ValidateAsync};
    });
builder.Services.AddAuthorization();
builder.Services.AddHostedService<ExpiredExamsWorker>();
builder.Services.AddOpenApi(options=>options.AddDocumentTransformer((document,context,ct)=>{
    document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();
    document.Components.SecuritySchemes["Bearer"]=new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
        Type=Microsoft.OpenApi.Models.SecuritySchemeType.Http,Scheme="bearer",BearerFormat="JWT",Description="Token obtenido en POST /api/auth/login." };
    return Task.CompletedTask;
}));
builder.Services.AddOpenApi(options=>options.AddOperationTransformer((operation,context,ct)=>{
    if(context.Description.ActionDescriptor.EndpointMetadata.OfType<Microsoft.AspNetCore.Authorization.IAuthorizeData>().Any())
        operation.Security.Add(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
            [new Microsoft.OpenApi.Models.OpenApiSecurityScheme {Reference=new Microsoft.OpenApi.Models.OpenApiReference {Type=Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,Id="Bearer"}}]=Array.Empty<string>() });
    return Task.CompletedTask;
}));

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
if(args.Contains("--bootstrap-admin"))
{
    using var scope=app.Services.CreateScope();
    await ForensicEdu.Infrastructure.Services.FirstAdministrator.CreateAsync(
        scope.ServiceProvider.GetRequiredService<IApplicationDbContext>(),scope.ServiceProvider.GetRequiredService<IPasswordService>(),
        builder.Configuration["BootstrapAdmin:Email"],builder.Configuration["BootstrapAdmin:Password"]);
    Console.WriteLine("Primer administrador creado. Inicie sesión mediante /api/auth/login.");
    return;
}
app.UseExceptionHandler();

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
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.UseHttpsRedirection();
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
// Keep the SPA separate from API and documentation routes.

app.MapFallbackToFile("/app/{*path:nonfile}", "index.html");

app.Run();
