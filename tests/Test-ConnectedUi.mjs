import {spawn,spawnSync} from 'node:child_process';
import {createServer} from 'node:net';
import {mkdir,mkdtemp,cp,writeFile,readFile} from 'node:fs/promises';
import {tmpdir} from 'node:os';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {randomBytes,createHash,createHmac} from 'node:crypto';

const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');const dll=process.env.API_TEST_DLL;const pgPort=Number(process.env.FORENSIC_TEST_PG_PORT);
if(!dll||!Number.isInteger(pgPort)||pgPort<1024||pgPort===5432)throw new Error('Defina API_TEST_DLL y FORENSIC_TEST_PG_PORT para PostgreSQL local de prueba.');
const pgBin=process.env.FORENSIC_TEST_PG_BIN??'C:/Program Files/PostgreSQL/17/bin';
const output=process.env.UI_TEST_OUTPUT??path.join(tmpdir(),'forensicedu-ui-results');await mkdir(output,{recursive:true});
const work=await mkdtemp(path.join(tmpdir(),'forensicedu-ui-'));const database='forensicedu_test_ui_'+randomBytes(6).toString('hex');const pgArgs=['-h','127.0.0.1','-p',String(pgPort),'-U','postgres'];
function execute(file,args,options={}){const result=spawnSync(file,args,{windowsHide:true,encoding:'utf8',timeout:30000,...options});if(result.status!==0)throw new Error(path.basename(file)+': '+result.stdout+' '+result.stderr);return result.stdout;}
execute(path.join(pgBin,'createdb.exe'),[...pgArgs,database]);execute(path.join(pgBin,'psql.exe'),[...pgArgs,'-d',database,'-v','ON_ERROR_STOP=1','-f',path.join(root,'database-postgresql.sql')]);
function sql(query){return execute(path.join(pgBin,'psql.exe'),[...pgArgs,'-d',database,'-tAc',query]).trim();}
const probe=createServer();await new Promise(r=>probe.listen(0,'127.0.0.1',r));const port=probe.address().port;await new Promise(r=>probe.close(r));
const origin=`http://127.0.0.1:${port}`;const password='UI-'+randomBytes(12).toString('hex');const studentEmail='student-ui@example.invalid';const adminEmail='admin-ui@example.invalid';
const env={...process.env,ASPNETCORE_ENVIRONMENT:'Production',DOTNET_ENVIRONMENT:'Production',ASPNETCORE_URLS:origin,DatabaseProvider:'PostgreSQL',
 ConnectionStrings__DefaultConnection:`Host=127.0.0.1;Port=${pgPort};Database=${database};Username=postgres`,JwtSettings__Secret:randomBytes(64).toString('base64'),JwtSettings__Issuer:'ForensicEduApi',JwtSettings__Audience:'ForensicEduApp',BootstrapAdmin__Email:adminEmail,BootstrapAdmin__Password:password};
