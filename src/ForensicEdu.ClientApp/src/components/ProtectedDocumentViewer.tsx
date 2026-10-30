import {useEffect,useRef,useState} from 'react';
import {createPortal} from 'react-dom';
import {apiRequest,readProtectedResponse} from '../services/apiTransport';
import {message,secondary} from './PlatformUi';
import type {PDFDocumentProxy} from 'pdfjs-dist';

export function ProtectedDocumentViewer({enrollmentId,documentId,onClose}:{enrollmentId:string;documentId:string;onClose:()=>void}){
 const [info,setInfo]=useState<{title:string;fileType:string;watermark:string}|null>(null);
 const [pdf,setPdf]=useState<PDFDocumentProxy|null>(null);const [html,setHtml]=useState('');
 const [error,setError]=useState('');const [loading,setLoading]=useState(true);const [page,setPage]=useState(1);
 const [zoom,setZoom]=useState(1);const [width,setWidth]=useState(800);const [rendering,setRendering]=useState(false);
 const canvas=useRef<HTMLCanvasElement>(null);const viewport=useRef<HTMLDivElement>(null);const dialog=useRef<HTMLDivElement>(null);
 const closeRef=useRef(onClose);closeRef.current=onClose;
 const path=`/student/enrollments/${enrollmentId}/documents/${documentId}`;
 useEffect(()=>{
  const previous=document.activeElement as HTMLElement|null;const overflow=document.body.style.overflow;document.body.style.overflow='hidden';
  dialog.current?.focus();
  const key=(e:KeyboardEvent)=>{
   if(e.key==='Escape'){e.preventDefault();closeRef.current();}
   if((e.ctrlKey||e.metaKey)&&['p','s'].includes(e.key.toLowerCase()))e.preventDefault();
   if(e.key==='Tab'){
    const elements=dialog.current?.querySelectorAll<HTMLElement>('button:not(:disabled), iframe');
    if(!elements?.length)return;
    const first=elements[0],last=elements[elements.length-1];
    if(e.shiftKey&&(document.activeElement===first||document.activeElement===dialog.current)){e.preventDefault();last.focus();}
    else if(!e.shiftKey&&document.activeElement===last){e.preventDefault();first.focus();}
   }
  };
  document.addEventListener('keydown',key,true);
  return()=>{document.body.style.overflow=overflow;document.removeEventListener('keydown',key,true);previous?.focus();};
 },[]);
 useEffect(()=>{
  const controller=new AbortController();let disposed=false;let activePdf:PDFDocumentProxy|null=null;let destroy:(()=>void)|undefined;
  setLoading(true);setError('');setHtml('');setPdf(null);setPage(1);
  void (async()=>{
   try{
    const metadata=await apiRequest<{title:string;fileType:string;watermark:string}>(path,{signal:controller.signal,cache:'no-store'});
    const res=await readProtectedResponse(path+'/content',controller.signal);
    if(disposed)return;setInfo(metadata);
    if(metadata.fileType==='PDF'){
     const pdfjs=await import('pdfjs-dist');
     const worker=await import('pdfjs-dist/build/pdf.worker.min.mjs?url');pdfjs.GlobalWorkerOptions.workerSrc=worker.default;
     const task=pdfjs.getDocument({data:new Uint8Array(await res.arrayBuffer()),useWasm:false,useSystemFonts:true,disableAutoFetch:true});
     destroy=()=>{void task.destroy();};
     activePdf=await task.promise;if(disposed){destroy?.();return;}
     if(activePdf.numPages>1000)throw new Error('El documento supera el límite de 1000 páginas.');setPdf(activePdf);
    }else if(metadata.fileType==='DOCX')setHtml(await res.text());
    else throw new Error('Este formato no tiene vista de lectura.');
   }catch(e){if(!disposed){setError(message(e));setPdf(null);setHtml('');}}
   finally{if(!disposed)setLoading(false);}
  })();
  // Revoked enrollment or expired sessions clear an already-open viewer on recheck.
  const timer=setInterval(()=>{void apiRequest(path,{cache:'no-store'}).catch(()=>{if(!disposed){setError('Tu acceso al material dejó de estar disponible.');setHtml('');setPdf(null);destroy?.();}});},60000);
  return()=>{disposed=true;controller.abort();clearInterval(timer);destroy?.();};
 },[path]);
 useEffect(()=>{if(!viewport.current)return;const observer=new ResizeObserver(entries=>setWidth(Math.max(240,entries[0].contentRect.width-32)));observer.observe(viewport.current);return()=>observer.disconnect();},[]);
 useEffect(()=>{
  if(!pdf||!canvas.current||!info)return;let cancelled=false;let render:{cancel:()=>void}|undefined;setRendering(true);
  void (async()=>{try{
   const item=await pdf.getPage(page);if(cancelled)return;
   const base=item.getViewport({scale:1});const display=Math.min(width,1100)*zoom;
   const scale=Math.min(display/base.width*Math.min(window.devicePixelRatio||1,2),Math.sqrt(12000000/(base.width*base.height)));
   const view=item.getViewport({scale});const target=canvas.current!;target.width=Math.floor(view.width);target.height=Math.floor(view.height);target.style.width=`${display}px`;target.style.maxWidth='none';
   const ctx=target.getContext('2d')!;const task=item.render({canvas:target,canvasContext:ctx,viewport:view});render=task;await task.promise;if(cancelled)return;
   // Draw the personal watermark into the rendered pixels rather than a removable text layer.
   ctx.save();ctx.fillStyle='rgba(35,69,85,0.20)';ctx.font=`600 ${Math.max(14,view.width/42)}px sans-serif`;ctx.textAlign='center';
   for(let y=view.height*.22;y<view.height;y+=view.height*.3){ctx.save();ctx.translate(view.width/2,y);ctx.rotate(-Math.PI/7);ctx.fillText(info.watermark,0,0,view.width*.91);ctx.restore();}ctx.restore();
  }catch(e){if(!cancelled)setError('No se pudo mostrar esta página. Prueba con un PDF sin contraseña.');}finally{if(!cancelled)setRendering(false);}})();
  return()=>{cancelled=true;render?.cancel();};
 },[pdf,page,width,zoom,info]);
 return createPortal(<div className="document-overlay" role="presentation" onContextMenu={e=>e.preventDefault()} onCopy={e=>e.preventDefault()} onCut={e=>e.preventDefault()} onDragStart={e=>e.preventDefault()}>
  <div ref={dialog} className="document-dialog" role="dialog" aria-modal="true" aria-labelledby="document-title" tabIndex={-1}>
   <header className="document-toolbar"><div className="min-w-0"><p className="text-xs text-muted">MATERIAL DEL CURSO · LECTURA PERSONAL</p><h2 id="document-title" className="font-bold truncate">{info?.title||'Abriendo material'}</h2></div><button className={secondary} onClick={onClose} aria-label="Cerrar material">Cerrar ✕</button></header>
   {pdf&&!error&&<nav className="document-controls" aria-label="Páginas del documento"><button className={secondary} disabled={page<=1||rendering} onClick={()=>setPage(p=>p-1)}>Anterior</button><span aria-live="polite">Página {page} de {pdf.numPages}</span><button className={secondary} disabled={page>=pdf.numPages||rendering} onClick={()=>setPage(p=>p+1)}>Siguiente</button><button className={secondary} disabled={zoom<=.75||rendering} onClick={()=>setZoom(z=>Math.max(.75,z-.25))} aria-label="Reducir zoom">−</button><span>{Math.round(zoom*100)}%</span><button className={secondary} disabled={zoom>=2||rendering} onClick={()=>setZoom(z=>Math.min(2,z+.25))} aria-label="Aumentar zoom">+</button></nav>}
   <div ref={viewport} className="document-content" aria-busy={loading||rendering}>
    {loading&&<p className="p-6 text-center" role="status">Preparando tu material…</p>}
    {error&&<div className="p-6 text-center" role="alert">{error}</div>}
    {pdf&&!error&&<canvas ref={canvas} className="document-canvas" aria-label={`Página ${page} del material`} />}
    {html&&!error&&<iframe sandbox="" referrerPolicy="no-referrer" className="document-word" title="Lectura de documento Word" srcDoc={html}/>}
   </div><footer className="document-footer">Material de uso personal · {info?.watermark||'Acceso protegido'}</footer>
  </div></div>,document.body);
}
