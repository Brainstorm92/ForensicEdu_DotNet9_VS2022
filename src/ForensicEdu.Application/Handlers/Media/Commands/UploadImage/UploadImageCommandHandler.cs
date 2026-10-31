using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Media.Commands.UploadImage;
public sealed class UploadImageCommandHandler(IImageStorage storage,IImageProcessor processor,ICurrentUser current):IRequestHandler<UploadImageCommand,ApplicationResult>
{
 public async Task<ApplicationResult> Handle(UploadImageCommand request,CancellationToken ct){
  if(!EndpointSupport.HasRole(current,"1","2"))return ApplicationResult.Forbidden();
  byte[] image;try{image=await processor.NormalizeAsync(request.Bytes,ct);}catch(InvalidDataException e){return ApplicationResult.Invalid(new{message=e.Message});}
  var id=Guid.NewGuid().ToString("N");
  try{await storage.SaveAsync(id,image,ct);}catch(Exception e) when(e is not OperationCanceledException){return ApplicationResult.Unavailable(new{message="No pudimos subir la imagen. Inténtalo de nuevo en unos momentos."});}
  return ApplicationResult.Success(new{id,url="/api/media/images/"+id,contentType="image/webp",size=image.Length});
 }
}
