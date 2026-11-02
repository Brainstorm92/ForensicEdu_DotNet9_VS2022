using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Pages;
public sealed class PaymentsPage:AcademyPage
{
 public PaymentsPage(IServiceProvider s):base(s,"Mis pagos"){}
 protected override async void OnAppearing(){base.OnAppearing();if(!RequireAccount())return;await Run(async()=>{var payments=await Api.AllAsync<Payment>("student/payments");Body.Clear();Header("MI EXPEDIENTE","Mis pagos","Consulta la revisión de tus comprobantes.");foreach(var p in payments)Body.Add(Ui.Card(Ui.Stack(Ui.Eyebrow(p.StatusLabel),Ui.Text(p.ProductTitle,20,true),Ui.Text($"S/ {p.Amount:0.00}",23,true),Ui.Text($"{p.OrderCode} · {p.BankName}\nOperación {p.OperationNumber} · {p.RegisteredAt.ToLocalTime():dd/MM/yyyy}",12,false,"Muted"),Ui.Text(p.HasVoucher?"Comprobante recibido":"Comprobante pendiente de adjuntar",12))));if(payments.Count==0)Body.Add(Ui.Text("Aún no tienes pagos registrados.",14,false,"Muted"));});}
}
