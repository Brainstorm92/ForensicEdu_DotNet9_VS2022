using ForensicEdu.Infrastructure.Media;
using ForensicEdu.Application.Handlers.Media.Commands.UploadImage;
using ForensicEdu.Application.Handlers.Media.Queries.GetImage;
using ForensicEdu.Infrastructure.Resilience;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using FluentValidation;
using ForensicEdu.Application;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Auth;
using ForensicEdu.Application.Contracts.Orders;
using ForensicEdu.Application.Contracts.Settings;
using ForensicEdu.Application.Contracts.Admin;
using ForensicEdu.Application.Handlers.Auth.Commands.Register;
using ForensicEdu.Application.Handlers.Auth.Commands.Login;
using ForensicEdu.Application.Handlers.Courses.Queries.GetById;
using ForensicEdu.Application.Handlers.Orders.Commands.CreateOrderAndSubmitPayment;
using ForensicEdu.Application.Handlers.Orders.Commands.UploadVoucher;
using ForensicEdu.Application.Handlers.Orders.Queries.GetUserOrders;
using ForensicEdu.Application.Handlers.Admin.Commands.ApprovePayment;
using ForensicEdu.Application.Handlers.Admin.Commands.RevokeCertificate;
using ForensicEdu.Application.Handlers.Admin.Queries.GetAuditLogs;
using ForensicEdu.Application.Handlers.Settings.Commands.Set;
using ForensicEdu.Application.Handlers.ExamSessions.Commands.Save;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

void Check(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine("OK: " + message); }
var assembly = typeof(ForensicEdu.Application.DependencyInjection).Assembly;
Check(!assembly.GetReferencedAssemblies().Any(a => a.Name!.StartsWith("Microsoft.AspNetCore") || a.Name == "Npgsql"), "Application no depende de HTTP ni Npgsql");
var requests = assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IRequest<ApplicationResult>).IsAssignableFrom(t)).ToArray();
Check(requests.Length == 83, "83 comandos y consultas de MediatR descubiertos");

var services = new ServiceCollection();
services.AddLogging(); services.AddApplicationServices();
services.AddSingleton<IGoogleIdentityValidator,DisabledGoogle>();
services.AddSingleton<IEmailConfirmation,TestConfirmation>();
services.AddDatabaseResilience(new ConfigurationBuilder().Build());
services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
services.AddScoped<ICoursePurchaseGate,CoursePurchaseGate>();
services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
services.AddScoped<IUnitOfWork>(p => p.GetRequiredService<ApplicationDbContext>());
var actor = new TestUser(); services.AddSingleton<ICurrentUser>(actor);
services.AddScoped<IPasswordService, PasswordService>(); services.AddScoped<IPersistenceErrors, PersistenceErrors>();
services.AddScoped<ITokenService, TestToken>(); services.AddScoped<IVoucherStorage, TestStorage>();
services.AddScoped<IPdfCertificateService, TestPdf>();
services.AddSingleton<IDocumentStorage,ForensicEdu.Infrastructure.Documents.RustFsDocumentStorage>();services.AddSingleton<IDocumentProcessor,ForensicEdu.Infrastructure.Documents.OfficeDocumentProcessor>();services.Configure<RustFsOptions>(_=>{});
services.AddSingleton<IImageStorage,TestImageStorage>();services.AddSingleton<IImageProcessor,WebImageProcessor>();
using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
using var scope = provider.CreateScope(); var sender = scope.ServiceProvider.GetRequiredService<ISender>();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
foreach (var request in requests) Check(scope.ServiceProvider.GetService(typeof(IRequestHandler<,>).MakeGenericType(request, typeof(ApplicationResult))) is not null, "Handler registrado: " + request.Name);

async Task Reject(IRequest<ApplicationResult> request, string message)
{
    try { await sender.Send(request); } catch (ValidationException) { Console.WriteLine("OK: " + message); return; }
    throw new Exception(message);
}
await Reject(new AuthRegisterCommand(new RegisterRequest("", "bad", "", "", "123")), "Pipeline rechaza registro inválido antes de persistir");
Check(!await db.Users.AnyAsync(), "Registro inválido no escribe usuarios");
await Reject(new AuthRegisterCommand(null!), "Pipeline maneja DTO nulo sin NullReferenceException");
await Reject(new AdminGetAuditLogsQuery(-1), "Pipeline limita tamaño de consultas de auditoría");
await Reject(new SettingsSetCommand("X", new SettingRequest(null!)), "Pipeline rechaza valores nulos de configuración");
await Reject(new ExamSessionsSaveCommand("attempt", null!), "Pipeline rechaza respuestas nulas de examen");
await Reject(new AdminRevokeCertificateCommand("CF-1", new RevokeRequest("")), "Pipeline exige motivo de revocación");
var registration = new RegisterRequest("Ana Forense", "ana@example.invalid", "12345678", "999888777", "ExamplePassword123!",
    "Ana", "Forense", "DNI", "Perú", "Licenciatura", "Instituto de prueba", "Perito", true, true, new DateOnly(1996,2,29));
