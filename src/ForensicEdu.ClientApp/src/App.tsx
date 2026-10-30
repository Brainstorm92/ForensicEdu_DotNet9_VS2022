import React from 'react';
import { AccountAccess } from './components/AccountAccess';
import { AppProvider, useApp } from './context/AppContext';
import { Header } from './components/Header';
import { Footer } from './components/Footer';
import { FloatingSupport } from './components/FloatingSupport';
import { CatalogSection } from './components/CatalogSection';
import { StudentDashboard } from './components/StudentDashboard';
import { AdminPanel } from './components/AdminPanel';
import { ClientReviewGuide } from './components/ClientReviewGuide';
import { PublicQRVerifier } from './components/PublicQRVerifier';
import { DotnetArchitectureExplorer } from './components/DotnetArchitectureExplorer';
import { ManualGuiaSection } from './components/ManualGuiaSection';

const MainContent: React.FC = () => {
  const { activeTab } = useApp();

  return (
    <main className="min-h-[calc(100vh-140px)] flex flex-col justify-start">
      {activeTab === 'catalogo' && <CatalogSection />}
      {activeTab === 'mis-cursos' && <StudentDashboard currentSubTab="mis-cursos" />}
      {activeTab === 'mis-eventos' && <StudentDashboard currentSubTab="mis-eventos" />}
      {activeTab === 'asesoria' && <StudentDashboard currentSubTab="asesoria" />}
      {activeTab === 'mis-certificados' && <StudentDashboard currentSubTab="mis-certificados" />}
      {activeTab === 'mis-pagos' && <StudentDashboard currentSubTab="mis-pagos" />}
      {activeTab === 'mis-consultas' && <StudentDashboard currentSubTab="mis-consultas" />}
      {activeTab === 'perfil' && <StudentDashboard currentSubTab="perfil" />}
      {activeTab === 'admin' && <AdminPanel />}
      {activeTab === 'guia-cliente' && <ClientReviewGuide />}
      {activeTab === 'verificador-publico' && <PublicQRVerifier />}
      {activeTab === 'dotnet-architecture' && <DotnetArchitectureExplorer />}
      {activeTab === 'manual-guia' && <ManualGuiaSection />}
    </main>
  );
};

export default function App() {
  return (
    <AppProvider>
      <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col font-sans selection:bg-amber-500 selection:text-slate-950">
        <AccountAccess />
        <Header />
        <div className="flex-1">
          <MainContent />
        </div>
        <Footer />
        <FloatingSupport />
      </div>
    </AppProvider>
  );
}
