namespace ForensicEdu.Mobile.Core.Services;
public interface ITokenStore{Task<string?> ReadAsync();Task WriteAsync(string token);Task ClearAsync();}
