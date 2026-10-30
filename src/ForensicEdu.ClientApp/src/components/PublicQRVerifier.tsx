import React, { useState } from 'react';
import { useApp } from '../context/AppContext';
import {
  ShieldCheck,
  Search,
  CheckCircle2,
  AlertTriangle,
  XCircle,
  Award,
  User,
  Calendar,
  QrCode,
  ExternalLink,
  Clock,
  Sparkles
} from 'lucide-react';

export const PublicQRVerifier: React.FC = () => {
  const { verifyCertificateByCode, certificates, publicVerificationCode, setPublicVerificationCode } = useApp();

  const [searchCode, setSearchCode] = useState(publicVerificationCode || 'CF-2026-BAL-000027');
  const [searched, setSearched] = useState(true);

  const matchedCert = verifyCertificateByCode(searchCode);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setSearched(true);
  };

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-10 space-y-8 animate-in fade-in duration-300">
      {/* Verification Portal Header */}
      <div className="text-center space-y-3">
        <div className="w-14 h-14 rounded-2xl bg-cyan-500/10 border border-cyan-500/30 text-cyan-700 flex items-center justify-center mx-auto shadow-lg shadow-cyan-500/10">
          <QrCode className="w-8 h-8" />
        </div>
        <h1 className="text-2xl sm:text-3xl font-extrabold text-ink tracking-tight">
          Portal Oficial de Verificación Pública de Certificados QR
        </h1>
        <p className="text-xs sm:text-sm text-muted max-w-xl mx-auto leading-relaxed">
          Página de validación pública de autenticidad pericial (Sección 13). Comprueba en tiempo real el registro, nota y vigencia de cualquier diploma emitido por FORENSE EDU.
        </p>
      </div>

      {/* Code Search Input Form */}
      <form onSubmit={handleSearch} className="max-w-xl mx-auto flex gap-2">
        <div className="relative flex-1">
          <Search className="w-4 h-4 text-muted absolute left-4 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            required
            placeholder="Ingresa el código (ej. CF-2026-BAL-000027)"
            value={searchCode}
            onChange={(e) => setSearchCode(e.target.value)}
            className="w-full bg-panel border border-line/80 rounded-2xl pl-11 pr-4 py-3 text-xs sm:text-sm text-ink font-mono placeholder-slate-500 focus:outline-none focus:border-cyan-400 shadow-xl"
          />
        </div>
        <button
          type="submit"
          className="px-6 py-3 rounded-2xl bg-cyan-500 hover:bg-cyan-400 text-ink font-bold text-xs sm:text-sm transition-all shadow-lg shadow-cyan-500/20"
        >
          Verificar
        </button>
      </form>

      {/* Quick click suggestions for testing */}
      <div className="text-center text-xs text-muted space-y-1">
        <span>Códigos emitidos en el sistema para probar:</span>
        <div className="flex flex-wrap items-center justify-center gap-2 pt-1 font-mono">
          {certificates.map(c => (
            <button
              key={c.id}
              onClick={() => { setSearchCode(c.certificateCode); setSearched(true); }}
              className="px-2.5 py-1 rounded-lg bg-panel border border-line hover:border-cyan-400 text-cyan-700 text-xs transition-colors"
            >
              {c.certificateCode} ({c.status})
            </button>
          ))}
        </div>
      </div>

      {/* Verification Result Card */}
      {searched && (
        <div className="pt-2">
          {matchedCert ? (
            <div className="p-6 sm:p-8 rounded-3xl bg-panel border border-line shadow-xl space-y-6 animate-in zoom-in-95 duration-200">
              {/* Status Header Badge */}
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-6 border-b border-line">
                <div className="flex items-center gap-3">
                  {matchedCert.status === 'VÁLIDO' ? (
                    <div className="w-12 h-12 rounded-2xl bg-emerald-500/20 text-emerald-700 border border-emerald-500/40 flex items-center justify-center shrink-0">
                      <CheckCircle2 className="w-7 h-7" />
                    </div>
                  ) : (
                    <div className="w-12 h-12 rounded-2xl bg-rose-500/20 text-rose-700 border border-rose-500/40 flex items-center justify-center shrink-0">
                      <XCircle className="w-7 h-7" />
                    </div>
                  )}
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="font-mono text-sm sm:text-base font-bold text-cyan-700">
                        {matchedCert.certificateCode}
                      </span>
                      <span className={`text-[10px] px-2.5 py-0.5 rounded-full font-bold border ${
                        matchedCert.status === 'VÁLIDO'
                          ? 'bg-emerald-50 text-emerald-700 border-emerald-200'
                          : 'bg-rose-50 text-rose-700 border-rose-200'
                      }`}>
                        DOCUMENTO {matchedCert.status}
                      </span>
                    </div>
                    <p className="text-xs text-muted mt-0.5">
                      Registro oficial verificado en los servidores de FORENSE EDU
                    </p>
                  </div>
                </div>

                {matchedCert.status === 'REVOCADO' && (
                  <div className="p-2.5 rounded-xl bg-rose-50/60 border border-rose-200 text-xs text-rose-700">
                    Motivo de revocación: {matchedCert.revocationReason || 'Disposición pericial'}
                  </div>
                )}
              </div>

              {/* Verified Details Grid (Section 13) */}
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4 text-xs">
                <div className="p-4 rounded-xl bg-canvas border border-line space-y-1">
                  <div className="text-muted flex items-center gap-1.5">
                    <User className="w-3.5 h-3.5 text-brand" />
                    <span>Estudiante Acreditado</span>
                  </div>
                  <div className="font-bold text-ink text-sm">{matchedCert.studentName}</div>
                  <div className="text-[11px] text-muted">Doc: {matchedCert.studentDocument}</div>
                </div>

                <div className="p-4 rounded-xl bg-canvas border border-line space-y-1 sm:col-span-2">
                  <div className="text-muted flex items-center gap-1.5">
                    <Award className="w-3.5 h-3.5 text-brand" />
                    <span>Programa Especializado Aprobado</span>
                  </div>
                  <div className="font-bold text-ink text-sm">{matchedCert.courseTitle}</div>
                  <div className="text-[11px] text-muted">Carga académica: {matchedCert.durationHours} horas oficiales</div>
                </div>

                <div className="p-4 rounded-xl bg-canvas border border-line space-y-1">
                  <div className="text-muted flex items-center gap-1.5">
                    <CheckCircle2 className="w-3.5 h-3.5 text-emerald-700" />
                    <span>Calificación Definitiva</span>
                  </div>
                  <div className="font-bold text-emerald-700 text-base font-mono">
                    {matchedCert.finalScore} / 20
                  </div>
                  <div className="text-[10px] text-muted">Nota mínima requerida: 13.00</div>
                </div>

                <div className="p-4 rounded-xl bg-canvas border border-line space-y-1">
                  <div className="text-muted flex items-center gap-1.5">
                    <Calendar className="w-3.5 h-3.5 text-muted" />
                    <span>Fecha de Emisión</span>
                  </div>
                  <div className="font-bold text-ink text-sm">{matchedCert.issueDate}</div>
                  <div className="text-[10px] text-muted">Validez permanente acreditada</div>
                </div>

                <div className="p-4 rounded-xl bg-canvas border border-line space-y-1">
                  <div className="text-muted flex items-center gap-1.5">
                    <ShieldCheck className="w-3.5 h-3.5 text-cyan-700" />
                    <span>Firma Digital & QR</span>
                  </div>
                  <div className="font-bold text-cyan-700 text-sm">Válido en Juicio Oral</div>
                  <div className="text-[10px] text-muted">Hash SHA-256 verificado</div>
                </div>
              </div>

              {/* Footer notice */}
              <div className="p-3.5 rounded-xl bg-canvas border border-line text-[11px] text-muted flex items-center justify-between">
                <span>Certificación protegida contra alteraciones conforme a la Ley de Firmas y Certificados Digitales.</span>
                <span className="font-mono text-cyan-700">FORENSE EDU S.A.C.</span>
              </div>
            </div>
          ) : (
            <div className="p-8 rounded-3xl bg-panel border border-rose-200/80 text-center space-y-3">
              <AlertTriangle className="w-12 h-12 text-rose-700 mx-auto" />
              <h3 className="text-base font-bold text-ink">Certificado no encontrado</h3>
              <p className="text-xs text-muted max-w-md mx-auto">
                No existe ningún certificado registrado bajo el código <span className="font-mono text-brand font-bold">{searchCode}</span>. Verifica el código e intenta nuevamente.
              </p>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
