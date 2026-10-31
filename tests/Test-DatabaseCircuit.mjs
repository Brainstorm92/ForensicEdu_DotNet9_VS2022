import {spawn,spawnSync} from 'node:child_process';
import {createServer} from 'node:net';
import {mkdir,mkdtemp} from 'node:fs/promises';
import {tmpdir} from 'node:os';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {randomBytes} from 'node:crypto';

const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const dll=process.env.API_TEST_DLL;
if(!dll)throw new Error('Defina API_TEST_DLL con la API compilada.');
const bin=process.env.FORENSIC_TEST_PG_BIN??'C:/Program Files/PostgreSQL/17/bin';
const directory=await mkdtemp(path.join(tmpdir(),'forensicedu-circuit-'));
const data=path.join(directory,'data');const work=path.join(directory,'api');await mkdir(work);
const run=(file,args)=>{
  const result=spawnSync(path.join(bin,file+'.exe'),args,{encoding:'utf8',windowsHide:true,timeout:30000,...(file==='pg_ctl'?{stdio:'ignore'}:{})});
  if(result.status!==0)throw new Error(file+': '+result.stdout+' '+result.stderr);
};
async function freePort(){const s=createServer();await new Promise(r=>s.listen(0,'127.0.0.1',r));const p=s.address().port;await new Promise(r=>s.close(r));return p;}
const pgPort=await freePort();const apiPort=await freePort();
run('initdb',['-D',data,'--encoding=UTF8','--locale=C','--auth=trust','--username=postgres']);
let postgres=false;let server;let logs='';
function startPostgres(){run('pg_ctl',['-D',data,'-l',path.join(directory,'server.log'),'-o',`-h 127.0.0.1 -p ${pgPort}`,'-w','start']);postgres=true;}
function stopPostgres(){run('pg_ctl',['-D',data,'-m','fast','-w','stop']);postgres=false;}
try {
  startPostgres();
  run('createdb',['-h','127.0.0.1','-p',String(pgPort),'-U','postgres','forensicedu_test_circuit']);
  run('psql',['-h','127.0.0.1','-p',String(pgPort),'-U','postgres','-d','forensicedu_test_circuit','-v','ON_ERROR_STOP=1','-f',path.join(root,'database-postgresql.sql')]);
  server=spawn('dotnet',[dll,'--contentRoot',work],{cwd:work,windowsHide:true,stdio:['ignore','pipe','pipe'],env:{
    ...process.env,ASPNETCORE_ENVIRONMENT:'Production',DOTNET_ENVIRONMENT:'Production',ASPNETCORE_URLS:`http://127.0.0.1:${apiPort}`,
    DatabaseProvider:'PostgreSQL',ConnectionStrings__DefaultConnection:`Host=127.0.0.1;Port=${pgPort};Database=forensicedu_test_circuit;Username=postgres;Pooling=false;Timeout=1`,
    JwtSettings__Secret:randomBytes(64).toString('base64'),JwtSettings__Issuer:'test',JwtSettings__Audience:'test',
    Resilience__PostgreSQL__MinimumThroughput:'2',Resilience__PostgreSQL__FailureRatio:'0.5',
    Resilience__PostgreSQL__BreakDurationSeconds:'4',Resilience__PostgreSQL__SamplingDurationSeconds:'30'
  }});
  server.stdout.on('data',b=>logs=(logs+b).slice(-16000));server.stderr.on('data',b=>logs=(logs+b).slice(-16000));
  const origin=`http://127.0.0.1:${apiPort}`;
  let ready=false;
  for(let i=0;i<100;i++){try{await fetch(origin);ready=true;break;}catch{}if(server.exitCode!==null)throw new Error(logs);await new Promise(r=>setTimeout(r,100));}
  if(!ready)throw new Error('API no inició: '+logs);
  const healthy=await fetch(origin+'/api/courses');if(healthy.status!==200)throw new Error('No se pudo consultar PostgreSQL');
  console.log('OK: consulta HTTP con PostgreSQL real disponible');
  stopPostgres();
  let opened=false;
  for(let i=0;i<4;i++){
    const response=await fetch(origin+'/api/courses');
    const after=Number(response.headers.get('retry-after'));
    if(response.status!==503)throw new Error('Se esperaba 503 con PostgreSQL detenido: '+response.status);
    if(after>0 && after<=4){opened=true;break;}
  }
  if(!opened)throw new Error('El circuito no abrió: '+logs);
  console.log('OK: caída real de PostgreSQL abre circuito y devuelve 503 con Retry-After');
  const fast=await fetch(origin+'/api/courses');
  if(fast.status!==503 || Number(fast.headers.get('retry-after'))>4)throw new Error('El circuito abierto no rechaza consultas');
  console.log('OK: circuito abierto rechaza nuevas consultas');
  startPostgres();
  await new Promise(r=>setTimeout(r,4200));
  const recovered=await fetch(origin+'/api/courses');
  if(recovered.status!==200)throw new Error('El circuito no se recuperó: '+recovered.status+' '+logs);
  const closed=await fetch(origin+'/api/courses');if(closed.status!==200)throw new Error('El circuito no volvió al estado cerrado');
  console.log('OK: reinicio real de PostgreSQL recupera la API y cierra circuito');
  console.log('PASS: circuit breaker HTTP y PostgreSQL real, sin utilizar Contabo.');
} finally {
  if(server)server.kill();
  if(postgres)stopPostgres();
}
