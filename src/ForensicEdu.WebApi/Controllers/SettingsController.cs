using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Enums;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Contracts.Settings;
using ForensicEdu.Application.Handlers.Settings.Queries.Social;
using ForensicEdu.Application.Handlers.Settings.Commands.Set;
namespace ForensicEdu.WebApi.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController(ISender sender) : ControllerBase
{
    [HttpGet("social")]
    public async Task<IActionResult> Social()
    {
        return (await sender.Send(new SettingsSocialQuery(), HttpContext.RequestAborted)).ToActionResult();
    }

    [Authorize(Roles = "1,2")]
    [HttpPut("{key}")]
    public async Task<IActionResult> Set(string key, SettingRequest request)
    {
        return (await sender.Send(new SettingsSetCommand(key, request), HttpContext.RequestAborted)).ToActionResult();
    }
}
