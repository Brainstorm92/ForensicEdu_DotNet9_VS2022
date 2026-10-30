using System;
using ForensicEdu.Application.Common.Interfaces;
using QRCoder;

namespace ForensicEdu.Infrastructure.Services;

public class QrCodeService : IQrCodeService
{
    public byte[] GeneratePngQrCode(string payloadUrl, int pixelsPerModule = 20)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payloadUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(pixelsPerModule);
    }

    public string GenerateBase64QrCode(string payloadUrl, int pixelsPerModule = 20)
    {
        var bytes = GeneratePngQrCode(payloadUrl, pixelsPerModule);
        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }
}
