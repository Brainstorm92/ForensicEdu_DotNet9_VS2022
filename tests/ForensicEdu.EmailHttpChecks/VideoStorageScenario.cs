using System.Net;
using Amazon.S3.Model;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Infrastructure.Media;
using ForensicEdu.Infrastructure.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

static class VideoStorageScenario
{
 public static async Task Run(string[] args){
  using var timeout=new CancellationTokenSource(TimeSpan.FromMinutes(3));var ct=timeout.Token;
  var config=new ConfigurationManager();config.SetBasePath(Path.GetFullPath("src/ForensicEdu.WebApi"));config.AddJsonFile("appsettings.json").AddUserSecrets("ForensicEdu-Local-Development").AddJsonFile("appsettings.Local.json",optional:true).AddEnvironmentVariables();await config.AddOpenBaoSecretsAsync(ct);config.AddEnvironmentVariables();
  var options=config.GetSection("ObjectStorage").Get<RustFsOptions>()??new();var storage=new RustFsVideoStorage(Options.Create(options));using var client=storage.Client();
  await client.HeadBucketAsync(new HeadBucketRequest{BucketName=options.Bucket},ct);Console.WriteLine("OK: bucket privado accesible mediante la configuración existente.");
  const string origin="https://forense9-webforense.wv6a0u.easypanel.host";
  if(args.Contains("--video-storage-cors")){
   CORSConfiguration cors;
   try{cors=(await client.GetCORSConfigurationAsync(options.Bucket,ct)).Configuration??new();}catch(Amazon.S3.AmazonS3Exception e) when(e.ErrorCode=="NoSuchCORSConfiguration"||e.StatusCode==HttpStatusCode.NotFound){cors=new();}
   cors.Rules??=[];cors.Rules.RemoveAll(r=>r.Id=="forensicedu-private-video");
   cors.Rules.Add(new CORSRule{Id="forensicedu-private-video",AllowedOrigins=[origin,"http://localhost:3000","http://localhost:5000","http://localhost:5173"],AllowedMethods=["GET","HEAD","PUT"],AllowedHeaders=["content-type","range"],ExposeHeaders=["ETag","Content-Length","Content-Range","Accept-Ranges"],MaxAgeSeconds=600});
   await client.PutCORSConfigurationAsync(options.Bucket,cors,ct);Console.WriteLine("OK: regla CORS agregada para el frontend y desarrollo local, conservando las otras reglas.");
  }
  var endpoint=(string.IsNullOrWhiteSpace(options.PublicEndpoint)?options.Endpoint:options.PublicEndpoint).TrimEnd('/');
  using var http=new HttpClient(new HttpClientHandler{AllowAutoRedirect=false});
  using(var request=new HttpRequestMessage(HttpMethod.Options,endpoint+"/"+options.Bucket+"/video-uploads/probe.mp4")){request.Headers.Add("Origin",origin);request.Headers.Add("Access-Control-Request-Method","PUT");request.Headers.Add("Access-Control-Request-Headers","content-type");using var result=await http.SendAsync(request,ct);var allowed=result.Headers.TryGetValues("Access-Control-Allow-Origin",out var values)&&values.Any(v=>v==origin||v=="*");Console.WriteLine("CORS de subida desde producción: "+(result.IsSuccessStatusCode&&allowed?"correcto":"pendiente; HTTP "+(int)result.StatusCode));if(!result.IsSuccessStatusCode||!allowed){Console.WriteLine("Si el listener no aplica CORS de bucket, configurar RUSTFS_CORS_ALLOWED_ORIGINS en Easypanel y reiniciar RustFS.");}}
 }
}
