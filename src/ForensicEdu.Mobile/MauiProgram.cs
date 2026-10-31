using ForensicEdu.Mobile.Core.Services;
using ForensicEdu.Mobile.Services;
namespace ForensicEdu.Mobile;
public static class MauiProgram
{
 public static MauiApp CreateMauiApp(){
#if ANDROID
  Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("ForenseField",(handler,view)=>{handler.PlatformView.BackgroundTintList=Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);handler.PlatformView.SetPadding(0,0,0,0);});
#endif
  var builder=MauiApp.CreateBuilder().UseMauiApp<App>().ConfigureFonts(fonts=>{fonts.AddFont("OpenSans-Regular.ttf","Jakarta");fonts.AddFont("OpenSans-Semibold.ttf","JakartaBold");});
  builder.Services.AddSingleton<IGoogleAccountPicker,GoogleAccountPicker>();builder.Services.AddSingleton<ITokenStore,DeviceTokenStore>();builder.Services.AddSingleton<MobileSettings>();
  builder.Services.AddSingleton(sp=>new HttpClient(new HttpClientHandler{AllowAutoRedirect=false}){BaseAddress=sp.GetRequiredService<MobileSettings>().ApiBaseAddress,Timeout=TimeSpan.FromSeconds(25)});
  builder.Services.AddSingleton<AcademyApi>();builder.Services.AddSingleton<AccountSession>();builder.Services.AddSingleton(TimeProvider.System);
  builder.Services.AddSingleton<IScreenProtection,ScreenProtection>();builder.Services.AddTransient<IDocumentPreview,DocumentPreview>();return builder.Build();}
}
