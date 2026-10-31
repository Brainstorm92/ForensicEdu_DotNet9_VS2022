using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetDocumentAccess;
public sealed class GetDocumentAccessQueryHandler(IApplicationDbContext db,ICurrentUser current):IRequestHandler<GetDocumentAccessQuery,ApplicationResult>
{
 public async Task<ApplicationResult> Handle(GetDocumentAccessQuery request,CancellationToken ct)
 {
  var access=await PrivateDocuments.AuthorizeAsync(db,current,request.Id,request.DocumentId,ct);
  if(access==null)return ApplicationResult.NotFound();
  var (document,user)=access.Value;
  if(PrivateDocuments.Key(document)==null)return ApplicationResult.Unavailable(new{message="Este material debe volver a subirse como archivo privado."});
  return ApplicationResult.Success(new{document.Title,document.FileType,watermark=PrivateDocuments.Watermark(user)});
 }
}