var registered = await sender.Send(new AuthRegisterCommand(registration));
Check(registered.Outcome == ResultOutcome.Success && await db.Users.CountAsync() == 1, "Registro real mediante ISender y handler");
var student = await db.Users.SingleAsync(); actor.UserId = student.Id;
Check(student.PasswordHash != registration.Password && new PasswordService().Verify(registration.Password, student.PasswordHash), "Contraseña pasa por adaptador BCrypt");
Check((await sender.Send(new AuthLoginCommand(new LoginRequest(registration.Email, registration.Password)))).Outcome==ResultOutcome.Forbidden,"Registro nuevo exige confirmar el correo antes del login");
student.EmailConfirmedAt=DateTime.UtcNow;await db.SaveChangesAsync();
var login = await sender.Send(new AuthLoginCommand(new LoginRequest(registration.Email, registration.Password)));
Check(login.Outcome == ResultOutcome.Success, "Login mediante MediatR y adaptador de token");
Check((await sender.Send(new CoursesGetByIdQuery("missing"))).Outcome == ResultOutcome.NotFound, "Curso inexistente devuelve NotFound");
Check((await sender.Send(new OrdersGetUserOrdersQuery("another-user"))).Outcome == ResultOutcome.Forbidden, "Una cuenta no consulta órdenes ajenas");
var course = new Course { Id = "course", Title = "Balística", Slug = "balistica", Price = 100 };
db.Courses.Add(course); await db.SaveChangesAsync();
var checkout = new CreateOrderRequest(student.Id, course.Id, ProductType.CURSO, 99, "BANK", "OP-1", DateTime.UtcNow.AddMinutes(-1), "Ana Forense");
Check((await sender.Send(new OrdersCreateOrderAndSubmitPaymentCommand(checkout))).Outcome == ResultOutcome.Invalid && !await db.Orders.AnyAsync(), "Precio del servidor impide alterar monto desde el cliente");
Check((await sender.Send(new OrdersCreateOrderAndSubmitPaymentCommand(checkout with { Amount = 100 }))).Outcome == ResultOutcome.Success, "Checkout crea orden y evidencia por MediatR");
var order = await db.Orders.SingleAsync(); var payment = await db.PaymentEvidences.SingleAsync();
await Reject(new OrdersUploadVoucherCommand(order.Id, new byte[0]), "Pipeline rechaza voucher vacío");
Check((await sender.Send(new OrdersUploadVoucherCommand(order.Id, new byte[] {37,80,68,70,45,49}))).Outcome == ResultOutcome.Success, "Voucher pasa por almacenamiento privado abstracto");
actor.UserId = "admin";
Check((await sender.Send(new AdminApprovePaymentCommand(payment.Id))).Outcome == ResultOutcome.Success, "Aprobación procesa matrícula y vigencia");
db.ChangeTracker.Clear(); student = await db.Users.SingleAsync(); payment = await db.PaymentEvidences.SingleAsync();
Check(await db.Enrollments.CountAsync() == 1 && student.ValidUntil == payment.ValidatedAt!.Value.AddYears(2), "Matrícula y renovación persistidas desde la misma operación");
Check((await sender.Send(new AdminApprovePaymentCommand(payment.Id))).Outcome == ResultOutcome.Invalid && await db.Enrollments.CountAsync() == 1, "Repetir aprobación no duplica matrícula");
Check((await sender.Send(new SettingsSetCommand("X", new SettingRequest("http://invalid.example")))).Outcome == ResultOutcome.Invalid, "Configuración impide enlaces sin HTTPS");
Check((await sender.Send(new SettingsSetCommand("ExamRetakeWaitHours", new SettingRequest("24")))).Outcome == ResultOutcome.NoContent, "Configuración válida usa handler y auditoría");
using var canceled = new CancellationTokenSource(); canceled.Cancel();
try { await sender.Send(new CoursesGetByIdQuery(course.Id), canceled.Token); throw new Exception("No se respetó cancelación"); }
catch (OperationCanceledException) { Console.WriteLine("OK: MediatR propaga cancelación"); }
await Reject(new UploadImageCommand(Array.Empty<byte>()),"Subida vacía rechazada por el pipeline");
await Reject(new UploadImageCommand(new byte[5242881]),"Límite de 5 MB validado en Application");
await Reject(new GetImageQuery("../voucher"),"Lectura de imágenes rechaza rutas arbitrarias");
Check((await sender.Send(new UploadImageCommand(System.Text.Encoding.UTF8.GetBytes("archivo falso")))).Outcome==ResultOutcome.Invalid,"Contenido falso rechazado sin subirlo");
using(var bitmap=new SkiaSharp.SKBitmap(2500,100)){
 bitmap.Erase(SkiaSharp.SKColors.Teal);using var raw=SkiaSharp.SKImage.FromBitmap(bitmap);using var png=raw.Encode(SkiaSharp.SKEncodedImageFormat.Png,100);
 var uploaded=await sender.Send(new UploadImageCommand(png.ToArray()));Check(uploaded.Outcome==ResultOutcome.Success,"La subida válida pasa por procesamiento y almacenamiento");
 var result=JsonSerializer.SerializeToElement(uploaded.Value);var imageId=result.GetProperty("id").GetString()!;
 var fetched=await sender.Send(new GetImageQuery(imageId));var content=(DownloadContent)fetched.Value!;
 using var normalized=SkiaSharp.SKBitmap.Decode(content.Bytes);Check(normalized.Width==1920&&normalized.Height<=1920&&content.ContentType=="image/webp","Imagen normalizada a WebP y limitada a 1920 píxeles");
 Check(result.GetProperty("url").GetString()=="/api/media/images/"+imageId,"La URL guardada no expira ni contiene credenciales");
 var imageStore=(TestImageStorage)scope.ServiceProvider.GetRequiredService<IImageStorage>();imageStore.Fail=true;
 Check((await sender.Send(new UploadImageCommand(png.ToArray()))).Outcome==ResultOutcome.Unavailable,"Fallo del almacenamiento no informa un éxito de subida");imageStore.Fail=false;
}
Check((await sender.Send(new GetImageQuery(Guid.NewGuid().ToString("N")))).Outcome==ResultOutcome.NotFound,"Imagen inexistente devuelve NotFound");
Console.WriteLine("Todas las comprobaciones de arquitectura y casos de uso finalizaron correctamente. No se utilizó PostgreSQL remoto.");

