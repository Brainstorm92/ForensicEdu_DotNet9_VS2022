# Manual Técnico de Arquitectura y Guía de Desarrollo

> Documento heredado del prototipo: algunas capacidades descritas no están integradas o son demostraciones. Consulta `GUIA_PROYECTO.md` para la estructura real y `REVISION_REQUISITOS.md` para el estado verificado y los pendientes.
## Plataforma Web Educativa Forense, Eventos y Certificación Digital (.NET 9 + React)
### Basado en la Especificación Funcional Oficial (Francisco v5)

---

## 1. Visión General del Sistema y Filosofía Arquitectónica

La plataforma **ForensicEdu** ha sido concebida como un ecosistema corporativo de alta especialización forense y pericial judicial. Implementa una separación de responsabilidades estricta entre el **Backend (.NET 9 C# 13 bajo Clean Architecture)** y el **Frontend interactivo (React 18 + TypeScript + Vite + Tailwind CSS)**.

### Pilares Fundamentales del Sistema:
1. **Regla de Oro de Compra:** `1 Pago Aprobado = 1 Producto (Curso o Evento o Asesoría)`. Nunca un mismo pago puede reutilizarse para matricular múltiples cursos.
2. **Motor Antifraude Criptográfico:** Cada pago genera una **Huella Bancaria Normalizada** (`Banco|NumOperacion|Fecha|Monto|Titular`) y un **Hash SHA-256** del comprobante (voucher). Si una huella o hash coincide con un pago ya aprobado, el sistema bloquea inmediatamente la operación.
3. **Ciclo de Vida de Cuenta Inteligente:** 
   - Cuenta nueva creada: **30 días de gracia** sin compras. Si no compra en 30 días, pasa a `PENDIENTE_DE_ELIMINACIÓN`.
   - Primer pago aprobado: Pasa a estado `ACTIVA` y adquiere **2 años de vigencia**.
   - Cada nueva compra aprobada: Renueva automáticamente por **otros 2 años** a partir de la fecha de ese último pago.
4. **Evaluación Rigurosa de 10 Minutos:** Examen cronometrado de 600 segundos en servidor, nota mínima aprobatoria de **13.00/20**. Si aprueba en el 1º intento, obtiene certificación directa. Si desaprueba, entra a un periodo de reposo pedagógico de **24 horas** antes de habilitar su 2º y último intento.
5. **Certificación Criptográfica con QR Público:** Certificados con código único institucional (`CF-YYYY-SHORTCODE-XXXXXX`) y código QR dinámico consultable públicamente por fiscalías, peritos y estudiantes sin requerir inicio de sesión.

---

## 2. Arquitectura del Backend (.NET 9 C# 13)

El backend sigue las directrices del **Clean Architecture** (Arquitectura Cebolla / Hexagonal) donde las dependencias fluyen estrictamente de afuera hacia adentro:

```text
[ ForensicEdu.WebApi ]           -> Capa Externa (Controllers REST, OpenAPI Scalar, CORS, Hangfire)
         ↓
[ ForensicEdu.Infrastructure ]   -> Persistencia EF Core, BCrypt, QuestPDF, QRCoder, Jobs
         ↓
[ ForensicEdu.Application ]      -> Casos de uso, Interfaces, Antifraude, Evaluaciones, DTOs
         ↓
[ ForensicEdu.Domain ]           -> Núcleo Puro: Entidades del Negocio, Enums y Reglas Inmutables
```

### 2.1 Capa de Dominio (`ForensicEdu.Domain`)
Ubicación: `src/ForensicEdu.Domain/`
* **Sin dependencias externas:** No depende de Entity Framework ni de ASP.NET Core; es C# puro.
* **`Entities/DomainEntities.cs`:**
  * `User`: Información académica, DNI, vigencia de cuenta, historial de compras.
  * `Course`, `CourseModule`, `CourseDocument`: Estructura pedagógica, control de descarga de PDFs y streaming de videos.
  * `LiveEvent`: Eventos síncronos (Congresos, Seminarios), con sala que se activa 15 minutos antes.
  * `AdvisoryService`: Asesoría pericial personalizada con consultor asignado y enlace a WhatsApp.
  * `Order`: Registro de orden de compra única (`ORD-YYYY-XXXXXX`).
  * `PaymentEvidence`: Comprobante de pago con campos antifraude (`PaymentFingerprint`, `VoucherFileHash`).
  * `Enrollment`: Matrícula del alumno, avance porcentual, estado de examen y nota final.
  * `ExamAttempt`: Historial de intentos (1º y 2º), puntaje (0 a 20), duración y respuestas JSON.
  * `DigitalCertificate`: Registro oficial del certificado emitido, URL de validación QR y estado (`VALIDO`, `REVOCADO`, `ANULADO`).
  * `CertificateVerificationLog`: Trazabilidad y auditoría de cada escaneo QR público (IP, navegador, fecha).
  * `SupportTicket`: Sistema de tickets y consultas con respuestas asociadas.
  * `AuditLog`: Registro inmutable de cada acción administrativa o financiera.
  * `SystemSocialSetting`: Configuración dinámica de enlaces a redes sociales (Facebook, Instagram, etc.).
* **`Enums/DomainEnums.cs`:**
  * Estados normalizados con compatibilidad en mayúsculas y PascalCase para evitar discrepancias de serialización JSON.

### 2.2 Capa de Aplicación (`ForensicEdu.Application`)
Ubicación: `src/ForensicEdu.Application/`
* **`Common/Interfaces/IApplicationDbContext.cs`:** Abstracción que expone los `DbSet<T>` para desacoplar el acceso a datos.
* **`Common/Interfaces/IServiceInterfaces.cs`:** Contratos para antifraude, generación de QR, PDFs, ciclo de vida de cuentas y evaluación.
* **`Services/AntifraudService.cs`:**
  * `GenerateFingerprint(...)`: Crea la cadena única: `BANCO|NUM_OPERACION|FECHA|MONTO|TITULAR`.
  * `ComputeVoucherSha256(...)`: Calcula el hash criptográfico del archivo de voucher.
  * `EvaluatePaymentAsync(...)`: Valida contra la base de datos si la huella o el hash ya existen en algún pago `APROBADO`.
* **`Services/EvaluationService.cs`:**
  * `SubmitExamAttemptAsync(...)`: Valida tiempos, calcula puntaje sobre 20, verifica si aplica 2º intento o 24 horas de espera, y genera el certificado automáticamente al alcanzar nota >= 13.
* **`Services/AccountLifecycleService.cs`:**
  * `ExtendValidityOnApprovedPaymentAsync(...)`: Extiende la cuenta por 2 años desde la fecha de compra.
  * `ProcessDailyAccountExpirationsAsync(...)`: Marca cuentas sin compras mayores a 30 días como `PENDIENTE_DE_ELIMINACION`.

### 2.3 Capa de Infraestructura (`ForensicEdu.Infrastructure`)
Ubicación: `src/ForensicEdu.Infrastructure/`
* **`Persistence/ApplicationDbContext.cs`:** Configura Entity Framework Core con Fluent API, índices únicos e integridad referencial.
* **Proveedores soportados:**
  * **In-Memory:** Preconfigurado por defecto para arrancar inmediatamente sin instalar SQL Server.
  * **SQL Server / PostgreSQL:** Listos para conectar mediante cadenas de conexión en `appsettings.json`.
* **`BackgroundJobs/AccountLifecycleHangfireJob.cs`:** Tarea programada en segundo plano con **Hangfire** que se ejecuta a las 00:00 UTC para auditar vencimientos.
* **Generación de Documentos y Criptografía:**
  * `QRCoder`: Genera imágenes PNG del código QR de validación.
  * `QuestPDF`: Genera certificados PDF vectoriales de alta calidad listos para impresión oficial.
  * `BCrypt.Net-Next`: Encriptación segura de contraseñas con salado criptográfico.

### 2.4 Capa Web API (`ForensicEdu.WebApi`)
Ubicación: `src/ForensicEdu.WebApi/`
* **Controladores REST:**
  * `AuthController`: `/api/auth/login`, `/api/auth/register`.
  * `CoursesController`: `/api/courses` (catálogo y módulos).
  * `OrdersController`: `/api/orders/checkout` (creación de orden y validación antifraude de comprobante).
  * `ExamsController`: `/api/exams/submit` (envío cronometrado de examen).
  * `CertificatesController`: `/api/certificates/verify/{code}`, `/api/certificates/download/{code}/pdf`.
  * `AdminController`: `/api/admin/pending-payments`, `/api/admin/payments/{id}/approve`, `/api/admin/certificates/{code}/revoke`, `/api/admin/audit-logs`.
* **Documentación Interactiva:** Integración con **Scalar** en `/scalar/v1` (la interfaz OpenAPI moderna para .NET 9).
* **CORS:** Configurado para admitir conexiones desde `http://localhost:3000`, `http://localhost:5173` y orígenes HTTPS.

---

## 3. Arquitectura del Frontend (React 18 + TypeScript)

Ubicación: `src/ForensicEdu.ClientApp/` (dentro de la solución VS 2022) y en la raíz del proyecto.

### 3.1 Tecnologías Utilizadas:
* **React 18 con TypeScript:** Componentes funcionales tipados con interfaces estrictas.
* **Vite:** Empaquetador ultrarrápido con Hot Module Replacement (HMR < 50ms).
* **Tailwind CSS:** Diseño moderno con estética forense profesional (tonos pizarra oscura, esmeralda y ámbar pericial).
* **Lucide React:** Iconografía vectorial institucional.
* **Canvas-Confetti:** Animación celebratoria al aprobar exámenes y obtener certificados.

### 3.2 Estructura de Componentes:
* **`Header.tsx`:** Barra superior con simulador de roles en vivo, indicador de vigencia de cuenta y botón de descarga del ZIP.
* **`CatalogSection.tsx`:** Catálogo de cursos, eventos en vivo y asesoría con botones de compra independiente.
* **`OrderAndPaymentModal.tsx`:** Modal de checkout donde se genera la orden y se registra el voucher con simulación antifraude.
* **`StudentDashboard.tsx`:** Panel completo del alumno con submódulos:
  * *Mis Cursos*: Acceso al aula virtual y progreso.
  * *Mis Eventos*: Cuenta regresiva y sala en vivo (15 minutos antes).
  * *Asesoría*: Canal privado con perito judicial y WhatsApp.
  * *Mis Certificados*: Descarga y visualización con QR.
  * *Mis Pagos*: Historial de órdenes y comprobantes.
  * *Mis Consultas*: Soporte por tickets.
  * *Perfil*: Edición de datos personales y vigencia.
* **`VirtualClassroom.tsx`:** Aula virtual con streaming protegido (sin descarga directa), documentos descargables con marca de agua y botón de rendir examen.
* **`ExamModal.tsx`:** Evaluación cronometrada a 10 minutos (600s), preguntas aleatorias, nota sobre 20, bloqueo por regla de 24h y segundo intento.
* **`CertificateView.tsx`:** Certificado digital con diseño de diploma de honor, código QR dinámico y botón de exportación PDF.
* **`PublicQRVerifier.tsx`:** Verificador público institucional para consultas por código o escáner QR.
* **`AdminPanel.tsx`:** Panel administrativo con pestañas de validación de pagos (detección de fraudes), gestión de certificados (revocaciones), auditoría y enlaces de redes sociales.
* **`FloatingSupport.tsx`:** Botón flotante accesible desde cualquier pantalla con enlaces a WhatsApp, soporte y tickets.

---

## 4. Guía Paso a Paso para Desarrolladores: ¿Cómo Hacer Cambios?

### 4.1 ¿Cómo agregar un nuevo curso al catálogo?
1. **En Backend (`ForensicEdu.Infrastructure/Persistence/ApplicationDbContext.cs`):**
   En el método de inicialización o seeding, añade una nueva instancia de `Course`:
   ```csharp
   new Course
   {
       Id = Guid.NewGuid().ToString(),
       Code = "CF-DOC-2026",
       ShortCode = "DOC",
       Title = "Documentología y Grafotecnia Forense",
       Category = "Forense",
       Price = 280.00m,
       AcademicHours = 60,
       Instructor = "Dra. Sofía Mendoza",
       QuestionsJson = "[...]"
   }
   ```
2. **En Frontend (`src/data/mockData.ts`):**
   Agrega el objeto en la lista `MOCK_COURSES` para que aparezca instantáneamente en la interfaz visual.

### 4.2 ¿Cómo agregar un nuevo campo a una Entidad?
1. **Modificar la Entidad:** Abre `src/ForensicEdu.Domain/Entities/DomainEntities.cs` y agrega la propiedad (ejemplo: `public string CarnetColegiado { get; set; } = string.Empty;` en `User`).
2. **Configurar en EF Core (si aplica longitud o índice):** Abre `src/ForensicEdu.Infrastructure/Persistence/ApplicationDbContext.cs` y en `OnModelCreating` añade:
   ```csharp
   modelBuilder.Entity<User>(entity => {
       entity.Property(e => e.CarnetColegiado).HasMaxLength(50);
   });
   ```
3. **Actualizar el DTO o Controlador:** Si el campo se recibe en un registro o login, agrégalo al request de `AuthController.cs`.
4. **Recompilar:** En Visual Studio pulsa `Ctrl + Shift + B`.

### 4.3 ¿Cómo cambiar el tiempo del examen o la nota mínima?
1. Abre `src/ForensicEdu.Application/Services/EvaluationService.cs`.
2. Para cambiar el tiempo máximo de duración:
   ```csharp
   DurationSeconds = 600; // 600 segundos = 10 minutos
   ```
3. Para cambiar la nota mínima aprobatoria:
   ```csharp
   bool approved = score >= 13.0m; // Modificar el umbral aquí
   ```
4. Para cambiar las horas de espera antes del 2º intento:
   ```csharp
   nextAttemptAllowedAt = DateTime.UtcNow.AddHours(24); // Configurar horas aquí
   ```

### 4.4 ¿Cómo cambiar de Base de Datos en Memoria a SQL Server o PostgreSQL?
Abre `src/ForensicEdu.Infrastructure/DependencyInjection.cs`:
* **Para SQL Server:**
  Descomenta la línea de SQL Server y coloca tu cadena de conexión:
  ```csharp
  services.AddDbContext<ApplicationDbContext>(options =>
      options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
  ```
* **En `appsettings.json` de `ForensicEdu.WebApi`:**
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ForensicEduDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
  ```

---

## 5. Matriz de Estados del Sistema

| Entidad | Estados Oficiales | Significado |
| :--- | :--- | :--- |
| **Orden** | `PENDIENTE_VALIDACION` | Creada, esperando revisión del voucher. |
| | `APROBADO` | Pago validado; habilita matrícula o evento. |
| | `RECHAZADO` | Comprobante rechazado por finanzas. |
| | `CANCELADA` | Orden anulada por el usuario o tiempo. |
| **Pago** | `PENDIENTE` | Registrado por el usuario, en cola contable. |
| | `EN_REVISION` | En proceso de verificación con el banco. |
| | `APROBADO` | Válido; renueva cuenta 2 años y matricula. |
| | `DUPLICADO` | Huella o voucher ya existía en otra compra aprobada. |
| | `OBSERVADO` | Requiere que el usuario aclare datos. |
| **Matrícula** | `ACTIVA` | Acceso regular al aula virtual. |
| | `COMPLETADA` | Evaluado y aprobado satisfactoriamente. |
| | `DESAPROBADA` | Desaprobó el segundo intento definitivo. |
| **Certificado**| `VALIDO` | Oficial y verificable públicamente. |
| | `REVOCADO` | Anulado administrativamente por causa registrada. |
| | `ANULADO` | Cancelado por error material. |
| **Cuenta** | `REGISTRADA` | Nueva cuenta (dispone de 30 días). |
| | `ACTIVO` | Cuenta con compras vigentes (2 años). |
| | `PENDIENTE_DE_ELIMINACION` | Transcurrieron 30 días sin compras aprobadas. |
| | `SUSPENDIDO` | Vencieron los 2 años sin nuevas compras. |

---

## 6. Procedimiento de Ejecución en Visual Studio 2022

1. Abre `ForensicEdu.sln` con **Visual Studio 2022**.
2. Verifica que el proyecto de inicio sea **`ForensicEdu.WebApi`** (aparece en **negrita** en el Explorador de soluciones).
3. En la barra superior, junto al botón verde de Play ▶️, asegúrate de seleccionar el perfil **`http`** o **`https`**.
4. Pulsa **F5**.
5. Se abrirá la documentación de la API en `https://localhost:7148/scalar/v1` o `http://localhost:5000/scalar/v1`.
6. Para iniciar el frontend interactivo en paralelo: abre una terminal en `src/ForensicEdu.ClientApp/` y corre `npm run dev`. Se abrirá la interfaz en `http://localhost:3000` con proxy directo a la API.
