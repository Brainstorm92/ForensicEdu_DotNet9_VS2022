using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.SaveQuestionBank;

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
