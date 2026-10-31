namespace ForensicEdu.Mobile.Services;
public sealed class DocumentPreview:IDocumentPreview{public int PageCount=>0;public Task OpenPdfAsync(byte[] bytes)=>throw new NotSupportedException("El visor nativo iOS se habilitará en la siguiente etapa.");public Task<byte[]> PageAsync(int index)=>throw new NotSupportedException();public void Dispose(){}}
