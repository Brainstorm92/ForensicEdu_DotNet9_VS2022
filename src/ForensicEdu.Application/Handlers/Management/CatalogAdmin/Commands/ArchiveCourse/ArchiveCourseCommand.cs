using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.ArchiveCourse;

public sealed record ArchiveCourseCommand(string Id) : IRequest<ApplicationResult>;
