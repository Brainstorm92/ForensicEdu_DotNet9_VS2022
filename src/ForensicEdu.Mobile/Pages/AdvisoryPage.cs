using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Pages;
public sealed class AdvisoryPage:AcademyPage
{
 public AdvisoryPage(IServiceProvider s):base(s,"Asesoría pericial"){}
 protected override async void OnAppearing(){base.OnAppearing();await Run(async()=>{var products=await Api.AllAsync<Advisory>("advisories",false);var mine=Session.User==null?new List<Advisory>():await Api.GetAsync<List<Advisory>>("student/advisories");Body.Clear();Header("ATENCIÓN ESPECIALIZADA","Asesoría pericial","Consulta las especialidades y contacta a tu asesor después de la aprobación del pago.");foreach(var a in products){var owned=mine.FirstOrDefault(x=>x.Id==a.Id);Body.Add(Ui.Card(Ui.Stack(Ui.Eyebrow(a.Specialty),Ui.Text(a.Title,23,true),Ui.Text(a.Description,14,false,"Muted"),Ui.Text(a.Specialist,15,true),Ui.Action(owned!=null?"Contactar especialista":$"Solicitar · S/ {a.Price:0.00}",()=>Run(async()=>{if(Session.User==null){((App)Application.Current!).ShowLogin();return;}if(owned==null){await Push(new CheckoutPage(Services,a.Id,3,a.Title,a.Price));return;}var contact=owned.WhatsappContact.Trim();var number=new string(contact.Where(char.IsDigit).ToArray());Uri? uri=null;if(Uri.TryCreate(contact,UriKind.Absolute,out var direct)&&direct.Scheme=="https"&&(direct.Host=="wa.me"||direct.Host=="api.whatsapp.com"))uri=direct;else if(number.Length is >=9 and <=15)uri=new Uri("https://wa.me/"+number);if(uri==null)throw new ApiException("El canal del especialista todavía no está configurado.");await Browser.Default.OpenAsync(uri,BrowserLaunchMode.SystemPreferred);})))));}if(products.Count==0)Body.Add(Ui.Text("No hay asesorías disponibles por el momento.",14,false,"Muted"));});}
}
