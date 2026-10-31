import React from 'react';
import { useApp } from '../context/AppContext';
import {
  ShieldCheck,
  Mail,
  Phone,
  MapPin,
  ExternalLink,
  MessageCircle
} from 'lucide-react';

export const Footer: React.FC = () => {
  const { socialLinks, setActiveTab } = useApp();

  return (
    <footer className="bg-canvas border-t border-line text-muted text-sm">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 mb-10">
          {/* Column 1: Brand & Overview */}
          <div className="space-y-4">
            <div className="flex items-center gap-3">
              <div className="w-9 h-9 rounded-xl bg-brand flex items-center justify-center text-ink font-bold">
                <ShieldCheck className="w-5 h-5" />
              </div>
              <span className="font-bold text-lg text-ink">ACADEMIA FORENSE</span>
            </div>
            <p className="text-xs text-muted leading-relaxed">
              Plataforma web educativa especializada en Ciencias Forenses, Criminalística, Peritaje Judicial, eventos internacionales en vivo y emisión de certificados digitales con validación QR pública.
            </p>
            <div className="pt-2 text-xs text-muted">
              Formación especializada y certificados con verificación digital.
            </div>
          </div>

          {/* Column 2: Forensic Specializations */}
          <div>
            <h4 className="text-ink font-semibold text-xs uppercase tracking-wider mb-4 text-brand">
              Cursos Especializados
            </h4>
            <ul className="space-y-2 text-xs">
              <li className="hover:text-ink transition-colors cursor-pointer" onClick={() => setActiveTab('catalogo')}>Balística Forense</li>
              <li className="hover:text-ink transition-colors cursor-pointer" onClick={() => setActiveTab('catalogo')}>Explosivos Forenses</li>
              <li className="hover:text-ink transition-colors cursor-pointer" onClick={() => setActiveTab('catalogo')}>Escena del Crimen</li>
              <li className="hover:text-ink transition-colors cursor-pointer" onClick={() => setActiveTab('catalogo')}>Biología Forense</li>
              <li className="hover:text-ink transition-colors cursor-pointer" onClick={() => setActiveTab('catalogo')}>Accidentología Vial</li>
              <li className="hover:text-ink transition-colors cursor-pointer" onClick={() => setActiveTab('catalogo')}>Identificación Vehicular</li>
              <li className="hover:text-ink transition-colors cursor-pointer" onClick={() => setActiveTab('catalogo')}>Informática Forense</li>
              <li className="hover:text-ink transition-colors cursor-pointer" onClick={() => setActiveTab('catalogo')}>Documentología</li>
            </ul>
          </div>

          {/* Column 3: Redes Sociales Administrables (Sección 15.1) */}
          <div>
            <h4 className="text-ink font-semibold text-xs uppercase tracking-wider mb-4 text-brand">
              Redes Sociales Oficiales
            </h4>
            <p className="text-xs text-muted mb-3">
              Síguenos y conoce nuestras novedades:
            </p>
            <div className="flex flex-col gap-2 text-xs">
              {socialLinks.whatsapp.enabled && (
                <a
                  href={`https://wa.me/${socialLinks.whatsapp.number.replace(/\D/g, '')}`}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-2 text-emerald-700 hover:text-emerald-700 transition-colors"
                >
                  <MessageCircle className="w-3.5 h-3.5" />
                  <span>WhatsApp ({socialLinks.whatsapp.number})</span>
                  <ExternalLink className="w-3 h-3 text-muted" />
                </a>
              )}
              {socialLinks.facebook.enabled && (
                <a
                  href={socialLinks.facebook.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-2 hover:text-ink transition-colors"
                >
                  <span>Facebook Oficial</span>
                  <ExternalLink className="w-3 h-3 text-muted" />
                </a>
              )}
              {socialLinks.instagram.enabled && (
                <a
                  href={socialLinks.instagram.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-2 hover:text-ink transition-colors"
                >
                  <span>Instagram Forense</span>
                  <ExternalLink className="w-3 h-3 text-muted" />
                </a>
              )}
              {socialLinks.youtube.enabled && (
                <a
                  href={socialLinks.youtube.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-2 hover:text-ink transition-colors"
                >
                  <span>YouTube Masterclasses</span>
                  <ExternalLink className="w-3 h-3 text-muted" />
                </a>
              )}
              {socialLinks.tiktok.enabled && (
                <a
                  href={socialLinks.tiktok.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-2 hover:text-ink transition-colors"
                >
                  <span>TikTok Educativo</span>
                  <ExternalLink className="w-3 h-3 text-muted" />
                </a>
              )}
              {socialLinks.linkedin.enabled && (
                <a
                  href={socialLinks.linkedin.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-2 hover:text-ink transition-colors"
                >
                  <span>LinkedIn Institucional</span>
                  <ExternalLink className="w-3 h-3 text-muted" />
                </a>
              )}
              {socialLinks.x.enabled && (
                <a
                  href={socialLinks.x.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-2 hover:text-ink transition-colors"
                >
                  <span>X (Twitter)</span>
                  <ExternalLink className="w-3 h-3 text-muted" />
                </a>
              )}
            </div>
          </div>

          {/* Column 4: Contact & Support */}
          <div>
            <h4 className="text-ink font-semibold text-xs uppercase tracking-wider mb-4 text-brand">
              Soporte y Atención
            </h4>
            <div className="space-y-3 text-xs">
              <div className="flex items-start gap-2.5">
                <MapPin className="w-4 h-4 text-muted shrink-0 mt-0.5" />
                <span>Av. Javier Prado Este 2875, San Borja, Lima, Perú</span>
              </div>
              <div className="flex items-center gap-2.5">
                <Mail className="w-4 h-4 text-muted shrink-0" />
                <span>contacto@forense.edu.pe</span>
              </div>
              <div className="flex items-center gap-2.5">
                <Phone className="w-4 h-4 text-muted shrink-0" />
                <span>+51 (1) 714-9000 / Central Pericial</span>
              </div>
              <div className="pt-2">
              </div>
            </div>
          </div>
        </div>

        {/* Bottom bar */}
        <div className="pt-8 border-t border-line flex flex-col sm:flex-row items-center justify-between gap-4 text-xs text-muted">
          <div>
            © {new Date().getFullYear()} ACADEMIA FORENSE. Todos los derechos reservados. Sistema Integral de Procesos de Plataforma Educativa.
          </div>
          <div className="flex items-center gap-4">
            <span className="hover:text-muted cursor-pointer">Términos y Condiciones</span>
            <span>•</span>
            <span className="hover:text-muted cursor-pointer">Política de Privacidad</span>
            <span>•</span>
            <span className="hover:text-muted cursor-pointer" onClick={() => setActiveTab('verificador-publico')}>Verificación de QR</span>
          </div>
        </div>
      </div>
    </footer>
  );
};
