using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Pages;

public sealed class LoginPage:AcademyPage
{
 public LoginPage(IServiceProvider services):base(services,"Acceso institucional")
 {
  NavigationPage.SetHasNavigationBar(this,false);Body.Spacing=18;
  var portal=Ui.Card(Ui.Eyebrow("●  PORTAL DE ACCESO INSTITUCIONAL"));portal.Padding=new Thickness(14,9);portal.HorizontalOptions=LayoutOptions.Center;Body.Add(portal);
  var shield=new Image{Source="logo.png",WidthRequest=160,HeightRequest=160,Aspect=Aspect.AspectFit,HorizontalOptions=LayoutOptions.Center};
  var name=Ui.Text("FORENSIQ",25,true);name.HorizontalTextAlignment=TextAlignment.Center;
  var subtitle=Ui.Text("Academia Pericial & Laboratorio Académico",11,false,"Muted");subtitle.HorizontalTextAlignment=TextAlignment.Center;
  var intro=Ui.Text("Ingresa a tu aula, materiales y certificados",10,false,"Outline");intro.HorizontalTextAlignment=TextAlignment.Center;
  var hero=Ui.Stack(shield,name,subtitle,intro);hero.Spacing=8;hero.Margin=new Thickness(0,4,0,8);Body.Add(hero);
  var user=Ui.Entry("Ej. usuario o correo@ejemplo.com");var password=Ui.Entry("Tu contraseña",true);
  var reveal=new ImageButton{Source="eye.png",WidthRequest=32,HeightRequest=38,Padding=8,BackgroundColor=Colors.Transparent};SemanticProperties.SetDescription(reveal,"Mostrar u ocultar contraseña");reveal.Clicked+=(_,_)=>password.IsPassword=!password.IsPassword;
  var login=Ui.Action("Iniciar sesión institucional   →",()=>Run(async()=>{if(string.IsNullOrWhiteSpace(user.Text)||string.IsNullOrWhiteSpace(password.Text))throw new ApiException("Completa usuario y contraseña.");await Session.LoginAsync(user.Text,password.Text);((App)Application.Current!).ShowAcademy();}));
  var hint=Ui.Text("Tu sesión se guarda de forma segura en este dispositivo.",10,false,"Outline");
  var form=Ui.Stack(Ui.Field("Correo electrónico o usuario",Ui.Input(user,"badge.png")),Ui.Field("Contraseña",Ui.Input(password,"lock.png",reveal)),hint,login);form.Spacing=16;form.Add(Ui.Text("O CONTINÚA CON",9,false,"Outline"));var google=Ui.Action("Continuar con Google",()=>Run(async()=>{var config=await Api.GetAsync<Core.Models.GoogleConfiguration>("auth/google/config",false);if(!config.Enabled)throw new ApiException("El acceso con Google no está disponible por ahora.");var credential=await Services.GetRequiredService<Services.IGoogleAccountPicker>().PickAsync(config.ClientId,config.Nonce);if(credential==null)return;var result=await Session.GoogleAsync(credential);if(result.Token!=null)((App)Application.Current!).ShowAcademy();else await Push(new GoogleCompletionPage(Services,credential,result));}),true);google.ImageSource="google.png";google.ContentLayout=new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left,10);google.SetDynamicResource(Button.TextColorProperty,"Ink");form.Add(google);var card=Ui.Card(form);card.Padding=16;Body.Add(card);
  var line=Ui.Text("O ACCEDE DIRECTAMENTE",9,false,"Outline");line.HorizontalTextAlignment=TextAlignment.Center;line.Margin=new Thickness(0,8,0,0);Body.Add(line);
  var verify=Ui.Stack(Ui.Text("Validar diploma / certificado   →",13,true),Ui.Text("Consulta pública. No requiere iniciar sesión.",10,false,"Muted"));
  var verification=Ui.Card(Ui.Row(new Image{Source="certificate.png",WidthRequest=28,HeightRequest=28},verify));var tap=new TapGestureRecognizer();tap.Tapped+=async(_,_)=>await Push(new VerificationPage(services));verification.GestureRecognizers.Add(tap);SemanticProperties.SetDescription(verification,"Validar certificado");Body.Add(verification);
  var register=Ui.Text("¿Aún no eres miembro de la comunidad?",11,false,"Muted");register.HorizontalTextAlignment=TextAlignment.Center;Body.Add(Ui.Stack(register,Ui.Action("Regístrate como estudiante   ↗",()=>Push(new RegistrationPage(services)),true)));
  Body.Add(Ui.Action("Explorar especialidades",()=>{((App)Application.Current!).ShowAcademy();return Task.CompletedTask;},true));
  var footer=Ui.Text("FORENSIQ · FORMACIÓN ESPECIALIZADA",9,false,"Outline");footer.HorizontalTextAlignment=TextAlignment.Center;footer.Margin=new Thickness(0,10);Body.Add(footer);
 }
}
