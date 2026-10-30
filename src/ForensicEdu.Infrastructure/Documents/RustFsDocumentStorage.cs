using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Infrastructure.Media;
using Microsoft.Extensions.Options;
namespace ForensicEdu.Infrastructure.Documents;
public sealed class RustFsDocumentStorage(IOptions<RustFsOptions> settings):IDocumentStorage
{
 private AmazonS3Client Client(){
  var o=settings.Value;
  if(!Uri.TryCreate(o.Endpoint,UriKind.Absolute,out var uri)||uri.Scheme!="https"||string.IsNullOrWhiteSpace(o.AccessKey)||string.IsNullOrWhiteSpace(o.SecretKey)||string.IsNullOrWhiteSpace(o.Bucket))throw new InvalidOperationException("El almacenamiento privado no está configurado.");
  return new AmazonS3Client(new BasicAWSCredentials(o.AccessKey,o.SecretKey),new AmazonS3Config{
   ServiceURL=o.Endpoint,ForcePathStyle=true,AuthenticationRegion=o.Region,MaxErrorRetry=0,Timeout=TimeSpan.FromSeconds(30),
   RequestChecksumCalculation=RequestChecksumCalculation.WHEN_REQUIRED,ResponseChecksumValidation=ResponseChecksumValidation.WHEN_REQUIRED});
 }
 private static string Key(string key){
  if(!key.StartsWith("documents/",StringComparison.Ordinal))throw new ArgumentException("Ruta de material inválida.");
  var file=key[10..];var ext=Path.GetExtension(file);
  if(ext is not (".pdf" or ".docx") || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(file),"N",out _) || file.Contains('/') || file.Contains('\\'))throw new ArgumentException("Ruta de material inválida.");
  return key;
 }
 public async Task SaveAsync(string key,byte[] bytes,string contentType,CancellationToken ct){
  key=Key(key);using var client=Client();using var stream=new MemoryStream(bytes,false);
  await client.PutObjectAsync(new PutObjectRequest{BucketName=settings.Value.Bucket,Key=key,InputStream=stream,ContentType=contentType,UseChunkEncoding=false,AutoCloseStream=false},ct);
 }
 public async Task<byte[]?> ReadAsync(string key,CancellationToken ct){
  key=Key(key);using var client=Client();
  try{using var res=await client.GetObjectAsync(settings.Value.Bucket,key,ct);
   if(res.ContentLength>PrivateDocuments.MaxBytes)throw new InvalidDataException();
   using var output=new MemoryStream();var buffer=new byte[81920];int count;
   while((count=await res.ResponseStream.ReadAsync(buffer,ct))>0){if(output.Length+count>PrivateDocuments.MaxBytes)throw new InvalidDataException();await output.WriteAsync(buffer.AsMemory(0,count),ct);}
   return output.ToArray();
  }catch(AmazonS3Exception e) when(e.StatusCode==System.Net.HttpStatusCode.NotFound){return null;}
 }
 public async Task DeleteAsync(string key,CancellationToken ct){key=Key(key);using var client=Client();await client.DeleteObjectAsync(settings.Value.Bucket,key,ct);}
}
