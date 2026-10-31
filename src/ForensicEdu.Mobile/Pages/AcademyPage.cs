using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Services;
using ForensicEdu.Mobile.Core.ViewModels;
namespace ForensicEdu.Mobile.Pages;
public abstract class AcademyPage:ContentPage
{
 protected readonly IServiceProvider Services;protected AcademyApi Api=>Services.GetRequiredService<AcademyApi>();protected AccountSession Session=>Services.GetRequiredService<AccountSession>();
 protected VerticalStackLayout Body{get;}=new(){Spacing=16,Padding=new Thickness(16,12,16,28)};
 protected ScreenState State{get;}=new();private readonly Label error=Ui.Text("",13,false,"Muted");private readonly ActivityIndicator loading=new(){Color=Palette.Accent,IsVisible=false};
 protected AcademyPage(IServiceProvider services,string title){Services=services;Title=title;SetDynamicResource(BackgroundColorProperty,"Canvas");Shell.SetNavBarIsVisible(this,true);var grid=new Grid{RowDefinitions={new RowDefinition(GridLength.Auto),new RowDefinition(GridLength.Auto),new RowDefinition(GridLength.Star)}};
  error.Margin=new Thickness(18,4);error.IsVisible=false;loading.HeightRequest=20;grid.Add(error);grid.Add(loading,0,1);grid.Add(new ScrollView{Content=Body},0,2);Content=grid;
  State.PropertyChanged+=(_,_)=>{error.Text=State.Error;error.IsVisible=!string.IsNullOrWhiteSpace(State.Error);loading.IsRunning=State.IsBusy;loading.IsVisible=State.IsBusy;Body.IsEnabled=!State.IsBusy;};
 }
 protected Task Run(Func<Task> action)=>State.RunAsync(action);
 protected Task Push(Page page)=>Navigation.PushAsync(page);
 protected bool RequireAccount(){if(Session.User!=null)return true;Body.Clear();Body.Add(Ui.Brand());Body.Add(Ui.Card(Ui.Stack(Ui.Text("Tu formación te espera",24,true),Ui.Text("Inicia sesión para consultar tu expediente, pagos y materiales.",14,false,"Muted"),Ui.Action("Iniciar sesión",()=>{((App)Application.Current!).ShowLogin();return Task.CompletedTask;}))));return false;}
 protected override void OnAppearing(){base.OnAppearing();Services.GetRequiredService<ForensicEdu.Mobile.Services.IScreenProtection>().SetProtected(false);}
 protected new static string Id(string value)=>AcademyApi.Id(value);
 protected void Header(string eyebrow,string title,string subtitle){Body.Add(Ui.Brand());Body.Add(Ui.Stack(Ui.Eyebrow(eyebrow),Ui.Text(title,22,true),Ui.Text(subtitle,11,false,"Muted")));}
}
