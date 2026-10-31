using ForensicEdu.Application.Common.Interfaces;
using SkiaSharp;
namespace ForensicEdu.Infrastructure.Media;
public sealed class WebImageProcessor:IImageProcessor
{
 public Task<byte[]> NormalizeAsync(byte[] bytes,CancellationToken ct){
  ct.ThrowIfCancellationRequested();
  using var source=SKData.CreateCopy(bytes);using var codec=SKCodec.Create(source);
  if(codec==null||codec.EncodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png or SKEncodedImageFormat.Webp))throw new InvalidDataException("Selecciona una imagen JPG, PNG o WebP válida.");
  var width=codec.Info.Width;var height=codec.Info.Height;
  if(width<=0||height<=0||(long)width*height>24000000||width>12000||height>12000)throw new InvalidDataException("La imagen debe tener como máximo 24 megapíxeles y 12000 píxeles por lado.");
  using var bitmap=new SKBitmap(new SKImageInfo(width,height,SKColorType.Rgba8888,SKAlphaType.Premul));
  if(codec.GetPixels(bitmap.Info,bitmap.GetPixels())!=SKCodecResult.Success)throw new InvalidDataException("La imagen está dañada o incompleta.");
  var rotated=codec.EncodedOrigin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
  var ow=rotated?height:width;var oh=rotated?width:height;var scale=Math.Min(1d,1920d/Math.Max(ow,oh));
  using var surface=SKSurface.Create(new SKImageInfo(Math.Max(1,(int)(ow*scale)),Math.Max(1,(int)(oh*scale)),SKColorType.Rgba8888,SKAlphaType.Premul));
  var canvas=surface.Canvas;canvas.Clear(SKColors.Transparent);canvas.Scale((float)scale);
  switch(codec.EncodedOrigin){
   case SKEncodedOrigin.TopRight:canvas.Translate(width,0);canvas.Scale(-1,1);break;
   case SKEncodedOrigin.BottomRight:canvas.Translate(width,height);canvas.RotateDegrees(180);break;
   case SKEncodedOrigin.BottomLeft:canvas.Translate(0,height);canvas.Scale(1,-1);break;
   case SKEncodedOrigin.LeftTop:canvas.RotateDegrees(90);canvas.Scale(1,-1);break;
   case SKEncodedOrigin.RightTop:canvas.Translate(height,0);canvas.RotateDegrees(90);break;
   case SKEncodedOrigin.RightBottom:canvas.Translate(height,width);canvas.RotateDegrees(90);canvas.Scale(-1,1);break;
   case SKEncodedOrigin.LeftBottom:canvas.Translate(0,width);canvas.RotateDegrees(270);break;
  }
  canvas.DrawBitmap(bitmap,0,0);canvas.Flush();ct.ThrowIfCancellationRequested();
  using var image=surface.Snapshot();using var encoded=image.Encode(SKEncodedImageFormat.Webp,82);
  if(encoded==null)throw new InvalidDataException("No pudimos preparar la imagen.");
  return Task.FromResult(encoded.ToArray());
 }
}
