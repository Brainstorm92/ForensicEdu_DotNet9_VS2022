using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
namespace ForensicEdu.Mobile.Pages;

public sealed class MyCoursesPage:AcademyPage
{
 private List<Course> courses=[];private List<Enrollment> enrollments=[];private string filter="Todos";
 private readonly VerticalStackLayout cards=new(){Spacing=12};
 public MyCoursesPage(IServiceProvider s):base(s,"Mis cursos"){Shell.SetNavBarIsVisible(this,false);}
 protected override async void OnAppearing()
 {
  base.OnAppearing();if(!RequireAccount())return;
  await Run(async()=>{courses=await Api.GetAsync<List<Course>>("courses",false);enrollments=await Api.GetAsync<List<Enrollment>>("student/enrollments");Draw();});
 }
 private static bool PendingExam(Enrollment e)=>e.ProgressPercent==100&&e.FinalGrade is not >=13;
 private void Draw()
 {
  Body.Clear();Header("CURSOS","Mi formación","Expediente académico y plataforma de acreditación");var user=Session.User!;
  var credential=Ui.Stack(Ui.Row(Ui.Eyebrow("● EXPEDIENTE ACADÉMICO"),new Image{Source="brand.png",WidthRequest=18,HeightRequest=18}),Ui.Text(user.Name,16,true),Ui.Text("Documento de identidad: "+user.Dni,10,false,"Muted"),Ui.Card(Ui.Stack(Ui.Eyebrow("ESTADO DE MATRÍCULA"),Ui.Text(user.Validity,11,true))));credential.Spacing=6;
  var credentialCard=Ui.Card(credential);credentialCard.SetDynamicResource(BackgroundColorProperty,"Raised");Body.Add(credentialCard);
  var tabs=new HorizontalStackLayout{Spacing=6};
  foreach(var name in new[]{"Todos","En progreso","Por evaluar","Aprobados"}){var count=enrollments.Count(e=>Matches(e,name));var button=Ui.Action($"{name}  {count}",()=>{filter=name;Draw();return Task.CompletedTask;},name!=filter);button.FontSize=9;button.MinimumHeightRequest=30;button.Padding=new Thickness(10,5);button.CornerRadius=14;tabs.Add(button);}
  Body.Add(new ScrollView{Orientation=ScrollOrientation.Horizontal,Content=tabs,HorizontalScrollBarVisibility=ScrollBarVisibility.Never});cards.Clear();if(cards.Parent is Layout oldParent)oldParent.Remove(cards);Body.Add(cards);
  foreach(var e in enrollments.Where(e=>Matches(e,filter)))
  {
   var course=courses.FirstOrDefault(c=>c.Id==e.CourseId);var status=e.FinalGrade>=13?"✓ Aprobado":PendingExam(e)?"Evaluación pendiente":"En curso";
   var text=Ui.Stack(Ui.Eyebrow(course?.ShortCode??"CURSO"),Ui.Text(course?.Title??"Curso matriculado",16,true));text.Spacing=5;
   var head=new Grid{ColumnDefinitions={new ColumnDefinition(54),new ColumnDefinition(GridLength.Star)},ColumnSpacing=12};var image=Api.PublicImage(course?.ImageUrl??"");
   head.Add(image==null?new Image{Source="courses.png",HeightRequest=44,WidthRequest=44}:new Image{Source=image,HeightRequest=54,WidthRequest=54,Aspect=Aspect.AspectFill});head.Add(text,1);
   var content=Ui.Stack(Ui.Row(Ui.Eyebrow(status),Ui.Text($"{course?.AcademicHours??0} horas",9,false,"Muted")),head,Ui.Row(new Image{Source="profile.png",WidthRequest=19,HeightRequest=19},Ui.Text(course?.Instructor??"Docente del curso",10,false,"Muted")),Ui.Row(Ui.Text("Progreso modular",10,false,"Muted"),Ui.Text(e.ProgressPercent+"%",11,true)),Ui.Progress(e.ProgressPercent/100d),Ui.Action(e.ProgressPercent==100?"Ingresar al aula y examen   →":"Continuar formación   →",()=>Push(new ClassroomPage(Services,e))));content.Spacing=10;cards.Add(Ui.Card(content));
  }
  if(cards.Count==0)cards.Add(Ui.Card(Ui.Stack(Ui.Text("No hay cursos en esta sección",16,true),Ui.Text("Las matrículas aparecen después de aprobar el pago.",11,false,"Muted"))));
 }
 private static bool Matches(Enrollment e,string name)=>name switch{"En progreso"=>e.ProgressPercent<100,"Por evaluar"=>PendingExam(e),"Aprobados"=>e.FinalGrade>=13,_=>true};
}
