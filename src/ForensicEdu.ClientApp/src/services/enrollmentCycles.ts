import type {Enrollment} from '../types';
export const canReenroll=(e?:Enrollment|null)=>!!e&&(e.canReenroll ??
 ((e.status==='ACTIVA'||e.status==='DESAPROBADA')&&e.examStatus==='DESAPROBADA'&&e.attemptsUsed>=2));
export function currentEnrollment(rows:Enrollment[],userId:string,courseId:string){
 return rows.filter(e=>e.userId===userId&&e.courseId===courseId).sort((a,b)=>
  Number(b.status==='ACTIVA'||b.status==='COMPLETADA')-Number(a.status==='ACTIVA'||a.status==='COMPLETADA')||
  new Date(b.enrollmentDate).getTime()-new Date(a.enrollmentDate).getTime()||b.id.localeCompare(a.id))[0];
}
