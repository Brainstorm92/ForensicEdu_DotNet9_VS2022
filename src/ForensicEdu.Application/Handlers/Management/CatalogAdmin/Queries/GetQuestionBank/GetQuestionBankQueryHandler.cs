using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetQuestionBank;

public sealed class GetQuestionBankQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetQuestionBankQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetQuestionBankQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var item=await db.Courses.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==request.Id,ct);if(item==null)return ApplicationResult.NotFound();
        return ApplicationResult.Success(JsonSerializer.Deserialize<JsonElement>(item.QuestionsJson));
    }
}
