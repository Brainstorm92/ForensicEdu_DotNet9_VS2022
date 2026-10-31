using ForensicEdu.Mobile.Core.Services;
namespace ForensicEdu.Mobile.Services;
public sealed class DeviceTokenStore:ITokenStore
{
 private const string Key="forense.session.v1";
 public async Task<string?> ReadAsync(){try{return await SecureStorage.Default.GetAsync(Key);}catch{SecureStorage.Default.Remove(Key);return null;}}
 public Task WriteAsync(string token)=>SecureStorage.Default.SetAsync(Key,token);
 public Task ClearAsync(){SecureStorage.Default.Remove(Key);return Task.CompletedTask;}
}
