import React,{createContext,useCallback,useContext,useEffect,useRef,useState} from 'react';
import QRCode from 'qrcode';
import {useAuth,profile as authProfile} from './AuthContext';
import {apiRequest,getAll,jsonRequest,downloadFile} from '../services/apiTransport';
import * as dto from '../services/platformDtos';
import * as map from '../services/platformMappers';
import type {Course,EventItem,AdvisoryItem,UserProfile,Order,Payment,Enrollment,Certificate,SupportTicket,SocialLinksConfig,AuditLogItem,ProductType,PaymentStatus,CertificateStatus,UserRole} from '../types';

const guest:UserProfile={id:'',nombres:'Invitado',apellidos:'',tipoDocumento:'DNI',numeroDocumento:'',paisResidencia:'',email:'',telefono:'',nivelEducativo:'Licenciatura',institucionEducativa:'',profesion:'',role:'estudiante',createdAt:'',accountStatus:'REGISTRADA',validUntil:'',hasApprovedPurchase:false};
const noAdvisory:AdvisoryItem={id:'',title:'Asesorías',specialist:'',specialty:'',description:'No hay asesorías publicadas.',price:0,whatsappContact:'',bannerUrl:''};
const emptySocials:SocialLinksConfig={facebook:{url:'',enabled:false},instagram:{url:'',enabled:false},youtube:{url:'',enabled:false},tiktok:{url:'',enabled:false},linkedin:{url:'',enabled:false},x:{url:'',enabled:false},whatsapp:{url:'',enabled:false,number:''}};
interface Snapshot {courses:Course[];events:EventItem[];advisory:AdvisoryItem;orders:Order[];payments:Payment[];enrollments:Enrollment[];certificates:Certificate[];tickets:SupportTicket[];allUsers:UserProfile[];auditLogs:AuditLogItem[];socialLinks:SocialLinksConfig;profile:UserProfile|null;}
const blank=():Snapshot=>({courses:[],events:[],advisory:noAdvisory,orders:[],payments:[],enrollments:[],certificates:[],tickets:[],allUsers:[],auditLogs:[],socialLinks:structuredClone(emptySocials),profile:null});
export interface BankInput {banco:Payment['banco'];numeroOperacion:string;fechaPago:string;monto:number;titularTransferencia:string;}
interface AppState extends Snapshot {
 selectedEnrollmentId:string|null;setSelectedEnrollmentId:(id:string|null)=>void;
 currentUser:UserProfile;activeTab:string;setActiveTab:(tab:string)=>void;selectedCourseId:string|null;setSelectedCourseId:(id:string|null)=>void;
 publicVerificationCode:string|null;setPublicVerificationCode:(code:string|null)=>void;isLoading:boolean;dataError:string;clearError:()=>void;reload:()=>Promise<void>;
 checkout:(type:ProductType,id:string,input:BankInput)=>Promise<{orderId:string;orderCode:string}>;
 uploadVoucher:(orderId:string,file:File)=>Promise<void>;reviewPayment:(id:string,status:PaymentStatus,reason?:string)=>Promise<void>;
 updateModuleProgress:(enrollmentId:string,moduleId:string)=>Promise<void>;updateCurrentUserProfile:(data:Partial<UserProfile>)=>Promise<void>;
 updateCertificateStatus:(id:string,status:CertificateStatus,reason?:string)=>Promise<void>;createTicket:(subject:string,category:SupportTicket['category'],message:string)=>Promise<SupportTicket>;
 replyTicket:(id:string,message:string)=>Promise<void>;updateTicketStatus:(id:string,status:SupportTicket['status'])=>Promise<void>;updateSocialLinks:(value:SocialLinksConfig)=>Promise<void>;
 verifyCertificateByCode:(code:string)=>Certificate|undefined;
 createOrder:(type:ProductType,id:string)=>Order;registerPayment:(id:string,data:unknown)=>{success:boolean;isDuplicate:boolean;payment:Payment;message:string};
 switchUserRole:(role:UserRole)=>void;switchUserById:(id:string)=>void;simulateValidityChange:(days:number)=>void;resetAllDataToDefault:()=>void;bypassSecondAttemptWait:(id:string)=>void;resetExamForDemo:(id:string)=>void;addAuditLog:(...args:unknown[])=>void;
}
const Context=createContext<AppState|null>(null);
const sectionFromEmail=()=>{const allowed=['catalogo','mis-cursos','mis-eventos','asesoria','mis-consultas','perfil','notificaciones'];const section=new URLSearchParams(window.location.hash.slice(1)).get('section');return section&&allowed.includes(section)?section:'catalogo';};
const removed=():never=>{throw new Error('Esta función no está disponible.');};
const can=(role:UserRole,...allowed:UserRole[])=>allowed.includes(role);
export function AppProvider({children}:{children:React.ReactNode}) {
 const {user}=useAuth();const [data,setData]=useState<Snapshot>(blank);const [isLoading,setLoading]=useState(false);const [dataError,setError]=useState('');
 const [selectedEnrollmentId,setSelectedEnrollmentId]=useState<string|null>(null);
 const [activeTab,setActiveTab]=useState('catalogo');const [selectedCourseId,setSelectedCourseId]=useState<string|null>(null);const [publicVerificationCode,setPublicVerificationCode]=useState<string|null>(null);const epoch=useRef(0);
 const reload=useCallback(async()=>{
  const version=++epoch.current;setLoading(true);
  try {
   const [publicCourses,eventRows,advisoryRows,socialRows]=await Promise.all([apiRequest<dto.CourseDto[]>('/courses'),getAll<dto.EventDto>('/events'),getAll<dto.AdvisoryDto>('/advisories'),apiRequest<{key:string;value:string}[]>('/settings/social')]);
   const next=blank();next.courses=publicCourses.map(map.course);next.events=eventRows.map(map.event);next.advisory=advisoryRows[0]?map.advisory(advisoryRows[0]):noAdvisory;
   for(const row of socialRows){const key=row.key.toLowerCase() as keyof SocialLinksConfig;if(key in next.socialLinks){next.socialLinks[key].url=row.value;next.socialLinks[key].enabled=!!row.value;if(key==='whatsapp')next.socialLinks.whatsapp.number=row.value.replace(/\D/g,'');}}
   if(user){
    const financial=can(user.role,'superadmin','admin_financiero');const academic=can(user.role,'superadmin','admin_academico');const support=can(user.role,'superadmin','soporte');
    const [me,orderRows,paymentRows,enrollmentRows,certificateRows,ticketRows,accountRows,auditRows]=await Promise.all([
     apiRequest<Parameters<typeof authProfile>[0]>('/auth/me'),apiRequest<dto.OrderDto[]>(`/orders/user/${encodeURIComponent(user.id)}`),
     getAll<dto.PaymentDto>(financial?'/admin/payments':'/student/payments'),academic?getAll<dto.EnrollmentDto>('/admin/enrollments'):apiRequest<dto.EnrollmentDto[]>('/student/enrollments'),
     getAll<dto.CertificateDto>(academic?'/admin/certificates':'/student/certificates'),support?getAll<dto.TicketDto>('/admin/tickets'):apiRequest<dto.TicketDto[]>('/student/tickets'),
     support?getAll<dto.AccountDto>('/admin/users'):Promise.resolve([]),financial||academic?apiRequest<dto.AuditDto[]>('/admin/audit-logs?limit=100'):Promise.resolve([])]);
    next.profile=authProfile(me);next.orders=orderRows.map(x=>map.order(x,next.profile!,next.courses,next.events,next.advisory));next.payments=paymentRows.map(map.payment);next.enrollments=enrollmentRows.map(map.enrollment);
    next.certificates=await Promise.all(certificateRows.map(async x=>{const certificate=map.certificate(x,next.profile!);return {...certificate,qrDataUrl:await QRCode.toDataURL(certificate.verificationUrl)};}));
    next.tickets=ticketRows.map(map.ticket);next.allUsers=accountRows.map(map.account);if(!next.allUsers.some(x=>x.id===user.id))next.allUsers.push(next.profile);next.auditLogs=auditRows.map(map.audit);
    for(const enrollment of next.enrollments.filter(e=>e.userId===user.id && (e.status==='ACTIVA'||e.status==='COMPLETADA'))){
     const classroom=await apiRequest<dto.ClassroomDto>(`/student/enrollments/${enrollment.id}/classroom`);
     const existing=next.courses.find(x=>x.id===enrollment.courseId);
     if(existing)existing.modules=classroom.modules.map(map.courseModule);
     else next.courses.push(map.course({...classroom.course,id:enrollment.courseId,modules:classroom.modules}));
     enrollment.certificateId=certificateRows.find(x=>x.enrollmentId===enrollment.id)?.id;
    }
   }
   if(version===epoch.current){setData(next);setError('');}
  }catch(error){if(version===epoch.current)setError(error instanceof Error?error.message:'No pudimos cargar la información. Inténtalo de nuevo.');}
  finally{if(version===epoch.current)setLoading(false);}
 },[user?.id,user?.role]);
 useEffect(()=>{setData(blank());setSelectedCourseId(null);setSelectedEnrollmentId(null);setActiveTab(sectionFromEmail());void reload();return ()=>{epoch.current++;};},[reload]);
 useEffect(()=>{if(!user)return;const timer=setInterval(()=>{if(!document.hidden)void reload();},15000);return ()=>clearInterval(timer);},[reload,user?.id]);
 useEffect(()=>{const openSection=()=>{if(window.location.hash.startsWith('#section='))setActiveTab(sectionFromEmail());};window.addEventListener('hashchange',openSection);return()=>window.removeEventListener('hashchange',openSection);},[]);
 const currentUser=data.profile||user||guest;
 const run=async<T,>(action:()=>Promise<T>):Promise<T>=>{setError('');try{const result=await action();await reload();return result;}catch(error){setError(error instanceof Error?error.message:'La operación no se confirmó.');throw error;}};
 const requireLogin=()=>{if(!user){window.dispatchEvent(new CustomEvent('forensicedu:open-auth'));throw new Error('Inicia sesión para continuar.');}};
 const reviewPayment=(id:string,status:PaymentStatus,reason?:string)=>run(async()=>{
  if(status==='APROBADO')await apiRequest(`/admin/payments/${id}/approve`,jsonRequest('POST'));
  else {const value=({'EN_REVISIÓN':2,OBSERVADO:6,RECHAZADO:4} as Record<string,number>)[status];if(!value)throw new Error('El estado de pago duplicado se asigna automáticamente.');await apiRequest(`/admin/payments/${id}/review`,jsonRequest('PUT',{status:value,reason:reason||'Revisión administrativa'}));}
 });
 const checkout=(type:ProductType,id:string,input:BankInput)=>run(async()=>{requireLogin();return apiRequest<{orderId:string;orderCode:string}>('/orders/checkout',jsonRequest('POST',{userId:user!.id,productId:id,productType:({curso:1,evento:2,asesoria:3})[type],amount:input.monto,bankName:input.banco,operationNumber:input.numeroOperacion,operationDate:new Date(input.fechaPago+'T00:00:00Z').toISOString(),accountHolderName:input.titularTransferencia}));});
 const uploadVoucher=(orderId:string,file:File)=>run(async()=>{const form=new FormData();form.append('file',file);await apiRequest(`/orders/${orderId}/voucher`,{method:'POST',body:form});});
 const createTicket=(subject:string,category:SupportTicket['category'],message:string)=>run(async()=>{requireLogin();return map.ticket(await apiRequest<dto.TicketDto>('/student/tickets',jsonRequest('POST',{subject,category,message})));});
 const updateProfile=(changes:Partial<UserProfile>)=>run(async()=>{const p={...currentUser,...changes};await apiRequest('/auth/me',jsonRequest('PUT',{nombres:p.nombres,apellidos:p.apellidos,phone:p.telefono,paisResidencia:p.paisResidencia,nivelEducativo:p.nivelEducativo,institucionEducativa:p.institucionEducativa,profesion:p.profesion}));});
 return <Context.Provider value={{...data,selectedEnrollmentId,setSelectedEnrollmentId,currentUser,activeTab,setActiveTab,selectedCourseId,setSelectedCourseId,publicVerificationCode,setPublicVerificationCode,isLoading,dataError,clearError:()=>setError(''),reload,
  checkout,uploadVoucher,reviewPayment,updateCurrentUserProfile:updateProfile,
  updateModuleProgress:(id,module)=>run(async()=>{await apiRequest(`/student/enrollments/${id}/modules/${module}/complete`,jsonRequest('PUT'));}),
  updateCertificateStatus:(id,status,reason)=>run(async()=>{const cert=data.certificates.find(x=>x.id===id);if(!cert)throw new Error('Certificado no encontrado.');if(status!=='REVOCADO')throw new Error('Solo está disponible la revocación administrativa.');await apiRequest(`/admin/certificates/${encodeURIComponent(cert.certificateCode)}/revoke`,jsonRequest('POST',{reason:reason||'Revocación administrativa'}));}),
  createTicket,replyTicket:(id,message)=>run(async()=>{await apiRequest(`/student/tickets/${id}/reply`,jsonRequest('POST',{message}));}),
  updateTicketStatus:(id,status)=>run(async()=>{await apiRequest(`/admin/tickets/${id}/status`,jsonRequest('PUT',{status:({'ABIERTO':1,'EN_ATENCIÓN':2,RESPONDIDO:3,CERRADO:4})[status]}));}),
  updateSocialLinks:(links)=>run(async()=>{for(const [key,value] of Object.entries(links)){const serverKey=({facebook:'Facebook',instagram:'Instagram',youtube:'YouTube',tiktok:'TikTok',linkedin:'LinkedIn',x:'X',whatsapp:'WhatsApp'} as Record<string,string>)[key];await apiRequest(`/settings/${serverKey}`,jsonRequest('PUT',{value:value.enabled?value.url:''}));}}),
  verifyCertificateByCode:code=>data.certificates.find(x=>x.certificateCode===code.trim().toUpperCase()),
  createOrder:removed,registerPayment:removed,switchUserRole:removed,switchUserById:removed,simulateValidityChange:removed,resetAllDataToDefault:removed,bypassSecondAttemptWait:removed,resetExamForDemo:removed,addAuditLog:removed
 }}>{children}</Context.Provider>;
}
export function useApp(){const context=useContext(Context);if(!context)throw new Error('Falta AppProvider');return context;}
