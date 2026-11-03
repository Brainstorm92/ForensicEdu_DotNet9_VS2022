using System.Net;using System.Net.Http.Json;using System.Net.Http.Headers;using System.Text.Json;
using ForensicEdu.Domain.Entities;using ForensicEdu.Domain.Enums;using ForensicEdu.Infrastructure.Persistence;using ForensicEdu.Infrastructure.Services;using ForensicEdu.Application.Common.Interfaces;using Microsoft.EntityFrameworkCore;using Microsoft.Extensions.DependencyInjection;
static class ExamScenario
{
 public static readonly object[] Questions=[new{id="single",question="¿Cuál es la opción correcta?",options=new[]{"Opción A","Opción B","Opción C"},correctOptionIndex=1,explanation="Privado del docente"},new{id="multi",question="Selecciona las dos evidencias que corresponden al caso.",options=new[]{"Evidencia A","Evidencia B","Evidencia C","Evidencia D"},correctOptionIndex=0,allowMultiple=true,correctOptionIndexes=new[]{0,2},explanation="Privado del docente"}];
 public static async Task Seed(IServiceProvider services){using var scope=services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
  if(!await db.Users.AnyAsync(u=>u.Id=="admin-fixture"))db.Users.Add(new User{Id="admin-fixture",Role=UserRole.SUPERADMIN,Email="admin@example.invalid"});
  var student=(await db.Users.FindAsync("student"))!;student.Status=AccountStatus.ACTIVO;student.FullName="Alumno de prueba";student.Nombres="Alumno";student.Apellidos="de prueba";student.ValidUntil=DateTime.UtcNow.AddDays(30);
  foreach(var suffix in new[]{"ready","other","incomplete","unpaid","empty","expired","partial","extra"}){
   var course=new Course{Id="exam-course-"+suffix,Title="Evaluación forense · "+suffix,ShortCode="EXAM",AcademicHours=60,ImageUrl="/assets/course-placeholder.svg",QuestionsJson=suffix=="empty"?"[]":JsonSerializer.Serialize(Questions)};db.Courses.Add(course);
   db.CourseModules.Add(new CourseModule{Id="exam-module-"+suffix,CourseId=course.Id,Title="Fundamentos del curso"});
   db.Orders.Add(new Order{Id="exam-order-"+suffix,OrderCode="EXAM-"+suffix,UserId="student",ProductId=course.Id,ProductType=ProductType.CURSO,Status=suffix=="unpaid"?OrderStatus.PENDIENTE_VALIDACION:OrderStatus.APROBADO});
   db.Enrollments.Add(new Enrollment{Id="exam-"+suffix,CourseId=course.Id,UserId="student",OrderId="exam-order-"+suffix,ProgressPercent=suffix=="incomplete"?0:100,CompletedModulesJson=suffix=="incomplete"?"[]":JsonSerializer.Serialize(new[]{"exam-module-"+suffix})});
  }await db.SaveChangesAsync();
 }
 public static async Task Run(HttpClient http,IServiceProvider services,TokenService tokens){
  await Seed(services);int count=0;void Check(bool value,string label){if(!value)throw new Exception(label);Console.WriteLine("OK: "+label);count++;}
  void As(string id,int role=6)=>http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",tokens.Create(new User{Id=id,Role=(UserRole)role}));
  async Task<JsonElement> Json(HttpResponseMessage response){var body=await response.Content.ReadAsStringAsync();if(!response.IsSuccessStatusCode)throw new Exception("HTTP "+(int)response.StatusCode+": "+body);return JsonDocument.Parse(body).RootElement.Clone();}
  Task<HttpResponseMessage> Start(string suffix)=>http.PostAsJsonAsync("/api/exam-sessions/exam-"+suffix+"/start",new{});
  async Task<HttpResponseMessage> Save(string id,object body,string? revision=null){using var request=new HttpRequestMessage(HttpMethod.Put,"/api/exam-sessions/"+id+"/answers"){Content=JsonContent.Create(body)};if(revision!=null)request.Headers.Add("If-Match","\""+revision+"\"");return await http.SendAsync(request);}
  object Submission(string suffix,string id,Dictionary<string,int[]>? multiple=null,Dictionary<string,int>? singles=null,string? revision=null)=>new{enrollmentId="exam-"+suffix,userId="student",attemptId=id,selectedAnswers=singles??new(){["single"]=1},selectedMultipleAnswers=multiple??new(){["multi"]=[0,2]},durationSeconds=1,answerRevision=revision};
  Check((await Start("ready")).StatusCode==HttpStatusCode.Unauthorized,"Examen requiere sesión");As("student");
  Check((await Start("incomplete")).StatusCode==HttpStatusCode.BadRequest,"No inicia con módulos pendientes");Check((await Start("unpaid")).StatusCode==HttpStatusCode.BadRequest,"No inicia con pago sin aprobar");Check((await Start("empty")).StatusCode==HttpStatusCode.BadRequest,"No consume intento si falta banco de preguntas");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Check(await db.ExamAttempts.CountAsync()==0,"Confirmaciones y validaciones fallidas no crean intentos");}
  As("admin-fixture",1);var invalid=new{questions=new[]{new{id="bad",question="Pregunta",options=new[]{"A","B"},correctOptionIndex=0,allowMultiple=true,correctOptionIndexes=Array.Empty<int>(),explanation=""}}};Check((await http.PutAsJsonAsync("/api/admin/courses/exam-course-ready/questions",invalid)).StatusCode==HttpStatusCode.BadRequest,"Administrador debe marcar una respuesta correcta como mínimo");
  Check((await http.PutAsJsonAsync("/api/admin/courses/exam-course-ready/questions",new{questions=Questions})).IsSuccessStatusCode,"Administrador guarda un banco mixto de respuestas únicas y múltiples");
  As("student");Check((await http.GetAsync("/api/admin/courses/exam-course-ready/questions")).StatusCode==HttpStatusCode.Forbidden,"Alumno no consulta las soluciones del administrador");
  var starts=await Task.WhenAll(Start("ready"),Start("ready"));var first=await Json(starts[0]);var second=await Json(starts[1]);var id=first.GetProperty("id").GetString()!;var revision=first.GetProperty("answerRevision").GetString()!;
  Check(id==second.GetProperty("id").GetString(),"Doble inicio concurrente reutiliza el mismo intento");var firstText=first.GetRawText();Check(!firstText.Contains("correctOption")&&!firstText.Contains("explanation")&&firstText.Contains("allowMultiple"),"Sesión no revela respuestas correctas ni explicaciones");
  Check((await Start("other")).StatusCode==HttpStatusCode.Conflict,"Una evaluación activa impide abrir otra del mismo alumno");
  var active=await Json(await http.GetAsync("/api/exam-sessions/active"));Check(active.GetProperty("session").GetProperty("id").GetString()==id,"Servidor permite descubrir el examen tras recargar");
  As("intruder");Check((await http.GetAsync("/api/exam-sessions/"+id)).StatusCode==HttpStatusCode.NotFound,"No recupera evaluaciones ajenas");Check((await Save(id,new{single=1})).StatusCode==HttpStatusCode.NotFound,"No modifica respuestas ajenas");As("student");
  Check((await Save(id,new{single=new[]{0,1}})).StatusCode==HttpStatusCode.BadRequest,"Pregunta única rechaza varias selecciones");Check((await Save(id,new{multi=new[]{0,0}})).StatusCode==HttpStatusCode.BadRequest,"No acepta opciones repetidas");Check((await Save(id,new{multi=new[]{0,99}})).StatusCode==HttpStatusCode.BadRequest,"No acepta opciones fuera del rango");Check((await Save(id,new{unknown=1})).StatusCode==HttpStatusCode.BadRequest,"No acepta preguntas inventadas");
  var saved=await Json(await Save(id,new{single=1,multi=new[]{0,2}},revision));var nextRevision=saved.GetProperty("answerRevision").GetString()!;
  Check((await Save(id,new{single=0},revision)).StatusCode==HttpStatusCode.Conflict,"Otra pestaña no sobrescribe una revisión más reciente");
  var resumed=await Json(await Start("ready"));Check(resumed.GetProperty("expiresAt").GetString()==first.GetProperty("expiresAt").GetString()&&resumed.GetProperty("savedMultipleAnswers").GetProperty("multi").GetArrayLength()==2&&resumed.GetProperty("savedAnswers").GetProperty("single").GetInt32()==1,"Recarga conserva tiempo original y ambos tipos de respuestas");
  Check((await http.PostAsJsonAsync("/api/exams/submit",Submission("ready",id,revision:revision))).StatusCode==HttpStatusCode.Conflict,"Finalización antigua no reemplaza respuestas recientes");
  As("admin-fixture",1);Check((await http.PutAsJsonAsync("/api/admin/courses/exam-course-ready/questions",new{questions=new[]{new{id="new",question="Nueva pregunta",options=new[]{"X","Y"},correctOptionIndex=0,explanation=""}}})).IsSuccessStatusCode,"Se puede editar el banco sin alterar el intento iniciado");As("student");
  var submissions=await Task.WhenAll(http.PostAsJsonAsync("/api/exams/submit",Submission("ready",id,revision:nextRevision)),http.PostAsJsonAsync("/api/exams/submit",Submission("ready",id,revision:nextRevision)));var result=await Json(submissions[0]);var repeated=await Json(submissions[1]);Check(result.GetProperty("score").GetDecimal()==20&&repeated.GetProperty("attemptId").GetString()==id,"Envíos simultáneos califican el snapshot una sola vez");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Check(await db.DigitalCertificates.CountAsync(c=>c.EnrollmentId=="exam-ready")==1&&await db.ExamAttempts.CountAsync(a=>a.EnrollmentId=="exam-ready")==1,"No duplica certificado ni intento");}
  var finished=await Json(await http.GetAsync("/api/exam-sessions/"+id));Check(finished.GetProperty("result").GetProperty("score").GetDecimal()==20,"Respuesta perdida al enviar se recupera con el resultado persistido");Check((await Save(id,new{single=0})).StatusCode==HttpStatusCode.Conflict,"No cambia respuestas de un intento cerrado");Check((await Start("ready")).StatusCode==HttpStatusCode.Conflict,"Aprobación bloquea nuevos intentos");
  foreach(var suffix in new[]{"expired","partial","extra"}){
   var session=await Json(await Start(suffix));var attemptId=session.GetProperty("id").GetString()!;
   if(suffix=="expired"){
    await Save(attemptId,new{single=1,multi=new[]{0,2}});using var scope=services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();(await db.ExamAttempts.FindAsync(attemptId))!.StartedAt=DateTime.UtcNow.AddMinutes(-11);await db.SaveChangesAsync();Check((await Save(attemptId,new{single=0})).StatusCode==HttpStatusCode.Conflict,"Vencimiento bloquea guardados tardíos");
   }
   var selection=suffix=="partial"?new[]{0}:suffix=="extra"?new[]{0,1,2}:new[]{1};
   var value=await Json(await http.PostAsJsonAsync("/api/exams/submit",Submission(suffix,attemptId,new(){["multi"]=selection},new(){["single"]=suffix=="expired"?0:1})));
   Check(value.GetProperty("score").GetDecimal()==(suffix=="expired"?20:10),suffix=="expired"?"Al vencer califica únicamente lo guardado a tiempo":suffix=="partial"?"Selección parcial no suma puntos en pregunta múltiple":"Marcar una opción incorrecta invalida la pregunta múltiple");
   if(suffix=="partial")Check((await Start(suffix)).StatusCode==HttpStatusCode.Conflict,"Respeta el periodo de repaso tras desaprobar");
  }
  Console.WriteLine($"{count} comprobaciones de evaluación superadas; base aislada en memoria.");
 }
}
