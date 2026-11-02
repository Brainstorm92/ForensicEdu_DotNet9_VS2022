using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
namespace ForensicEdu.Application.Services;

public static class ExamAnswers
{
 public static string Revision(string json)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
 public static bool Multiple(JsonElement q)=>q.TryGetProperty("allowMultiple",out var m)&&m.ValueKind==JsonValueKind.True;
 public static int[] Correct(JsonElement q)=>Multiple(q)&&q.TryGetProperty("correctOptionIndexes",out var a)&&a.ValueKind==JsonValueKind.Array?a.EnumerateArray().Select(v=>v.GetInt32()).ToArray():[q.GetProperty("correctOptionIndex").GetInt32()];
 public static Dictionary<string,int[]> Read(string json)
 {
  using var doc=JsonDocument.Parse(json);return Read(doc.RootElement.EnumerateObject().ToDictionary(p=>p.Name,p=>p.Value.Clone()));
 }
 public static Dictionary<string,int[]> Read(Dictionary<string,JsonElement> source)=>source.ToDictionary(p=>p.Key,p=>p.Value.ValueKind==JsonValueKind.Number?[p.Value.GetInt32()]:p.Value.ValueKind==JsonValueKind.Array?p.Value.EnumerateArray().Select(x=>x.GetInt32()).ToArray():throw new InvalidOperationException("Las respuestas deben ser opciones válidas."));
 public static void Validate(string snapshot,Dictionary<string,int[]> answers)
 {
  using var doc=JsonDocument.Parse(snapshot);var questions=doc.RootElement.EnumerateArray().ToDictionary(q=>q.GetProperty("id").GetString()!);
  foreach(var (id,values) in answers){if(!questions.TryGetValue(id,out var q)||values==null||values.Length>8||values.Distinct().Count()!=values.Length||(!Multiple(q)&&values.Length>1)||values.Any(v=>v<0||(q.TryGetProperty("options",out var options)&&v>=options.GetArrayLength())))throw new InvalidOperationException("Hay opciones fuera del banco de preguntas o selecciones inválidas.");}
 }
 public static (int correct,int total) Grade(string snapshot,Dictionary<string,int[]> answers)
 {
  using var doc=JsonDocument.Parse(snapshot);var questions=doc.RootElement.EnumerateArray().ToArray();return(questions.Count(q=>answers.TryGetValue(q.GetProperty("id").GetString()!,out var selected)&&selected.Length>0&&selected.Order().SequenceEqual(Correct(q).Order())),questions.Length);
 }
 public static object Session(ExamAttempt attempt)
 {
  using var doc=JsonDocument.Parse(attempt.QuestionsSnapshotJson);var answers=Read(attempt.AnswersJson);
  var questions=doc.RootElement.EnumerateArray().Select(q=>new {id=q.GetProperty("id").GetString(),question=q.GetProperty("question").GetString(),options=q.GetProperty("options").EnumerateArray().Select(o=>o.GetString()).ToArray(),allowMultiple=Multiple(q)}).ToArray();
  var multiple=questions.Where(q=>q.allowMultiple).Select(q=>q.id!).ToHashSet();
  return new{attempt.Id,attempt.AttemptNumber,attempt.StartedAt,expiresAt=attempt.StartedAt.AddMinutes(10),serverTime=DateTime.UtcNow,questions,answerRevision=Revision(attempt.AnswersJson),savedAnswers=answers.Where(p=>!multiple.Contains(p.Key)&&p.Value.Length==1).ToDictionary(p=>p.Key,p=>p.Value[0]),savedMultipleAnswers=answers.Where(p=>multiple.Contains(p.Key)).ToDictionary(p=>p.Key,p=>p.Value)};
 }
 public static ExamResultDto Result(ExamAttempt attempt,string? certificate)
 {
  var grade=Grade(attempt.QuestionsSnapshotJson,Read(attempt.AnswersJson));return new(attempt.Id,attempt.AttemptNumber,attempt.Score,attempt.Approved,attempt.Status.ToString(),grade.correct,grade.total,attempt.NextAttemptAllowedAt,certificate,attempt.Approved?"Evaluación aprobada. Tu certificado está disponible.":attempt.AttemptNumber==1?"Evaluación finalizada. Consulta la fecha habilitada para tu segundo intento.":"Agotaste los dos intentos de esta matrícula.");
 }
}

public sealed class ExamAnswerConflictException():InvalidOperationException("Las respuestas cambiaron en otra pestaña. Recupera la versión del servidor antes de enviar.");
