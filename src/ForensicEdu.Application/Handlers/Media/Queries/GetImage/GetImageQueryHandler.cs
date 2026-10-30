using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Media.Queries.GetImage;
public sealed class GetImageQueryHandler(IImageStorage storage):IRequestHandler<GetImageQuery,ApplicationResult>
{
 public async Task<ApplicationResult> Handle(GetImageQuery request,CancellationToken ct){
  try{var bytes=await storage.ReadAsync(request.Id,ct);return bytes==null?ApplicationResult.NotFound():ApplicationResult.Download(bytes,"image/webp",request.Id+".webp");}
  catch(Exception e) when(e is not OperationCanceledException){return ApplicationResult.Unavailable(new{message="La imagen no está disponible en este momento."});}
 }
}
