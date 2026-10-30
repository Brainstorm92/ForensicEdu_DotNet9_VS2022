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
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-10 animate-in fade-in duration-300">
      {/* Hero Catalog Banner */}
      <div className="relative rounded-3xl overflow-hidden bg-gradient-to-r from-slate-900 via-slate-850 to-slate-900 border border-slate-800 p-8 sm:p-12 shadow-2xl">
        <div className="max-w-3xl space-y-4">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-amber-500/10 border border-amber-400/30 text-amber-300 text-xs font-semibold">
            <Sparkles className="w-4 h-4 text-amber-400" />
            <span>Formación y Peritaje Criminalístico de Élite</span>
          </div>
          <h1 className="text-3xl sm:text-4xl font-extrabold text-white tracking-tight leading-tight">
            Programas Oficiales en Ciencias Forenses y Certificación con QR
          </h1>
          <p className="text-slate-300 text-sm sm:text-base leading-relaxed">
            Explora las 8 especialidades periciales, congresos y seminarios en vivo, y asesorías personalizadas en juicio oral. Cuentas con aula virtual con streaming protegido, exámenes cronometrados de 10 min y diplomas verificables.
          </p>

          {/* Quick Stats */}
          <div className="pt-2 flex flex-wrap gap-6 text-xs text-slate-400">
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-emerald-400" />
              <span>8 Especialidades Forenses</span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-emerald-400" />
              <span>Certificados QR Válidos</span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-emerald-400" />
              <span>Asesoría Pericial Directa</span>
            </div>
          </div>
        </div>
      </div>

      {/* Filter and Search Bar */}
      <div className="flex flex-col sm:flex-row items-center justify-between gap-4 p-2 bg-slate-900 border border-slate-800 rounded-2xl">
        {/* Category Pills */}
        <div className="flex items-center gap-1.5 overflow-x-auto w-full sm:w-auto p-1">
          <button
            onClick={() => setActiveFilter('all')}
            className={`px-4 py-2 rounded-xl text-xs font-semibold transition-all whitespace-nowrap ${
              activeFilter === 'all'
                ? 'bg-amber-500 text-slate-950 shadow-md shadow-amber-500/20'
                : 'text-slate-300 hover:text-white hover:bg-slate-800'
            }`}
          >
            Todos los Productos ({courses.length + events.length + 1})
          </button>
          <button
            onClick={() => setActiveFilter('cursos')}
            className={`px-4 py-2 rounded-xl text-xs font-semibold transition-all whitespace-nowrap ${
              activeFilter === 'cursos'
                ? 'bg-amber-500 text-slate-950 shadow-md shadow-amber-500/20'
                : 'text-slate-300 hover:text-white hover:bg-slate-800'
            }`}
          >
            Cursos Forenses ({courses.length})
          </button>
          <button
            onClick={() => setActiveFilter('eventos')}
            className={`px-4 py-2 rounded-xl text-xs font-semibold transition-all whitespace-nowrap ${
              activeFilter === 'eventos'
                ? 'bg-amber-500 text-slate-950 shadow-md shadow-amber-500/20'
                : 'text-slate-300 hover:text-white hover:bg-slate-800'
            }`}
          >
            Eventos en Vivo ({events.length})
          </button>
          <button
            onClick={() => setActiveFilter('asesoria')}
            className={`px-4 py-2 rounded-xl text-xs font-semibold transition-all whitespace-nowrap ${
              activeFilter === 'asesoria'
                ? 'bg-amber-500 text-slate-950 shadow-md shadow-amber-500/20'
                : 'text-slate-300 hover:text-white hover:bg-slate-800'
            }`}
          >
            Asesoría Pericial (1)
          </button>
        </div>

        {/* Search */}
        <div className="relative w-full sm:w-72 px-1">
          <Search className="w-4 h-4 text-slate-400 absolute left-4 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="Buscar por especialidad..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full bg-slate-950 border border-slate-700/80 rounded-xl pl-10 pr-4 py-2 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-amber-400"
          />
        </div>
      </div>

      {/* SECTION 5.1: 8 CURSOS FORENSES */}
      {(activeFilter === 'all' || activeFilter === 'cursos') && (
        <section className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="text-xl font-bold text-white flex items-center gap-2">
                <BookOpen className="w-5 h-5 text-amber-400" />
                <span>Cursos Especializados en Ciencias Forenses (Sección 5.1)</span>
              </h2>
              <p className="text-xs text-slate-400">Modalidad virtual asincrónica con streaming protegido y certificación QR</p>
            </div>
            <span className="text-xs font-semibold text-slate-400">
              {filteredCourses.length} cursos listados
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {filteredCourses.map((course) => {
              const enrolled = isEnrolled(course.id);
              return (
                <div
                  key={course.id}
                  className="bg-slate-900 border border-slate-800 hover:border-slate-700 rounded-2xl overflow-hidden flex flex-col group transition-all duration-200 hover:-translate-y-1 shadow-lg"
                >
                  {/* Banner */}
                  <div className="relative h-44 overflow-hidden bg-slate-950">
                    <img
                      src={course.bannerUrl}
                      alt={course.title}
                      className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                    />
                    <div className="absolute inset-0 bg-gradient-to-t from-slate-950 via-slate-950/40 to-transparent"></div>
                    <span className="absolute top-3 left-3 text-[10px] font-mono font-bold tracking-wider px-2 py-0.5 rounded bg-slate-900/90 text-amber-300 border border-amber-500/30">
                      {course.code}
                    </span>
                    {enrolled && (
                      <span className="absolute top-3 right-3 text-[10px] font-bold px-2 py-0.5 rounded-full bg-emerald-500 text-slate-950 flex items-center gap-1 shadow-md">
                        <CheckCircle2 className="w-3 h-3" />
                        <span>Matriculado</span>
                      </span>
                    )}
                    <div className="absolute bottom-3 left-3 right-3 flex items-center justify-between text-xs text-slate-300">
                      <span className="flex items-center gap-1">
                        <Clock className="w-3.5 h-3.5 text-amber-400" />
                        <span>{course.durationHours} horas acad.</span>
                      </span>
                      <span className="font-bold text-amber-400 text-sm">
                        S/. {course.price.toFixed(2)}
                      </span>
                    </div>
                  </div>

                  {/* Body */}
                  <div className="p-4 flex-1 flex flex-col justify-between space-y-3">
                    <div>
                      <h3 className="font-bold text-base text-white group-hover:text-amber-400 transition-colors line-clamp-1">
                        {course.title}
                      </h3>
                      <p className="text-xs text-slate-400 mt-1 line-clamp-2 leading-relaxed">
                        {course.shortDescription}
                      </p>
                    </div>

                    <div className="pt-2 border-t border-slate-800/80 space-y-3">
                      <div className="flex items-center gap-2 text-[11px] text-slate-400">
                        <User className="w-3.5 h-3.5 text-slate-500 shrink-0" />
                        <span className="truncate">{course.instructor}</span>
                      </div>

                      <div className="flex gap-2">
                        <button
                          onClick={() => setCourseDetailsModal(course)}
                          className="flex-1 py-2 px-2.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 text-xs font-medium transition-colors"
                        >
                          Ver Detalles
                        </button>

                        {enrolled ? (
                          <button
                            onClick={() => {
                              setSelectedCourseId(course.id);
                              setActiveTab('mis-cursos');
                            }}
                            className="py-2 px-3 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-bold transition-colors flex items-center gap-1"
                          >
                            <Video className="w-3.5 h-3.5" />
                            <span>Aula</span>
                          </button>
                        ) : (
                          <button
                            onClick={() => setSelectedProduct({ item: course, type: 'curso' })}
                            className="py-2 px-3 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 text-xs font-bold transition-colors flex items-center gap-1 shadow-md shadow-amber-500/20"
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
              <h2 className="text-xl font-bold text-white flex items-center gap-2">
                <Calendar className="w-5 h-5 text-amber-400" />
                <span>Eventos en Vivo & Congresos (Sección 5.2 & 14)</span>
              </h2>
              <p className="text-xs text-slate-400">Transmisiones en tiempo real, congresos periciales y seminarios interactivos</p>
            </div>
            <span className="text-xs font-semibold text-slate-400">
              {filteredEvents.length} eventos
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {filteredEvents.map((evt) => (
              <div
                key={evt.id}
                className="bg-slate-900 border border-slate-800 rounded-2xl overflow-hidden flex flex-col group hover:border-slate-700 transition-all duration-200 hover:-translate-y-1 shadow-lg"
              >
                <div className="relative h-40 overflow-hidden bg-slate-950">
                  <img
                    src={evt.bannerUrl}
                    alt={evt.title}
                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                  />
                  <div className="absolute inset-0 bg-gradient-to-t from-slate-950 via-slate-950/40 to-transparent"></div>
                  <span className="absolute top-3 left-3 text-[10px] font-bold px-2 py-0.5 rounded bg-indigo-900/90 text-indigo-200 border border-indigo-700">
                    {evt.type}
                  </span>
                  {evt.accessStatus === 'EN_VIVO' && (
                    <span className="absolute top-3 right-3 text-[10px] font-bold px-2 py-0.5 rounded-full bg-rose-600 text-white flex items-center gap-1 animate-pulse shadow-md">
                      <span className="w-1.5 h-1.5 rounded-full bg-white"></span>
                      <span>EN VIVO</span>
                    </span>
                  )}
                  <div className="absolute bottom-2.5 left-3 right-3 flex items-center justify-between text-xs text-slate-300">
                    <span>{evt.date}</span>
                    <span className="font-bold text-amber-400 text-sm">S/. {evt.price.toFixed(2)}</span>
                  </div>
                </div>

                <div className="p-4 flex-1 flex flex-col justify-between space-y-3">
                  <div>
                    <h3 className="font-bold text-sm text-white group-hover:text-amber-400 transition-colors line-clamp-2">
                      {evt.title}
                    </h3>
                    <p className="text-xs text-slate-400 mt-1 line-clamp-2">
                      {evt.description}
                    </p>
                  </div>

                  <div className="pt-2 border-t border-slate-800 space-y-2.5">
                    <div className="text-[11px] text-slate-400 truncate">
                      Ponente: {evt.speaker}
                    </div>
                    <button
                      onClick={() => setSelectedProduct({ item: evt, type: 'evento' })}
                      className="w-full py-2 px-3 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 text-xs font-bold transition-colors flex items-center justify-center gap-1.5 shadow-md shadow-amber-500/20"
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
          <div className="p-6 sm:p-8 rounded-3xl bg-gradient-to-r from-slate-900 via-emerald-950/40 to-slate-900 border border-emerald-800/60 shadow-xl relative overflow-hidden">
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 items-center">
              <div className="lg:col-span-2 space-y-3">
                <span className="text-[10px] font-bold uppercase tracking-wider px-2.5 py-0.5 rounded-full bg-emerald-500/20 text-emerald-300 border border-emerald-500/40">
                  Servicio Especializado Independiente (Sección 6)
                </span>
                <h3 className="text-2xl font-bold text-white">
                  {advisory.title}
                </h3>
                <p className="text-xs sm:text-sm text-slate-300 leading-relaxed">
                  {advisory.description}
                </p>
                <div className="pt-2 flex flex-wrap gap-4 text-xs text-slate-300">
                  <div className="flex items-center gap-1.5">
                    <User className="w-4 h-4 text-amber-400" />
                    <span>Consultor: <strong className="text-white">{advisory.specialist}</strong> ({advisory.specialty})</span>
                  </div>
                  <div className="flex items-center gap-1.5">
                    <MessageCircle className="w-4 h-4 text-emerald-400" />
                    <span>Conexión directa por WhatsApp</span>
                  </div>
                </div>
              </div>

              <div className="bg-slate-950/80 p-5 rounded-2xl border border-slate-800 text-center space-y-3">
                <div className="text-xs text-slate-400">Honorario por Sesión Pericial:</div>
                <div className="text-3xl font-extrabold text-amber-400">
                  S/. {advisory.price.toFixed(2)} <span className="text-xs text-slate-400 font-normal">PEN</span>
                </div>
                <button
                  onClick={() => setSelectedProduct({ item: advisory, type: 'asesoria' })}
                  className="w-full py-2.5 px-4 rounded-xl bg-emerald-500 hover:bg-emerald-400 text-slate-950 font-bold text-xs flex items-center justify-center gap-2 shadow-lg shadow-emerald-500/20 transition-all"
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
        <div className="fixed inset-0 z-50 bg-slate-950/80 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-slate-900 border border-slate-700 rounded-3xl max-w-2xl w-full max-h-[90vh] overflow-y-auto shadow-2xl p-6 space-y-5">
            <div className="flex items-start justify-between gap-4">
              <div>
                <span className="text-xs font-mono font-bold text-amber-400">{courseDetailsModal.code}</span>
                <h3 className="text-xl font-bold text-white mt-1">{courseDetailsModal.title}</h3>
                <p className="text-xs text-slate-400">{courseDetailsModal.instructorTitle}</p>
              </div>
              <button 
                onClick={() => setCourseDetailsModal(null)}
                className="p-1 rounded-md text-slate-400 hover:text-white"
              >
                ✕
              </button>
            </div>

            <p className="text-xs text-slate-300 leading-relaxed">
              {courseDetailsModal.fullDescription}
            </p>

            <div className="space-y-3">
              <h4 className="font-bold text-xs text-amber-400 uppercase tracking-wider">
                Módulos del Curso y Streaming Autenticado (Sección 11.2)
              </h4>
              <div className="space-y-2">
                {courseDetailsModal.modules.length > 0 ? (
                  courseDetailsModal.modules.map((m, idx) => (
                    <div key={m.id} className="p-3 rounded-xl bg-slate-950 border border-slate-800 flex items-center justify-between text-xs">
                      <div className="flex items-center gap-2.5">
                        <Video className="w-4 h-4 text-slate-400" />
                        <span className="text-white font-medium">{m.title}</span>
                      </div>
                      <span className="text-slate-500 text-[11px]">{m.duration}</span>
                    </div>
                  ))
                ) : (
                  <div className="p-3 rounded-xl bg-slate-950 text-slate-400 text-xs">
                    4 Módulos de especialización pericial grabados en alta definición con casos forenses reales.
                  </div>
                )}
              </div>
            </div>

            <div className="space-y-2">
              <h4 className="font-bold text-xs text-amber-400 uppercase tracking-wider">
                Materiales Educativos Incluidos (Sección 11.1)
              </h4>
              <ul className="text-xs text-slate-300 space-y-1.5 list-disc list-inside">
                {courseDetailsModal.materialesIncluidos.map((mat, i) => (
                  <li key={i}>{mat}</li>
                ))}
              </ul>
            </div>

            <div className="pt-4 border-t border-slate-800 flex items-center justify-between">
              <div>
                <span className="text-xs text-slate-400">Inversión:</span>
                <span className="text-xl font-bold text-amber-400 ml-2">S/. {courseDetailsModal.price.toFixed(2)}</span>
              </div>
              <div className="flex gap-2">
                <button
                  onClick={() => setCourseDetailsModal(null)}
                  className="px-4 py-2 rounded-xl bg-slate-800 text-slate-300 text-xs"
                >
                  Cerrar
                </button>
                <button
                  onClick={() => {
                    const c = courseDetailsModal;
                    setCourseDetailsModal(null);
                    setSelectedProduct({ item: c, type: 'curso' });
                  }}
                  className="px-5 py-2 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs"
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
