using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Management;
using Microsoft.Extensions.Options;
namespace ForensicEdu.Infrastructure.Media;
public sealed class RustFsVideoStorage(IOptions<RustFsOptions> settings):IPrivateVideoStorage
{
 public AmazonS3Client Client(bool browser=false){var o=settings.Value;var endpoint=browser&&!string.IsNullOrWhiteSpace(o.PublicEndpoint)?o.PublicEndpoint:o.Endpoint;
  if(!Uri.TryCreate(endpoint,UriKind.Absolute,out var uri)||uri.Scheme!="https"||!string.IsNullOrEmpty(uri.UserInfo)||!string.IsNullOrEmpty(uri.Query)||!string.IsNullOrEmpty(uri.Fragment)||string.IsNullOrWhiteSpace(o.AccessKey)||string.IsNullOrWhiteSpace(o.SecretKey)||string.IsNullOrWhiteSpace(o.Bucket))throw new InvalidOperationException("Configure el endpoint HTTPS S3 y las credenciales de ObjectStorage.");
  return new AmazonS3Client(new BasicAWSCredentials(o.AccessKey,o.SecretKey),new AmazonS3Config{ServiceURL=endpoint,ForcePathStyle=true,AuthenticationRegion=o.Region,MaxErrorRetry=0,Timeout=TimeSpan.FromSeconds(60),RequestChecksumCalculation=RequestChecksumCalculation.WHEN_REQUIRED,ResponseChecksumValidation=ResponseChecksumValidation.WHEN_REQUIRED});
 }
 private static string Check(string key){if(!PrivateVideos.Key(key)&&!PrivateVideos.Key(key,true))throw new ArgumentException("Referencia de video inválida.");return key;}
 public async Task<string> UploadUrlAsync(string key,DateTime expires,CancellationToken ct){if(!PrivateVideos.Key(key,true))throw new ArgumentException("Referencia de subida inválida.");using var client=Client(true);return await client.GetPreSignedURLAsync(new GetPreSignedUrlRequest{BucketName=settings.Value.Bucket,Key=key,Verb=HttpVerb.PUT,Expires=expires,ContentType="video/mp4"}).WaitAsync(ct);}
 public async Task<VideoProbe?> ProbeAsync(string key,CancellationToken ct){Check(key);using var client=Client();try{var head=await client.GetObjectMetadataAsync(settings.Value.Bucket,key,ct);if(head.ContentLength<16||head.ContentLength>PrivateVideos.MaxBytes)return new(head.ContentLength,false);
  using var res=await client.GetObjectAsync(new GetObjectRequest{BucketName=settings.Value.Bucket,Key=key,ByteRange=new ByteRange(0,31)},ct);var bytes=new byte[12];await res.ResponseStream.ReadExactlyAsync(bytes,ct);return new(head.ContentLength,bytes[4]==(byte)'f'&&bytes[5]==(byte)'t'&&bytes[6]==(byte)'y'&&bytes[7]==(byte)'p'&&head.Headers.ContentType?.Split(';')[0]=="video/mp4");
 }catch(AmazonS3Exception e) when(e.StatusCode==System.Net.HttpStatusCode.NotFound){return null;}}
 public async Task PublishAsync(string stagingKey,string key,CancellationToken ct){if(!PrivateVideos.Key(stagingKey,true)||!PrivateVideos.Key(key))throw new ArgumentException("Referencia inválida.");using var client=Client();await client.CopyObjectAsync(new CopyObjectRequest{SourceBucket=settings.Value.Bucket,SourceKey=stagingKey,DestinationBucket=settings.Value.Bucket,DestinationKey=key},ct);}
 public async Task<VideoPlayback> PlaybackAsync(string key,CancellationToken ct){if(!PrivateVideos.Key(key))throw new ArgumentException("Referencia inválida.");using var client=Client(true);var expires=DateTime.UtcNow.AddMinutes(15);var url=await client.GetPreSignedURLAsync(new GetPreSignedUrlRequest{BucketName=settings.Value.Bucket,Key=key,Verb=HttpVerb.GET,Expires=expires,ResponseHeaderOverrides=new ResponseHeaderOverrides{ContentType="video/mp4",ContentDisposition="inline",CacheControl="private, no-store"}}).WaitAsync(ct);return new(url,expires);}
 public async Task DeleteAsync(string key,CancellationToken ct){Check(key);using var client=Client();await client.DeleteObjectAsync(settings.Value.Bucket,key,ct);}
 public async Task CleanStagingAsync(CancellationToken ct){using var client=Client();var cutoff=DateTime.UtcNow.AddDays(-1);string? next=null;do{var page=await client.ListObjectsV2Async(new ListObjectsV2Request{BucketName=settings.Value.Bucket,Prefix="video-uploads/",ContinuationToken=next,MaxKeys=100},ct);foreach(var item in page.S3Objects??[]){if(PrivateVideos.Key(item.Key,true)&&item.LastModified<cutoff)await client.DeleteObjectAsync(settings.Value.Bucket,item.Key,ct);}next=page.NextContinuationToken;}while(!string.IsNullOrEmpty(next));}
}
