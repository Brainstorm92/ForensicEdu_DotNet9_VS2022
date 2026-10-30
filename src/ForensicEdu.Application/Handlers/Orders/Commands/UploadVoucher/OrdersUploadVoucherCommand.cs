using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Orders.Commands.UploadVoucher;

public sealed record OrdersUploadVoucherCommand(string OrderId, byte[] Bytes) : IRequest<ApplicationResult>;
