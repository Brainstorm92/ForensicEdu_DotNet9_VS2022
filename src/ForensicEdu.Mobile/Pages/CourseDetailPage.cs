using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
namespace ForensicEdu.Mobile.Pages;
public sealed class CourseDetailPage:AcademyPage
{
 public CourseDetailPage(IServiceProvider s,Course course):base(s,"Detalle y matrícula"){
  Header("ESPECIALIDAD FORENSE",course.Title,course.ShortDescription);var image=Api.PublicImage(course.ImageUrl);if(image!=null)Body.Add(new Image{Source=image,HeightRequest=210,Aspect=Aspect.AspectFill});
  Body.Add(Ui.Card(Ui.Stack(Ui.Eyebrow("TU FORMACIÓN"),Ui.Row(Ui.Text(course.PriceLabel,28,true),Ui.Text($"{course.AcademicHours} h académicas",12)),Ui.Text(course.Modalidad,13,false,"Muted"),Ui.Text("Docente: "+course.Instructor,14,true),Ui.Action("Matricularme",()=>{if(Session.User==null){((App)Application.Current!).ShowLogin();return Task.CompletedTask;}return Push(new CheckoutPage(s,course.Id,1,course.Title,course.SalePrice));})),true));
  Body.Add(Ui.Text("Acerca de esta especialidad",21,true));Body.Add(Ui.Text(course.FullDescription.Length>0?course.FullDescription:course.ShortDescription));if(course.Requisitos.Length>0)Body.Add(Ui.Card(Ui.Stack(Ui.Text("Requisitos",16,true),Ui.Text(course.Requisitos,13,false,"Muted"))));
  Body.Add(Ui.Text("Contenido del curso",21,true));foreach(var m in course.Modules.OrderBy(x=>x.OrderIndex))Body.Add(Ui.Card(Ui.Stack(Ui.Text(m.Title,16,true),Ui.Text($"{m.Duration} · {m.Documents.Count} materiales",12,false,"Muted"))));
 }
}
