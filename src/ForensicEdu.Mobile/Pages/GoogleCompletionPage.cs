using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Pages;

public sealed class GoogleCompletionPage:AcademyPage
{
 public GoogleCompletionPage(IServiceProvider services,string credential,GoogleAccessResult result):base(services,"Completar acceso con Google")
 {
  Header("CUENTA GOOGLE",result.RequiresLinkPassword?"Vincula tu cuenta":"Completa tu perfil",result.Email);
  if(result.RequiresLinkPassword){var password=Ui.Entry("Contraseña de tu cuenta actual",true);Body.Add(Ui.Text("Ya existe una cuenta con ese correo. Confirma tu contraseña para vincularla con Google.",12,false,"Muted"));Body.Add(Ui.Field("Contraseña actual",Ui.Input(password,"lock.png")));Body.Add(Ui.Action("Vincular e ingresar",()=>Run(async()=>{if(string.IsNullOrWhiteSpace(password.Text))throw new ApiException("Ingresa tu contraseña actual.");var response=await Session.GoogleAsync(credential,linkPassword:password.Text);if(response.Token==null)throw new ApiException(response.Message);((App)Application.Current!).ShowAcademy();})));return;}
  Entry names=Ui.Entry("Nombres"),last=Ui.Entry("Apellidos"),dni=Ui.Entry("Documento de identidad",keyboard:Keyboard.Numeric),phone=Ui.Entry("Teléfono",keyboard:Keyboard.Telephone),country=Ui.Entry("País"),institution=Ui.Entry("Institución educativa"),profession=Ui.Entry("Profesión");names.Text=result.Nombres;last.Text=result.Apellidos;country.Text="Perú";
  foreach(var field in new[]{("Nombres",names),("Apellidos",last),("Documento",dni),("Teléfono",phone),("País",country),("Institución",institution),("Profesión",profession)})Body.Add(Ui.Field(field.Item1,Ui.Input(field.Item2,"badge.png")));
  var level=new Picker{Title="Nivel educativo",ItemsSource=new[]{"Pregrado","Bachiller","Licenciatura","Maestría","Doctorado"}};level.SetDynamicResource(Picker.TextColorProperty,"Ink");level.SelectedIndex=2;Body.Add(Ui.Field("Formación",level));
  var terms=new CheckBox{Color=Palette.Accent};var privacy=new CheckBox{Color=Palette.Accent};Body.Add(Ui.Row(Ui.Text("Acepto los términos y condiciones de la plataforma.",11),terms));Body.Add(Ui.Row(Ui.Text("Consiento el tratamiento de datos para gestionar mi cuenta educativa.",11),privacy));
  Body.Add(Ui.Action("Crear cuenta con Google",()=>Run(async()=>{if(!terms.IsChecked||!privacy.IsChecked)throw new ApiException("Acepta los términos y el tratamiento de datos.");var profile=new GoogleProfile(names.Text??"",last.Text??"",dni.Text??"",phone.Text??"","DNI",country.Text??"",level.SelectedItem?.ToString()??"",institution.Text??"",profession.Text??"",true,true);var response=await Session.GoogleAsync(credential,profile);if(response.Token==null)throw new ApiException(response.Message);((App)Application.Current!).ShowAcademy();})));
 }
}
