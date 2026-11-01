import {ResponsiveDialog} from './dialogs/ResponsiveDialog';
import {useEffect,useRef,useState} from 'react';
import {useApp} from '../context/AppContext';
import {apiRequest,jsonRequest} from '../services/apiTransport';
import type {ExamSessionDto,ExamResultDto} from '../services/platformDtos';
import type {Enrollment} from '../types';
import {button,secondary,Notice,message} from './PlatformUi';
export function ExamModal({enrollment,courseTitle,onClose}:{enrollment:Enrollment;courseTitle:string;onClose:()=>void}){
 const {currentUser,reload,setActiveTab}=useApp();const [session,setSession]=useState<ExamSessionDto|null>(null);const [answers,setAnswers]=useState<Record<string,number>>({});
 const answersRef=useRef<Record<string,number>>({});const queue=useRef<Promise<void>>(Promise.resolve());const [index,setIndex]=useState(0);const [left,setLeft]=useState(600);const [busy,setBusy]=useState(false);const [error,setError]=useState('');const [saved,setSaved]=useState('');const [result,setResult]=useState<ExamResultDto|null>(null);const submitting=useRef(false);const alive=useRef(true);const offset=useRef(0);const startRequest=useRef<Promise<ExamSessionDto>|null>(null);
 useEffect(()=>{alive.current=true;startRequest.current??=apiRequest<ExamSessionDto>(`/exam-sessions/${enrollment.id}/start`,jsonRequest('POST'));
  startRequest.current.then(value=>{if(!alive.current)return;offset.current=new Date(value.serverTime).getTime()-Date.now();setSession(value);answersRef.current=value.savedAnswers||{};setAnswers(answersRef.current);setLeft(Math.max(0,Math.ceil((new Date(value.expiresAt).getTime()-Date.now()-offset.current)/1000)));}).catch(err=>{if(alive.current)setError(message(err));});
  return ()=>{alive.current=false;};
 },[enrollment.id]);
 const submit=async()=>{if(!session||submitting.current||result)return;submitting.current=true;setBusy(true);setError('');
  try {await queue.current.catch(()=>{});const remaining=new Date(session.expiresAt).getTime()-Date.now()-offset.current;
   if(remaining>0)await apiRequest(`/exam-sessions/${session.id}/answers`,jsonRequest('PUT',answersRef.current));
   const value=await apiRequest<ExamResultDto>('/exams/submit',jsonRequest('POST',{enrollmentId:enrollment.id,userId:currentUser.id,selectedAnswers:answersRef.current,durationSeconds:600-left}));
   if(alive.current)setResult(value);await reload();
  }catch(err){if(alive.current)setError(message(err));}finally{submitting.current=false;if(alive.current)setBusy(false);}
 };
 const submitRef=useRef(submit);submitRef.current=submit;
 useEffect(()=>{if(!session||result)return;const tick=()=>{const remaining=Math.max(0,Math.ceil((new Date(session.expiresAt).getTime()-Date.now()-offset.current)/1000));setLeft(remaining);if(remaining===0&&!submitting.current)void submitRef.current();};const timer=setInterval(tick,1000);return ()=>clearInterval(timer);},[session,result]);
 const choose=(id:string,value:number)=>{if(!session||busy||result||left===0)return;const next={...answersRef.current,[id]:value};answersRef.current=next;setAnswers(next);setSaved('Guardando respuestas…');
  queue.current=queue.current.catch(()=>{}).then(async()=>{await apiRequest(`/exam-sessions/${session.id}/answers`,jsonRequest('PUT',next));if(alive.current)setSaved('Respuestas guardadas.');}).catch(err=>{if(alive.current){setSaved('');setError(message(err));}throw err;});void queue.current.catch(()=>{});
 };
 const question=session?.questions[index];
 return <ResponsiveDialog labelledBy="exam-title" maxWidth="48rem" closeLabel="Cerrar evaluación" closeDisabled={busy} onClose={onClose}>
 <header className="flex items-start justify-between gap-3"><div><h2 id="exam-title" className="text-xl font-bold">Evaluación · {courseTitle}</h2><p className="text-sm text-muted">10 minutos · Nota mínima 13/20 · Máximo dos intentos</p></div></header>
 <Notice error>{error}</Notice>
 {result?<><Notice>{result.message}</Notice><p data-testid="exam-result" className="text-3xl font-bold">{result.score}/20 · {result.approved?'Aprobado':'Desaprobado'}</p><p>Evaluación finalizada.</p>{result.certificateCode&&<p className="font-mono break-all text-xs">{result.certificateCode}</p>}<button className={button} onClick={()=>{onClose();setActiveTab(result.approved?'mis-certificados':'mis-cursos');}}>Ver mis resultados</button></>:session&&question?<>
 <div className="flex justify-between gap-3 text-sm"><span>Intento {session.attemptNumber} · Pregunta {index+1} de {session.questions.length}</span><strong aria-label="Tiempo restante">{Math.floor(left/60)}:{String(left%60).padStart(2,'0')}</strong></div>
 <h3 data-question-id={question.id} className="text-lg font-semibold">{question.question}</h3>
 <div className="space-y-3">{question.options.map((option,i)=><button data-option-index={i} key={i} disabled={busy||left===0} onClick={()=>choose(question.id,i)} className={`w-full text-left p-4 rounded-xl border ${answers[question.id]===i?'border-brand bg-brand/10 text-brand':'border-line bg-canvas'}`}>{String.fromCharCode(65+i)}. {option}</button>)}</div>
 <p className="text-xs text-muted" aria-live="polite">{saved}</p>
 <div className="flex justify-between flex-wrap gap-3"><button className={secondary} disabled={index===0||busy} onClick={()=>setIndex(i=>i-1)}>Anterior</button>{index<session.questions.length-1?<button className={button} disabled={busy} onClick={()=>setIndex(i=>i+1)}>Siguiente</button>:<button data-testid="submit-exam" className={button} disabled={busy} onClick={()=>void submit()}>{busy?'Enviando…':'Finalizar evaluación'}</button>}</div>
 </>:!error?<p>Preparando tu evaluación…</p>:<p className="text-sm text-muted">No se pudo iniciar la evaluación. Cierra esta ventana y revisa tu matrícula.</p>}
 </ResponsiveDialog>;
}
