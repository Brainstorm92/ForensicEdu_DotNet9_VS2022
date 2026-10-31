import { Moon, Sun } from 'lucide-react';
import { useTheme } from '../context/ThemeContext';

export function ThemeToggle() {
  const { theme, toggleTheme } = useTheme();
  const dark = theme === 'dark';
  const label = dark ? 'Cambiar a modo claro' : 'Cambiar a modo nocturno';
  return <button type="button" onClick={toggleTheme} aria-label={label} aria-pressed={dark} title={label}
    data-testid="theme-toggle" className="inline-flex shrink-0 items-center justify-center gap-2 rounded-xl border border-line bg-surface-low px-3 py-2 text-ink hover:bg-surface-high transition-colors">
    {dark ? <Sun size={18} aria-hidden="true" /> : <Moon size={18} aria-hidden="true" />}
    <span className="hidden sm:inline text-xs font-medium">{dark ? 'Modo claro' : 'Modo nocturno'}</span>
  </button>;
}
