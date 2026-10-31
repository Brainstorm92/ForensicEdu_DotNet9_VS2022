using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Enums;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Handlers.Courses.Queries.GetAll;
using ForensicEdu.Application.Handlers.Courses.Queries.GetById;
namespace ForensicEdu.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return (await sender.Send(new CoursesGetAllQuery(), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        return (await sender.Send(new CoursesGetByIdQuery(id), HttpContext.RequestAborted)).ToActionResult();
    }
}
