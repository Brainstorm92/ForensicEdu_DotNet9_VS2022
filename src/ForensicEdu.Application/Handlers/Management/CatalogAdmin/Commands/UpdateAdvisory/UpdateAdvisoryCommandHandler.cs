using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateAdvisory;

public sealed class UpdateAdvisoryCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<UpdateAdvisoryCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(UpdateAdvisoryCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var input=request.Input;
        var item=await db.AdvisoryServices.FirstOrDefaultAsync(x=>x.Id==request.Id,ct);if(item==null)return ApplicationResult.NotFound();
        item.Title=input.Title.Trim();item.Specialist=input.Specialist;item.Specialty=input.Specialty;item.Description=input.Description;item.Price=input.Price;item.WhatsappContact=input.WhatsappContact;
        EndpointSupport.Audit(db,current,"ADVISORY_SAVED","AdvisoryService",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.Success(new {item.Id,item.Title});
    }
}
