using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Handlers.Management.AccountManagement.Commands.UpdateProfile;
using ForensicEdu.Application.Handlers.Management.AccountManagement.Commands.ChangePassword;
namespace ForensicEdu.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public sealed class AccountManagementController(ISender sender) : ControllerBase
{
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] ProfileInput input)
        => (await sender.Send(new UpdateProfileCommand(input),HttpContext.RequestAborted)).ToActionResult();

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordInput input)
        => (await sender.Send(new ChangePasswordCommand(input),HttpContext.RequestAborted)).ToActionResult();
}
