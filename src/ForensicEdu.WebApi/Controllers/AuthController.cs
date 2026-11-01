using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Enums;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Contracts.Auth;
using ForensicEdu.Application.Handlers.Auth.Queries.Me;
using ForensicEdu.Application.Handlers.Auth.Commands.Login;
using ForensicEdu.Application.Handlers.Auth.Commands.Register;
namespace ForensicEdu.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        return (await sender.Send(new AuthMeQuery(), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        return (await sender.Send(new AuthLoginCommand(request), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpPost("register")]
    [EnableRateLimiting("email-verification")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        return (await sender.Send(new AuthRegisterCommand(request), HttpContext.RequestAborted)).ToActionResult();
    }
}
