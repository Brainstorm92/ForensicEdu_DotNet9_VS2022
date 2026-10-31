import { createContext, useContext, useEffect, useRef, useState, type ReactNode } from 'react';

type Theme = 'light' | 'dark';
const key = 'forensicedu-theme';
function storedTheme(): Theme | null {
  try { const value = localStorage.getItem(key); return value === 'light' || value === 'dark' ? value : null; }
  catch { return null; }
}
function preferredTheme(): Theme {
  return storedTheme() ?? (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
}
const ThemeContext = createContext<{ theme: Theme; toggleTheme: () => void } | null>(null);

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setTheme] = useState<Theme>(preferredTheme);
  const explicit = useRef(storedTheme() !== null);
  useEffect(() => {
    document.documentElement.dataset.theme = theme;
    document.documentElement.classList.toggle('dark', theme === 'dark');
  }, [theme]);
  useEffect(() => {
    const media = window.matchMedia('(prefers-color-scheme: dark)');
    const onSystemChange = (event: MediaQueryListEvent) => { if (!explicit.current) setTheme(event.matches ? 'dark' : 'light'); };
    const onStorage = (event: StorageEvent) => {
      if (event.key === key || event.key === null) { explicit.current = storedTheme() !== null; setTheme(preferredTheme()); }
    };
    media.addEventListener('change', onSystemChange);
    window.addEventListener('storage', onStorage);
    return () => { media.removeEventListener('change', onSystemChange); window.removeEventListener('storage', onStorage); };
  }, []);
  const toggleTheme = () => {
    const next = theme === 'dark' ? 'light' : 'dark';
    explicit.current = true;
    try { localStorage.setItem(key, next); } catch { /* The switch also works when storage is blocked. */ }
    setTheme(next);
  };
  return <ThemeContext.Provider value={{ theme, toggleTheme }}>{children}</ThemeContext.Provider>;
}
export function useTheme() {
  const context = useContext(ThemeContext);
  if (!context) throw new Error('useTheme requiere ThemeProvider');
  return context;
}
