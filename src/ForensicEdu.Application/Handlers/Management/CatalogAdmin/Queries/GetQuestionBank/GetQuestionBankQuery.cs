using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetQuestionBank;

public sealed record GetQuestionBankQuery(string Id) : IRequest<ApplicationResult>;
