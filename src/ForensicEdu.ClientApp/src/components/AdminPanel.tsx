import React, { useState } from 'react';
import { useApp } from '../context/AppContext';
import { 
  ShieldAlert, 
  CreditCard, 
  BookOpen, 
  Award, 
  MessageSquare, 
  Share2, 
  History, 
  Users, 
  CheckCircle2, 
  XCircle, 
  AlertTriangle, 
  Clock, 
  Search,
  Filter,
  Eye,
  ExternalLink,
  Save,
  Send,
  RotateCcw
} from 'lucide-react';
import { PaymentStatus, CertificateStatus, SupportTicket, SocialLinksConfig } from '../types';

export const AdminPanel: React.FC = () => {
  const { 
    currentUser, 
    payments, 
    reviewPayment, 
    enrollments, 
    certificates, 
    updateCertificateStatus,
    tickets, 
    replyTicket, 
    updateTicketStatus,
    socialLinks, 
    updateSocialLinks,
    auditLogs,
    allUsers,
    courses
  } = useApp();

  const [adminTab, setAdminTab] = useState<'financiero' | 'academico' | 'soporte' | 'redes' | 'auditoria' | 'cuentas'>('financiero');

  // Filter state for payments
  const [paymentFilter, setPaymentFilter] = useState<string>('todos');

  // Rejection/Observation modal state
  const [actionPaymentModal, setActionPaymentModal] = useState<{
    paymentId: string;
    action: 'RECHAZAR' | 'OBSERVAR' | 'DUPLICADO';
  } | null>(null);
  const [actionReason, setActionReason] = useState('');

  // Revocation modal state
  const [actionCertModal, setActionCertModal] = useState<string | null>(null);
  const [certRevocationReason, setCertRevocationReason] = useState('');

  // Ticket reply state
  const [replyTicketId, setReplyTicketId] = useState<string | null>(null);
  const [replyText, setReplyText] = useState('');

  // Social links form state
  const [editableSocials, setEditableSocials] = useState<SocialLinksConfig>(socialLinks);
  const [socialsSaved, setSocialsSaved] = useState(false);

  const filteredPayments = payments.filter(p => {
    if (paymentFilter === 'todos') return true;
    return p.status === paymentFilter;
  });

  const handleConfirmPaymentAction = () => {
    if (!actionPaymentModal) return;
    const { paymentId, action } = actionPaymentModal;
    const statusMap: Record<string, PaymentStatus> = {
      RECHAZAR: 'RECHAZADO',
      OBSERVAR: 'OBSERVADO',
      DUPLICADO: 'DUPLICADO',
    };
    reviewPayment(paymentId, statusMap[action], actionReason);
    setActionPaymentModal(null);
    setActionReason('');
  };

  const handleConfirmCertRevocation = () => {
    if (!actionCertModal) return;
    const cert = certificates.find(c => c.id === actionCertModal);
    if (!cert) return;
    const newStatus: CertificateStatus = cert.status === 'VÁLIDO' ? 'REVOCADO' : 'VÁLIDO';
    updateCertificateStatus(cert.id, newStatus, certRevocationReason || 'Disposición pericial');
    setActionCertModal(null);
    setCertRevocationReason('');
  };

  const handleSendTicketReply = (ticketId: string) => {
    if (!replyText.trim()) return;
    replyTicket(ticketId, replyText);
    setReplyText('');
    setReplyTicketId(null);
  };

  const handleSaveSocials = (e: React.FormEvent) => {
    e.preventDefault();
    updateSocialLinks(editableSocials);
    setSocialsSaved(true);
    setTimeout(() => setSocialsSaved(false), 3000);
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6 animate-in fade-in duration-300">
      {/* Admin Header */}
      <div className="p-6 rounded-3xl bg-slate-900 border border-slate-800 flex flex-col md:flex-row md:items-center justify-between gap-4 shadow-xl">
        <div className="flex items-center gap-3.5">
          <div className="w-12 h-12 rounded-2xl bg-purple-500/20 text-purple-400 border border-purple-500/30 flex items-center justify-center font-bold">
            <ShieldAlert className="w-6 h-6" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-xl sm:text-2xl font-bold text-white">Panel Administrativo Central</h1>
              <span className="text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded bg-purple-900 text-purple-200 border border-purple-700">
                Sección 17 & 18
              </span>
            </div>
            <p className="text-xs text-slate-400 mt-0.5">
              Operando como: <strong className="text-white">{currentUser.nombres} {currentUser.apellidos}</strong> ({currentUser.role})
            </p>
          </div>
        </div>

        {/* Quick Tabs Bar */}
        <div className="flex items-center gap-1.5 overflow-x-auto p-1 bg-slate-950 rounded-2xl border border-slate-800 text-xs">
          <button
            onClick={() => setAdminTab('financiero')}
            className={`px-3 py-1.5 rounded-xl font-semibold transition-colors flex items-center gap-1.5 whitespace-nowrap ${
              adminTab === 'financiero' ? 'bg-amber-500 text-slate-950' : 'text-slate-300 hover:text-white'
            }`}
          >
            <CreditCard className="w-3.5 h-3.5" />
            <span>Financiero & Antifraude</span>
          </button>

          <button
            onClick={() => setAdminTab('academico')}
            className={`px-3 py-1.5 rounded-xl font-semibold transition-colors flex items-center gap-1.5 whitespace-nowrap ${
              adminTab === 'academico' ? 'bg-amber-500 text-slate-950' : 'text-slate-300 hover:text-white'
            }`}
          >
            <Award className="w-3.5 h-3.5" />
            <span>Académico & Notas</span>
          </button>

          <button
            onClick={() => setAdminTab('soporte')}
            className={`px-3 py-1.5 rounded-xl font-semibold transition-colors flex items-center gap-1.5 whitespace-nowrap ${
              adminTab === 'soporte' ? 'bg-amber-500 text-slate-950' : 'text-slate-300 hover:text-white'
            }`}
          >
            <MessageSquare className="w-3.5 h-3.5" />
            <span>Tickets Soporte</span>
          </button>

          <button
            onClick={() => setAdminTab('redes')}
            className={`px-3 py-1.5 rounded-xl font-semibold transition-colors flex items-center gap-1.5 whitespace-nowrap ${
              adminTab === 'redes' ? 'bg-amber-500 text-slate-950' : 'text-slate-300 hover:text-white'
            }`}
          >
            <Share2 className="w-3.5 h-3.5" />
            <span>Redes Sociales</span>
          </button>

          <button
            onClick={() => setAdminTab('cuentas')}
            className={`px-3 py-1.5 rounded-xl font-semibold transition-colors flex items-center gap-1.5 whitespace-nowrap ${
              adminTab === 'cuentas' ? 'bg-amber-500 text-slate-950' : 'text-slate-300 hover:text-white'
            }`}
          >
            <Users className="w-3.5 h-3.5" />
            <span>Vigencia Cuentas</span>
          </button>

          <button
            onClick={() => setAdminTab('auditoria')}
            className={`px-3 py-1.5 rounded-xl font-semibold transition-colors flex items-center gap-1.5 whitespace-nowrap ${
              adminTab === 'auditoria' ? 'bg-amber-500 text-slate-950' : 'text-slate-300 hover:text-white'
            }`}
          >
            <History className="w-3.5 h-3.5" />
            <span>Audit Log</span>
          </button>
        </div>
      </div>

      {/* =========================================================================
          TAB 1: GESTIÓN FINANCIERA & CONTROL ANTIFRAUDE (SECCIÓN 8 & 9)
         ========================================================================= */}
      {adminTab === 'financiero' && (
        <div className="space-y-4">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 p-4 rounded-2xl bg-slate-900 border border-slate-800">
            <div>
              <h3 className="text-base font-bold text-white flex items-center gap-2">
                <CreditCard className="w-4 h-4 text-emerald-400" />
                <span>Validación de Pagos Bancarios & Detección de Duplicados</span>
              </h3>
              <p className="text-xs text-slate-400">
                Aprobación habilita inmediatamente la matrícula y extiende la vigencia del usuario por 2 años.
              </p>
            </div>

            {/* Filter pills */}
            <div className="flex items-center gap-1.5 text-xs">
              <span className="text-slate-400">Filtrar:</span>
              {['todos', 'PENDIENTE', 'EN_REVISIÓN', 'APROBADO', 'DUPLICADO', 'RECHAZADO'].map(st => (
                <button
                  key={st}
                  onClick={() => setPaymentFilter(st)}
                  className={`px-2.5 py-1 rounded-lg font-medium transition-colors ${
                    paymentFilter === st ? 'bg-amber-500 text-slate-950 font-bold' : 'bg-slate-950 text-slate-400 hover:text-white'
                  }`}
                >
                  {st}
                </button>
              ))}
            </div>
          </div>

          {/* Payments Table */}
          <div className="bg-slate-900 border border-slate-800 rounded-3xl overflow-hidden shadow-xl">
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs text-slate-300">
                <thead className="bg-slate-950 text-slate-400 font-semibold border-b border-slate-800">
                  <tr>
                    <th className="p-4">Orden / Fecha</th>
                    <th className="p-4">Usuario / Titular</th>
                    <th className="p-4">Banco & Operación</th>
                    <th className="p-4">Monto</th>
                    <th className="p-4">Huella Antifraude</th>
                    <th className="p-4">Estado</th>
                    <th className="p-4 text-right">Acciones de Validación</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-800/80">
                  {filteredPayments.map((p) => (
                    <tr key={p.id} className="hover:bg-slate-850/50 transition-colors">
                      <td className="p-4">
                        <div className="font-mono font-bold text-amber-400">{p.orderCode}</div>
                        <div className="text-[10px] text-slate-500">{new Date(p.registeredAt).toLocaleDateString()}</div>
                      </td>
                      <td className="p-4">
                        <div className="font-medium text-white">{p.userName}</div>
                        <div className="text-[11px] text-slate-400">Titular: {p.titularTransferencia}</div>
                        <div className="text-[10px] text-slate-500">{p.userEmail}</div>
                      </td>
                      <td className="p-4">
                        <div className="font-semibold text-white">{p.banco}</div>
                        <div className="font-mono text-cyan-300 text-xs">Op: {p.numeroOperacion}</div>
                      </td>
                      <td className="p-4 font-bold text-white font-mono text-sm">
                        S/. {p.monto.toFixed(2)}
                      </td>
                      <td className="p-4 max-w-xs">
                        <div className="font-mono text-[9px] text-slate-400 truncate bg-slate-950 p-1.5 rounded border border-slate-800" title={p.paymentFingerprint}>
                          {p.paymentFingerprint}
                        </div>
                        {p.rejectionReason && (
                          <div className="text-[10px] text-rose-400 mt-1">
                            {p.rejectionReason}
                          </div>
                        )}
                      </td>
                      <td className="p-4">
                        <span className={`px-2.5 py-0.5 rounded-full border text-[10px] font-bold ${
                          p.status === 'APROBADO' ? 'bg-emerald-950 text-emerald-300 border-emerald-700' :
                          p.status === 'DUPLICADO' ? 'bg-purple-950 text-purple-300 border-purple-700 animate-pulse font-extrabold' :
                          p.status === 'RECHAZADO' ? 'bg-rose-950 text-rose-300 border-rose-700' :
                          p.status === 'OBSERVADO' ? 'bg-orange-950 text-orange-300 border-orange-700' :
                          'bg-amber-950 text-amber-300 border-amber-700'
                        }`}>
                          {p.status}
                        </span>
                      </td>
                      <td className="p-4 text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          {p.status !== 'APROBADO' && (
                            <button
                              onClick={() => reviewPayment(p.id, 'APROBADO')}
                              className="px-2.5 py-1 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-white font-bold text-[11px] transition-colors"
                              title="Aprobar pago y generar matrícula"
                            >
                              Aprobar
                            </button>
                          )}
                          <button
                            onClick={() => setActionPaymentModal({ paymentId: p.id, action: 'OBSERVAR' })}
                            className="px-2 py-1 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300 text-[11px]"
                            title="Observar información"
                          >
                            Observar
                          </button>
                          <button
                            onClick={() => setActionPaymentModal({ paymentId: p.id, action: 'RECHAZAR' })}
                            className="px-2 py-1 rounded-lg bg-rose-950 hover:bg-rose-900 border border-rose-800 text-rose-300 text-[11px]"
                            title="Rechazar pago"
                          >
                            Rechazar
                          </button>
                          <button
                            onClick={() => setActionPaymentModal({ paymentId: p.id, action: 'DUPLICADO' })}
                            className="px-2 py-1 rounded-lg bg-purple-950 hover:bg-purple-900 border border-purple-800 text-purple-300 text-[11px]"
                            title="Marcar como duplicado"
                          >
                            Duplicado
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* =========================================================================
          TAB 2: GESTIÓN ACADÉMICA & CERTIFICADOS (SECCIÓN 12 & 13)
         ========================================================================= */}
      {adminTab === 'academico' && (
        <div className="space-y-6">
          {/* Enrollments & Grades Summary */}
          <div className="p-5 rounded-3xl bg-slate-900 border border-slate-800 space-y-4">
            <h3 className="font-bold text-sm text-white">Seguimiento Académico, Intentos y Calificaciones (Sección 12)</h3>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs text-slate-300">
                <thead className="bg-slate-950 text-slate-400 font-semibold border-b border-slate-800">
                  <tr>
                    <th className="p-3">Estudiante</th>
                    <th className="p-3">Curso</th>
                    <th className="p-3">Avance</th>
                    <th className="p-3">1º Intento</th>
                    <th className="p-3">2º Intento</th>
                    <th className="p-3">Nota Definitiva</th>
                    <th className="p-3">Estado Académico</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-800/80">
                  {enrollments.map((enr) => {
                    const u = allUsers.find(x => x.id === enr.userId);
                    const c = courses.find(x => x.id === enr.courseId);
                    return (
                      <tr key={enr.id} className="hover:bg-slate-850/40">
                        <td className="p-3 font-medium text-white">{u?.nombres} {u?.apellidos}</td>
                        <td className="p-3">{c?.title}</td>
                        <td className="p-3">{enr.progressPercent}%</td>
                        <td className="p-3 font-mono">{enr.firstAttemptScore !== undefined ? `${enr.firstAttemptScore}/20` : 'Pendiente'}</td>
                        <td className="p-3 font-mono">{enr.secondAttemptScore !== undefined ? `${enr.secondAttemptScore}/20` : '-'}</td>
                        <td className="p-3 font-mono font-bold text-amber-400">{enr.finalScore !== undefined ? `${enr.finalScore}/20` : '-'}</td>
                        <td className="p-3">
                          <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                            enr.examStatus === 'APROBADA' ? 'bg-emerald-950 text-emerald-300 border border-emerald-700' :
                            enr.examStatus === 'DESAPROBADA' ? 'bg-rose-950 text-rose-300 border border-rose-700' :
                            'bg-slate-800 text-slate-400'
                          }`}>
                            {enr.examStatus}
                          </span>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>

          {/* Certificates Management with Revocation Action */}
          <div className="p-5 rounded-3xl bg-slate-900 border border-slate-800 space-y-4">
            <h3 className="font-bold text-sm text-white">Certificados Digitales Emitidos y Control de Revocación (Sección 13)</h3>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs text-slate-300">
                <thead className="bg-slate-950 text-slate-400 font-semibold border-b border-slate-800">
                  <tr>
                    <th className="p-3">Código Único</th>
                    <th className="p-3">Participante</th>
                    <th className="p-3">Curso</th>
                    <th className="p-3">Nota</th>
                    <th className="p-3">Fecha Emisión</th>
                    <th className="p-3">Estado QR</th>
                    <th className="p-3 text-right">Acción de Revocación</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-800/80">
                  {certificates.map((cert) => (
                    <tr key={cert.id} className="hover:bg-slate-850/40">
                      <td className="p-3 font-mono font-bold text-cyan-400">{cert.certificateCode}</td>
                      <td className="p-3 font-medium text-white">{cert.studentName}</td>
                      <td className="p-3">{cert.courseTitle}</td>
                      <td className="p-3 font-mono font-bold text-amber-400">{cert.finalScore}/20</td>
                      <td className="p-3">{cert.issueDate}</td>
                      <td className="p-3">
                        <span className={`px-2.5 py-0.5 rounded-full text-[10px] font-bold border ${
                          cert.status === 'VÁLIDO' ? 'bg-emerald-950 text-emerald-300 border-emerald-700' : 'bg-rose-950 text-rose-300 border-rose-700'
                        }`}>
                          {cert.status}
                        </span>
                      </td>
                      <td className="p-3 text-right">
                        <button
                          onClick={() => setActionCertModal(cert.id)}
                          className={`px-3 py-1 rounded-lg text-xs font-semibold ${
                            cert.status === 'VÁLIDO'
                              ? 'bg-rose-950 hover:bg-rose-900 border border-rose-800 text-rose-300'
                              : 'bg-emerald-950 hover:bg-emerald-900 border border-emerald-800 text-emerald-300'
                          }`}
                        >
                          {cert.status === 'VÁLIDO' ? 'Revocar Certificado' : 'Reactivar Validez'}
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* =========================================================================
          TAB 3: TICKETS DE SOPORTE (SECCIÓN 15.2)
         ========================================================================= */}
      {adminTab === 'soporte' && (
        <div className="space-y-4">
          <div className="p-4 rounded-2xl bg-slate-900 border border-slate-800">
            <h3 className="font-bold text-base text-white">Gestión de Tickets y Consultas de Usuarios (Sección 15.2)</h3>
            <p className="text-xs text-slate-400">Atención y resolución con trazabilidad y estados: ABIERTO, EN_ATENCIÓN, RESPONDIDO, CERRADO</p>
          </div>

          <div className="grid grid-cols-1 gap-4">
            {tickets.map((t) => (
              <div key={t.id} className="p-5 rounded-2xl bg-slate-900 border border-slate-800 space-y-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="font-mono font-bold text-amber-400 text-xs">{t.ticketCode}</span>
                    <span className="text-[10px] px-2 py-0.5 rounded bg-slate-950 text-slate-300 border border-slate-800">{t.category}</span>
                    <span className="text-xs text-slate-400">• De: <strong className="text-white">{t.userName}</strong> ({t.userEmail})</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className={`text-[10px] px-2.5 py-0.5 rounded-full font-bold ${
                      t.status === 'RESPONDIDO' ? 'bg-emerald-950 text-emerald-300 border border-emerald-700' :
                      t.status === 'CERRADO' ? 'bg-slate-800 text-slate-400' :
                      'bg-amber-950 text-amber-300 border border-amber-700'
                    }`}>
                      {t.status}
                    </span>
                    <select
                      value={t.status}
                      onChange={(e) => updateTicketStatus(t.id, e.target.value as any)}
                      className="bg-slate-950 border border-slate-700 text-xs text-slate-300 rounded px-2 py-1"
                    >
                      <option value="ABIERTO">ABIERTO</option>
                      <option value="EN_ATENCIÓN">EN_ATENCIÓN</option>
                      <option value="RESPONDIDO">RESPONDIDO</option>
                      <option value="CERRADO">CERRADO</option>
                    </select>
                  </div>
                </div>

                <div className="font-bold text-sm text-white">{t.subject}</div>
                <p className="text-xs text-slate-300 bg-slate-950 p-3 rounded-xl border border-slate-850">
                  {t.message}
                </p>

                {/* Responses */}
                {t.responses.length > 0 && (
                  <div className="space-y-2 pt-2 border-t border-slate-850">
                    {t.responses.map((r, i) => (
                      <div key={i} className="p-2.5 rounded-lg bg-emerald-950/20 border border-emerald-800/40 text-xs space-y-0.5">
                        <div className="flex items-center justify-between text-[10px] text-slate-400">
                          <span className="font-semibold text-emerald-300">{r.author}</span>
                          <span>{new Date(r.timestamp).toLocaleString()}</span>
                        </div>
                        <p className="text-slate-200">{r.message}</p>
                      </div>
                    ))}
                  </div>
                )}

                {/* Reply Box */}
                {replyTicketId === t.id ? (
                  <div className="pt-2 space-y-2">
                    <textarea
                      rows={3}
                      placeholder="Escribe la respuesta oficial para el estudiante..."
                      value={replyText}
                      onChange={(e) => setReplyText(e.target.value)}
                      className="w-full bg-slate-950 border border-slate-700 rounded-xl p-2.5 text-xs text-white resize-none"
                    />
                    <div className="flex justify-end gap-2">
                      <button
                        onClick={() => setReplyTicketId(null)}
                        className="px-3 py-1 rounded-lg text-slate-400 text-xs"
                      >
                        Cancelar
                      </button>
                      <button
                        onClick={() => handleSendTicketReply(t.id)}
                        className="px-4 py-1.5 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-white font-bold text-xs flex items-center gap-1.5"
                      >
                        <Send className="w-3.5 h-3.5" />
                        <span>Publicar Respuesta</span>
                      </button>
                    </div>
                  </div>
                ) : (
                  <div className="pt-1">
                    <button
                      onClick={() => setReplyTicketId(t.id)}
                      className="px-3 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-amber-300 text-xs font-semibold"
                    >
                      + Responder a este ticket
                    </button>
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* =========================================================================
          TAB 4: CONFIGURACIÓN DE REDES SOCIALES (SECCIÓN 15.1)
         ========================================================================= */}
      {adminTab === 'redes' && (
        <form onSubmit={handleSaveSocials} className="p-6 rounded-3xl bg-slate-900 border border-slate-800 space-y-6">
          <div className="flex items-center justify-between pb-4 border-b border-slate-800">
            <div>
              <h3 className="text-base font-bold text-white flex items-center gap-2">
                <Share2 className="w-5 h-5 text-amber-400" />
                <span>Gestión de Enlaces de Redes Sociales (Sección 15.1)</span>
              </h3>
              <p className="text-xs text-slate-400 mt-0.5">
                Regla técnica: Los enlaces deben ser administrables sin modificar código y abrir en pestaña nueva.
              </p>
            </div>
            {socialsSaved && (
              <span className="text-xs font-bold text-emerald-400 bg-emerald-950 px-3 py-1 rounded-full border border-emerald-700">
                ✓ Guardado exitosamente
              </span>
            )}
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 text-xs">
            {/* WhatsApp */}
            <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-bold text-emerald-400">WhatsApp de Soporte</span>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editableSocials.whatsapp.enabled}
                    onChange={(e) => setEditableSocials(prev => ({
                      ...prev,
                      whatsapp: { ...prev.whatsapp, enabled: e.target.checked }
                    }))}
                    className="rounded text-amber-400 focus:ring-0"
                  />
                  <span className="text-[11px] text-slate-400">Activo</span>
                </label>
              </div>
              <input
                type="text"
                value={editableSocials.whatsapp.number}
                onChange={(e) => setEditableSocials(prev => ({
                  ...prev,
                  whatsapp: { ...prev.whatsapp, number: e.target.value }
                }))}
                className="w-full bg-slate-900 border border-slate-700 rounded-lg p-2 text-white"
                placeholder="+51 987 654 321"
              />
            </div>

            {/* Facebook */}
            <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-bold text-white">Facebook</span>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editableSocials.facebook.enabled}
                    onChange={(e) => setEditableSocials(prev => ({
                      ...prev,
                      facebook: { ...prev.facebook, enabled: e.target.checked }
                    }))}
                    className="rounded text-amber-400 focus:ring-0"
                  />
                  <span className="text-[11px] text-slate-400">Activo</span>
                </label>
              </div>
              <input
                type="text"
                value={editableSocials.facebook.url}
                onChange={(e) => setEditableSocials(prev => ({
                  ...prev,
                  facebook: { ...prev.facebook, url: e.target.value }
                }))}
                className="w-full bg-slate-900 border border-slate-700 rounded-lg p-2 text-white"
              />
            </div>

            {/* Instagram */}
            <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-bold text-white">Instagram</span>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editableSocials.instagram.enabled}
                    onChange={(e) => setEditableSocials(prev => ({
                      ...prev,
                      instagram: { ...prev.instagram, enabled: e.target.checked }
                    }))}
                  />
                  <span className="text-[11px] text-slate-400">Activo</span>
                </label>
              </div>
              <input
                type="text"
                value={editableSocials.instagram.url}
                onChange={(e) => setEditableSocials(prev => ({
                  ...prev,
                  instagram: { ...prev.instagram, url: e.target.value }
                }))}
                className="w-full bg-slate-900 border border-slate-700 rounded-lg p-2 text-white"
              />
            </div>

            {/* YouTube */}
            <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-bold text-white">YouTube</span>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editableSocials.youtube.enabled}
                    onChange={(e) => setEditableSocials(prev => ({
                      ...prev,
                      youtube: { ...prev.youtube, enabled: e.target.checked }
                    }))}
                  />
                  <span className="text-[11px] text-slate-400">Activo</span>
                </label>
              </div>
              <input
                type="text"
                value={editableSocials.youtube.url}
                onChange={(e) => setEditableSocials(prev => ({
                  ...prev,
                  youtube: { ...prev.youtube, url: e.target.value }
                }))}
                className="w-full bg-slate-900 border border-slate-700 rounded-lg p-2 text-white"
              />
            </div>

            {/* TikTok */}
            <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-bold text-white">TikTok</span>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editableSocials.tiktok.enabled}
                    onChange={(e) => setEditableSocials(prev => ({
                      ...prev,
                      tiktok: { ...prev.tiktok, enabled: e.target.checked }
                    }))}
                  />
                  <span className="text-[11px] text-slate-400">Activo</span>
                </label>
              </div>
              <input
                type="text"
                value={editableSocials.tiktok.url}
                onChange={(e) => setEditableSocials(prev => ({
                  ...prev,
                  tiktok: { ...prev.tiktok, url: e.target.value }
                }))}
                className="w-full bg-slate-900 border border-slate-700 rounded-lg p-2 text-white"
              />
            </div>

            {/* LinkedIn */}
            <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-bold text-white">LinkedIn</span>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editableSocials.linkedin.enabled}
                    onChange={(e) => setEditableSocials(prev => ({
                      ...prev,
                      linkedin: { ...prev.linkedin, enabled: e.target.checked }
                    }))}
                  />
                  <span className="text-[11px] text-slate-400">Activo</span>
                </label>
              </div>
              <input
                type="text"
                value={editableSocials.linkedin.url}
                onChange={(e) => setEditableSocials(prev => ({
                  ...prev,
                  linkedin: { ...prev.linkedin, url: e.target.value }
                }))}
                className="w-full bg-slate-900 border border-slate-700 rounded-lg p-2 text-white"
              />
            </div>
          </div>

          <div className="pt-2 flex justify-end">
            <button
              type="submit"
              className="px-6 py-2.5 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs flex items-center gap-2 shadow-lg shadow-amber-500/20"
            >
              <Save className="w-4 h-4" />
              <span>Guardar Configuración de Redes</span>
            </button>
          </div>
        </form>
      )}

      {/* =========================================================================
          TAB 5: VIGENCIA DE CUENTAS (SECCIÓN 4.3 & 20)
         ========================================================================= */}
      {adminTab === 'cuentas' && (
        <div className="p-6 rounded-3xl bg-slate-900 border border-slate-800 space-y-4 shadow-xl">
          <div className="flex items-center justify-between pb-3 border-b border-slate-800">
            <div>
              <h3 className="font-bold text-base text-white">Directorio de Cuentas y Control de Vigencia (Sección 4.3)</h3>
              <p className="text-xs text-slate-400">
                Regla: 30 días sin compra aprobada = Proceso de eliminación • 1er pago aprobado = 2 años de vigencia
              </p>
            </div>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-slate-300">
              <thead className="bg-slate-950 text-slate-400 font-semibold border-b border-slate-800">
                <tr>
                  <th className="p-3">Usuario</th>
                  <th className="p-3">Rol</th>
                  <th className="p-3">Fecha Creación</th>
                  <th className="p-3">Compras Aprobadas</th>
                  <th className="p-3">Vence el</th>
                  <th className="p-3">Días Restantes</th>
                  <th className="p-3">Estado de la Cuenta</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800/80">
                {allUsers.map((u) => {
                  const days = Math.ceil((new Date(u.validUntil).getTime() - Date.now()) / (1000 * 60 * 60 * 24));
                  return (
                    <tr key={u.id} className="hover:bg-slate-850/40">
                      <td className="p-3">
                        <div className="font-bold text-white">{u.nombres} {u.apellidos}</div>
                        <div className="text-[10px] text-slate-500">{u.email}</div>
                      </td>
                      <td className="p-3 font-mono text-cyan-300">{u.role}</td>
                      <td className="p-3">{new Date(u.createdAt).toLocaleDateString()}</td>
                      <td className="p-3">
                        {u.hasApprovedPurchase ? (
                          <span className="text-emerald-400 font-semibold">✓ Con Compras (Plan 2 años)</span>
                        ) : (
                          <span className="text-amber-400 font-semibold">Sin Compras (Ventana 30d)</span>
                        )}
                      </td>
                      <td className="p-3 font-mono">{new Date(u.validUntil).toLocaleDateString()}</td>
                      <td className="p-3 font-mono font-bold">
                        <span className={days <= 7 ? 'text-rose-400' : 'text-emerald-400'}>
                          {days > 0 ? `${days} días` : '0 días (Vencida)'}
                        </span>
                      </td>
                      <td className="p-3">
                        <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                          u.accountStatus === 'ACTIVA' ? 'bg-emerald-950 text-emerald-300 border border-emerald-700' :
                          u.accountStatus === 'PENDIENTE_DE_ELIMINACIÓN' ? 'bg-rose-950 text-rose-300 border border-rose-700 animate-pulse' :
                          'bg-indigo-950 text-indigo-300 border border-indigo-700'
                        }`}>
                          {u.accountStatus}
                        </span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* =========================================================================
          TAB 6: AUDITORÍA & TRAZABILIDAD (SECCIÓN 21)
         ========================================================================= */}
      {adminTab === 'auditoria' && (
        <div className="p-6 rounded-3xl bg-slate-900 border border-slate-800 space-y-4 shadow-xl">
          <div className="flex items-center justify-between pb-3 border-b border-slate-800">
            <div>
              <h3 className="font-bold text-base text-white flex items-center gap-2">
                <History className="w-5 h-5 text-amber-400" />
                <span>Registro Inmutable de Auditoría & Trazabilidad (Sección 21)</span>
              </h3>
              <p className="text-xs text-slate-400">
                Bitácora de eventos críticos: logins, órdenes, pagos, detección de fraude, notas y certificados
              </p>
            </div>
            <span className="text-xs font-mono text-slate-400">{auditLogs.length} eventos registrados</span>
          </div>

          <div className="divide-y divide-slate-800/80 max-h-[550px] overflow-y-auto">
            {auditLogs.map((log) => {
              const actionColors: Record<string, string> = {
                APROBAR_PAGO: 'text-emerald-400 bg-emerald-950 border-emerald-800',
                DETECTAR_DUPLICADO: 'text-rose-300 bg-rose-950 border-rose-700 font-bold',
                RECHAZAR_PAGO: 'text-rose-400 bg-rose-950 border-rose-800',
                EMITIR_CERTIFICADO: 'text-cyan-300 bg-cyan-950 border-cyan-800',
                REVOCAR_CERTIFICADO: 'text-purple-300 bg-purple-950 border-purple-800',
                CREAR_ORDEN: 'text-amber-300 bg-amber-950 border-amber-800',
                REGISTRAR_PAGO: 'text-blue-300 bg-blue-950 border-blue-800',
                LOGIN: 'text-slate-300 bg-slate-800 border-slate-700',
              };

              return (
                <div key={log.id} className="py-3 flex items-start gap-3 text-xs hover:bg-slate-850/40 px-2 rounded-lg transition-colors">
                  <span className="font-mono text-[11px] text-slate-500 shrink-0 w-36">
                    {new Date(log.timestamp).toLocaleString()}
                  </span>
                  <span className={`px-2 py-0.5 rounded text-[10px] border shrink-0 font-mono ${actionColors[log.action] || 'bg-slate-800 text-slate-300 border-slate-700'}`}>
                    {log.action}
                  </span>
                  <div className="flex-1 min-w-0">
                    <span className="font-semibold text-white mr-1.5">{log.userName} ({log.userRole}):</span>
                    <span className="text-slate-300">{log.details}</span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Payment Action Modal (Observar / Rechazar / Duplicado) */}
      {actionPaymentModal && (
        <div className="fixed inset-0 z-50 bg-slate-950/80 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-slate-900 border border-slate-700 rounded-2xl max-w-md w-full p-6 space-y-4 shadow-2xl">
            <h3 className="font-bold text-sm text-white">
              Marcar Pago como {actionPaymentModal.action}
            </h3>
            <p className="text-xs text-slate-400">
              Ingresa el motivo administrativo o justificación para el usuario y el registro de auditoría.
            </p>
            <textarea
              rows={3}
              required
              placeholder="Indica el motivo..."
              value={actionReason}
              onChange={(e) => setActionReason(e.target.value)}
              className="w-full bg-slate-950 border border-slate-700 rounded-xl p-2.5 text-xs text-white resize-none"
            />
            <div className="flex justify-end gap-2">
              <button
                onClick={() => setActionPaymentModal(null)}
                className="px-3 py-1.5 rounded-lg text-slate-400 text-xs"
              >
                Cancelar
              </button>
              <button
                onClick={handleConfirmPaymentAction}
                className="px-4 py-1.5 rounded-lg bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs"
              >
                Confirmar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Certificate Revocation Modal */}
      {actionCertModal && (
        <div className="fixed inset-0 z-50 bg-slate-950/80 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-slate-900 border border-slate-700 rounded-2xl max-w-md w-full p-6 space-y-4 shadow-2xl">
            <h3 className="font-bold text-sm text-white">
              Cambiar Estado de Certificado
            </h3>
            <p className="text-xs text-slate-400">
              Ingresa el motivo pericial de revocación o reactivación que se reflejará públicamente al escanear el QR:
            </p>
            <input
              type="text"
              placeholder="Ej. Incumplimiento ético pericial / Rectificación"
              value={certRevocationReason}
              onChange={(e) => setCertRevocationReason(e.target.value)}
              className="w-full bg-slate-950 border border-slate-700 rounded-xl p-2.5 text-xs text-white"
            />
            <div className="flex justify-end gap-2">
              <button
                onClick={() => setActionCertModal(null)}
                className="px-3 py-1.5 rounded-lg text-slate-400 text-xs"
              >
                Cancelar
              </button>
              <button
                onClick={handleConfirmCertRevocation}
                className="px-4 py-1.5 rounded-lg bg-rose-600 hover:bg-rose-500 text-white font-bold text-xs"
              >
                Aplicar Cambio en QR
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
