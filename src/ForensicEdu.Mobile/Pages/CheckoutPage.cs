using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Pages;
public sealed class CheckoutPage:AcademyPage
{
 private CheckoutResult? order;private byte[]? voucher;private string filename="",mime="";
 public CheckoutPage(IServiceProvider s,string productId,int type,string title,decimal amount):base(s,"Registrar pago"){
 Header("MATRÍCULA",title,"Registra tu transferencia. El acceso se activa después de la aprobación del comprobante.");Body.Add(Ui.Card(Ui.Text($"Importe: S/ {amount:0.00}",25,true)));
 var bank=Ui.Entry("Banco de origen");var operation=Ui.Entry("Número de operación");var holder=Ui.Entry("Titular de la cuenta");var date=new DatePicker{Date=DateTime.Today,MaximumDate=DateTime.Today};var picked=Ui.Text("Sin comprobante seleccionado",12,false,"Muted");
 Body.Add(Ui.Field("Banco",bank));Body.Add(Ui.Field("Operación",operation));Body.Add(Ui.Field("Titular",holder));Body.Add(Ui.Field("Fecha",date));Body.Add(picked);
 Body.Add(Ui.Action("Adjuntar comprobante",()=>Run(async()=>{var file=await FilePicker.Default.PickAsync(new PickOptions{PickerTitle="Comprobante JPG, PNG o PDF"});if(file==null)return;var ext=Path.GetExtension(file.FileName).ToLowerInvariant();mime=ext switch{".jpg" or ".jpeg"=>"image/jpeg",".png"=>"image/png",".pdf"=>"application/pdf",_=>throw new ApiException("Selecciona un archivo JPG, PNG o PDF.")};using var stream=await file.OpenReadAsync();using var output=new MemoryStream();var buffer=new byte[81920];int count;while((count=await stream.ReadAsync(buffer))>0){if(output.Length+count>5*1024*1024)throw new ApiException("El comprobante admite hasta 5 MB.");output.Write(buffer,0,count);}voucher=output.ToArray();filename=file.FileName;picked.Text=filename;}),true));
 Body.Add(Ui.Action("Enviar comprobante",()=>Run(async()=>{if(Session.User==null)throw new ApiException("Inicia sesión para continuar.");if(voucher==null)throw new ApiException("Adjunta tu comprobante.");if(string.IsNullOrWhiteSpace(bank.Text)||string.IsNullOrWhiteSpace(operation.Text)||string.IsNullOrWhiteSpace(holder.Text))throw new ApiException("Completa los datos de la transferencia.");
 order??=await Api.WriteAsync<CheckoutResult>(HttpMethod.Post,"orders/checkout",new CheckoutInput(Session.User.Id,productId,type,amount,bank.Text.Trim(),operation.Text.Trim(),DateTime.SpecifyKind(date.Date??DateTime.Today,DateTimeKind.Local).ToUniversalTime(),holder.Text.Trim()));
 bank.IsEnabled=operation.IsEnabled=holder.IsEnabled=date.IsEnabled=false;
 await Api.UploadVoucherAsync(order.OrderId,voucher,filename,mime);Body.Clear();Header("PAGO REGISTRADO","Recibimos tu comprobante",$"Orden {order.OrderCode}");Body.Add(Ui.Card(Ui.Stack(Ui.Text("Pendiente de revisión",22,true),Ui.Text("Puedes consultar el estado en Mis pagos. Tu matrícula se habilita cuando el equipo aprueba el pago.",14,false,"Muted"))));Body.Add(Ui.Action("Ver mis pagos",()=>Push(new PaymentsPage(Services))));
 })));
 }
}
