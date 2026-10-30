import React, { useState } from 'react';
import { useApp } from '../context/AppContext';
import { 
  MessageCircle, 
  X, 
  Send, 
  HelpCircle, 
  PhoneCall, 
  Mail, 
  CheckCircle2, 
  AlertCircle
} from 'lucide-react';
import { SupportTicket } from '../types';

export const FloatingSupport: React.FC = () => {
  const { socialLinks, createTicket, setActiveTab } = useApp();
  const [isOpen, setIsOpen] = useState(false);
  const [ticketSubject, setTicketSubject] = useState('');
  const [ticketCategory, setTicketCategory] = useState<SupportTicket['category']>('General');
  const [ticketMessage, setTicketMessage] = useState('');
  const [ticketSubmitted, setTicketSubmitted] = useState<string | null>(null);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!ticketSubject.trim() || !ticketMessage.trim()) return;

    const created = createTicket(ticketSubject, ticketCategory, ticketMessage);
    setTicketSubmitted(created.ticketCode);
    setTicketSubject('');
    setTicketMessage('');
  };

  return (
    <>
      {/* Floating Button Stack */}
      <div className="fixed bottom-6 right-6 z-50 flex flex-col items-end gap-3">
        {/* WhatsApp Direct Button */}
        {socialLinks.whatsapp.enabled && (
          <a
            href={`https://wa.me/${socialLinks.whatsapp.number.replace(/\D/g, '')}?text=Hola,%20solicito%20asistencia%20en%20la%20Plataforma%20Educativa%20Forense.`}
            target="_blank"
            rel="noopener noreferrer"
            className="w-13 h-13 rounded-full bg-emerald-600 hover:bg-emerald-500 text-white flex items-center justify-center shadow-xl shadow-emerald-950/40 transition-all hover:scale-110 group relative"
            title="Chat directo por WhatsApp"
          >
            <MessageCircle className="w-7 h-7" />
            <span className="absolute right-full mr-3 top-1/2 -translate-y-1/2 bg-slate-900 text-white text-xs px-2.5 py-1 rounded-md shadow-md whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none border border-slate-800">
              WhatsApp Directo ({socialLinks.whatsapp.number})
            </span>
          </a>
        )}

        {/* Support Widget Trigger Button */}
        <button
          onClick={() => { setIsOpen(!isOpen); setTicketSubmitted(null); }}
          className={`w-13 h-13 rounded-full flex items-center justify-center shadow-xl transition-all hover:scale-105 relative ${
            isOpen 
              ? 'bg-slate-800 text-slate-200 border border-slate-700' 
              : 'bg-amber-500 text-slate-950 hover:bg-amber-400 shadow-amber-950/30'
          }`}
          title="Centro de Ayuda y Tickets"
        >
          {isOpen ? <X className="w-6 h-6" /> : <HelpCircle className="w-7 h-7" />}
        </button>
      </div>

      {/* Support Dialog Modal */}
      {isOpen && (
        <div className="fixed bottom-22 right-6 z-50 w-96 max-w-[calc(100vw-2rem)] bg-slate-900 border border-slate-700/80 rounded-2xl shadow-2xl shadow-slate-950/80 overflow-hidden flex flex-col animate-in fade-in slide-in-from-bottom-4 duration-200">
          {/* Header */}
          <div className="bg-gradient-to-r from-slate-950 to-slate-900 p-4 border-b border-slate-800 flex items-center justify-between">
            <div className="flex items-center gap-2.5">
              <div className="w-8 h-8 rounded-lg bg-amber-500/20 border border-amber-500/40 flex items-center justify-center text-amber-400">
                <HelpCircle className="w-4 h-4" />
              </div>
              <div>
                <h3 className="font-bold text-sm text-white">Centro de Ayuda & Soporte</h3>
                <p className="text-[11px] text-slate-400">Sección 15.2: Canales oficiales</p>
              </div>
            </div>
            <button
              onClick={() => setIsOpen(false)}
              className="p-1 rounded-md text-slate-400 hover:text-white hover:bg-slate-800 transition-colors"
            >
              <X className="w-4 h-4" />
            </button>
          </div>

          {/* Body */}
          <div className="p-4 space-y-4 max-h-[70vh] overflow-y-auto">
            {/* Quick Contact Links */}
            <div className="grid grid-cols-2 gap-2 text-xs">
              <a
                href={`https://wa.me/${socialLinks.whatsapp.number.replace(/\D/g, '')}`}
                target="_blank"
                rel="noopener noreferrer"
                className="p-2.5 rounded-lg bg-emerald-950/40 border border-emerald-800/60 text-emerald-300 hover:bg-emerald-900/40 flex items-center gap-2 transition-colors"
              >
                <PhoneCall className="w-3.5 h-3.5 text-emerald-400" />
                <span className="font-medium">WhatsApp Pericial</span>
              </a>
              <a
                href="mailto:soporte@forense.edu.pe"
                className="p-2.5 rounded-lg bg-slate-800 border border-slate-700 text-slate-200 hover:bg-slate-750 flex items-center gap-2 transition-colors"
              >
                <Mail className="w-3.5 h-3.5 text-amber-400" />
                <span className="font-medium">Correo Soporte</span>
              </a>
            </div>

            {ticketSubmitted ? (
              <div className="p-4 rounded-xl bg-emerald-950/50 border border-emerald-700/80 text-center space-y-2">
                <CheckCircle2 className="w-8 h-8 text-emerald-400 mx-auto" />
                <h4 className="font-bold text-sm text-white">Ticket Registrado Exitosamente</h4>
                <p className="text-xs text-slate-300">
                  Código de seguimiento: <span className="font-mono font-bold text-amber-300">{ticketSubmitted}</span>
                </p>
                <p className="text-[11px] text-slate-400">
                  El equipo de soporte responderá a la brevedad. Puedes consultar el historial en tu panel.
                </p>
                <div className="pt-2 flex gap-2">
                  <button
                    onClick={() => {
                      setIsOpen(false);
                      setActiveTab('mis-consultas');
                    }}
                    className="flex-1 py-1.5 px-3 rounded-lg bg-amber-500 text-slate-950 font-bold text-xs hover:bg-amber-400 transition-colors"
                  >
                    Ver Mis Consultas
                  </button>
                  <button
                    onClick={() => setTicketSubmitted(null)}
                    className="py-1.5 px-3 rounded-lg bg-slate-800 text-slate-300 text-xs hover:bg-slate-700"
                  >
                    Nuevo
                  </button>
                </div>
              </div>
            ) : (
              <form onSubmit={handleSubmit} className="space-y-3">
                <div className="text-xs font-semibold text-slate-300 flex items-center justify-between">
                  <span>Crear Ticket de Consulta Interna</span>
                  <span className="text-[10px] text-amber-400/90 font-normal">Trazabilidad en tiempo real</span>
                </div>

                <div>
                  <label className="block text-[11px] text-slate-400 mb-1">Categoría</label>
                  <select
                    value={ticketCategory}
                    onChange={(e) => setTicketCategory(e.target.value as any)}
                    className="w-full bg-slate-950 border border-slate-700 rounded-lg px-2.5 py-1.5 text-xs text-white focus:outline-none focus:border-amber-400"
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
                  <label className="block text-[11px] text-slate-400 mb-1">Asunto</label>
                  <input
                    type="text"
                    required
                    placeholder="Ej. Validación de pago para Balística"
                    value={ticketSubject}
                    onChange={(e) => setTicketSubject(e.target.value)}
                    className="w-full bg-slate-950 border border-slate-700 rounded-lg px-2.5 py-1.5 text-xs text-white focus:outline-none focus:border-amber-400"
                  />
                </div>

                <div>
                  <label className="block text-[11px] text-slate-400 mb-1">Detalle de la consulta</label>
                  <textarea
                    rows={3}
                    required
                    placeholder="Describe tu consulta con detalle..."
                    value={ticketMessage}
                    onChange={(e) => setTicketMessage(e.target.value)}
                    className="w-full bg-slate-950 border border-slate-700 rounded-lg px-2.5 py-1.5 text-xs text-white focus:outline-none focus:border-amber-400 resize-none"
                  />
                </div>

                <button
                  type="submit"
                  className="w-full py-2 px-3 rounded-lg bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs flex items-center justify-center gap-1.5 transition-colors shadow-md shadow-amber-500/20"
                >
                  <Send className="w-3.5 h-3.5" />
                  <span>Enviar Ticket a Soporte</span>
                </button>
              </form>
            )}
          </div>
        </div>
      )}
    </>
  );
};
