using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForensicEdu.Application.Common.Interfaces;

public interface IQrCodeService
{
    byte[] GeneratePngQrCode(string payloadUrl, int pixelsPerModule = 20);
    string GenerateBase64QrCode(string payloadUrl, int pixelsPerModule = 20);
}
