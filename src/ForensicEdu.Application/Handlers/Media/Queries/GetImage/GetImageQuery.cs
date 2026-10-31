using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Media.Queries.GetImage;
public sealed record GetImageQuery(string Id):IRequest<ApplicationResult>;
