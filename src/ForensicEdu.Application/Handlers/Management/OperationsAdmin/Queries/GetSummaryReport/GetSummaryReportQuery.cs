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
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetSummaryReport;

public sealed record GetSummaryReportQuery() : IRequest<ApplicationResult>;
public sealed class GetSummaryReportQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetSummaryReportQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetSummaryReportQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1")) return ApplicationResult.Forbidden();
        return ApplicationResult.Success(new {
            users=await db.Users.CountAsync(ct),courses=await db.Courses.CountAsync(ct),events=await db.LiveEvents.CountAsync(ct),
            pendingPayments=await db.PaymentEvidences.CountAsync(x=>x.Status==PaymentStatus.PENDIENTE || x.Status==PaymentStatus.EN_REVISION,ct),
            approvedAmount=await db.PaymentEvidences.Where(x=>x.Status==PaymentStatus.APROBADO).Select(x=>(decimal?)x.Amount).SumAsync(ct)??0,
            enrollments=await db.Enrollments.CountAsync(ct),validCertificates=await db.DigitalCertificates.CountAsync(x=>x.Status==CertificateStatus.VALIDO,ct),
            openTickets=await db.SupportTickets.CountAsync(x=>x.Status!=TicketStatus.CERRADO,ct) });
    }
}
