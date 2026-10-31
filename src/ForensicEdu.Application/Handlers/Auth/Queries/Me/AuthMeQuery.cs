using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Auth.Queries.Me;

public sealed record AuthMeQuery() : IRequest<ApplicationResult>;
