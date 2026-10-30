using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Enums;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Contracts.Orders;
using ForensicEdu.Application.Handlers.Orders.Commands.CreateOrderAndSubmitPayment;
using ForensicEdu.Application.Handlers.Orders.Queries.GetUserOrders;
using ForensicEdu.Application.Handlers.Orders.Commands.UploadVoucher;
namespace ForensicEdu.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController(ISender sender) : ControllerBase
{
    [HttpPost("checkout")]
    public async Task<IActionResult> CreateOrderAndSubmitPayment([FromBody] CreateOrderRequest request)
    {
        return (await sender.Send(new OrdersCreateOrderAndSubmitPaymentCommand(request), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserOrders(string userId)
    {
        return (await sender.Send(new OrdersGetUserOrdersQuery(userId), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpPost("{orderId}/voucher")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadVoucher(string orderId, IFormFile file)
    {
        if (file.Length == 0 || file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Adjunte JPG, PNG o PDF de hasta 5 MB." });
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, HttpContext.RequestAborted);
        var bytes = stream.ToArray();
        return (await sender.Send(new OrdersUploadVoucherCommand(orderId, bytes), HttpContext.RequestAborted)).ToActionResult();
    }
}
