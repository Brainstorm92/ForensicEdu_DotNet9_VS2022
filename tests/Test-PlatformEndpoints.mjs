import { spawn, spawnSync } from 'node:child_process';
import { createServer } from 'node:net';
import { randomBytes } from 'node:crypto';
import { mkdir, readFile, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const dll=process.env.API_TEST_DLL;
const pgPort=Number(process.env.FORENSIC_TEST_PG_PORT);
if(!dll || !Number.isInteger(pgPort) || pgPort<1024 || pgPort===5432)throw new Error('Defina API_TEST_DLL y FORENSIC_TEST_PG_PORT para una instancia de prueba local aislada.');
const pgBin=process.env.FORENSIC_TEST_PG_BIN ?? 'C:/Program Files/PostgreSQL/17/bin';
const dbName='forensicedu_test_'+randomBytes(8).toString('hex');
const work=path.join(tmpdir(),dbName);await mkdir(work,{recursive:true});
const pgArgs=['-h','127.0.0.1','-p',String(pgPort),'-U','postgres'];
function execute(file,args,options={}) {
  const result=spawnSync(file,args,{encoding:'utf8',windowsHide:true,...options});
  if(result.status!==0)throw new Error(path.basename(file)+': '+result.stdout+' '+result.stderr);
  return result.stdout;
}
execute(path.join(pgBin,'createdb.exe'),[...pgArgs,dbName]);
execute(path.join(pgBin,'psql.exe'),[...pgArgs,'-d',dbName,'-v','ON_ERROR_STOP=1','-f',path.join(root,'database-postgresql.sql')]);
const probe=createServer();await new Promise(resolve=>probe.listen(0,'127.0.0.1',resolve));const port=probe.address().port;await new Promise(resolve=>probe.close(resolve));
const origin=`http://127.0.0.1:${port}`;
const password='Test-'+randomBytes(16).toString('hex');
const env={...process.env,ASPNETCORE_ENVIRONMENT:'Development',DOTNET_ENVIRONMENT:'Development',ASPNETCORE_URLS:origin,
  DatabaseProvider:'PostgreSQL',ConnectionStrings__DefaultConnection:`Host=127.0.0.1;Port=${pgPort};Database=${dbName};Username=postgres`,
  JwtSettings__Secret:randomBytes(64).toString('base64'),JwtSettings__Issuer:'ForensicEduApi',JwtSettings__Audience:'ForensicEduApp',
  BootstrapAdmin__Email:'admin@example.invalid',BootstrapAdmin__Password:password};
execute('dotnet',[dll,'--bootstrap-admin','--contentRoot',work],{env,cwd:work});
const bootstrapAgain=spawnSync('dotnet',[dll,'--bootstrap-admin','--contentRoot',work],{env,cwd:work,windowsHide:true,encoding:'utf8'});
if(bootstrapAgain.status===0)throw new Error('Bootstrap permitió crear otro administrador');
const server=spawn('dotnet',[dll,'--contentRoot',work],{env,cwd:work,windowsHide:true,stdio:['ignore','pipe','pipe']});
let logs='';server.stdout.on('data',b=>logs=(logs+b).slice(-16000));server.stderr.on('data',b=>logs=(logs+b).slice(-16000));
let count=0;
async function call(method,url,{token,body,status=200,form,raw=false}={}) {
  const headers={};if(token)headers.Authorization='Bearer '+token;if(body!==undefined)headers['Content-Type']='application/json';
  const response=await fetch(origin+url,{method,headers,body:form??(body===undefined?undefined:JSON.stringify(body))});
  const bytes=Buffer.from(await response.arrayBuffer());const text=bytes.toString('utf8');
  if(response.status!==status)throw new Error(`${method} ${url}: ${response.status} en vez de ${status}: ${text}\n${logs}`);
  count++;console.log(`OK ${response.status} ${method} ${url}`);
  if(raw)return bytes;return text?JSON.parse(text):null;
}
const get=(url,token)=>call('GET',url,{token});
const post=(url,body,token,status=200)=>call('POST',url,{token,body,status});
const put=(url,body,token,status=204)=>call('PUT',url,{token,body,status});
const remove=(url,token,status=204)=>call('DELETE',url,{token,status});
const check=(condition,message)=>{if(!condition)throw new Error(message);};
try {
  let ready=false;
  for(let i=0;i<150;i++){try {await fetch(origin+'/scalar/v1');ready=true;break;}catch{} if(server.exitCode!==null)throw new Error(logs);await new Promise(r=>setTimeout(r,100));}
  if(!ready)throw new Error('API no inició: '+logs);
  const schema=await get('/openapi/v1.json');check(schema.components.securitySchemes.Bearer,'Falta Bearer en OpenAPI');
  const declared=JSON.parse(await readFile(path.join(root,'ENDPOINTS_IMPLEMENTADOS.json'),'utf8'));
  for(const endpoint of declared){const base=endpoint.group==='PublicCatalog'?'api':endpoint.group==='AccountManagement'?'api/auth':endpoint.group==='StudentWorkspace'?'api/student':'api/admin';
    const wanted='/'+base+'/'+endpoint.route;const actual=Object.keys(schema.paths).find(p=>p.toLowerCase()===wanted.toLowerCase());
    check(actual && schema.paths[actual][endpoint.verb.toLowerCase()],`Falta endpoint en OpenAPI: ${wanted}`);}
  const admin=(await post('/api/auth/login',{email:env.BootstrapAdmin__Email,password})).token;
  const register=async (email,dni)=>post('/api/auth/register',{fullName:'Ana Prueba',email,dni,phone:'999888777',password,
    nombres:'Ana',apellidos:'Prueba',tipoDocumento:'DNI',paisResidencia:'Perú',nivelEducativo:'Licenciatura',institucionEducativa:'Instituto Test',profesion:'Perito',acceptTerms:true,acceptPrivacy:true});
  const studentAccount=await register('student@example.invalid','10000001');const otherAccount=await register('other@example.invalid','10000002');
  const student=(await post('/api/auth/login',{email:'student@example.invalid',password})).token;
  const other=(await post('/api/auth/login',{email:'other@example.invalid',password})).token;
  await call('GET','/api/admin/courses',{token:student,status:403});await call('GET','/api/admin/users',{status:401});
  await call('GET','/api/admin/courses?page=0',{token:admin,status:400});
  await get('/api/admin/users',admin);await get('/api/admin/users/'+studentAccount.userId,admin);
  await put('/api/admin/users/'+otherAccount.userId+'/role',{role:5},admin);
  const support=(await post('/api/auth/login',{email:'other@example.invalid',password})).token;
  await call('GET','/api/student/tickets',{token:other,status:401});
  await call('POST','/api/admin/courses',{token:support,body:{},status:403});
  await put('/api/admin/users/'+otherAccount.userId+'/status',{status:5},admin);
  await post('/api/auth/login',{email:'other@example.invalid',password},undefined,403);
  await call('GET','/api/student/tickets',{token:support,status:401});
  await put('/api/admin/users/'+otherAccount.userId+'/status',{status:2},admin);
  await put('/api/auth/me',{nombres:'Ana',apellidos:'Actualizada',phone:'999111222',paisResidencia:'Perú',nivelEducativo:'Maestría',institucionEducativa:'Instituto Test',profesion:'Perito'},student);
  check((await get('/api/auth/me',student)).apellidos==='Actualizada','Perfil no persistido');
  const newPassword=password+'!';await put('/api/auth/password',{currentPassword:'incorrect',newPassword},student,400);
  await put('/api/auth/password',{currentPassword:password,newPassword},student);await post('/api/auth/login',{email:'student@example.invalid',password},undefined,401);
  await post('/api/auth/login',{email:'student@example.invalid',password:newPassword});
  const courseInput={title:'Balística de prueba',slug:'balistica-test',category:'Forense',price:100,academicHours:40,instructor:'Docente de prueba',shortDescription:'Curso para integración',isPublished:true};
  const course=await post('/api/admin/courses',courseInput,admin);
  await post('/api/admin/courses',courseInput,admin,409);
  await put('/api/admin/courses/'+course.id,{...courseInput,fullDescription:'Contenido actualizado'},admin,200);
  await get('/api/admin/courses',admin);await get('/api/admin/courses/'+course.id,admin);
  check((await get('/api/courses')).some(c=>c.id===course.id),'Curso no aparece en catálogo');await get('/api/courses/'+course.id);
  const module=await post(`/api/admin/courses/${course.id}/modules`,{title:'Módulo 1',orderIndex:0,videoStreamingUrl:'https://example.invalid/video.m3u8'},admin);
  await put(`/api/admin/courses/${course.id}/modules/${module.id}`,{title:'Módulo actualizado',orderIndex:0,videoStreamingUrl:'https://example.invalid/video.m3u8'},admin,200);
  const doc=await post(`/api/admin/courses/${course.id}/modules/${module.id}/documents`,{title:'Guía',storageUrl:'https://example.invalid/guia.pdf'},admin);
  await put(`/api/admin/courses/${course.id}/modules/${module.id}/documents/${doc.id}`,{title:'Guía actualizada',storageUrl:'https://example.invalid/guia.pdf'},admin,200);
  const tempModule=await post(`/api/admin/courses/${course.id}/modules`,{title:'Temporal',orderIndex:1},admin);
  const tempDoc=await post(`/api/admin/courses/${course.id}/modules/${tempModule.id}/documents`,{title:'Temporal',storageUrl:'https://example.invalid/temp.pdf'},admin);
  await remove(`/api/admin/courses/${course.id}/modules/${tempModule.id}/documents/${tempDoc.id}`,admin);
  await remove(`/api/admin/courses/${course.id}/modules/${tempModule.id}`,admin);
  const bank={questions:[{id:'q1',question:'Pregunta de prueba',options:['Incorrecta','Correcta'],correctOptionIndex:1}]};
  await put(`/api/admin/courses/${course.id}/questions`,bank,admin);await get(`/api/admin/courses/${course.id}/questions`,admin);
  await call('GET',`/api/admin/courses/${course.id}/questions`,{token:student,status:403});
  const eventInput={title:'Evento test',type:'Seminario',description:'Integración',price:50,eventDate:new Date(Date.now()+300000).toISOString(),timeSlot:'14:00 UTC',speaker:'Ponente',roomUrl:'https://example.invalid/sala'};
  const event=await post('/api/admin/events',eventInput,admin);await put('/api/admin/events/'+event.id,eventInput,admin,200);
  await get('/api/admin/events',admin);await get('/api/admin/events/'+event.id,admin);
  check(!(JSON.stringify(await get('/api/events'))).includes('roomUrl'),'Catálogo expone sala');await get('/api/events/'+event.id);
  const advisoryInput={title:'Asesoría test',specialist:'Consultor',specialty:'Forense',description:'Consulta',price:20,whatsappContact:'+51999999999'};
  const advisory=await post('/api/admin/advisories',advisoryInput,admin);await put('/api/admin/advisories/'+advisory.id,advisoryInput,admin,200);
  await get('/api/admin/advisories',admin);await get('/api/admin/advisories/'+advisory.id,admin);
  check(!JSON.stringify(await get('/api/advisories')).includes('whatsappContact'),'Catálogo expone contacto privado');await get('/api/advisories/'+advisory.id);
  const unused=await post('/api/admin/advisories',{...advisoryInput,title:'Sin ventas'},admin);await remove('/api/admin/advisories/'+unused.id,admin);
  const checkout=async(productId,productType,amount,operation)=>post('/api/orders/checkout',{userId:studentAccount.userId,productId,productType,amount,bankName:'BCP',operationNumber:operation,operationDate:new Date(Date.now()-1000).toISOString(),accountHolderName:'Ana Prueba'},student);
  const order=await checkout(course.id,1,100,'OP1');
  let payments=(await get('/api/admin/payments',admin)).items;const payment=payments.find(p=>p.orderId===order.orderId);
  await get('/api/admin/payments/'+payment.id,admin);await put('/api/admin/payments/'+payment.id+'/review',{status:6,reason:'Adjunte voucher'},admin);
  await call('GET','/api/admin/payments/'+payment.id+'/voucher',{token:admin,status:404});
  const upload=async(orderId,suffix)=>{const form=new FormData();form.append('file',new Blob([`%PDF-1.4\n${suffix}`],{type:'application/pdf'}),'voucher.pdf');return call('POST',`/api/orders/${orderId}/voucher`,{token:student,form});};
  await upload(order.orderId,'COURSE');const voucher=await call('GET','/api/admin/payments/'+payment.id+'/voucher',{token:admin,raw:true});check(voucher.subarray(0,5).toString()==='%PDF-','Voucher no persistido');
  await post('/api/admin/payments/'+payment.id+'/approve',undefined,admin);
  await put('/api/admin/payments/'+payment.id+'/review',{status:4,reason:'No debe cambiar'},admin,409);
  await get('/api/admin/pending-payments',admin);await get('/api/orders/user/'+studentAccount.userId,student);await get('/api/student/payments',student);
  const enrollment=(await get('/api/student/enrollments',student)).find(e=>e.courseId===course.id);
  const classroom=await get(`/api/student/enrollments/${enrollment.id}/classroom`,student);check(classroom.modules[0].documents[0].title==='Guía actualizada','Contenido no persistido');
  await call('GET',`/api/student/enrollments/${enrollment.id}/classroom`,{token:support,status:404});
  await get(`/api/student/enrollments/${enrollment.id}/modules/${module.id}/video`,student);
  await get(`/api/student/enrollments/${enrollment.id}/documents/${doc.id}`,student);
  await remove(`/api/admin/courses/${course.id}/modules/${module.id}`,admin,409);
  await put(`/api/student/enrollments/${enrollment.id}/modules/${module.id}/complete`,undefined,student,200);
  const exam=await post(`/api/exam-sessions/${enrollment.id}/start`,undefined,student);
  check(!JSON.stringify(exam.questions).includes('correctOptionIndex'),'Examen expone clave');
  await put('/api/exam-sessions/'+exam.id+'/answers',{q1:1},student);
  const result=await post('/api/exams/submit',{enrollmentId:enrollment.id,userId:studentAccount.userId,selectedAnswers:{q1:1},durationSeconds:10},student);
  check(result.approved && result.score===20,'Examen no calificó correctamente');
  await get(`/api/student/enrollments/${enrollment.id}/attempts`,student);const certs=await get('/api/student/certificates',student);check(certs.total===1,'Certificado no persistido');
  const code=certs.items[0].certificateCode;check((await get('/api/certificates/verify/'+code)).isValid,'Certificado no verifica');
  await call('GET','/api/certificates/download/'+code+'/pdf',{status:401});
  await call('GET','/api/certificates/download/'+code+'/pdf',{token:support,status:403});
  const pdf=await call('GET','/api/certificates/download/'+code+'/pdf',{token:student,raw:true});check(pdf.subarray(0,5).toString()==='%PDF-','Certificado no es PDF');
  await post('/api/admin/certificates/'+code+'/revoke',{reason:'Prueba de revocación'},admin);
  check(!(await get('/api/certificates/verify/'+code)).isValid,'Revocación no persistida');
  await call('GET','/api/certificates/download/'+code+'/pdf',{token:student,status:409});
  for(const [product,type,amount,operation] of [[event,2,50,'OP2'],[advisory,3,20,'OP3']]) {
    const sale=await checkout(product.id,type,amount,operation);await upload(sale.orderId,operation);
    const pay=(await get('/api/admin/payments',admin)).items.find(p=>p.orderId===sale.orderId);await post('/api/admin/payments/'+pay.id+'/approve',undefined,admin);
  }
  await get('/api/student/events',student);await get('/api/student/events/'+event.id+'/access',student);await get('/api/student/advisories',student);
  await remove('/api/admin/advisories/'+advisory.id,admin,409);await remove('/api/admin/events/'+event.id,admin);
  await call('GET','/api/student/events/'+event.id+'/access',{token:student,status:409});
  const ticket=await post('/api/student/tickets',{subject:'Consulta',category:'General',message:'Consulta de prueba'},student);
  await get('/api/admin/tickets',support);await get('/api/admin/tickets/'+ticket.id,support);
  await post('/api/student/tickets/'+ticket.id+'/reply',{message:'Respuesta de soporte'},support);
  await put('/api/admin/tickets/'+ticket.id+'/status',{status:4},support);await get('/api/student/tickets',student);
  await get('/api/admin/enrollments',admin);await get('/api/admin/exam-attempts',admin);await get('/api/admin/certificates',admin);
  const report=await get('/api/admin/reports/summary',admin);check(report.approvedAmount===170,'Reporte financiero incorrecto');
  await get('/api/admin/audit-logs',admin);await put('/api/settings/ExamRetakeWaitHours',{value:'24'},admin);
  await remove('/api/admin/courses/'+course.id,admin);await call('GET','/api/courses/'+course.id,{status:404});
  const persisted=execute(path.join(pgBin,'psql.exe'),[...pgArgs,'-d',dbName,'-tAc','SELECT count(*) FROM "PaymentEvidences" WHERE "Status"=3']);
  check(Number(persisted.trim())===3,'La verificación SQL no confirmó los pagos');
  console.log(`PASS: ${count} solicitudes HTTP verificadas con PostgreSQL real local. Base: ${dbName}. Contabo no fue utilizado.`);
} finally { server.kill(); }
