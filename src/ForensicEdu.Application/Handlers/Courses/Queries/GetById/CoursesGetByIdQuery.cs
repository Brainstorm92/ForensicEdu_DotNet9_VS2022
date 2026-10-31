using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Courses.Queries.GetById;

public sealed record CoursesGetByIdQuery(string Id) : IRequest<ApplicationResult>;
