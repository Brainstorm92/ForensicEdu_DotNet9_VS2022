using ForensicEdu.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
namespace ForensicEdu.Infrastructure.Services;

public sealed class PrivateVoucherStorage(IHostEnvironment environment) : IVoucherStorage
{
    public async Task<ForensicEdu.Application.Common.Results.DownloadContent?> ReadAsync(string storageName,CancellationToken cancellationToken)
    {
        if(!System.Text.RegularExpressions.Regex.IsMatch(storageName ?? "",@"^[a-f0-9]{32}\.(pdf|png|jpg)$"))return null;
        var path=Path.Combine(environment.ContentRootPath,"App_Data","vouchers",storageName!);
        if(!File.Exists(path))return null;
        var bytes=await File.ReadAllBytesAsync(path,cancellationToken);
        var contentType=Path.GetExtension(path) switch {".pdf"=>"application/pdf",".png"=>"image/png",_=>"image/jpeg"};
        return new(bytes,contentType,Path.GetFileName(path));
    }
    public async Task<string> SaveAsync(byte[] bytes, string extension, CancellationToken cancellationToken)
    {
        if (extension is not (".pdf" or ".png" or ".jpg")) throw new ArgumentException("Formato no permitido.", nameof(extension));
        var folder = Path.Combine(environment.ContentRootPath, "App_Data", "vouchers");
        Directory.CreateDirectory(folder);
        var name = Guid.NewGuid().ToString("N") + extension;
        await File.WriteAllBytesAsync(Path.Combine(folder, name), bytes, cancellationToken);
        return name;
    }
}
