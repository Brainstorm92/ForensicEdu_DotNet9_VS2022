using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Application.Common.Validation;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.SaveQuestionBank;

public sealed record SaveQuestionBankCommand(string Id, QuestionBankInput Input) : IRequest<ApplicationResult>;
public sealed class SaveQuestionBankCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<SaveQuestionBankCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(SaveQuestionBankCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var item=await db.Courses.FirstOrDefaultAsync(x=>x.Id==request.Id,ct);if(item==null)return ApplicationResult.NotFound();
        item.QuestionsJson=JsonSerializer.Serialize(request.Input.Questions,EndpointSupport.Json);
        EndpointSupport.Audit(db,current,"QUESTION_BANK_SAVED","Course",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.NoContent();
    }
}

public sealed class SaveQuestionBankCommandValidator : AbstractValidator<SaveQuestionBankCommand>
{
    public SaveQuestionBankCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new QuestionBankInputValidator());
    }
}
