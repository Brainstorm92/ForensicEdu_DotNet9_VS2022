import React, { useState } from 'react';
import { useAuth, type Registration } from '../context/AuthContext';

const initial: Registration = { nombres: '', apellidos: '', email: '', dni: '', phone: '', password: '', tipoDocumento: 'DNI',
  paisResidencia: 'Perú', nivelEducativo: 'Licenciatura', institucionEducativa: '', profesion: '', acceptTerms: false, acceptPrivacy: false };

export function AccountAccess() {
  const { user, register, login, logout, refresh } = useAuth();
  const [mode, setMode] = useState<'login' | 'register' | null>(null);
  const [form, setForm] = useState<Registration>({ ...initial });
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [savedId, setSavedId] = useState('');
  const change = (key: keyof Registration, value: string | boolean) => setForm(prev => ({ ...prev, [key]: value }));
  const open = (next: 'login' | 'register') => { setMode(next); setError(''); setSuccess(''); setSavedId(''); setForm({ ...initial }); };
  const close = () => { if (!busy) { setMode(null); setForm({ ...initial }); setError(''); } };
  async function submit(event: React.FormEvent) {
    event.preventDefault(); setBusy(true); setError(''); setSuccess('');
    try {
      if (mode === 'register') {
        const id = await register(form); setSavedId(id);
        setSuccess('Cuenta creada en la base de datos. Inicie sesión con su correo y contraseña.');
        setForm(prev => ({ ...initial, email: prev.email })); setMode('login');
      } else { await login(form.email, form.password); setForm({ ...initial }); setMode(null); }
    } catch (err) { setError(err instanceof Error ? err.message : 'No fue posible completar la operación.'); }
    finally { setBusy(false); }
  }
  const inputClass = 'w-full rounded-lg border border-slate-600 bg-slate-900 px-3 py-2 text-white focus:outline-amber-400';
  const textField = (key: keyof Registration, label: string, type = 'text', maxLength = 100) =>
    <label className="block text-sm text-slate-200">{label}<input name={key} value={String(form[key])} required type={type}
      maxLength={maxLength} autoComplete={key === 'password' ? (mode === 'register' ? 'new-password' : 'current-password') : key === 'email' ? 'email' : undefined}
      minLength={key === 'password' && mode === 'register' ? 12 : undefined}
      onChange={e => change(key, e.target.value)} className={inputClass} /></label>;
  return <>
    <section className="bg-slate-900 border-b border-amber-500/30 px-4 py-3" aria-label="Cuenta de usuario">
      <div className="max-w-7xl mx-auto flex flex-wrap items-center justify-between gap-3">
        {user ? <div><strong className="text-emerald-300">Sesión iniciada: {user.nombres} {user.apellidos}</strong>
          <p className="text-xs text-slate-300">{user.email} · ID: {user.id}</p>
          <p className="text-xs text-slate-400">Registro y sesión conectados a la base de datos. Las demás secciones conservan funciones de demostración.</p></div>
          : <div><strong className="text-white">Accede con tu cuenta</strong><p className="text-xs text-slate-400">Crea una cuenta o inicia sesión. El selector de usuarios de ejemplo es una demostración independiente.</p></div>}
        <div className="flex gap-2">
          {user ? <><button disabled={busy} className="rounded-lg border border-slate-600 px-3 py-2 text-white" onClick={async () => {
            setBusy(true); setError(''); try { await refresh(); setSuccess('Datos consultados nuevamente en la base de datos.'); }
            catch (e) { setError(e instanceof Error ? e.message : 'No se pudo consultar el perfil.'); } finally { setBusy(false); }
          }}>Verificar mi registro</button><button className="rounded-lg border border-slate-600 px-3 py-2 text-white" onClick={() => { logout(); setSuccess(''); setError(''); }}>Cerrar sesión</button></>
          : <><button className="rounded-lg border border-amber-400 px-3 py-2 text-amber-300" onClick={() => open('login')}>Iniciar sesión</button>
            <button className="rounded-lg bg-amber-400 px-3 py-2 font-semibold text-slate-950" onClick={() => open('register')}>Crear cuenta</button></>}
        </div>
      </div>
      {!mode && (error || success) && <p role="status" className={error ? 'text-rose-300' : 'text-emerald-300'}>{error || success}</p>}
    </section>
    {mode && <div className="fixed inset-0 z-[100] overflow-y-auto bg-black/80 p-4 flex items-start justify-center" onKeyDown={e => { if (e.key === 'Escape') close(); }}>
      <section role="dialog" aria-modal="true" aria-labelledby="account-title" className="w-full max-w-2xl rounded-2xl border border-slate-700 bg-slate-950 p-6 my-6 shadow-2xl">
        <div className="flex justify-between items-center"><h2 id="account-title" className="text-xl font-bold text-white">{mode === 'register' ? 'Crear cuenta' : 'Iniciar sesión'}</h2>
          <button type="button" aria-label="Cerrar formulario" disabled={busy} onClick={close} className="text-slate-300 p-2">Cerrar</button></div>
        {success && <div role="status" className="my-4 rounded-lg bg-emerald-950 p-3 text-emerald-200">{success}{savedId && <p className="text-xs break-all mt-1">ID de registro: {savedId}</p>}</div>}
        {error && <p role="alert" className="my-4 rounded-lg bg-rose-950 p-3 text-rose-200">{error}</p>}
        <form onSubmit={submit} className="mt-4 space-y-4">
          <fieldset disabled={busy} className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {mode === 'register' && <>{textField('nombres', 'Nombres')}{textField('apellidos', 'Apellidos')}
              <label className="text-sm text-slate-200">Tipo de documento<select className={inputClass} value={form.tipoDocumento} onChange={e => change('tipoDocumento', e.target.value)}>
                {['DNI', 'CE', 'Pasaporte'].map(value => <option key={value}>{value}</option>)}</select></label>
              {textField('dni', 'Número de documento', 'text', 20)}{textField('phone', 'Teléfono / WhatsApp', 'tel', 30)}{textField('paisResidencia', 'País de residencia')}
              <label className="text-sm text-slate-200">Nivel educativo<select className={inputClass} value={form.nivelEducativo} onChange={e => change('nivelEducativo', e.target.value)}>
                {['Pregrado', 'Bachiller', 'Licenciatura', 'Maestría', 'Doctorado'].map(value => <option key={value}>{value}</option>)}</select></label>
              {textField('institucionEducativa', 'Institución educativa')}{textField('profesion', 'Profesión')}</>}
            {textField('email', 'Correo electrónico', 'email', 200)}{textField('password', mode === 'register' ? 'Contraseña (12 a 72 caracteres)' : 'Contraseña', 'password', 72)}
            {mode === 'register' && <div className="sm:col-span-2 space-y-3 text-sm text-slate-300">
              <label className="flex gap-2"><input type="checkbox" required checked={form.acceptTerms} onChange={e => change('acceptTerms', e.target.checked)} />Acepto los términos y condiciones de la plataforma.</label>
              <label className="flex gap-2"><input type="checkbox" required checked={form.acceptPrivacy} onChange={e => change('acceptPrivacy', e.target.checked)} />Consiento el tratamiento de mis datos personales para gestionar mi cuenta y servicios educativos.</label>
            </div>}
          </fieldset>
          <button disabled={busy} className="w-full rounded-lg bg-amber-400 px-4 py-3 font-bold text-slate-950 disabled:opacity-50">{busy ? 'Procesando…' : mode === 'register' ? 'Registrar cuenta' : 'Entrar'}</button>
          <button type="button" disabled={busy} className="text-sm text-amber-300" onClick={() => open(mode === 'login' ? 'register' : 'login')}>{mode === 'login' ? 'No tengo cuenta: registrarme' : 'Ya tengo cuenta: iniciar sesión'}</button>
        </form>
      </section>
    </div>}
  </>;
}
