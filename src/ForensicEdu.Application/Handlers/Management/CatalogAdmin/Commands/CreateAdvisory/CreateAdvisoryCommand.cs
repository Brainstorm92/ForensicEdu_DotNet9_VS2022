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
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateAdvisory;

public sealed record CreateAdvisoryCommand(AdvisoryInput Input) : IRequest<ApplicationResult>;
public sealed class CreateAdvisoryCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<CreateAdvisoryCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CreateAdvisoryCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var input=request.Input;
        var item=new AdvisoryService();db.AdvisoryServices.Add(item);
        item.Title=input.Title.Trim();item.Specialist=input.Specialist;item.Specialty=input.Specialty;item.Description=input.Description;item.Price=input.Price;item.WhatsappContact=input.WhatsappContact;
        EndpointSupport.Audit(db,current,"ADVISORY_SAVED","AdvisoryService",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.Success(new {item.Id,item.Title});
    }
}

public sealed class CreateAdvisoryCommandValidator : AbstractValidator<CreateAdvisoryCommand>
{
    public CreateAdvisoryCommandValidator()
    {
        RuleFor(x=>x.Input).NotNull().SetValidator(new AdvisoryInputValidator());
    }
}
