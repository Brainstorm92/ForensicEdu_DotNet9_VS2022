import React, { createContext, useContext, useState } from 'react';
import { API_BASE_URL } from '../services/apiClient';
import type { UserProfile, UserRole } from '../types';

export interface Registration {
  nombres: string; apellidos: string; email: string; dni: string; phone: string; password: string;
  tipoDocumento: string; paisResidencia: string; nivelEducativo: string; institucionEducativa: string;
  profesion: string; acceptTerms: boolean; acceptPrivacy: boolean;
}
interface AccountDto {
  id: string; nombres: string; apellidos: string; fullName: string; email: string; dni: string; phone: string;
  tipoDocumento: UserProfile['tipoDocumento']; paisResidencia: string; nivelEducativo: UserProfile['nivelEducativo'];
  institucionEducativa: string; profesion: string; role: number; status: number; registeredAt: string;
  validUntil: string; hasApprovedPurchase: boolean; lastPurchaseDate?: string;
}

// Tokens and passwords are never persisted in localStorage.
let accessToken: string | null = null;
export function authHeaders(): Record<string, string> { return accessToken ? { Authorization: `Bearer ${accessToken}` } : {}; }
export async function accountRequest(path: string, init: RequestInit = {}) {
  let response: Response;
  const headers = new Headers(init.headers);
  if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`);
  try { response = await fetch(`${API_BASE_URL}${path}`, { ...init, headers }); }
  catch { throw new Error('No se pudo conectar con la API. Compruebe que WebApi esté ejecutándose.'); }
  const data = await response.json().catch(() => null);
  if (!response.ok) throw new Error(data?.message || (response.status === 401 ? 'La sesión no es válida. Inicie sesión de nuevo.' :
    response.status >= 500 ? 'El servidor no pudo procesar la solicitud. Inténtelo nuevamente.' : 'Revise los datos ingresados.'));
  if (!data) throw new Error('La API devolvió una respuesta inesperada. Compruebe la configuración del servidor.');
  return data;
}
const roles: UserRole[] = ['estudiante', 'superadmin', 'admin_academico', 'admin_financiero', 'docente', 'soporte', 'estudiante', 'consultor'];
function profile(dto: AccountDto): UserProfile {
  return { id: dto.id, nombres: dto.nombres || dto.fullName, apellidos: dto.apellidos || '',
    email: dto.email, numeroDocumento: dto.dni, telefono: dto.phone, tipoDocumento: dto.tipoDocumento,
    paisResidencia: dto.paisResidencia, nivelEducativo: dto.nivelEducativo,
    institucionEducativa: dto.institucionEducativa, profesion: dto.profesion,
    role: roles[dto.role] || 'estudiante', createdAt: dto.registeredAt, validUntil: dto.validUntil,
    hasApprovedPurchase: dto.hasApprovedPurchase, lastPurchaseDate: dto.lastPurchaseDate,
    accountStatus: dto.status === 1 ? 'REGISTRADA' : dto.status === 2 ? 'ACTIVA' : dto.status === 6 ? 'PENDIENTE_DE_ELIMINACIÓN' : 'VENCIDA' };
}
interface AuthState {
  user: UserProfile | null;
  register: (data: Registration) => Promise<string>;
  login: (email: string, password: string) => Promise<void>;
  refresh: () => Promise<void>;
  logout: () => void;
}
const AuthContext = createContext<AuthState | null>(null);
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(null);
  const refresh = async () => { setUser(profile(await accountRequest('/auth/me'))); };
  const register = async (data: Registration) => {
    const result = await accountRequest('/auth/register', { method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ...data, fullName: `${data.nombres} ${data.apellidos}` }) });
    return result.userId as string;
  };
  const login = async (email: string, password: string) => {
    accessToken = null; setUser(null);
    const result = await accountRequest('/auth/login', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ email, password }) });
    accessToken = result.token;
    try { await refresh(); } catch (error) { accessToken = null; throw error; }
  };
  const logout = () => { accessToken = null; setUser(null); };
  return <AuthContext.Provider value={{ user, register, login, refresh, logout }}>{children}</AuthContext.Provider>;
}
export function useAuth() {
  const auth = useContext(AuthContext);
  if (!auth) throw new Error('Falta AuthProvider');
  return auth;
}
