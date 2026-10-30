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
  FileArchive
} from 'lucide-react';
import { UserRole } from '../types';
import { DownloadZipModal } from './DownloadZipModal';
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

  const [isZipModalOpen, setIsZipModalOpen] = useState(false);

  const pendingPaymentsCount = payments.filter(p => p.status === 'PENDIENTE' || p.status === 'EN_REVISIÓN').length;
  const duplicateAlertsCount = payments.filter(p => p.status === 'DUPLICADO').length;
  const openTicketsCount = tickets.filter(t => t.status === 'ABIERTO' || t.status === 'EN_ATENCIÓN').length;

  // Calculate days remaining for account validity
  const validUntilDate = new Date(currentUser.validUntil);
  const now = new Date();
  const diffDays = Math.ceil((validUntilDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));

  const roleLabels: Record<UserRole, { label: string; badgeClass: string }> = {
    superadmin: { label: 'Superadministrador', badgeClass: 'bg-purple-900/80 text-purple-200 border-purple-700' },
    admin_academico: { label: 'Admin Académico', badgeClass: 'bg-blue-900/80 text-blue-200 border-blue-700' },
    admin_financiero: { label: 'Admin Financiero', badgeClass: 'bg-emerald-900/80 text-emerald-200 border-emerald-700' },
    docente: { label: 'Docente Pericial', badgeClass: 'bg-amber-900/80 text-amber-200 border-amber-700' },
    soporte: { label: 'Soporte Técnico', badgeClass: 'bg-cyan-900/80 text-cyan-200 border-cyan-700' },
    estudiante: { label: 'Estudiante', badgeClass: 'bg-indigo-900/80 text-indigo-200 border-indigo-700' },
    consultor: { label: 'Consultor Forense', badgeClass: 'bg-rose-900/80 text-rose-200 border-rose-700' },
  };

  return (
    <header className="sticky top-0 z-40 bg-slate-900/95 backdrop-blur-md border-b border-slate-800 shadow-xl">
      {/* Top Banner: Role Quick Switcher & Account Lifecycle Bar */}
      <div className="bg-slate-950 border-b border-slate-800/80 px-4 py-2">
        <div className="max-w-7xl mx-auto flex flex-wrap items-center justify-between gap-3 text-xs">
          {/* Quick Role Tester Selector */}
          {!authenticatedUser && <div className="flex items-center gap-2 flex-wrap">
            <span className="text-slate-400 font-medium flex items-center gap-1.5">
              <Users className="w-3.5 h-3.5 text-amber-400" />
              <span>Simular Rol / Usuario:</span>
            </span>
            <div className="flex flex-wrap items-center gap-1.5">
              {allUsers.map((user) => {
                const isSelected = user.id === currentUser.id;
                return (
                  <button
                    key={user.id}
                    onClick={() => switchUserById(user.id)}
                    className={`px-2.5 py-1 rounded-md transition-all font-medium border flex items-center gap-1.5 ${
                      isSelected
                        ? 'bg-amber-500 text-slate-950 border-amber-400 shadow-sm shadow-amber-500/20 font-semibold'
                        : 'bg-slate-900 text-slate-300 border-slate-700/80 hover:bg-slate-800 hover:text-white'
                    }`}
                  >
                    <span>{user.nombres.split(' ')[0]} ({user.role})</span>
                    {user.id === 'usr-student-new' && (
                      <span className="px-1 py-0.2 text-[9px] bg-red-950 text-red-300 rounded border border-red-700">30d</span>
                    )}
                  </button>
                );
              })}
            </div>
          </div>

          }
          {/* Account Validity Status for Current User */}
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-1.5 px-2.5 py-1 rounded-md bg-slate-900 border border-slate-800">
              <Clock className="w-3.5 h-3.5 text-amber-400" />
              <span className="text-slate-400">Vigencia cuenta:</span>
              <span className={`font-semibold ${diffDays <= 7 ? 'text-rose-400' : 'text-emerald-400'}`}>
                {currentUser.hasApprovedPurchase 
                  ? `${diffDays} días restantes (Plan 2 años)` 
                  : `${diffDays > 0 ? diffDays : 0} días restantes (Regla 30 días s/ compra)`}
              </span>
            </div>
            
            <button
              onClick={() => setIsZipModalOpen(true)}
              className="flex items-center gap-1.5 px-3 py-1 rounded-md font-bold bg-gradient-to-r from-emerald-500 to-teal-500 hover:from-emerald-400 hover:to-teal-400 text-slate-950 shadow-md shadow-emerald-500/25 text-xs transition-all border border-emerald-400"
              title="Descargar solución completa .NET 9 para Visual Studio 2022"
            >
              <Download className="w-3.5 h-3.5 text-slate-950 stroke-[2.5]" />
              <span>Descargar .NET 9 ZIP</span>
            </button>

            <button
              onClick={() => setActiveTab('dotnet-architecture')}
              className={`flex items-center gap-1.5 px-3 py-1 rounded-md font-semibold border transition-all ${
                activeTab === 'dotnet-architecture'
                  ? 'bg-amber-500 text-slate-950 border-amber-400 shadow-sm'
                  : 'bg-amber-500/10 text-amber-300 border-amber-500/30 hover:bg-amber-500/20'
              }`}
            >
              <Code2 className="w-3.5 h-3.5 text-amber-400" />
              <span>Solución .NET 9 (C#)</span>
            </button>

            <button
              onClick={() => setActiveTab('manual-guia')}
              className={`flex items-center gap-1.5 px-3 py-1 rounded-md font-semibold border transition-all ${
                activeTab === 'manual-guia'
                  ? 'bg-emerald-500 text-slate-950 border-emerald-400 shadow-sm'
                  : 'bg-emerald-500/10 text-emerald-300 border-emerald-500/30 hover:bg-emerald-500/20'
              }`}
            >
              <BookOpen className="w-3.5 h-3.5 text-emerald-400" />
              <span>Manual Arquitectura</span>
            </button>

            <button
              onClick={() => setActiveTab('guia-cliente')}
              className={`flex items-center gap-1 px-3 py-1 rounded-md font-medium border transition-colors ${
                activeTab === 'guia-cliente'
                  ? 'bg-indigo-600 text-white border-indigo-500'
                  : 'bg-indigo-950/60 text-indigo-300 border-indigo-700/60 hover:bg-indigo-900/60'
              }`}
            >
              <Sparkles className="w-3.5 h-3.5 text-amber-300" />
              <span>Auditoría Requerimiento (Flujo v5)</span>
            </button>
          </div>
        </div>
      </div>

      {/* Main Header Bar */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3.5 flex items-center justify-between gap-4">
        {/* Brand */}
        <div 
          onClick={() => { setActiveTab('catalogo'); setSelectedCourseId(null); }}
          className="flex items-center gap-3 cursor-pointer group"
        >
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-amber-500 to-amber-300 flex items-center justify-center shadow-lg shadow-amber-500/20 group-hover:scale-105 transition-transform">
            <ShieldCheck className="w-6 h-6 text-slate-950" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <span className="font-bold text-lg text-white tracking-tight group-hover:text-amber-400 transition-colors">
                FORENSE EDU
              </span>
              <span className="text-[10px] uppercase font-bold tracking-wider px-1.5 py-0.5 rounded bg-amber-500/10 text-amber-400 border border-amber-500/30">
                Oficial
              </span>
            </div>
            <p className="text-[11px] text-slate-400 font-medium">Plataforma Web Educativa, Eventos & Certificación QR</p>
          </div>
        </div>

        {/* Navigation Tabs */}
        <nav className="hidden xl:flex items-center gap-1 text-sm font-medium">
          <button
            onClick={() => { setActiveTab('catalogo'); setSelectedCourseId(null); }}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 ${
              activeTab === 'catalogo'
                ? 'bg-amber-500/10 text-amber-400 font-semibold border border-amber-500/20'
                : 'text-slate-300 hover:text-white hover:bg-slate-800/60'
            }`}
          >
            <BookOpen className="w-4 h-4" />
            <span>Catálogo</span>
          </button>

          <button
            onClick={() => setActiveTab('mis-cursos')}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 relative ${
              activeTab === 'mis-cursos'
                ? 'bg-amber-500/10 text-amber-400 font-semibold border border-amber-500/20'
                : 'text-slate-300 hover:text-white hover:bg-slate-800/60'
            }`}
          >
            <Video className="w-4 h-4 text-amber-400" />
            <span>Mis Cursos y Evaluaciones</span>
          </button>

          <button
            onClick={() => setActiveTab('mis-eventos')}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 ${
              activeTab === 'mis-eventos'
                ? 'bg-amber-500/10 text-amber-400 font-semibold border border-amber-500/20'
                : 'text-slate-300 hover:text-white hover:bg-slate-800/60'
            }`}
          >
            <Clock className="w-4 h-4" />
            <span>Eventos en Vivo</span>
          </button>

          <button
            onClick={() => setActiveTab('asesoria')}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 ${
              activeTab === 'asesoria'
                ? 'bg-amber-500/10 text-amber-400 font-semibold border border-amber-500/20'
                : 'text-slate-300 hover:text-white hover:bg-slate-800/60'
            }`}
          >
            <MessageSquare className="w-4 h-4 text-emerald-400" />
            <span>Asesoría Pericial</span>
          </button>

          <button
            onClick={() => setActiveTab('mis-certificados')}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 ${
              activeTab === 'mis-certificados'
                ? 'bg-amber-500/10 text-amber-400 font-semibold border border-amber-500/20'
                : 'text-slate-300 hover:text-white hover:bg-slate-800/60'
            }`}
          >
            <Award className="w-4 h-4 text-amber-400" />
            <span>Certificados QR</span>
            {certificates.filter(c => c.userId === currentUser.id).length > 0 && (
              <span className="w-2 h-2 rounded-full bg-amber-400"></span>
            )}
          </button>

          <button
            onClick={() => setActiveTab('mis-pagos')}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 ${
              activeTab === 'mis-pagos'
                ? 'bg-amber-500/10 text-amber-400 font-semibold border border-amber-500/20'
                : 'text-slate-300 hover:text-white hover:bg-slate-800/60'
            }`}
          >
            <CreditCard className="w-4 h-4" />
            <span>Mis Pagos</span>
          </button>

          <button
            onClick={() => setActiveTab('verificador-publico')}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 ${
              activeTab === 'verificador-publico'
                ? 'bg-amber-500/10 text-amber-400 font-semibold border border-amber-500/20'
                : 'text-slate-300 hover:text-white hover:bg-slate-800/60'
            }`}
          >
            <ExternalLink className="w-4 h-4 text-cyan-400" />
            <span>Validador QR</span>
          </button>

          <button
            onClick={() => setActiveTab('dotnet-architecture')}
            className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 ${
              activeTab === 'dotnet-architecture'
                ? 'bg-amber-500 text-slate-950 font-bold shadow-sm'
                : 'text-amber-300 hover:text-white hover:bg-amber-500/10 border border-amber-500/30'
            }`}
          >
            <Code2 className="w-4 h-4 text-amber-400" />
            <span>Backend .NET 9</span>
          </button>

          {/* Admin panel tab for privileged roles */}
          {currentUser.role !== 'estudiante' && (
            <button
              onClick={() => setActiveTab('admin')}
              className={`px-3 py-2 rounded-lg transition-colors flex items-center gap-2 relative ${
                activeTab === 'admin'
                  ? 'bg-purple-600/20 text-purple-300 font-semibold border border-purple-500/40'
                  : 'text-purple-300 hover:bg-purple-950/40'
              }`}
            >
              <Settings className="w-4 h-4 text-purple-400" />
              <span>Panel Admin</span>
              {(pendingPaymentsCount > 0 || duplicateAlertsCount > 0) && (
                <span className="px-1.5 py-0.2 rounded-full text-[10px] font-bold bg-rose-600 text-white animate-pulse">
                  {pendingPaymentsCount + duplicateAlertsCount}
                </span>
              )}
            </button>
          )}
        </nav>

        {/* User Badge & Profile Button */}
        <div className="flex items-center gap-3">
          <button
            onClick={() => setIsZipModalOpen(true)}
            className="hidden sm:flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-emerald-500/15 border border-emerald-500/40 text-emerald-300 hover:bg-emerald-500/25 transition-all text-xs font-bold"
          >
            <Download className="w-3.5 h-3.5 text-emerald-400" />
            <span>Descargar ZIP .NET 9</span>
          </button>
          <button
            onClick={() => setActiveTab('perfil')}
            className={`flex items-center gap-2.5 px-3 py-1.5 rounded-lg border transition-all text-left ${
              activeTab === 'perfil'
                ? 'bg-slate-800 border-amber-400/50 shadow-sm'
                : 'bg-slate-900 border-slate-700/80 hover:bg-slate-800'
            }`}
          >
            <div className="w-8 h-8 rounded-full bg-amber-500/20 border border-amber-500/40 flex items-center justify-center text-amber-300 font-bold text-xs">
              {currentUser.nombres.charAt(0)}{currentUser.apellidos.charAt(0)}
            </div>
            <div className="hidden sm:block">
              <div className="text-xs font-semibold text-white truncate max-w-[140px]">
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
      <div className="xl:hidden flex items-center overflow-x-auto gap-1 px-4 py-2 bg-slate-950/80 border-t border-slate-800/60 text-xs">
        <button
          onClick={() => { setActiveTab('catalogo'); setSelectedCourseId(null); }}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'catalogo' ? 'bg-amber-500 text-slate-950 font-bold' : 'text-slate-300'}`}
        >
          Catálogo
        </button>
        <button
          onClick={() => setActiveTab('mis-cursos')}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'mis-cursos' ? 'bg-amber-500 text-slate-950 font-bold' : 'text-slate-300'}`}
        >
          Cursos y Evaluaciones
        </button>
        <button
          onClick={() => setActiveTab('mis-eventos')}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'mis-eventos' ? 'bg-amber-500 text-slate-950 font-bold' : 'text-slate-300'}`}
        >
          Eventos en Vivo
        </button>
        <button
          onClick={() => setActiveTab('asesoria')}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'asesoria' ? 'bg-amber-500 text-slate-950 font-bold' : 'text-slate-300'}`}
        >
          Asesoría
        </button>
        <button
          onClick={() => setActiveTab('mis-certificados')}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'mis-certificados' ? 'bg-amber-500 text-slate-950 font-bold' : 'text-slate-300'}`}
        >
          Certificados QR
        </button>
        <button
          onClick={() => setActiveTab('mis-pagos')}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'mis-pagos' ? 'bg-amber-500 text-slate-950 font-bold' : 'text-slate-300'}`}
        >
          Mis Pagos
        </button>
        <button
          onClick={() => setActiveTab('verificador-publico')}
          className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'verificador-publico' ? 'bg-amber-500 text-slate-950 font-bold' : 'text-slate-300'}`}
        >
          Validador QR
        </button>
        {currentUser.role !== 'estudiante' && (
          <button
            onClick={() => setActiveTab('admin')}
            className={`px-2.5 py-1.5 rounded-md whitespace-nowrap ${activeTab === 'admin' ? 'bg-purple-600 text-white font-bold' : 'text-purple-300'}`}
          >
            Admin ({pendingPaymentsCount + duplicateAlertsCount})
          </button>
        )}
      </div>

      {/* Modal with guaranteed 1-click download, copy link and new tab fallbacks */}
      <DownloadZipModal isOpen={isZipModalOpen} onClose={() => setIsZipModalOpen(false)} />
    </header>
  );
};
