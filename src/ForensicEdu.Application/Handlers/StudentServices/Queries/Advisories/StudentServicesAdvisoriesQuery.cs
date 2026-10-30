using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.StudentServices;

namespace ForensicEdu.Application.Handlers.StudentServices.Queries.Advisories;
public sealed record StudentServicesAdvisoriesQuery() : IRequest<ApplicationResult>;
public sealed class StudentServicesAdvisoriesQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<StudentServicesAdvisoriesQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(StudentServicesAdvisoriesQuery message, CancellationToken cancellationToken)
    {


        var products = db.Orders.Where(o => o.UserId == current.UserId && o.Status == OrderStatus.APROBADO && o.ProductType == ProductType.ASESORIA).Select(o => o.ProductId);
        return ApplicationResult.Success(await db.AdvisoryServices.Where(a => products.Contains(a.Id))
            .Select(a => new { a.Id, a.Title, a.Specialist, a.Specialty, a.WhatsappContact }).ToListAsync(cancellationToken));
    }
}
