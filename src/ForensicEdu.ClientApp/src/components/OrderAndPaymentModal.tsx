import {useRef,useState} from 'react';
import {useApp} from '../context/AppContext';
import {useAuth} from '../context/AuthContext';
import type {Course,EventItem,AdvisoryItem,ProductType,Payment} from '../types';
import {input,button,secondary,Notice,message} from './PlatformUi';
export function OrderAndPaymentModal({item,productType,onClose}:{item:Course|EventItem|AdvisoryItem;productType:ProductType;onClose:()=>void}){
 const {user}=useAuth();const {checkout,uploadVoucher,setActiveTab}=useApp();const [busy,setBusy]=useState(false);const flight=useRef(false);
 const [error,setError]=useState('');const [saved,setSaved]=useState(false);const [order,setOrder]=useState<{orderId:string;orderCode:string}|null>(null);
 const [bank,setBank]=useState<Payment['banco']>('BCP');const [operation,setOperation]=useState('');const [date,setDate]=useState(new Date().toISOString().slice(0,10));const [holder,setHolder]=useState(user?`${user.nombres} ${user.apellidos}`:'');const [file,setFile]=useState<File|null>(null);
 const submit=async(e:React.FormEvent)=>{e.preventDefault();if(flight.current)return;flight.current=true;setBusy(true);setError('');
  try {if(!file)throw new Error('Adjunta el comprobante bancario.');if(file.size>5*1024*1024)throw new Error('El comprobante debe pesar hasta 5 MB.');
   const result=order||await checkout(productType,item.id,{banco:bank,numeroOperacion:operation,fechaPago:date,monto:item.price,titularTransferencia:holder});setOrder(result);
   await uploadVoucher(result.orderId,file);setSaved(true);
  }catch(err){setError(message(err));}finally{flight.current=false;setBusy(false);}
 };
 return <div className="fixed inset-0 z-50 overflow-y-auto bg-black/60 p-3 sm:p-6 flex items-start justify-center" onKeyDown={e=>{if(e.key==='Escape'&&!busy)onClose();}}>
 <section role="dialog" aria-modal="true" aria-labelledby="payment-title" className="w-full max-w-xl rounded-2xl bg-panel border border-line p-5 sm:p-7 space-y-5 my-auto">
 <header className="flex justify-between gap-3"><div><h2 id="payment-title" className="text-xl font-bold">Registrar pago bancario</h2><p className="text-muted text-sm">{item.title} · S/ {item.price.toFixed(2)}</p></div><button className={secondary} onClick={onClose} disabled={busy} aria-label="Cerrar pago">Cerrar</button></header>
 {!user?<><Notice error>Inicia sesión para registrar la compra con tu cuenta.</Notice><button className={button} onClick={()=>{onClose();window.dispatchEvent(new CustomEvent('forensicedu:open-auth'));}}>Iniciar sesión</button></>:saved?<>
 <Notice>Recibimos tu pago y comprobante. Tu matrícula se habilitará cuando aprobemos el pago.</Notice><p className="break-all font-mono text-xs">Orden: {order?.orderCode}</p><button className={button} onClick={()=>{setActiveTab('mis-pagos');onClose();}}>Ver mis pagos</button>
 </>:<form onSubmit={submit} className="space-y-4">
 <Notice error>{error}</Notice>{order&&<p className="text-sm text-muted break-all">Orden creada: {order.orderCode}. Si falló la carga, vuelve a adjuntar el comprobante; se usará esta misma orden.</p>}
 <fieldset disabled={busy||!!order} className="grid sm:grid-cols-2 gap-4">
 <label className="text-sm space-y-1">Banco<select className={input} value={bank} onChange={e=>setBank(e.target.value as Payment['banco'])}>{['BCP','BBVA','Interbank','Scotiabank','Banco de la Nación','Yape / Plin'].map(x=><option key={x}>{x}</option>)}</select></label>
 <label className="text-sm space-y-1">Número de operación<input data-testid="payment-operation" className={input} required maxLength={100} value={operation} onChange={e=>setOperation(e.target.value)}/></label>
 <label className="text-sm space-y-1">Fecha del pago<input className={input} required type="date" value={date} max={new Date().toISOString().slice(0,10)} onChange={e=>setDate(e.target.value)}/></label>
 <label className="text-sm space-y-1">Titular de la transferencia<input className={input} required maxLength={200} value={holder} onChange={e=>setHolder(e.target.value)}/></label>
 </fieldset>
 <label className="block text-sm space-y-2">Comprobante JPG, PNG o PDF (hasta 5 MB)<input data-testid="payment-voucher" className={input} type="file" required accept="image/jpeg,image/png,application/pdf" disabled={busy} onChange={e=>setFile(e.target.files?.[0]||null)}/></label>
 <p className="text-sm text-muted">Un pago corresponde a un solo producto. Adjuntar el voucher no supone que el pago ya esté aprobado.</p>
 <button data-testid="submit-payment" className={button+' w-full'} disabled={busy} type="submit">{busy?'Registrando…':order?'Guardar comprobante':'Registrar pago'}</button>
 </form>}
 </section></div>;
}
