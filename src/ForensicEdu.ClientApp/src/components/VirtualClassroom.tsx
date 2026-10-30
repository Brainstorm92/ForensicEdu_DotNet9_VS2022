import React, { useState } from 'react';
import { useApp } from '../context/AppContext';
import { 
  Play, 
  CheckCircle2, 
  FileText, 
  Download, 
  Award, 
  Clock, 
  ShieldCheck, 
  Lock, 
  AlertCircle,
  Sparkles,
  BookOpen,
  ArrowLeft
} from 'lucide-react';
import { Course, CourseModule, Enrollment } from '../types';
import { ExamModal } from './ExamModal';

interface VirtualClassroomProps {
  courseId: string;
  onBack: () => void;
}

export const VirtualClassroom: React.FC<VirtualClassroomProps> = ({ courseId, onBack }) => {
  const { courses, enrollments, updateModuleProgress, currentUser, setActiveTab, resetExamForDemo } = useApp();

  const course = courses.find(c => c.id === courseId) || courses[0];
  const enrollment = enrollments.find(e => e.courseId === course.id && e.userId === currentUser.id);

  // If course has modules defined in mockData, use them; otherwise create standard forensic modules
  const modules: CourseModule[] = course.modules && course.modules.length > 0 ? course.modules : [
    {
      id: 'm1',
      title: 'Módulo 1: Fundamentos y Marco Teórico Pericial',
      duration: '1h 30m',
      videoUrl: 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4',
      streamingId: 'stream-01',
      documents: [
        { id: 'd1', title: `Guía Pericial de ${course.title}.pdf`, type: 'PDF', size: '4.2 MB', downloadUrl: '#' },
        { id: 'd2', title: 'Marco Normativo y Cadena de Custodia.docx', type: 'DOCX', size: '1.1 MB', downloadUrl: '#' },
      ],
    },
    {
      id: 'm2',
      title: 'Módulo 2: Procedimiento de Campo y Fijación Científica',
      duration: '2h 15m',
      videoUrl: 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4',
      streamingId: 'stream-02',
      documents: [
        { id: 'd3', title: 'Protocolo de Búsqueda y Embalaje de Indicios.pdf', type: 'PDF', size: '3.8 MB', downloadUrl: '#' },
      ],
    },
    {
      id: 'm3',
      title: 'Módulo 3: Análisis Instrumental y Casuística Forense',
      duration: '2h 00m',
      videoUrl: 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4',
      streamingId: 'stream-03',
      documents: [
        { id: 'd4', title: 'Casos Reales Resueltos en Juicio Oral.pdf', type: 'PDF', size: '5.5 MB', downloadUrl: '#' },
      ],
    },
    {
      id: 'm4',
      title: 'Módulo 4: Elaboración y Sustentación del Informe Pericial',
      duration: '1h 45m',
      videoUrl: 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4',
      streamingId: 'stream-04',
      documents: [
        { id: 'd5', title: 'Plantilla Oficial de Dictamen Pericial.docx', type: 'DOCX', size: '950 KB', downloadUrl: '#' },
      ],
    },
  ];

  const [activeModule, setActiveModule] = useState<CourseModule>(modules[0]);
  const [showExamModal, setShowExamModal] = useState(false);
  const [downloadNotice, setDownloadNotice] = useState<string | null>(null);

  if (!enrollment) {
    return (
      <div className="max-w-4xl mx-auto p-8 text-center space-y-4">
        <div className="w-16 h-16 rounded-full bg-amber-500/10 text-amber-400 flex items-center justify-center mx-auto">
          <Lock className="w-8 h-8" />
        </div>
        <h2 className="text-xl font-bold text-white">Matrícula no habilitada</h2>
        <p className="text-xs text-slate-400 max-w-md mx-auto">
          Para ingresar al aula virtual de este curso, debes contar con un pago bancario aprobado correspondiente a este producto.
        </p>
        <button
          onClick={onBack}
          className="px-4 py-2 rounded-xl bg-amber-500 text-slate-950 font-bold text-xs"
        >
          Volver a Mis Cursos
        </button>
      </div>
    );
  }

  const isModuleCompleted = (modId: string) => enrollment.completedModules.includes(modId);

  const handleDownloadDoc = (docTitle: string) => {
    setDownloadNotice(`Documento "${docTitle}" descargado con marca de agua digital: ${currentUser.nombres} ${currentUser.apellidos} - DNI: ${currentUser.numeroDocumento}`);
    setTimeout(() => setDownloadNotice(null), 5000);
  };

  const handleCompleteModule = (modId: string) => {
    updateModuleProgress(enrollment.id, modId);
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6 space-y-6 animate-in fade-in duration-200">
      {/* Back button & Course Heading */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-slate-800">
        <div className="flex items-center gap-3">
          <button
            onClick={onBack}
            className="p-2 rounded-xl bg-slate-900 border border-slate-800 hover:bg-slate-800 text-slate-300 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" />
          </button>
          <div>
            <div className="flex items-center gap-2">
              <span className="text-xs font-mono font-bold text-amber-400">{course.code}</span>
              <span className="text-slate-500">•</span>
              <span className="text-xs text-slate-400">{course.durationHours} horas académicas</span>
            </div>
            <h1 className="text-xl sm:text-2xl font-bold text-white mt-0.5">{course.title}</h1>
          </div>
        </div>

        {/* Exam Launch CTA Button */}
        <div className="flex items-center gap-2 flex-wrap">
          {enrollment.examStatus === 'APROBADA' ? (
            <>
              <button
                onClick={() => setActiveTab('mis-certificados')}
                className="px-4 py-2.5 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white font-bold text-xs flex items-center gap-2 shadow-lg shadow-emerald-950"
              >
                <Award className="w-4 h-4" />
                <span>Aprobado ({enrollment.finalScore}/20) • Ver Certificado QR</span>
              </button>
              <button
                onClick={() => setShowExamModal(true)}
                className="px-3.5 py-2.5 rounded-xl bg-amber-500/20 hover:bg-amber-500/30 text-amber-300 border border-amber-500/40 font-semibold text-xs flex items-center gap-1.5 transition-colors"
                title="Rendir nuevamente para practicar o demostrar"
              >
                <Clock className="w-3.5 h-3.5" />
                <span>Rendir Evaluación (10 min)</span>
              </button>
            </>
          ) : enrollment.attemptsUsed === 2 && enrollment.examStatus === 'DESAPROBADA' ? (
            <div className="flex items-center gap-2">
              <div className="px-4 py-2.5 rounded-xl bg-rose-950/80 border border-rose-700 text-rose-300 text-xs font-bold">
                Desaprobado Definitivo ({enrollment.finalScore}/20)
              </div>
              <button
                onClick={() => resetExamForDemo(enrollment.id)}
                className="px-3 py-2 rounded-xl bg-slate-800 text-slate-300 hover:text-white text-xs"
              >
                Reiniciar (Demo)
              </button>
            </div>
          ) : (
            <button
              onClick={() => setShowExamModal(true)}
              className="px-6 py-2.5 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-extrabold text-xs sm:text-sm flex items-center gap-2 shadow-xl shadow-amber-500/30 transition-all hover:scale-105"
            >
              <Clock className="w-4 h-4" />
              <span>
                {enrollment.attemptsUsed === 1 
                  ? 'Rendir Re-evaluación (2º Intento)' 
                  : 'Rendir Evaluación (10 min)'}
              </span>
            </button>
          )}
        </div>
      </div>

      {downloadNotice && (
        <div className="p-3.5 rounded-xl bg-emerald-950/70 border border-emerald-600 text-emerald-200 text-xs flex items-center gap-2 shadow-md animate-in fade-in">
          <ShieldCheck className="w-4 h-4 text-emerald-400 shrink-0" />
          <span>{downloadNotice}</span>
        </div>
      )}

      {/* Main Grid: Video Stream Player on Left (2 cols), Module list on Right (1 col) */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left: Secure Streaming Player (Section 11.2) */}
        <div className="lg:col-span-2 space-y-4">
          <div className="relative rounded-2xl overflow-hidden bg-slate-950 border border-slate-800 aspect-video shadow-2xl group select-none">
            {/* Dynamic Watermark overlay moving gently (Section 11.2) */}
            <div className="absolute top-4 left-4 z-20 pointer-events-none opacity-40 bg-slate-950/80 px-2.5 py-1 rounded border border-slate-700 text-[10px] font-mono text-slate-300">
              {currentUser.nombres} {currentUser.apellidos} • {currentUser.tipoDocumento} {currentUser.numeroDocumento} • Streaming Seguro
            </div>

            {/* Video Player */}
            <video
              key={activeModule.id}
              controls
              controlsList="nodownload"
              onContextMenu={(e) => e.preventDefault()}
              className="w-full h-full object-cover"
              poster={course.bannerUrl}
            >
              <source src={activeModule.videoUrl} type="video/mp4" />
              Tu navegador no soporta reproducción de video.
            </video>
          </div>

          {/* Module description and completion button */}
          <div className="p-5 rounded-2xl bg-slate-900 border border-slate-800 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <span className="text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded bg-amber-500/10 text-amber-400 border border-amber-500/20">
                Streaming Autenticado
              </span>
              <h2 className="font-bold text-base text-white mt-1.5">{activeModule.title}</h2>
              <p className="text-xs text-slate-400 mt-0.5">Duración estimada: {activeModule.duration}</p>
            </div>

            <button
              onClick={() => handleCompleteModule(activeModule.id)}
              className={`px-4 py-2.5 rounded-xl font-semibold text-xs flex items-center gap-2 transition-all shrink-0 ${
                isModuleCompleted(activeModule.id)
                  ? 'bg-emerald-950 border border-emerald-700 text-emerald-300'
                  : 'bg-slate-800 hover:bg-slate-700 text-white'
              }`}
            >
              <CheckCircle2 className={`w-4 h-4 ${isModuleCompleted(activeModule.id) ? 'text-emerald-400' : 'text-slate-400'}`} />
              <span>{isModuleCompleted(activeModule.id) ? 'Módulo Completado' : 'Marcar como Completado'}</span>
            </button>
          </div>

          {/* Downloadable Documents for Active Module (Section 11.1) */}
          <div className="p-5 rounded-2xl bg-slate-900 border border-slate-800 space-y-3">
            <h3 className="font-bold text-xs uppercase tracking-wider text-amber-400 flex items-center gap-2">
              <FileText className="w-4 h-4" />
              <span>Materiales Educativos Descargables del Módulo (Sección 11.1)</span>
            </h3>
            <p className="text-xs text-slate-400">
              Los documentos descargados incorporan marca de agua con tus nombres, documento y fecha conforme a la política académica.
            </p>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5 pt-1">
              {activeModule.documents.map((doc) => (
                <div
                  key={doc.id}
                  className="p-3 rounded-xl bg-slate-950 border border-slate-800 flex items-center justify-between text-xs hover:border-slate-700 transition-colors"
                >
                  <div className="flex items-center gap-2.5 min-w-0">
                    <span className="w-7 h-7 rounded-lg bg-indigo-950 border border-indigo-800 text-indigo-300 text-[10px] font-bold flex items-center justify-center shrink-0">
                      {doc.type}
                    </span>
                    <div className="truncate">
                      <div className="text-white font-medium truncate">{doc.title}</div>
                      <div className="text-[10px] text-slate-500">{doc.size}</div>
                    </div>
                  </div>
                  <button
                    onClick={() => handleDownloadDoc(doc.title)}
                    className="p-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-200 shrink-0 ml-2"
                    title="Descargar con marca de agua"
                  >
                    <Download className="w-4 h-4" />
                  </button>
                </div>
              ))}
            </div>
          </div>

          {/* Section 12: Dedicated Evaluation Card in Aula Virtual */}
          <div className="p-6 rounded-2xl bg-gradient-to-r from-slate-900 via-amber-950/20 to-slate-900 border-2 border-amber-500/40 space-y-4 shadow-xl">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
              <div className="space-y-1">
                <span className="text-[10px] font-bold uppercase tracking-wider px-2.5 py-0.5 rounded-full bg-amber-500/20 text-amber-300 border border-amber-500/30">
                  Evaluación Final del Curso (Sección 12)
                </span>
                <h3 className="text-lg font-bold text-white flex items-center gap-2">
                  <Clock className="w-5 h-5 text-amber-400" />
                  <span>Examen Cronometrado de 10 Minutos</span>
                </h3>
                <p className="text-xs text-slate-300 max-w-xl leading-relaxed">
                  Puntaje 0 a 20. Nota mínima aprobatoria 13/20. Si apruebas (≥13) se emite tu certificado digital con QR público de forma inmediata.
                </p>
              </div>

              <div className="shrink-0">
                {enrollment.examStatus === 'APROBADA' ? (
                  <div className="flex flex-col sm:flex-row items-center gap-2">
                    <button
                      onClick={() => setActiveTab('mis-certificados')}
                      className="w-full sm:w-auto px-5 py-3 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white font-bold text-xs sm:text-sm flex items-center justify-center gap-2 shadow-lg shadow-emerald-950"
                    >
                      <Award className="w-5 h-5" />
                      <span>Ver Certificado QR</span>
                    </button>
                    <button
                      onClick={() => setShowExamModal(true)}
                      className="w-full sm:w-auto px-5 py-3 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs sm:text-sm flex items-center justify-center gap-2 shadow-lg shadow-amber-500/20"
                    >
                      <Clock className="w-5 h-5" />
                      <span>Rendir Evaluación (10 min)</span>
                    </button>
                  </div>
                ) : enrollment.attemptsUsed === 2 && enrollment.examStatus === 'DESAPROBADA' ? (
                  <div className="px-5 py-3 rounded-xl bg-rose-950/90 border border-rose-700 text-rose-300 text-xs font-bold text-center">
                    Desaprobado Definitivo ({enrollment.finalScore}/20)
                  </div>
                ) : (
                  <button
                    onClick={() => setShowExamModal(true)}
                    className="w-full sm:w-auto px-6 py-3.5 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-extrabold text-xs sm:text-sm flex items-center justify-center gap-2 shadow-xl shadow-amber-500/30 transition-all hover:scale-105"
                  >
                    <Clock className="w-5 h-5" />
                    <span>
                      {enrollment.attemptsUsed === 1 
                        ? 'Rendir Re-evaluación (2º Intento)' 
                        : 'Iniciar Examen Ahora (10 Min)'}
                    </span>
                  </button>
                )}
              </div>
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-4 gap-2.5 pt-2 text-[11px] border-t border-slate-800 text-slate-400">
              <div>• Duración: <strong className="text-white">10 minutos</strong></div>
              <div>• Nota Mínima: <strong className="text-amber-400">13 / 20</strong></div>
              <div>• Intentos permitidos: <strong className="text-white">Máx. 2</strong></div>
              <div>• Estado: <strong className={enrollment.examStatus === 'APROBADA' ? 'text-emerald-400' : 'text-slate-300'}>{enrollment.examStatus}</strong></div>
            </div>
          </div>
        </div>

        {/* Right: Modules Playlist & Progress Bar */}
        <div className="space-y-4">
          <div className="p-5 rounded-2xl bg-slate-900 border border-slate-800 space-y-4">
            <div>
              <div className="flex items-center justify-between text-xs text-slate-300 mb-1.5 font-medium">
                <span>Progreso Académico del Curso</span>
                <span className="font-bold text-amber-400">{enrollment.progressPercent}%</span>
              </div>
              <div className="w-full h-2 rounded-full bg-slate-800 overflow-hidden">
                <div 
                  className="h-full bg-amber-500 transition-all duration-300"
                  style={{ width: `${enrollment.progressPercent}%` }}
                ></div>
              </div>
            </div>

            <div className="border-t border-slate-800 pt-3">
              <h4 className="text-xs font-bold text-white uppercase tracking-wider mb-2">
                Contenido del Programa (4 Módulos)
              </h4>
              <div className="space-y-2">
                {modules.map((mod, index) => {
                  const isActive = mod.id === activeModule.id;
                  const isDone = isModuleCompleted(mod.id);
                  return (
                    <button
                      key={mod.id}
                      onClick={() => setActiveModule(mod)}
                      className={`w-full text-left p-3 rounded-xl border text-xs transition-all flex items-start gap-3 ${
                        isActive
                          ? 'bg-amber-500/15 border-amber-400/80 text-amber-200 font-medium'
                          : 'bg-slate-950 border-slate-800 text-slate-300 hover:bg-slate-850'
                      }`}
                    >
                      <span className={`w-5 h-5 rounded-full flex items-center justify-center text-[10px] font-bold shrink-0 mt-0.5 ${
                        isDone
                          ? 'bg-emerald-500 text-slate-950'
                          : isActive
                          ? 'bg-amber-400 text-slate-950'
                          : 'bg-slate-800 text-slate-400'
                      }`}>
                        {isDone ? '✓' : index + 1}
                      </span>
                      <div className="flex-1 min-w-0">
                        <div className="truncate font-medium">{mod.title}</div>
                        <div className="text-[10px] text-slate-500 mt-0.5">{mod.duration} • {mod.documents.length} docs</div>
                      </div>
                    </button>
                  );
                })}
              </div>
            </div>
          </div>

          {/* Evaluation Rules Summary Box */}
          <div className="p-4 rounded-2xl bg-indigo-950/40 border border-indigo-800/60 text-xs text-indigo-200 space-y-2">
            <div className="font-bold text-amber-300 flex items-center gap-1.5">
              <Sparkles className="w-4 h-4" />
              <span>Reglas de Evaluación (Sección 12):</span>
            </div>
            <ul className="space-y-1 text-[11px] text-slate-300 list-disc list-inside">
              <li>Duración: <strong>10 minutos</strong> cronometrados.</li>
              <li>Nota mínima aprobatoria: <strong>13/20</strong>.</li>
              <li>Si apruebas (≥13) pasas <strong>directamente al certificado</strong> (sin re-evaluación).</li>
              <li>Si desapruebas (&lt;13) pasas al <strong>2º intento tras 24 horas</strong>.</li>
              <li>La nota del 2º intento <strong>reemplaza a la primera</strong>.</li>
            </ul>
          </div>
        </div>
      </div>

      {/* Exam Modal */}
      {showExamModal && (
        <ExamModal
          enrollment={enrollment}
          courseTitle={course.title}
          onClose={() => setShowExamModal(false)}
        />
      )}
    </div>
  );
};
