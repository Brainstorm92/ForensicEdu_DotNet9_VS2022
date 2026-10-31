using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Pages;
public sealed class EventsPage:AcademyPage
{
 public EventsPage(IServiceProvider s):base(s,"Eventos"){Shell.SetNavBarIsVisible(this,false);}
 protected override async void OnAppearing(){base.OnAppearing();await Run(async()=>{var events=await Api.AllAsync<LiveEvent>("events",false);var mine=Session.User==null?new List<LiveEvent>():await Api.GetAsync<List<LiveEvent>>("student/events");Body.Clear();Header("EN VIVO","Eventos y congresos","Encuentros con especialistas de ciencias forenses.");foreach(var e in events){var enrolled=mine.Any(x=>x.Id==e.Id);var stack=Ui.Stack(Ui.Eyebrow(e.EventDate.ToLocalTime().ToString("dd MMM yyyy")),Ui.Text(e.Title,22,true),Ui.Text(e.Description,14,false,"Muted"),Ui.Text($"{e.Speaker} · {e.TimeSlot}",12,false,"Muted"));stack.Add(Ui.Action(enrolled?"Acceder al evento":$"Inscribirme · S/ {e.Price:0.00}",()=>Run(async()=>{if(Session.User==null){((App)Application.Current!).ShowLogin();return;}if(enrolled){var link=await Api.GetAsync<LinkResult>($"student/events/{Id(e.Id)}/access");if(!Uri.TryCreate(link.Url,UriKind.Absolute,out var uri)||uri.Scheme!="https")throw new ApiException("El enlace del evento no está disponible.");await Browser.Default.OpenAsync(uri,BrowserLaunchMode.SystemPreferred);}else await Push(new CheckoutPage(Services,e.Id,2,e.Title,e.Price));})));Body.Add(Ui.Card(stack));}if(events.Count==0)Body.Add(Ui.Text("Próximamente anunciaremos nuevos eventos.",14,false,"Muted"));});}
}
