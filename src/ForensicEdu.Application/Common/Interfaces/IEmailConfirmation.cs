using ForensicEdu.Domain.Entities;
namespace ForensicEdu.Application.Common.Interfaces;
public enum ConfirmationDelivery { Sent,Disabled,Unavailable,Failed,Throttled }
public interface IEmailConfirmation
{
 Task<ConfirmationDelivery> SendInitialAsync(User user,CancellationToken ct);
 Task RequestAsync(string email,CancellationToken ct);
 Task<bool> ConfirmAsync(string token,CancellationToken ct);
}
public interface IEmailConfirmationTokens
{
 string Create(string userId,string email,string stamp);
 (string UserId,string Email,string Stamp)? Read(string token);
}
