using Android.Graphics;
using Android.Graphics.Pdf;
using Android.OS;
namespace ForensicEdu.Mobile.Services;
public sealed class DocumentPreview:IDocumentPreview
{
 private PdfRenderer? renderer;private ParcelFileDescriptor? descriptor;private string? path;
 public int PageCount=>renderer?.PageCount??0;
 public async Task OpenPdfAsync(byte[] bytes){Dispose();path=System.IO.Path.Combine(FileSystem.CacheDirectory,Guid.NewGuid()+".pdf");await File.WriteAllBytesAsync(path,bytes);descriptor=ParcelFileDescriptor.Open(new Java.IO.File(path),ParcelFileMode.ReadOnly);renderer=new PdfRenderer(descriptor!);}
 public Task<byte[]> PageAsync(int index)=>Task.Run(()=>{using var page=renderer!.OpenPage(index);var width=1200;var ratio=(double)page.Height/Math.Max(1,page.Width);width=(int)Math.Min(width,Math.Sqrt(4_000_000/Math.Max(.01,ratio)));var height=Math.Max(1,(int)(width*ratio));using var bitmap=Bitmap.CreateBitmap(width,height,Bitmap.Config.Argb8888!)!;bitmap.EraseColor(Android.Graphics.Color.White);page.Render(bitmap,null,null,PdfRenderMode.ForDisplay);using var stream=new MemoryStream();bitmap.Compress(Bitmap.CompressFormat.Png!,100,stream);return stream.ToArray();});
 public void Dispose(){renderer?.Close();renderer?.Dispose();renderer=null;descriptor?.Close();descriptor?.Dispose();descriptor=null;if(path!=null&&File.Exists(path))File.Delete(path);path=null;}
}
