using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListUsers;

public sealed record ListUsersQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
