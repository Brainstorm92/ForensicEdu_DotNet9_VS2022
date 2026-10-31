using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using ForensicEdu.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
namespace ForensicEdu.Infrastructure.Media;
public sealed class RustFsImageStorage(IOptions<RustFsOptions> settings):IImageStorage
{
 private AmazonS3Client Client(){
  var o=settings.Value;
  if(!Uri.TryCreate(o.Endpoint,UriKind.Absolute,out var uri)||uri.Scheme!="https"||string.IsNullOrWhiteSpace(o.AccessKey)||string.IsNullOrWhiteSpace(o.SecretKey)||string.IsNullOrWhiteSpace(o.Bucket))
   throw new InvalidOperationException("El almacenamiento de imágenes no está configurado.");
  return new AmazonS3Client(new BasicAWSCredentials(o.AccessKey,o.SecretKey),new AmazonS3Config{
   ServiceURL=o.Endpoint,ForcePathStyle=true,AuthenticationRegion=o.Region,MaxErrorRetry=0,Timeout=TimeSpan.FromSeconds(20),
   RequestChecksumCalculation=RequestChecksumCalculation.WHEN_REQUIRED,ResponseChecksumValidation=ResponseChecksumValidation.WHEN_REQUIRED});
 }
 private static string Key(string id){if(!Guid.TryParseExact(id,"N",out _))throw new ArgumentException("Identificador de imagen inválido.");return "images/"+id+".webp";}
 public async Task SaveAsync(string id,byte[] bytes,CancellationToken ct){
  using var client=Client();using var stream=new MemoryStream(bytes,false);
  await client.PutObjectAsync(new PutObjectRequest{BucketName=settings.Value.Bucket,Key=Key(id),InputStream=stream,ContentType="image/webp",UseChunkEncoding=false,AutoCloseStream=false},ct);
 }
 public async Task<byte[]?> ReadAsync(string id,CancellationToken ct){
  using var client=Client();
  try{using var res=await client.GetObjectAsync(settings.Value.Bucket,Key(id),ct);
   if(res.ContentLength>5*1024*1024)throw new InvalidDataException("La imagen supera el tamaño permitido.");
   using var output=new MemoryStream();var buffer=new byte[81920];int count;
   while((count=await res.ResponseStream.ReadAsync(buffer,ct))>0){if(output.Length+count>5*1024*1024)throw new InvalidDataException("La imagen supera el tamaño permitido.");await output.WriteAsync(buffer.AsMemory(0,count),ct);}
   return output.ToArray();
  }catch(AmazonS3Exception e) when(e.StatusCode==System.Net.HttpStatusCode.NotFound){return null;}
 }
}
