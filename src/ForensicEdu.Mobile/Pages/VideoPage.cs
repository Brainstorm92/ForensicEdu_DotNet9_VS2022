using ForensicEdu.Mobile.Controls;
namespace ForensicEdu.Mobile.Pages;
public sealed class VideoPage:AcademyPage
{
 public VideoPage(IServiceProvider s,string title,string url):base(s,"Clase"){
  Header("CLASE VIRTUAL",title,"Material audiovisual de tu curso.");
  if(!Uri.TryCreate(url,UriKind.Absolute,out var uri)||uri.Scheme!="https"){Body.Add(Ui.Text("El enlace de la clase no está disponible."));return;}
  string? videoId=null;if(uri.Host is "youtube.com" or "www.youtube.com" or "m.youtube.com"){var query=System.Web.HttpUtility.ParseQueryString(uri.Query);videoId=query["v"];if(videoId==null&&uri.AbsolutePath.StartsWith("/embed/"))videoId=uri.Segments.Last();}else if(uri.Host=="youtu.be")videoId=uri.AbsolutePath.Trim('/');
  var user=Session.User;var mark=System.Net.WebUtility.HtmlEncode($"{user?.Name} · {user?.Dni} · Uso personal");
  if(videoId!=null&&System.Text.RegularExpressions.Regex.IsMatch(videoId,"^[A-Za-z0-9_-]{11}$")){
   Body.Add(new WebView{HeightRequest=280,Source=new HtmlWebViewSource{Html=$"<html><body style='margin:0;background:#0e1321'><iframe width='100%' height='250' src='https://www.youtube-nocookie.com/embed/{videoId}' allow='encrypted-media; fullscreen' allowfullscreen></iframe><div style='color:#46efcf;font:12px sans-serif'>{mark}</div></body></html>",BaseUrl="https://www.youtube-nocookie.com"}});
  }else Body.Add(Ui.Action("Abrir video del proveedor",async()=>{await Browser.Default.OpenAsync(uri,BrowserLaunchMode.SystemPreferred);}));
 }
}
