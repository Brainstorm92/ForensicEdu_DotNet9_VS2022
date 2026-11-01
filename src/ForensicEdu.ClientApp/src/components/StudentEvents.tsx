import {useEffect,useRef,useState} from 'react';
import {CalendarDays,Clock3,ExternalLink,Radio,ShieldCheck,Video} from 'lucide-react';
import type {EventItem,Order} from '../types';
import {apiRequest} from '../services/apiTransport';
import {Empty,Notice,button,message} from './PlatformUi';

const dateFormat=new Intl.DateTimeFormat('es-PE',{day:'numeric',month:'long',year:'numeric',timeZone:'America/Lima'});
const timeFormat=new Intl.DateTimeFormat('es-PE',{hour:'2-digit',minute:'2-digit',timeZone:'America/Lima'});
function accessState(event:EventItem,now:number){
 if(event.accessStatus==='CANCELADO')return {available:false,label:'Evento cancelado',detail:'El acceso a la sala está cerrado.'};
 if(event.accessStatus==='FINALIZADO')return {available:false,label:'Evento finalizado',detail:'El acceso a la sala está cerrado.'};
 const start=Date.parse(event.date);
 if(!Number.isFinite(start))return {available:false,label:'Fecha pendiente',detail:'La fecha del evento está pendiente de configuración.'};
 if(now<start-15*60*1000)return {available:false,label:'Próxima apertura',detail:'El acceso se habilita 15 minutos antes del inicio.'};
 return {available:true,label:event.accessStatus==='EN_VIVO'?'En vivo ahora':'Acceso habilitado',detail:'La sala se abrirá en una pestaña nueva.'};
}

