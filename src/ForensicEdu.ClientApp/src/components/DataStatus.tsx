import {useApp} from '../context/AppContext';
export function DataStatus(){
 const {dataError,isLoading,reload,clearError}=useApp();
 if(!dataError)return null;
 return <div className="max-w-7xl mx-auto px-4 py-2"><div role="alert" className="p-3 rounded-xl border border-rose-200 bg-rose-50 text-rose-700 flex flex-wrap items-center justify-between gap-3"><span>{dataError}</span><div className="flex gap-3"><button onClick={()=>void reload()} disabled={isLoading} className="border border-line rounded-lg px-3 py-2">{isLoading?'Reintentando…':'Reintentar'}</button><button onClick={clearError} aria-label="Cerrar aviso">Cerrar</button></div></div></div>;
}
