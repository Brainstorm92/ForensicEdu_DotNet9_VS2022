import { spawn } from 'node:child_process';
import { createServer } from 'node:net';
import { createHmac, randomBytes } from 'node:crypto';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

// Run against an owned process with fake local PostgreSQL configuration.
// All tested paths finish before opening a database connection.
const docsOnly = process.argv.includes('--docs');
const root = process.argv.slice(2).find(arg => !arg.startsWith('--')) ?? path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const api = path.join(root, 'src/ForensicEdu.WebApi');
const dll = process.env.API_TEST_DLL ?? path.join(api, 'bin/Debug/net9.0/ForensicEdu.WebApi.dll');
const probe = createServer();
await new Promise(resolve => probe.listen(0, '127.0.0.1', resolve));
const port = probe.address().port;
await new Promise(resolve => probe.close(resolve));
const secret = randomBytes(64).toString('base64');
const server = spawn('dotnet', [dll], { cwd: api, windowsHide: true, stdio: ['ignore', 'pipe', 'pipe'], env: {
  ...process.env, ASPNETCORE_ENVIRONMENT: docsOnly ? 'Development' : 'Production', DOTNET_ENVIRONMENT: docsOnly ? 'Development' : 'Production',
  ASPNETCORE_URLS: `http://127.0.0.1:${port}`, DatabaseProvider: 'PostgreSQL',
  ConnectionStrings__DefaultConnection: 'Host=127.0.0.1;Port=9;Database=isolated_architecture_test;Username=test;Password=not-a-real-password;Timeout=1',
  JwtSettings__Secret: secret, JwtSettings__Issuer: 'ForensicEduApi', JwtSettings__Audience: 'ForensicEduApp'
} });
let logs = '';
server.stdout.on('data', b => { logs = (logs+b).slice(-12000); });
server.stderr.on('data', b => { logs = (logs+b).slice(-12000); });
function token(role) {
  const base64 = value => Buffer.from(JSON.stringify(value)).toString('base64url');
  const header = base64({ alg: 'HS256', typ: 'JWT' });
  const payload = base64({ sub: 'test-user', role, iss: 'ForensicEduApi', aud: 'ForensicEduApp', exp: Math.floor(Date.now()/1000)+60 });
  return header+'.'+payload+'.'+createHmac('sha256',secret).update(header+'.'+payload).digest('base64url');
}
async function check(url, expected, { method='GET', body, role }={}) {
  const headers = { 'Content-Type': 'application/json' }; if(role) headers.Authorization='Bearer '+token(role);
  const response = await fetch(`http://127.0.0.1:${port}`+url, { method, headers, body:body===undefined?undefined:JSON.stringify(body) });
  const text = await response.text();
  if(response.status!==expected) throw new Error(`${method} ${url}: expected ${expected}, got ${response.status}: ${text}`);
  if(response.status===400 && !text.includes('errors')) throw new Error('Falta detalle de validación: '+url);
  console.log(`OK: ${method} ${url} -> ${expected}`);
}
try {
  let ready=false;
  for(let i=0;i<150;i++) {
    if(server.exitCode!==null) throw new Error('La API terminó antes de iniciar: '+logs);
    try { await fetch(`http://127.0.0.1:${port}/`); ready=true; break; } catch {}
    await new Promise(resolve=>setTimeout(resolve,100));
  }
  if(!ready) throw new Error('La API no inició: '+logs);
  if (docsOnly) {
    const origin = `http://127.0.0.1:${port}`;
    const home = await fetch(origin+'/', {redirect:'manual'});
    if(home.status!==302 || home.headers.get('location')!=='/scalar/v1') throw new Error('La raíz no redirige a Scalar');
    const docs = await fetch(origin+'/scalar/v1');
    if(docs.status!==200 || !(await docs.text()).toLowerCase().includes('scalar')) throw new Error('Scalar no disponible');
    const schema = await fetch(origin+'/openapi/v1.json');
    if(schema.status!==200 || !Object.keys((await schema.json()).paths).some(p => p.toLowerCase() === '/api/auth/login')) throw new Error('Contrato OpenAPI no disponible');
    const front = await fetch(origin+'/app/');
    if(front.status!==200 || !(await front.text()).includes('id="root"')) throw new Error('Interfaz no disponible en /app/');
    const missing = await fetch(origin+'/api/does-not-exist');
    if(missing.status!==404) throw new Error('Ruta API inexistente no devuelve 404');
    console.log('OK: raíz redirige a Scalar, documentación y OpenAPI disponibles, React en /app/, API inexistente devuelve 404.');
  } else {
  await check('/api/auth/me',401);
  await check('/api/admin/pending-payments',401);
  await check('/api/orders/user/another-user',401);
  await check('/api/auth/login',400,{method:'POST',body:{email:'',password:''}});
  await check('/api/auth/register',400,{method:'POST',body:{fullName:'',email:'invalid',dni:'',phone:'',password:''}});
  await check('/api/certificates/verify/%20',400);
  console.log('Pruebas HTTP correctas. No se accedió a la base remota.');
  }
} finally { server.kill(); }
