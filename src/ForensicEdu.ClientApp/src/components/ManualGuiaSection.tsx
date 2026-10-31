import React, { useState } from 'react';
import {
  BookOpen,
  Code2,
  ShieldCheck,
  CheckCircle2,
  Copy,
  FileText,
  Layers,
  Terminal,
  Settings,
  Key,
  Clock,
  Award,
  AlertTriangle,
  Search,
  ExternalLink,
  ChevronRight,
  Database,
  Cpu,
  Workflow
} from 'lucide-react';

export const ManualGuiaSection: React.FC = () => {
  const [activeSection, setActiveSection] = useState<'arquitectura' | 'flujos' | 'desarrollo' | 'estados' | 'antifraude'>('arquitectura');
  const [searchTerm, setSearchTerm] = useState('');
  const [copiedIndex, setCopiedIndex] = useState<string | null>(null);

  const handleCopy = (code: string, id: string) => {
    navigator.clipboard.writeText(code);
    setCopiedIndex(id);
    setTimeout(() => setCopiedIndex(null), 2000);
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8 animate-in fade-in duration-300">
      {/* Hero Header */}
      <div className="relative overflow-hidden rounded-2xl bg-gradient-to-r from-panel via-indigo-950 to-panel border border-indigo-200/40 p-8 shadow-xl">
        <div className="absolute top-0 right-0 w-96 h-96 bg-brand/10 rounded-full blur-3xl pointer-events-none" />

        <div className="relative z-10 flex flex-col md:flex-row items-start md:items-center justify-between gap-6">
          <div className="space-y-3">
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-bold bg-brand/20 text-brand border border-brand/30">
              <BookOpen className="w-3.5 h-3.5" />
              <span>Documentación Técnica Oficial (Francisco v5)</span>
            </div>
            <h1 className="text-3xl font-black text-ink tracking-tight">
              Manual de Arquitectura y Guía de Desarrollo
            </h1>
            <p className="text-muted text-sm max-w-2xl leading-relaxed">
              Guía exhaustiva para comprender, modificar, depurar y extender tanto el Backend en <strong>.NET 9 C# (Clean Architecture)</strong> como el Frontend interactivo en <strong>React 18 + TypeScript</strong>.
            </p>
          </div>

          <div className="flex flex-col sm:flex-row gap-3 shrink-0">
            <div className="p-3 rounded-xl bg-canvas/70 border border-line text-center">
              <span className="block text-2xl font-black text-emerald-700">4</span>
              <span className="text-[11px] text-muted">Capas C# Clean Arch</span>
            </div>
            <div className="p-3 rounded-xl bg-canvas/70 border border-line text-center">
              <span className="block text-2xl font-black text-brand">10</span>
              <span className="text-[11px] text-muted">Flujos de Negocio</span>
            </div>
            <div className="p-3 rounded-xl bg-canvas/70 border border-line text-center">
              <span className="block text-2xl font-black text-indigo-700">100%</span>
              <span className="text-[11px] text-muted">Forense y Antifraude</span>
            </div>
          </div>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div className="flex flex-wrap items-center gap-2 border-b border-line pb-3">
        <button
          onClick={() => setActiveSection('arquitectura')}
          className={`px-4 py-2.5 rounded-xl font-bold text-xs flex items-center gap-2 transition-all cursor-pointer ${
            activeSection === 'arquitectura'
              ? 'bg-brand text-ink shadow-lg shadow-brand/20'
              : 'bg-panel text-muted hover:text-ink border border-line hover:bg-surface-low'
          }`}
        >
          <Layers className="w-4 h-4" />
          <span>1. Arquitectura .NET 9 (4 Capas)</span>
        </button>

        <button
          onClick={() => setActiveSection('flujos')}
          className={`px-4 py-2.5 rounded-xl font-bold text-xs flex items-center gap-2 transition-all cursor-pointer ${
            activeSection === 'flujos'
              ? 'bg-brand text-ink shadow-lg shadow-brand/20'
              : 'bg-panel text-muted hover:text-ink border border-line hover:bg-surface-low'
          }`}
        >
          <Workflow className="w-4 h-4" />
          <span>2. Los 10 Flujos de Negocio</span>
        </button>

        <button
          onClick={() => setActiveSection('antifraude')}
          className={`px-4 py-2.5 rounded-xl font-bold text-xs flex items-center gap-2 transition-all cursor-pointer ${
            activeSection === 'antifraude'
              ? 'bg-brand text-ink shadow-lg shadow-brand/20'
              : 'bg-panel text-muted hover:text-ink border border-line hover:bg-surface-low'
          }`}
        >
          <ShieldCheck className="w-4 h-4" />
          <span>3. Motor Antifraude y QR</span>
        </button>

        <button
          onClick={() => setActiveSection('desarrollo')}
          className={`px-4 py-2.5 rounded-xl font-bold text-xs flex items-center gap-2 transition-all cursor-pointer ${
            activeSection === 'desarrollo'
              ? 'bg-brand text-ink shadow-lg shadow-brand/20'
              : 'bg-panel text-muted hover:text-ink border border-line hover:bg-surface-low'
          }`}
        >
          <Code2 className="w-4 h-4" />
          <span>4. ¿Cómo Hacer Cambios?</span>
        </button>

        <button
          onClick={() => setActiveSection('estados')}
          className={`px-4 py-2.5 rounded-xl font-bold text-xs flex items-center gap-2 transition-all cursor-pointer ${
            activeSection === 'estados'
              ? 'bg-brand text-ink shadow-lg shadow-brand/20'
              : 'bg-panel text-muted hover:text-ink border border-line hover:bg-surface-low'
          }`}
        >
          <Database className="w-4 h-4" />
          <span>5. Matriz de Estados</span>
        </button>
      </div>

      {/* SECTION 1: ARQUITECTURA .NET 9 */}
      {activeSection === 'arquitectura' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="p-5 rounded-2xl bg-panel/90 border border-line space-y-3">
              <div className="flex items-center justify-between">
                <span className="px-2 py-0.5 rounded text-[10px] font-bold bg-brand/20 text-brand border border-brand/30">Capa 1</span>
                <Cpu className="w-5 h-5 text-brand" />
              </div>
              <h3 className="font-bold text-ink text-base">ForensicEdu.Domain</h3>
              <p className="text-xs text-muted leading-relaxed">
                El núcleo puro del sistema sin dependencias. Contiene las 15 entidades forenses y enums de estado inmutables.
              </p>
              <div className="text-[11px] text-muted font-mono">
                User, Course, Order, PaymentEvidence, DigitalCertificate, ExamAttempt
              </div>
            </div>

            <div className="p-5 rounded-2xl bg-panel/90 border border-line space-y-3">
              <div className="flex items-center justify-between">
                <span className="px-2 py-0.5 rounded text-[10px] font-bold bg-blue-500/20 text-blue-700 border border-blue-500/30">Capa 2</span>
                <Workflow className="w-5 h-5 text-blue-700" />
              </div>
              <h3 className="font-bold text-ink text-base">ForensicEdu.Application</h3>
              <p className="text-xs text-muted leading-relaxed">
                Casos de uso y reglas de negocio. Aloja el motor antifraude, evaluador de exámenes a 10 min y ciclo de vida de cuentas.
              </p>
              <div className="text-[11px] text-muted font-mono">
                AntifraudService, EvaluationService, AccountLifecycleService
              </div>
            </div>

            <div className="p-5 rounded-2xl bg-panel/90 border border-line space-y-3">
              <div className="flex items-center justify-between">
                <span className="px-2 py-0.5 rounded text-[10px] font-bold bg-emerald-500/20 text-emerald-700 border border-emerald-500/30">Capa 3</span>
                <Database className="w-5 h-5 text-emerald-700" />
              </div>
              <h3 className="font-bold text-ink text-base">ForensicEdu.Infrastructure</h3>
              <p className="text-xs text-muted leading-relaxed">
                Persistencia con Entity Framework Core (In-Memory, SQL Server), BCrypt para contraseñas, QuestPDF y QRCoder.
              </p>
              <div className="text-[11px] text-muted font-mono">
                ApplicationDbContext, HangfireJobs, PdfCertificateService
              </div>
            </div>

            <div className="p-5 rounded-2xl bg-panel/90 border border-line space-y-3">
              <div className="flex items-center justify-between">
                <span className="px-2 py-0.5 rounded text-[10px] font-bold bg-purple-500/20 text-purple-700 border border-purple-500/30">Capa 4</span>
                <Terminal className="w-5 h-5 text-purple-700" />
              </div>
              <h3 className="font-bold text-ink text-base">ForensicEdu.WebApi</h3>
              <p className="text-xs text-muted leading-relaxed">
                Controladores REST HTTP, documentación interactiva Scalar/OpenAPI en /scalar/v1, CORS configurado y seguridad.
              </p>
              <div className="text-[11px] text-muted font-mono">
                AuthController, OrdersController, CertificatesController, Admin
              </div>
            </div>
          </div>

          <div className="p-6 rounded-2xl bg-panel/80 border border-line space-y-4">
            <h3 className="text-lg font-bold text-ink flex items-center gap-2">
              <Terminal className="w-5 h-5 text-emerald-700" />
              <span>Diagrama de Flujo de Solución en Visual Studio 2022</span>
            </h3>
            <pre className="p-4 rounded-xl bg-canvas font-mono text-xs text-muted overflow-x-auto leading-relaxed border border-line">
{`ForensicEdu.sln (Visual Studio 2022)
│
├── 📂 src/
│   ├── 📁 ForensicEdu.Domain/          (.NET 9 C# 13)
│   │   ├── Entities/DomainEntities.cs  -> User, Course, Order, PaymentEvidence, etc.
│   │   └── Enums/DomainEnums.cs        -> PaymentStatus, CertificateStatus, UserRole
│   │
│   ├── 📁 ForensicEdu.Application/     (.NET 9 C# 13)
│   │   ├── Common/Interfaces/          -> IApplicationDbContext, IAntifraudService, etc.
│   │   └── Services/                   -> AntifraudService, EvaluationService, AccountLifecycle
│   │
│   ├── 📁 ForensicEdu.Infrastructure/  (.NET 9 C# 13)
│   │   ├── Persistence/                -> ApplicationDbContext (EF Core con Fluent API)
│   │   ├── BackgroundJobs/             -> Hangfire job diario de vencimientos a medianoche UTC
│   │   └── Services/                   -> Generación de QR (QRCoder) y PDF (QuestPDF)
│   │
│   ├── 📁 ForensicEdu.WebApi/          (.NET 9 Web API)
│   │   ├── Controllers/                -> Auth, Courses, Orders, Exams, Certificates, Admin
│   │   ├── Properties/launchSettings   -> Perfiles "http" y "https" para F5 con Scalar docs
│   │   └── Program.cs                  -> Inyección de dependencias, CORS y OpenAPI
│   │
│   └── 📁 ForensicEdu.ClientApp/       (React 18 + Vite TypeScript .esproj)
│       ├── vite.config.ts              -> Proxy configurado para reenviar /api a localhost:7148
│       ├── package.json                -> React 18, Tailwind CSS, Lucide
│       └── src/                        -> Catálogo, Aula Virtual, Certificados QR, Admin`}
            </pre>
          </div>
        </div>
      )}

      {/* SECTION 2: LOS 10 FLUJOS DE NEGOCIO */}
      {activeSection === 'flujos' && (
        <div className="space-y-4">
          {[
            {
              num: '1',
              title: 'Registro, Autenticación y Regla 30 Días / 2 Años',
              desc: 'Una cuenta por persona con Apellidos, Nombres, DNI, Email, Nivel Educativo y Profesión. Dispone de 30 días calendario para realizar su 1ª compra aprobada. Si no lo hace, pasa a PENDIENTE_DE_ELIMINACIÓN. Al aprobarse su 1ª compra, pasa a ACTIVA con 2 años de vigencia. Cada nueva compra renueva automáticamente otros 2 años desde la fecha de ese pago.',
              color: 'text-brand'
            },
            {
              num: '2',
              title: 'Catálogo de Cursos, Eventos en Vivo y Asesoría Personalizada',
              desc: 'Cursos asincrónicos (Balística, Explosivos, Escena del Crimen, etc.), eventos sincrónicos (Congresos, Seminarios con sala que abre 15 min antes), y Asesoría Pericial privada con perito oficial asignado (Cmdte. Manuel Sotomayor Vega) y enlace a WhatsApp.',
              color: 'text-blue-700'
            },
            {
              num: '3',
              title: 'Órdenes de Compra y Regla Estricta "1 Pago = 1 Producto"',
              desc: 'El sistema genera una orden interna única (ORD-YYYY-XXXXXX). Queda terminantemente prohibido usar un mismo pago para matricular dos cursos o un curso y un evento.',
              color: 'text-emerald-700'
            },
            {
              num: '4',
              title: 'Registro de Evidencia de Pago y Motor Antifraude',
              desc: 'El alumno registra Banco, Número de Operación, Fecha, Monto, Titular y Voucher (JPG/PNG/PDF). El backend genera la huella bancaria normalizada y el hash SHA-256 del archivo.',
              color: 'text-rose-700'
            },
            {
              num: '5',
              title: 'Validación Contable y Activación Automática de Matrícula',
              desc: 'El administrador financiero revisa los vouchers pendientes. Al aprobar, el sistema habilita automáticamente el acceso al curso o evento en Mis Cursos y renueva la cuenta por 2 años.',
              color: 'text-indigo-700'
            },
            {
              num: '6',
              title: 'Aula Virtual y Streaming Protegido (Sin Descarga Directa)',
              desc: 'Los materiales PDF descargables llevan registro y marca de agua. Los videos se reproducen en streaming autenticado sin botón de descarga ni exposición de URLs públicas directas.',
              color: 'text-cyan-700'
            },
            {
              num: '7',
              title: 'Evaluación de 10 Minutos y Re-evaluación a las 24 Horas',
              desc: 'Examen de 10 minutos (600s) cronometrado por servidor, nota de 0 a 20, mínimo aprobatorio 13.00. Si saca >= 13, aprueba directamente. Si saca < 13, debe esperar 24 horas antes del 2º intento. Si desaprueba el 2º intento, queda desaprobado definitivo sin más intentos.',
              color: 'text-purple-700'
            },
            {
              num: '8',
              title: 'Certificación Criptográfica con Código Único y QR Público',
              desc: 'Al aprobar (>=13), se genera el certificado con código único (CF-YYYY-SHORTCODE-XXXXXX) y QR que apunta a la página de validación pública institucional con estados VÁLIDO, REVOCADO o ANULADO.',
              color: 'text-teal-400'
            },
            {
              num: '9',
              title: 'Redes Sociales Dinámicas y Sistema de Soporte/Tickets',
              desc: 'Los enlaces a Facebook, Instagram, YouTube, TikTok y WhatsApp son 100% administrables desde la base de datos sin tocar código. Sistema de consultas por tickets (TK-YYYY-XXXX).',
              color: 'text-brand'
            },
            {
              num: '10',
              title: 'Auditoría Integral y Trazabilidad Multirrol',
              desc: 'Toda acción crítica (aprobación de pagos, detección de fraude, emisión o revocación de certificados) queda grabada en la tabla AuditLogs con fecha UTC, usuario responsable e IP.',
              color: 'text-emerald-700'
            }
          ].map((flow) => (
            <div key={flow.num} className="p-5 rounded-xl bg-panel border border-line flex items-start gap-4">
              <div className="w-9 h-9 rounded-xl bg-canvas border border-line flex items-center justify-center font-black text-sm text-ink shrink-0">
                {flow.num}
              </div>
              <div className="space-y-1">
                <h4 className={`text-base font-bold ${flow.color}`}>{flow.title}</h4>
                <p className="text-xs text-muted leading-relaxed">{flow.desc}</p>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* SECTION 3: MOTOR ANTIFRAUDE Y QR */}
      {activeSection === 'antifraude' && (
        <div className="space-y-6">
          <div className="p-6 rounded-2xl bg-panel/90 border border-line space-y-4">
            <h3 className="text-lg font-bold text-ink flex items-center gap-2">
              <ShieldCheck className="w-5 h-5 text-rose-700" />
              <span>Algoritmo de la Huella Bancaria Normalizada</span>
            </h3>
            <p className="text-xs text-muted leading-relaxed">
              Para impedir que un alumno malicioso intente registrar el mismo número de operación con variaciones de mayúsculas o espacios, el servicio <code>AntifraudService</code> normaliza estrictamente los 5 componentes:
            </p>
            <div className="p-4 rounded-xl bg-canvas border border-line font-mono text-xs text-brand">
              BANCO_NORMALIZADO | NUM_OPERACION_NORMALIZADO | YYYY-MM-DD | MONTO_F2 | TITULAR_NORMALIZADO
            </div>
            <p className="text-xs text-muted">
              Ejemplo real: <code>BCP|84920194|2026-10-25|220.00|JUAN CARLOS PEREZ</code>
            </p>
          </div>

          <div className="p-6 rounded-2xl bg-panel/90 border border-line space-y-4">
            <h3 className="text-lg font-bold text-ink flex items-center gap-2">
              <Award className="w-5 h-5 text-brand" />
              <span>Estructura del Código y Validación QR de Certificados</span>
            </h3>
            <p className="text-xs text-muted leading-relaxed">
              Cada certificado emitido por el Instituto Forense posee una nomenclatura criptográfica reglamentaria:
            </p>
            <div className="p-4 rounded-xl bg-canvas border border-line font-mono text-xs text-emerald-700">
              CF - [AÑO] - [CÓDIGO_CORTO_CURSO] - [CORRELATIVO_6_DÍGITOS]
            </div>
            <p className="text-xs text-muted">
              Ejemplo: <code>CF-2026-BAL-000027</code> para Balística Forense. El código QR apunta directamente a la URL pública: <code>https://forensic-edu.org/verificar/CF-2026-BAL-000027</code>, donde se muestra el estado en tiempo real (VÁLIDO, REVOCADO con causa, o ANULADO).
            </p>
          </div>
        </div>
      )}

      {/* SECTION 4: ¿CÓMO HACER CAMBIOS? */}
      {activeSection === 'desarrollo' && (
        <div className="space-y-6">
          <div className="p-6 rounded-2xl bg-panel/90 border border-line space-y-3">
            <h3 className="text-base font-bold text-ink flex items-center gap-2">
              <Code2 className="w-5 h-5 text-indigo-700" />
              <span>1. ¿Cómo agregar un nuevo curso o evento?</span>
            </h3>
            <p className="text-xs text-muted leading-relaxed">
              En el Backend, abre <code>ForensicEdu.Infrastructure/Persistence/ApplicationDbContext.cs</code> y añade en la semilla o migración:
            </p>
            <pre className="p-3.5 rounded-xl bg-canvas font-mono text-xs text-muted overflow-x-auto border border-line">
{`new Course
{
    Id = Guid.NewGuid().ToString(),
    Code = "CF-DOC-2026",
    ShortCode = "DOC",
    Title = "Documentología y Grafotecnia Forense",
    Price = 280.00m,
    AcademicHours = 60,
    Instructor = "Dra. Sofía Mendoza"
};`}
            </pre>
          </div>

          <div className="p-6 rounded-2xl bg-panel/90 border border-line space-y-3">
            <h3 className="text-base font-bold text-ink flex items-center gap-2">
              <Clock className="w-5 h-5 text-brand" />
              <span>2. ¿Cómo modificar el tiempo del examen o la nota mínima?</span>
            </h3>
            <p className="text-xs text-muted leading-relaxed">
              Abre <code>src/ForensicEdu.Application/Services/EvaluationService.cs</code>:
            </p>
            <pre className="p-3.5 rounded-xl bg-canvas font-mono text-xs text-muted overflow-x-auto border border-line">
{`// Modificar duración (actualmente 10 min = 600 segundos):
DurationSeconds = 600;

// Modificar nota mínima (actualmente 13 sobre 20):
bool approved = score >= 13.0m;

// Modificar tiempo de espera para el 2º intento (actualmente 24 horas):
nextAttemptAllowedAt = DateTime.UtcNow.AddHours(24);`}
            </pre>
          </div>

          <div className="p-6 rounded-2xl bg-panel/90 border border-line space-y-3">
            <h3 className="text-base font-bold text-ink flex items-center gap-2">
              <Database className="w-5 h-5 text-emerald-700" />
              <span>3. ¿Cómo cambiar a SQL Server en lugar de In-Memory?</span>
            </h3>
            <p className="text-xs text-muted leading-relaxed">
              En <code>ForensicEdu.Infrastructure/DependencyInjection.cs</code>, descomenta la línea de SQL Server:
            </p>
            <pre className="p-3.5 rounded-xl bg-canvas font-mono text-xs text-muted overflow-x-auto border border-line">
{`services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));`}
            </pre>
          </div>
        </div>
      )}

      {/* SECTION 5: MATRIZ DE ESTADOS */}
      {activeSection === 'estados' && (
        <div className="p-6 rounded-2xl bg-panel/90 border border-line space-y-4">
          <h3 className="text-lg font-bold text-ink flex items-center gap-2">
            <Database className="w-5 h-5 text-indigo-700" />
            <span>Matriz de Estados Oficiales del Sistema (Francisco v5)</span>
          </h3>
          <div className="overflow-x-auto">
            <table className="w-full text-xs text-left">
              <thead className="bg-canvas text-muted uppercase text-[10px] tracking-wider border-b border-line">
                <tr>
                  <th className="py-3 px-4">Entidad</th>
                  <th className="py-3 px-4">Estados Permitidos</th>
                  <th className="py-3 px-4">Efecto en el Sistema</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-line/80 text-muted">
                <tr>
                  <td className="py-3 px-4 font-bold text-ink">Orden</td>
                  <td className="py-3 px-4 font-mono text-brand">PENDIENTE_VALIDACION, APROBADO, RECHAZADO, CANCELADA</td>
                  <td className="py-3 px-4">Solo cuando pasa a APROBADO se activa la matrícula y la vigencia.</td>
                </tr>
                <tr>
                  <td className="py-3 px-4 font-bold text-ink">Pago / Voucher</td>
                  <td className="py-3 px-4 font-mono text-emerald-700">PENDIENTE, EN_REVISION, APROBADO, RECHAZADO, DUPLICADO, OBSERVADO</td>
                  <td className="py-3 px-4">DUPLICADO bloquea el acceso si la huella bancaria ya existía.</td>
                </tr>
                <tr>
                  <td className="py-3 px-4 font-bold text-ink">Cuenta Alumno</td>
                  <td className="py-3 px-4 font-mono text-cyan-700">REGISTRADA, ACTIVO, PROXIMA_A_VENCER, PENDIENTE_DE_ELIMINACION, SUSPENDIDO</td>
                  <td className="py-3 px-4">30 días sin compra = Eliminación. Con compra = 2 años de vigencia.</td>
                </tr>
                <tr>
                  <td className="py-3 px-4 font-bold text-ink">Evaluación</td>
                  <td className="py-3 px-4 font-mono text-purple-700">NO_INICIADA, EN_CURSO, APROBADO, DESAPROBADO_TEMPORAL, DESAPROBADO_DEFINITIVO</td>
                  <td className="py-3 px-4">Temporal activa espera de 24h para 2º intento. Definitivo bloquea futuros intentos.</td>
                </tr>
                <tr>
                  <td className="py-3 px-4 font-bold text-ink">Certificado</td>
                  <td className="py-3 px-4 font-mono text-teal-300">VALIDO, REVOCADO, ANULADO, VENCIDO</td>
                  <td className="py-3 px-4">El QR público muestra claramente si el documento fue REVOCADO y la razón oficial.</td>
                </tr>
                <tr>
                  <td className="py-3 px-4 font-bold text-ink">Ticket Soporte</td>
                  <td className="py-3 px-4 font-mono text-indigo-700">ABIERTO, EN_ATENCION, RESPONDIDO, CERRADO</td>
                  <td className="py-3 px-4">Canal formal de consultas académicas y técnicas con trazabilidad.</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};
