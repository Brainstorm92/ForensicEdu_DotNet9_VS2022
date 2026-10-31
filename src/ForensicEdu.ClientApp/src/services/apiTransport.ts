export const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';
let accessToken: string | null = null;
export function setAccessToken(value: string | null) { accessToken = value; }
export function authHeaders(): Record<string,string> { return accessToken ? {Authorization:`Bearer ${accessToken}`} : {}; }
export class ApiError extends Error { constructor(message: string, public status: number, public fields:Record<string,string[]>={}) { super(message); } }
async function response(path: string, init: RequestInit = {}) {
  const token = accessToken; const headers = new Headers(init.headers);
  if (token) headers.set('Authorization',`Bearer ${token}`);
  let res: Response;
  try { res = await fetch(`${API_BASE_URL}${path}`,{...init,headers}); }
  catch { throw new ApiError('No pudimos completar la operación. Revisa tu conexión e inténtalo de nuevo.',0); }
  if (!res.ok) {
    const data = await res.json().catch(()=>null);
    if(res.status===401 && token && token===accessToken){setAccessToken(null);window.dispatchEvent(new Event('forensicedu:session-expired'));}
    const errors=data?.errors ? Object.values(data.errors).flat().join(' ') : '';
    throw new ApiError(errors || data?.message || data?.title || `No se pudo completar la solicitud (${res.status}).`,res.status,data?.errors||{});
  }
  return res;
}
export async function apiRequest<T=unknown>(path: string, init: RequestInit = {}): Promise<T> {
  const res = await response(path,init);
  if(res.status===204)return undefined as T;
  if(!res.headers.get('content-type')?.includes('json'))throw new ApiError('No pudimos procesar la respuesta. Inténtalo de nuevo.',res.status);
  return res.json() as Promise<T>;
}
export const jsonRequest = (method: string, body?: unknown): RequestInit => ({method,headers:{'Content-Type':'application/json'},...(body===undefined?{}:{body:JSON.stringify(body)})});
export async function downloadFile(path: string, name: string) {
  const res=await response(path); const blob=await res.blob();const url=URL.createObjectURL(blob);
  const a=document.createElement('a');a.href=url;a.download=res.headers.get('content-disposition')?.match(/filename="?([^";]+)"?/i)?.[1]||name;a.click();setTimeout(()=>URL.revokeObjectURL(url),10000);
}
export async function getAll<T>(path: string): Promise<T[]> {
  const items:T[]=[];let page=1;
  for(;;){const result=await apiRequest<{items:T[];total:number}>(`${path}${path.includes('?')?'&':'?'}page=${page}&pageSize=100`);
    items.push(...result.items);if(!result.items.length || items.length>=result.total)return items;page++;}
}

// Authenticated in-app reads. No public URL or persistent object URL is created.
export const readProtectedResponse = (path:string,signal?:AbortSignal) => response(path,{signal,cache:"no-store"});
