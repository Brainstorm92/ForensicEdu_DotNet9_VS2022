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
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8 animate-in fade-in duration-300">
      {/* =========================================================================
          TAB: MIS CURSOS (AULA VIRTUAL Y EVALUACIONES)
         ========================================================================= */}
      {currentSubTab === 'mis-cursos' && (
        <div className="space-y-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
            <div>
              <h2 className="text-2xl font-bold text-white flex items-center gap-2.5">
                <BookOpen className="w-6 h-6 text-amber-400" />
                <span>Mis Cursos Matriculados (Sección 10)</span>
              </h2>
              <p className="text-xs text-slate-400">
                Acceso al aula virtual, materiales descargables con marca de agua y evaluaciones cronometradas
              </p>
            </div>
            <button
              onClick={() => setActiveTab('catalogo')}
              className="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 text-xs font-semibold w-fit"
            >
              + Explorar Más Cursos
            </button>
          </div>

          {myEnrollments.length === 0 ? (
            <div className="p-12 rounded-3xl bg-slate-900 border border-slate-800 text-center space-y-3">
              <BookOpen className="w-12 h-12 text-slate-600 mx-auto" />
              <h3 className="font-bold text-base text-white">No tienes cursos matriculados aún</h3>
              <p className="text-xs text-slate-400 max-w-md mx-auto">
                Conforme a la Sección 10, la matrícula se genera automáticamente una vez que el pago bancario correspondiente es aprobado por administración.
              </p>
              <button
                onClick={() => setActiveTab('catalogo')}
                className="px-5 py-2.5 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs"
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
                    className="p-5 rounded-2xl bg-slate-900 border border-slate-800 hover:border-slate-700 transition-all flex flex-col justify-between space-y-4 shadow-xl"
                  >
                    <div className="space-y-3">
                      <div className="flex items-center justify-between">
                        <span className="text-[10px] font-mono font-bold px-2 py-0.5 rounded bg-slate-950 text-amber-400 border border-amber-500/30">
                          {c.code}
                        </span>
                        <span className={`text-[10px] px-2 py-0.5 rounded-full font-bold ${
                          enr.examStatus === 'APROBADA'
                            ? 'bg-emerald-950 text-emerald-300 border border-emerald-700'
                            : enr.examStatus === 'DESAPROBADA'
                            ? 'bg-rose-950 text-rose-300 border border-rose-700'
                            : 'bg-indigo-950 text-indigo-300 border border-indigo-700'
                        }`}>
                          {enr.examStatus === 'APROBADA' ? `Aprobado (${enr.finalScore}/20)` : enr.examStatus === 'DESAPROBADA' ? `Desaprobado (${enr.finalScore}/20)` : 'En Estudio'}
                        </span>
                      </div>

                      <h3 className="font-bold text-base text-white">{c.title}</h3>
                      <p className="text-xs text-slate-400 line-clamp-2">{c.shortDescription}</p>

                      {/* Progress bar */}
                      <div className="space-y-1.5 pt-1">
                        <div className="flex items-center justify-between text-[11px] text-slate-400 font-medium">
                          <span>Avance de Módulos</span>
                          <span className="text-amber-400 font-bold">{enr.progressPercent}%</span>
                        </div>
                        <div className="w-full h-1.5 rounded-full bg-slate-800 overflow-hidden">
                          <div
                            className="h-full bg-amber-500"
                            style={{ width: `${enr.progressPercent}%` }}
                          ></div>
                        </div>
                      </div>
                    </div>

                    {/* Evaluation status indicator */}
                    <div className="p-2.5 rounded-xl bg-slate-950 border border-slate-800 text-[11px] flex items-center justify-between">
                      <div className="flex items-center gap-1.5 text-slate-300">
                        <Clock className="w-3.5 h-3.5 text-amber-400" />
                        <span>Evaluación 10 min:</span>
                      </div>
                      <span className={`font-semibold ${enr.examStatus === 'APROBADA' ? 'text-emerald-400' : enr.examStatus === 'DESAPROBADA' ? 'text-rose-400' : 'text-amber-400'}`}>
                        {enr.examStatus === 'APROBADA' ? `Aprobado (${enr.finalScore}/20)` : enr.attemptsUsed > 0 ? `Intento ${enr.attemptsUsed}/2 (${enr.finalScore}/20)` : 'Pendiente (Mín. 13)'}
                      </span>
                    </div>

                    <div className="pt-2 border-t border-slate-800/80 flex flex-wrap gap-2">
                      <button
                        onClick={() => setSelectedCourseId(c.id)}
                        className="flex-1 py-2 px-3 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs flex items-center justify-center gap-1.5 shadow-md shadow-amber-500/20"
                      >
                        <Play className="w-3.5 h-3.5" />
                        <span>Aula & Examen</span>
                      </button>

                      {enr.certificateId ? (
                        <button
                          onClick={() => setSelectedCertForView(enr.certificateId!)}
                          className="py-2 px-3 rounded-xl bg-emerald-950 hover:bg-emerald-900 border border-emerald-700 text-emerald-300 font-semibold text-xs flex items-center gap-1"
                          title="Ver Certificado QR"
                        >
                          <Award className="w-4 h-4" />
                          <span>Certificado QR</span>
                        </button>
                      ) : (
                        <button
                          onClick={() => setSelectedCourseId(c.id)}
                          className="py-2 px-3 rounded-xl bg-indigo-950 hover:bg-indigo-900 border border-indigo-700 text-indigo-300 font-semibold text-xs flex items-center gap-1"
                          title="Rendir Examen de 10 min"
                        >
                          <Clock className="w-4 h-4 text-amber-400" />
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
            <h2 className="text-2xl font-bold text-white flex items-center gap-2.5">
              <Calendar className="w-6 h-6 text-amber-400" />
              <span>Mis Eventos y Congresos en Vivo (Sección 14)</span>
            </h2>
            <p className="text-xs text-slate-400">
              Salas de transmisión en tiempo real habilitadas 15 minutos antes de la hora programada
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {events.map((evt) => (
              <div
                key={evt.id}
                className="p-5 rounded-2xl bg-slate-900 border border-slate-800 flex flex-col justify-between space-y-4"
              >
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <span className="text-[10px] uppercase font-bold px-2 py-0.5 rounded bg-indigo-900 text-indigo-200 border border-indigo-700">
                      {evt.type}
                    </span>
                    <span className="text-xs font-semibold text-amber-400">{evt.date} • {evt.time}</span>
                  </div>

                  <h3 className="font-bold text-base text-white">{evt.title}</h3>
                  <p className="text-xs text-slate-300">{evt.description}</p>
                  <div className="text-[11px] text-slate-400">Expositor: <strong>{evt.speaker}</strong></div>
                </div>

                <div className="p-3 rounded-xl bg-slate-950 border border-slate-800 text-xs flex items-center justify-between">
                  <div className="flex items-center gap-2 text-slate-400">
                    <Clock className="w-4 h-4 text-amber-400" />
                    <span>Transmisión en directo (Zoom Webinar / RTMP)</span>
                  </div>
                  <a
                    href={evt.roomUrl || '#'}
                    target="_blank"
                    rel="noreferrer"
                    className="px-3 py-1.5 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-white font-bold text-xs flex items-center gap-1 shadow-md shadow-emerald-950"
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
            <h2 className="text-2xl font-bold text-white flex items-center gap-2.5">
              <PhoneCall className="w-6 h-6 text-emerald-400" />
              <span>Asesoría Personalizada y Consultoría Pericial (Sección 6)</span>
            </h2>
            <p className="text-xs text-slate-400">
              Servicio independiente de los cursos. Atención directa mediante WhatsApp con el consultor asignado.
            </p>
          </div>

          <div className="p-6 sm:p-8 rounded-3xl bg-slate-900 border border-slate-800 space-y-6">
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 pb-6 border-b border-slate-800">
              <div className="space-y-2">
                <span className="text-[10px] font-bold uppercase tracking-wider px-2 py-0.5 rounded bg-emerald-500/20 text-emerald-300 border border-emerald-500/30">
                  Consultor Oficial Asignado
                </span>
                <h3 className="text-xl font-bold text-white">{advisory.specialist}</h3>
                <p className="text-xs text-amber-400 font-medium">{advisory.specialty}</p>
                <p className="text-xs text-slate-400 max-w-xl">
                  {advisory.description}
                </p>
              </div>

              <div className="p-4 rounded-2xl bg-slate-950 border border-slate-800 text-center space-y-2 shrink-0">
                <div className="text-[11px] text-slate-400">Canal de Atención Exclusivo:</div>
                <a
                  href={`https://wa.me/${advisory.whatsappContact.replace(/\D/g, '')}?text=Hola%20Cmdte.%20Sotomayor,%20tengo%20una%20consulta%20pericial%20referente%20a%20mi%20caso.`}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="px-5 py-2.5 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white font-bold text-xs flex items-center justify-center gap-2 shadow-lg shadow-emerald-950"
                >
                  <MessageSquare className="w-4 h-4" />
                  <span>Contactar por WhatsApp</span>
                </a>
                <div className="text-[10px] text-slate-500">{advisory.whatsappContact}</div>
              </div>
            </div>

            {/* Protocol & Business Rules */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-xs text-slate-300">
              <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-1">
                <div className="font-bold text-white">1. Asignación y Registro</div>
                <p className="text-slate-400 text-[11px]">
                  El sistema registra la asesoría adquirida, el consultor asignado, fecha, estado y trazabilidad.
                </p>
              </div>
              <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-1">
                <div className="font-bold text-white">2. Agenda y Documentos</div>
                <p className="text-slate-400 text-[11px]">
                  El estudiante puede enviar expedientes para lectura pericial y preparación previa de juicio oral.
                </p>
              </div>
              <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-1">
                <div className="font-bold text-white">3. Trazabilidad Judicial</div>
                <p className="text-slate-400 text-[11px]">
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
              <h2 className="text-2xl font-bold text-white flex items-center gap-2.5">
                <Award className="w-6 h-6 text-amber-400" />
                <span>Mis Certificados Digitales con QR (Sección 13)</span>
              </h2>
              <p className="text-xs text-slate-400">
                Certificaciones emitidas para notas aprobatorias ≥ 13/20 con verificación pública inviolable
              </p>
            </div>
            <button
              onClick={() => setActiveTab('verificador-publico')}
              className="px-4 py-2 rounded-xl bg-cyan-950 border border-cyan-700 text-cyan-300 text-xs font-semibold hover:bg-cyan-900 transition-colors flex items-center gap-1.5 w-fit"
            >
              <ExternalLink className="w-3.5 h-3.5" />
              <span>Abrir Validador QR Público</span>
            </button>
          </div>

          {myCertificates.length === 0 ? (
            <div className="p-12 rounded-3xl bg-slate-900 border border-slate-800 text-center space-y-3">
              <Award className="w-12 h-12 text-slate-600 mx-auto" />
              <h3 className="font-bold text-base text-white">Aún no cuentas con certificados emitidos</h3>
              <p className="text-xs text-slate-400 max-w-md mx-auto">
                Conforme a la Sección 12 y 13, solo se emite certificado si el estudiante obtiene una nota de <strong className="text-white">13/20 o superior</strong> en la evaluación del curso.
              </p>
              <button
                onClick={() => setActiveTab('mis-cursos')}
                className="px-5 py-2.5 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs"
              >
                Ir a Mis Cursos para Rendir Evaluación
              </button>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {myCertificates.map((cert) => (
                <div
                  key={cert.id}
                  className="p-5 rounded-2xl bg-slate-900 border border-slate-800 hover:border-amber-400/50 transition-all space-y-4 shadow-xl"
                >
                  <div className="flex items-center justify-between">
                    <span className="font-mono text-xs font-bold text-cyan-400">
                      {cert.certificateCode}
                    </span>
                    <span className={`text-[10px] px-2 py-0.5 rounded-full font-bold ${
                      cert.status === 'VÁLIDO'
                        ? 'bg-emerald-950 text-emerald-300 border border-emerald-700'
                        : 'bg-rose-950 text-rose-300 border border-rose-700'
                    }`}>
                      {cert.status}
                    </span>
                  </div>

                  <div>
                    <h3 className="font-bold text-base text-white">{cert.courseTitle}</h3>
                    <div className="text-xs text-slate-400 mt-1">
                      Nota Aprobatoria: <strong className="text-amber-400">{cert.finalScore}/20</strong> • {cert.durationHours} horas
                    </div>
                    <div className="text-[11px] text-slate-500 mt-0.5">Fecha de emisión: {cert.issueDate}</div>
                  </div>

                  {/* QR Preview */}
                  <div className="p-3 rounded-xl bg-slate-950 border border-slate-800 flex items-center gap-3">
                    {cert.qrDataUrl ? (
                      <img 
                        src={cert.qrDataUrl} 
                        alt="QR Verificación" 
                        className="w-14 h-14 rounded-lg bg-white p-1 shrink-0"
                      />
                    ) : (
                      <div className="w-14 h-14 rounded-lg bg-slate-800 animate-pulse"></div>
                    )}
                    <div className="text-[11px] text-slate-400">
                      Escanea o valida públicamente la autenticidad institucional del documento.
                    </div>
                  </div>

                  <div className="pt-2 flex gap-2">
                    <button
                      onClick={() => setSelectedCertForView(cert.id)}
                      className="flex-1 py-2 px-3 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs flex items-center justify-center gap-1.5"
                    >
                      <Award className="w-3.5 h-3.5" />
                      <span>Ver Diploma Oficial</span>
                    </button>
                    <button
                      onClick={() => {
                        setPublicVerificationCode(cert.certificateCode);
                        setActiveTab('verificador-publico');
                      }}
                      className="py-2 px-3 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 text-xs font-semibold flex items-center gap-1"
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
            <h2 className="text-2xl font-bold text-white flex items-center gap-2.5">
              <CreditCard className="w-6 h-6 text-amber-400" />
              <span>Mis Pagos Bancarios & Órdenes de Compra (Sección 8 & 9)</span>
            </h2>
            <p className="text-xs text-slate-400">
              Historial de transferencias bancarias, vouchers registrados y control de duplicidad antifraude
            </p>
          </div>

          <div className="bg-slate-900 border border-slate-800 rounded-3xl overflow-hidden shadow-xl">
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs text-slate-300">
                <thead className="bg-slate-950 text-slate-400 font-semibold border-b border-slate-800">
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
                <tbody className="divide-y divide-slate-800/80">
                  {myPayments.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="p-8 text-center text-slate-500">
                        No hay pagos registrados para este usuario.
                      </td>
                    </tr>
                  ) : (
                    myPayments.map((p) => {
                      const statusBadges: Record<string, string> = {
                        APROBADO: 'bg-emerald-950 text-emerald-300 border-emerald-700',
                        PENDIENTE: 'bg-amber-950 text-amber-300 border-amber-700',
                        EN_REVISIÓN: 'bg-blue-950 text-blue-300 border-blue-700',
                        RECHAZADO: 'bg-rose-950 text-rose-300 border-rose-700',
                        DUPLICADO: 'bg-purple-950 text-purple-300 border-purple-700 animate-pulse',
                        OBSERVADO: 'bg-orange-950 text-orange-300 border-orange-700',
                      };

                      return (
                        <tr key={p.id} className="hover:bg-slate-850/50 transition-colors">
                          <td className="p-4 font-mono font-bold text-amber-400">{p.orderCode}</td>
                          <td className="p-4 font-medium text-white max-w-xs truncate">{p.productTitle}</td>
                          <td className="p-4">
                            <div className="font-semibold text-white">{p.banco}</div>
                            <div className="font-mono text-slate-400 text-[11px]">Op: {p.numeroOperacion}</div>
                          </td>
                          <td className="p-4">{p.fechaPago}</td>
                          <td className="p-4 font-bold text-white">S/. {p.monto.toFixed(2)}</td>
                          <td className="p-4">
                            <span className={`px-2.5 py-0.5 rounded-full border text-[10px] font-bold ${statusBadges[p.status] || 'bg-slate-800 text-slate-300'}`}>
                              {p.status}
                            </span>
                            {p.status === 'DUPLICADO' && (
                              <div className="text-[10px] text-rose-400 mt-1 max-w-xs">
                                ⚠️ Operación rechazada por coincidir con pago previo aprobado.
                              </div>
                            )}
                          </td>
                          <td className="p-4">
                            <span className="text-[11px] text-slate-400 font-mono underline cursor-pointer hover:text-amber-400 truncate block max-w-[120px]">
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
            <h2 className="text-2xl font-bold text-white flex items-center gap-2.5">
              <MessageSquare className="w-6 h-6 text-amber-400" />
              <span>Mis Consultas y Tickets de Soporte (Sección 15.2)</span>
            </h2>
            <p className="text-xs text-slate-400">
              Sistema interno de atención y trazabilidad con respuestas del equipo técnico
            </p>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Create Ticket Form */}
            <div className="p-6 rounded-3xl bg-slate-900 border border-slate-800 space-y-4">
              <h3 className="font-bold text-sm text-white">Crear Nueva Consulta</h3>

              {newTicketSuccess && (
                <div className="p-3 rounded-xl bg-emerald-950 border border-emerald-700 text-emerald-300 text-xs">
                  ✓ Ticket enviado correctamente. Recibirás respuesta por este canal.
                </div>
              )}

              <form onSubmit={handleCreateTicket} className="space-y-3 text-xs">
                <div>
                  <label className="block text-slate-400 mb-1">Categoría</label>
                  <select
                    value={ticketCategory}
                    onChange={(e) => setTicketCategory(e.target.value as any)}
                    className="w-full bg-slate-950 border border-slate-700 rounded-lg p-2 text-white"
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
                  <label className="block text-slate-400 mb-1">Asunto</label>
                  <input
                    type="text"
                    required
                    placeholder="Título breve de tu consulta"
                    value={ticketSubject}
                    onChange={(e) => setTicketSubject(e.target.value)}
                    className="w-full bg-slate-950 border border-slate-700 rounded-lg p-2 text-white"
                  />
                </div>

                <div>
                  <label className="block text-slate-400 mb-1">Mensaje</label>
                  <textarea
                    rows={4}
                    required
                    placeholder="Describe en detalle tu situación..."
                    value={ticketMessage}
                    onChange={(e) => setTicketMessage(e.target.value)}
                    className="w-full bg-slate-950 border border-slate-700 rounded-lg p-2 text-white resize-none"
                  />
                </div>

                <button
                  type="submit"
                  className="w-full py-2 px-3 rounded-lg bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs"
                >
                  Enviar Ticket
                </button>
              </form>
            </div>

            {/* Tickets History */}
            <div className="lg:col-span-2 space-y-4">
              <h3 className="font-bold text-sm text-white">Historial de Tickets ({myTickets.length})</h3>

              {myTickets.length === 0 ? (
                <div className="p-8 rounded-2xl bg-slate-900 border border-slate-800 text-center text-xs text-slate-500">
                  No has registrado consultas aún.
                </div>
              ) : (
                myTickets.map((t) => (
                  <div key={t.id} className="p-5 rounded-2xl bg-slate-900 border border-slate-800 space-y-3">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <span className="font-mono text-xs font-bold text-amber-400">{t.ticketCode}</span>
                        <span className="text-[10px] px-2 py-0.5 rounded bg-slate-950 text-slate-400 border border-slate-800">
                          {t.category}
                        </span>
                      </div>
                      <span className={`text-[10px] px-2.5 py-0.5 rounded-full font-bold ${
                        t.status === 'RESPONDIDO'
                          ? 'bg-emerald-950 text-emerald-300 border border-emerald-700'
                          : 'bg-amber-950 text-amber-300 border border-amber-700'
                      }`}>
                        {t.status}
                      </span>
                    </div>

                    <h4 className="font-bold text-sm text-white">{t.subject}</h4>
                    <p className="text-xs text-slate-300 bg-slate-950/60 p-3 rounded-xl border border-slate-850">
                      {t.message}
                    </p>

                    {t.responses.length > 0 && (
                      <div className="space-y-2 pt-2 border-t border-slate-800">
                        <div className="text-[11px] font-semibold text-emerald-400">Respuestas del Soporte:</div>
                        {t.responses.map((resp, i) => (
                          <div key={i} className="p-3 rounded-xl bg-emerald-950/30 border border-emerald-800/40 text-xs space-y-1">
                            <div className="flex items-center justify-between text-[10px] text-slate-400">
                              <span className="font-bold text-emerald-300">{resp.author}</span>
                              <span>{new Date(resp.timestamp).toLocaleString()}</span>
                            </div>
                            <p className="text-slate-200">{resp.message}</p>
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
            <h2 className="text-2xl font-bold text-white flex items-center gap-2.5">
              <User className="w-6 h-6 text-amber-400" />
              <span>Mi Perfil y Vigencia de Cuenta (Sección 4.3)</span>
            </h2>
            <p className="text-xs text-slate-400">
              Gestión de identidad pericial, reglas de retención y renovación automática de membresía
            </p>
          </div>

          {/* Account Validity Rule Highlight Box */}
          <div className="p-6 rounded-3xl bg-slate-900 border border-slate-800 space-y-4">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-slate-800">
              <div>
                <span className="text-[10px] font-bold uppercase tracking-wider px-2 py-0.5 rounded bg-indigo-900 text-indigo-300">
                  Regla de Negocio Sección 4.3
                </span>
                <h3 className="text-lg font-bold text-white mt-1">Estado de Vigencia de tu Cuenta</h3>
                <p className="text-xs text-slate-400">
                  {currentUser.hasApprovedPurchase
                    ? 'Cuenta activa por 2 años desde tu último pago aprobado. Se renueva automáticamente con cada nueva compra.'
                    : 'Cuenta nueva: Dispones de 30 días para realizar y obtener la aprobación de una compra. De lo contrario, pasa a proceso de eliminación.'}
                </p>
              </div>

              <div className="p-4 rounded-2xl bg-slate-950 border border-slate-800 text-center space-y-1 shrink-0">
                <div className="text-[10px] text-slate-400">Días restantes de acceso:</div>
                <div className={`text-3xl font-extrabold ${diffDays <= 7 ? 'text-rose-400' : 'text-emerald-400'}`}>
                  {diffDays > 0 ? diffDays : 0} <span className="text-xs font-normal text-slate-400">días</span>
                </div>
                <div className="text-[10px] text-slate-500 font-mono">Vence: {new Date(currentUser.validUntil).toLocaleDateString()}</div>
              </div>
            </div>

            {/* Notification alert simulation as recommended in Section 4.3: 7 days, 3 days, 1 day before */}
            {diffDays <= 7 && (
              <div className="p-4 rounded-2xl bg-rose-950/60 border border-rose-700 text-xs text-rose-200 flex items-start gap-3">
                <AlertTriangle className="w-5 h-5 text-rose-400 shrink-0 mt-0.5" />
                <div>
                  <strong className="block font-bold">ALERTA DE VENCIMIENTO DE CUENTA (Sección 4.3):</strong>
                  Tu cuenta vencerá en {diffDays} días. Para mantener activa tu cuenta por otros 2 años adicionales y conservar acceso a tus certificados y materiales, adquiere un nuevo curso o evento antes de la fecha límite.
                </div>
              </div>
            )}

            {/* Testing buttons for demo */}
            <div className="p-4 rounded-2xl bg-slate-950 border border-slate-800 text-xs flex flex-wrap items-center justify-between gap-3">
              <span className="text-slate-400">
                🛠️ <strong>Simulador para Pruebas del Cliente:</strong> Ajustar fecha de vencimiento:
              </span>
              <div className="flex gap-2">
                <button
                  onClick={() => simulateValidityChange(-25)}
                  className="px-3 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-200"
                >
                  Adelantar 25 días (Provocar alerta 5d)
                </button>
                <button
                  onClick={() => simulateValidityChange(-35)}
                  className="px-3 py-1.5 rounded-lg bg-rose-950 hover:bg-rose-900 text-rose-200 border border-rose-800"
                >
                  Adelantar 35 días (Vencida)
                </button>
                <button
                  onClick={() => simulateValidityChange(730)}
                  className="px-3 py-1.5 rounded-lg bg-emerald-950 hover:bg-emerald-900 text-emerald-200 border border-emerald-800"
                >
                  Restablecer 2 años (+730d)
                </button>
              </div>
            </div>
          </div>

          {/* Personal Info Display (Section 4.1) */}
          <div className="p-6 rounded-3xl bg-slate-900 border border-slate-800 space-y-4">
            <h3 className="font-bold text-sm text-white">Datos del Perfil del Estudiante (Sección 4.1)</h3>
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4 text-xs">
              <div className="p-3.5 rounded-xl bg-slate-950 border border-slate-800 space-y-1">
                <div className="text-slate-500">Nombres y Apellidos</div>
                <div className="font-semibold text-white">{currentUser.nombres} {currentUser.apellidos}</div>
              </div>
              <div className="p-3.5 rounded-xl bg-slate-950 border border-slate-800 space-y-1">
                <div className="text-slate-500">Documento de Identidad</div>
                <div className="font-semibold text-white">{currentUser.tipoDocumento} {currentUser.numeroDocumento}</div>
              </div>
              <div className="p-3.5 rounded-xl bg-slate-950 border border-slate-800 space-y-1">
                <div className="text-slate-500">Correo Electrónico (Login)</div>
                <div className="font-semibold text-white">{currentUser.email}</div>
              </div>
              <div className="p-3.5 rounded-xl bg-slate-950 border border-slate-800 space-y-1">
                <div className="text-slate-500">Teléfono / WhatsApp</div>
                <div className="font-semibold text-white">{currentUser.telefono}</div>
              </div>
              <div className="p-3.5 rounded-xl bg-slate-950 border border-slate-800 space-y-1">
                <div className="text-slate-500">Nivel Educativo</div>
                <div className="font-semibold text-white">{currentUser.nivelEducativo}</div>
              </div>
              <div className="p-3.5 rounded-xl bg-slate-950 border border-slate-800 space-y-1">
                <div className="text-slate-500">Profesión Actual</div>
                <div className="font-semibold text-white">{currentUser.profesion}</div>
              </div>
              <div className="sm:col-span-2 md:col-span-3 p-3.5 rounded-xl bg-slate-950 border border-slate-800 space-y-1">
                <div className="text-slate-500">Institución de Procedencia o Pregrado</div>
                <div className="font-semibold text-white">{currentUser.institucionEducativa}</div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
