import {useDialogViewport} from '../dialogs/useDialogViewport';
import {useDialogScrollLock} from '../dialogs/useDialogScrollLock';
import {useEffect,useRef,useState} from 'react';
import {X,Mail,LockKeyhole} from 'lucide-react';
import {apiRequest,jsonRequest} from '../../services/apiTransport';
import {AuthField} from './AuthField';

export function PasswordRecoveryDialog({token,onClose,onLogin}:{token:string|null;onClose:()=>void;onLogin:()=>void}){
 const [email,setEmail]=useState('');const [password,setPassword]=useState('');const [confirm,setConfirm]=useState('');
 const [busy,setBusy]=useState(false);const [error,setError]=useState('');const [notice,setNotice]=useState('');const [done,setDone]=useState(false);const dialog=useRef<HTMLElement>(null);
 useDialogScrollLock(true);
 useDialogViewport(dialog);
 useEffect(()=>{const previous=document.activeElement as HTMLElement|null;dialog.current?.querySelector<HTMLInputElement>('input')?.focus();return()=>{if(previous?.isConnected)previous.focus();};},[]);
 async function submit(e:React.FormEvent){e.preventDefault();setError('');setNotice('');
  if(token&&(password.length<12||new TextEncoder().encode(password).length>72)){setError('Usa al menos 12 caracteres y como máximo 72 bytes.');return;}
  if(token&&password!==confirm){setError('Las contraseñas no coinciden.');return;}
  setBusy(true);try{const result=await apiRequest<{message:string}>(token?'/auth/reset-password':'/auth/forgot-password',jsonRequest('POST',token?{token,password}:{email:email.trim()}));setNotice(result.message);setPassword('');setConfirm('');if(token)setDone(true);}catch(e){setError(e instanceof Error?e.message:'No se pudo completar la solicitud.');}finally{setBusy(false);}
 }
 return <div className="auth-overlay" onKeyDown={e=>{if(e.key!=='Tab')return;const nodes=[...dialog.current!.querySelectorAll<HTMLElement>('button:not(:disabled),input:not(:disabled)')];const first=nodes[0],last=nodes.at(-1);if(e.shiftKey&&document.activeElement===first){e.preventDefault();last?.focus();}else if(!e.shiftKey&&document.activeElement===last){e.preventDefault();first?.focus();}}}>
 <section ref={dialog} role="dialog" aria-modal="true" aria-labelledby="recovery-title" className="auth-compact-dialog">
 <button type="button" className="auth-close" aria-label="Cerrar recuperación" disabled={busy} onClick={onClose}><X size={20}/></button><div className="auth-compact-content">
 <span className="inline-flex rounded-2xl bg-brand/10 p-3 text-brand mb-4">{token?<LockKeyhole/>:<Mail/>}</span><h1 id="recovery-title" className="text-2xl font-bold">{token?'Crea una nueva contraseña':'Recupera tu contraseña'}</h1>
 <p className="text-sm text-muted mt-2 mb-5">{token?'El enlace vence en 30 minutos y solo puede utilizarse una vez.':'Escribe el correo de tu cuenta. Te enviaremos un enlace para cambiar tu contraseña.'}</p>
 {error&&<p role="alert" className="auth-error mb-4">{error}</p>}{notice&&<p role="status" className="text-sm rounded-xl border border-brand/30 bg-brand/5 p-3 mb-4">{notice}</p>}
 {!done&&<form onSubmit={submit}><fieldset disabled={busy} className="space-y-4">{token?<><AuthField name="newPassword" label="Nueva contraseña" type="password" autoComplete="new-password" value={password} onChange={setPassword} maxLength={72} hint="Usa al menos 12 caracteres."/><AuthField name="confirmNewPassword" label="Confirmar contraseña" type="password" autoComplete="new-password" value={confirm} onChange={setConfirm} maxLength={72}/></>:<label className="block text-sm font-medium space-y-1.5"><span>Correo electrónico</span><input className="auth-input" name="recoveryEmail" type="email" autoComplete="email" required maxLength={200} value={email} onChange={e=>setEmail(e.target.value)}/></label>}
 <button className="auth-primary w-full justify-center" type="submit">{busy?'Procesando…':token?'Guardar nueva contraseña':'Enviar enlace de recuperación'}</button></fieldset></form>}
 <button className="auth-secondary w-full justify-center mt-4" disabled={busy} onClick={onLogin}>Volver a iniciar sesión</button>
 </div></section></div>;
}