export function StudentEvents({events,orders,userId}:{events:EventItem[];orders:Order[];userId:string}){
 const [now,setNow]=useState(Date.now);const [opening,setOpening]=useState<string|null>(null);const [error,setError]=useState('');const [manualLink,setManualLink]=useState<{eventId:string;url:string}|null>(null);const pending=useRef(false);
 useEffect(()=>{const timer=window.setInterval(()=>setNow(Date.now()),30000);return()=>window.clearInterval(timer);},[]);
 const accredited=events.flatMap(event=>{const order=orders.find(order=>order.userId===userId&&order.productType==='evento'&&order.productId===event.id&&order.status==='APROBADA');return order?[{event,order}]:[];});
 async function enter(event:EventItem){
  if(pending.current)return;
  setError('');setManualLink(null);
  pending.current=true;setOpening(event.id);
  try{
   const result=await apiRequest<{url?:string|null}>('/student/events/'+encodeURIComponent(event.id)+'/access');
   if(typeof result.url!=='string'||!result.url.trim())throw new Error('El evento aún no tiene un enlace de acceso configurado. Contacta a soporte o inténtalo más tarde.');
   let url:URL;
   try{url=new URL(result.url.trim());}catch{throw new Error('El enlace del evento no es válido. Contacta a soporte.');}
   if(url.protocol!=='https:'||url.username||url.password)throw new Error('El enlace del evento no es válido. Contacta a soporte.');
   // No tab is created until the API authorizes access and the URL is validated.
   const tab=window.open(url.href,'_blank');
   if(tab)tab.opener=null;
   else{setManualLink({eventId:event.id,url:url.href});setError('El navegador bloqueó la pestaña nueva. Puedes abrir la sala con el enlace que aparece en la tarjeta.');}
  }catch(e){setError(message(e));}
  finally{pending.current=false;setOpening(null);}
 }
 return <section className="space-y-5" aria-label="Mis eventos y congresos"><div className="flex flex-wrap justify-between gap-3 items-start"><p className="text-sm text-muted max-w-2xl">Consulta tus inscripciones acreditadas y accede a la sala virtual desde 15 minutos antes del inicio.</p><span className="inline-flex items-center gap-2 rounded-full border border-brand/30 bg-brand/10 px-3 py-1.5 text-xs font-semibold text-brand"><Video size={14}/>Sala externa · Meet o Zoom</span></div>
 <Notice error>{error}</Notice>
 <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line pb-4"><p className="inline-flex items-center gap-2 text-sm font-semibold text-brand"><ShieldCheck size={17}/>Eventos con inscripción acreditada ({accredited.length})</p><p className="text-xs text-muted">Acceso personal con pago aprobado</p></div>
 {!accredited.length?<Empty>No tienes eventos con pago aprobado. Revisa el catálogo para conocer los próximos congresos y seminarios.</Empty>:<div className="grid lg:grid-cols-2 gap-5">{accredited.map(({event,order})=>{
  const state=accessState(event,now);const validDate=Number.isFinite(Date.parse(event.date));const live=event.accessStatus==='EN_VIVO'&&state.available;
  return <article key={event.id} className="min-w-0 flex flex-col rounded-2xl border border-line bg-panel p-5 sm:p-6 shadow-sm" aria-labelledby={'my-event-'+event.id}>
   <div className="flex flex-wrap items-center justify-between gap-3"><span className="inline-flex rounded-md bg-brand/10 px-2.5 py-1 text-[11px] font-bold uppercase tracking-wide text-brand">{event.type||'Evento'}</span>{live?<span className="inline-flex items-center gap-1.5 rounded-full border border-brand/30 bg-brand/10 px-2.5 py-1 text-[11px] font-bold uppercase text-brand"><span className="h-1.5 w-1.5 rounded-full bg-brand"/>En vivo ahora</span>:<span className="inline-flex items-center gap-1.5 text-xs text-muted"><Clock3 size={14}/>{state.label}</span>}</div>
   <h2 id={'my-event-'+event.id} className="text-lg sm:text-xl font-bold leading-snug break-words mt-4">{event.title}</h2>
   <p className="flex items-start gap-2 text-xs text-muted mt-3"><CalendarDays size={15} className="shrink-0 mt-0.5"/><span>{validDate?dateFormat.format(new Date(event.date)):'Fecha por confirmar'}{validDate&&<> · {timeFormat.format(new Date(event.date))} (Lima)</>}{event.time&&<span className="block mt-1">Horario: {event.time}</span>}</span></p>
   {event.description&&<p className="text-sm text-muted leading-relaxed mt-4 whitespace-pre-line break-words">{event.description}</p>}
   <p className="text-sm text-muted mt-4">Expositor: <span className="font-semibold text-ink">{event.speaker||'Por confirmar'}</span></p>
   <dl className="rounded-xl border border-line bg-canvas p-3 sm:p-4 space-y-3 text-xs mt-5 mb-5"><div className="flex flex-wrap items-start justify-between gap-2"><dt className="text-muted">Orden acreditada</dt><dd className="font-mono font-semibold text-brand break-all">{order.orderCode||'Pago aprobado'}</dd></div><div className="flex flex-wrap items-start justify-between gap-2"><dt className="text-muted">Estado del acceso</dt><dd className={state.available?'font-semibold text-brand':'font-semibold text-ink'}>{state.label}</dd></div><div className="flex flex-wrap items-start justify-between gap-2"><dt className="text-muted">Inscripción</dt><dd className="text-brand font-medium inline-flex items-center gap-1"><ShieldCheck size={13}/>Pago aprobado</dd></div></dl>
   <div className="mt-auto"><button type="button" className={button+' w-full min-h-11'} disabled={opening!==null||!state.available} onClick={()=>void enter(event)}><Radio size={16} className="shrink-0"/><span>{opening===event.id?'Validando acceso…':'Ingresar al evento'}</span><ExternalLink size={15} className="shrink-0"/></button>{manualLink?.eventId===event.id&&<a href={manualLink.url} target="_blank" rel="noopener noreferrer" className="mt-3 inline-flex w-full items-center justify-center gap-2 text-sm font-semibold text-brand underline">Abrir sala en una pestaña nueva<ExternalLink size={14}/></a>}<p className="text-xs text-muted text-center mt-2 leading-relaxed">{state.detail}</p></div>
  </article>;
 })}</div>}
 </section>;
}
