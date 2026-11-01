import React, { createContext, useContext, useState, useEffect } from 'react';
import { apiRequest, setAccessToken } from '../services/apiTransport';
export { authHeaders } from '../services/apiTransport';
import type { UserProfile, UserRole } from '../types';

export interface Registration {
  nombres: string; apellidos: string; email: string; dni: string; phone: string; password: string;
  tipoDocumento: string; paisResidencia: string; nivelEducativo: string; institucionEducativa: string;
  profesion: string; birthDate: string; acceptTerms: boolean; acceptPrivacy: boolean;
}
export interface RegistrationResult {userId:string;message:string;confirmationRequired:boolean;emailSent:boolean}
interface AccountDto {
  id: string; nombres: string; apellidos: string; fullName: string; email: string; dni: string; phone: string; birthDate?:string|null;
  tipoDocumento: UserProfile['tipoDocumento']; paisResidencia: string; nivelEducativo: UserProfile['nivelEducativo'];
  institucionEducativa: string; profesion: string; role: number; status: number; registeredAt: string;
  validUntil: string; hasApprovedPurchase: boolean; lastPurchaseDate?: string;
}

export const accountRequest = apiRequest<any>;
const roles: UserRole[] = ['estudiante', 'superadmin', 'admin_academico', 'admin_financiero', 'docente', 'soporte', 'estudiante', 'consultor'];
export function profile(dto: AccountDto): UserProfile {
  return { id: dto.id, nombres: dto.nombres || dto.fullName, apellidos: dto.apellidos || '',
    birthDate: dto.birthDate||undefined, email: dto.email, numeroDocumento: dto.dni, telefono: dto.phone, tipoDocumento: dto.tipoDocumento,
    paisResidencia: dto.paisResidencia, nivelEducativo: dto.nivelEducativo,
    institucionEducativa: dto.institucionEducativa, profesion: dto.profesion,
    role: roles[dto.role] || 'estudiante', createdAt: dto.registeredAt, validUntil: dto.validUntil || '',
    hasApprovedPurchase: dto.hasApprovedPurchase, lastPurchaseDate: dto.lastPurchaseDate,
    accountStatus: (['REGISTRADA','REGISTRADA','ACTIVA','PRÓXIMA_A_VENCER','VENCIDA','SUSPENDIDA','PENDIENTE_DE_ELIMINACIÓN','ELIMINADA'] as const)[Number(dto.status)]||'VENCIDA' };
}
export interface GoogleAuthResult {token?:string;requiresRegistration:boolean;requiresLinkPassword:boolean;email:string;nombres:string;apellidos:string;message:string;}
interface AuthState {
  user: UserProfile | null;
  register: (data: Registration) => Promise<RegistrationResult>;
  login: (email: string, password: string) => Promise<void>;
  googleLogin:(credential:string,profile?:Omit<Registration,'email'|'password'>,linkPassword?:string)=>Promise<GoogleAuthResult>;
  refresh: () => Promise<void>;
  logout: () => void;
}
const AuthContext = createContext<AuthState | null>(null);
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(null);
  useEffect(()=>{const expired=()=>setUser(null);window.addEventListener('forensicedu:session-expired',expired);return ()=>window.removeEventListener('forensicedu:session-expired',expired);},[]);
  const refresh = async () => { setUser(profile(await accountRequest('/auth/me'))); };
  const register = async (data: Registration) => {
    const result = await accountRequest('/auth/register', { method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ...data, fullName: `${data.nombres} ${data.apellidos}` }) });
    return result as RegistrationResult;
  };
  const login = async (email: string, password: string) => {
    setAccessToken(null); setUser(null);
    const result = await accountRequest('/auth/login', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ email, password }) });
    setAccessToken(result.token);
    try { await refresh(); } catch (error) { setAccessToken(null); throw error; }
  };
  const googleLogin=async(credential:string,profile?:Omit<Registration,'email'|'password'>,linkPassword?:string)=>{
    const result=await apiRequest<GoogleAuthResult>('/auth/google',{method:'POST',credentials:'include',headers:{'Content-Type':'application/json'},body:JSON.stringify({credential,profile,linkPassword})});
    if(result.token){setAccessToken(result.token);try{await refresh();}catch(error){setAccessToken(null);setUser(null);throw error;}}return result;
  };
  const logout = () => { setAccessToken(null); setUser(null); };
  return <AuthContext.Provider value={{ user, register, login, googleLogin, refresh, logout }}>{children}</AuthContext.Provider>;
}
export function useAuth() {
  const auth = useContext(AuthContext);
  if (!auth) throw new Error('Falta AuthProvider');
  return auth;
}
