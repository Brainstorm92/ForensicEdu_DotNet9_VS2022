namespace ForensicEdu.Mobile.Core.Services;
public sealed class ApiException(string message,int status=0):Exception(message){public int Status{get;}=status;}
