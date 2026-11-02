namespace ForensicEdu.Mobile.Services;
public interface IScreenProtection{void SetProtected(bool enabled);}
public interface IDocumentPreview:IDisposable{int PageCount{get;}Task OpenPdfAsync(byte[] bytes);Task<byte[]> PageAsync(int index);}
