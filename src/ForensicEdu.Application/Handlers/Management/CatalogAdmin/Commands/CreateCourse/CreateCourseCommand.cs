using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateCourse;

public sealed record CreateCourseCommand(CourseInput Input) : IRequest<ApplicationResult>;
