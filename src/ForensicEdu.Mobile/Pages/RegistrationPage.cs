using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Pages;
public sealed class RegistrationPage:AcademyPage
{
 public RegistrationPage(IServiceProvider s):base(s,"Crear cuenta"){
  Header("FORMA PARTE","Comienza tu formación","Completa tus datos como en el registro de la web.");
  Entry names=Ui.Entry("Nombres"),last=Ui.Entry("Apellidos"),email=Ui.Entry("Correo electrónico",keyboard:Keyboard.Email),dni=Ui.Entry("Documento de identidad",keyboard:Keyboard.Numeric),phone=Ui.Entry("Teléfono",keyboard:Keyboard.Telephone),country=Ui.Entry("País"),institution=Ui.Entry("Institución educativa"),profession=Ui.Entry("Profesión"),password=Ui.Entry("Mínimo 12 caracteres",true),confirm=Ui.Entry("Repite tu contraseña",true);country.Text="Perú";
  var level=new Picker{Title="Nivel educativo",TextColor=(Color)Application.Current!.Resources["Ink"],ItemsSource=new[]{"Pregrado","Bachiller","Licenciatura","Maestría","Doctorado"}};level.SelectedIndex=2;
  foreach(var v in new[]{("Nombres",names),("Apellidos",last),("Correo",email),("Documento",dni),("Teléfono",phone),("País",country),("Institución",institution),("Profesión",profession),("Contraseña",password),("Confirmación",confirm)})Body.Add(Ui.Field(v.Item1,v.Item2));Body.Add(Ui.Field("Formación",level));
  var terms=new CheckBox{Color=Palette.Accent};var privacy=new CheckBox{Color=Palette.Accent};Body.Add(Ui.Row(Ui.Text("Acepto los términos y condiciones de la plataforma.",12),terms));Body.Add(Ui.Row(Ui.Text("Consiento el tratamiento de mis datos para gestionar mi cuenta y servicios educativos.",12),privacy));
  Body.Add(Ui.Action("Registrar cuenta",()=>Run(async()=>{if(password.Text!=confirm.Text)throw new ApiException("Las contraseñas deben coincidir.");if(!terms.IsChecked||!privacy.IsChecked)throw new ApiException("Debes aceptar los términos y el tratamiento de datos.");
   await Api.WriteAsync<System.Text.Json.JsonElement>(HttpMethod.Post,"auth/register",new RegisterInput(names.Text??"",last.Text??"",email.Text??"",password.Text??"",dni.Text??"",phone.Text??"",country.Text??"",level.SelectedItem?.ToString()??"",institution.Text??"",profession.Text??"",true,true),false);
   await DisplayAlertAsync("Cuenta creada","Ya puedes iniciar sesión con tu correo y contraseña.","Continuar");await Navigation.PopAsync();
  })));
 }
}
