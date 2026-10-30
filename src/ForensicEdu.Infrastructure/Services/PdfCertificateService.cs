using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ForensicEdu.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ForensicEdu.Infrastructure.Services;

public class PdfCertificateService : IPdfCertificateService
{
    private readonly IQrCodeService _qrCodeService;

    public PdfCertificateService(IQrCodeService qrCodeService)
    {
        _qrCodeService = qrCodeService;
        // QuestPDF Community License registration
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateCertificatePdfAsync(
        string studentFullName,
        string studentDni,
        string courseTitle,
        int academicHours,
        string certificateCode,
        string qrValidationUrl,
        DateTime issueDate,
        decimal finalGrade,
        CancellationToken cancellationToken = default)
    {
        var qrImageBytes = _qrCodeService.GeneratePngQrCode(qrValidationUrl, 15);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));

                page.Content().Border(3).BorderColor("#C5A059").Padding(12).Column(col =>
                {
                    // Header with institutional titles
                    col.Item().AlignCenter().Text("INSTITUTO INTERNACIONAL DE CIENCIAS FORENSES Y CRIMINALÍSTICA")
                        .Bold().FontSize(18).FontColor("#0F172A");

                    col.Item().AlignCenter().Text("DIRECCIÓN ACADÉMICA Y DE CERTIFICACIÓN PROFESIONAL")
                        .FontSize(11).FontColor("#64748B").LetterSpacing(0.08f);

                    col.Item().PaddingVertical(10).AlignCenter().Text("OTORGA EL PRESENTE")
                        .Bold().FontSize(13).FontColor("#C5A059");

                    col.Item().AlignCenter().Text("DIPLOMA DE CERTIFICACIÓN")
                        .Bold().FontSize(26).FontColor("#1E293B");

                    col.Item().PaddingTop(8).AlignCenter().Text("A favor de:")
                        .Italic().FontSize(11).FontColor("#475569");

                    // Student Name
                    col.Item().PaddingVertical(6).AlignCenter().Text(studentFullName.ToUpperInvariant())
                        .Bold().FontSize(22).FontColor("#0F172A");

                    col.Item().AlignCenter().Text($"Identificado(a) con D.N.I. Nº {studentDni}")
                        .FontSize(11).FontColor("#475569");

                    col.Item().PaddingTop(12).AlignCenter().Text("Por haber aprobado satisfactoriamente todas las exigencias académicas y periciales del curso de especialización:")
                        .FontSize(11).FontColor("#334155");

                    // Course Name
                    col.Item().PaddingVertical(6).AlignCenter().Text($"\"{courseTitle.ToUpperInvariant()}\"")
                        .Bold().FontSize(16).FontColor("#0F766E");

                    col.Item().AlignCenter().Text($"Con una duración lectiva de {academicHours} horas académicas y calificación final de {finalGrade:F1}/20.")
                        .FontSize(11).FontColor("#334155");

                    col.Item().PaddingTop(15).Row(row =>
                    {
                        // QR and Code Section
                        row.RelativeItem(1).Column(c =>
                        {
                            c.Item().Width(80).Height(80).Image(qrImageBytes);
                            c.Item().PaddingTop(4).Text($"Cód: {certificateCode}").FontSize(8).Bold().FontColor("#0F172A");
                            c.Item().Text("Escanee para verificar autenticidad").FontSize(7).FontColor("#64748B");
                        });

                        // Middle details
                        row.RelativeItem(1).AlignCenter().Column(c =>
                        {
                            c.Item().PaddingTop(25).Text("Expedido en Lima, Perú").FontSize(10).FontColor("#475569");
                            c.Item().Text($"Fecha de Emisión: {issueDate:dd/MM/yyyy}").FontSize(10).Bold().FontColor("#0F172A");
                            c.Item().Text("ESTADO: REGISTRO VÁLIDO").FontSize(9).Bold().FontColor("#16A34A");
                        });

                        // Signature
                        row.RelativeItem(1).AlignRight().Column(c =>
                        {
                            c.Item().PaddingTop(30).LineHorizontal(1).LineColor("#94A3B8");
                            c.Item().AlignCenter().Text("Dr. Roberto Silva Mendoza").Bold().FontSize(10).FontColor("#0F172A");
                            c.Item().AlignCenter().Text("Director de Certificaciones Forenses").FontSize(8).FontColor("#64748B");
                        });
                    });
                });
            });
        });

        using var memoryStream = new MemoryStream();
        document.GeneratePdf(memoryStream);
        return Task.FromResult(memoryStream.ToArray());
    }
}
