import React, { useState } from 'react';
import { useApp } from '../context/AppContext';
import { 
  X, 
  CreditCard, 
  Upload, 
  ShieldCheck, 
  AlertTriangle, 
  CheckCircle2, 
  FileText, 
  Building2, 
  Calendar, 
  User, 
  DollarSign,
  ArrowRight,
  ShieldAlert
} from 'lucide-react';
import { Course, EventItem, AdvisoryItem, ProductType } from '../types';

interface OrderAndPaymentModalProps {
  item: Course | EventItem | AdvisoryItem;
  productType: ProductType;
  onClose: () => void;
}

export const OrderAndPaymentModal: React.FC<OrderAndPaymentModalProps> = ({ item, productType, onClose }) => {
  const { createOrder, registerPayment, reviewPayment, setActiveTab } = useApp();

  const [step, setStep] = useState<'order' | 'payment' | 'result'>('order');
  const [createdOrderCode, setCreatedOrderCode] = useState<string>('');
  const [createdOrderId, setCreatedOrderId] = useState<string>('');

  // Payment Form States
  const [banco, setBanco] = useState<'BCP' | 'BBVA' | 'Interbank' | 'Scotiabank' | 'Banco de la Nación' | 'Yape / Plin'>('BCP');
  const [numeroOperacion, setNumeroOperacion] = useState('');
  const [fechaPago, setFechaPago] = useState(new Date().toISOString().split('T')[0]);
  const [monto, setMonto] = useState(item.price);
  const [titularTransferencia, setTitularTransferencia] = useState('');
  const [voucherFile, setVoucherFile] = useState<File | null>(null);
  const [voucherPreview, setVoucherPreview] = useState<string | null>(null);

  // Result state
  const [paymentResult, setPaymentResult] = useState<{
    success: boolean;
    isDuplicate: boolean;
    paymentId?: string;
    message: string;
  } | null>(null);

  const handleCreateOrder = () => {
    const order = createOrder(productType, item.id);
    setCreatedOrderId(order.id);
    setCreatedOrderCode(order.orderCode);
    setStep('payment');
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      setVoucherFile(file);
      const reader = new FileReader();
      reader.onload = (event) => {
        setVoucherPreview(event.target?.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleRegisterPayment = (e: React.FormEvent) => {
    e.preventDefault();
    if (!numeroOperacion.trim() || !titularTransferencia.trim()) return;

    const res = registerPayment(createdOrderId, {
      banco,
      numeroOperacion,
      fechaPago,
      monto,
      titularTransferencia,
      voucherFileName: voucherFile ? voucherFile.name : `voucher_${numeroOperacion}.jpg`,
      voucherUrl: voucherPreview || undefined,
    });

    setPaymentResult({
      success: res.success,
      isDuplicate: res.isDuplicate,
      paymentId: res.payment.id,
      message: res.message,
    });
    setStep('result');
  };

  const handleFastApproveForDemo = () => {
    if (paymentResult?.paymentId) {
      reviewPayment(paymentResult.paymentId, 'APROBADO');
      onClose();
      if (productType === 'curso') {
        setActiveTab('mis-cursos');
      } else if (productType === 'evento') {
        setActiveTab('mis-eventos');
      } else {
        setActiveTab('asesoria');
      }
    }
  };

  return (
    <div className="fixed inset-0 z-50 bg-slate-950/80 backdrop-blur-sm flex items-center justify-center p-4">
      <div className="bg-slate-900 border border-slate-700/80 rounded-3xl max-w-xl w-full max-h-[90vh] overflow-y-auto shadow-2xl flex flex-col animate-in zoom-in-95 duration-200">
        {/* Modal Header */}
        <div className="p-5 border-b border-slate-800 flex items-center justify-between bg-slate-950">
          <div className="flex items-center gap-2.5">
            <div className="w-8 h-8 rounded-lg bg-amber-500/20 text-amber-400 flex items-center justify-center font-bold">
              <CreditCard className="w-4 h-4" />
            </div>
            <div>
              <h3 className="font-bold text-sm text-white">
                {step === 'order' && 'Paso 1: Generación de Orden de Compra'}
                {step === 'payment' && 'Paso 2: Registro de Pago Bancario'}
                {step === 'result' && 'Paso 3: Validación Antifraude'}
              </h3>
              <p className="text-[11px] text-slate-400">Regla estricta: 1 Pago = 1 Producto (Secciones 7 y 8)</p>
            </div>
          </div>
          <button 
            onClick={onClose}
            className="p-1 rounded-md text-slate-400 hover:text-white hover:bg-slate-800"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Modal Content */}
        <div className="p-6 space-y-5">
          {/* STEP 1: ORDER CONFIRMATION */}
          {step === 'order' && (
            <div className="space-y-4">
              <div className="p-4 rounded-2xl bg-slate-950 border border-slate-800 flex items-start gap-4">
                <img 
                  src={item.bannerUrl} 
                  alt={item.title} 
                  className="w-20 h-20 rounded-xl object-cover shrink-0 border border-slate-700"
                />
                <div className="flex-1 min-w-0">
                  <span className="text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded bg-amber-500/10 text-amber-400 border border-amber-500/20">
                    {productType.toUpperCase()}
                  </span>
                  <h4 className="font-bold text-base text-white mt-1 leading-snug">{item.title}</h4>
                  <div className="text-sm font-semibold text-amber-400 mt-2">
                    Inversión: S/. {item.price.toFixed(2)} PEN
                  </div>
                </div>
              </div>

              <div className="p-4 rounded-xl bg-indigo-950/40 border border-indigo-800/60 text-xs text-indigo-200 space-y-1.5">
                <div className="font-bold text-amber-300 flex items-center gap-1.5">
                  <ShieldCheck className="w-4 h-4" />
                  <span>Regla Crítica de Compra (Sección 7 del Documento):</span>
                </div>
                <p>
                  Cada compra genera una orden interna única. Un pago bancario corresponde estrictamente a un solo producto y no puede ser reutilizado para habilitar más de un curso o evento.
                </p>
              </div>

              <div className="pt-2 flex justify-end gap-3">
                <button
                  type="button"
                  onClick={onClose}
                  className="px-4 py-2 rounded-xl text-slate-300 text-xs font-semibold hover:bg-slate-800"
                >
                  Cancelar
                </button>
                <button
                  type="button"
                  onClick={handleCreateOrder}
                  className="px-5 py-2.5 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs flex items-center gap-2 shadow-lg shadow-amber-500/20"
                >
                  <span>Generar Orden de Compra</span>
                  <ArrowRight className="w-4 h-4" />
                </button>
              </div>
            </div>
          )}

          {/* STEP 2: PAYMENT REGISTRATION WITH ANTIFRAUD FIELDS */}
          {step === 'payment' && (
            <form onSubmit={handleRegisterPayment} className="space-y-4">
              <div className="flex items-center justify-between p-3 rounded-xl bg-slate-950 border border-slate-800 text-xs">
                <div>
                  <span className="text-slate-400">Orden asignada:</span>
                  <span className="font-mono font-bold text-amber-400 ml-2">{createdOrderCode}</span>
                </div>
                <div>
                  <span className="text-slate-400">Monto exacto:</span>
                  <span className="font-bold text-white ml-2">S/. {monto.toFixed(2)}</span>
                </div>
              </div>

              {/* Bank Accounts Info Box */}
              <div className="p-3.5 rounded-xl bg-slate-950 border border-slate-800 text-[11px] text-slate-400 space-y-1">
                <div className="font-semibold text-slate-200">Cuentas Bancarias Oficiales:</div>
                <div className="grid grid-cols-2 gap-2 text-slate-300">
                  <div>• BCP Soles: 191-94827103-0-12 (CCI: 0021910094827103012)</div>
                  <div>• BBVA Soles: 0011-0345-0200876543</div>
                  <div>• Interbank: 200-3001928475</div>
                  <div>• Yape / Plin: 987 654 321 (Forense Edu SAC)</div>
                </div>
              </div>

              {/* Form Fields as defined in Section 8.1 */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
                <div>
                  <label className="block text-slate-400 mb-1 flex items-center gap-1">
                    <Building2 className="w-3.5 h-3.5 text-amber-400" />
                    <span>Banco de Origen</span>
                  </label>
                  <select
                    value={banco}
                    onChange={(e) => setBanco(e.target.value as any)}
                    className="w-full bg-slate-950 border border-slate-700 rounded-lg p-2 text-white focus:outline-none focus:border-amber-400"
                  >
                    <option value="BCP">BCP (Banco de Crédito)</option>
                    <option value="BBVA">BBVA Continental</option>
                    <option value="Interbank">Interbank</option>
                    <option value="Scotiabank">Scotiabank</option>
                    <option value="Banco de la Nación">Banco de la Nación</option>
                    <option value="Yape / Plin">Yape / Plin</option>
                  </select>
                </div>

                <div>
                  <label className="block text-slate-400 mb-1 flex items-center gap-1">
                    <CreditCard className="w-3.5 h-3.5 text-amber-400" />
                    <span>Número de Operación *</span>
                  </label>
                  <input
                    type="text"
                    required
                    placeholder="Ej. 008927163"
                    value={numeroOperacion}
                    onChange={(e) => setNumeroOperacion(e.target.value)}
                    className="w-full bg-slate-950 border border-slate-700 rounded-lg p-2 text-white font-mono focus:outline-none focus:border-amber-400"
                  />
                  <span className="text-[10px] text-slate-500">
                    Tip: Para probar duplicado usa <button type="button" onClick={() => setNumeroOperacion('0049281726')} className="text-amber-400 underline font-mono">0049281726</button>
                  </span>
                </div>

                <div>
                  <label className="block text-slate-400 mb-1 flex items-center gap-1">
                    <Calendar className="w-3.5 h-3.5 text-amber-400" />
                    <span>Fecha de la Transferencia</span>
                  </label>
                  <input
                    type="date"
                    required
                    value={fechaPago}
                    onChange={(e) => setFechaPago(e.target.value)}
                    className="w-full bg-slate-950 border border-slate-700 rounded-lg p-2 text-white focus:outline-none focus:border-amber-400"
                  />
                </div>

                <div>
                  <label className="block text-slate-400 mb-1 flex items-center gap-1">
                    <DollarSign className="w-3.5 h-3.5 text-amber-400" />
                    <span>Monto Pagado (S/.)</span>
                  </label>
                  <input
                    type="number"
                    step="0.01"
                    required
                    value={monto}
                    onChange={(e) => setMonto(parseFloat(e.target.value) || 0)}
                    className="w-full bg-slate-950 border border-slate-700 rounded-lg p-2 text-white font-mono focus:outline-none focus:border-amber-400"
                  />
                </div>

                <div className="sm:col-span-2">
                  <label className="block text-slate-400 mb-1 flex items-center gap-1">
                    <User className="w-3.5 h-3.5 text-amber-400" />
                    <span>Titular de la Cuenta Transferente *</span>
                  </label>
                  <input
                    type="text"
                    required
                    placeholder="Nombre completo de quien transfirió"
                    value={titularTransferencia}
                    onChange={(e) => setTitularTransferencia(e.target.value)}
                    className="w-full bg-slate-950 border border-slate-700 rounded-lg p-2 text-white focus:outline-none focus:border-amber-400"
                  />
                </div>

                <div className="sm:col-span-2">
                  <label className="block text-slate-400 mb-1 flex items-center gap-1">
                    <Upload className="w-3.5 h-3.5 text-amber-400" />
                    <span>Voucher o Comprobante (JPG, PNG o PDF)</span>
                  </label>
                  <div className="border-2 border-dashed border-slate-700 hover:border-amber-400/60 rounded-xl p-4 text-center cursor-pointer transition-colors bg-slate-950/60">
                    <input
                      type="file"
                      accept="image/*,.pdf"
                      onChange={handleFileChange}
                      className="hidden"
                      id="voucher-input"
                    />
                    <label htmlFor="voucher-input" className="cursor-pointer space-y-1 block">
                      <Upload className="w-6 h-6 text-slate-400 mx-auto" />
                      <div className="text-xs text-slate-300 font-medium">
                        {voucherFile ? voucherFile.name : 'Haz clic para adjuntar comprobante'}
                      </div>
                      <div className="text-[10px] text-slate-500">Se generará huella hash SHA-256 para prevenir reusos</div>
                    </label>
                  </div>
                </div>
              </div>

              <div className="pt-2 flex justify-end gap-3">
                <button
                  type="button"
                  onClick={() => setStep('order')}
                  className="px-4 py-2 rounded-xl text-slate-400 text-xs hover:text-white"
                >
                  Volver
                </button>
                <button
                  type="submit"
                  className="px-5 py-2.5 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs flex items-center gap-2 shadow-lg shadow-amber-500/20"
                >
                  <ShieldCheck className="w-4 h-4" />
                  <span>Registrar Evidencia de Pago</span>
                </button>
              </div>
            </form>
          )}

          {/* STEP 3: RESULT SCREEN (NORMAL SUCCESS OR DUPLICATE FRAUD ALERT) */}
          {step === 'result' && paymentResult && (
            <div className="space-y-4 text-center py-2">
              {paymentResult.isDuplicate ? (
                /* ANTIFRAUD ALERT DISPLAY */
                <div className="p-5 rounded-2xl bg-rose-950/60 border border-rose-600 space-y-3 text-left">
                  <div className="flex items-center gap-2.5 text-rose-300 font-bold text-base">
                    <ShieldAlert className="w-6 h-6 text-rose-400 shrink-0" />
                    <span>ALERTA DE CONTROL ANTIFRAUDE (Sección 9)</span>
                  </div>
                  <p className="text-xs text-rose-200 leading-relaxed">
                    {paymentResult.message}
                  </p>
                  <div className="p-3 rounded-lg bg-rose-900/40 border border-rose-700/60 text-[11px] text-rose-300">
                    <span className="font-bold">Acción del Sistema:</span> El pago ha sido catalogado automáticamente como <span className="font-mono font-bold text-white bg-rose-950 px-1.5 py-0.5 rounded">DUPLICADO</span> y la orden ha sido cancelada para proteger contra fraude. No se ha emitido matrícula ni acceso.
                  </div>
                  <div className="pt-2 flex gap-2">
                    <button
                      onClick={() => setStep('payment')}
                      className="flex-1 py-2 rounded-xl bg-rose-800 hover:bg-rose-700 text-white font-semibold text-xs"
                    >
                      Intentar con otro número de operación
                    </button>
                    <button
                      onClick={onClose}
                      className="px-4 py-2 rounded-xl bg-slate-800 text-slate-300 text-xs hover:bg-slate-700"
                    >
                      Cerrar
                    </button>
                  </div>
                </div>
              ) : (
                /* NORMAL SUCCESS REGISTRATION */
                <div className="p-5 rounded-2xl bg-slate-950 border border-slate-800 space-y-3">
                  <CheckCircle2 className="w-12 h-12 text-emerald-400 mx-auto" />
                  <h4 className="font-bold text-base text-white">Pago Registrado Correctamente</h4>
                  <p className="text-xs text-slate-300 max-w-md mx-auto leading-relaxed">
                    Tu pago bancario por <span className="font-semibold text-white">S/. {monto.toFixed(2)}</span> para la orden <span className="font-mono text-amber-300 font-bold">{createdOrderCode}</span> se encuentra en estado <span className="font-semibold text-amber-400">PENDIENTE DE REVISIÓN</span>.
                  </p>
                  <p className="text-[11px] text-slate-400">
                    El administrador financiero validará el abono en cuenta bancaria. Al aprobarse, el curso aparecerá de inmediato en tu sección <span className="text-amber-400">Mis Cursos</span>.
                  </p>

                  {/* Fast Action Demo button for reviewing client flow */}
                  <div className="pt-3 border-t border-slate-800 space-y-2">
                    <div className="text-[11px] text-indigo-300 bg-indigo-950/60 p-2.5 rounded-lg border border-indigo-800/80">
                      ⚡ <strong>Modo Demostración / Auditoría:</strong> Puedes simular la aprobación administrativa inmediata de este pago con el siguiente botón:
                    </div>
                    <div className="flex gap-2">
                      <button
                        onClick={handleFastApproveForDemo}
                        className="flex-1 py-2 px-3 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white font-bold text-xs flex items-center justify-center gap-1.5 transition-colors shadow-md shadow-emerald-950"
                      >
                        <ShieldCheck className="w-4 h-4" />
                        <span>Aprobar Pago Ahora e Ingresar al Aula Virtual</span>
                      </button>
                      <button
                        onClick={() => { onClose(); setActiveTab('mis-pagos'); }}
                        className="py-2 px-4 rounded-xl bg-slate-800 text-slate-300 text-xs hover:bg-slate-700"
                      >
                        Ir a Mis Pagos
                      </button>
                    </div>
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
