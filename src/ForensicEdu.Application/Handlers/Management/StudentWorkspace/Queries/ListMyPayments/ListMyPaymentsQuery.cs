using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.ListMyPayments;

public sealed record ListMyPaymentsQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
