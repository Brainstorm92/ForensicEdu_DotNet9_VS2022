import type {ReactNode} from 'react';
export const input='w-full min-w-0 rounded-xl border border-line bg-panel px-3 py-2.5 text-ink';
export const button='inline-flex items-center justify-center gap-2 rounded-xl bg-brand px-4 py-2.5 font-semibold text-sm disabled:opacity-50';
export const secondary='inline-flex items-center justify-center rounded-xl border border-line bg-surface-low px-3 py-2 text-sm text-ink disabled:opacity-50';
export function Notice({error,children}:{error?:boolean;children:ReactNode}){return children?<div role={error?'alert':'status'} className={`rounded-xl border p-3 text-sm ${error?'border-rose-200 bg-rose-50 text-rose-700':'border-emerald-200 bg-emerald-50 text-emerald-700'}`}>{children}</div>:null;}
export function Panel({children}:{children:ReactNode}){return <section className="min-w-0 rounded-2xl border border-line bg-panel p-5 sm:p-6 space-y-4">{children}</section>;}
export function Empty({children}:{children:ReactNode}){return <Panel><p className="text-muted">{children}</p></Panel>;}
export const message=(error:unknown)=>error instanceof Error?error.message:'La operación no se confirmó.';
