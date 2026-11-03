namespace ForensicEdu.Mobile.Core.Models;
public static class StudentAccess
{
 public static bool SecureLink(string? value,out Uri? uri){uri=null;return Uri.TryCreate(value,UriKind.Absolute,out uri)&&uri.Scheme=="https"&&string.IsNullOrEmpty(uri.UserInfo);}
 public static bool EventAvailable(LiveEvent item,DateTimeOffset now)=>item.AccessStatus is not ("CANCELADO" or "FINALIZADO")&&item.EventDate!=default&&now>=item.EventDate.AddMinutes(-15);
 public static string EventLabel(LiveEvent item,DateTimeOffset now)=>item.AccessStatus switch{"CANCELADO"=>"Evento cancelado","FINALIZADO"=>"Evento finalizado",_=>EventAvailable(item,now)?"Ingresar al evento":"Abre 15 minutos antes del inicio"};
}
