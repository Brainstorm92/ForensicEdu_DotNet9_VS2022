using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
using ForensicEdu.Mobile.Core.ViewModels;
namespace ForensicEdu.Mobile.Pages;
public sealed class CatalogPage:AcademyPage
{
 private readonly CatalogViewModel model;private readonly VerticalStackLayout list=new(){Spacing=16};private readonly SearchBar search=new(){Placeholder="Buscar especialidad o curso"};
 public CatalogPage(IServiceProvider s):base(s,"Catálogo"){
  model=new(Api);Shell.SetNavBarIsVisible(this,false);Header("APRENDE. ESPECIALÍZATE. AVANZA.","Catálogo de especialidades","Formación especializada en ciencias forenses.");
  Body.Add(Ui.Card(Ui.Stack(Ui.Eyebrow("ACADEMIA FORENSE"),Ui.Text("Ciencia, formación y precisión.",26,true),Ui.Text("Cursos, evaluaciones y certificados verificables en una sola academia.",13,false,"Muted")),true));
  search.TextChanged+=(_,_)=>Draw();Body.Add(search);Body.Add(Ui.Action("Actualizar catálogo",Load,true));Body.Add(list);
 }
 protected override async void OnAppearing(){base.OnAppearing();await Load();}
 private Task Load()=>Run(async()=>{await model.LoadAsync();if(!string.IsNullOrWhiteSpace(model.Error))throw new Core.Services.ApiException(model.Error);Draw();});
 private void Draw(){list.Clear();foreach(var c in model.Search(search.Text??""))list.Add(CourseCard(c));if(list.Count==0)list.Add(Ui.Card(Ui.Text("No encontramos cursos con ese criterio.",14,false,"Muted")));}
 private View CourseCard(Course c){var content=Ui.Stack();var image=Api.PublicImage(c.ImageUrl);if(image!=null)content.Add(new Image{Source=image,HeightRequest=170,Aspect=Aspect.AspectFill});content.Add(Ui.Row(Ui.Eyebrow(c.Category.Length>0?c.Category:"CURSO FORENSE"),Ui.Text(c.PriceLabel,17,true)));content.Add(Ui.Text(c.Title,21,true));content.Add(Ui.Text(c.ShortDescription,13,false,"Muted"));content.Add(Ui.Text($"{c.AcademicHours} horas académicas · {c.Instructor}",12,false,"Muted"));content.Add(Ui.Action("Ver especialidad  →",()=>Push(new CourseDetailPage(Services,c))));return Ui.Card(content);}
}
