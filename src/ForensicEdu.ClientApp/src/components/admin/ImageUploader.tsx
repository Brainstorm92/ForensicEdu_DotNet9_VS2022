import {useEffect,useRef,useState} from 'react';
import {apiRequest} from '../../services/apiTransport';
import {mediaImageUrl} from '../../services/mediaImages';
import {Notice,secondary,message} from '../PlatformUi';
export function ImageUploader({label,value,onChange,onBusyChange,disabled=false,testId}:{label:string;value:string;onChange:(url:string)=>void;onBusyChange:(busy:boolean)=>void;disabled?:boolean;testId:string}){
 const [busy,setBusy]=useState(false);const [error,setError]=useState('');const [notice,setNotice]=useState('');const control=useRef<AbortController|null>(null);const input=useRef<HTMLInputElement>(null);
 useEffect(()=>()=>control.current?.abort(),[]);
 async function upload(file:File){
  setError('');setNotice('');
  if(!['image/jpeg','image/png','image/webp'].includes(file.type)){setError('Selecciona una imagen JPG, PNG o WebP.');return;}
  if(file.size>5*1024*1024){setError('La imagen debe pesar como máximo 5 MB.');return;}
  setBusy(true);onBusyChange(true);const abort=new AbortController();control.current=abort;
  try{const form=new FormData();form.append('file',file);const result=await apiRequest<{url:string}>('/admin/media/images',{method:'POST',body:form,signal:abort.signal});if(!abort.signal.aborted){onChange(result.url);setNotice('Imagen subida. Guarda el curso para aplicar el cambio.');}}
  catch(e){if(!abort.signal.aborted)setError(message(e));}
  finally{if(!abort.signal.aborted){setBusy(false);onBusyChange(false);}if(input.current)input.current.value='';}
 }
 return <div className="space-y-3 rounded-xl border border-line p-4"><p className="text-sm font-semibold">{label}</p>{value?<img data-testid={`${testId}-preview`} src={mediaImageUrl(value)} alt={label} className="h-40 w-full object-contain rounded-lg bg-canvas"/>:<div className="h-24 rounded-lg bg-canvas grid place-items-center text-xs text-muted">Sin imagen</div>}<label className="block text-sm">{busy?'Subiendo imagen…':value?'Cambiar imagen':'Subir imagen'}<input ref={input} data-testid={testId} type="file" accept="image/jpeg,image/png,image/webp" disabled={disabled||busy} className="block w-full mt-2 text-sm" onChange={e=>{const file=e.target.files?.[0];if(file)void upload(file);}}/></label><p className="text-xs text-muted">JPG, PNG o WebP · Hasta 5 MB</p>{value&&<button type="button" className={secondary} disabled={disabled||busy} onClick={()=>{onChange('');setNotice('');}}>Quitar imagen</button>}<Notice error>{error}</Notice><Notice>{notice}</Notice></div>;
}
