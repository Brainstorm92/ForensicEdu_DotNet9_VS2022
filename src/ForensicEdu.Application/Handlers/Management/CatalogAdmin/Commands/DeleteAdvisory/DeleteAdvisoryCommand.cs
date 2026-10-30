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
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.DeleteAdvisory;

public sealed record DeleteAdvisoryCommand(string Id) : IRequest<ApplicationResult>;
public sealed class DeleteAdvisoryCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<DeleteAdvisoryCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(DeleteAdvisoryCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var item=await db.AdvisoryServices.FirstOrDefaultAsync(x=>x.Id==request.Id,ct);if(item==null)return ApplicationResult.NotFound();
        if(await db.Orders.AnyAsync(x=>x.ProductId==item.Id && x.ProductType==ProductType.ASESORIA,ct))return ApplicationResult.Conflict(new {message="La asesoría tiene órdenes y debe conservarse."});db.AdvisoryServices.Remove(item);EndpointSupport.Audit(db,current,"ADVISORY_REMOVED","AdvisoryService",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.NoContent();
    }
}

public sealed class DeleteAdvisoryCommandValidator : AbstractValidator<DeleteAdvisoryCommand>
{
    public DeleteAdvisoryCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
