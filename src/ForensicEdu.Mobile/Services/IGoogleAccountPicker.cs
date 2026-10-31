namespace ForensicEdu.Mobile.Services;
public interface IGoogleAccountPicker{Task<string?> PickAsync(string clientId,string nonce);}
