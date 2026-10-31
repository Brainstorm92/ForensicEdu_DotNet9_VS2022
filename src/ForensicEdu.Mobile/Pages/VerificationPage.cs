using ForensicEdu.Mobile.Controls;
using ForensicEdu.Mobile.Core.Models;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Pages;
public sealed class VerificationPage:AcademyPage
{
 public VerificationPage(IServiceProvider s,string initial=""):base(s,"Validar certificado"){
 Header("VERIFICACIÓN PÚBLICA","Valida un certificado","Ingresa el código o pega el enlace de validación del QR.");var code=Ui.Entry("Código del certificado");code.Text=initial;var result=Ui.Stack();Body.Add(Ui.Field("Código o enlace",code));Body.Add(Ui.Action("Validar certificado",()=>Run(async()=>{var value=(code.Text??"").Trim();if(Uri.TryCreate(value,UriKind.Absolute,out var uri))value=uri.AbsolutePath.TrimEnd('/').Split('/').Last();if(value.Length==0||value.Length>200)throw new ApiException("Ingresa un código válido.");var info=await Api.GetAsync<Verification>("certificates/verify/"+Id(value),false);result.Clear();result.Add(Ui.Card(Ui.Stack(Ui.Eyebrow(info.IsValid?"CERTIFICADO VÁLIDO":"NO VÁLIDO"),Ui.Text(info.Student,22,true),Ui.Text(info.Course,18,true),Ui.Text($"{info.Code} · {info.Status}\nCalificación: {info.Grade}/20",13,false,"Muted"))));})));Body.Add(result);
 }
}
