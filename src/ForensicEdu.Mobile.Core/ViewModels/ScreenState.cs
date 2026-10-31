using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace ForensicEdu.Mobile.Core.ViewModels;
public class ScreenState:INotifyPropertyChanged
{
 private bool busy;private string error="";
 public bool IsBusy{get=>busy;private set{busy=value;Changed();}}
 public int ErrorStatus{get;private set;}
 public string Error{get=>error;private set{error=value;Changed();}}
 public event PropertyChangedEventHandler? PropertyChanged;
 protected void Changed([CallerMemberName]string? name=null)=>PropertyChanged?.Invoke(this,new(name));
 public async Task RunAsync(Func<Task> work){if(IsBusy)return;IsBusy=true;ErrorStatus=0;Error="";try{await work();}catch(OperationCanceledException){}catch(Exception e){ErrorStatus=(e as ForensicEdu.Mobile.Core.Services.ApiException)?.Status??0;Error=e is ForensicEdu.Mobile.Core.Services.ApiException?e.Message:"No pudimos completar la operación. Inténtalo nuevamente.";}finally{IsBusy=false;}}
}
