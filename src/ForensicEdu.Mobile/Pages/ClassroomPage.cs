using System.Text.Json;
using System.Net;
using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
namespace ForensicEdu.Mobile.Pages;
public sealed class ClassroomPage:AcademyPage
{
 private Enrollment enrollment;private Classroom? classroom;private string active="";
 public ClassroomPage(IServiceProvider s,Enrollment e):base(s,"Aula virtual"){enrollment=e;}
 protected override async void OnAppearing(){base.OnAppearing();if(!RequireAccount())return;await Reload();}
 private Task Reload()=>Run(async()=>{classroom=await Api.GetAsync<Classroom>($"student/enrollments/{Id(enrollment.Id)}/classroom");enrollment=(await Api.GetAsync<List<Enrollment>>("student/enrollments")).First(x=>x.Id==enrollment.Id);if(active.Length==0)active=classroom.Modules.FirstOrDefault()?.Id??"";Draw();});
 private void Draw(){if(classroom==null)return;Body.Clear();Header("AULA VIRTUAL",classroom.Course.Title,$"{classroom.ProgressPercent}% completado · {classroom.Course.AcademicHours} horas");Body.Add(Ui.Progress(classroom.ProgressPercent/100d));
  var completed=JsonSerializer.Deserialize<List<string>>(enrollment.CompletedModulesJson)??[];var ordered=classroom.Modules.OrderBy(x=>x.OrderIndex).ToList();var module=ordered.FirstOrDefault(x=>x.Id==active);
  if(module!=null){var unlocked=ordered.TakeWhile(x=>x.Id!=module.Id).All(x=>completed.Contains(x.Id));var card=Ui.Stack(Ui.Eyebrow("CLASE ACTUAL"),Ui.Text(module.Title,22,true),Ui.Text(module.Duration,12,false,"Muted"));
   var video=Ui.Action("Reproducir clase",()=>Run(async()=>{var link=await Api.GetAsync<LinkResult>($"student/enrollments/{Id(enrollment.Id)}/modules/{Id(module.Id)}/video");await Push(new VideoPage(Services,module.Title,link.Url));}));video.IsEnabled=unlocked;card.Add(video);
   var finish=Ui.Action(completed.Contains(module.Id)?"Módulo completado ✓":"Marcar como completado",()=>Run(async()=>{await Api.WriteAsync(HttpMethod.Put,$"student/enrollments/{Id(enrollment.Id)}/modules/{Id(module.Id)}/complete");classroom=await Api.GetAsync<Classroom>($"student/enrollments/{Id(enrollment.Id)}/classroom");enrollment=(await Api.GetAsync<List<Enrollment>>("student/enrollments")).First(x=>x.Id==enrollment.Id);Draw();}));finish.IsEnabled=unlocked&&!completed.Contains(module.Id);card.Add(finish);Body.Add(Ui.Card(card,true));
   Body.Add(Ui.Text("Materiales de estudio",21,true));foreach(var material in module.Documents)Body.Add(Ui.Card(Ui.Stack(Ui.Eyebrow(material.FileType+" · "+material.FileSize),Ui.Text(material.Title,16,true),Ui.Action("Leer material protegido",()=>Push(new MaterialReaderPage(Services,enrollment.Id,material)),true))));
  }
  Body.Add(Ui.Text("Estructura del curso",21,true));foreach(var m in ordered)Body.Add(Ui.Action((completed.Contains(m.Id)?"✓ ":"○ ")+m.Title,()=>{active=m.Id;Draw();return Task.CompletedTask;},true));
  var exam=Ui.Stack(Ui.Eyebrow("ACREDITACIÓN"),Ui.Text("Evaluación final",22,true),Ui.Text("10 minutos · Nota mínima 13/20 · Máximo dos intentos",13,false,"Muted"),Ui.Text($"Intentos utilizados: {enrollment.AttemptsUsed}/2",12,false,"Muted"));
  if(enrollment.FinalGrade>=13)exam.Add(Ui.Text($"Curso aprobado · {enrollment.FinalGrade}/20",18,true));else{var start=Ui.Action("Iniciar evaluación",async()=>{if(await DisplayAlertAsync("Evaluación cronometrada","El intento se registra al iniciar y el tiempo continúa si cierras la app. ¿Continuar?","Iniciar","Cancelar"))await Push(new ExamPage(Services,enrollment.Id));});var waiting=enrollment.SecondAttemptAvailableAt is {} next&&next>DateTimeOffset.UtcNow;if(waiting){var available=enrollment.SecondAttemptAvailableAt!.Value.ToLocalTime();exam.Add(Ui.Text($"Próximo intento: {available:dd/MM/yyyy} a las {available:HH:mm} (hora local)",13,false,"Muted"));}start.IsEnabled=!waiting&&classroom.ProgressPercent==100&&(enrollment.AttemptsUsed<2||enrollment.ExamStatus==2);exam.Add(start);}Body.Add(Ui.Card(exam));
 }
}