const documentTest=process.env.PRIVATE_DOCUMENT_TEST==='1';
const mediaTest=process.env.MEDIA_TEST_USE_REAL_STORAGE==='1';
if(mediaTest){const config=JSON.parse(await readFile(path.join(root,'src/ForensicEdu.WebApi/appsettings.Local.json'),'utf8'));for(const [key,value] of Object.entries(config.ObjectStorage??{}))env['ObjectStorage__'+key]=value;if(!env.ObjectStorage__SecretKey)throw new Error('Falta la configuración local de ObjectStorage.');}
if(process.env.GOOGLE_TEST_USE_REAL_CLIENT==='1'){const config=JSON.parse(await readFile(path.join(root,'src/ForensicEdu.WebApi/appsettings.Local.json'),'utf8'));env.Authentication__Google__Enabled='true';env.Authentication__Google__ClientId=config.Authentication.Google.ClientId;}
execute('dotnet',[dll,'--bootstrap-admin','--contentRoot',work],{env,cwd:work});
execute('dotnet',[dll,'--provision-admin','--contentRoot',work],{env:{...env,AdminProvision__Username:'BALISTIC',AdminProvision__Password:password},cwd:work});
const seedEnv={...env,ASPNETCORE_ENVIRONMENT:'Development',DOTNET_ENVIRONMENT:'Development'};
execute('dotnet',[dll,'--seed-demo','--contentRoot',work],{env:seedEnv,cwd:work});execute('dotnet',[dll,'--seed-demo','--contentRoot',work],{env:seedEnv,cwd:work});
if(sql('SELECT count(*) FROM "Courses"')!=='2')throw new Error('El seed duplicó cursos');
await cp(path.join(root,'src/ForensicEdu.ClientApp/dist'),path.join(work,'wwwroot'),{recursive:true});
const voucher=path.join(work,'voucher.pdf');await writeFile(voucher,'%PDF-1.4\nCOMPROBANTE DE PRUEBA UI\n');
const server=spawn('dotnet',[dll,'--contentRoot',work],{env,cwd:work,windowsHide:true,stdio:['ignore','pipe','pipe']});let serverLogs='';server.stdout.on('data',b=>serverLogs=(serverLogs+b).slice(-12000));server.stderr.on('data',b=>serverLogs=(serverLogs+b).slice(-12000));
const imageFile=path.join(work,'portada.png');await writeFile(imageFile,Buffer.from('iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAAKklEQVR4nGNg2LKAtmjUglELRi0YtWDUglELRi0YtWDUglELRi0YKhYAAG8sUEx09ES/AAAAAElFTkSuQmCC','base64'));
let chrome,ws;const errors=[];
try {
 let ready=false;for(let i=0;i<150;i++){try{const r=await fetch(origin+'/api/courses');if(r.ok){ready=true;break;}}catch{}await new Promise(r=>setTimeout(r,100));}if(!ready)throw new Error('API no inició: '+serverLogs);
 chrome=spawn(process.env.CHROME_BIN??'C:/Program Files/Google/Chrome/Application/chrome.exe',['--headless=new','--disable-gpu','--no-first-run','--no-default-browser-check','--remote-debugging-port=9229','--user-data-dir='+path.join(work,'chrome'),'about:blank'],{windowsHide:true,stdio:'ignore'});
 let target;for(let i=0;i<100;i++){try{target=(await(await fetch('http://127.0.0.1:9229/json/list')).json()).find(x=>x.type==='page');if(target)break;}catch{}await new Promise(r=>setTimeout(r,100));}if(!target)throw new Error('Chrome no inició');
 ws=new WebSocket(target.webSocketDebuggerUrl);await new Promise((resolve,reject)=>{ws.onopen=resolve;ws.onerror=reject;});let sequence=0;const pending=new Map();
 ws.onmessage=e=>{const item=JSON.parse(e.data);if(item.id){const p=pending.get(item.id);pending.delete(item.id);if(item.error)p.reject(new Error(JSON.stringify(item.error)));else p.resolve(item.result);}if(item.method==='Runtime.exceptionThrown')errors.push(item.params.exceptionDetails.text);};
 const send=(method,params={})=>new Promise((resolve,reject)=>{const id=++sequence;pending.set(id,{resolve,reject});ws.send(JSON.stringify({id,method,params}));});
 const evaluate=async expression=>{const result=await send('Runtime.evaluate',{expression,returnByValue:true,awaitPromise:true});if(result.exceptionDetails)throw new Error(JSON.stringify(result.exceptionDetails));return result.result.value;};
 const wait=async expression=>{for(let i=0;i<180;i++){if(await evaluate(expression))return;await new Promise(r=>setTimeout(r,100));}throw new Error('Timeout: '+expression+'\n'+await evaluate('document.body.innerText')+'\n'+serverLogs);};
 const click=async text=>{const result=await evaluate(`(()=>{const b=[...document.querySelectorAll('button')].find(b=>b.getBoundingClientRect().width>0 && (b.textContent.trim()===${JSON.stringify(text)}||(${JSON.stringify(text)}==='Panel Admin'&&b.textContent.trim().startsWith('Panel Admin'))));if(!b)return false;b.click();return true;})()`);if(!result)throw new Error('Botón no encontrado: '+text);};
 const field=async(selector,value)=>{await evaluate(`(()=>{const el=document.querySelector(${JSON.stringify(selector)});if(!el)throw new Error('Campo no encontrado: '+${JSON.stringify(selector)});el.focus();})()`);await evaluate(`document.querySelector(${JSON.stringify(selector)}).select()`);await send('Input.insertText',{text:value});await wait(`document.querySelector(${JSON.stringify(selector)})?.value===${JSON.stringify(value)}`);await new Promise(resolve=>setTimeout(resolve,60));};
 const screenshot=async name=>writeFile(path.join(output,name+'.png'),Buffer.from((await send('Page.captureScreenshot',{format:'png'})).data,'base64'));
 await send('Page.enable');await send('Runtime.enable');await send('Network.enable');await send('Emulation.setDeviceMetricsOverride',{width:1440,height:1000,deviceScaleFactor:1,mobile:false});
 await send('Page.navigate',{url:origin+'/app/'});await wait('!!document.querySelector(\'[data-testid="buy-seed-course-balistica"]\')');
 await evaluate(`localStorage.setItem('forense_orders',JSON.stringify([{id:'fake-local-order'}]));localStorage.setItem('forense_enrollments',JSON.stringify([{id:'fake-local-enrollment'}]));`);
 await click('Crear cuenta');await wait('!!document.querySelector(\'input[name="nombres"]\')');
 for(const [name,value] of Object.entries({nombres:'Integración',apellidos:'Navegador',email:studentEmail,password,confirmPassword:password}))await field(`input[name="${name}"]`,value);
 await screenshot('registro-paso-cuenta');if(process.env.GOOGLE_TEST_USE_REAL_CLIENT==='1')await wait(`!!document.querySelector('[data-testid="google-signin"] iframe')`);await click('Continuar');
 for(const [name,value] of Object.entries({dni:'90000001',phone:'999888777'}))await field(`input[name="${name}"]`,value);
 await click('Continuar');
 for(const [name,value] of Object.entries({institucionEducativa:'Instituto de prueba',profesion:'Perito'}))await field(`input[name="${name}"]`,value);
 await evaluate(`document.querySelectorAll('[role="dialog"] input[type="checkbox"]').forEach(x=>x.click())`);await click('Registrar cuenta');
 await wait(`document.body.innerText.includes('Cuenta creada. Inicia sesión')`);await field('input[name="password"]',password);await screenshot('inicio-sesion');await click('Entrar');await wait(`document.body.innerText.includes('Sesión iniciada:') && !document.querySelector('[role="dialog"]')`);
 if(sql(`SELECT count(*) FROM "Users" WHERE "Email"='${studentEmail}'`)!=='1')throw new Error('El registro UI no persistió');
 await wait('!!document.querySelector(\'[data-testid="buy-seed-course-balistica"]\')');await evaluate(`document.querySelector('[data-testid="buy-seed-course-balistica"]').click()`);await wait('!!document.querySelector(\'[data-testid="payment-operation"]\')');
 await field('[data-testid="payment-operation"]','UI-OP-001');let doc=await send('DOM.getDocument');let file=await send('DOM.querySelector',{nodeId:doc.root.nodeId,selector:'[data-testid="payment-voucher"]'});await send('DOM.setFileInputFiles',{nodeId:file.nodeId,files:[voucher]});
 await click('Registrar pago');await wait(`document.body.innerText.includes('Recibimos tu pago y comprobante')`);await click('Ver mis pagos');
 if(sql('SELECT count(*) FROM "Orders"')!=='1'||sql('SELECT count(*) FROM "PaymentEvidences" WHERE "Status"=1')!=='1')throw new Error('El pago UI no persistió');
 if(sql('SELECT count(*) FROM "Enrollments"')!=='0')throw new Error('Se habilitó una matrícula sin aprobación');
 await screenshot('pago-pendiente');console.log('OK: registro y pago desde la interfaz persistidos; sin matrícula antes de aprobación.');
 const paymentId=sql('SELECT "Id" FROM "PaymentEvidences"');
 const login=async email=>{await click('Cerrar sesión');await wait(`document.body.innerText.includes('Accede con tu cuenta')`);await click('Iniciar sesión');await field('input[name="email"]',email);await field('input[name="password"]',password);await click('Entrar');await wait(`!document.querySelector('[role="dialog"]') && document.body.innerText.includes('Sesión iniciada:')`);};
 await login('BALISTIC');await wait(`!![...document.querySelectorAll('button')].find(b=>b.textContent.trim().startsWith('Panel Admin'))`);await click('Panel Admin');await wait(`!!document.querySelector('[data-testid="approve-${paymentId}"]')`);await evaluate(`document.querySelector('[data-testid="approve-${paymentId}"]').click()`);
 await wait(`document.querySelector('[data-payment-id="${paymentId}"]')?.getAttribute('data-status')==='APROBADO'`);
 if(sql('SELECT count(*) FROM "Enrollments"')!=='1')throw new Error('La aprobación UI no creó matrícula');console.log('OK: aprobación administrativa desde la interfaz creó matrícula en PostgreSQL.');


 if(documentTest){
  await click('Cursos y contenidos');await wait(`!![...document.querySelectorAll('h3')].find(h=>h.textContent.includes('Balística básica'))`);
  await evaluate(`(()=>{const h=[...document.querySelectorAll('h3')].find(h=>h.textContent.includes('Balística básica'));[...h.parentElement.querySelectorAll('button')].find(b=>b.textContent==='Editar curso').click();})()`);
  await wait(`!!document.querySelector('[data-testid="course-title"]')`);await click('Módulos y materiales');
  for(const [name,fixture] of [['Guía PDF protegida',process.env.DOCUMENT_PDF_FILE],['Guía Word protegida',process.env.DOCUMENT_DOCX_FILE]]){
   await wait(`!![...document.querySelectorAll('button')].find(b=>b.textContent.trim()==='Subir material privado'&&!b.disabled)`);
   await field('input[aria-label="Nombre del material"]',name);const dom=await send('DOM.getDocument');const input=await send('DOM.querySelector',{nodeId:dom.root.nodeId,selector:'input[aria-label="Archivo del material"]'});
   await send('DOM.setFileInputFiles',{nodeId:input.nodeId,files:[fixture]});await click('Subir material privado');await wait(`document.body.innerText.includes(${JSON.stringify(name)}) && ![...document.querySelectorAll('button')].find(b=>b.textContent.includes('Guardando material'))`);
  }
  if(sql(`SELECT count(*) FROM "CourseDocument" WHERE "StorageUrl" LIKE 'rustfs://documents/%' AND "RequireWatermark"=true`)!=='2')throw new Error('Materiales privados no persistidos');
  await screenshot('materiales-subidos');await click('← Lista de cursos');
  console.log('OK: PDF y DOCX subidos por UI a almacenamiento privado y vinculados al módulo en PostgreSQL.');
 }
 await click('Cursos y contenidos');await click('+ Nuevo curso');
 for(const [key,value] of Object.entries({title:'[PRUEBA UI] Curso creado en administración',slug:'curso-creado-ui',instructor:'Docente UI',shortDescription:'Contenido creado desde el navegador',price:'19',academicHours:'4'}))await field(`[data-testid="course-${key}"]`,value);

 if(mediaTest){
  for(const testId of ['course-image-upload','instructor-image-upload']){let dom=await send('DOM.getDocument');const input=await send('DOM.querySelector',{nodeId:dom.root.nodeId,selector:`[data-testid="${testId}"]`});await send('DOM.setFileInputFiles',{nodeId:input.nodeId,files:[imageFile]});await wait(`document.querySelector('[data-testid="${testId}-preview"]')?.naturalWidth===32`);}
  await wait(`![...document.querySelectorAll('button')].find(b=>b.textContent.trim()==='Guardar curso')?.disabled`);await screenshot('curso-imagenes-rustfs');
 }
 await click('Guardar curso');await wait(`document.body.innerText.includes('Curso creado. Ya puedes agregar')`);
 const newCourse=sql(`SELECT "Id" FROM "Courses" WHERE "Slug"='curso-creado-ui'`);if(!newCourse)throw new Error('El editor no persistió el curso');

 if(mediaTest){
  const saved=JSON.parse(sql(`SELECT json_build_object('image',"ImageUrl",'teacher',"InstructorImageUrl") FROM "Courses" WHERE "Id"='${newCourse}'`));
  for(const url of [saved.image,saved.teacher]){if(!/^\/api\/media\/images\/[a-f0-9]{32}$/.test(url))throw new Error('No se guardó una referencia de imagen estable');const res=await fetch(origin+url);const image=Buffer.from(await res.arrayBuffer());if(res.status!==200||res.headers.get('content-type')!=='image/webp'||image.toString('ascii',8,12)!=='WEBP')throw new Error('No se recupera la imagen desde RustFS');const id=url.split('/').pop();const direct=await fetch(env.ObjectStorage__Endpoint+'/'+env.ObjectStorage__Bucket+'/images/'+id+'.webp');if(direct.status!==403)throw new Error('El objeto no está protegido por el bucket privado');}
  const token=(await(await fetch(origin+'/api/auth/login',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({email:'BALISTIC',password})})).json()).token;
  const upload=async(bytes,type,auth)=>{const form=new FormData();form.append('file',new Blob([bytes],{type}),'archivo.png');return fetch(origin+'/api/admin/media/images',{method:'POST',headers:auth?{Authorization:'Bearer '+auth}:{},body:form});};
  if((await upload('no es una imagen','image/png',token)).status!==400)throw new Error('Se aceptó un PNG falso');
  if((await upload('x','image/png',null)).status!==401)throw new Error('Se permitió subir sin sesión');
  const studentToken=(await(await fetch(origin+'/api/auth/login',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({email:studentEmail,password})})).json()).token;
  if((await upload('x','image/png',studentToken)).status!==403)throw new Error('Un alumno pudo subir imágenes de catálogo');
  const oversized=await upload(new Uint8Array(5500001),'image/png',token);if(![400,413].includes(oversized.status))throw new Error('No se limitó el tamaño del archivo: '+oversized.status);
  if((await fetch(origin+'/api/media/images/'+randomBytes(16).toString('hex'))).status!==404)throw new Error('No se devuelve 404 para imagen inexistente');
  console.log('OK: portada y foto subidas desde navegador a RustFS privado, referencias guardadas en PostgreSQL y lectura pública por la aplicación; archivos falsos, exceso de tamaño y accesos no autorizados rechazados.');
 }
 await click('Módulos y materiales');await field('input[name="title"]','Módulo creado desde UI');await click('Agregar módulo');await wait(`document.body.innerText.includes('Materiales')`);
 if(sql(`SELECT count(*) FROM "CourseModule" WHERE "CourseId"='${newCourse}'`)!=='1')throw new Error('El editor no guardó módulo');
 await click('Banco de preguntas');await click('+ Agregar pregunta');await field('textarea','Pregunta creada desde UI');
 for(let i=0;i<4;i++)await field(`[aria-label="Opción ${i+1}"]`,'Respuesta '+(i+1));await click('Guardar banco de preguntas');await wait(`document.body.innerText.includes('Banco de preguntas guardado.')`);
 if(!sql(`SELECT "QuestionsJson" FROM "Courses" WHERE "Id"='${newCourse}'`).includes('Pregunta creada desde UI'))throw new Error('El banco de preguntas no persistió');
 await click('Gestión de usuarios');await click('+ Nuevo usuario');
 for(const [name,value] of Object.entries({nombres:'Operador',apellidos:'UI',email:'academic-ui@example.invalid'}))await field(`input[name="${name}"]`,value);
 await click('Continuar');await click('Continuar');await field('input[name="password"]',password);await click('Crear usuario');await wait(`document.body.innerText.includes('Usuario creado.')`);
 if(sql(`SELECT "Role" FROM "Users" WHERE "Email"='academic-ui@example.invalid'`)!=='2')throw new Error('El alta no guardó rol académico');
 await click('Roles y permisos');await wait(`document.body.innerText.includes('Siete perfiles institucionales')`);await click('Modo nocturno');await screenshot('administracion-roles');
 await send('Emulation.setDeviceMetricsOverride',{width:390,height:844,deviceScaleFactor:1,mobile:true});await screenshot('administracion-movil');
 if(await evaluate('document.documentElement.scrollWidth>innerWidth+1'))throw new Error('El panel desborda en móvil');
 await send('Emulation.setDeviceMetricsOverride',{width:1440,height:1000,deviceScaleFactor:1,mobile:false});
 console.log('OK: acceso BALISTIC, alta de usuario académico y creación de curso, módulo y preguntas desde UI confirmados con SQL.');
 await login(studentEmail);await click('Mis Cursos y Evaluaciones');await wait('!!document.querySelector(\'[data-testid="classroom-seed-course-balistica"]\')');await evaluate(`document.querySelector('[data-testid="classroom-seed-course-balistica"]').click()`);

 if(documentTest){
  await wait(`document.body.innerText.includes('Guía PDF protegida')`);
  const open=async name=>evaluate(`(()=>{const row=[...document.querySelectorAll('span')].find(s=>s.textContent.startsWith(${JSON.stringify(name)}));row.parentElement.querySelector('button').click();})()`);
  await open('Guía PDF protegida');await wait(`!!document.querySelector('.document-canvas') && document.querySelector('.document-canvas').width>100 && document.querySelector('.document-content').getAttribute('aria-busy')==='false'`);
  await screenshot('lector-pdf-protegido');
  if(await evaluate(`!!document.querySelector('.document-dialog a[download],.document-dialog embed,.document-dialog object')`))throw new Error('Se expuso descarga o visor nativo');
  await send('Emulation.setDeviceMetricsOverride',{width:390,height:844,deviceScaleFactor:1,mobile:true});await screenshot('lector-pdf-movil');
  if(await evaluate(`document.documentElement.scrollWidth>innerWidth+1`))throw new Error('Visor PDF desborda móvil');
  await send('Emulation.setEmulatedMedia',{media:'print'});if(await evaluate(`getComputedStyle(document.querySelector('.document-overlay')).display!=='none'`))throw new Error('No se oculta el lector al imprimir');await send('Emulation.setEmulatedMedia',{media:''});
  await click('Cerrar ✕');await send('Emulation.setDeviceMetricsOverride',{width:1440,height:1000,deviceScaleFactor:1,mobile:false});
  await open('Guía Word protegida');await wait(`!!document.querySelector('iframe.document-word')`);
  if(!await evaluate(`(()=>{const f=document.querySelector('iframe.document-word');return f.getAttribute('sandbox')===''&&f.srcdoc.includes('Navegador')&&f.srcdoc.includes('90000001')&&f.srcdoc.includes('watermark')&&!f.srcdoc.includes('<script>');})()`))throw new Error('Vista Word no aislada o sin marca de agua');
  await screenshot('lector-word-protegido');await click('Cerrar ✕');
  const studentToken=(await(await fetch(origin+'/api/auth/login',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({email:studentEmail,password})})).json()).token;
  const adminToken=(await(await fetch(origin+'/api/auth/login',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({email:'BALISTIC',password})})).json()).token;
  const enrollmentId=sql('SELECT "Id" FROM "Enrollments" LIMIT 1');const docId=sql(`SELECT "Id" FROM "CourseDocument" WHERE "FileType"='PDF' AND "StorageUrl" LIKE 'rustfs://documents/%' LIMIT 1`);
  const route=`/api/student/enrollments/${enrollmentId}/documents/${docId}/content`;
  const read=await fetch(origin+route,{headers:{Authorization:'Bearer '+studentToken}});
  if(read.status!==200||!read.headers.get('cache-control')?.includes('no-store')||read.headers.get('content-disposition')?.includes('attachment'))throw new Error('Lectura PDF o cabeceras inválidas');
  if((await fetch(origin+route)).status!==401)throw new Error('Archivo accesible sin sesión');
  if((await fetch(origin+route,{headers:{Authorization:'Bearer '+adminToken}})).status!==404)throw new Error('Otra cuenta accede a matrícula ajena');
  const key=sql(`SELECT "StorageUrl" FROM "CourseDocument" WHERE "Id"='${docId}'`).replace('rustfs://','');
  if((await fetch(env.ObjectStorage__Endpoint+'/'+env.ObjectStorage__Bucket+'/'+key)).status!==403)throw new Error('Archivo directo en RustFS no es privado');
  const malformed=new FormData();malformed.append('title','Falso');malformed.append('file',new Blob(['no es PDF'],{type:'application/pdf'}),'fake.pdf');
  const uploadRoute='/api/admin/courses/seed-course-balistica/modules/seed-course-balistica-m1/documents/upload';
  if((await fetch(origin+uploadRoute,{method:'POST',headers:{Authorization:'Bearer '+adminToken},body:malformed})).status!==400)throw new Error('PDF falso aceptado');
  if((await fetch(origin+uploadRoute,{method:'POST',headers:{Authorization:'Bearer '+studentToken},body:malformed})).status!==403)throw new Error('Alumno pudo subir materiales');
  sql(`UPDATE "Enrollments" SET "Status"='CANCELADA' WHERE "Id"='${enrollmentId}'`);
  if((await fetch(origin+route,{headers:{Authorization:'Bearer '+studentToken}})).status!==404)throw new Error('Matrícula cancelada sigue leyendo');
  sql(`UPDATE "Enrollments" SET "Status"='ACTIVA' WHERE "Id"='${enrollmentId}'`);
  console.log('OK: lectura PDF en canvas y Word aislado con marca personal; responsive, impresión, anonimato, IDOR, matrícula cancelada y bucket privado verificados.');
 }
 await wait('!!document.querySelector(\'[data-testid="complete-seed-course-balistica-m1"]\')');await evaluate(`document.querySelector('[data-testid="complete-seed-course-balistica-m1"]').click()`);await wait(`document.body.innerText.includes('Progreso: 50%')`);
 await click('2. [PRUEBA] Repaso y preparación de la evaluación');await evaluate(`document.querySelector('[data-testid="complete-seed-course-balistica-m2"]').click()`);await wait(`document.body.innerText.includes('Progreso: 100%')`);
 await evaluate(`document.querySelector('[data-testid="start-exam"]').click()`);await wait('!!document.querySelector(\'[data-question-id]\')');
 const correct={'seed-q1':1,'seed-q2':0,'seed-q3':1};
 for(let i=0;i<3;i++){const id=await evaluate(`document.querySelector('[data-question-id]').getAttribute('data-question-id')`);await evaluate(`document.querySelector('[data-option-index="${correct[id]}"]').click()`);if(i<2)await click('Siguiente');}
 await click('Finalizar evaluación');await wait(`document.querySelector('[data-testid="exam-result"]')?.textContent.includes('20/20')`);await screenshot('examen-persistido');
 if(sql('SELECT count(*) FROM "ExamAttempts" WHERE "Score"=20 AND "Approved"=true')!=='1'||sql('SELECT count(*) FROM "DigitalCertificates"')!=='1')throw new Error('El examen UI no persistió su resultado y certificado');
 await click('Ver mis resultados');await wait(`document.body.innerText.includes('Mis certificados') && document.body.innerText.includes('20/20')`);console.log('OK: progreso, respuestas, nota y certificado desde navegador confirmados con SQL.');
 await send('Page.reload');await wait(`document.body.innerText.includes('Accede con tu cuenta')`);await click('Iniciar sesión');await field('input[name="email"]',studentEmail);await field('input[name="password"]',password);await click('Entrar');await wait(`!document.querySelector('[role="dialog"]') && document.body.innerText.includes('Sesión iniciada:')`);
 await click('Certificados QR');await wait(`document.body.innerText.includes('20/20')`);console.log('OK: la información se recupera tras recargar e iniciar sesión; no se usa localStorage académico.');
 await click('Catálogo');await wait('!!document.querySelector(\'[data-testid="buy-seed-course-escena"]\')');await evaluate(`document.querySelector('[data-testid="buy-seed-course-escena"]').click()`);await wait('!!document.querySelector(\'[data-testid="payment-operation"]\')');await field('[data-testid="payment-operation"]','UI-BLOCKED');
 doc=await send('DOM.getDocument');file=await send('DOM.querySelector',{nodeId:doc.root.nodeId,selector:'[data-testid="payment-voucher"]'});await send('DOM.setFileInputFiles',{nodeId:file.nodeId,files:[voucher]});await send('Network.setBlockedURLs',{urls:['*api/orders/checkout*']});await click('Registrar pago');await wait(`document.body.innerText.includes('No pudimos completar la operación')`);
 if(sql('SELECT count(*) FROM "Orders"')!=='1'||await evaluate(`document.body.innerText.includes('Recibimos tu pago y comprobante')`))throw new Error('Se simuló un éxito con API bloqueada');
 await send('Network.setBlockedURLs',{urls:[]});await click('Cerrar');console.log('OK: fallo de red muestra error y no genera un éxito ni registros locales.');
 await send('Emulation.setDeviceMetricsOverride',{width:390,height:844,deviceScaleFactor:1,mobile:true});await screenshot('movil-conectado');
 await click('Cerrar sesión');await wait(`document.body.innerText.includes('Accede con tu cuenta')`);await click('Iniciar sesión');await screenshot('login-movil-nocturno');if(await evaluate('document.documentElement.scrollWidth>innerWidth+1'))throw new Error('El acceso desborda en móvil');
 if(errors.length)throw new Error('Errores JavaScript: '+errors.join(', '));
 await writeFile(path.join(output,'resultado.json'),JSON.stringify({database,users:sql('SELECT count(*) FROM "Users"'),orders:sql('SELECT count(*) FROM "Orders"'),payments:sql('SELECT count(*) FROM "PaymentEvidences"'),enrollments:sql('SELECT count(*) FROM "Enrollments"'),attempts:sql('SELECT count(*) FROM "ExamAttempts"'),certificates:sql('SELECT count(*) FROM "DigitalCertificates"'),errors},null,2));
 console.log('PASS: recorrido real desde navegador hasta PostgreSQL. Base aislada: '+database);
 await send('Browser.close');
 }finally{
 try { if(documentTest){
  const keys=sql(`SELECT "StorageUrl" FROM "CourseDocument" WHERE "StorageUrl" LIKE 'rustfs://documents/%'`).split(/\r?\n/).map(x=>x.trim()).filter(Boolean).map(x=>x.replace('rustfs://',''));
  for(const key of keys){
   if(!/^documents\/[a-f0-9]{32}\.(pdf|docx)$/.test(key)){console.log('Skipped non-fixture cleanup key');continue;}
   const url=new URL(env.ObjectStorage__Endpoint+'/'+env.ObjectStorage__Bucket+'/'+key);const date=new Date().toISOString().replace(/[:-]|\.\d{3}/g,'');const day=date.slice(0,8),region=env.ObjectStorage__Region||'us-east-1';
   const hash=text=>createHash('sha256').update(text).digest('hex');const hmac=(key,value)=>createHmac('sha256',key).update(value).digest();const payload=hash('');
   const headers=`host:${url.host}\nx-amz-content-sha256:${payload}\nx-amz-date:${date}\n`;const signed='host;x-amz-content-sha256;x-amz-date';const scope=`${day}/${region}/s3/aws4_request`;
   const canonical=`DELETE\n${url.pathname}\n\n${headers}\n${signed}\n${payload}`;const signingKey=hmac(hmac(hmac(hmac('AWS4'+env.ObjectStorage__SecretKey,day),region),'s3'),'aws4_request');
   const signature=createHmac('sha256',signingKey).update(`AWS4-HMAC-SHA256\n${date}\n${scope}\n${hash(canonical)}`).digest('hex');
   const response=await fetch(url,{method:'DELETE',headers:{'x-amz-date':date,'x-amz-content-sha256':payload,Authorization:`AWS4-HMAC-SHA256 Credential=${env.ObjectStorage__AccessKey}/${scope}, SignedHeaders=${signed}, Signature=${signature}`}});
   if(!response.ok)console.log('Fixture cleanup pending HTTP '+response.status);
  }
  console.log('Document fixture cleanup attempted for '+keys.length+' private objects.');
 }
 }catch{console.log('Document fixture cleanup pending.');}
 if(ws)ws.close();if(chrome)chrome.kill();server.kill();
}
