using ForensicEdu.Mobile.Core.Services;
using ForensicEdu.Mobile.Pages;
using ForensicEdu.Mobile.Controls;
namespace ForensicEdu.Mobile;
public partial class App:Application
{
 private readonly IServiceProvider services;
 public App(IServiceProvider services,AcademyApi api){InitializeComponent();this.services=services;Palette.Apply(Preferences.Default.Get("dark-theme",true));api.AuthenticationExpired+=()=>MainThread.BeginInvokeOnMainThread(async()=>{await services.GetRequiredService<AccountSession>().LogoutAsync();ShowLogin();});}
 protected override Window CreateWindow(IActivationState? state){var window=new Window(new NavigationPage(new LoginPage(services)));window.Created+=async(_,_)=>{try{if(await services.GetRequiredService<AccountSession>().RestoreAsync())ShowAcademy();}catch{}};return window;}
 public void ShowAcademy()=>Windows[0].Page=new AppShell(services);
 public void ShowLogin()=>Windows[0].Page=new NavigationPage(new LoginPage(services));
}
