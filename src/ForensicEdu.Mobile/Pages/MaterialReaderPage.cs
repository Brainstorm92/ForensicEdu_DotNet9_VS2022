using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Pages;
public sealed class MaterialReaderPage:AcademyPage
{
 private readonly string enrollment;private readonly Material material;private readonly Services.IDocumentPreview preview;private int page;private DocumentAccess? access;private readonly Image image=new(){Aspect=Aspect.AspectFit};private readonly Label counter=Ui.Text("",12,false,"Muted");private IDispatcherTimer? timer;
 public MaterialReaderPage(IServiceProvider s,string enrollmentId,Material document):base(s,document.Title){enrollment=enrollmentId;material=document;preview=s.GetRequiredService<Services.IDocumentPreview>();}
 protected override async void OnAppearing(){base.OnAppearing();Services.GetRequiredService<Services.IScreenProtection>().SetProtected(true);await Run(async()=>{var path=$"student/enrollments/{Id(enrollment)}/documents/{Id(material.Id)}";access=await Api.GetAsync<DocumentAccess>(path);var bytes=await Api.BytesAsync(path+"/content");Body.Clear();Header("LECTURA PERSONAL",material.Title,"Material de uso exclusivo dentro de tu aula.");if(material.FileType.Contains("pdf",StringComparison.OrdinalIgnoreCase)){await preview.OpenPdfAsync(bytes);var grid=new Grid();grid.Add(image);grid.Add(new Label{Text=access.Watermark,TextColor=Color.FromArgb("#77858D"),Opacity=.55,Rotation=-25,FontSize=16,InputTransparent=true,HorizontalOptions=LayoutOptions.Center,VerticalOptions=LayoutOptions.Center});Body.Add(grid);Body.Add(Ui.Row(Ui.Action("Anterior",()=>Run(()=>Render(-1)),true),Ui.Action("Siguiente",()=>Run(()=>Render(1)),true)));Body.Add(counter);await Render(0);}else{var web=new WebView{HeightRequest=620,Source=new HtmlWebViewSource{Html=System.Text.Encoding.UTF8.GetString(bytes)}};web.HandlerChanged+=(_,_)=>{
#if ANDROID
 if(web.Handler?.PlatformView is Android.Webkit.WebView native){native.Settings.JavaScriptEnabled=false;native.Settings.AllowFileAccess=false;native.Settings.AllowContentAccess=false;native.Settings.CacheMode=Android.Webkit.CacheModes.NoCache;}
#endif
 };web.Navigating+=(_,e)=>{if(e.Url!="about:blank"&&!e.Url.StartsWith("data:text/html",StringComparison.OrdinalIgnoreCase))e.Cancel=true;};Body.Add(web);}Body.Add(Ui.Text(access.Watermark,10,false,"Muted"));timer=Dispatcher.CreateTimer();timer.Interval=TimeSpan.FromMinutes(1);timer.Tick+=async(_,_)=>{try{await Api.GetAsync<DocumentAccess>(path);}catch{timer.Stop();Body.Clear();Body.Add(Ui.Text("La sesión de lectura terminó. Vuelve a abrir el material.",18,true));}};timer.Start();});}
 private Task Render(int delta)=>RunPage(delta);
 private async Task RunPage(int delta){page=Math.Clamp(page+delta,0,Math.Max(0,preview.PageCount-1));var bytes=await preview.PageAsync(page);image.Source=ImageSource.FromStream(()=>new MemoryStream(bytes));counter.Text=$"Página {page+1} de {preview.PageCount}";}
 protected override void OnDisappearing(){timer?.Stop();preview.Dispose();Services.GetRequiredService<Services.IScreenProtection>().SetProtected(false);base.OnDisappearing();}
}
