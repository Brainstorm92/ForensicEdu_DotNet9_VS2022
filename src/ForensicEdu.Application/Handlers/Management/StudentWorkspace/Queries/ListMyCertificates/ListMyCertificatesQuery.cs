using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.ListMyCertificates;

public sealed record ListMyCertificatesQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
