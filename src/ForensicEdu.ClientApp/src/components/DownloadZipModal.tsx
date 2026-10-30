import React, { useState } from 'react';
import { 
  Download, 
  Copy, 
  ExternalLink, 
  CheckCircle2, 
  X, 
  FileArchive, 
  FolderCheck, 
  Terminal,
  AlertCircle,
  HelpCircle,
  Layers,
  Sparkles
} from 'lucide-react';

interface DownloadZipModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const DownloadZipModal: React.FC<DownloadZipModalProps> = ({ isOpen, onClose }) => {
  const [downloading, setDownloading] = useState(false);
  const [downloadSuccess, setDownloadSuccess] = useState(false);
  const [copied, setCopied] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  if (!isOpen) return null;

  // The full URL to the zip archive
  const zipPath = '/ForensicEdu_DotNet9_VS2022.zip';
  const fullZipUrl = `${window.location.origin}${zipPath}`;

  const handleBlobDownload = async () => {
    setDownloading(true);
    setErrorMsg(null);
    try {
      const response = await fetch(zipPath);
      if (!response.ok) {
        throw new Error(`Error HTTP: ${response.status} ${response.statusText}`);
      }
      const blob = await response.blob();
      const blobUrl = window.URL.createObjectURL(blob);
      
      const a = document.createElement('a');
      a.style.display = 'none';
      a.href = blobUrl;
      a.download = 'ForensicEdu_DotNet9_VS2022.zip';
      document.body.appendChild(a);
      a.click();
      
      setTimeout(() => {
        document.body.removeChild(a);
        window.URL.revokeObjectURL(blobUrl);
      }, 2000);

      setDownloading(false);
      setDownloadSuccess(true);
      setTimeout(() => setDownloadSuccess(false), 4000);
    } catch (err: any) {
      console.error('Fetch download failed, attempting fallback direct link:', err);
      try {
        const a = document.createElement('a');
        a.href = fullZipUrl;
        a.download = 'ForensicEdu_DotNet9_VS2022.zip';
        a.target = '_blank';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        setDownloading(false);
        setDownloadSuccess(true);
      } catch (fallbackErr: any) {
        setDownloading(false);
        setErrorMsg('El navegador restringió la descarga en este marco. Usa la opción "Copiar enlace directo" y pégalo en una pestaña nueva.');
      }
    }
  };

  const handleCopyLink = () => {
    navigator.clipboard.writeText(fullZipUrl);
    setCopied(true);
    setTimeout(() => setCopied(false), 2500);
  };

