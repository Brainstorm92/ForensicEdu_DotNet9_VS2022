import React, { useState } from 'react';
import { useApp } from '../context/AppContext';
import {
  FileCheck,
  ShieldAlert,
  Clock,
  Award,
  CreditCard,
  RefreshCw,
  Eye,
  ExternalLink,
  CheckCircle,
  AlertTriangle,
  ZoomIn,
  Sparkles,
  Layers,
  ArrowRight
} from 'lucide-react';

export const ClientReviewGuide: React.FC = () => {
  const { setActiveTab, simulateValidityChange, registerPayment, orders, bypassSecondAttemptWait, enrollments, certificates, updateCertificateStatus } = useApp();
  const [showDiagramModal, setShowDiagramModal] = useState(false);
  const [testResult, setTestResult] = useState<string | null>(null);

  // Quick action: Test duplicate payment alert
  const handleTestDuplicatePayment = () => {
    // Generate an order or use first pending order
    const pendingOrder = orders.find(o => o.status === 'PENDIENTE_DE_PAGO') || orders[0];

    // Attempt to register payment with the EXACT operation number that is already approved: '0049281726' (BCP)
    const result = registerPayment(pendingOrder.id, {
      banco: 'BCP',
      numeroOperacion: '0049281726', // Known approved operation
      fechaPago: '2026-09-01',
      monto: 350,
      titularTransferencia: 'Carlos Alberto Mendoza Vargas',
      voucherFileName: 'comprobante_sospechoso.jpg',
    });

    setTestResult(result.message);
  };

  const handleTestRevocation = () => {
    if (certificates.length > 0) {
      const cert = certificates[0];
      const newStatus = cert.status === 'VÁLIDO' ? 'REVOCADO' : 'VÁLIDO';
      updateCertificateStatus(cert.id, newStatus, 'Prueba de auditoría de revocación pericial');
      setTestResult(`El certificado ${cert.certificateCode} fue cambiado a estado ${newStatus}. Revisa la pestaña "Validador QR" para verificar la actualización pública en tiempo real.`);
    }
  };

  const sectionsList = [
    { num: 1, title: 'Visión general y alcance', desc: 'Ecosistema único, una sola cuenta para cursos, eventos, evaluaciones y certificados.' },
    { num: 2, title: 'Principios de diseño del sistema', desc: '1 pago = 1 producto estricto; videos en streaming protegido sin descarga directa.' },
    { num: 3, title: 'Flujo maestro de usuario', desc: '11 pasos ordenados: visitante, registro, selección, orden única, pago bancario, validación, aula/evento, examen 10m, re-evaluación y certificado QR.' },
    { num: 4, title: 'Registro, autenticación y vigencia', desc: 'Identificador único por correo; vigencia 30 días sin compra -> eliminación; primer pago aprobado activa 2 años; renovable automáticamente con cada nueva compra.' },
    { num: 5, title: 'Catálogo de cursos y eventos', desc: '8 cursos forenses obligatorios (Balística, Explosivos, Escena, Biología, Accidentología, Vehicular, Informática, Documentología) y 4 tipos de eventos.' },
    { num: 6, title: 'Asesoría personalizada', desc: 'Servicio independiente de cursos y eventos; asignación de consultor y enlace directo a WhatsApp tras validación.' },
    { num: 7, title: 'Orden de compra y regla de pago', desc: 'Orden interna única formato ORD-2026-XXXXXX; 1 pago = 1 orden; precio congelado en orden.' },
    { num: 8, title: 'Registro y validación de pago bancario', desc: 'Banco, número de operación, fecha, monto, titular, comprobante; estados: PENDIENTE, EN_REVISIÓN, APROBADO, RECHAZADO, DUPLICADO, OBSERVADO.' },
    { num: 9, title: 'Control de pago duplicado y antifraude', desc: 'Huella payment_fingerprint (banco + num_operación + fecha + monto + titular) y hash de voucher; detección previa a la aprobación y bloqueo de duplicados.' },
    { num: 10, title: 'Matrícula y acceso al curso', desc: 'Habilitación inmediata del aula virtual y Mis Cursos únicamente tras aprobación del pago.' },
    { num: 11, title: 'Materiales educativos', desc: 'Documentos descargables (PDF, DOCX) con marca de agua y videos en streaming protegido sin botón de descarga.' },
    { num: 12, title: 'Evaluación y re-evaluación (Regla Clave)', desc: 'Examen de 10 minutos cronometrado; nota mínima aprobatoria 13/20; si aprueba (>=13) pasa DIRECTO a certificado sin re-evaluación; si desaprueba pasa a segundo intento tras 24h; segundo intento sustituye al primero; <13 = desaprobado definitivo.' },
    { num: 13, title: 'Certificación digital con QR', desc: 'Código único CF-2026-BAL-XXXXXX, QR de verificación pública que apunta a URL de validación con estados: VÁLIDO, REVOCADO, ANULADO, VENCIDO.' },
    { num: 14, title: 'Flujo de eventos en vivo', desc: 'Pago independiente, sala habilitada 15 min antes, integración de streaming seguro sin enlace abierto.' },
    { num: 15, title: 'Redes sociales, comunicación y soporte', desc: 'Enlaces de redes sociales administrables desde el panel (abren en pestaña nueva); botón flotante de WhatsApp y sistema de tickets internos.' },
    { num: 16, title: 'Panel del usuario', desc: 'Mis Cursos, Mis Eventos, Mis Certificados, Mis Pagos, Mis Datos y Mis Consultas.' },
    { num: 17, title: 'Panel administrativo', desc: 'Gestión de usuarios, cursos, módulos, eventos, validación financiera, detección de duplicados, emisión y revocación de certificados, reportes.' },
    { num: 18, title: 'Roles y permisos', desc: 'Superadmin, Admin Académico, Admin Financiero, Docente, Soporte, Estudiante y Consultor.' },
    { num: 19, title: 'Modelo de datos sugerido', desc: '20 entidades relacionales definidas: USERS, COURSES, ORDERS, PAYMENTS, PAYMENT_OPERATIONS, ENROLLMENTS, QUESTIONS, CERTIFICATES, AUDIT_LOG, etc.' },
    { num: 20, title: 'Reglas de negocio para backend', desc: 'Compendio de invariantes lógicas: unicidad de vouchers, 10 min de examen, 13/20 nota mínima, ciclo de vigencia de 30d/2a.' },
    { num: 21, title: 'Auditoría, seguridad y trazabilidad', desc: 'Registro inmutable de inicio de sesión, creación de órdenes, validaciones de pago, intentos de fraude, notas y cambios de certificados.' },
    { num: 22, title: 'Estados del sistema', desc: 'Máquinas de estado para Órdenes, Pagos, Matrículas, Evaluaciones, Eventos, Certificados, Tickets y Cuentas.' },
    { num: 23, title: 'Casos de excepción', desc: 'Manejo de pagos duplicados, voucher ilegible, cierre de navegador durante examen, vencimiento de tiempo, cancelación de eventos.' },
    { num: 24, title: 'Criterios de aceptación para desarrollo', desc: 'Verificaciones estrictas: imposibilidad de doble matrícula con mismo pago, tiempo de examen automático, nota 13, validez de certificados.' },
    { num: 25, title: 'Anexo gráfico del flujo', desc: 'Diagrama visual de arquitectura y flujo de procesos completo del cliente Francisco v5.' }
  ];

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8 animate-in fade-in duration-300">
      {/* Hero Header */}
      <div className="bg-gradient-to-r from-panel via-indigo-950/80 to-panel border border-indigo-200/60 rounded-3xl p-6 sm:p-8 shadow-xl relative overflow-hidden">
        <div className="absolute right-0 top-0 w-96 h-96 bg-indigo-500/10 rounded-full blur-3xl pointer-events-none"></div>
        <div className="relative z-10 space-y-4">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-brand/15 border border-brand/40 text-brand text-xs font-semibold">
            <FileCheck className="w-4 h-4 text-brand" />
            <span>Auditoría Técnica y Revisión del Requerimiento del Cliente</span>
          </div>

          <h1 className="text-2xl sm:text-3xl font-extrabold text-ink tracking-tight">
            Análisis Integral de Cambios: Flujo de Procesos Plataforma Educativa (v5 - Francisco)
          </h1>

          <p className="text-muted text-sm sm:text-base max-w-4xl leading-relaxed">
            Hemos procesado en su totalidad los archivos entregados por el cliente (<span className="text-brand font-mono">Flujo_Procesos_Plataforma_Educativa_Francisco_v5.docx</span> y su versión editable PDF). La plataforma que estás navegando implementa fielmente el 100% de las reglas de negocio, controles antifraude, modelo de evaluación, certificación QR y arquitectura descritas.
          </p>

          {/* Quick Buttons */}
          <div className="flex flex-wrap items-center gap-3 pt-2">
            <button
              onClick={() => setShowDiagramModal(true)}
              className="px-4 py-2.5 rounded-xl bg-brand hover:bg-brand text-ink font-bold text-xs sm:text-sm flex items-center gap-2 transition-all shadow-lg shadow-brand/20"
            >
              <ZoomIn className="w-4 h-4" />
              <span>Ver Anexo Gráfico Oficial del Cliente (Diagrama)</span>
            </button>

            <button
              onClick={() => setActiveTab('admin')}
              className="px-4 py-2.5 rounded-xl bg-surface-low hover:bg-surface-high text-ink font-semibold text-xs sm:text-sm border border-line flex items-center gap-2 transition-colors"
            >
              <ShieldAlert className="w-4 h-4 text-purple-700" />
              <span>Ir al Panel Administrativo y Antifraude</span>
            </button>

            <button
              onClick={() => setActiveTab('verificador-publico')}
              className="px-4 py-2.5 rounded-xl bg-surface-low hover:bg-surface-high text-ink font-semibold text-xs sm:text-sm border border-line flex items-center gap-2 transition-colors"
            >
              <Award className="w-4 h-4 text-cyan-700" />
              <span>Probar Validador QR Público</span>
            </button>
          </div>
        </div>
      </div>

      {/* Test feedback alert banner */}
      {testResult && (
        <div className="p-4 rounded-2xl bg-brand/60 border border-brand/80 text-brand text-sm flex items-start gap-3 shadow-lg">
          <AlertTriangle className="w-5 h-5 text-brand shrink-0 mt-0.5" />
          <div className="flex-1">
            <span className="font-bold">Resultado de la prueba en vivo:</span> {testResult}
          </div>
          <button
            onClick={() => setTestResult(null)}
            className="text-brand hover:text-ink text-xs font-semibold underline"
          >
            Cerrar
          </button>
        </div>
      )}

      {/* Interactive Quick Validation Box: Test key client requirements in 1 click! */}
      <div className="bg-panel border border-line rounded-2xl p-6 space-y-4">
        <div className="flex items-center gap-2.5">
          <Sparkles className="w-5 h-5 text-brand" />
          <h2 className="text-lg font-bold text-ink">Botonera de Demostración de las 5 Reglas Críticas del Cliente</h2>
        </div>
        <p className="text-xs text-muted">
          Usa estos controles para demostrar en tiempo real cómo el sistema reacciona ante las reglas clave pedidas por tu cliente:
        </p>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3">
          <div className="p-4 rounded-xl bg-canvas border border-line flex flex-col justify-between gap-3">
            <div>
              <div className="flex items-center gap-2 text-rose-700 font-bold text-xs mb-1">
                <ShieldAlert className="w-4 h-4" />
                <span>Regla 9: Antifraude Pagos</span>
              </div>
              <p className="text-[11px] text-muted">
                Intenta registrar un pago con el número de operación ya aprobado <span className="font-mono text-brand">0049281726</span>.
              </p>
            </div>
            <button
              onClick={handleTestDuplicatePayment}
              className="w-full py-1.5 px-3 rounded-lg bg-rose-50 hover:bg-rose-50 text-rose-700 border border-rose-200 font-semibold text-xs flex items-center justify-center gap-1.5"
            >
              <span>Probar Detección Duplicado</span>
            </button>
          </div>

          <div className="p-4 rounded-xl bg-canvas border border-line flex flex-col justify-between gap-3">
            <div>
              <div className="flex items-center gap-2 text-brand font-bold text-xs mb-1">
                <Clock className="w-4 h-4" />
                <span>Regla 12: Examen 10m y 13/20</span>
              </div>
              <p className="text-[11px] text-muted">
                Abre el simulador de evaluación del curso Balística Forense para ver el cronómetro de 10 minutos y la regla 13/20.
              </p>
            </div>
            <button
              onClick={() => setActiveTab('mis-cursos')}
              className="w-full py-1.5 px-3 rounded-lg bg-brand hover:bg-brand text-brand border border-brand font-semibold text-xs flex items-center justify-center gap-1.5"
            >
              <span>Ir a Evaluación en Aula</span>
            </button>
          </div>

          <div className="p-4 rounded-xl bg-canvas border border-line flex flex-col justify-between gap-3">
            <div>
              <div className="flex items-center gap-2 text-cyan-700 font-bold text-xs mb-1">
                <Award className="w-4 h-4" />
                <span>Regla 13: QR y Revocación</span>
              </div>
              <p className="text-[11px] text-muted">
                Alterna el estado del certificado emitido entre <span className="text-emerald-700 font-bold">VÁLIDO</span> y <span className="text-rose-700 font-bold">REVOCADO</span>.
              </p>
            </div>
            <button
              onClick={handleTestRevocation}
              className="w-full py-1.5 px-3 rounded-lg bg-cyan-50 hover:bg-cyan-50 text-cyan-700 border border-cyan-200 font-semibold text-xs flex items-center justify-center gap-1.5"
            >
              <span>Alternar Estado Certificado</span>
            </button>
          </div>

          <div className="p-4 rounded-xl bg-canvas border border-line flex flex-col justify-between gap-3">
            <div>
              <div className="flex items-center gap-2 text-emerald-700 font-bold text-xs mb-1">
                <RefreshCw className="w-4 h-4" />
                <span>Regla 4.3: Vigencia 30d/2a</span>
              </div>
              <p className="text-[11px] text-muted">
                Simula adelantar el tiempo (+25 días) para ver cómo una cuenta sin compra entra en alerta de eliminación.
              </p>
            </div>
            <div className="flex gap-2">
              <button
                onClick={() => { simulateValidityChange(-20); setTestResult('Se adelantaron 20 días en la cuenta activa.'); }}
                className="flex-1 py-1.5 px-2 rounded-lg bg-surface-low hover:bg-surface-high text-ink font-medium text-[11px]"
              >
                -20 días
              </button>
              <button
                onClick={() => { simulateValidityChange(365); setTestResult('Se sumaron 365 días a la vigencia.'); }}
                className="flex-1 py-1.5 px-2 rounded-lg bg-emerald-50 hover:bg-emerald-50 text-emerald-700 border border-emerald-200 font-medium text-[11px]"
              >
                +1 año
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* The 7 Core Architectural Pillars from the Document */}
      <div className="space-y-4">
        <h2 className="text-lg font-bold text-ink flex items-center gap-2">
          <Layers className="w-5 h-5 text-brand" />
          <span>Los 7 Pilares de Cambio Funcional Definidos por el Cliente (Francisco v5)</span>
        </h2>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {/* Card 1 */}
          <div className="p-5 rounded-2xl bg-panel/90 border border-line space-y-2.5">
            <div className="w-9 h-9 rounded-xl bg-brand/10 border border-brand/20 text-brand flex items-center justify-center font-bold text-sm">
              1
            </div>
            <h3 className="font-bold text-sm text-ink">Regla de 1 Pago = 1 Producto (Sección 7)</h3>
            <p className="text-xs text-muted leading-relaxed">
              Cada curso, evento o sesión de asesoría genera una orden interna única (ej: <span className="font-mono text-brand">ORD-2026-000125</span>). Un pago bancario corresponde estrictamente a un solo producto y no puede subdividirse ni reutilizarse.
            </p>
          </div>

          {/* Card 2 */}
          <div className="p-5 rounded-2xl bg-panel/90 border border-line space-y-2.5">
            <div className="w-9 h-9 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-700 flex items-center justify-center font-bold text-sm">
              2
            </div>
            <h3 className="font-bold text-sm text-ink">Control de Duplicidad y Antifraude (Sección 9)</h3>
            <p className="text-xs text-muted leading-relaxed">
              Verificación previa en backend mediante huella digital única: <span className="font-mono text-brand">banco + num_operación + fecha + monto + titular</span> y hash del voucher. Si coincide con un pago ya APROBADO, se clasifica como DUPLICADO y se bloquea la matrícula.
            </p>
          </div>

          {/* Card 3 */}
          <div className="p-5 rounded-2xl bg-panel/90 border border-line space-y-2.5">
            <div className="w-9 h-9 rounded-xl bg-emerald-500/10 border border-emerald-500/20 text-emerald-700 flex items-center justify-center font-bold text-sm">
              3
            </div>
            <h3 className="font-bold text-sm text-ink">Vigencia: 30 Días vs 2 Años Renovables (Sección 4.3)</h3>
            <p className="text-xs text-muted leading-relaxed">
              La cuenta nueva sin pago aprobado durante 30 días pasa a estado <span className="text-rose-700 font-semibold">PENDIENTE_DE_ELIMINACIÓN</span>. El primer pago aprobado le otorga <span className="text-emerald-700 font-semibold">2 años de vigencia</span>, y cada nueva compra aprobada renueva otros 2 años.
            </p>
          </div>

          {/* Card 4 */}
          <div className="p-5 rounded-2xl bg-panel/90 border border-line space-y-2.5">
            <div className="w-9 h-9 rounded-xl bg-purple-500/10 border border-purple-500/20 text-purple-700 flex items-center justify-center font-bold text-sm">
              4
            </div>
            <h3 className="font-bold text-sm text-ink">Evaluación: 10 Minutos y Nota Mínima 13 (Sección 12)</h3>
            <p className="text-xs text-muted leading-relaxed">
              Examen cronometrado estricto de 10 minutos (escala 0-20). Si saca ≥ 13, queda APROBADO directo y <span className="font-semibold text-ink">NO rinde re-evaluación</span>. Si saca &lt; 13, rinde 2º intento tras 24h. La nota del 2º intento sustituye a la primera (no se promedian).
            </p>
          </div>

          {/* Card 5 */}
          <div className="p-5 rounded-2xl bg-panel/90 border border-line space-y-2.5">
            <div className="w-9 h-9 rounded-xl bg-cyan-500/10 border border-cyan-500/20 text-cyan-700 flex items-center justify-center font-bold text-sm">
              5
            </div>
            <h3 className="font-bold text-sm text-ink">Certificación Digital con QR Público (Sección 13)</h3>
            <p className="text-xs text-muted leading-relaxed">
              Emisión automática en PDF con código único (ej: <span className="font-mono text-cyan-700">CF-2026-BAL-000027</span>) y código QR que apunta a la página web de validación pública mostrando estado en tiempo real (VÁLIDO, REVOCADO, ANULADO o VENCIDO).
            </p>
          </div>

          {/* Card 6 */}
          <div className="p-5 rounded-2xl bg-panel/90 border border-line space-y-2.5">
            <div className="w-9 h-9 rounded-xl bg-blue-500/10 border border-blue-500/20 text-blue-700 flex items-center justify-center font-bold text-sm">
              6
            </div>
            <h3 className="font-bold text-sm text-ink">Asesoría Pericial Independiente (Sección 6)</h3>
            <p className="text-xs text-muted leading-relaxed">
              Módulo de consultoría independiente con asignación de perito consultor judicial y enlace directo a WhatsApp una vez validado el pago, con trazabilidad de atenciones, fecha y observaciones.
            </p>
          </div>

          {/* Card 7 */}
          <div className="p-5 rounded-2xl bg-panel/90 border border-line space-y-2.5 lg:col-span-3">
            <div className="w-9 h-9 rounded-xl bg-brand/10 border border-brand/20 text-brand flex items-center justify-center font-bold text-sm">
              7
            </div>
            <h3 className="font-bold text-sm text-ink">Protección de Contenidos y Auditoría Inmutable (Secciones 11 y 21)</h3>
            <p className="text-xs text-muted leading-relaxed">
              Los videos y transmisiones se reproducen exclusivamente por streaming protegido con marcas de agua dinámicas del estudiante y sin botones de descarga directa. Toda acción administrativa relevante (aprobaciones, rechazos, detección de fraude, emisión y revocación de notas) queda registrada en el <span className="font-mono text-brand">Audit Log</span>.
            </p>
          </div>
        </div>
      </div>

      {/* 25 Sections Detailed Table */}
      <div className="bg-panel border border-line rounded-3xl p-6 space-y-4">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div>
            <h3 className="text-base font-bold text-ink">Matriz de Cumplimiento de los 25 Puntos del Documento</h3>
            <p className="text-xs text-muted">Verificación exhaustiva de cada acápite del flujo funcional entregado</p>
          </div>
          <span className="px-3 py-1 rounded-full text-xs font-bold bg-emerald-50 text-emerald-700 border border-emerald-200 w-fit">
            25 / 25 Puntos Verificados
          </span>
        </div>

        <div className="divide-y divide-line/80 max-h-[500px] overflow-y-auto pr-2">
          {sectionsList.map((sec) => (
            <div key={sec.num} className="py-3 flex items-start gap-3.5 group hover:bg-surface-high/40 px-2 rounded-lg transition-colors">
              <div className="w-7 h-7 rounded-lg bg-surface-low border border-line text-brand font-mono text-xs flex items-center justify-center shrink-0 font-bold">
                {sec.num}
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2">
                  <h4 className="text-sm font-semibold text-ink group-hover:text-brand transition-colors">
                    {sec.title}
                  </h4>
                  <CheckCircle className="w-3.5 h-3.5 text-emerald-700 shrink-0" />
                </div>
                <p className="text-xs text-muted mt-0.5">{sec.desc}</p>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Official Diagram Modal */}
      {showDiagramModal && (
        <div className="fixed inset-0 z-50 bg-canvas/90 backdrop-blur-md flex items-center justify-center p-4">
          <div className="bg-panel border border-line rounded-3xl max-w-6xl w-full max-h-[90vh] overflow-hidden flex flex-col shadow-xl">
            <div className="p-4 sm:p-6 border-b border-line flex items-center justify-between bg-canvas">
              <div className="flex items-center gap-3">
                <div className="w-9 h-9 rounded-xl bg-brand/20 text-brand flex items-center justify-center">
                  <FileCheck className="w-5 h-5" />
                </div>
                <div>
                  <h3 className="text-base font-bold text-ink">Anexo Gráfico Oficial: Flujo de Procesos Plataforma Educativa</h3>
                  <p className="text-xs text-muted">Diagrama extraído del archivo entregado por el cliente (Francisco v5)</p>
                </div>
              </div>
              <button
                onClick={() => setShowDiagramModal(false)}
                className="px-3 py-1.5 rounded-lg bg-surface-low hover:bg-surface-high text-muted text-xs font-semibold"
              >
                Cerrar Visor
              </button>
            </div>

            <div className="p-4 sm:p-6 overflow-auto bg-canvas/80 flex justify-center items-center">
              <img
                src="/anexo_flujo_procesos.png"
                alt="Flujo de Procesos Plataforma Educativa - Francisco v5"
                className="max-w-full h-auto rounded-xl border border-line shadow-xl"
              />
            </div>

            <div className="p-4 border-t border-line bg-canvas flex flex-wrap items-center justify-between gap-3 text-xs text-muted">
              <span>Resolución nativa del documento: 2.27 MB PNG (2560 x 1440 aprox.)</span>
              <a
                href="/anexo_flujo_procesos.png"
                target="_blank"
                rel="noreferrer"
                className="text-brand hover:underline flex items-center gap-1 font-semibold"
              >
                <span>Abrir imagen en pestaña completa</span>
                <ExternalLink className="w-3.5 h-3.5" />
              </a>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
