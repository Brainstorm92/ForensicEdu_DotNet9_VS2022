using ForensicEdu.Mobile.Core.Services;
using ForensicEdu.Mobile.Core.Models;
namespace ForensicEdu.Mobile.Core.ViewModels;
public sealed class ExamViewModel(AcademyApi api,TimeProvider clock):ScreenState
{
 private readonly SemaphoreSlim gate=new(1,1);private TimeSpan offset;
 public ExamSession? Session{get;private set;}public ExamResult? Result{get;private set;}
 public Dictionary<string,int> Answers{get;private set;}=[];
 public int RemainingSeconds=>Session==null?600:Math.Max(0,(int)Math.Ceiling((Session.ExpiresAt-clock.GetUtcNow()-offset).TotalSeconds));
 public async Task StartAsync(string enrollmentId){Session=await api.WriteAsync<ExamSession>(HttpMethod.Post,$"exam-sessions/{AcademyApi.Id(enrollmentId)}/start",new{});offset=Session.ServerTime-clock.GetUtcNow();Answers=new(Session.SavedAnswers);}
 public async Task ChooseAsync(string id,int choice){if(Session==null||Result!=null||RemainingSeconds==0)return;Answers[id]=choice;await gate.WaitAsync();try{await api.WriteAsync(HttpMethod.Put,$"exam-sessions/{AcademyApi.Id(Session.Id)}/answers",new Dictionary<string,int>(Answers));}finally{gate.Release();}}
 public async Task SubmitAsync(string enrollmentId,string userId){if(Session==null||Result!=null)return;await gate.WaitAsync();try{if(Result!=null)return;if(RemainingSeconds>0)await api.WriteAsync(HttpMethod.Put,$"exam-sessions/{AcademyApi.Id(Session.Id)}/answers",Answers);Result=await api.WriteAsync<ExamResult>(HttpMethod.Post,"exams/submit",new{enrollmentId,userId,selectedAnswers=Answers,durationSeconds=Math.Clamp(600-RemainingSeconds,0,600)});}finally{gate.Release();}}
}
