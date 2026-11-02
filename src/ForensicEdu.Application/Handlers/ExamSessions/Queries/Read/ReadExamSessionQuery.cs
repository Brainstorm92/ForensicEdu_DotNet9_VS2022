using MediatR;using FluentValidation;using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Common.Interfaces;using ForensicEdu.Application.Common.Results;using ForensicEdu.Application.Services;
namespace ForensicEdu.Application.Handlers.ExamSessions.Queries.Read;
public sealed record ReadExamSessionQuery(string? AttemptId=null):IRequest<ApplicationResult>;
public sealed class ReadExamSessionQueryValidator:AbstractValidator<ReadExamSessionQuery>{public ReadExamSessionQueryValidator(){When(x=>x.AttemptId!=null,()=>RuleFor(x=>x.AttemptId).NotEmpty().MaximumLength(200));}}
public sealed class ReadExamSessionQueryHandler(IApplicationDbContext db,ICurrentUser current):IRequestHandler<ReadExamSessionQuery,ApplicationResult>
{
 public async Task<ApplicationResult> Handle(ReadExamSessionQuery query,CancellationToken ct){
  var attempts=db.ExamAttempts.AsNoTracking().Where(a=>a.UserId==current.UserId);var attempt=query.AttemptId==null?await attempts.Where(a=>a.CompletedAt==null).OrderBy(a=>a.StartedAt).FirstOrDefaultAsync(ct):await attempts.FirstOrDefaultAsync(a=>a.Id==query.AttemptId,ct);
  if(attempt==null)return query.AttemptId==null?ApplicationResult.Success(null):ApplicationResult.NotFound();
  var title=await db.Courses.Where(c=>c.Id==attempt.CourseId).Select(c=>c.Title).FirstOrDefaultAsync(ct);
  var certificate=attempt.CompletedAt!=null?await db.DigitalCertificates.Where(c=>c.EnrollmentId==attempt.EnrollmentId).Select(c=>c.CertificateCode).FirstOrDefaultAsync(ct):null;
  return ApplicationResult.Success(new{attempt.EnrollmentId,courseTitle=title,session=attempt.CompletedAt==null?ExamAnswers.Session(attempt):null,result=attempt.CompletedAt!=null?ExamAnswers.Result(attempt,certificate):null});
 }
}
