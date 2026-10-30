@echo off
setlocal
cd /d "%~dp0src\ForensicEdu.ClientApp"
if not exist "node_modules\vite\bin\vite.js" (
  echo Instalando dependencias de la interfaz...
  call npm install
  if errorlevel 1 (
    pause
    exit /b 1
  )
)
set "BROWSER=chrome"
echo Abriendo la interfaz en http://localhost:3000/
echo Mantenga esta ventana abierta mientras utiliza la aplicacion.
call npm run dev -- --open
if errorlevel 1 pause
