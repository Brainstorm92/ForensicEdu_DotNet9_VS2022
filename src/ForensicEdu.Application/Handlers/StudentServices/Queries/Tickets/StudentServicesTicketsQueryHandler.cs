using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.StudentServices.Queries.Tickets;

public sealed class StudentServicesTicketsQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<StudentServicesTicketsQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(StudentServicesTicketsQuery message, CancellationToken cancellationToken)
    {


        return ApplicationResult.Success(await db.SupportTickets.Where(t => t.UserId == current.UserId)
        .OrderByDescending(t => t.CreatedAt).ToListAsync(cancellationToken));
    }
}
