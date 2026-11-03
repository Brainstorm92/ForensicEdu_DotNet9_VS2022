using System.Net;
using System.Text.Json;
using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Pages;
public sealed class VideoPage:AcademyPage
{
 private VideoPlayback playback;private readonly string path;private WebView? player;private IDispatcherTimer? renewal;private bool refreshing;private bool visible;
 public VideoPage(IServiceProvider services,string title,VideoPlayback initial,string accessPath):base(services,"Clase"){playback=initial;path=accessPath;Header("CLASE VIRTUAL",title,"Material audiovisual de uso personal.");}
 protected override async void OnAppearing(){base.OnAppearing();visible=true;Services.GetRequiredService<Services.IScreenProtection>().SetProtected(true);await Run(async()=>{if(playback.ExpiresAt is {} expiry&&expiry<=DateTimeOffset.UtcNow.AddMinutes(1))playback=await Api.GetAsync<VideoPlayback>(path);Draw();});renewal=Dispatcher.CreateTimer();renewal.Interval=TimeSpan.FromSeconds(20);renewal.Tick+=async(_,_)=>{if(playback.PrivateVideo&&playback.ExpiresAt is {} expiry&&expiry<=DateTimeOffset.UtcNow.AddMinutes(1))await Renew();};renewal.Start();}
 private void Draw(double position=0,bool resume=false){
  if(!StudentAccess.SecureLink(playback.Url,out var uri))throw new ApiException("El enlace de la clase no está disponible.");
  if(player!=null){player.Source="about:blank";Body.Remove(player);player=null;}
  string? videoId=null;if(uri!.Host is "youtube.com" or "www.youtube.com" or "m.youtube.com" or "www.youtube-nocookie.com"){videoId=System.Web.HttpUtility.ParseQueryString(uri.Query)["v"];if(videoId==null&&(uri.AbsolutePath.StartsWith("/embed/")||uri.AbsolutePath.StartsWith("/shorts/")))videoId=uri.Segments.Last().Trim('/');}else if(uri.Host=="youtu.be")videoId=uri.AbsolutePath.Trim('/');
  var mark=WebUtility.HtmlEncode($"{Session.User?.Name} · {Session.User?.Dni} · Uso personal");
  var content=videoId!=null&&System.Text.RegularExpressions.Regex.IsMatch(videoId,"^[A-Za-z0-9_-]{11}$")?$"<iframe width='100%' height='260' src='https://www.youtube-nocookie.com/embed/{videoId}' allow='encrypted-media; fullscreen' allowfullscreen></iframe>":$"<video id='lesson' controls playsinline controlslist='nodownload noremoteplayback' disablepictureinpicture preload='metadata' style='width:100%;height:260px' src='{WebUtility.HtmlEncode(uri.AbsoluteUri)}'></video><script>const v=document.getElementById('lesson');v.addEventListener('loadedmetadata',()=>{{v.currentTime={JsonSerializer.Serialize(position)};if({(resume?"true":"false")})v.play().catch(()=>{{}});}});</script>";
  player=new WebView{HeightRequest=310,Source=new HtmlWebViewSource{Html=$"<html><meta name='referrer' content='no-referrer'><body style='margin:0;background:#0e1321;color:#46efcf;font:12px sans-serif' oncontextmenu='return false'><div style='position:relative'>{content}<span style='position:absolute;top:40%;left:8%;pointer-events:none;opacity:.65'>{mark}</span></div><p>Acceso personal al material del curso</p></body></html>",BaseUrl=Api.BaseAddress.GetLeftPart(UriPartial.Authority)}};
  player.Navigating+=(_,e)=>{if(e.Url!="about:blank"&&!e.Url.StartsWith("data:text/html",StringComparison.OrdinalIgnoreCase))e.Cancel=true;};Body.Add(player);
 }
 private async Task Renew(){if(refreshing||!visible)return;refreshing=true;await Run(async()=>{try{var position=0d;var resume=false;if(player!=null){var time=await player.EvaluateJavaScriptAsync("String(document.getElementById('lesson')?.currentTime || 0)");double.TryParse(time.Trim('"'),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out position);resume=(await player.EvaluateJavaScriptAsync("String(document.getElementById('lesson')?.paused === false)")).Trim('"')=="true";}var next=await Api.GetAsync<VideoPlayback>(path);if(!visible)return;playback=next;Draw(position,resume);}catch{if(player!=null){player.Source="about:blank";Body.Remove(player);player=null;}renewal?.Stop();Body.Add(Ui.Action("Reintentar reproducción",async()=>{await Renew();renewal?.Start();},true));throw;}finally{refreshing=false;}});}
 protected override void OnDisappearing(){visible=false;renewal?.Stop();if(player!=null)player.Source="about:blank";Services.GetRequiredService<Services.IScreenProtection>().SetProtected(false);base.OnDisappearing();}
}
