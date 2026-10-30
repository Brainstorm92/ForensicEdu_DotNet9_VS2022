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
    <div className="responsive-page w-full min-w-0 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6 animate-in fade-in duration-300">
      {/* Admin Header */}
      <div className="p-6 rounded-3xl bg-panel border border-line flex flex-col md:flex-row md:items-center justify-between gap-4 shadow-xl">
        <div className="flex items-center gap-3.5">
          <div className="w-12 h-12 rounded-2xl bg-purple-500/20 text-purple-700 border border-purple-500/30 flex items-center justify-center font-bold">
            <ShieldAlert className="w-6 h-6" />
          </div>
          <div>
            <div className="flex flex-wrap items-center gap-2">
              <h1 className="text-xl sm:text-2xl font-bold text-ink">Panel Administrativo Central</h1>
              <span className="text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded bg-purple-50 text-purple-700 border border-purple-200">
                Sección 17 & 18
              </span>
            </div>
            <p className="text-xs text-muted mt-0.5">
              Operando como: <strong className="text-ink">{currentUser.nombres} {currentUser.apellidos}</strong> ({currentUser.role})
            </p>
          </div>
        </div>

        {/* Quick Tabs Bar */}
        <div className="w-full min-w-0 flex items-center gap-1.5 overflow-x-auto p-1 bg-canvas rounded-2xl border border-line text-xs">
          <button
            onClick={() => setAdminTab('financiero')}
            className={`px-3 py-1.5 rounded-xl font-semibold transition-colors flex items-center gap-1.5 whitespace-nowrap ${
              adminTab === 'financiero' ? 'bg-brand text-ink' : 'text-muted hover:text-ink'
            }`}
          >
            <CreditCard className="w-3.5 h-3.5" />
            <span>Financiero & Antifraude</span>
          </button>

          <button
            onClick={() => setAdminTab('academico')}
            className={`px-3 py-1.5 rounded-xl font-semibold transition-colors flex items-center gap-1.5 whitespace-nowrap ${
              adminTab === 'academico' ? 'bg-brand text-ink' : 'text-muted hover:text-ink'
            }`}
          >
            <Award className="w-3.5 h-3.5" />
            <span>Académico & Notas</span>
          </button>

          <button
            onClick={() => setAdminTab('soporte')}
            className={`px-3 py-1.5 rounded-xl font-semibold transition-colors flex items-center gap-1.5 whitespace-nowrap ${
              adminTab === 'soporte' ? 'bg-brand text-ink' : 'text-muted hover:text-ink'
            }`}
          >
            <MessageSquare className="w-3.5 h-3.5" />
            <span>Tickets Soporte</span>
          </button>

          <button
            onClick={() => setAdminTab('redes')}
            className={`px-3 py-1.5 rounded-xl font-semibold transition-colors flex items-center gap-1.5 whitespace-nowrap ${
              adminTab === 'redes' ? 'bg-brand text-ink' : 'text-muted hover:text-ink'
            }`}
          >
            <Share2 className="w-3.5 h-3.5" />
            <span>Redes Sociales</span>
          </button>

          <button
            onClick={() => setAdminTab('cuentas')}
            className={`px-3 py-1.5 rounded-xl font-semibold transition-colors flex items-center gap-1.5 whitespace-nowrap ${
              adminTab === 'cuentas' ? 'bg-brand text-ink' : 'text-muted hover:text-ink'
            }`}
          >
            <Users className="w-3.5 h-3.5" />
            <span>Vigencia Cuentas</span>
          </button>

          <button
            onClick={() => setAdminTab('auditoria')}
            className={`px-3 py-1.5 rounded-xl font-semibold transition-colors flex items-center gap-1.5 whitespace-nowrap ${
              adminTab === 'auditoria' ? 'bg-brand text-ink' : 'text-muted hover:text-ink'
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
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 p-4 rounded-2xl bg-panel border border-line">
            <div>
              <h3 className="text-base font-bold text-ink flex items-center gap-2">
                <CreditCard className="w-4 h-4 text-emerald-700" />
                <span>Validación de Pagos Bancarios & Detección de Duplicados</span>
              </h3>
              <p className="text-xs text-muted">
                Aprobación habilita inmediatamente la matrícula y extiende la vigencia del usuario por 2 años.
              </p>
            </div>

            {/* Filter pills */}
            <div className="flex items-center gap-1.5 text-xs">
              <span className="text-muted">Filtrar:</span>
              {['todos', 'PENDIENTE', 'EN_REVISIÓN', 'APROBADO', 'DUPLICADO', 'RECHAZADO'].map(st => (
                <button
                  key={st}
                  onClick={() => setPaymentFilter(st)}
                  className={`px-2.5 py-1 rounded-lg font-medium transition-colors ${
                    paymentFilter === st ? 'bg-brand text-ink font-bold' : 'bg-canvas text-muted hover:text-ink'
                  }`}
                >
                  {st}
                </button>
              ))}
            </div>
          </div>

          {/* Payments Table */}
          <div className="bg-panel border border-line rounded-3xl overflow-hidden shadow-xl">
            <div className="responsive-table overflow-x-auto" tabIndex={0} role="region" aria-label="Tabla desplazable">
              <table className="w-full text-left text-xs text-muted">
                <thead className="bg-canvas text-muted font-semibold border-b border-line">
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
                <tbody className="divide-y divide-line/80">
                  {filteredPayments.map((p) => (
                    <tr key={p.id} className="hover:bg-surface-high/50 transition-colors">
                      <td className="p-4">
                        <div className="font-mono font-bold text-brand">{p.orderCode}</div>
                        <div className="text-[10px] text-muted">{new Date(p.registeredAt).toLocaleDateString()}</div>
                      </td>
                      <td className="p-4">
                        <div className="font-medium text-ink">{p.userName}</div>
                        <div className="text-[11px] text-muted">Titular: {p.titularTransferencia}</div>
                        <div className="text-[10px] text-muted">{p.userEmail}</div>
                      </td>
                      <td className="p-4">
                        <div className="font-semibold text-ink">{p.banco}</div>
                        <div className="font-mono text-cyan-700 text-xs">Op: {p.numeroOperacion}</div>
                      </td>
                      <td className="p-4 font-bold text-ink font-mono text-sm">
                        S/. {p.monto.toFixed(2)}
                      </td>
                      <td className="p-4 max-w-xs">
                        <div className="font-mono text-[9px] text-muted truncate bg-canvas p-1.5 rounded border border-line" title={p.paymentFingerprint}>
                          {p.paymentFingerprint}
                        </div>
                        {p.rejectionReason && (
                          <div className="text-[10px] text-rose-700 mt-1">
                            {p.rejectionReason}
                          </div>
                        )}
                      </td>
                      <td className="p-4">
                        <span className={`px-2.5 py-0.5 rounded-full border text-[10px] font-bold ${
                          p.status === 'APROBADO' ? 'bg-emerald-50 text-emerald-700 border-emerald-200' :
                          p.status === 'DUPLICADO' ? 'bg-purple-50 text-purple-700 border-purple-200 animate-pulse font-extrabold' :
                          p.status === 'RECHAZADO' ? 'bg-rose-50 text-rose-700 border-rose-200' :
                          p.status === 'OBSERVADO' ? 'bg-orange-50 text-orange-700 border-orange-200' :
                          'bg-brand text-brand border-brand'
                        }`}>
                          {p.status}
                        </span>
                      </td>
                      <td className="p-4 text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          {p.status !== 'APROBADO' && (
                            <button
                              onClick={() => reviewPayment(p.id, 'APROBADO')}
                              className="px-2.5 py-1 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-ink font-bold text-[11px] transition-colors"
                              title="Aprobar pago y generar matrícula"
                            >
                              Aprobar
                            </button>
                          )}
                          <button
                            onClick={() => setActionPaymentModal({ paymentId: p.id, action: 'OBSERVAR' })}
                            className="px-2 py-1 rounded-lg bg-surface-low hover:bg-surface-high text-muted text-[11px]"
                            title="Observar información"
                          >
                            Observar
                          </button>
                          <button
                            onClick={() => setActionPaymentModal({ paymentId: p.id, action: 'RECHAZAR' })}
                            className="px-2 py-1 rounded-lg bg-rose-50 hover:bg-rose-50 border border-rose-200 text-rose-700 text-[11px]"
                            title="Rechazar pago"
                          >
                            Rechazar
                          </button>
                          <button
                            onClick={() => setActionPaymentModal({ paymentId: p.id, action: 'DUPLICADO' })}
                            className="px-2 py-1 rounded-lg bg-purple-50 hover:bg-purple-50 border border-purple-200 text-purple-700 text-[11px]"
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
          <div className="p-5 rounded-3xl bg-panel border border-line space-y-4">
            <h3 className="font-bold text-sm text-ink">Seguimiento Académico, Intentos y Calificaciones (Sección 12)</h3>
            <div className="responsive-table overflow-x-auto" tabIndex={0} role="region" aria-label="Tabla desplazable">
              <table className="w-full text-left text-xs text-muted">
                <thead className="bg-canvas text-muted font-semibold border-b border-line">
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
                <tbody className="divide-y divide-line/80">
                  {enrollments.map((enr) => {
                    const u = allUsers.find(x => x.id === enr.userId);
                    const c = courses.find(x => x.id === enr.courseId);
                    return (
                      <tr key={enr.id} className="hover:bg-surface-high/40">
                        <td className="p-3 font-medium text-ink">{u?.nombres} {u?.apellidos}</td>
                        <td className="p-3">{c?.title}</td>
                        <td className="p-3">{enr.progressPercent}%</td>
                        <td className="p-3 font-mono">{enr.firstAttemptScore !== undefined ? `${enr.firstAttemptScore}/20` : 'Pendiente'}</td>
                        <td className="p-3 font-mono">{enr.secondAttemptScore !== undefined ? `${enr.secondAttemptScore}/20` : '-'}</td>
                        <td className="p-3 font-mono font-bold text-brand">{enr.finalScore !== undefined ? `${enr.finalScore}/20` : '-'}</td>
                        <td className="p-3">
                          <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                            enr.examStatus === 'APROBADA' ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' :
                            enr.examStatus === 'DESAPROBADA' ? 'bg-rose-50 text-rose-700 border border-rose-200' :
                            'bg-surface-low text-muted'
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
          <div className="p-5 rounded-3xl bg-panel border border-line space-y-4">
            <h3 className="font-bold text-sm text-ink">Certificados Digitales Emitidos y Control de Revocación (Sección 13)</h3>
            <div className="responsive-table overflow-x-auto" tabIndex={0} role="region" aria-label="Tabla desplazable">
              <table className="w-full text-left text-xs text-muted">
                <thead className="bg-canvas text-muted font-semibold border-b border-line">
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
                <tbody className="divide-y divide-line/80">
                  {certificates.map((cert) => (
                    <tr key={cert.id} className="hover:bg-surface-high/40">
                      <td className="p-3 font-mono font-bold text-cyan-700">{cert.certificateCode}</td>
                      <td className="p-3 font-medium text-ink">{cert.studentName}</td>
                      <td className="p-3">{cert.courseTitle}</td>
                      <td className="p-3 font-mono font-bold text-brand">{cert.finalScore}/20</td>
                      <td className="p-3">{cert.issueDate}</td>
                      <td className="p-3">
                        <span className={`px-2.5 py-0.5 rounded-full text-[10px] font-bold border ${
                          cert.status === 'VÁLIDO' ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-rose-50 text-rose-700 border-rose-200'
                        }`}>
                          {cert.status}
                        </span>
                      </td>
                      <td className="p-3 text-right">
                        <button
                          onClick={() => setActionCertModal(cert.id)}
                          className={`px-3 py-1 rounded-lg text-xs font-semibold ${
                            cert.status === 'VÁLIDO'
                              ? 'bg-rose-50 hover:bg-rose-50 border border-rose-200 text-rose-700'
                              : 'bg-emerald-50 hover:bg-emerald-50 border border-emerald-200 text-emerald-700'
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
          <div className="p-4 rounded-2xl bg-panel border border-line">
            <h3 className="font-bold text-base text-ink">Gestión de Tickets y Consultas de Usuarios (Sección 15.2)</h3>
            <p className="text-xs text-muted">Atención y resolución con trazabilidad y estados: ABIERTO, EN_ATENCIÓN, RESPONDIDO, CERRADO</p>
          </div>

          <div className="grid grid-cols-1 gap-4">
            {tickets.map((t) => (
              <div key={t.id} className="p-5 rounded-2xl bg-panel border border-line space-y-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="font-mono font-bold text-brand text-xs">{t.ticketCode}</span>
                    <span className="text-[10px] px-2 py-0.5 rounded bg-canvas text-muted border border-line">{t.category}</span>
                    <span className="text-xs text-muted">• De: <strong className="text-ink">{t.userName}</strong> ({t.userEmail})</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className={`text-[10px] px-2.5 py-0.5 rounded-full font-bold ${
                      t.status === 'RESPONDIDO' ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' :
                      t.status === 'CERRADO' ? 'bg-surface-low text-muted' :
                      'bg-brand text-brand border border-brand'
                    }`}>
                      {t.status}
                    </span>
                    <select
                      value={t.status}
                      onChange={(e) => updateTicketStatus(t.id, e.target.value as any)}
                      className="bg-canvas border border-line text-xs text-muted rounded px-2 py-1"
                    >
                      <option value="ABIERTO">ABIERTO</option>
                      <option value="EN_ATENCIÓN">EN_ATENCIÓN</option>
                      <option value="RESPONDIDO">RESPONDIDO</option>
                      <option value="CERRADO">CERRADO</option>
                    </select>
                  </div>
                </div>

                <div className="font-bold text-sm text-ink">{t.subject}</div>
                <p className="text-xs text-muted bg-canvas p-3 rounded-xl border border-slate-850">
                  {t.message}
                </p>

                {/* Responses */}
                {t.responses.length > 0 && (
                  <div className="space-y-2 pt-2 border-t border-slate-850">
                    {t.responses.map((r, i) => (
                      <div key={i} className="p-2.5 rounded-lg bg-emerald-50/20 border border-emerald-200/40 text-xs space-y-0.5">
                        <div className="flex items-center justify-between text-[10px] text-muted">
                          <span className="font-semibold text-emerald-700">{r.author}</span>
                          <span>{new Date(r.timestamp).toLocaleString()}</span>
                        </div>
                        <p className="text-ink">{r.message}</p>
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
                      className="w-full bg-canvas border border-line rounded-xl p-2.5 text-xs text-ink resize-none"
                    />
                    <div className="flex justify-end gap-2">
                      <button
                        onClick={() => setReplyTicketId(null)}
                        className="px-3 py-1 rounded-lg text-muted text-xs"
                      >
                        Cancelar
                      </button>
                      <button
                        onClick={() => handleSendTicketReply(t.id)}
                        className="px-4 py-1.5 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-ink font-bold text-xs flex items-center gap-1.5"
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
                      className="px-3 py-1.5 rounded-lg bg-surface-low hover:bg-surface-high text-brand text-xs font-semibold"
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
        <form onSubmit={handleSaveSocials} className="p-6 rounded-3xl bg-panel border border-line space-y-6">
          <div className="flex items-center justify-between pb-4 border-b border-line">
            <div>
              <h3 className="text-base font-bold text-ink flex items-center gap-2">
                <Share2 className="w-5 h-5 text-brand" />
                <span>Gestión de Enlaces de Redes Sociales (Sección 15.1)</span>
              </h3>
              <p className="text-xs text-muted mt-0.5">
                Regla técnica: Los enlaces deben ser administrables sin modificar código y abrir en pestaña nueva.
              </p>
            </div>
            {socialsSaved && (
              <span className="text-xs font-bold text-emerald-700 bg-emerald-50 px-3 py-1 rounded-full border border-emerald-200">
                ✓ Guardado exitosamente
              </span>
            )}
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 text-xs">
            {/* WhatsApp */}
            <div className="p-4 rounded-xl bg-canvas border border-line space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-bold text-emerald-700">WhatsApp de Soporte</span>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editableSocials.whatsapp.enabled}
                    onChange={(e) => setEditableSocials(prev => ({
                      ...prev,
                      whatsapp: { ...prev.whatsapp, enabled: e.target.checked }
                    }))}
                    className="rounded text-brand focus:ring-0"
                  />
                  <span className="text-[11px] text-muted">Activo</span>
                </label>
              </div>
              <input
                type="text"
                value={editableSocials.whatsapp.number}
                onChange={(e) => setEditableSocials(prev => ({
                  ...prev,
                  whatsapp: { ...prev.whatsapp, number: e.target.value }
                }))}
                className="w-full bg-panel border border-line rounded-lg p-2 text-ink"
                placeholder="+51 987 654 321"
              />
            </div>

            {/* Facebook */}
            <div className="p-4 rounded-xl bg-canvas border border-line space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-bold text-ink">Facebook</span>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editableSocials.facebook.enabled}
                    onChange={(e) => setEditableSocials(prev => ({
                      ...prev,
                      facebook: { ...prev.facebook, enabled: e.target.checked }
                    }))}
                    className="rounded text-brand focus:ring-0"
                  />
                  <span className="text-[11px] text-muted">Activo</span>
                </label>
              </div>
              <input
                type="text"
                value={editableSocials.facebook.url}
                onChange={(e) => setEditableSocials(prev => ({
                  ...prev,
                  facebook: { ...prev.facebook, url: e.target.value }
                }))}
                className="w-full bg-panel border border-line rounded-lg p-2 text-ink"
              />
            </div>

            {/* Instagram */}
            <div className="p-4 rounded-xl bg-canvas border border-line space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-bold text-ink">Instagram</span>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editableSocials.instagram.enabled}
                    onChange={(e) => setEditableSocials(prev => ({
                      ...prev,
                      instagram: { ...prev.instagram, enabled: e.target.checked }
                    }))}
                  />
                  <span className="text-[11px] text-muted">Activo</span>
                </label>
              </div>
              <input
                type="text"
                value={editableSocials.instagram.url}
                onChange={(e) => setEditableSocials(prev => ({
                  ...prev,
                  instagram: { ...prev.instagram, url: e.target.value }
                }))}
                className="w-full bg-panel border border-line rounded-lg p-2 text-ink"
              />
            </div>

            {/* YouTube */}
            <div className="p-4 rounded-xl bg-canvas border border-line space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-bold text-ink">YouTube</span>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editableSocials.youtube.enabled}
                    onChange={(e) => setEditableSocials(prev => ({
                      ...prev,
                      youtube: { ...prev.youtube, enabled: e.target.checked }
                    }))}
                  />
                  <span className="text-[11px] text-muted">Activo</span>
                </label>
              </div>
              <input
                type="text"
                value={editableSocials.youtube.url}
                onChange={(e) => setEditableSocials(prev => ({
                  ...prev,
                  youtube: { ...prev.youtube, url: e.target.value }
                }))}
                className="w-full bg-panel border border-line rounded-lg p-2 text-ink"
              />
            </div>

            {/* TikTok */}
            <div className="p-4 rounded-xl bg-canvas border border-line space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-bold text-ink">TikTok</span>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editableSocials.tiktok.enabled}
                    onChange={(e) => setEditableSocials(prev => ({
                      ...prev,
                      tiktok: { ...prev.tiktok, enabled: e.target.checked }
                    }))}
                  />
                  <span className="text-[11px] text-muted">Activo</span>
                </label>
              </div>
              <input
                type="text"
                value={editableSocials.tiktok.url}
                onChange={(e) => setEditableSocials(prev => ({
                  ...prev,
                  tiktok: { ...prev.tiktok, url: e.target.value }
                }))}
                className="w-full bg-panel border border-line rounded-lg p-2 text-ink"
              />
            </div>

            {/* LinkedIn */}
            <div className="p-4 rounded-xl bg-canvas border border-line space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-bold text-ink">LinkedIn</span>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editableSocials.linkedin.enabled}
                    onChange={(e) => setEditableSocials(prev => ({
                      ...prev,
                      linkedin: { ...prev.linkedin, enabled: e.target.checked }
                    }))}
                  />
                  <span className="text-[11px] text-muted">Activo</span>
                </label>
              </div>
              <input
                type="text"
                value={editableSocials.linkedin.url}
                onChange={(e) => setEditableSocials(prev => ({
                  ...prev,
                  linkedin: { ...prev.linkedin, url: e.target.value }
                }))}
                className="w-full bg-panel border border-line rounded-lg p-2 text-ink"
              />
            </div>
          </div>

          <div className="pt-2 flex justify-end">
            <button
              type="submit"
              className="px-6 py-2.5 rounded-xl bg-brand hover:bg-brand text-ink font-bold text-xs flex items-center gap-2 shadow-lg shadow-brand/20"
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
        <div className="p-6 rounded-3xl bg-panel border border-line space-y-4 shadow-xl">
          <div className="flex items-center justify-between pb-3 border-b border-line">
            <div>
              <h3 className="font-bold text-base text-ink">Directorio de Cuentas y Control de Vigencia (Sección 4.3)</h3>
              <p className="text-xs text-muted">
                Regla: 30 días sin compra aprobada = Proceso de eliminación • 1er pago aprobado = 2 años de vigencia
              </p>
            </div>
          </div>

          <div className="responsive-table overflow-x-auto" tabIndex={0} role="region" aria-label="Tabla desplazable">
            <table className="w-full text-left text-xs text-muted">
              <thead className="bg-canvas text-muted font-semibold border-b border-line">
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
              <tbody className="divide-y divide-line/80">
                {allUsers.map((u) => {
                  const days = Math.ceil((new Date(u.validUntil).getTime() - Date.now()) / (1000 * 60 * 60 * 24));
                  return (
                    <tr key={u.id} className="hover:bg-surface-high/40">
                      <td className="p-3">
                        <div className="font-bold text-ink">{u.nombres} {u.apellidos}</div>
                        <div className="text-[10px] text-muted">{u.email}</div>
                      </td>
                      <td className="p-3 font-mono text-cyan-700">{u.role}</td>
                      <td className="p-3">{new Date(u.createdAt).toLocaleDateString()}</td>
                      <td className="p-3">
                        {u.hasApprovedPurchase ? (
                          <span className="text-emerald-700 font-semibold">✓ Con Compras (Plan 2 años)</span>
                        ) : (
                          <span className="text-brand font-semibold">Sin Compras (Ventana 30d)</span>
                        )}
                      </td>
                      <td className="p-3 font-mono">{new Date(u.validUntil).toLocaleDateString()}</td>
                      <td className="p-3 font-mono font-bold">
                        <span className={days <= 7 ? 'text-rose-700' : 'text-emerald-700'}>
                          {days > 0 ? `${days} días` : '0 días (Vencida)'}
                        </span>
                      </td>
                      <td className="p-3">
                        <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                          u.accountStatus === 'ACTIVA' ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' :
                          u.accountStatus === 'PENDIENTE_DE_ELIMINACIÓN' ? 'bg-rose-50 text-rose-700 border border-rose-200 animate-pulse' :
                          'bg-indigo-50 text-indigo-700 border border-indigo-200'
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
        <div className="p-6 rounded-3xl bg-panel border border-line space-y-4 shadow-xl">
          <div className="flex items-center justify-between pb-3 border-b border-line">
            <div>
              <h3 className="font-bold text-base text-ink flex items-center gap-2">
                <History className="w-5 h-5 text-brand" />
                <span>Registro Inmutable de Auditoría & Trazabilidad (Sección 21)</span>
              </h3>
              <p className="text-xs text-muted">
                Bitácora de eventos críticos: logins, órdenes, pagos, detección de fraude, notas y certificados
              </p>
            </div>
            <span className="text-xs font-mono text-muted">{auditLogs.length} eventos registrados</span>
          </div>

          <div className="divide-y divide-line/80 max-h-[550px] overflow-y-auto">
            {auditLogs.map((log) => {
              const actionColors: Record<string, string> = {
                APROBAR_PAGO: 'text-emerald-700 bg-emerald-50 border-emerald-200',
                DETECTAR_DUPLICADO: 'text-rose-700 bg-rose-50 border-rose-200 font-bold',
                RECHAZAR_PAGO: 'text-rose-700 bg-rose-50 border-rose-200',
                EMITIR_CERTIFICADO: 'text-cyan-700 bg-cyan-50 border-cyan-200',
                REVOCAR_CERTIFICADO: 'text-purple-700 bg-purple-50 border-purple-200',
                CREAR_ORDEN: 'text-brand bg-brand border-brand',
                REGISTRAR_PAGO: 'text-blue-700 bg-blue-50 border-blue-200',
                LOGIN: 'text-muted bg-surface-low border-line',
              };

              return (
                <div key={log.id} className="py-3 flex items-start gap-3 text-xs hover:bg-surface-high/40 px-2 rounded-lg transition-colors">
                  <span className="font-mono text-[11px] text-muted shrink-0 w-36">
                    {new Date(log.timestamp).toLocaleString()}
                  </span>
                  <span className={`px-2 py-0.5 rounded text-[10px] border shrink-0 font-mono ${actionColors[log.action] || 'bg-surface-low text-muted border-line'}`}>
                    {log.action}
                  </span>
                  <div className="flex-1 min-w-0">
                    <span className="font-semibold text-ink mr-1.5">{log.userName} ({log.userRole}):</span>
                    <span className="text-muted">{log.details}</span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Payment Action Modal (Observar / Rechazar / Duplicado) */}
      {actionPaymentModal && (
        <div className="fixed inset-0 z-50 bg-canvas/80 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-panel border border-line rounded-2xl max-w-md w-full p-6 space-y-4 shadow-xl">
            <h3 className="font-bold text-sm text-ink">
              Marcar Pago como {actionPaymentModal.action}
            </h3>
            <p className="text-xs text-muted">
              Ingresa el motivo administrativo o justificación para el usuario y el registro de auditoría.
            </p>
            <textarea
              rows={3}
              required
              placeholder="Indica el motivo..."
              value={actionReason}
              onChange={(e) => setActionReason(e.target.value)}
              className="w-full bg-canvas border border-line rounded-xl p-2.5 text-xs text-ink resize-none"
            />
            <div className="flex justify-end gap-2">
              <button
                onClick={() => setActionPaymentModal(null)}
                className="px-3 py-1.5 rounded-lg text-muted text-xs"
              >
                Cancelar
              </button>
              <button
                onClick={handleConfirmPaymentAction}
                className="px-4 py-1.5 rounded-lg bg-brand hover:bg-brand text-ink font-bold text-xs"
              >
                Confirmar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Certificate Revocation Modal */}
      {actionCertModal && (
        <div className="fixed inset-0 z-50 bg-canvas/80 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-panel border border-line rounded-2xl max-w-md w-full p-6 space-y-4 shadow-xl">
            <h3 className="font-bold text-sm text-ink">
              Cambiar Estado de Certificado
            </h3>
            <p className="text-xs text-muted">
              Ingresa el motivo pericial de revocación o reactivación que se reflejará públicamente al escanear el QR:
            </p>
            <input
              type="text"
              placeholder="Ej. Incumplimiento ético pericial / Rectificación"
              value={certRevocationReason}
              onChange={(e) => setCertRevocationReason(e.target.value)}
              className="w-full bg-canvas border border-line rounded-xl p-2.5 text-xs text-ink"
            />
            <div className="flex justify-end gap-2">
              <button
                onClick={() => setActionCertModal(null)}
                className="px-3 py-1.5 rounded-lg text-muted text-xs"
              >
                Cancelar
              </button>
              <button
                onClick={handleConfirmCertRevocation}
                className="px-4 py-1.5 rounded-lg bg-rose-600 hover:bg-rose-500 text-ink font-bold text-xs"
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
