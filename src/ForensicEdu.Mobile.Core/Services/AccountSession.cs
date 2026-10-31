using ForensicEdu.Mobile.Core.Models;
namespace ForensicEdu.Mobile.Core.Services;
public sealed class AccountSession(AcademyApi api,ITokenStore tokens)
{
 public Account? User{get;private set;}
 public async Task LoginAsync(string username,string password){var result=await api.WriteAsync<LoginResult>(HttpMethod.Post,"auth/login",new{email=username.Trim(),password},false);await tokens.WriteAsync(result.Token);try{User=await api.GetAsync<Account>("auth/me");}catch{await LogoutAsync();throw;}}
 public async Task<bool> RestoreAsync(){if(string.IsNullOrWhiteSpace(await tokens.ReadAsync()))return false;try{User=await api.GetAsync<Account>("auth/me");return true;}catch(ApiException e)when(e.Status==401){await LogoutAsync();return false;}}
 public async Task<GoogleAccessResult> GoogleAsync(string credential,GoogleProfile? profile=null,string? linkPassword=null)
 {
  var result=await api.WriteAsync<GoogleAccessResult>(HttpMethod.Post,"auth/google",new{credential,profile,linkPassword},false);
  if(!string.IsNullOrWhiteSpace(result.Token)){await tokens.WriteAsync(result.Token);try{User=await api.GetAsync<Account>("auth/me");}catch{await LogoutAsync();throw;}}
  return result;
 }
 public async Task RefreshAsync()=>User=await api.GetAsync<Account>("auth/me");
 public async Task LogoutAsync(){User=null;await tokens.ClearAsync();}
}
