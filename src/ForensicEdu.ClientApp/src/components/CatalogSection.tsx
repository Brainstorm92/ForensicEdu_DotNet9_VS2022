import React, { useState } from 'react';
import { useApp } from '../context/AppContext';
import {
  BookOpen,
  Clock,
  User,
  ShieldCheck,
  Video,
  Calendar,
  ArrowRight,
  CheckCircle2,
  Search,
  Filter,
  Sparkles,
  MessageCircle,
  FileText,
  Lock
} from 'lucide-react';
import { Course, EventItem, AdvisoryItem, ProductType } from '../types';
import { OrderAndPaymentModal } from './OrderAndPaymentModal';

export const CatalogSection: React.FC = () => {
  const { courses, events, advisory, enrollments, setActiveTab, setSelectedCourseId } = useApp();

  const [activeFilter, setActiveFilter] = useState<'all' | 'cursos' | 'eventos' | 'asesoria'>('all');
  const [searchQuery, setSearchQuery] = useState('');

  // Modals
  const [selectedProduct, setSelectedProduct] = useState<{
    item: Course | EventItem | AdvisoryItem;
    type: ProductType;
  } | null>(null);

  const [courseDetailsModal, setCourseDetailsModal] = useState<Course | null>(null);

  const filteredCourses = courses.filter(c =>
    c.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    c.shortDescription.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const filteredEvents = events.filter(e =>
    e.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    e.description.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const isEnrolled = (courseId: string) => {
    return enrollments.some(e => e.courseId === courseId && (e.status === 'ACTIVA' || e.status === 'COMPLETADA'));
  };

  return (
    <div className="stitch-catalog max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-10 animate-in fade-in duration-300">
      {/* Hero Catalog Banner */}
      <div className="stitch-hero relative rounded-3xl overflow-hidden bg-gradient-to-r from-panel via-surface-low to-panel border border-line p-8 sm:p-12 shadow-xl">
        <div className="max-w-3xl space-y-4">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-brand/10 border border-brand/30 text-brand text-xs font-semibold">
            <Sparkles className="w-4 h-4 text-brand" />
            <span>Formación especializada · Ciencias forenses</span>
          </div>
          <h1 className="text-3xl sm:text-4xl font-extrabold text-ink tracking-tight leading-tight">
            Ciencia, formación y precisión forense
          </h1>
          <p className="text-muted text-sm sm:text-base leading-relaxed">
            Explora las 8 especialidades periciales, congresos y seminarios en vivo, y asesorías personalizadas en juicio oral. Cuentas con aula virtual con streaming protegido, exámenes cronometrados de 10 min y diplomas verificables.
          </p>

          {/* Quick Stats */}
          <div className="pt-2 flex flex-wrap gap-6 text-xs text-muted">
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-emerald-700" />
              <span>8 Especialidades Forenses</span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-emerald-700" />
              <span>Certificados QR Válidos</span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-emerald-700" />
              <span>Asesoría Pericial Directa</span>
            </div>
          </div>
        </div>
      </div>

      {/* Filter and Search Bar */}
      <div className="flex flex-col sm:flex-row items-center justify-between gap-4 p-2 bg-panel border border-line rounded-2xl">
        {/* Category Pills */}
        <div className="flex items-center gap-1.5 overflow-x-auto w-full sm:w-auto p-1">
          <button
            onClick={() => setActiveFilter('all')}
            className={`px-4 py-2 rounded-xl text-xs font-semibold transition-all whitespace-nowrap ${
              activeFilter === 'all'
                ? 'bg-brand text-ink shadow-md shadow-brand/20'
                : 'text-muted hover:text-ink hover:bg-surface-low'
            }`}
          >
            Todos los Productos ({courses.length + events.length + 1})
          </button>
          <button
            onClick={() => setActiveFilter('cursos')}
            className={`px-4 py-2 rounded-xl text-xs font-semibold transition-all whitespace-nowrap ${
              activeFilter === 'cursos'
                ? 'bg-brand text-ink shadow-md shadow-brand/20'
                : 'text-muted hover:text-ink hover:bg-surface-low'
            }`}
          >
            Cursos Forenses ({courses.length})
          </button>
          <button
            onClick={() => setActiveFilter('eventos')}
            className={`px-4 py-2 rounded-xl text-xs font-semibold transition-all whitespace-nowrap ${
              activeFilter === 'eventos'
                ? 'bg-brand text-ink shadow-md shadow-brand/20'
                : 'text-muted hover:text-ink hover:bg-surface-low'
            }`}
          >
            Eventos en Vivo ({events.length})
          </button>
          <button
            onClick={() => setActiveFilter('asesoria')}
            className={`px-4 py-2 rounded-xl text-xs font-semibold transition-all whitespace-nowrap ${
              activeFilter === 'asesoria'
                ? 'bg-brand text-ink shadow-md shadow-brand/20'
                : 'text-muted hover:text-ink hover:bg-surface-low'
            }`}
          >
            Asesoría Pericial (1)
          </button>
        </div>

        {/* Search */}
        <div className="relative w-full sm:w-72 px-1">
          <Search className="w-4 h-4 text-muted absolute left-4 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="Buscar por especialidad..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full bg-canvas border border-line/80 rounded-xl pl-10 pr-4 py-2 text-xs text-ink placeholder-slate-500 focus:outline-none focus:border-brand"
          />
        </div>
      </div>

      {/* SECTION 5.1: 8 CURSOS FORENSES */}
      {(activeFilter === 'all' || activeFilter === 'cursos') && (
        <section className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="text-xl font-bold text-ink flex items-center gap-2">
                <BookOpen className="w-5 h-5 text-brand" />
                <span>Cursos de Especialización Forense</span>
              </h2>
              <p className="text-xs text-muted">Modalidad virtual asincrónica con streaming protegido y certificación QR</p>
            </div>
            <span className="text-xs font-semibold text-muted">
              {filteredCourses.length} cursos listados
            </span>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-6">
            {filteredCourses.map((course) => {
              const enrolled = isEnrolled(course.id);
              return (
                <div
                  key={course.id}
                  className="bg-panel border border-line hover:border-line rounded-2xl overflow-hidden flex flex-col group transition-all duration-200 hover:-translate-y-1 shadow-lg"
                >
                  {/* Banner */}
                  <div className="relative h-44 overflow-hidden bg-canvas">
                    <img
                      src={course.bannerUrl}
                      alt={course.title}
                      className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                    />
                    <div className="absolute inset-0 stitch-image-shade bg-gradient-to-t from-navy via-navy/40 to-transparent"></div>
                    <span className="absolute top-3 left-3 text-[10px] font-mono font-bold tracking-wider px-2 py-0.5 rounded bg-panel/90 text-brand border border-brand/30">
                      {course.code}
                    </span>
                    {enrolled && (
                      <span className="absolute top-3 right-3 text-[10px] font-bold px-2 py-0.5 rounded-full bg-emerald-500 text-ink flex items-center gap-1 shadow-md">
                        <CheckCircle2 className="w-3 h-3" />
                        <span>Matriculado</span>
                      </span>
                    )}
                    <div className="absolute bottom-3 left-3 right-3 flex items-center justify-between text-xs text-muted">
                      <span className="flex items-center gap-1">
                        <Clock className="w-3.5 h-3.5 text-brand" />
                        <span>{course.durationHours} horas acad.</span>
                      </span>
                      <span className="font-bold text-brand text-sm">
                        S/. {course.price.toFixed(2)}
                      </span>
                    </div>
                  </div>

                  {/* Body */}
                  <div className="p-4 flex-1 flex flex-col justify-between space-y-3">
                    <div>
                      <h3 className="font-bold text-base text-ink group-hover:text-brand transition-colors line-clamp-1">
                        {course.title}
                      </h3>
                      <p className="text-xs text-muted mt-1 line-clamp-2 leading-relaxed">
                        {course.shortDescription}
                      </p>
                    </div>

                    <div className="pt-2 border-t border-line/80 space-y-3">
                      <div className="flex items-center gap-2 text-[11px] text-muted">
                        <User className="w-3.5 h-3.5 text-muted shrink-0" />
                        <span className="truncate">{course.instructor}</span>
                      </div>

                      <div className="flex gap-2">
                        <button
                          onClick={() => setCourseDetailsModal(course)}
                          className="flex-1 py-2 px-2.5 rounded-xl bg-surface-low hover:bg-surface-high text-ink text-xs font-medium transition-colors"
                        >
                          Ver Detalles
                        </button>

                        {enrolled ? (
                          <button
                            onClick={() => {
                              setSelectedCourseId(course.id);
                              setActiveTab('mis-cursos');
                            }}
                            className="py-2 px-3 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-ink text-xs font-bold transition-colors flex items-center gap-1"
                          >
                            <Video className="w-3.5 h-3.5" />
                            <span>Aula</span>
                          </button>
                        ) : (
                          <button
                            onClick={() => setSelectedProduct({ item: course, type: 'curso' })}
                            className="py-2 px-3 rounded-xl bg-brand hover:bg-brand text-ink text-xs font-bold transition-colors flex items-center gap-1 shadow-md shadow-brand/20"
                          >
                            <span>Comprar</span>
                            <ArrowRight className="w-3.5 h-3.5" />
                          </button>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </section>
      )}

      {/* SECTION 5.2: EVENTOS EN VIVO */}
      {(activeFilter === 'all' || activeFilter === 'eventos') && (
        <section className="space-y-4 pt-4">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="text-xl font-bold text-ink flex items-center gap-2">
                <Calendar className="w-5 h-5 text-brand" />
                <span>Eventos en Vivo & Congresos (Sección 5.2 & 14)</span>
              </h2>
              <p className="text-xs text-muted">Transmisiones en tiempo real, congresos periciales y seminarios interactivos</p>
            </div>
            <span className="text-xs font-semibold text-muted">
              {filteredEvents.length} eventos
            </span>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-6">
            {filteredEvents.map((evt) => (
              <div
                key={evt.id}
                className="bg-panel border border-line rounded-2xl overflow-hidden flex flex-col group hover:border-line transition-all duration-200 hover:-translate-y-1 shadow-lg"
              >
                <div className="relative h-40 overflow-hidden bg-canvas">
                  <img
                    src={evt.bannerUrl}
                    alt={evt.title}
                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                  />
                  <div className="absolute inset-0 stitch-image-shade bg-gradient-to-t from-navy via-navy/40 to-transparent"></div>
                  <span className="absolute top-3 left-3 text-[10px] font-bold px-2 py-0.5 rounded bg-indigo-50/90 text-indigo-700 border border-indigo-200">
                    {evt.type}
                  </span>
                  {evt.accessStatus === 'EN_VIVO' && (
                    <span className="absolute top-3 right-3 text-[10px] font-bold px-2 py-0.5 rounded-full bg-rose-600 text-ink flex items-center gap-1 animate-pulse shadow-md">
                      <span className="w-1.5 h-1.5 rounded-full bg-panel"></span>
                      <span>EN VIVO</span>
                    </span>
                  )}
                  <div className="absolute bottom-2.5 left-3 right-3 flex items-center justify-between text-xs text-muted">
                    <span>{evt.date}</span>
                    <span className="font-bold text-brand text-sm">S/. {evt.price.toFixed(2)}</span>
                  </div>
                </div>

                <div className="p-4 flex-1 flex flex-col justify-between space-y-3">
                  <div>
                    <h3 className="font-bold text-sm text-ink group-hover:text-brand transition-colors line-clamp-2">
                      {evt.title}
                    </h3>
                    <p className="text-xs text-muted mt-1 line-clamp-2">
                      {evt.description}
                    </p>
                  </div>

                  <div className="pt-2 border-t border-line space-y-2.5">
                    <div className="text-[11px] text-muted truncate">
                      Ponente: {evt.speaker}
                    </div>
                    <button
                      onClick={() => setSelectedProduct({ item: evt, type: 'evento' })}
                      className="w-full py-2 px-3 rounded-xl bg-brand hover:bg-brand text-ink text-xs font-bold transition-colors flex items-center justify-center gap-1.5 shadow-md shadow-brand/20"
                    >
                      <span>Inscribirse al Evento</span>
                      <ArrowRight className="w-3.5 h-3.5" />
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </section>
      )}

      {/* SECTION 6: ASESORÍA PERSONALIZADA */}
      {(activeFilter === 'all' || activeFilter === 'asesoria') && (
        <section className="pt-4">
          <div className="p-6 sm:p-8 rounded-3xl bg-gradient-to-r from-panel via-emerald-950/40 to-panel border border-emerald-200/60 shadow-xl relative overflow-hidden">
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 items-center">
              <div className="lg:col-span-2 space-y-3">
                <span className="text-[10px] font-bold uppercase tracking-wider px-2.5 py-0.5 rounded-full bg-emerald-500/20 text-emerald-700 border border-emerald-500/40">
                  Servicio Especializado Independiente
                </span>
                <h3 className="text-2xl font-bold text-ink">
                  {advisory.title}
                </h3>
                <p className="text-xs sm:text-sm text-muted leading-relaxed">
                  {advisory.description}
                </p>
                <div className="pt-2 flex flex-wrap gap-4 text-xs text-muted">
                  <div className="flex items-center gap-1.5">
                    <User className="w-4 h-4 text-brand" />
                    <span>Consultor: <strong className="text-ink">{advisory.specialist}</strong> ({advisory.specialty})</span>
                  </div>
                  <div className="flex items-center gap-1.5">
                    <MessageCircle className="w-4 h-4 text-emerald-700" />
                    <span>Conexión directa por WhatsApp</span>
                  </div>
                </div>
              </div>

              <div className="bg-canvas/80 p-5 rounded-2xl border border-line text-center space-y-3">
                <div className="text-xs text-muted">Honorario por Sesión Pericial:</div>
                <div className="text-3xl font-extrabold text-brand">
                  S/. {advisory.price.toFixed(2)} <span className="text-xs text-muted font-normal">PEN</span>
                </div>
                <button
                  onClick={() => setSelectedProduct({ item: advisory, type: 'asesoria' })}
                  className="w-full py-2.5 px-4 rounded-xl bg-emerald-500 hover:bg-emerald-400 text-ink font-bold text-xs flex items-center justify-center gap-2 shadow-lg shadow-emerald-500/20 transition-all"
                >
                  <MessageCircle className="w-4 h-4" />
                  <span>Adquirir y Conectar con Consultor</span>
                </button>
              </div>
            </div>
          </div>
        </section>
      )}

      {/* Order & Payment Modal */}
      {selectedProduct && (
        <OrderAndPaymentModal
          item={selectedProduct.item}
          productType={selectedProduct.type}
          onClose={() => setSelectedProduct(null)}
        />
      )}

      {/* Course Details Modal */}
      {courseDetailsModal && (
        <div className="fixed inset-0 z-50 bg-canvas/80 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-panel border border-line rounded-3xl max-w-2xl w-full max-h-[90vh] overflow-y-auto shadow-xl p-6 space-y-5">
            <div className="flex items-start justify-between gap-4">
              <div>
                <span className="text-xs font-mono font-bold text-brand">{courseDetailsModal.code}</span>
                <h3 className="text-xl font-bold text-ink mt-1">{courseDetailsModal.title}</h3>
                <p className="text-xs text-muted">{courseDetailsModal.instructorTitle}</p>
              </div>
              <button
                onClick={() => setCourseDetailsModal(null)}
                className="p-1 rounded-md text-muted hover:text-ink"
              >
                ✕
              </button>
            </div>

            <p className="text-xs text-muted leading-relaxed">
              {courseDetailsModal.fullDescription}
            </p>

            <div className="space-y-3">
              <h4 className="font-bold text-xs text-brand uppercase tracking-wider">
                Módulos del Curso y Streaming Autenticado
              </h4>
              <div className="space-y-2">
                {courseDetailsModal.modules.length > 0 ? (
                  courseDetailsModal.modules.map((m, idx) => (
                    <div key={m.id} className="p-3 rounded-xl bg-canvas border border-line flex items-center justify-between text-xs">
                      <div className="flex items-center gap-2.5">
                        <Video className="w-4 h-4 text-muted" />
                        <span className="text-ink font-medium">{m.title}</span>
                      </div>
                      <span className="text-muted text-[11px]">{m.duration}</span>
                    </div>
                  ))
                ) : (
                  <div className="p-3 rounded-xl bg-canvas text-muted text-xs">
                    4 Módulos de especialización pericial grabados en alta definición con casos forenses reales.
                  </div>
                )}
              </div>
            </div>

            <div className="space-y-2">
              <h4 className="font-bold text-xs text-brand uppercase tracking-wider">
                Materiales Educativos Incluidos
              </h4>
              <ul className="text-xs text-muted space-y-1.5 list-disc list-inside">
                {courseDetailsModal.materialesIncluidos.map((mat, i) => (
                  <li key={i}>{mat}</li>
                ))}
              </ul>
            </div>

            <div className="pt-4 border-t border-line flex items-center justify-between">
              <div>
                <span className="text-xs text-muted">Inversión:</span>
                <span className="text-xl font-bold text-brand ml-2">S/. {courseDetailsModal.price.toFixed(2)}</span>
              </div>
              <div className="flex gap-2">
                <button
                  onClick={() => setCourseDetailsModal(null)}
                  className="px-4 py-2 rounded-xl bg-surface-low text-muted text-xs"
                >
                  Cerrar
                </button>
                <button
                  onClick={() => {
                    const c = courseDetailsModal;
                    setCourseDetailsModal(null);
                    setSelectedProduct({ item: c, type: 'curso' });
                  }}
                  className="px-5 py-2 rounded-xl bg-brand hover:bg-brand text-ink font-bold text-xs"
                >
                  Comprar Curso
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
