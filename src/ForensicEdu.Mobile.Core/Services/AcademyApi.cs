using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ForensicEdu.Mobile.Core.Models;
namespace ForensicEdu.Mobile.Core.Services;

public sealed class AcademyApi(HttpClient client,ITokenStore tokens)
{
 public static readonly JsonSerializerOptions Json=new(JsonSerializerDefaults.Web);
 public event Action? AuthenticationExpired;
 public Uri BaseAddress=>client.BaseAddress!;
 public static string Id(string id)=>Uri.EscapeDataString(id);
 public string? PublicImage(string value){if(string.IsNullOrWhiteSpace(value))return null;var uri=new Uri(new Uri(BaseAddress.GetLeftPart(UriPartial.Authority)),value);return uri.Scheme=="https"||(uri.Scheme=="http"&&BaseAddress.Scheme=="http"&&uri.Authority==BaseAddress.Authority)?uri.AbsoluteUri:null;}
 public async Task<HttpResponseMessage> SendAsync(HttpMethod method,string path,HttpContent? content=null,bool authenticate=true,CancellationToken ct=default){
  if(Uri.TryCreate(path,UriKind.Absolute,out _)||path.StartsWith('/'))throw new ArgumentException("Usa una ruta relativa de la API.");
  using var request=new HttpRequestMessage(method,path){Content=content};
  if(authenticate){var token=await tokens.ReadAsync();if(!string.IsNullOrWhiteSpace(token))request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);}
  HttpResponseMessage response;
  try{response=await client.SendAsync(request,HttpCompletionOption.ResponseContentRead,ct);}
  catch(OperationCanceledException) when(!ct.IsCancellationRequested){throw new ApiException("La conexión tardó demasiado. Inténtalo nuevamente.");}
  catch(HttpRequestException){throw new ApiException("No pudimos conectar con la academia. Revisa tu conexión.");}
  if(response.IsSuccessStatusCode)return response;
  var status=(int)response.StatusCode;var message=status==401?"Tu sesión terminó. Vuelve a iniciar sesión.":"No se pudo completar la operación.";
  try{using var data=JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));if(data.RootElement.TryGetProperty("message",out var m))message=m.GetString()??message;else if(data.RootElement.TryGetProperty("title",out var t))message=t.GetString()??message;if(data.RootElement.TryGetProperty("errors",out var errors)&&errors.ValueKind==JsonValueKind.Object)message=string.Join(" ",errors.EnumerateObject().SelectMany(x=>x.Value.EnumerateArray().Select(y=>y.GetString())));}catch(JsonException){}
  response.Dispose();if(status==401&&authenticate){await tokens.ClearAsync();AuthenticationExpired?.Invoke();}throw new ApiException(message,status);
 }
 public async Task<T> GetAsync<T>(string path,bool auth=true,CancellationToken ct=default){using var response=await SendAsync(HttpMethod.Get,path,authenticate:auth,ct:ct);return await response.Content.ReadFromJsonAsync<T>(Json,ct)??throw new ApiException("La respuesta del servidor está incompleta.");}
 public async Task<T> WriteAsync<T>(HttpMethod method,string path,object body,bool auth=true,CancellationToken ct=default){using var response=await SendAsync(method,path,JsonContent.Create(body,options:Json),auth,ct);return await response.Content.ReadFromJsonAsync<T>(Json,ct)??throw new ApiException("La respuesta del servidor está incompleta.");}
 public async Task WriteAsync(HttpMethod method,string path,object? body=null,CancellationToken ct=default){using var response=await SendAsync(method,path,body==null?null:JsonContent.Create(body,options:Json),ct:ct);}
 public async Task<List<T>> AllAsync<T>(string path,bool auth=true){var items=new List<T>();for(var page=1;page<=100;page++){var batch=await GetAsync<Paged<T>>($"{path}?page={page}&pageSize=100",auth);items.AddRange(batch.Items);if(batch.Items.Count==0||items.Count>=batch.Total)break;}return items;}
 public async Task UploadVoucherAsync(string orderId,byte[] bytes,string name,string type){using var multipart=new MultipartFormDataContent();var file=new ByteArrayContent(bytes);file.Headers.ContentType=new MediaTypeHeaderValue(type);multipart.Add(file,"file",name);using var response=await SendAsync(HttpMethod.Post,$"orders/{Id(orderId)}/voucher",multipart);}
 public async Task<byte[]> BytesAsync(string path,CancellationToken ct=default){using var response=await SendAsync(HttpMethod.Get,path,ct:ct);return await response.Content.ReadAsByteArrayAsync(ct);}
}
