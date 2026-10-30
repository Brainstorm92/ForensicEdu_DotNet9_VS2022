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
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateModule;

public sealed record CreateModuleCommand(string Id, ModuleInput Input) : IRequest<ApplicationResult>;
public sealed class CreateModuleCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<CreateModuleCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CreateModuleCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var course=await db.Courses.Include(x=>x.Modules).FirstOrDefaultAsync(x=>x.Id==request.Id,ct);
        if(course==null)return ApplicationResult.NotFound();
        var input=request.Input;
        var item=new CourseModule {CourseId=course.Id};course.Modules.Add(item);db.CourseModules.Add(item);
        if(course.Modules.Any(x=>x.Id!=item.Id && x.OrderIndex==input.OrderIndex))return ApplicationResult.Conflict(new {message="Ese orden ya pertenece a otro módulo."});
        item.Title=input.Title.Trim();item.OrderIndex=input.OrderIndex;item.Duration=input.Duration;item.VideoStreamingUrl=input.VideoStreamingUrl;
        EndpointSupport.Audit(db,current,"MODULE_SAVED","CourseModule",item.Id);await db.SaveChangesAsync(ct);
        return ApplicationResult.Success(new {item.Id,item.Title,item.OrderIndex});
    }
}

public sealed class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new ModuleInputValidator());
    }
}
