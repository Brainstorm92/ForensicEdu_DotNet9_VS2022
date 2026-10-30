using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.SaveQuestionBank;

public sealed record SaveQuestionBankCommand(string Id, QuestionBankInput Input) : IRequest<ApplicationResult>;
