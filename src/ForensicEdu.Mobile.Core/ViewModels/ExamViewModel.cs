using ForensicEdu.Mobile.Core.Services;
using ForensicEdu.Mobile.Core.Models;
namespace ForensicEdu.Mobile.Core.ViewModels;
public sealed class ExamViewModel(AcademyApi api,TimeProvider clock):ScreenState
{
 private readonly SemaphoreSlim gate=new(1,1);private TimeSpan offset;
 public ExamSession? Session{get;private set;}public ExamResult? Result{get;private set;}
 public Dictionary<string,int> Answers{get;private set;}=[];public Dictionary<string,int[]> MultipleAnswers{get;private set;}=[];
 private Dictionary<string,object> Payload()=>Answers.ToDictionary(p=>p.Key,p=>(object)p.Value).Concat(MultipleAnswers.Select(p=>new KeyValuePair<string,object>(p.Key,p.Value))).ToDictionary(p=>p.Key,p=>p.Value);
 public int RemainingSeconds=>Session==null?600:Math.Max(0,(int)Math.Ceiling((Session.ExpiresAt-clock.GetUtcNow()-offset).TotalSeconds));
 public async Task StartAsync(string enrollmentId){Session=await api.WriteAsync<ExamSession>(HttpMethod.Post,$"exam-sessions/{AcademyApi.Id(enrollmentId)}/start",new{});offset=Session.ServerTime-clock.GetUtcNow();Answers=new(Session.SavedAnswers);MultipleAnswers=new(Session.SavedMultipleAnswers);}
 public async Task ChooseAsync(string id,int choice){if(Session==null||Result!=null||RemainingSeconds==0)return;if(Session.Questions.FirstOrDefault(q=>q.Id==id)?.AllowMultiple==true){var selected=MultipleAnswers.GetValueOrDefault(id,[]);MultipleAnswers[id]=selected.Contains(choice)?selected.Where(v=>v!=choice).ToArray():[..selected,choice];}else Answers[id]=choice;await gate.WaitAsync();try{await api.WriteAsync(HttpMethod.Put,$"exam-sessions/{AcademyApi.Id(Session.Id)}/answers",Payload());}finally{gate.Release();}}
 public async Task SubmitAsync(string enrollmentId,string userId){if(Session==null||Result!=null)return;await gate.WaitAsync();try{if(Result!=null)return;if(RemainingSeconds>0)await api.WriteAsync(HttpMethod.Put,$"exam-sessions/{AcademyApi.Id(Session.Id)}/answers",Payload());Result=await api.WriteAsync<ExamResult>(HttpMethod.Post,"exams/submit",new{enrollmentId,userId,selectedAnswers=Answers,selectedMultipleAnswers=MultipleAnswers,attemptId=Session.Id,durationSeconds=Math.Clamp(600-RemainingSeconds,0,600)});}finally{gate.Release();}}
}
