namespace ForensicEdu.Mobile.Core.Models;
public sealed record Question(string Id,string QuestionText,List<string> Options){[System.Text.Json.Serialization.JsonPropertyName("question")]public string Text{get;init;}="";}
public sealed record ExamQuestion{public string Id{get;init;}="";public string Question{get;init;}="";public List<string> Options{get;init;}=[];}
public sealed record ExamSession{public string Id{get;init;}="";public int AttemptNumber{get;init;}public DateTimeOffset ExpiresAt{get;init;}public DateTimeOffset ServerTime{get;init;}public List<ExamQuestion> Questions{get;init;}=[];public Dictionary<string,int> SavedAnswers{get;init;}=[];}
public sealed record ExamResult{public decimal Score{get;init;}public bool Approved{get;init;}public string Message{get;init;}="";public string? CertificateCode{get;init;}}
