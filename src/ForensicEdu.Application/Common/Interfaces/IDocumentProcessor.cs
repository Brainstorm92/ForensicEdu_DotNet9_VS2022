namespace ForensicEdu.Application.Common.Interfaces;
public interface IDocumentProcessor
{
 string Validate(byte[] bytes, string fileName);
 string RenderWord(byte[] bytes, string watermark);
}
