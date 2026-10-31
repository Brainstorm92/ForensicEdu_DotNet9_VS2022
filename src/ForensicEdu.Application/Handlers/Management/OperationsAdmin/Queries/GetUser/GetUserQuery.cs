using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetUser;

public sealed record GetUserQuery(string Id) : IRequest<ApplicationResult>;