  const handleOpenInNewTab = () => {
    window.open(fullZipUrl, '_blank');
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm animate-fadeIn">
      <div className="bg-slate-900 border border-slate-700/80 rounded-2xl max-w-lg w-full p-6 shadow-2xl relative overflow-hidden space-y-5">
        {/* Decorative background glow */}
        <div className="absolute top-0 right-0 w-64 h-64 bg-emerald-500/10 rounded-full blur-3xl pointer-events-none" />

        {/* Header */}
        <div className="flex items-start justify-between gap-4 relative z-10">
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 rounded-xl bg-gradient-to-tr from-emerald-600 to-teal-500 flex items-center justify-center shadow-lg shadow-emerald-500/20 text-slate-950">
              <FileArchive className="w-7 h-7" />
            </div>
            <div>
              <div className="flex items-center gap-1.5">
                <h2 className="text-lg font-extrabold text-white">Solución Fullstack .NET 9 + React</h2>
                <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-500/20 text-emerald-300 border border-emerald-500/30">VS 2022</span>
              </div>
              <p className="text-xs text-slate-400">ForensicEdu_DotNet9_VS2022.zip (~4.8 MB)</p>
            </div>
          </div>
          <button 
            onClick={onClose}
            className="text-slate-400 hover:text-white p-1 rounded-lg hover:bg-slate-800 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content & Status */}
        <div className="space-y-4 text-xs relative z-10">
          <div className="p-3.5 rounded-xl bg-slate-950 border border-slate-800 space-y-2">
            <div className="flex items-center gap-2 text-emerald-400 font-bold">
              <FolderCheck className="w-4 h-4" />
              <span>5 Proyectos incluidos y listos para ejecutar con F5:</span>
            </div>
            <ul className="text-slate-300 space-y-1.5 pl-6 list-disc text-[11px]">
              <li><strong>ForensicEdu.WebApi</strong>: Controladores REST, OpenAPI/Scalar, Hangfire y CORS configurado.</li>
              <li><strong>ForensicEdu.ClientApp</strong>: Frontend interactivo en React + TypeScript (.esproj) con proxy.</li>
              <li><strong>ForensicEdu.Domain</strong>: Entidades y Enums con mayúsculas y PascalCase sincronizados.</li>
              <li><strong>ForensicEdu.Application</strong>: Casos de uso, DTOs y validación antifraude.</li>
              <li><strong>ForensicEdu.Infrastructure</strong>: EF Core, BCrypt, generación QR y PDF.</li>
            </ul>
          </div>

          {downloadSuccess && (
            <div className="p-3 rounded-lg bg-emerald-500/15 border border-emerald-500/40 text-emerald-300 flex items-center gap-2.5">
              <CheckCircle2 className="w-5 h-5 text-emerald-400 shrink-0" />
              <span>¡Descarga iniciada! Revisa tu carpeta de Descargas en Windows.</span>
            </div>
          )}

          {errorMsg && (
            <div className="p-3 rounded-lg bg-rose-500/15 border border-rose-500/40 text-rose-300 flex items-start gap-2.5">
              <AlertCircle className="w-5 h-5 text-rose-400 shrink-0 mt-0.5" />
              <span>{errorMsg}</span>
            </div>
          )}

          {/* Action buttons */}
          <div className="space-y-2.5 pt-2">
            {/* Primary Download Button */}
            <button
              onClick={handleBlobDownload}
              disabled={downloading}
              className="w-full py-3 px-4 rounded-xl bg-gradient-to-r from-emerald-500 to-teal-500 hover:from-emerald-400 hover:to-teal-400 text-slate-950 font-black text-sm flex items-center justify-center gap-2.5 shadow-lg shadow-emerald-500/25 transition-all disabled:opacity-50 cursor-pointer"
            >
              <Download className="w-4 h-4 text-slate-950" />
              <span>{downloading ? 'Generando descarga...' : 'Descargar ZIP Completo (.NET 9 + React)'}</span>
            </button>

            <div className="grid grid-cols-2 gap-2">
              {/* Copy URL option */}
              <button
                onClick={handleCopyLink}
                className="py-2.5 px-3 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 font-semibold text-xs flex items-center justify-center gap-2 border border-slate-700 transition-colors cursor-pointer"
              >
                {copied ? <CheckCircle2 className="w-4 h-4 text-emerald-400" /> : <Copy className="w-4 h-4 text-amber-400" />}
                <span>{copied ? '¡Enlace Copiado!' : 'Copiar Enlace'}</span>
              </button>

              {/* Open in new tab option */}
              <button
                onClick={handleOpenInNewTab}
                className="py-2.5 px-3 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 font-semibold text-xs flex items-center justify-center gap-2 border border-slate-700 transition-colors cursor-pointer"
              >
                <ExternalLink className="w-4 h-4 text-indigo-400" />
                <span>Abrir en Pestaña</span>
              </button>
            </div>
          </div>

          <div className="pt-2 border-t border-slate-800/80 text-[11px] text-slate-400 flex items-center gap-1.5 justify-center">
            <Sparkles className="w-3.5 h-3.5 text-amber-400" />
            <span>Descomprime, abre <strong>ForensicEdu.sln</strong> en VS 2022 y pulsa <strong>F5</strong>.</span>
          </div>
        </div>
      </div>
    </div>
  );
};
