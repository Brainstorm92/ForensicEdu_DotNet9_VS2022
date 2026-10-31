# ForensicEdu - Solución Completa Fullstack (.NET 9 + React) en Visual Studio 2022

Plataforma Educativa Forense desarrollada bajo estándares corporativos:
- **Backend:** .NET 9 C# 13 con Arquitectura Limpia (Domain, Application, Infrastructure, WebApi) + OpenAPI / Scalar.
- **Frontend:** React 18, TypeScript, Tailwind CSS, Vite + Lucide Icons (`ForensicEdu.ClientApp.esproj`).

---

## Estructura de la Solución en Visual Studio 2022

```text
ForensicEdu.sln
│
├── 📂 src/
│   ├── 📁 ForensicEdu.Domain/          -> Entidades, Enums de estados forenses y reglas.
│   ├── 📁 ForensicEdu.Application/     -> Casos de uso, interfaces, DTOs y lógica antifraude.
│   ├── 📁 ForensicEdu.Infrastructure/  -> EF Core (In-Memory / SQL Server / PostgreSQL), JWT, QR, PDF.
│   ├── 📁 ForensicEdu.WebApi/          -> Controladores REST API, Hangfire y Scalar docs (https://localhost:7148).
│   └── 📁 ForensicEdu.ClientApp/       -> Frontend interactivo React + TypeScript (.esproj) (http://localhost:3000).
```

---

## Pasos para Ejecutar con F5 (Backend + Frontend Simultáneos)

### 1. Abrir la Solución
Haz doble clic sobre `ForensicEdu.sln` para abrirlo en **Visual Studio 2022**.

### 2. Instalar dependencias del Frontend (Solo la primera vez)
Abre una terminal en la carpeta `src/ForensicEdu.ClientApp/` y ejecuta:
```bash
npm install
```
*(Visual Studio 2022 también puede restaurar los paquetes npm automáticamente si tienes instalada la carga de trabajo "Desarrollo web y de Node.js")*.

### 3. Configurar Inicio Múltiple (Multiple Startup Projects)
1. En el **Explorador de Soluciones**, haz clic derecho sobre la solución `Solución 'ForensicEdu' (5 proyectos)`.
2. Selecciona **Propiedades** (*Properties*).
3. En la barra izquierda, haz clic en **Proyecto de inicio común** ➔ **Varios proyectos de inicio** (*Multiple startup projects*).
4. Configura:
   - **`ForensicEdu.WebApi`** ➔ Acción: **Iniciar** (*Start*).
   - **`ForensicEdu.ClientApp`** ➔ Acción: **Ninguno** (*None*). WebApi compila y sirve la interfaz.
5. Haz clic en **Aceptar**.

### 4. Presionar F5
Al presionar **F5**:
- Con el perfil `http`, la documentación de la API se abre en `http://localhost:5000/scalar/v1`; la interfaz está en `http://localhost:5000/app/`. La documentación de la API está en `/scalar/v1`.
- WebApi ejecuta `npm run build` y sirve la interfaz desde `wwwroot`; no es necesario mantener Vite ni el depurador JavaScript activos. Recompila para ver cambios en React.
- Para trabajar con Hot Reload de React, puedes ejecutar `npm run dev` en ClientApp y abrir `http://localhost:3000`, manteniendo WebApi activo en el puerto 5000.
