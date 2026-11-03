using ForensicEdu.Mobile.Core.Services;
using ForensicEdu.Mobile.Core.Models;
namespace ForensicEdu.Mobile.Core.ViewModels;
public sealed class ExamViewModel(AcademyApi api,TimeProvider clock):ScreenState
{
 private readonly SemaphoreSlim gate=new(1,1);private TimeSpan offset;private string revision="";public bool HasConflict{get;private set;}
 public ExamSession? Session{get;private set;}public ExamResult? Result{get;private set;}
 public Dictionary<string,int> Answers{get;private set;}=[];public Dictionary<string,int[]> MultipleAnswers{get;private set;}=[];
 private Dictionary<string,object> Payload()=>Answers.ToDictionary(p=>p.Key,p=>(object)p.Value).Concat(MultipleAnswers.Select(p=>new KeyValuePair<string,object>(p.Key,p.Value))).ToDictionary(p=>p.Key,p=>p.Value);
 public int RemainingSeconds=>Session==null?600:Math.Max(0,(int)Math.Ceiling((Session.ExpiresAt-clock.GetUtcNow()-offset).TotalSeconds));
 public async Task StartAsync(string enrollmentId){Session=await api.WriteAsync<ExamSession>(HttpMethod.Post,$"exam-sessions/{AcademyApi.Id(enrollmentId)}/start",new{});revision=Session.AnswerRevision;HasConflict=false;offset=Session.ServerTime-clock.GetUtcNow();Answers=new(Session.SavedAnswers);MultipleAnswers=new(Session.SavedMultipleAnswers);}
 public async Task RecoverAsync(){if(Session==null)return;await gate.WaitAsync();try{var snapshot=await api.GetAsync<ExamSnapshot>($"exam-sessions/{AcademyApi.Id(Session.Id)}");if(snapshot.Result!=null){Result=snapshot.Result;HasConflict=false;return;}if(snapshot.Session is {} current){Session=current;revision=current.AnswerRevision;offset=current.ServerTime-clock.GetUtcNow();Answers=new(current.SavedAnswers);MultipleAnswers=new(current.SavedMultipleAnswers);HasConflict=false;}}finally{gate.Release();}}
 private async Task SaveAsync(){try{revision=await api.SaveExamAnswersAsync(Session!.Id,Payload(),revision);}catch(ApiException e)when(e.Status==409){HasConflict=true;throw;}}
 public async Task ChooseAsync(string id,int choice){await gate.WaitAsync();try{if(Session==null||Result!=null||RemainingSeconds==0||HasConflict)return;var question=Session.Questions.FirstOrDefault(q=>q.Id==id);if(question==null||choice<0||choice>=question.Options.Count)return;if(question.AllowMultiple){var selected=MultipleAnswers.GetValueOrDefault(id,[]);MultipleAnswers[id]=selected.Contains(choice)?selected.Where(v=>v!=choice).ToArray():[..selected,choice];}else Answers[id]=choice;await SaveAsync();}finally{gate.Release();}}
 public async Task SubmitAsync(string enrollmentId,string userId){if(Session==null||Result!=null)return;await gate.WaitAsync();try{if(Result!=null)return;if(HasConflict)throw new ApiException("Recupera las respuestas del servidor antes de finalizar.",409);if(RemainingSeconds>0)await SaveAsync();try{Result=await api.WriteAsync<ExamResult>(HttpMethod.Post,"exams/submit",new{enrollmentId,userId,selectedAnswers=Answers,selectedMultipleAnswers=MultipleAnswers,answerRevision=revision,attemptId=Session.Id,durationSeconds=Math.Clamp(600-RemainingSeconds,0,600)});}catch(ApiException e)when(e.Status==409){HasConflict=true;throw;}}finally{gate.Release();}}
}
