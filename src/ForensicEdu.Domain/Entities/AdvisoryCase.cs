namespace ForensicEdu.Domain.Entities;

public class AdvisoryCase
{
 public string Id {get;set;}=Guid.NewGuid().ToString();
 public string OrderId {get;set;}="";
 public string AdvisoryServiceId {get;set;}="";
 public string StudentId {get;set;}="";
 public string? ConsultantId {get;set;}
 public string Status {get;set;}="PENDIENTE_ASIGNACION";
 public DateTime? ScheduledAt {get;set;}
 public string Summary {get;set;}="";
 public string InternalNotes {get;set;}="";
 public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
 public DateTime UpdatedAt {get;set;}=DateTime.UtcNow;
 public string Version {get;set;}=Guid.NewGuid().ToString();
 public Order Order {get;set;}=null!;
 public AdvisoryService AdvisoryService {get;set;}=null!;
 public User Student {get;set;}=null!;
 public User? Consultant {get;set;}
 public ICollection<AdvisorySession> Sessions {get;set;}=new List<AdvisorySession>();
 public ICollection<AdvisoryCareHistory> History {get;set;}=new List<AdvisoryCareHistory>();
}
public class AdvisorySession
{
 public string Id {get;set;}=Guid.NewGuid().ToString();
 public string AdvisoryCaseId {get;set;}="";
 public DateTime ScheduledAt {get;set;}
 public int DurationMinutes {get;set;}=60;
 public string Status {get;set;}="PROGRAMADA";
 public string Summary {get;set;}="";
 public string InternalNotes {get;set;}="";
 public DateTime UpdatedAt {get;set;}=DateTime.UtcNow;
 public AdvisoryCase AdvisoryCase {get;set;}=null!;
}
public class AdvisoryCareHistory
{
 public string Id {get;set;}=Guid.NewGuid().ToString();
 public string AdvisoryCaseId {get;set;}="";
 public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
 public string ActorId {get;set;}="";
 public string Action {get;set;}="";
 public string Details {get;set;}="";
 public AdvisoryCase AdvisoryCase {get;set;}=null!;
}
