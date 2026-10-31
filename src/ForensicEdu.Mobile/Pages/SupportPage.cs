using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Pages;
public sealed class SupportPage:AcademyPage
{
 public SupportPage(IServiceProvider s):base(s,"Soporte"){}
 protected override async void OnAppearing(){base.OnAppearing();if(!RequireAccount())return;await Run(async()=>{var tickets=await Api.GetAsync<System.Text.Json.JsonElement>("student/tickets");Body.Clear();Header("ESTAMOS PARA AYUDARTE","Centro de soporte","Envía tu consulta al equipo de la academia.");var subject=Ui.Entry("Asunto");var category=Ui.Entry("Categoría");category.Text="Académico";var message=new Editor{Placeholder="Describe tu consulta",HeightRequest=140};Body.Add(Ui.Field("Asunto",subject));Body.Add(Ui.Field("Categoría",category));Body.Add(Ui.Field("Consulta",message));Body.Add(Ui.Action("Enviar consulta",()=>Run(async()=>{await Api.WriteAsync(HttpMethod.Post,"student/tickets",new{subject=subject.Text??"",category=category.Text??"",message=message.Text??""});await DisplayAlertAsync("Consulta enviada","El equipo revisará tu solicitud.","Aceptar");message.Text="";})));if(tickets.ValueKind==System.Text.Json.JsonValueKind.Array)foreach(var t in tickets.EnumerateArray()){var title=t.TryGetProperty("subject",out var v)?v.GetString():"Consulta";var status=t.TryGetProperty("status",out var state)?state.ToString():"";Body.Add(Ui.Card(Ui.Stack(Ui.Text(title??"Consulta",16,true),Ui.Text(status,12,false,"Muted"))));}});}
}
