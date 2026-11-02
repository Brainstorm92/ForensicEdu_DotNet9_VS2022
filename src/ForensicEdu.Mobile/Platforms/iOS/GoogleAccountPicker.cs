namespace ForensicEdu.Mobile.Services;
public sealed class GoogleAccountPicker:IGoogleAccountPicker{public Task<string?> PickAsync(string clientId,string nonce)=>throw new Core.Services.ApiException("El acceso con Google para iOS estará disponible en la siguiente versión.");}
