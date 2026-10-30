using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Courses.Queries.GetAll;

public sealed record CoursesGetAllQuery() : IRequest<ApplicationResult>;
