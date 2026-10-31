using System.IO.Compression;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Management;
namespace ForensicEdu.Infrastructure.Documents;

// DOCX is rendered from an allowlist of WordprocessingML elements. No Office scripts,
// embedded HTML, external relationships, stylesheets or links are executed or fetched.
public sealed class OfficeDocumentProcessor:IDocumentProcessor
{
 private static readonly XNamespace W="http://schemas.openxmlformats.org/wordprocessingml/2006/main";
 private static readonly XNamespace R="http://schemas.openxmlformats.org/officeDocument/2006/relationships";
 private static readonly XNamespace A="http://schemas.openxmlformats.org/drawingml/2006/main";
 private const long MaxExpanded=64*1024*1024;
 public string Validate(byte[] bytes,string fileName){
  if(bytes.Length==0||bytes.Length>PrivateDocuments.MaxBytes)throw new InvalidDataException("El material debe pesar como máximo 20 MB.");
  var ext=Path.GetExtension(fileName).ToLowerInvariant();
  if(ext==".pdf"){
   if(bytes.Length<16 || !bytes.AsSpan(0,5).SequenceEqual("%PDF-"u8) || !Encoding.ASCII.GetString(bytes,Math.Max(0,bytes.Length-2048),Math.Min(2048,bytes.Length)).Contains("%%EOF"))throw new InvalidDataException("El archivo no es un PDF válido.");
   return "PDF";
  }
  if(ext!=".docx")throw new InvalidDataException("Usa PDF o Word (.docx). Guarda los archivos .doc antiguos como .docx o PDF.");
  try{
   using var archive=Open(bytes);
   if(archive.GetEntry("[Content_Types].xml")==null || archive.GetEntry("word/document.xml")==null)throw new InvalidDataException();
   var types=Xml(archive.GetEntry("[Content_Types].xml")!);
   if(!types.Descendants().Attributes("ContentType").Any(x=>x.Value=="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"))throw new InvalidDataException();
   if(archive.Entries.Any(e=>e.FullName.Contains("vbaProject",StringComparison.OrdinalIgnoreCase)||e.FullName.StartsWith("word/embeddings/",StringComparison.OrdinalIgnoreCase)))throw new InvalidDataException();
   foreach(var part in archive.Entries.Where(e=>e.FullName.EndsWith(".rels",StringComparison.OrdinalIgnoreCase)))Xml(part);
   var document=Xml(archive.GetEntry("word/document.xml")!);
   if(document.Root?.Name!=W+"document" || document.Descendants(W+"altChunk").Any())throw new InvalidDataException();
   return "DOCX";
  }catch(Exception e) when(e is InvalidDataException or XmlException or IOException){throw new InvalidDataException("El Word no es válido, contiene objetos incrustados o supera los límites de lectura.");}
 }
 private static ZipArchive Open(byte[] bytes){
  var archive=new ZipArchive(new MemoryStream(bytes,false),ZipArchiveMode.Read);
  try{
   if(archive.Entries.Count>2048)throw new InvalidDataException();
   long total=0;var names=new HashSet<string>(StringComparer.Ordinal);
   foreach(var e in archive.Entries){
    total+=e.Length;
    if(total>MaxExpanded || e.Length>16*1024*1024 || !names.Add(e.FullName) || e.FullName.Split('/').Any(p=>p=="..") || e.FullName.Contains('\\'))throw new InvalidDataException();
   }
   return archive;
  }catch{archive.Dispose();throw;}
 }
 private static XDocument Xml(ZipArchiveEntry entry){
  using var stream=entry.Open();using var reader=XmlReader.Create(stream,new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=16*1024*1024});var doc=XDocument.Load(reader);if(doc.Descendants().Any(e=>e.Ancestors().Take(65).Count()>64))throw new InvalidDataException();return doc;
 }
 private static string Escape(string text)=>WebUtility.HtmlEncode(text);
 public string RenderWord(byte[] bytes,string watermark){
  Validate(bytes,"material.docx");using var archive=Open(bytes);var document=Xml(archive.GetEntry("word/document.xml")!);
  var images=new Dictionary<string,string>();
  var relEntry=archive.GetEntry("word/_rels/document.xml.rels");
  if(relEntry!=null)foreach(var rel in Xml(relEntry).Root?.Elements()??Enumerable.Empty<XElement>()){
   var id=(string?)rel.Attribute("Id");var target=(string?)rel.Attribute("Target");
   if(id==null||target==null||(string?)rel.Attribute("TargetMode")=="External"||!target.StartsWith("media/",StringComparison.Ordinal)||target.Contains("..")||target.Contains('\\'))continue;
   var image=archive.GetEntry("word/"+target);if(image==null||image.Length>8*1024*1024)continue;
   using var input=image.Open();using var output=new MemoryStream();input.CopyTo(output);var data=output.ToArray();
   var mime=data.AsSpan().StartsWith(new byte[]{137,80,78,71,13,10,26,10})?"image/png":data.Length>3&&data[0]==255&&data[1]==216&&data[2]==255?"image/jpeg":null;
   if(mime!=null)images[id]="data:"+mime+";base64,"+Convert.ToBase64String(data);
  }
  var blocks=0;long renderedImages=0;
  string Render(XElement node){
   if(++blocks>100000)throw new InvalidDataException();
   if(node.Name==W+"t")return Escape(node.Value);
   if(node.Name==W+"tab")return " &emsp; ";if(node.Name==W+"br"||node.Name==W+"cr")return "<br>";
   if(node.Name==A+"blip"){var id=(string?)node.Attribute(R+"embed");if(id==null||!images.TryGetValue(id,out var src))return "";renderedImages+=src.Length;if(renderedImages>24*1024*1024)throw new InvalidDataException();return "<img alt=\"Imagen del material\" src=\""+src+"\">";}
   if(node.Name==W+"object"||node.Name==W+"pict"||node.Name==W+"instrText"||node.Name==W+"del"||node.Name==W+"pPr"||node.Name==W+"rPr")return "";
   var inner=string.Concat(node.Elements().Select(Render));
   if(node.Name==W+"r"){
    var props=node.Element(W+"rPr");if(props?.Element(W+"b")!=null)inner="<strong>"+inner+"</strong>";if(props?.Element(W+"i")!=null)inner="<em>"+inner+"</em>";if(props?.Element(W+"u")!=null)inner="<u>"+inner+"</u>";return inner;
   }
   if(node.Name==W+"p"){
    var style=(string?)node.Element(W+"pPr")?.Element(W+"pStyle")?.Attribute(W+"val");
    var tag=style is "Heading1" or "Titulo1"?"h1":style is "Heading2" or "Titulo2"?"h2":style=="Heading3"?"h3":"p";
    var bullet=node.Element(W+"pPr")?.Element(W+"numPr")!=null?"• ":"";return "<"+tag+">"+bullet+(inner.Length==0?"<br>":inner)+"</"+tag+">";
   }
   var tableTag=node.Name==W+"tbl"?"table":node.Name==W+"tr"?"tr":node.Name==W+"tc"?"td":null;
   return tableTag==null?inner:"<"+tableTag+">"+inner+"</"+tableTag+">";
  }
  var content=Render(document.Root!);
  return "<!doctype html><html lang=\"es\"><head><meta charset=\"utf-8\"><meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'none'; img-src data:; style-src 'unsafe-inline'; base-uri 'none'; form-action 'none'\"><style>body{margin:0;background:#e8edf2;color:#17202e;font:16px/1.65 Georgia,serif;user-select:none}article{max-width:820px;min-height:95vh;margin:20px auto;padding:48px;background:white;overflow-wrap:anywhere;box-sizing:border-box}table{border-collapse:collapse;width:100%}td{border:1px solid #ccd4dd;padding:8px}img{max-width:100%;height:auto}.watermark{position:fixed;inset:0;display:flex;align-items:center;justify-content:center;pointer-events:none;z-index:5;overflow:hidden}.watermark span{transform:rotate(-28deg);color:rgba(37,64,88,.24);font:600 20px sans-serif;max-width:90%;text-align:center;line-height:2.5}footer{font:12px sans-serif;color:#64748b}@media(max-width:600px){article{padding:20px;margin:0}}@media print{html{display:none!important}}</style></head><body><article>"+content+"<footer>Vista de lectura de Word · Material de uso personal</footer></article><div class=\"watermark\"><span>"+Escape(watermark)+"<br>"+Escape(watermark)+"</span></div></body></html>";
 }
}
