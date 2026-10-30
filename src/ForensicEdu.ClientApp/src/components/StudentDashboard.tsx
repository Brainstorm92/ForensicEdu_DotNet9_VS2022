import React, { useState } from 'react';
import { useApp } from '../context/AppContext';
import {
  BookOpen,
  Calendar,
  Award,
  CreditCard,
  Clock,
  User,
  MessageSquare,
  ExternalLink,
  Play,
  AlertTriangle,
  CheckCircle2,
  FileText,
  AlertCircle,
  Download,
  Share2,
  RefreshCw,
  PhoneCall
} from 'lucide-react';
import { VirtualClassroom } from './VirtualClassroom';
import { CertificateView } from './CertificateView';

interface StudentDashboardProps {
  currentSubTab: 'mis-cursos' | 'mis-eventos' | 'mis-certificados' | 'mis-pagos' | 'asesoria' | 'mis-consultas' | 'perfil';
}

export const StudentDashboard: React.FC<StudentDashboardProps> = ({ currentSubTab }) => {
  const {
    currentUser,
    courses,
    events,
    advisory,
    enrollments,
    orders,
    payments,
    certificates,
    tickets,
    createTicket,
    selectedCourseId,
    setSelectedCourseId,
    setActiveTab,
    setPublicVerificationCode,
    updateCurrentUserProfile,
    simulateValidityChange
  } = useApp();

  const [selectedCertForView, setSelectedCertForView] = useState<string | null>(null);

  // New ticket state
  const [ticketSubject, setTicketSubject] = useState('');
  const [ticketCategory, setTicketCategory] = useState<'Pagos' | 'Acceso a Curso' | 'Evaluación' | 'Certificado' | 'Asesoría' | 'General'>('General');
  const [ticketMessage, setTicketMessage] = useState('');
  const [newTicketSuccess, setNewTicketSuccess] = useState(false);

  // If a course is selected for the virtual classroom view:
  if (selectedCourseId) {
    return (
      <VirtualClassroom
        courseId={selectedCourseId}
        onBack={() => setSelectedCourseId(null)}
      />
    );
  }

  // If a certificate is selected for the luxury view:
  if (selectedCertForView) {
    const cert = certificates.find(c => c.id === selectedCertForView);
    if (cert) {
      return (
        <CertificateView
          certificate={cert}
          onBack={() => setSelectedCertForView(null)}
        />
      );
    }
  }

  const myEnrollments = enrollments.filter(e => e.userId === currentUser.id);
  const myPayments = payments.filter(p => p.userId === currentUser.id);
  const myCertificates = certificates.filter(c => c.userId === currentUser.id);
  const myTickets = tickets.filter(t => t.userId === currentUser.id);

  // Calculate validity days
  const validDate = new Date(currentUser.validUntil);
  const diffDays = Math.ceil((validDate.getTime() - Date.now()) / (1000 * 60 * 60 * 24));

  const handleCreateTicket = (e: React.FormEvent) => {
    e.preventDefault();
    if (!ticketSubject.trim() || !ticketMessage.trim()) return;
    createTicket(ticketSubject, ticketCategory, ticketMessage);
    setTicketSubject('');
    setTicketMessage('');
    setNewTicketSuccess(true);
    setTimeout(() => setNewTicketSuccess(false), 4000);
  };

  return (
    <div className="responsive-page w-full min-w-0 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8 animate-in fade-in duration-300">
      {/* =========================================================================
          TAB: MIS CURSOS (AULA VIRTUAL Y EVALUACIONES)
         ========================================================================= */}
      {currentSubTab === 'mis-cursos' && (
        <div className="space-y-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
            <div>
              <h2 className="text-2xl font-bold text-ink flex items-center gap-2.5">
                <BookOpen className="w-6 h-6 text-brand" />
                <span>Mis Cursos Matriculados (Sección 10)</span>
              </h2>
              <p className="text-xs text-muted">
                Acceso al aula virtual, materiales descargables con marca de agua y evaluaciones cronometradas
              </p>
            </div>
            <button
              onClick={() => setActiveTab('catalogo')}
              className="px-4 py-2 rounded-xl bg-surface-low hover:bg-surface-high text-ink text-xs font-semibold w-fit"
            >
              + Explorar Más Cursos
            </button>
          </div>

          {myEnrollments.length === 0 ? (
            <div className="p-12 rounded-3xl bg-panel border border-line text-center space-y-3">
              <BookOpen className="w-12 h-12 text-muted mx-auto" />
              <h3 className="font-bold text-base text-ink">No tienes cursos matriculados aún</h3>
              <p className="text-xs text-muted max-w-md mx-auto">
                Conforme a la Sección 10, la matrícula se genera automáticamente una vez que el pago bancario correspondiente es aprobado por administración.
              </p>
              <button
                onClick={() => setActiveTab('catalogo')}
                className="px-5 py-2.5 rounded-xl bg-brand hover:bg-brand text-ink font-bold text-xs"
              >
                Ver Catálogo de Especialidades
              </button>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {myEnrollments.map((enr) => {
                const c = courses.find(x => x.id === enr.courseId);
                if (!c) return null;
                return (
                  <div
                    key={enr.id}
                    className="p-5 rounded-2xl bg-panel border border-line hover:border-line transition-all flex flex-col justify-between space-y-4 shadow-xl"
                  >
                    <div className="space-y-3">
                      <div className="flex items-center justify-between">
                        <span className="text-[10px] font-mono font-bold px-2 py-0.5 rounded bg-canvas text-brand border border-brand/30">
                          {c.code}
                        </span>
                        <span className={`text-[10px] px-2 py-0.5 rounded-full font-bold ${
                          enr.examStatus === 'APROBADA'
                            ? 'bg-emerald-50 text-emerald-700 border border-emerald-200'
                            : enr.examStatus === 'DESAPROBADA'
                            ? 'bg-rose-50 text-rose-700 border border-rose-200'
                            : 'bg-indigo-50 text-indigo-700 border border-indigo-200'
                        }`}>
                          {enr.examStatus === 'APROBADA' ? `Aprobado (${enr.finalScore}/20)` : enr.examStatus === 'DESAPROBADA' ? `Desaprobado (${enr.finalScore}/20)` : 'En Estudio'}
                        </span>
                      </div>

                      <h3 className="font-bold text-base text-ink">{c.title}</h3>
                      <p className="text-xs text-muted line-clamp-2">{c.shortDescription}</p>

                      {/* Progress bar */}
                      <div className="space-y-1.5 pt-1">
                        <div className="flex items-center justify-between text-[11px] text-muted font-medium">
                          <span>Avance de Módulos</span>
                          <span className="text-brand font-bold">{enr.progressPercent}%</span>
                        </div>
                        <div className="w-full h-1.5 rounded-full bg-surface-low overflow-hidden">
                          <div
                            className="h-full bg-brand"
                            style={{ width: `${enr.progressPercent}%` }}
                          ></div>
                        </div>
                      </div>
                    </div>

                    {/* Evaluation status indicator */}
                    <div className="p-2.5 rounded-xl bg-canvas border border-line text-[11px] flex items-center justify-between">
                      <div className="flex items-center gap-1.5 text-muted">
                        <Clock className="w-3.5 h-3.5 text-brand" />
                        <span>Evaluación 10 min:</span>
                      </div>
                      <span className={`font-semibold ${enr.examStatus === 'APROBADA' ? 'text-emerald-700' : enr.examStatus === 'DESAPROBADA' ? 'text-rose-700' : 'text-brand'}`}>
                        {enr.examStatus === 'APROBADA' ? `Aprobado (${enr.finalScore}/20)` : enr.attemptsUsed > 0 ? `Intento ${enr.attemptsUsed}/2 (${enr.finalScore}/20)` : 'Pendiente (Mín. 13)'}
                      </span>
                    </div>

                    <div className="pt-2 border-t border-line/80 flex flex-wrap gap-2">
                      <button
                        onClick={() => setSelectedCourseId(c.id)}
                        className="flex-1 py-2 px-3 rounded-xl bg-brand hover:bg-brand text-ink font-bold text-xs flex items-center justify-center gap-1.5 shadow-md shadow-brand/20"
                      >
                        <Play className="w-3.5 h-3.5" />
                        <span>Aula & Examen</span>
                      </button>

                      {enr.certificateId ? (
                        <button
                          onClick={() => setSelectedCertForView(enr.certificateId!)}
                          className="py-2 px-3 rounded-xl bg-emerald-50 hover:bg-emerald-50 border border-emerald-200 text-emerald-700 font-semibold text-xs flex items-center gap-1"
                          title="Ver Certificado QR"
                        >
                          <Award className="w-4 h-4" />
                          <span>Certificado QR</span>
                        </button>
                      ) : (
                        <button
                          onClick={() => setSelectedCourseId(c.id)}
                          className="py-2 px-3 rounded-xl bg-indigo-50 hover:bg-indigo-50 border border-indigo-200 text-indigo-700 font-semibold text-xs flex items-center gap-1"
                          title="Rendir Examen de 10 min"
                        >
                          <Clock className="w-4 h-4 text-brand" />
                          <span>Rendir Test</span>
                        </button>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      )}

      {/* =========================================================================
          TAB: MIS EVENTOS (EN VIVO)
         ========================================================================= */}
      {currentSubTab === 'mis-eventos' && (
        <div className="space-y-6">
          <div>
            <h2 className="text-2xl font-bold text-ink flex items-center gap-2.5">
              <Calendar className="w-6 h-6 text-brand" />
              <span>Mis Eventos y Congresos en Vivo (Sección 14)</span>
            </h2>
            <p className="text-xs text-muted">
              Salas de transmisión en tiempo real habilitadas 15 minutos antes de la hora programada
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {events.map((evt) => (
              <div
                key={evt.id}
                className="p-5 rounded-2xl bg-panel border border-line flex flex-col justify-between space-y-4"
              >
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <span className="text-[10px] uppercase font-bold px-2 py-0.5 rounded bg-indigo-50 text-indigo-700 border border-indigo-200">
                      {evt.type}
                    </span>
                    <span className="text-xs font-semibold text-brand">{evt.date} • {evt.time}</span>
                  </div>

                  <h3 className="font-bold text-base text-ink">{evt.title}</h3>
                  <p className="text-xs text-muted">{evt.description}</p>
                  <div className="text-[11px] text-muted">Expositor: <strong>{evt.speaker}</strong></div>
                </div>

                <div className="p-3 rounded-xl bg-canvas border border-line text-xs flex items-center justify-between">
                  <div className="flex items-center gap-2 text-muted">
                    <Clock className="w-4 h-4 text-brand" />
                    <span>Transmisión en directo (Zoom Webinar / RTMP)</span>
                  </div>
                  <a
                    href={evt.roomUrl || '#'}
                    target="_blank"
                    rel="noreferrer"
                    className="px-3 py-1.5 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-ink font-bold text-xs flex items-center gap-1 shadow-md shadow-emerald-950"
                  >
                    <span>Ingresar a Sala</span>
                    <ExternalLink className="w-3.5 h-3.5" />
                  </a>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* =========================================================================
          TAB: ASESORÍA PERICIAL INDEPENDIENTE (SECCIÓN 6)
         ========================================================================= */}
      {currentSubTab === 'asesoria' && (
        <div className="space-y-6">
          <div>
            <h2 className="text-2xl font-bold text-ink flex items-center gap-2.5">
              <PhoneCall className="w-6 h-6 text-emerald-700" />
              <span>Asesoría Personalizada y Consultoría Pericial (Sección 6)</span>
            </h2>
            <p className="text-xs text-muted">
              Servicio independiente de los cursos. Atención directa mediante WhatsApp con el consultor asignado.
            </p>
          </div>

          <div className="p-6 sm:p-8 rounded-3xl bg-panel border border-line space-y-6">
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 pb-6 border-b border-line">
              <div className="space-y-2">
                <span className="text-[10px] font-bold uppercase tracking-wider px-2 py-0.5 rounded bg-emerald-500/20 text-emerald-700 border border-emerald-500/30">
                  Consultor Oficial Asignado
                </span>
                <h3 className="text-xl font-bold text-ink">{advisory.specialist}</h3>
                <p className="text-xs text-brand font-medium">{advisory.specialty}</p>
                <p className="text-xs text-muted max-w-xl">
                  {advisory.description}
                </p>
              </div>

              <div className="p-4 rounded-2xl bg-canvas border border-line text-center space-y-2 shrink-0">
                <div className="text-[11px] text-muted">Canal de Atención Exclusivo:</div>
                <a
                  href={`https://wa.me/${advisory.whatsappContact.replace(/\D/g, '')}?text=Hola%20Cmdte.%20Sotomayor,%20tengo%20una%20consulta%20pericial%20referente%20a%20mi%20caso.`}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="px-5 py-2.5 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-ink font-bold text-xs flex items-center justify-center gap-2 shadow-lg shadow-emerald-950"
                >
                  <MessageSquare className="w-4 h-4" />
                  <span>Contactar por WhatsApp</span>
                </a>
                <div className="text-[10px] text-muted">{advisory.whatsappContact}</div>
              </div>
            </div>

            {/* Protocol & Business Rules */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-xs text-muted">
              <div className="p-4 rounded-xl bg-canvas border border-line space-y-1">
                <div className="font-bold text-ink">1. Asignación y Registro</div>
                <p className="text-muted text-[11px]">
                  El sistema registra la asesoría adquirida, el consultor asignado, fecha, estado y trazabilidad.
                </p>
              </div>
              <div className="p-4 rounded-xl bg-canvas border border-line space-y-1">
                <div className="font-bold text-ink">2. Agenda y Documentos</div>
                <p className="text-muted text-[11px]">
                  El estudiante puede enviar expedientes para lectura pericial y preparación previa de juicio oral.
                </p>
              </div>
              <div className="p-4 rounded-xl bg-canvas border border-line space-y-1">
                <div className="font-bold text-ink">3. Trazabilidad Judicial</div>
                <p className="text-muted text-[11px]">
                  Se emite informe de retroalimentación pericial al concluir la sesión de consultoría.
                </p>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* =========================================================================
          TAB: MIS CERTIFICADOS (CON CÓDIGO ÚNICO Y QR PÚBLICO - SECCIÓN 13)
         ========================================================================= */}
      {currentSubTab === 'mis-certificados' && (
        <div className="space-y-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
            <div>
              <h2 className="text-2xl font-bold text-ink flex items-center gap-2.5">
                <Award className="w-6 h-6 text-brand" />
                <span>Mis Certificados Digitales con QR (Sección 13)</span>
              </h2>
              <p className="text-xs text-muted">
                Certificaciones emitidas para notas aprobatorias ≥ 13/20 con verificación pública inviolable
              </p>
            </div>
            <button
              onClick={() => setActiveTab('verificador-publico')}
              className="px-4 py-2 rounded-xl bg-cyan-50 border border-cyan-200 text-cyan-700 text-xs font-semibold hover:bg-cyan-50 transition-colors flex items-center gap-1.5 w-fit"
            >
              <ExternalLink className="w-3.5 h-3.5" />
              <span>Abrir Validador QR Público</span>
            </button>
          </div>

          {myCertificates.length === 0 ? (
            <div className="p-12 rounded-3xl bg-panel border border-line text-center space-y-3">
              <Award className="w-12 h-12 text-muted mx-auto" />
              <h3 className="font-bold text-base text-ink">Aún no cuentas con certificados emitidos</h3>
              <p className="text-xs text-muted max-w-md mx-auto">
                Conforme a la Sección 12 y 13, solo se emite certificado si el estudiante obtiene una nota de <strong className="text-ink">13/20 o superior</strong> en la evaluación del curso.
              </p>
              <button
                onClick={() => setActiveTab('mis-cursos')}
                className="px-5 py-2.5 rounded-xl bg-brand hover:bg-brand text-ink font-bold text-xs"
              >
                Ir a Mis Cursos para Rendir Evaluación
              </button>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {myCertificates.map((cert) => (
                <div
                  key={cert.id}
                  className="p-5 rounded-2xl bg-panel border border-line hover:border-brand/50 transition-all space-y-4 shadow-xl"
                >
                  <div className="flex items-center justify-between">
                    <span className="font-mono text-xs font-bold text-cyan-700">
                      {cert.certificateCode}
                    </span>
                    <span className={`text-[10px] px-2 py-0.5 rounded-full font-bold ${
                      cert.status === 'VÁLIDO'
                        ? 'bg-emerald-50 text-emerald-700 border border-emerald-200'
                        : 'bg-rose-50 text-rose-700 border border-rose-200'
                    }`}>
                      {cert.status}
                    </span>
                  </div>

                  <div>
                    <h3 className="font-bold text-base text-ink">{cert.courseTitle}</h3>
                    <div className="text-xs text-muted mt-1">
                      Nota Aprobatoria: <strong className="text-brand">{cert.finalScore}/20</strong> • {cert.durationHours} horas
                    </div>
                    <div className="text-[11px] text-muted mt-0.5">Fecha de emisión: {cert.issueDate}</div>
                  </div>

                  {/* QR Preview */}
                  <div className="p-3 rounded-xl bg-canvas border border-line flex items-center gap-3">
                    {cert.qrDataUrl ? (
                      <img
                        src={cert.qrDataUrl}
                        alt="QR Verificación"
                        className="w-14 h-14 rounded-lg bg-panel p-1 shrink-0"
                      />
                    ) : (
                      <div className="w-14 h-14 rounded-lg bg-surface-low animate-pulse"></div>
                    )}
                    <div className="text-[11px] text-muted">
                      Escanea o valida públicamente la autenticidad institucional del documento.
                    </div>
                  </div>

                  <div className="pt-2 flex gap-2">
                    <button
                      onClick={() => setSelectedCertForView(cert.id)}
                      className="flex-1 py-2 px-3 rounded-xl bg-brand hover:bg-brand text-ink font-bold text-xs flex items-center justify-center gap-1.5"
                    >
                      <Award className="w-3.5 h-3.5" />
                      <span>Ver Diploma Oficial</span>
                    </button>
                    <button
                      onClick={() => {
                        setPublicVerificationCode(cert.certificateCode);
                        setActiveTab('verificador-publico');
                      }}
                      className="py-2 px-3 rounded-xl bg-surface-low hover:bg-surface-high text-ink text-xs font-semibold flex items-center gap-1"
                      title="Probar Validador Público"
                    >
                      <ExternalLink className="w-3.5 h-3.5" />
                      <span>Validar</span>
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* =========================================================================
          TAB: MIS PAGOS Y ÓRDENES (SECCIÓN 8 Y 9)
         ========================================================================= */}
      {currentSubTab === 'mis-pagos' && (
        <div className="space-y-6">
          <div>
            <h2 className="text-2xl font-bold text-ink flex items-center gap-2.5">
              <CreditCard className="w-6 h-6 text-brand" />
              <span>Mis Pagos Bancarios & Órdenes de Compra (Sección 8 & 9)</span>
            </h2>
            <p className="text-xs text-muted">
              Historial de transferencias bancarias, vouchers registrados y control de duplicidad antifraude
            </p>
          </div>

          <div className="bg-panel border border-line rounded-3xl overflow-hidden shadow-xl">
            <div className="responsive-table overflow-x-auto" tabIndex={0} role="region" aria-label="Tabla desplazable">
              <table className="w-full text-left text-xs text-muted">
                <thead className="bg-canvas text-muted font-semibold border-b border-line">
                  <tr>
                    <th className="p-4">Orden</th>
                    <th className="p-4">Producto Adquirido</th>
                    <th className="p-4">Banco & Operación</th>
                    <th className="p-4">Fecha Pago</th>
                    <th className="p-4">Monto</th>
                    <th className="p-4">Estado del Pago</th>
                    <th className="p-4">Comprobante</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-line/80">
                  {myPayments.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="p-8 text-center text-muted">
                        No hay pagos registrados para este usuario.
                      </td>
                    </tr>
                  ) : (
                    myPayments.map((p) => {
                      const statusBadges: Record<string, string> = {
                        APROBADO: 'bg-emerald-50 text-emerald-700 border-emerald-200',
                        PENDIENTE: 'bg-brand text-brand border-brand',
                        EN_REVISIÓN: 'bg-blue-50 text-blue-700 border-blue-200',
                        RECHAZADO: 'bg-rose-50 text-rose-700 border-rose-200',
                        DUPLICADO: 'bg-purple-50 text-purple-700 border-purple-200 animate-pulse',
                        OBSERVADO: 'bg-orange-50 text-orange-700 border-orange-200',
                      };

                      return (
                        <tr key={p.id} className="hover:bg-surface-high/50 transition-colors">
                          <td className="p-4 font-mono font-bold text-brand">{p.orderCode}</td>
                          <td className="p-4 font-medium text-ink max-w-xs truncate">{p.productTitle}</td>
                          <td className="p-4">
                            <div className="font-semibold text-ink">{p.banco}</div>
                            <div className="font-mono text-muted text-[11px]">Op: {p.numeroOperacion}</div>
                          </td>
                          <td className="p-4">{p.fechaPago}</td>
                          <td className="p-4 font-bold text-ink">S/. {p.monto.toFixed(2)}</td>
                          <td className="p-4">
                            <span className={`px-2.5 py-0.5 rounded-full border text-[10px] font-bold ${statusBadges[p.status] || 'bg-surface-low text-muted'}`}>
                              {p.status}
                            </span>
                            {p.status === 'DUPLICADO' && (
                              <div className="text-[10px] text-rose-700 mt-1 max-w-xs">
                                ⚠️ Operación rechazada por coincidir con pago previo aprobado.
                              </div>
                            )}
                          </td>
                          <td className="p-4">
                            <span className="text-[11px] text-muted font-mono underline cursor-pointer hover:text-brand truncate block max-w-[120px]">
                              {p.voucherFileName}
                            </span>
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* =========================================================================
          TAB: MIS CONSULTAS / TICKETS DE SOPORTE (SECCIÓN 15.2)
         ========================================================================= */}
      {currentSubTab === 'mis-consultas' && (
        <div className="space-y-6">
          <div>
            <h2 className="text-2xl font-bold text-ink flex items-center gap-2.5">
              <MessageSquare className="w-6 h-6 text-brand" />
              <span>Mis Consultas y Tickets de Soporte (Sección 15.2)</span>
            </h2>
            <p className="text-xs text-muted">
              Sistema interno de atención y trazabilidad con respuestas del equipo técnico
            </p>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Create Ticket Form */}
            <div className="p-6 rounded-3xl bg-panel border border-line space-y-4">
              <h3 className="font-bold text-sm text-ink">Crear Nueva Consulta</h3>

              {newTicketSuccess && (
                <div className="p-3 rounded-xl bg-emerald-50 border border-emerald-200 text-emerald-700 text-xs">
                  ✓ Ticket enviado correctamente. Recibirás respuesta por este canal.
                </div>
              )}

              <form onSubmit={handleCreateTicket} className="space-y-3 text-xs">
                <div>
                  <label className="block text-muted mb-1">Categoría</label>
                  <select
                    value={ticketCategory}
                    onChange={(e) => setTicketCategory(e.target.value as any)}
                    className="w-full bg-canvas border border-line rounded-lg p-2 text-ink"
                  >
                    <option value="General">General</option>
                    <option value="Pagos">Pagos y Vouchers</option>
                    <option value="Acceso a Curso">Acceso a Aula Virtual</option>
                    <option value="Evaluación">Evaluaciones y Notas</option>
                    <option value="Certificado">Certificado con QR</option>
                    <option value="Asesoría">Asesoría Pericial</option>
                  </select>
                </div>

                <div>
                  <label className="block text-muted mb-1">Asunto</label>
                  <input
                    type="text"
                    required
                    placeholder="Título breve de tu consulta"
                    value={ticketSubject}
                    onChange={(e) => setTicketSubject(e.target.value)}
                    className="w-full bg-canvas border border-line rounded-lg p-2 text-ink"
                  />
                </div>

                <div>
                  <label className="block text-muted mb-1">Mensaje</label>
                  <textarea
                    rows={4}
                    required
                    placeholder="Describe en detalle tu situación..."
                    value={ticketMessage}
                    onChange={(e) => setTicketMessage(e.target.value)}
                    className="w-full bg-canvas border border-line rounded-lg p-2 text-ink resize-none"
                  />
                </div>

                <button
                  type="submit"
                  className="w-full py-2 px-3 rounded-lg bg-brand hover:bg-brand text-ink font-bold text-xs"
                >
                  Enviar Ticket
                </button>
              </form>
            </div>

            {/* Tickets History */}
            <div className="lg:col-span-2 space-y-4">
              <h3 className="font-bold text-sm text-ink">Historial de Tickets ({myTickets.length})</h3>

              {myTickets.length === 0 ? (
                <div className="p-8 rounded-2xl bg-panel border border-line text-center text-xs text-muted">
                  No has registrado consultas aún.
                </div>
              ) : (
                myTickets.map((t) => (
                  <div key={t.id} className="p-5 rounded-2xl bg-panel border border-line space-y-3">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <span className="font-mono text-xs font-bold text-brand">{t.ticketCode}</span>
                        <span className="text-[10px] px-2 py-0.5 rounded bg-canvas text-muted border border-line">
                          {t.category}
                        </span>
                      </div>
                      <span className={`text-[10px] px-2.5 py-0.5 rounded-full font-bold ${
                        t.status === 'RESPONDIDO'
                          ? 'bg-emerald-50 text-emerald-700 border border-emerald-200'
                          : 'bg-brand text-brand border border-brand'
                      }`}>
                        {t.status}
                      </span>
                    </div>

                    <h4 className="font-bold text-sm text-ink">{t.subject}</h4>
                    <p className="text-xs text-muted bg-canvas/60 p-3 rounded-xl border border-slate-850">
                      {t.message}
                    </p>

                    {t.responses.length > 0 && (
                      <div className="space-y-2 pt-2 border-t border-line">
                        <div className="text-[11px] font-semibold text-emerald-700">Respuestas del Soporte:</div>
                        {t.responses.map((resp, i) => (
                          <div key={i} className="p-3 rounded-xl bg-emerald-50/30 border border-emerald-200/40 text-xs space-y-1">
                            <div className="flex items-center justify-between text-[10px] text-muted">
                              <span className="font-bold text-emerald-700">{resp.author}</span>
                              <span>{new Date(resp.timestamp).toLocaleString()}</span>
                            </div>
                            <p className="text-ink">{resp.message}</p>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      )}

      {/* =========================================================================
          TAB: PERFIL & VIGENCIA DE CUENTA (SECCIÓN 4.3 & 20)
         ========================================================================= */}
      {currentSubTab === 'perfil' && (
        <div className="space-y-6">
          <div>
            <h2 className="text-2xl font-bold text-ink flex items-center gap-2.5">
              <User className="w-6 h-6 text-brand" />
              <span>Mi Perfil y Vigencia de Cuenta (Sección 4.3)</span>
            </h2>
            <p className="text-xs text-muted">
              Gestión de identidad pericial, reglas de retención y renovación automática de membresía
            </p>
          </div>

          {/* Account Validity Rule Highlight Box */}
          <div className="p-6 rounded-3xl bg-panel border border-line space-y-4">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-line">
              <div>
                <span className="text-[10px] font-bold uppercase tracking-wider px-2 py-0.5 rounded bg-indigo-50 text-indigo-700">
                  Regla de Negocio Sección 4.3
                </span>
                <h3 className="text-lg font-bold text-ink mt-1">Estado de Vigencia de tu Cuenta</h3>
                <p className="text-xs text-muted">
                  {currentUser.hasApprovedPurchase
                    ? 'Cuenta activa por 2 años desde tu último pago aprobado. Se renueva automáticamente con cada nueva compra.'
                    : 'Cuenta nueva: Dispones de 30 días para realizar y obtener la aprobación de una compra. De lo contrario, pasa a proceso de eliminación.'}
                </p>
              </div>

              <div className="p-4 rounded-2xl bg-canvas border border-line text-center space-y-1 shrink-0">
                <div className="text-[10px] text-muted">Días restantes de acceso:</div>
                <div className={`text-3xl font-extrabold ${diffDays <= 7 ? 'text-rose-700' : 'text-emerald-700'}`}>
                  {diffDays > 0 ? diffDays : 0} <span className="text-xs font-normal text-muted">días</span>
                </div>
                <div className="text-[10px] text-muted font-mono">Vence: {new Date(currentUser.validUntil).toLocaleDateString()}</div>
              </div>
            </div>

            {/* Notification alert simulation as recommended in Section 4.3: 7 days, 3 days, 1 day before */}
            {diffDays <= 7 && (
              <div className="p-4 rounded-2xl bg-rose-50/60 border border-rose-200 text-xs text-rose-700 flex items-start gap-3">
                <AlertTriangle className="w-5 h-5 text-rose-700 shrink-0 mt-0.5" />
                <div>
                  <strong className="block font-bold">ALERTA DE VENCIMIENTO DE CUENTA (Sección 4.3):</strong>
                  Tu cuenta vencerá en {diffDays} días. Para mantener activa tu cuenta por otros 2 años adicionales y conservar acceso a tus certificados y materiales, adquiere un nuevo curso o evento antes de la fecha límite.
                </div>
              </div>
            )}

            {/* Testing buttons for demo */}
            <div className="p-4 rounded-2xl bg-canvas border border-line text-xs flex flex-wrap items-center justify-between gap-3">
              <span className="text-muted">
                🛠️ <strong>Simulador para Pruebas del Cliente:</strong> Ajustar fecha de vencimiento:
              </span>
              <div className="flex gap-2">
                <button
                  onClick={() => simulateValidityChange(-25)}
                  className="px-3 py-1.5 rounded-lg bg-surface-low hover:bg-surface-high text-ink"
                >
                  Adelantar 25 días (Provocar alerta 5d)
                </button>
                <button
                  onClick={() => simulateValidityChange(-35)}
                  className="px-3 py-1.5 rounded-lg bg-rose-50 hover:bg-rose-50 text-rose-700 border border-rose-200"
                >
                  Adelantar 35 días (Vencida)
                </button>
                <button
                  onClick={() => simulateValidityChange(730)}
                  className="px-3 py-1.5 rounded-lg bg-emerald-50 hover:bg-emerald-50 text-emerald-700 border border-emerald-200"
                >
                  Restablecer 2 años (+730d)
                </button>
              </div>
            </div>
          </div>

          {/* Personal Info Display (Section 4.1) */}
          <div className="p-6 rounded-3xl bg-panel border border-line space-y-4">
            <h3 className="font-bold text-sm text-ink">Datos del Perfil del Estudiante (Sección 4.1)</h3>
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4 text-xs">
              <div className="p-3.5 rounded-xl bg-canvas border border-line space-y-1">
                <div className="text-muted">Nombres y Apellidos</div>
                <div className="font-semibold text-ink">{currentUser.nombres} {currentUser.apellidos}</div>
              </div>
              <div className="p-3.5 rounded-xl bg-canvas border border-line space-y-1">
                <div className="text-muted">Documento de Identidad</div>
                <div className="font-semibold text-ink">{currentUser.tipoDocumento} {currentUser.numeroDocumento}</div>
              </div>
              <div className="p-3.5 rounded-xl bg-canvas border border-line space-y-1">
                <div className="text-muted">Correo Electrónico (Login)</div>
                <div className="font-semibold text-ink">{currentUser.email}</div>
              </div>
              <div className="p-3.5 rounded-xl bg-canvas border border-line space-y-1">
                <div className="text-muted">Teléfono / WhatsApp</div>
                <div className="font-semibold text-ink">{currentUser.telefono}</div>
              </div>
              <div className="p-3.5 rounded-xl bg-canvas border border-line space-y-1">
                <div className="text-muted">Nivel Educativo</div>
                <div className="font-semibold text-ink">{currentUser.nivelEducativo}</div>
              </div>
              <div className="p-3.5 rounded-xl bg-canvas border border-line space-y-1">
                <div className="text-muted">Profesión Actual</div>
                <div className="font-semibold text-ink">{currentUser.profesion}</div>
              </div>
              <div className="sm:col-span-2 md:col-span-3 p-3.5 rounded-xl bg-canvas border border-line space-y-1">
                <div className="text-muted">Institución de Procedencia o Pregrado</div>
                <div className="font-semibold text-ink">{currentUser.institucionEducativa}</div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
