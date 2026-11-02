using Android.Runtime;
using AndroidX.Credentials;
using Xamarin.GoogleAndroid.Libraries.Identity.GoogleId;
using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Services;

public sealed class GoogleAccountPicker:IGoogleAccountPicker
{
 public async Task<string?> PickAsync(string clientId,string nonce)
 {
  var activity=Platform.CurrentActivity??throw new ApiException("Vuelve a abrir la app para iniciar sesión con Google.");
  var option=new GetSignInWithGoogleOption.Builder(clientId).SetNonce(nonce).Build();
  var request=new GetCredentialRequest.Builder().AddCredentialOption(option).Build();
  var completion=new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
  using var signal=new Android.OS.CancellationSignal();using var callback=new CredentialCallback(completion);
  var manager=CredentialManager.Create(activity);
  manager.GetCredentialAsync(activity,request,signal,AndroidX.Core.Content.ContextCompat.GetMainExecutor(activity),callback);
  try{return await completion.Task.WaitAsync(TimeSpan.FromMinutes(2));}
  catch(TimeoutException){signal.Cancel();throw new ApiException("El acceso con Google tardó demasiado. Inténtalo nuevamente.");}
 }
 private sealed class CredentialCallback(TaskCompletionSource<string?> completion):Java.Lang.Object,ICredentialManagerCallback
 {
  public void OnResult(Java.Lang.Object? result)
  {
   try{var response=result?.JavaCast<GetCredentialResponse>();if(response?.Credential is not CustomCredential credential)throw new InvalidOperationException();var google=GoogleIdTokenCredential.CreateFrom(credential.Data);if(string.IsNullOrWhiteSpace(google.IdToken))throw new InvalidOperationException();completion.TrySetResult(google.IdToken);}
   catch{completion.TrySetException(new ApiException("No pudimos obtener tu identidad de Google. Vuelve a intentarlo."));}
  }
  public void OnError(Java.Lang.Object? error)
  {
   if(error?.GetType().Name.Contains("Cancellation",StringComparison.OrdinalIgnoreCase)==true){completion.TrySetResult(null);return;}
   completion.TrySetException(new ApiException("No se pudo acceder con Google. Comprueba tu cuenta del dispositivo e inténtalo nuevamente."));
  }
 }
}
