import {ProtectedDocumentViewer} from './ProtectedDocumentViewer';
import {ModuleVideo} from './ModuleVideo';
import {useEffect,useState} from 'react';
import {useApp} from '../context/AppContext';
import {apiRequest} from '../services/apiTransport';
import {ExamModal} from './ExamModal';
import {button,secondary,Panel,Empty,Notice,message} from './PlatformUi';
export function VirtualClassroom({courseId,onBack}:{courseId:string;onBack:()=>void}){
 const {courses,enrollments,currentUser,updateModuleProgress,setActiveTab}=useApp();const course=courses.find(x=>x.id===courseId);const enrollment=enrollments.find(x=>x.courseId===courseId&&x.userId===currentUser.id);
 const [documentId,setDocumentId]=useState('');const [active,setActive]=useState('');const [exam,setExam]=useState(false);const [error,setError]=useState('');const [busy,setBusy]=useState(false);const [video,setVideo]=useState('');
 useEffect(()=>{if(!active&&course?.modules[0])setActive(course.modules[0].id);},[course,active]);
 useEffect(()=>{setVideo('');setError('');setDocumentId('');},[active]);
 if(!course||!enrollment)return <div className="max-w-4xl mx-auto p-6"><Empty>Tu matrícula estará disponible cuando se apruebe el pago.</Empty><button className={secondary} onClick={onBack}>Volver</button></div>;
 const module=course.modules.find(x=>x.id===active);const moduleIndex=course.modules.findIndex(x=>x.id===active);const unlocked=course.modules.slice(0,moduleIndex).every(x=>enrollment.completedModules.includes(x.id));const approved=enrollment.examStatus==='APROBADA';const exhausted=enrollment.attemptsUsed>=2;const waiting=!!enrollment.secondAttemptAvailableAt&&new Date(enrollment.secondAttemptAvailableAt).getTime()>Date.now();
 const action=async(work:()=>Promise<void>)=>{setBusy(true);setError('');try{await work();}catch(err){setError(message(err));}finally{setBusy(false);}};
 return <div className="responsive-page max-w-7xl w-full mx-auto px-4 py-6 space-y-5">
 <div className="flex flex-wrap items-center justify-between gap-4"><div><button className={secondary} onClick={onBack}>Volver a mis cursos</button><h1 className="text-2xl font-bold mt-3">{course.title}</h1></div><span>Progreso: {enrollment.progressPercent}%</span></div><Notice error>{error}</Notice>
 <div className="grid lg:grid-cols-3 gap-5"><div className="lg:col-span-2 space-y-5"><Panel>
 <h2 className="text-xl font-semibold">{module?.title||'Contenido pendiente'}</h2>
 {module?<><p className="text-muted">{module.duration}</p>{video?<ModuleVideo url={video} title={module.title}/>:<p className="text-muted">Revisa los materiales del módulo antes de marcarlo como completado.</p>}
 <div className="flex gap-3 flex-wrap"><button className={secondary} disabled={busy||!unlocked} onClick={()=>void action(async()=>{const res=await apiRequest<{url:string}>(`/student/enrollments/${enrollment.id}/modules/${module.id}/video`);setVideo(res.url);})}>Abrir video</button>
 <button data-testid={`complete-${module.id}`} className={button} disabled={busy||!unlocked||enrollment.completedModules.includes(module.id)} onClick={()=>void action(()=>updateModuleProgress(enrollment.id,module.id))}>{enrollment.completedModules.includes(module.id)?'Módulo completado':'Marcar módulo como completado'}</button></div>
 {module.documents.map(doc=><div key={doc.id} className="flex justify-between gap-3 border-t border-line pt-3"><span>{doc.title} · {doc.type}</span><button className={secondary} onClick={()=>setDocumentId(doc.id)}>Ver material</button></div>)}
 </>:<p>No hay módulos configurados.</p>}</Panel>
 <Panel><h2 className="text-lg font-bold">Evaluación</h2><p className="text-muted">Tienes 10 minutos y un máximo de dos intentos. La nota mínima para aprobar es 13/20.</p>
 {approved?<><Notice>Curso aprobado: {enrollment.finalScore}/20.</Notice><button className={button} onClick={()=>setActiveTab('mis-certificados')}>Ver certificado</button></>:exhausted?<Notice error>Agotaste los dos intentos. No se emitió un certificado.</Notice>:waiting?<Notice>Reevaluación disponible el {new Date(enrollment.secondAttemptAvailableAt!).toLocaleString()}.</Notice>:<button data-testid="start-exam" className={button} disabled={enrollment.progressPercent<100||busy} onClick={()=>setExam(true)}>Rendir evaluación (10 minutos)</button>}
 {enrollment.progressPercent<100&&<p className="text-sm text-muted">Completa los módulos para habilitar la evaluación.</p>}</Panel></div>
 <Panel><h2 className="text-lg font-bold">Módulos del curso</h2>{course.modules.map((m,i)=><button key={m.id} className={`${m.id===active?button:secondary} w-full text-left justify-start`} onClick={()=>setActive(m.id)}>{i+1}. {m.title}{enrollment.completedModules.includes(m.id)?' ✓':''}</button>)}</Panel>
 </div>{documentId&&<ProtectedDocumentViewer enrollmentId={enrollment.id} documentId={documentId} onClose={()=>setDocumentId('')}/ >}{exam&&<ExamModal enrollment={enrollment} courseTitle={course.title} onClose={()=>setExam(false)}/>}</div>;
}
