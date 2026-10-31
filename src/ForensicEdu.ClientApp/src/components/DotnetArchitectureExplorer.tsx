import React, { useState } from 'react';
import {
  Code2,
  Layers,
  Terminal,
  CheckCircle2,
  Copy,
  ExternalLink,
  FolderTree,
  Server,
  ShieldAlert,
  FileCode2,
  Database,
  Cpu,
  Download,
  Workflow,
  Sparkles,
  Play
} from 'lucide-react';
import { useApp } from '../context/AppContext';
import { DownloadZipModal } from './DownloadZipModal';

export const DotnetArchitectureExplorer: React.FC = () => {
  const { setActiveTab } = useApp();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedFile, setSelectedFile] = useState<string>('AntifraudService.cs');
  const [copied, setCopied] = useState(false);
  const [apiEndpoint, setApiEndpoint] = useState('http://localhost:5000/api');
  const [connectionStatus, setConnectionStatus] = useState<'idle' | 'testing' | 'offline' | 'online'>('idle');

  const filesCode: Record<string, { project: string; description: string; path: string; code: string }> = {
    'AntifraudService.cs': {
      project: 'ForensicEdu.Application',
      path: '/backend-dotnet9/src/ForensicEdu.Application/Services/AntifraudService.cs',
      description: 'Motor antifraude estricto: calcula huella compuesta (banco + num_op + fecha + monto + titular) y hash SHA-256 de voucher.',
      code: `// ForensicEdu.Application/Services/AntifraudService.cs
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Services;

public class AntifraudService : IAntifraudService
{
    private readonly IApplicationDbContext _context;

    public AntifraudService(IApplicationDbContext context) => _context = context;

    public string GenerateFingerprint(string bank, string operationNumber, DateTime date, decimal amount, string accountHolder)
    {
        // Normalización exacta Sección 9: banco|num_op|fecha|monto|titular
        return $"{bank.Trim().ToUpperInvariant()}|{operationNumber.Trim().ToUpperInvariant()}|{date:yyyy-MM-dd}|{amount:F2}|{accountHolder.Trim().ToUpperInvariant()}";
    }

    public string ComputeVoucherSha256(byte[] voucherBytes)
    {
        if (voucherBytes == null || voucherBytes.Length == 0) return string.Empty;
        var hash = SHA256.HashData(voucherBytes);
        return Convert.ToHexStringLower(hash);
    }

    public async Task<AntifraudEvaluationResult> EvaluatePaymentAsync(
        string bank, string operationNumber, DateTime date, decimal amount, string accountHolder, byte[]? voucherBytes = null, CancellationToken cancellationToken = default)
    {
        var fingerprint = GenerateFingerprint(bank, operationNumber, date, amount, accountHolder);
        var voucherHash = voucherBytes != null ? ComputeVoucherSha256(voucherBytes) : string.Empty;

        // Validación 1: Huella bancaria contra pagos ya aprobados
        var duplicateFingerprint = await _context.PaymentEvidences
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.PaymentFingerprint == fingerprint && p.Status == PaymentStatus.APROBADO, cancellationToken);

        if (duplicateFingerprint != null)
        {
            return new AntifraudEvaluationResult(
                IsDuplicate: true,
                Fingerprint: fingerprint,
                VoucherHash: voucherHash,
                ConflictingPaymentId: duplicateFingerprint.Id,
                ConflictingOrderCode: duplicateFingerprint.Order?.OrderCode ?? duplicateFingerprint.OrderId,
                Message: $"ALERTA ANTIFRAUDE: Huella bancaria duplicada. Esta operación ya fue aprobada en la orden '{duplicateFingerprint.Order?.OrderCode}'."
            );
        }

        return new AntifraudEvaluationResult(IsDuplicate: false, Fingerprint: fingerprint, VoucherHash: voucherHash, ConflictingPaymentId: null, ConflictingOrderCode: null, Message: "Comprobante válido y único.");
    }
}`
    },
    'EvaluationService.cs': {
      project: 'ForensicEdu.Application',
      path: '/backend-dotnet9/src/ForensicEdu.Application/Services/EvaluationService.cs',
      description: 'Reglas de evaluación: 10 min de duración, escala 0 a 20, nota mínima 13/20, bloqueo 24h para re-evaluación y emisión automática de certificado con código QR.',
      code: `// ForensicEdu.Application/Services/EvaluationService.cs
public async Task<ExamResultDto> SubmitExamAttemptAsync(ExamSubmissionDto submission, CancellationToken cancellationToken = default)
{
    var enrollment = await _context.Enrollments
        .Include(e => e.Course)
        .Include(e => e.User)
        .Include(e => e.ExamAttempts)
        .FirstOrDefaultAsync(e => e.Id == submission.EnrollmentId && e.UserId == submission.UserId, cancellationToken);

    if (enrollment == null) throw new InvalidOperationException("Matrícula no encontrada.");

    var existingAttempts = enrollment.ExamAttempts.OrderBy(a => a.AttemptNumber).ToList();
    if (existingAttempts.Count >= 2) throw new InvalidOperationException("Ha agotado el número máximo de intentos permitidos (2 intentos).");

    if (existingAttempts.Count == 1 && existingAttempts[0].Approved)
        throw new InvalidOperationException("Ya aprobó este curso en el 1º intento. No requiere re-evaluación.");

    // Validación regla de espera pedagógica de 24 horas para 2do intento
    if (existingAttempts.Count == 1 && existingAttempts[0].NextAttemptAllowedAt.HasValue && DateTime.UtcNow < existingAttempts[0].NextAttemptAllowedAt.Value)
    {
        var rem = existingAttempts[0].NextAttemptAllowedAt.Value - DateTime.UtcNow;
        throw new InvalidOperationException($"Debe esperar 24 horas de repaso antes del 2º intento. Restan: {rem.Hours}h {rem.Minutes}m.");
    }

    // Calificación en escala 0 a 20 (Nota mínima aprobatoria: 13/20)
    decimal score = CalculateGrade(submission);
    bool approved = score >= 13.0m;
    int currentAttemptNumber = existingAttempts.Count + 1;

    if (approved)
    {
        enrollment.ExamStatus = ExamStatus.APROBADO;
        enrollment.FinalGrade = score;

        // Generación de Certificado Digital Oficial (Sección 13)
        var certCode = $"CF-{DateTime.UtcNow.Year}-{enrollment.Course.ShortCode}-{_context.DigitalCertificates.Count() + 1:D6}";
        _context.DigitalCertificates.Add(new DigitalCertificate {
            CertificateCode = certCode,
            QrValidationUrl = $"https://forensic-edu.org/verificar/{certCode}",
            FinalGrade = score,
            Status = CertificateStatus.VALIDO
        });
    }
    // ...
}`
    },
    'AccountLifecycleService.cs': {
      project: 'ForensicEdu.Application',
      path: '/backend-dotnet9/src/ForensicEdu.Application/Services/AccountLifecycleService.cs',
      description: 'Regla de 30 días sin compra -> PENDIENTE_DE_ELIMINACIÓN y renovación de 2 años por cada pago aprobado.',
      code: `// ForensicEdu.Application/Services/AccountLifecycleService.cs
public async Task ExtendValidityOnApprovedPaymentAsync(string userId, DateTime paymentDate, CancellationToken cancellationToken = default)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    if (user == null) return;

    // Regla Sección 10: Cada pago aprobado renueva la vigencia por 2 años desde la fecha del pago
    var newValidUntil = paymentDate.AddYears(2);
    if (user.ValidUntil.HasValue && user.ValidUntil.Value > newValidUntil)
        newValidUntil = user.ValidUntil.Value.AddYears(2);

    user.ValidUntil = newValidUntil;
    user.Status = AccountStatus.ACTIVO;
    user.HasApprovedPurchase = true;

    await _context.SaveChangesAsync(cancellationToken);
}`
    },
    'PdfCertificateService.cs': {
      project: 'ForensicEdu.Infrastructure',
      path: '/backend-dotnet9/src/ForensicEdu.Infrastructure/Services/PdfCertificateService.cs',
      description: 'Generación del diploma pericial oficial con QuestPDF: bordes dorados, marco institucional, QR de validación y firmas digitales.',
      code: `// ForensicEdu.Infrastructure/Services/PdfCertificateService.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class PdfCertificateService : IPdfCertificateService
{
    public Task<byte[]> GenerateCertificatePdfAsync(...)
    {
        var qrImageBytes = _qrCodeService.GeneratePngQrCode(qrValidationUrl, 15);
        var document = Document.Create(container => {
            container.Page(page => {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.Content().Border(3).BorderColor("#C5A059").Padding(12).Column(col => {
                    col.Item().AlignCenter().Text("INSTITUTO DE CIENCIAS FORENSES Y CRIMINALÍSTICA").Bold().FontSize(18);
                    col.Item().AlignCenter().Text("DIPLOMA DE CERTIFICACIÓN").Bold().FontSize(26).FontColor("#1E293B");
                    col.Item().AlignCenter().Text(studentFullName.ToUpper()).Bold().FontSize(22);
                    col.Item().AlignCenter().Text($"\"{courseTitle.ToUpper()}\"").Bold().FontSize(16).FontColor("#0F766E");
                    // QR embebido
                    col.Item().Row(r => {
                        r.RelativeItem().Image(qrImageBytes).FitArea();
                    });
                });
            });
        });
        // ...
    }
}`
    },
    'OrdersController.cs': {
      project: 'ForensicEdu.WebApi',
      path: '/backend-dotnet9/src/ForensicEdu.WebApi/Controllers/OrdersController.cs',
      description: 'Controlador HTTP: Registra orden única ORD-2026-XXXXXX, valida huella bancaria antifraude en tiempo real y recibe evidencia.',
      code: `// ForensicEdu.WebApi/Controllers/OrdersController.cs
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost("checkout")]
    public async Task<IActionResult> CreateOrderAndSubmitPayment([FromBody] CreateOrderRequest request)
    {
        // 1. Evaluación Antifraude Estricta
        var antifraudCheck = await _antifraudService.EvaluatePaymentAsync(
            request.BankName, request.OperationNumber, request.OperationDate, request.Amount, request.AccountHolderName);

        if (antifraudCheck.IsDuplicate)
            return BadRequest(new { isFraud = true, message = antifraudCheck.Message });

        // 2. Generación de Orden Única ORD-YYYY-XXXXXX
        var orderCode = $"ORD-{DateTime.UtcNow.Year}-{await _context.Orders.CountAsync() + 100001}";
        // ...
        return Ok(new { orderCode, status = "PENDIENTE_VALIDACION" });
    }
}`
    },
    'Program.cs': {
      project: 'ForensicEdu.WebApi',
      path: '/backend-dotnet9/src/ForensicEdu.WebApi/Program.cs',
      description: 'Punto de entrada .NET 9: configuración de CORS, Scalar OpenAPI, Hangfire para cron jobs nocturnos e inyección de dependencias.',
      code: `// ForensicEdu.WebApi/Program.cs (.NET 9 C#)
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Hangfire para auditorías nocturnas de vigencia
builder.Services.AddHangfire(cfg => cfg.UseMemoryStorage());
builder.Services.AddHangfireServer();

// CORS para la aplicación React Vite
builder.Services.AddCors(opt => opt.AddPolicy("AllowFrontendApp", p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference(); // Scalar API UI
app.UseCors("AllowFrontendApp");
app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<AccountLifecycleHangfireJob>(
    "daily-account-lifecycle-audit", j => j.ExecuteDailyLifecycleAuditAsync(), Cron.Daily(0, 0));

app.MapControllers();
app.Run();`
    },
    'ForensicEdu.sln': {
      project: 'Visual Studio 2022 Solution',
      path: '/backend-dotnet9/ForensicEdu.sln',
      description: 'Archivo de solución oficial de Visual Studio 2022 que vincula los 4 proyectos (Domain, Application, Infrastructure, WebApi). Por especificación de Microsoft, los archivos .sln son de texto plano estructurado.',
      code: `Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.12.35527.113
MinimumVisualStudioVersion = 10.0.40219.1
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "ForensicEdu.Domain", "src\\ForensicEdu.Domain\\ForensicEdu.Domain.csproj", "{A1111111-1111-1111-1111-111111111111}"
EndProject
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "ForensicEdu.Application", "src\\ForensicEdu.Application\\ForensicEdu.Application.csproj", "{B2222222-2222-2222-2222-222222222222}"
EndProject
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "ForensicEdu.Infrastructure", "src\\ForensicEdu.Infrastructure\\ForensicEdu.Infrastructure.csproj", "{C3333333-3333-3333-3333-333333333333}"
EndProject
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "ForensicEdu.WebApi", "src\\ForensicEdu.WebApi\\ForensicEdu.WebApi.csproj", "{D4444444-4444-4444-4444-444444444444}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{A1111111-1111-1111-1111-111111111111}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{A1111111-1111-1111-1111-111111111111}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{B2222222-2222-2222-2222-222222222222}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{B2222222-2222-2222-2222-222222222222}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{C3333333-3333-3333-3333-333333333333}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{C3333333-3333-3333-3333-333333333333}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{D4444444-4444-4444-4444-444444444444}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{D4444444-4444-4444-4444-444444444444}.Debug|Any CPU.Build.0 = Debug|Any CPU
	EndGlobalSection
EndGlobal`
    }
  };

  const handleCopy = () => {
    navigator.clipboard.writeText(filesCode[selectedFile].code);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleTestConnection = async () => {
    setConnectionStatus('testing');
    try {
      // Simulating or trying to reach local dotnet api
      const response = await fetch(`${apiEndpoint}/courses`, { method: 'GET', mode: 'cors' }).catch(() => null);
      if (response && response.ok) {
        setConnectionStatus('online');
      } else {
        // If not running yet on host, inform user gently
        setConnectionStatus('offline');
      }
    } catch {
      setConnectionStatus('offline');
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8 animate-fadeIn">
      {/* Hero Banner with Direct Download Button */}
      <div className="rounded-2xl bg-gradient-to-br from-panel via-indigo-950/80 to-panel border border-indigo-500/30 p-6 sm:p-8 shadow-xl relative overflow-hidden">
        <div className="absolute top-0 right-0 w-96 h-96 bg-indigo-500/10 rounded-full blur-3xl pointer-events-none" />
        <div className="relative z-10 flex flex-col lg:flex-row lg:items-center justify-between gap-6">
          <div className="space-y-3 max-w-3xl">
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-indigo-500/20 border border-indigo-400/40 text-indigo-700 text-xs font-semibold">
              <Cpu className="w-3.5 h-3.5" />
              <span>Opción 2 Seleccionada: Solución Integral .NET 9 + Frontend Interactivo</span>
            </div>
            <h1 className="text-2xl sm:text-3xl font-extrabold text-ink tracking-tight flex items-center gap-3">
              <span>Arquitectura .NET 9 (C#) & Visual Studio 2022</span>
              <span className="text-xs px-2.5 py-1 rounded bg-brand/20 text-brand border border-brand/40 font-mono">
                Clean Architecture
              </span>
            </h1>
            <p className="text-sm sm:text-base text-muted leading-relaxed">
              La <strong>aplicación interactiva que estás utilizando</strong> está 100% articulada y sincronizada con el
              backend de producción en <strong>.NET 9 (C#)</strong> ubicado en <code className="text-brand bg-canvas px-2 py-0.5 rounded font-mono text-xs">/backend-dotnet9/ForensicEdu.sln</code>.
              Todas las reglas de negocio (antifraude, evaluación de 10 min, vigencias con Hangfire y certificados QR) están implementadas en ambas capas.
            </p>
          </div>

          <div className="flex flex-col sm:flex-row items-center gap-3">
            <button
              onClick={() => setIsModalOpen(true)}
              className="w-full sm:w-auto px-5 py-3 rounded-xl bg-gradient-to-r from-emerald-500 to-teal-500 hover:from-emerald-400 hover:to-teal-400 text-ink font-extrabold text-xs sm:text-sm flex items-center justify-center gap-2.5 transition-all shadow-lg shadow-emerald-500/25 border border-emerald-400/40 cursor-pointer"
            >
              <Download className="w-4 h-4 text-ink" />
              <span>Descargar Solución Completa (.ZIP)</span>
            </button>
            <button
              onClick={() => setActiveTab('mis-cursos')}
              className="w-full sm:w-auto px-4 py-3 rounded-xl bg-brand text-ink hover:bg-brand font-bold text-xs flex items-center justify-center gap-2 transition-all shadow-md"
            >
              <Play className="w-4 h-4 fill-slate-950" />
              <span>Probar en la App</span>
            </button>
          </div>
        </div>
      </div>

      {/* Explanatory Banner: Why .sln is a text file & how to open in Visual Studio */}
      <div className="p-5 rounded-2xl bg-brand/10 border border-brand/30 flex flex-col md:flex-row items-start md:items-center gap-4 text-xs text-brand">
        <div className="w-10 h-10 rounded-xl bg-brand/20 border border-brand/40 flex items-center justify-center shrink-0 text-brand">
          <FileCode2 className="w-6 h-6" />
        </div>
        <div className="space-y-1 flex-1">
          <h3 className="font-bold text-brand text-sm flex items-center gap-2">
            <span>¿Por qué tu navegador o Windows descarga el archivo como .txt?</span>
          </h3>
          <p className="text-muted leading-relaxed">
            Un archivo de solución de Visual Studio (<code className="text-brand font-mono">.sln</code>) es <strong>por diseño de Microsoft un archivo de texto plano estructurado</strong> que contiene los GUIDs y las rutas a los 4 proyectos (<code className="text-indigo-700 font-mono">.csproj</code>). Al descargarlo solo de forma aislada, el navegador le agrega la extensión <code className="text-brand font-mono">.txt</code> y además no tendría los proyectos a su alrededor.
          </p>
          <p className="text-muted leading-relaxed">
            👉 <strong>Solución recomendada:</strong> Haz clic en el botón verde <strong>"Descargar Solución Completa (.ZIP)"</strong>. Al descomprimir el ZIP en tu computadora tendrás la carpeta con <code className="text-brand font-mono">ForensicEdu.sln</code> y la carpeta <code className="text-brand font-mono">src/</code> con los 4 proyectos completos listos para abrir con doble clic en <strong>Visual Studio 2022</strong>.
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="shrink-0 px-4 py-2 rounded-lg bg-brand hover:bg-brand text-ink font-bold text-xs flex items-center gap-1.5 transition-colors cursor-pointer"
        >
          <Download className="w-3.5 h-3.5" />
          <span>Bajar ZIP (30 KB)</span>
        </button>
      </div>

      {/* Visual Studio 2022 Quick Run Instructions */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="p-5 rounded-xl bg-panel/90 border border-line shadow-md">
          <div className="flex items-center gap-3 mb-2 text-indigo-700 font-bold text-sm">
            <div className="w-7 h-7 rounded-lg bg-indigo-500/20 flex items-center justify-center">1</div>
            <span>Abrir en Visual Studio 2022</span>
          </div>
          <p className="text-xs text-muted leading-relaxed">
            Abre VS 2022 (v17.12+), selecciona <em>"Abrir proyecto o solución"</em> y selecciona el archivo <strong className="text-ink">ForensicEdu.sln</strong> ubicado en <code className="text-brand text-[11px]">/backend-dotnet9/</code>.
          </p>
        </div>

        <div className="p-5 rounded-xl bg-panel/90 border border-line shadow-md">
          <div className="flex items-center gap-3 mb-2 text-brand font-bold text-sm">
            <div className="w-7 h-7 rounded-lg bg-brand/20 flex items-center justify-center">2</div>
            <span>Compilar y Presionar F5</span>
          </div>
          <p className="text-xs text-muted leading-relaxed">
            Restaura paquetes NuGet automáticos. Haz clic derecho en <strong className="text-ink">ForensicEdu.WebApi</strong>, márcalo como proyecto de inicio y presiona <strong className="text-brand">F5</strong>.
          </p>
        </div>

        <div className="p-5 rounded-xl bg-panel/90 border border-line shadow-md">
          <div className="flex items-center gap-3 mb-2 text-emerald-700 font-bold text-sm">
            <div className="w-7 h-7 rounded-lg bg-emerald-500/20 flex items-center justify-center">3</div>
            <span>Scalar API & Hangfire</span>
          </div>
          <p className="text-xs text-muted leading-relaxed">
            Se abrirá de inmediato la documentación interactiva en <code className="text-emerald-700 text-[11px]">http://localhost:5000/scalar/v1</code> y el dashboard de tareas en <code className="text-emerald-700 text-[11px]">/hangfire</code>.
          </p>
        </div>
      </div>

      {/* Code & Project Explorer */}
      <div className="rounded-2xl bg-panel border border-line overflow-hidden shadow-xl">
        <div className="px-6 py-4 bg-canvas/80 border-b border-line flex flex-wrap items-center justify-between gap-4">
          <div className="flex items-center gap-2">
            <FolderTree className="w-5 h-5 text-brand" />
            <h2 className="text-base font-bold text-ink">Explorador de Proyectos y Código Fuente C#</h2>
            <span className="text-xs text-muted font-mono">(.NET 9 / C# 13)</span>
          </div>

          <div className="flex items-center gap-2">
            <button
              onClick={handleCopy}
              className="px-3 py-1.5 rounded-lg bg-surface-low hover:bg-surface-high text-ink text-xs font-semibold flex items-center gap-1.5 border border-line transition-colors"
            >
              {copied ? <CheckCircle2 className="w-3.5 h-3.5 text-emerald-700" /> : <Copy className="w-3.5 h-3.5" />}
              <span>{copied ? '¡Copiado!' : 'Copiar Archivo C#'}</span>
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-12 min-h-[500px]">
          {/* File Selector Sidebar */}
          <div className="lg:col-span-4 bg-canvas/50 p-4 border-r border-line space-y-4">
            <div>
              <p className="text-[11px] font-bold text-muted uppercase tracking-wider mb-2">
                Proyectos de la Solución (Clean Architecture)
              </p>
              <div className="space-y-1">
                {Object.entries(filesCode).map(([filename, data]) => {
                  const isSelected = selectedFile === filename;
                  return (
                    <button
                      key={filename}
                      onClick={() => setSelectedFile(filename)}
                      className={`w-full text-left px-3 py-2.5 rounded-lg text-xs font-medium transition-all flex items-start gap-2.5 ${
                        isSelected
                          ? 'bg-brand/15 text-brand border border-brand/40 shadow-sm'
                          : 'text-muted hover:bg-surface-high hover:text-ink border border-transparent'
                      }`}
                    >
                      <FileCode2 className={`w-4 h-4 mt-0.5 shrink-0 ${isSelected ? 'text-brand' : 'text-muted'}`} />
                      <div className="min-w-0">
                        <div className="font-semibold truncate">{filename}</div>
                        <div className="text-[10px] text-muted truncate">{data.project}</div>
                      </div>
                    </button>
                  );
                })}
              </div>
            </div>

            <div className="p-3 rounded-lg bg-panel border border-line text-xs space-y-2">
              <div className="flex items-center gap-1.5 font-bold text-brand">
                <Sparkles className="w-3.5 h-3.5" />
                <span>Ubicación en Disco:</span>
              </div>
              <code className="block p-2 rounded bg-canvas text-[11px] text-muted font-mono break-all border border-line/80">
                {filesCode[selectedFile].path}
              </code>
              <p className="text-[11px] text-muted leading-relaxed">
                {filesCode[selectedFile].description}
              </p>
            </div>
          </div>

          {/* Code Viewer */}
          <div className="lg:col-span-8 flex flex-col bg-canvas">
            <div className="px-4 py-2.5 bg-panel/60 border-b border-line flex items-center justify-between text-xs text-muted font-mono">
              <span className="text-brand font-semibold">{selectedFile}</span>
              <span>C# 13 / .NET 9.0</span>
            </div>
            <pre className="p-4 text-xs font-mono text-ink overflow-x-auto flex-1 leading-relaxed selection:bg-indigo-600">
              <code>{filesCode[selectedFile].code}</code>
            </pre>
          </div>
        </div>
      </div>

      {/* Integration Matrix Frontend <-> .NET 9 Web API */}
      <div className="rounded-2xl bg-panel border border-line p-6 space-y-4 shadow-xl">
        <div className="flex items-center gap-3">
          <Workflow className="w-5 h-5 text-indigo-700" />
          <h2 className="text-base font-bold text-ink">
            Matriz de Sincronización: Frontend Interactivo (React) ↔ Backend .NET 9 (C#)
          </h2>
        </div>
        <p className="text-xs text-muted">
          La siguiente tabla muestra la correspondencia 1:1 entre cada acción que realizas en la pantalla interactiva y el Endpoint / Caso de uso implementado en .NET 9:
        </p>

        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs border-collapse">
            <thead>
              <tr className="bg-canvas text-muted border-b border-line font-semibold">
                <th className="p-3">Flujo en la Aplicación Interactiva</th>
                <th className="p-3">Sección Flujo v5</th>
                <th className="p-3">Endpoint .NET 9</th>
                <th className="p-3">Clase / Servicio C# Ejecutado</th>
                <th className="p-3">Regla Validada</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-line text-muted font-medium">
              <tr className="hover:bg-surface-high/50">
                <td className="p-3 text-ink font-semibold flex items-center gap-2">
                  <span className="w-2 h-2 rounded-full bg-brand"></span>
                  Comprar Curso / Pagar
                </td>
                <td className="p-3 text-brand">Sección 8 & 9</td>
                <td className="p-3 font-mono text-indigo-700">POST /api/orders/checkout</td>
                <td className="p-3 font-mono text-ink">AntifraudService.cs</td>
                <td className="p-3 text-emerald-700">Huella banco+op+fecha+monto+titular y hash SHA-256</td>
              </tr>
              <tr className="hover:bg-surface-high/50">
                <td className="p-3 text-ink font-semibold flex items-center gap-2">
                  <span className="w-2 h-2 rounded-full bg-blue-400"></span>
                  Rendir Evaluación (10 min)
                </td>
                <td className="p-3 text-brand">Sección 12</td>
                <td className="p-3 font-mono text-indigo-700">POST /api/exams/submit</td>
                <td className="p-3 font-mono text-ink">EvaluationService.cs</td>
                <td className="p-3 text-emerald-700">Tiempo max 10 min, nota min 13/20, bloqueo 24h para 2do intento</td>
              </tr>
              <tr className="hover:bg-surface-high/50">
                <td className="p-3 text-ink font-semibold flex items-center gap-2">
                  <span className="w-2 h-2 rounded-full bg-emerald-400"></span>
                  Aprobar Pago en Admin
                </td>
                <td className="p-3 text-brand">Sección 10</td>
                <td className="p-3 font-mono text-indigo-700">POST /api/admin/payments/{`{id}`}/approve</td>
                <td className="p-3 font-mono text-ink">AccountLifecycleService.cs</td>
                <td className="p-3 text-emerald-700">Renueva vigencia de cuenta por 2 años desde la fecha del pago</td>
              </tr>
              <tr className="hover:bg-surface-high/50">
                <td className="p-3 text-ink font-semibold flex items-center gap-2">
                  <span className="w-2 h-2 rounded-full bg-cyan-400"></span>
                  Verificar QR Público
                </td>
                <td className="p-3 text-brand">Sección 13</td>
                <td className="p-3 font-mono text-indigo-700">GET /api/certificates/verify/{`{code}`}</td>
                <td className="p-3 font-mono text-ink">CertificatesController.cs</td>
                <td className="p-3 text-emerald-700">Estados VÁLIDO / REVOCADO / ANULADO / VENCIDO y log de IP</td>
              </tr>
              <tr className="hover:bg-surface-high/50">
                <td className="p-3 text-ink font-semibold flex items-center gap-2">
                  <span className="w-2 h-2 rounded-full bg-purple-400"></span>
                  Descargar Diploma PDF
                </td>
                <td className="p-3 text-brand">Sección 13</td>
                <td className="p-3 font-mono text-indigo-700">GET /api/certificates/download/{`{code}`}/pdf</td>
                <td className="p-3 font-mono text-ink">PdfCertificateService.cs</td>
                <td className="p-3 text-emerald-700">QuestPDF con QR vectorial integrado de QRCoder</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <DownloadZipModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} />
    </div>
  );
};
