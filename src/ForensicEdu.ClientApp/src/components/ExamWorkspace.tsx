import {useCallback,useEffect,useRef,useState} from 'react';
import {useAuth} from '../context/AuthContext';
import {useApp} from '../context/AppContext';
import {apiRequest,jsonRequest,ApiError} from '../services/apiTransport';
import type {ExamSessionDto,ExamResultDto} from '../services/platformDtos';
import {ResponsiveDialog} from './dialogs/ResponsiveDialog';
import {ExamModal} from './ExamModal';
import {button,secondary,Notice,message} from './PlatformUi';
type Active={enrollmentId:string;courseTitle:string;session?:ExamSessionDto;result?:ExamResultDto};
type Request={enrollmentId:string;courseTitle:string};
const marker=(id:string)=>'forensicedu:exam:'+id;
export function ExamWorkspace(){
 const {user,isRestoring}=useAuth();const {reload,setActiveTab}=useApp();const [active,setActive]=useState<Active|null>(null);const [pending,setPending]=useState<Request|null>(null);const [busy,setBusy]=useState(false);const [error,setError]=useState('');const [recovering,setRecovering]=useState(false);const generation=useRef(0);const activeRef=useRef(active);activeRef.current=active;
 const remember=(id:string)=>{if(user)try{localStorage.setItem(marker(user.id),id);}catch{/* Server recovery remains available. */}};
 const restore=useCallback(async()=>{if(!user)return;const version=++generation.current;setError('');let saved='';try{saved=localStorage.getItem(marker(user.id))||'';}catch{}if(saved)setRecovering(true);
  try{let value=await apiRequest<Active|null>('/exam-sessions/active',{cache:'no-store',signal:AbortSignal.timeout(12000)});if(!value&&saved){try{value=await apiRequest<Active>('/exam-sessions/'+encodeURIComponent(saved),{cache:'no-store',signal:AbortSignal.timeout(12000)});}catch(e){if(e instanceof ApiError&&e.status===404){try{localStorage.removeItem(marker(user.id));}catch{}}else throw e;}}if(version===generation.current){setActive(value);if(value?.session)try{localStorage.setItem(marker(user.id),value.session.id);}catch{}}}
  catch(e){if(version===generation.current)setError(message(e));}finally{if(version===generation.current)setRecovering(false);}
 },[user?.id]);
 useEffect(()=>{setActive(null);setPending(null);void restore();return()=>{generation.current++;};},[restore]);
 useEffect(()=>{if(!user)return;const open=(event:Event)=>{if(activeRef.current)return;const request=(event as CustomEvent<Request>).detail;if(request?.enrollmentId&&request.courseTitle){setError('');setPending(request);}};const online=()=>{if(!activeRef.current)void restore();};window.addEventListener('forensicedu:request-exam',open);window.addEventListener('online',online);return()=>{window.removeEventListener('forensicedu:request-exam',open);window.removeEventListener('online',online);};},[user?.id,restore]);
 async function start(){if(!pending||busy||!user)return;setBusy(true);setError('');try{const session=await apiRequest<ExamSessionDto>('/exam-sessions/'+pending.enrollmentId+'/start',{...jsonRequest('POST'),signal:AbortSignal.timeout(15000)});remember(session.id);setActive({...pending,session});setPending(null);}catch(e){setError(message(e));}finally{setBusy(false);}}
 if(isRestoring)return <div className="exam-screen-overlay grid place-items-center" role="status">Recuperando tu sesión…</div>;
 if(!user)return null;
 if(active)return <ExamModal key={active.session?.id||active.result?.attemptId} initial={active.session} initialResult={active.result} enrollmentId={active.enrollmentId} courseTitle={active.courseTitle} onClose={()=>{try{localStorage.removeItem(marker(user.id));}catch{}setActive(null);setActiveTab('mis-cursos');void reload();}}/>;
 if(pending||recovering||error)return <ResponsiveDialog label={pending?'Antes de iniciar tu evaluación':'Recuperar evaluación'} closeDisabled={busy||recovering} onClose={()=>{setPending(null);setError('');}}><h2 className="font-display text-2xl font-bold">{pending?'¿Listo para rendir tu evaluación?':'Recuperar evaluación'}</h2>{pending&&<><p className="font-semibold">{pending.courseTitle}</p><ul className="list-disc pl-5 space-y-2 text-sm text-muted"><li>Dispones de 10 minutos continuos desde que confirmes.</li><li>Nota mínima 13/20 y máximo dos intentos.</li><li>Las preguntas indican si debes marcar una o varias respuestas.</li><li>Recargar, cerrar o cambiar de pestaña no detiene el reloj.</li><li>Al agotarse el tiempo, se califican las respuestas guardadas en el servidor.</li></ul></>}<Notice error>{error}</Notice>{recovering?<p>Recuperando tu evaluación…</p>:<div className="flex flex-wrap gap-3">{pending?<><button className={secondary} disabled={busy} onClick={()=>setPending(null)}>Seguir repasando</button><button data-testid="confirm-start-exam" className={button} disabled={busy} onClick={()=>void start()}>{busy?'Iniciando…':'Confirmar e iniciar evaluación'}</button></>:<button className={button} onClick={()=>void restore()}>Reintentar recuperación</button>}</div>}</ResponsiveDialog>;
 return null;
}
