import {NotificationBell} from './StudentNotifications';
import { ThemeToggle } from './ThemeToggle';
import React, { useState } from 'react';
import { useApp } from '../context/AppContext';
import {
  ShieldCheck,
  BookOpen,
  Video,
  Award,
  CreditCard,
  MessageSquare,
  Settings,
  UserCheck,
  FileText,
  LifeBuoy,
  ExternalLink,
  Clock,
  Sparkles,
  Users,
  Code2,
  Download,
  FileArchive, Menu, X
} from 'lucide-react';
import { UserRole } from '../types';
import { useAuth } from '../context/AuthContext';

export const Header: React.FC = () => {
  const { user: authenticatedUser } = useAuth();
  const {
    currentUser,
    allUsers,
    switchUserById,
    activeTab,
    setActiveTab,
    setSelectedCourseId,
    orders,
    payments,
    certificates,
    tickets
  } = useApp();

  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const pendingPaymentsCount = payments.filter(p => p.status === 'PENDIENTE' || p.status === 'EN_REVISIÓN').length;
  const duplicateAlertsCount = payments.filter(p => p.status === 'DUPLICADO').length;
  const openTicketsCount = tickets.filter(t => t.status === 'ABIERTO' || t.status === 'EN_ATENCIÓN').length;

  // Calculate days remaining for account validity
  const validUntilDate = new Date(currentUser.validUntil);
  const now = new Date();
  const diffDays = Math.ceil((validUntilDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));

  const roleLabels: Record<UserRole, { label: string; badgeClass: string }> = {
    superadmin: { label: 'Superadministrador', badgeClass: 'bg-purple-50/80 text-purple-700 border-purple-200' },
    admin_academico: { label: 'Admin Académico', badgeClass: 'bg-blue-50/80 text-blue-700 border-blue-200' },
    admin_financiero: { label: 'Admin Financiero', badgeClass: 'bg-emerald-50/80 text-emerald-700 border-emerald-200' },
    docente: { label: 'Docente Pericial', badgeClass: 'bg-brand/80 text-brand border-brand' },
    soporte: { label: 'Soporte Técnico', badgeClass: 'bg-cyan-50/80 text-cyan-700 border-cyan-200' },
    estudiante: { label: 'Estudiante', badgeClass: 'bg-indigo-50/80 text-indigo-700 border-indigo-200' },
    consultor: { label: 'Consultor Forense', badgeClass: 'bg-rose-50/80 text-rose-700 border-rose-200' },
  };

  return (
    <header className="sticky top-0 z-40 bg-panel/95 backdrop-blur-md border-b border-line shadow-xl">
      {/* Top Banner: Role Quick Switcher & Account Lifecycle Bar */}
      {/* Main Header Bar */}
      <div className="site-header-row max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3.5 flex flex-wrap items-center justify-between gap-4">
        {/* Brand */}
        <div
          onClick={() => { setActiveTab('catalogo'); setSelectedCourseId(null); }}
          className="site-brand flex items-center gap-3 cursor-pointer group"
        >
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-navy to-brand flex items-center justify-center shadow-lg shadow-brand/20 group-hover:scale-105 transition-transform">
            <ShieldCheck className="w-6 h-6 text-ink" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <span className="font-bold text-lg text-ink tracking-tight group-hover:text-brand transition-colors">
                ACADEMIA FORENSE
              </span>
              <span className="text-[10px] uppercase font-bold tracking-wider px-1.5 py-0.5 rounded bg-brand/10 text-brand border border-brand/30">
                Oficial
              </span>
            </div>
            <p className="text-[11px] text-muted font-medium">Formación especializada en ciencias forenses</p>
          </div>
        </div>

        {/* Navigation Tabs */}
        <nav className="hidden xl:flex order-3 basis-full flex-wrap items-center gap-1 text-sm font-medium">
          <button
            onClick={() => { setActiveTab('catalogo'); setSelectedCourseId(null); }}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 ${
              activeTab === 'catalogo'
                ? 'bg-brand/10 text-brand font-semibold border border-brand/20'
                : 'text-muted hover:text-ink hover:bg-surface-low/60'
            }`}
          >
            <BookOpen className="w-4 h-4" />
            <span>Catálogo</span>
          </button>

          <button
            onClick={() => setActiveTab('mis-cursos')}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 relative ${
              activeTab === 'mis-cursos'
                ? 'bg-brand/10 text-brand font-semibold border border-brand/20'
                : 'text-muted hover:text-ink hover:bg-surface-low/60'
            }`}
          >
            <Video className="w-4 h-4 text-brand" />
            <span>Mis Cursos y Evaluaciones</span>
          </button>

          <button
            onClick={() => setActiveTab('mis-eventos')}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 ${
              activeTab === 'mis-eventos'
                ? 'bg-brand/10 text-brand font-semibold border border-brand/20'
                : 'text-muted hover:text-ink hover:bg-surface-low/60'
            }`}
          >
            <Clock className="w-4 h-4" />
            <span>Eventos en Vivo</span>
          </button>

          <button
            onClick={() => setActiveTab('asesoria')}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 ${
              activeTab === 'asesoria'
                ? 'bg-brand/10 text-brand font-semibold border border-brand/20'
                : 'text-muted hover:text-ink hover:bg-surface-low/60'
            }`}
          >
            <MessageSquare className="w-4 h-4 text-emerald-700" />
            <span>Asesoría Pericial</span>
          </button>

          <button
            onClick={() => setActiveTab('mis-certificados')}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 ${
              activeTab === 'mis-certificados'
                ? 'bg-brand/10 text-brand font-semibold border border-brand/20'
                : 'text-muted hover:text-ink hover:bg-surface-low/60'
            }`}
          >
            <Award className="w-4 h-4 text-brand" />
            <span>Certificados QR</span>
            {certificates.filter(c => c.userId === currentUser.id).length > 0 && (
              <span className="w-2 h-2 rounded-full bg-brand"></span>
            )}
          </button>

          <button
            onClick={() => setActiveTab('mis-pagos')}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 ${
              activeTab === 'mis-pagos'
                ? 'bg-brand/10 text-brand font-semibold border border-brand/20'
                : 'text-muted hover:text-ink hover:bg-surface-low/60'
            }`}
          >
            <CreditCard className="w-4 h-4" />
            <span>Mis Pagos</span>
          </button>

          <button
            onClick={() => setActiveTab('verificador-publico')}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 ${
              activeTab === 'verificador-publico'
                ? 'bg-brand/10 text-brand font-semibold border border-brand/20'
                : 'text-muted hover:text-ink hover:bg-surface-low/60'
            }`}
          >
            <ExternalLink className="w-4 h-4 text-cyan-700" />
            <span>Validador QR</span>
          </button>

          {/* Admin panel tab for privileged roles */}
          {currentUser.role !== 'estudiante' && (
            <button
              onClick={() => setActiveTab('admin')}
              className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 relative ${
                activeTab === 'admin'
                  ? 'bg-purple-600/20 text-purple-700 font-semibold border border-purple-500/40'
                  : 'text-purple-700 hover:bg-purple-50/40'
              }`}
            >
              <Settings className="w-4 h-4 text-purple-700" />
              <span>{currentUser.role==='consultor'?'Panel de atención':'Panel Admin'}</span>
              {(pendingPaymentsCount > 0 || duplicateAlertsCount > 0) && (
                <span className="px-1.5 py-0.2 rounded-full text-[10px] font-bold bg-rose-600 text-ink animate-pulse">
                  {pendingPaymentsCount + duplicateAlertsCount}
                </span>
              )}
            </button>
          )}
        </nav>

        {/* User Badge & Profile Button */}
        <div className="flex items-center gap-3">
          <NotificationBell />
          <ThemeToggle />
          <button type="button" className="xl:hidden inline-flex items-center justify-center min-h-11 min-w-11 rounded-xl border border-line bg-surface-low text-ink"
            aria-label={isMobileMenuOpen ? 'Cerrar menú' : 'Abrir menú'} aria-expanded={isMobileMenuOpen} aria-controls="mobile-navigation"
            onClick={() => setIsMobileMenuOpen(open => !open)}>
            {isMobileMenuOpen ? <X size={20} /> : <Menu size={20} />}
          </button>
          <button
            onClick={() => setActiveTab('perfil')}
            className={`hidden sm:flex items-center gap-2.5 px-3 py-1.5 rounded-lg border transition-all text-left ${
              activeTab === 'perfil'
                ? 'bg-surface-low border-brand/50 shadow-sm'
                : 'bg-panel border-line/80 hover:bg-surface-low'
            }`}
          >
            <div className="w-8 h-8 rounded-full bg-brand/20 border border-brand/40 flex items-center justify-center text-brand font-bold text-xs">
              {currentUser.nombres.charAt(0)}{currentUser.apellidos.charAt(0)}
            </div>
            <div className="hidden sm:block">
              <div className="text-xs font-semibold text-ink truncate max-w-[140px]">
                {currentUser.nombres.split(' ')[0]} {currentUser.apellidos.split(' ')[0]}
              </div>
              <div className="flex items-center gap-1.5">
                <span className={`text-[10px] px-1.5 py-0.2 rounded border font-medium ${roleLabels[currentUser.role].badgeClass}`}>
                  {roleLabels[currentUser.role].label}
                </span>
              </div>
            </div>
          </button>
        </div>
      </div>

      {/* Mobile / Responsive secondary subnav for smaller screens */}
      <nav id="mobile-navigation" aria-label="Navegación principal" className={`${isMobileMenuOpen ? 'grid' : 'hidden'} xl:hidden mobile-navigation grid-cols-2 sm:grid-cols-3 gap-2 px-4 py-3 bg-canvas border-t border-line text-sm`}
        onClick={event => { if ((event.target as HTMLElement).closest('button')) setIsMobileMenuOpen(false); }}
        onKeyDown={event => { if (event.key === 'Escape') { setIsMobileMenuOpen(false); document.querySelector<HTMLButtonElement>('[aria-controls="mobile-navigation"]')?.focus(); } }}>
        <button
          onClick={() => { setActiveTab('catalogo'); setSelectedCourseId(null); }}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'catalogo' ? 'bg-brand text-ink font-bold' : 'text-muted'}`}
        >
          Catálogo
        </button>
        <button
          onClick={() => setActiveTab('mis-cursos')}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'mis-cursos' ? 'bg-brand text-ink font-bold' : 'text-muted'}`}
        >
          Cursos y Evaluaciones
        </button>
        <button
          onClick={() => setActiveTab('mis-eventos')}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'mis-eventos' ? 'bg-brand text-ink font-bold' : 'text-muted'}`}
        >
          Eventos en Vivo
        </button>
        <button
          onClick={() => setActiveTab('asesoria')}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'asesoria' ? 'bg-brand text-ink font-bold' : 'text-muted'}`}
        >
          Asesoría
        </button>
        <button
          onClick={() => setActiveTab('mis-certificados')}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'mis-certificados' ? 'bg-brand text-ink font-bold' : 'text-muted'}`}
        >
          Certificados QR
        </button>
        <button
          onClick={() => setActiveTab('mis-pagos')}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'mis-pagos' ? 'bg-brand text-ink font-bold' : 'text-muted'}`}
        >
          Mis Pagos
        </button>
        <button
          onClick={() => setActiveTab('verificador-publico')}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'verificador-publico' ? 'bg-brand text-ink font-bold' : 'text-muted'}`}
        >
          Validador QR
        </button>
        {currentUser.role !== 'estudiante' && (
          <button
            onClick={() => setActiveTab('admin')}
            className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'admin' ? 'bg-purple-600 text-ink font-bold' : 'text-purple-700'}`}
          >
            Admin ({pendingPaymentsCount + duplicateAlertsCount})
          </button>
        )}
        <button onClick={() => setActiveTab('perfil')} className="text-left px-3 py-2 text-muted">Mi perfil</button>
        {authenticatedUser && <button onClick={() => setActiveTab('notificaciones')} className="text-left px-3 py-2 text-muted">Notificaciones</button>}
        <button onClick={() => setActiveTab('mis-consultas')} className="text-left px-3 py-2 text-muted">Mis consultas</button>
      </nav>

    </header>
  );
};
