using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Media.Commands.UploadImage;
public sealed record UploadImageCommand(byte[] Bytes):IRequest<ApplicationResult>;
