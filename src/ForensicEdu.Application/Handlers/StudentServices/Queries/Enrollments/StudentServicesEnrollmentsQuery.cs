using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.StudentServices;

namespace ForensicEdu.Application.Handlers.StudentServices.Queries.Enrollments;
public sealed record StudentServicesEnrollmentsQuery() : IRequest<ApplicationResult>;
public sealed class StudentServicesEnrollmentsQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<StudentServicesEnrollmentsQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(StudentServicesEnrollmentsQuery message, CancellationToken cancellationToken)
    {


        return ApplicationResult.Success(await db.Enrollments.Where(e => e.UserId == current.UserId)
        .Select(e => new { e.Id, e.CourseId, e.Status, e.ProgressPercent, e.ExamStatus, e.FinalGrade, e.AttemptsUsed, e.SecondAttemptAvailableAt }).ToListAsync(cancellationToken));
    }
}
