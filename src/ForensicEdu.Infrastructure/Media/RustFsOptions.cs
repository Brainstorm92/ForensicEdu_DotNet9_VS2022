namespace ForensicEdu.Infrastructure.Media;
public sealed class RustFsOptions
{
 public string PublicEndpoint {get;set;}="";
 public string Endpoint {get;set;}="";
 public string Bucket {get;set;}="forense-media";
 public string AccessKey {get;set;}="";
 public string SecretKey {get;set;}="";
 public string Region {get;set;}="us-east-1";
}
