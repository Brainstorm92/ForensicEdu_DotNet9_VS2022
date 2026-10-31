using System.IO.Compression;
using System.Text;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Handlers.Documents.Commands.UploadCourseDocument;
using ForensicEdu.Application.Handlers.Documents.Queries.GetProtectedDocument;
using ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetDocumentAccess;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Documents;
using ForensicEdu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var count=0;
void Check(bool value,string label){if(!value)throw new Exception(label);count++;Console.WriteLine("OK: "+label);}
var processor=new OfficeDocumentProcessor();
byte[] Word(string text,bool malicious=false){
 using var output=new MemoryStream();using(var zip=new ZipArchive(output,ZipArchiveMode.Create,true)){
  void Part(string name,string value){using var writer=new StreamWriter(zip.CreateEntry(name).Open());writer.Write(value);}
  Part("[Content_Types].xml","""<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/></Types>""");
  Part("word/document.xml","""<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body><w:p><w:r><w:t>"""+System.Net.WebUtility.HtmlEncode(text)+"</w:t></w:r></w:p></w:body></w:document>");
  Part("word/_rels/document.xml.rels","""<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="evil" Target="https://outside.invalid/track" TargetMode="External"/></Relationships>""");
  if(malicious)Part("word/vbaProject.bin","macro");
 }return output.ToArray();
}
void Invalid(byte[] bytes,string name,string label){try{processor.Validate(bytes,name);throw new Exception(label);}catch(InvalidDataException){Check(true,label);}}
var pdf=Encoding.ASCII.GetBytes("%PDF-1.7\nsynthetic test\n%%EOF");var word=Word("Material Word <script>alert(1)</script>");
Check(processor.Validate(pdf,"guia.PDF")=="PDF","PDF signature and extension accepted");
Check(processor.Validate(word,"guia.docx")=="DOCX","DOCX package accepted");
Invalid(Encoding.UTF8.GetBytes("not a pdf"),"fake.pdf","Forged PDF rejected");Invalid(pdf,"old.doc","Legacy binary DOC rejected with conversion guidance");
Invalid(word,"fake.pdf","DOCX disguised as PDF rejected");Invalid(Word("macro",true),"unsafe.docx","Embedded VBA rejected");
Invalid(new byte[PrivateDocuments.MaxBytes+1],"large.pdf","20 MB size enforced");
var html=processor.RenderWord(word,"Alumno <img src=x> DNI prueba");
Check(!html.Contains("<script>")&&html.Contains("&lt;script&gt;"),"Word text is HTML encoded");
Check(!html.Contains("outside.invalid")&&html.Contains("default-src 'none'"),"External relationships not rendered, strict CSP included");
Check(html.Contains("&lt;img src=x&gt;")&&html.Contains("watermark"),"Personal watermark encoded safely");
using var db=new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
var actor=new Actor{UserId="admin",Role="1"};var storage=new Storage();
var course=new Course{Id="course-a",Title="Private"};var module=new CourseModule{Id="module-a",CourseId=course.Id,Course=course};course.Modules.Add(module);
var otherCourse=new Course{Id="course-b",Title="Other"};var otherModule=new CourseModule{Id="module-b",CourseId=otherCourse.Id,Course=otherCourse};otherCourse.Modules.Add(otherModule);
var user=new User{Id="student-a",FullName="Alumno Prueba",Dni="90000001",Role=UserRole.ESTUDIANTE,ValidUntil=DateTime.UtcNow.AddDays(30)};
var other=new User{Id="student-b",FullName="Otro Alumno"};
var order=new Order{Id="paid-order",UserId=user.Id,ProductId=course.Id,Status=OrderStatus.APROBADO};
var enrollment=new Enrollment{Id="enrollment-a",UserId=user.Id,CourseId=course.Id,OrderId=order.Id,Course=course};
db.AddRange(course,otherCourse,user,other,order,enrollment);await db.SaveChangesAsync();
var upload=new UploadCourseDocumentCommandHandler(db,actor,storage,processor);
var input=new UploadCourseDocumentCommand(course.Id,module.Id,"PDF privado","guia.pdf",pdf);
Check((await new UploadCourseDocumentCommandValidator().ValidateAsync(input)).IsValid,"Valid command passes validator");
Check(!(await new UploadCourseDocumentCommandValidator().ValidateAsync(input with{Bytes=new byte[PrivateDocuments.MaxBytes+1]})).IsValid,"Command size pipeline rejects oversized file");
Check((await upload.Handle(input with{ModuleId=otherModule.Id},default)).Outcome==ResultOutcome.NotFound,"Cannot attach material to another course module");
actor.Role="6";Check((await upload.Handle(input,default)).Outcome==ResultOutcome.Forbidden&&storage.Saves==0,"Student cannot upload course materials");
actor.Role="1";Check((await upload.Handle(input with{Bytes=Encoding.UTF8.GetBytes("forged")},default)).Outcome==ResultOutcome.Invalid&&storage.Saves==0,"Invalid file never reaches storage");
Check((await upload.Handle(input,default)).Outcome==ResultOutcome.Success,"Private upload persists metadata");var pdfDoc=await db.CourseDocuments.SingleAsync();
Check(pdfDoc.RequireWatermark&&PrivateDocuments.Key(pdfDoc)!=null&&storage.Saves==1,"Only private key persisted; mandatory watermark");
await upload.Handle(input with{Title="Word privado",FileName="guia.docx",Bytes=word},default);var wordDoc=await db.CourseDocuments.SingleAsync(x=>x.FileType=="DOCX");
actor.UserId=user.Id;actor.Role="6";
var read=new GetProtectedDocumentQueryHandler(db,actor,storage,processor);var meta=new GetDocumentAccessQueryHandler(db,actor);
var access=await meta.Handle(new(enrollment.Id,pdfDoc.Id),default);var accessJson=JsonSerializer.Serialize(access.Value);
Check(access.Outcome==ResultOutcome.Success&&!accessJson.Contains("rustfs")&&!accessJson.Contains("StorageUrl")&&!accessJson.Contains("url"),"Metadata has no storage or download URL");
var content=await read.Handle(new(enrollment.Id,pdfDoc.Id),default);Check(content.Value is DownloadContent c&&c.Bytes.SequenceEqual(pdf),"Paid owner can read PDF");
var wordContent=await read.Handle(new(enrollment.Id,wordDoc.Id),default);Check(wordContent.Value is DownloadContent w&&w.ContentType.StartsWith("text/html")&&Encoding.UTF8.GetString(w.Bytes).Contains("Alumno Prueba"),"Word returns safe HTML and personal watermark, not original DOCX");
actor.UserId=other.Id;var reads=storage.Reads;Check((await read.Handle(new(enrollment.Id,pdfDoc.Id),default)).Outcome==ResultOutcome.NotFound&&storage.Reads==reads,"IDOR blocked before storage read");
actor.UserId=user.Id;order.Status=OrderStatus.RECHAZADO;await db.SaveChangesAsync();Check((await read.Handle(new(enrollment.Id,pdfDoc.Id),default)).Outcome==ResultOutcome.NotFound,"Revoked payment blocks read");
order.Status=OrderStatus.APROBADO;order.ProductId=otherCourse.Id;await db.SaveChangesAsync();Check((await read.Handle(new(enrollment.Id,pdfDoc.Id),default)).Outcome==ResultOutcome.NotFound,"Payment for another course cannot authorize read");
order.ProductId=course.Id;enrollment.Status="CANCELADA";await db.SaveChangesAsync();Check((await read.Handle(new(enrollment.Id,pdfDoc.Id),default)).Outcome==ResultOutcome.NotFound,"Canceled enrollment blocked");
enrollment.Status="ACTIVA";user.Status=AccountStatus.SUSPENDIDO;await db.SaveChangesAsync();Check((await read.Handle(new(enrollment.Id,pdfDoc.Id),default)).Outcome==ResultOutcome.NotFound,"Suspended account blocked");
user.Status=AccountStatus.ACTIVO;user.ValidUntil=DateTime.UtcNow.AddDays(-1);await db.SaveChangesAsync();Check((await read.Handle(new(enrollment.Id,pdfDoc.Id),default)).Outcome==ResultOutcome.NotFound,"Expired account blocked");
user.ValidUntil=DateTime.UtcNow.AddDays(30);enrollment.Status="COMPLETADA";await db.SaveChangesAsync();Check((await read.Handle(new(enrollment.Id,pdfDoc.Id),default)).Outcome==ResultOutcome.Download,"Completed enrollment retains access");
pdfDoc.StorageUrl="https://outside.invalid/document.pdf";await db.SaveChangesAsync();Check((await meta.Handle(new(enrollment.Id,pdfDoc.Id),default)).Outcome==ResultOutcome.Unavailable,"Legacy public link blocked instead of returned");
Check((await read.Handle(new(enrollment.Id,Guid.NewGuid().ToString()),default)).Outcome==ResultOutcome.NotFound,"Unrelated document not accessible");
actor.UserId="admin";actor.Role="1";storage.Fail=true;var before=await db.CourseDocuments.CountAsync();Check((await upload.Handle(input,default)).Outcome==ResultOutcome.Unavailable&&await db.CourseDocuments.CountAsync()==before,"Storage failure does not create a document record");
Check(await db.AuditLogs.AnyAsync(x=>x.Action=="PRIVATE_DOCUMENT_VIEWED"),"Material reads audited");
Console.WriteLine($"PASS private documents: {count} security and rendering checks.");
sealed class Actor:ICurrentUser{public string UserId{get;set;}="";public string Role{get;set;}="";public bool IsInRole(string role)=>role==Role;}
sealed class Storage:IDocumentStorage{
 public int Saves,Reads;public bool Fail;private readonly Dictionary<string,byte[]> files=new();
 public Task SaveAsync(string key,byte[] bytes,string contentType,CancellationToken ct){if(Fail)throw new IOException();Saves++;files[key]=bytes;return Task.CompletedTask;}
 public Task<byte[]?> ReadAsync(string key,CancellationToken ct){Reads++;return Task.FromResult(files.GetValueOrDefault(key));}
 public Task DeleteAsync(string key,CancellationToken ct){files.Remove(key);return Task.CompletedTask;}
}