sealed class TestUser : ICurrentUser { public string UserId { get; set; } = "test"; public bool IsInRole(string role) => role == "1"; }
sealed class TestToken : ITokenService { public string Create(User user) => "test-token-" + user.Id; }
sealed class TestStorage : IVoucherStorage { public Task<DownloadContent?> ReadAsync(string name, CancellationToken ct) => Task.FromResult<DownloadContent?>(null); public Task<string> SaveAsync(byte[] bytes, string extension, CancellationToken ct) => Task.FromResult("test-voucher" + extension); }
sealed class TestPdf : IPdfCertificateService {
    public Task<byte[]> GenerateCertificatePdfAsync(string name, string dni, string title, int hours, string code, string url, DateTime issued, decimal grade, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<byte>());
}

sealed class TestImageStorage:IImageStorage {
 private readonly Dictionary<string,byte[]> images=new();public bool Fail{get;set;}
 public Task SaveAsync(string id,byte[] bytes,CancellationToken ct){if(Fail)throw new IOException("Fallo de prueba");images[id]=bytes;return Task.CompletedTask;}
 public Task<byte[]?> ReadAsync(string id,CancellationToken ct)=>Task.FromResult(images.GetValueOrDefault(id));
}

sealed class DisabledGoogle:IGoogleIdentityValidator{public bool Enabled=>false;public Task<GoogleIdentity?> ValidateAsync(string credential,string nonce,CancellationToken ct)=>Task.FromResult<GoogleIdentity?>(null);}

sealed class TestConfirmation:IEmailConfirmation{public Task<ConfirmationDelivery> SendInitialAsync(User u,CancellationToken ct)=>Task.FromResult(ConfirmationDelivery.Sent);public Task RequestAsync(string e,CancellationToken ct)=>Task.CompletedTask;public Task<bool> ConfirmAsync(string t,CancellationToken ct)=>Task.FromResult(false);}
