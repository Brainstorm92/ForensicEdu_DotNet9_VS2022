using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateCourse;

public sealed record UpdateCourseCommand(string Id, CourseInput Input) : IRequest<ApplicationResult>;
