using System.Net;
using System.Text;
using System.Text.Json;
using ForensicEdu.Infrastructure.Secrets;
using Microsoft.Extensions.Configuration;

var passed = 0;
OpenBaoOptions Options() => new() { Address="https://bao.example", RoleId="test-role", SecretId="test-secret" };
void Check(bool condition) { if (!condition) throw new Exception("Check failed"); passed++; }
async Task Reject(OpenBaoOptions options, HttpMessageHandler handler)
{
    try { await new OpenBaoConfigurationLoader(new HttpClient(handler)).LoadAsync(options); throw new Exception("Expected rejection"); }
    catch (InvalidOperationException ex) { Check(ex.Message.StartsWith("OpenBao:") && !ex.Message.Contains("sensitive-response")); }
}
var calls=0;
using var client=new HttpClient(new Fake(async request => {
    calls++;
    if(calls==1) {
        Check(request.RequestUri!.AbsolutePath=="/v1/auth/approle/login");
        Check(!request.Headers.Contains("X-Vault-Token"));
        using var body=JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
        Check(body.RootElement.GetProperty("secret_id").GetString()=="test-secret");
        return Reply(HttpStatusCode.OK,"""{"auth":{"client_token":"synthetic-token"}}""");
    }
    Check(request.Method==HttpMethod.Get && request.RequestUri!.AbsolutePath=="/v1/forense/data/produccion/api");
    Check(request.Headers.GetValues("X-Vault-Token").Single()=="synthetic-token");
    return Reply(HttpStatusCode.OK,"""{"data":{"data":{"ConnectionStrings":{"DefaultConnection":"synthetic-db"},"Authentication":{"Google":{"Enabled":true}},"ObjectStorage":{"Bucket":"test"}}}}""");
}));
var json=await new OpenBaoConfigurationLoader(client).LoadAsync(Options());
using var config=new ConfigurationManager();
config.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)));
Check(config["ConnectionStrings:DefaultConnection"]=="synthetic-db");
Check(config.GetValue<bool>("Authentication:Google:Enabled"));
Check(config["ObjectStorage:Bucket"]=="test");
await Reject(Options(),new Fake(_=>Task.FromResult(Reply(HttpStatusCode.Forbidden,"sensitive-response"))));
await Reject(Options(),new Fake(_=>Task.FromResult(Reply(HttpStatusCode.OK,"not json sensitive-response"))));
var missing=Options();missing.SecretId="";await Reject(missing,new Fake(_=>throw new Exception("Network must not be called")));
var insecure=Options();insecure.Address="http://bao.example";await Reject(insecure,new Fake(_=>throw new Exception("Network must not be called")));
var traversal=Options();traversal.SecretPath="../api";await Reject(traversal,new Fake(_=>throw new Exception("Network must not be called")));
foreach(var payload in new[]{ """{"data":{"data":null}}""", """{"data":{"data":{"OpenBao":{"SecretId":"sensitive-response"}}}}""" }) {
    var n=0;
    await Reject(Options(),new Fake(_=>Task.FromResult(++n==1 ? Reply(HttpStatusCode.OK,"""{"auth":{"client_token":"synthetic-token"}}""") : Reply(HttpStatusCode.OK,payload))));
}
var readCalls=0;
await Reject(Options(),new Fake(_=>Task.FromResult(++readCalls==1 ? Reply(HttpStatusCode.OK,"""{"auth":{"client_token":"synthetic-token"}}""") : Reply(HttpStatusCode.NotFound,"sensitive-response"))));
using var disabled=new ConfigurationManager();
await disabled.AddOpenBaoSecretsAsync();Check(disabled["ConnectionStrings:DefaultConnection"]==null);
Console.WriteLine($"PASS OpenBao: {passed} checks (synthetic credentials only).");
static HttpResponseMessage Reply(HttpStatusCode status,string body)=>new(status){Content=new StringContent(body)};
sealed class Fake(Func<HttpRequestMessage,Task<HttpResponseMessage>> send):HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)=>send(request);
}
