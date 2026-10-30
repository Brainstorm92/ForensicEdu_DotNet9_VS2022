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
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetMyExamHistory;

public sealed record GetMyExamHistoryQuery(string Id) : IRequest<ApplicationResult>;
public sealed class GetMyExamHistoryQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetMyExamHistoryQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetMyExamHistoryQuery request, CancellationToken ct)
    {
        if(!await db.Enrollments.AnyAsync(x=>x.Id==request.Id && x.UserId==current.UserId,ct))return ApplicationResult.NotFound();
        return ApplicationResult.Success(await db.ExamAttempts.AsNoTracking().Where(x=>x.EnrollmentId==request.Id && x.UserId==current.UserId).OrderBy(x=>x.AttemptNumber)
            .Select(x=>new {x.Id,x.AttemptNumber,x.Score,x.Approved,x.Status,x.StartedAt,x.CompletedAt,x.NextAttemptAllowedAt}).ToListAsync(ct));
    }
}

public sealed class GetMyExamHistoryQueryValidator : AbstractValidator<GetMyExamHistoryQuery>
{
    public GetMyExamHistoryQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
