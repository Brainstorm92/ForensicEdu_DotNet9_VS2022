import {useEffect,useState} from 'react';
import {useApp} from '../context/AppContext';
import {currentEnrollment,canReenroll} from '../services/enrollmentCycles';
import {apiRequest} from '../services/apiTransport';
import {OrderAndPaymentModal} from './OrderAndPaymentModal';
import {ResponsiveDialog} from './dialogs/ResponsiveDialog';
import {Panel,Empty,Notice,button,secondary,message} from './PlatformUi';
import type {Course,Enrollment} from '../types';
export function EnrollmentCycles(){
 const {enrollments,currentUser,courses,orders,payments,setActiveTab,setSelectedEnrollmentId,setSelectedCourseId}=useApp();
 const [purchase,setPurchase]=useState<Course|null>(null);const [history,setHistory]=useState<Enrollment|null>(null);
 const mine=enrollments.filter(e=>e.userId===currentUser.id).sort((a,b)=>new Date(b.enrollmentDate).getTime()-new Date(a.enrollmentDate).getTime());
 if(!mine.length)return <Empty>No tienes matrículas habilitadas. Se crearán cuando administración apruebe un pago real registrado.</Empty>;
 return <><div className="grid sm:grid-cols-2 xl:grid-cols-3 gap-5">{mine.map(e=>{
  const course=courses.find(c=>c.id===e.courseId);const latest=currentEnrollment(mine,currentUser.id,e.courseId);const previous=latest?.id!==e.id;
  const eligible=!previous&&canReenroll(e);const pending=payments.some(p=>['PENDIENTE','EN_REVISIÓN','OBSERVADO'].includes(p.status)&&orders.some(o=>o.id===p.orderId&&o.userId===currentUser.id&&o.productId===e.courseId&&o.productType==='curso'));
  return <Panel key={e.id}><div className="flex flex-wrap items-center gap-2"><span className="admin-badge">{previous?'Matrícula anterior':eligible?'Intentos agotados':'Matrícula actual'}</span></div><h2 className="text-lg font-bold">{course?.title||e.courseId}</h2><p className="text-xs text-muted">Matrícula del {new Date(e.enrollmentDate).toLocaleDateString('es-PE')} · {e.id.slice(0,8)}</p><p>Progreso: {e.progressPercent}% · Intentos: {e.attemptsUsed}/2</p><p className="text-sm text-muted">{e.examStatus}{e.finalScore!==undefined?' · '+e.finalScore+'/20':''}</p><div className="flex flex-wrap gap-2">
  {!previous&&(e.status==='ACTIVA'||e.status==='COMPLETADA')&&<button data-testid={'classroom-'+e.courseId} className={button} onClick={()=>{setSelectedEnrollmentId(e.id);setSelectedCourseId(e.courseId);}}>Entrar al aula</button>}
  <button className={secondary} onClick={()=>setHistory(e)}>Historial de intentos</button>
  {eligible&&(pending?<button className={secondary} onClick={()=>setActiveTab('mis-pagos')}>Ver pago en revisión</button>:course?.isActive!==false&&course?.isPublished!==false&&course&&<button data-testid={'reenroll-'+e.courseId} className={button} onClick={()=>setPurchase(course)}>Solicitar nueva matrícula</button>)}
  </div>{eligible&&<p className="text-xs text-muted">La nueva matrícula se habilita al aprobar el nuevo pago. Conservaremos tus notas anteriores y comenzarás con progreso cero.</p>}{previous&&<p className="text-xs text-muted">Historial conservado. La matrícula actual se muestra por separado.</p>}</Panel>;
 })}</div>{purchase&&<OrderAndPaymentModal item={purchase} productType="curso" reenrollment onClose={()=>setPurchase(null)}/ >}{history&&<AttemptHistory enrollment={history} onClose={()=>setHistory(null)}/ >}</>;
}
type Attempt={id:string;attemptNumber:number;score:number;approved:boolean;completedAt:string|null};
function AttemptHistory({enrollment,onClose}:{enrollment:Enrollment;onClose:()=>void}){
 const [attempts,setAttempts]=useState<Attempt[]>([]);const [error,setError]=useState('');const [loading,setLoading]=useState(true);
 useEffect(()=>{let live=true;void apiRequest<Attempt[]>('/student/enrollments/'+encodeURIComponent(enrollment.id)+'/attempts').then(rows=>{if(live)setAttempts(rows);}).catch(e=>{if(live)setError(message(e));}).finally(()=>{if(live)setLoading(false);});return()=>{live=false;};},[enrollment.id]);
 return <ResponsiveDialog label="Historial de intentos" closeLabel="Cerrar historial de intentos" onClose={onClose}><h2 className="text-xl font-bold">Historial de intentos</h2><p className="text-xs font-mono break-all text-muted">Matrícula: {enrollment.id}</p><Notice error>{error}</Notice>{loading?<p role="status">Cargando intentos…</p>:!error&&!attempts.length?<p className="text-muted">Esta matrícula todavía no tiene evaluaciones.</p>:attempts.map(a=><div key={a.id} className="rounded-xl border border-line p-4 space-y-2"><h3 className="font-semibold">Intento {a.attemptNumber}</h3><p>{a.completedAt?(a.approved?'Aprobado':'Desaprobado')+' · '+a.score+'/20':'En curso'}</p>{a.completedAt&&<p className="text-xs text-muted">{new Date(a.completedAt).toLocaleString('es-PE',{timeZone:'America/Lima'})} · Lima</p>}</div>)}</ResponsiveDialog>;
}
