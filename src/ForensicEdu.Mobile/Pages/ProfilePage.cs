using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Pages;
public sealed class ProfilePage:AcademyPage
{
 public ProfilePage(IServiceProvider s):base(s,"Mi perfil"){Shell.SetNavBarIsVisible(this,false);}
 protected override void OnAppearing(){base.OnAppearing();Body.Clear();Header("MI CUENTA",Session.User?.Name??"Bienvenido","Gestiona tu formación desde la academia.");if(Session.User is {} user)Body.Add(Ui.Card(Ui.Stack(Ui.Text(user.Name,22,true),Ui.Text(user.Email,13,false,"Muted"),Ui.Text("Documento: "+user.Dni,12,false,"Muted"))));var dark=new Switch{IsToggled=Preferences.Default.Get("dark-theme",true),OnColor=Palette.Accent};dark.Toggled+=(_,e)=>Palette.Apply(e.Value);Body.Add(Ui.Card(Ui.Row(Ui.Text("Modo nocturno",16,true),dark)));Body.Add(Ui.Action("Mis pagos",()=>Push(new PaymentsPage(Services)),true));Body.Add(Ui.Action("Asesoría pericial",()=>Push(new AdvisoryPage(Services)),true));Body.Add(Ui.Action("Validar certificado",()=>Push(new VerificationPage(Services)),true));Body.Add(Ui.Action("Soporte y consultas",()=>Push(new SupportPage(Services)),true));Body.Add(Ui.Action(Session.User==null?"Iniciar sesión":"Cerrar sesión",async()=>{await Session.LogoutAsync();((App)Application.Current!).ShowLogin();}));}
}
