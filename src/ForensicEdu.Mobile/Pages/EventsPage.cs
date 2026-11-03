using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Pages;
public sealed class EventsPage:AcademyPage
{
 private List<LiveEvent> events=[];private List<LiveEvent> mine=[];private IDispatcherTimer? ticker;private readonly List<(LiveEvent Item,Button Button)> accessButtons=[];
 public EventsPage(IServiceProvider s):base(s,"Eventos"){Shell.SetNavBarIsVisible(this,false);}
 protected override async void OnAppearing(){base.OnAppearing();await Reload();ticker=Dispatcher.CreateTimer();ticker.Interval=TimeSpan.FromSeconds(30);ticker.Tick+=(_,_)=>UpdateAccess();ticker.Start();}
 protected override void OnDisappearing(){ticker?.Stop();base.OnDisappearing();}
 private Task Reload()=>Run(async()=>{events=await Api.AllAsync<LiveEvent>("events",false);mine=Session.User==null?[]:await Api.GetAsync<List<LiveEvent>>("student/events");Draw();});
 private void UpdateAccess(){foreach(var (item,button) in accessButtons){button.Text=StudentAccess.EventLabel(item,DateTimeOffset.UtcNow);button.IsEnabled=StudentAccess.EventAvailable(item,DateTimeOffset.UtcNow);}}
 private void Draw(){Body.Clear();accessButtons.Clear();Header("EN VIVO","Eventos y congresos","Accede a tus salas con inscripción acreditada desde 15 minutos antes del inicio.");Body.Add(Ui.Action("Actualizar eventos",Reload,true));
  var listed=mine.Select(m=>events.FirstOrDefault(e=>e.Id==m.Id) is {} detail?detail with{AccessStatus=m.AccessStatus,EventDate=m.EventDate,TimeSlot=m.TimeSlot}:m).Concat(events.Where(e=>mine.All(m=>m.Id!=e.Id))).ToList();
  foreach(var item in listed){var enrolled=mine.Any(e=>e.Id==item.Id);var stack=Ui.Stack(Ui.Eyebrow(enrolled?"INSCRIPCIÓN ACREDITADA":"PRÓXIMO EVENTO"),Ui.Text(item.Title,22,true),Ui.Text(item.Description,14,false,"Muted"),Ui.Text($"{item.EventDate.ToLocalTime():dd/MM/yyyy HH:mm} (hora local) · {item.TimeSlot}",12,false,"Muted"),Ui.Text("Expositor: "+item.Speaker,12,false,"Muted"));
   if(enrolled){var button=Ui.Action("Ingresar al evento",()=>Run(async()=>{var link=await Api.GetAsync<LinkResult>($"student/events/{Id(item.Id)}/access");if(!StudentAccess.SecureLink(link.Url,out var uri))throw new ApiException("El enlace del evento no está disponible. Contacta a soporte.");await Browser.Default.OpenAsync(uri!,BrowserLaunchMode.SystemPreferred);}));accessButtons.Add((item,button));stack.Add(button);stack.Add(Ui.Text("La sala se abre en tu navegador. Acceso personal con pago aprobado.",11,false,"Muted"));}
   else if(item.AccessStatus is not ("CANCELADO" or "FINALIZADO"))stack.Add(Ui.Action($"Inscribirme · S/ {item.Price:0.00}",async()=>{if(Session.User==null){((App)Application.Current!).ShowLogin();return;}await Push(new CheckoutPage(Services,item.Id,2,item.Title,item.Price));}));else stack.Add(Ui.Text(StudentAccess.EventLabel(item,DateTimeOffset.UtcNow),12));Body.Add(Ui.Card(stack));}
  UpdateAccess();if(listed.Count==0)Body.Add(Ui.Text("Próximamente anunciaremos nuevos eventos.",14,false,"Muted"));
 }
}
