using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Services;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.ExamSessions.Commands.Save;
public sealed class ExamSessionsSaveCommandHandler(IApplicationDbContext db,ICurrentUser current,ICoursePurchaseGate gate):IRequestHandler<ExamSessionsSaveCommand,ApplicationResult>
{
 public Task<ApplicationResult> Handle(ExamSessionsSaveCommand message,CancellationToken ct)=>gate.ExecuteAsync(current.UserId!,"exam",async()=>{
  var attempt=await db.ExamAttempts.FirstOrDefaultAsync(a=>a.Id==message.AttemptId&&a.UserId==current.UserId,ct);if(attempt==null)return ApplicationResult.NotFound();
  if(attempt.CompletedAt!=null||DateTime.UtcNow>=attempt.StartedAt.AddMinutes(10))return ApplicationResult.Conflict(new{message="La evaluación está cerrada.",code="EXAM_CLOSED"});
  if(message.Revision!=null&&message.Revision!=ExamAnswers.Revision(attempt.AnswersJson))return ApplicationResult.Conflict(new{message="Las respuestas cambiaron en otra pestaña. Recupera la versión del servidor.",code="ANSWER_CONFLICT"});
  try{var answers=ExamAnswers.Read(message.Answers);ExamAnswers.Validate(attempt.QuestionsSnapshotJson,answers);attempt.AnswersJson=JsonSerializer.Serialize(answers);}
  catch(Exception e)when(e is JsonException or InvalidOperationException or FormatException or OverflowException){return ApplicationResult.Invalid(new{message="Revisa las opciones seleccionadas."});}
  await db.SaveChangesAsync(ct);return ApplicationResult.Success(new{answerRevision=ExamAnswers.Revision(attempt.AnswersJson)});
 },ct);
}
