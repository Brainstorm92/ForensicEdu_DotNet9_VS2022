import React, { useRef } from 'react';
import { Certificate } from '../types';
import { 
  ArrowLeft, 
  Printer, 
  Download, 
  ExternalLink, 
  ShieldCheck, 
  Award, 
  QrCode,
  CheckCircle2
} from 'lucide-react';
import { useApp } from '../context/AppContext';

interface CertificateViewProps {
  certificate: Certificate;
  onBack: () => void;
}

export const CertificateView: React.FC<CertificateViewProps> = ({ certificate, onBack }) => {
  const { setPublicVerificationCode, setActiveTab } = useApp();
  const printRef = useRef<HTMLDivElement>(null);

  const handlePrint = () => {
    window.print();
  };

  const handleGoToVerifier = () => {
    setPublicVerificationCode(certificate.certificateCode);
    setActiveTab('verificador-publico');
  };

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6 animate-in fade-in duration-300">
      {/* Top action bar */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-slate-800 print:hidden">
        <button
          onClick={onBack}
          className="flex items-center gap-2 text-xs font-semibold text-slate-300 hover:text-white transition-colors"
        >
          <ArrowLeft className="w-4 h-4" />
          <span>Volver a Mis Certificados</span>
        </button>

        <div className="flex items-center gap-3">
          <button
            onClick={handleGoToVerifier}
            className="px-3.5 py-2 rounded-xl bg-cyan-950 border border-cyan-700 text-cyan-300 hover:bg-cyan-900 text-xs font-semibold flex items-center gap-1.5 transition-colors"
          >
            <ExternalLink className="w-3.5 h-3.5" />
            <span>Validar QR en Portal Público</span>
          </button>

          <button
            onClick={handlePrint}
            className="px-4 py-2 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs flex items-center gap-2 shadow-lg shadow-amber-500/20 transition-all"
          >
            <Printer className="w-4 h-4" />
            <span>Imprimir / Guardar como PDF</span>
          </button>
        </div>
      </div>

      {/* LUXURY DIPLOMA LAYOUT FOR SCREEN & PRINT (Section 13) */}
      <div 
        ref={printRef}
        className="bg-white text-slate-950 p-8 sm:p-14 rounded-3xl shadow-2xl border-8 border-double border-amber-600/60 relative overflow-hidden print:p-8 print:border-amber-700 print:shadow-none"
      >
        {/* Ornate corner watermarks */}
        <div className="absolute top-4 left-4 w-12 h-12 border-t-2 border-l-2 border-amber-600/80 pointer-events-none"></div>
        <div className="absolute top-4 right-4 w-12 h-12 border-t-2 border-r-2 border-amber-600/80 pointer-events-none"></div>
        <div className="absolute bottom-4 left-4 w-12 h-12 border-b-2 border-l-2 border-amber-600/80 pointer-events-none"></div>
        <div className="absolute bottom-4 right-4 w-12 h-12 border-b-2 border-r-2 border-amber-600/80 pointer-events-none"></div>

        {/* Background faint watermark */}
        <div className="absolute inset-0 flex items-center justify-center opacity-[0.03] pointer-events-none select-none">
          <Award className="w-[500px] h-[500px] text-amber-900" />
        </div>

        <div className="relative z-10 space-y-8 text-center">
          {/* Header */}
          <div className="space-y-2">
            <div className="flex items-center justify-center gap-2 text-amber-700 font-bold tracking-widest text-xs uppercase">
              <ShieldCheck className="w-5 h-5" />
              <span>Instituto Iberoamericano de Criminalística y Ciencias Forenses</span>
            </div>
            <h1 className="text-3xl sm:text-4xl font-serif font-extrabold text-slate-900 tracking-wide uppercase">
              Certificado de Aprobación
            </h1>
            <p className="text-xs text-slate-600 font-medium tracking-wider">
              RESOLUCIÓN ACADÉMICA INSTITUCIONAL Nº 2026-FORENSE-EDU
            </p>
          </div>

          {/* Certificate body */}
          <div className="space-y-4 max-w-2xl mx-auto">
            <p className="text-xs sm:text-sm text-slate-600 italic">
              Por cuanto el(la) participante ha cumplido satisfactoriamente con los requisitos académicos y evaluaciones periciales del programa:
            </p>

            <div className="text-2xl sm:text-3xl font-serif font-extrabold text-amber-900 tracking-tight border-b-2 border-amber-500/40 pb-2 inline-block px-8">
              {certificate.studentName}
            </div>

            <p className="text-xs text-slate-600 font-medium">
              Con documento de identidad oficial Nº <strong className="text-slate-900 font-bold">{certificate.studentDocument}</strong>
            </p>

            <p className="text-xs sm:text-sm text-slate-700 leading-relaxed pt-2">
              Por haber aprobado con éxito el programa de especialización en:
            </p>

            <div className="text-xl sm:text-2xl font-serif font-bold text-slate-950 uppercase tracking-wide bg-amber-50/80 py-2.5 px-6 rounded-xl border border-amber-200/80 inline-block shadow-sm">
              {certificate.courseTitle}
            </div>

            <div className="flex flex-wrap items-center justify-center gap-6 text-xs text-slate-700 pt-2 font-medium">
              <div>
                Calificación Definitiva: <strong className="text-amber-800 text-sm font-bold font-mono">{certificate.finalScore} / 20</strong> (Mín. 13)
              </div>
              <div>•</div>
              <div>
                Carga Horaria: <strong className="text-slate-900 font-bold">{certificate.durationHours} Horas Académicas</strong>
              </div>
              <div>•</div>
              <div>
                Fecha de Emisión: <strong className="text-slate-900 font-bold">{certificate.issueDate}</strong>
              </div>
            </div>
          </div>

          {/* Footer: Signatures and QR Code */}
          <div className="pt-8 border-t border-slate-200 grid grid-cols-1 sm:grid-cols-3 gap-6 items-end text-center">
            {/* Signature 1 */}
            <div className="space-y-1">
              <div className="h-12 border-b border-slate-400 max-w-[160px] mx-auto flex items-end justify-center pb-1 font-serif italic text-slate-700 text-sm">
                Dra. P. Benavides R.
              </div>
              <div className="text-[11px] font-bold text-slate-800">Dra. Patricia Benavides Rojas</div>
              <div className="text-[10px] text-slate-500">Directora del Programa Forense</div>
            </div>

            {/* Central Official Seal & Status */}
            <div className="flex flex-col items-center justify-center space-y-1.5">
              <div className="w-16 h-16 rounded-full border-4 border-amber-600/40 bg-amber-500/10 flex items-center justify-center text-amber-700 shadow-inner">
                <Award className="w-8 h-8" />
              </div>
              <div className="text-[11px] font-bold text-amber-800 tracking-wider">
                SELLO OFICIAL REGISTRADO
              </div>
              <span className={`px-3 py-0.5 rounded-full text-[10px] font-bold border ${
                certificate.status === 'VÁLIDO'
                  ? 'bg-emerald-100 text-emerald-800 border-emerald-300'
                  : 'bg-rose-100 text-rose-800 border-rose-300'
              }`}>
                ESTADO: {certificate.status}
              </span>
            </div>

            {/* Signature 2 + QR Code (Section 13) */}
            <div className="space-y-1 sm:text-right flex flex-col items-center sm:items-end">
              <div className="flex items-center gap-3 bg-slate-50 p-2 rounded-xl border border-slate-200">
                <div className="text-left text-[10px] text-slate-600">
                  <div className="font-bold text-slate-800">CÓDIGO ÚNICO:</div>
                  <div className="font-mono text-amber-800 font-bold text-xs">{certificate.certificateCode}</div>
                  <div className="text-[9px] text-slate-500 mt-0.5">Escanea para verificar</div>
                </div>
                {certificate.qrDataUrl ? (
                  <img
                    src={certificate.qrDataUrl}
                    alt="Código QR de Verificación"
                    className="w-14 h-14 rounded border border-slate-300 p-0.5 bg-white"
                  />
                ) : (
                  <div className="w-14 h-14 bg-slate-200 rounded animate-pulse"></div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
