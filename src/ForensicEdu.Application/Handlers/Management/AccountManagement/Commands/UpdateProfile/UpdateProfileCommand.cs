using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.AccountManagement.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(ProfileInput Input) : IRequest<ApplicationResult>;
