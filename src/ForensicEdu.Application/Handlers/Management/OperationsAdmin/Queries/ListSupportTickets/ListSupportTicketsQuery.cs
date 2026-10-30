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
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListSupportTickets;

public sealed record ListSupportTicketsQuery(TicketStatus? Status = null, int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
public sealed class ListSupportTicketsQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListSupportTicketsQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListSupportTicketsQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "5")) return ApplicationResult.Forbidden();
        var query=db.SupportTickets.AsNoTracking().AsQueryable();if(request.Status.HasValue)query=query.Where(x=>x.Status==request.Status.Value);
        if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.Subject.ToLower().Contains(request.Search.Trim().ToLower()) || x.TicketCode.Contains(request.Search));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderByDescending(x=>x.CreatedAt).ThenBy(x=>x.Id).Select(x=>new {x.Id,x.TicketCode,x.UserId,x.UserName,x.UserEmail,x.Subject,x.Category,x.Message,x.Status,x.CreatedAt,x.ResponsesJson}),request.Page,request.PageSize,ct));
    }
}

public sealed class ListSupportTicketsQueryValidator : AbstractValidator<ListSupportTicketsQuery>
{
    public ListSupportTicketsQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
        RuleFor(x=>x.Status).IsInEnum().When(x=>x.Status.HasValue);
    }
}
