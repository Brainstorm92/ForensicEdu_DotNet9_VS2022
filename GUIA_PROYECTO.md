# Guía del proyecto ForensicEdu

Esta guía describe el código revisado el 30 de octubre de 2026. Para conocer el cumplimiento funcional, consulta `REVISION_REQUISITOS.md`. El manual anterior contiene afirmaciones de funcionalidades completas que todavía son simulaciones; esta guía distingue ambas cosas.

## Qué ejecutas con F5

Selecciona WebApi como único proyecto de inicio, perfil `http`. Al compilar, .NET ejecuta `npm run build` en ClientApp y copia el resultado a `WebApi/wwwroot`. WebApi sirve la interfaz y la API en el mismo proceso:

| Dirección | Contenido |
|---|---|
| http://localhost:5000/ | Interfaz React compilada |
| http://localhost:5000/scalar/v1 | Explorador de API, solo en Development |
| http://localhost:5000/openapi/v1.json | Contrato de API, solo en Development |
| /api/courses | Catálogo leído de PostgreSQL |
| /api/auth/register y /api/auth/login | Registro y autenticación |
| /api/orders/checkout | Orden y registro de datos bancarios |
| /api/orders/{orderId}/voucher | Adjuntar archivo mediante formulario multipart |
| /api/exam-sessions/{enrollmentId}/start | Inicio o recuperación del examen |
| /api/exam-sessions/{attemptId}/answers | Guardar respuestas mientras transcurre el examen |
| /api/exams/submit | Calificar y cerrar el intento |

El puerto 3000 solo se utiliza si ejecutas Vite con `npm run dev` para desarrollo con recarga automática. No es necesario para la interfaz servida por .NET. No abras dos instancias de WebApi en el puerto 5000. Detener F5 detiene el sitio.

## Mapa de carpetas

```text
ForensicEdu.sln                 Solución de Visual Studio
src/
  ForensicEdu.Domain/          Entidades y estados de negocio
    Entities/DomainEntities.cs
    Enums/DomainEnums.cs
  ForensicEdu.Application/     Reglas y contratos
    Common/Interfaces/        Acceso abstracto a datos y servicios
    Services/                 Evaluación, antifraude, vigencia
  ForensicEdu.Infrastructure/  Implementaciones técnicas
    Persistence/              Modelo Entity Framework Core
    Services/                 Generación QR y certificados PDF
    BackgroundJobs/           Revisión diaria de vigencias
    DependencyInjection.cs    Selección del proveedor de datos
  ForensicEdu.WebApi/          Entrada HTTP y aplicación ejecutable
    Controllers/              Endpoints y permisos
    Program.cs                Registro de servicios y rutas
    ExpiredExamsWorker.cs     Cierre de exámenes vencidos
    Properties/               Perfiles de inicio
    appsettings.json          Configuración sin secretos de producción
    wwwroot/                  Resultado generado; no editar
    App_Data/vouchers/        Archivos privados; no publicar como estáticos
  ForensicEdu.ClientApp/       Interfaz React 19 con TypeScript
    src/main.tsx              Entrada de React
    src/App.tsx               Selección de pantalla
    src/context/AppContext.tsx Estado y procesos de demostración
    src/components/           Pantallas y modales
    src/data/mockData.ts      Datos ficticios y preguntas de demostración
    src/services/apiClient.ts Cliente HTTP; su existencia no implica integración
    src/types/                Tipos de datos de la interfaz
    vite.config.ts            Compilación y proxy durante desarrollo
    package.json              Dependencias y comandos
    package-lock.json         Versiones restaurables
tests/ForensicEdu.BusinessChecks/ Pruebas ejecutables de reglas
database-postgresql.sql        Esquema inicial generado desde EF Core
compose.yaml                   PostgreSQL local opcional con Docker
```

## Cómo leer el código

Empieza por `DomainEntities.cs`: define usuario, producto, orden, pago, matrícula, intento y certificado. Luego lee `OrdersController` para entender cómo entra una solicitud. El controlador usa `IApplicationDbContext`, cuya implementación es `ApplicationDbContext`. EF Core traduce las consultas C# a PostgreSQL. Los servicios de Application ejecutan las reglas compartidas. Finalmente, lee `AppContext.tsx` y sus consumidores para ver cómo la interfaz representa los procesos.

Las referencias son: Domain no depende de las otras capas; Application depende de Domain; Infrastructure implementa contratos de Application; WebApi configura y reúne las capas. React es un cliente independiente que se compila a HTML, CSS y JavaScript.

## Lo que todavía ocurre en el navegador

### Registro y sesión conectados

La barra superior ofrece **Crear cuenta** e **Iniciar sesión**. El formulario envía los datos personales y académicos a `/api/auth/register`; después permite iniciar sesión contra `/api/auth/login` y recuperar el perfil con `/api/auth/me`. El botón **Verificar mi registro** vuelve a consultar el perfil persistido en PostgreSQL. La contraseña y el JWT no se guardan en localStorage. La sesión se mantiene en memoria: tras recargar debes iniciar sesión otra vez, pero la cuenta permanece en la base. Al cerrar sesión se elimina el token de memoria. Al iniciar sesión real se oculta el selector de usuarios de demostración.

Esta conexión abarca registro, login y lectura de perfil. Los cambios de perfil desde la pantalla heredada y las compras, pagos y exámenes demo todavía no están integrados. No interpretar sus mensajes locales como confirmaciones de escritura en PostgreSQL.

La prueba `tests/Test-AccountRegistration.ps1` verifica el mismo contrato HTTP que usa el formulario, incluyendo duplicados, contraseña incorrecta, perfil protegido y persistencia independiente en PostgreSQL. Genera una cuenta temporal identificable, la comprueba y la retira al terminar. Requiere una instancia de prueba activa (por defecto puerto 5088) y los secretos de desarrollo configurados.

AppContext mantiene usuarios, pagos, matrículas y certificados en `localStorage`, con usuarios y roles de ejemplo. Esas acciones no crean necesariamente filas en PostgreSQL. Los botones de cambiar rol, reiniciar examen y saltar la espera son utilidades de demostración. El temporizador visual del modal no sustituye el control del servidor. La integración completa debe reemplazar estas funciones por peticiones autenticadas y actualizar el estado desde las respuestas reales.

## PostgreSQL

El proveedor continúa siendo `PostgreSQL`. Se verificó la base remota facilitada por el propietario, inicialmente vacía, y se crearon 16 tablas en una transacción. La conexión está en los secretos locales de WebApi (`UserSecretsId: ForensicEdu-Local-Development`), no en el repositorio. Visual Studio la carga en Development. La instancia local no se utiliza. Para otro servidor configura `ConnectionStrings__DefaultConnection` mediante variables de entorno o secretos de usuario:

```text
Host=localhost;Port=5432;Database=ForensicEduDb;Username=TU_USUARIO;Password=TU_CLAVE
```

Si prefieres Docker, instala Docker Desktop, define `POSTGRES_PASSWORD` en `.env` y ejecuta `docker compose up -d`. Usa `forensicedu` como usuario. Importa `database-postgresql.sql` exclusivamente en una base nueva y vacía mediante pgAdmin o psql. El archivo crea tablas y no es una migración para bases existentes. No borres tablas para actualizar una base con información real. Deben añadirse migraciones versionadas antes de evolucionar un entorno con datos.

## Autenticación y permisos

### Configuración de desarrollo en Visual Studio

Si Visual Studio no encuentra los secretos de usuario, WebApi carga explícitamente `appsettings.Local.json` desde la carpeta del proyecto en Development. Se configuró ese archivo local con la conexión proporcionada. Está excluido del repositorio, de la copia al directorio de salida y de la publicación. No compartirlo. Las variables de entorno y los argumentos de inicio tienen precedencia. En producción no se carga ese archivo y deben configurarse las variables de conexión correspondientes.

Comprueba la conexión con `/api/health/database` (solo Development). Debe indicar `connected: true` y la base remota esperada. Si falta toda configuración, el arranque falla con un mensaje explícito en lugar de utilizar PostgreSQL local o guardar información en memoria. Los errores transitorios de conexión tienen hasta tres reintentos y devuelven HTTP 503 si no se recuperan.

Login verifica BCrypt y entrega un JWT de 8 horas. Envía `Authorization: Bearer TOKEN` en las solicitudes protegidas. En Development se genera una clave temporal: reiniciar invalida las sesiones. En producción debes establecer `JwtSettings__Secret` con una clave aleatoria de al menos 32 caracteres. No publicar claves en el repositorio.

Roles en el token: 1 superadministrador, 2 académico, 3 financiero, 4 docente, 5 soporte, 6 estudiante, 7 consultor. Las operaciones existentes de administración financiera y académica tienen restricciones separadas. No existe aún un proceso de aprovisionamiento del primer administrador. Registro público siempre crea estudiantes; no permitir que el navegador asigne roles administrativos.

## Reglas corregidas y pruebas

```powershell
dotnet run --project tests/ForensicEdu.BusinessChecks
npm --prefix src/ForensicEdu.ClientApp run lint
dotnet build src/ForensicEdu.WebApi/ForensicEdu.WebApi.csproj
```

Las pruebas usan EF InMemory y cubren reglas de negocio, no conexiones reales ni concurrencia de PostgreSQL. Incluyen 12.99 frente a 13.00, bloqueo del segundo intento después de aprobar, espera de 24 horas, respuestas tardías, renovación desde la aprobación y duplicidad por banco y operación.

## Dónde cambiar cada cosa

| Cambio | Lugar |
|---|---|
| Diseño de pantallas | ClientApp/src/components e index.css |
| Estados mostrados en la demo | AppContext.tsx y types/index.ts |
| Autenticación | AuthController y Program.cs |
| Proveedor y conexión | Infrastructure/DependencyInjection y configuración de WebApi |
| Antifraude | AntifraudService y aprobación administrativa |
| Vigencia | AccountLifecycleService |
| Calificación | EvaluationService |
| Tiempo, recuperación y respuestas | ExamSessionsController y ExpiredExamsWorker |
| PDF y QR | Infrastructure/Services |
| Nuevas tablas | Domain, IApplicationDbContext, ApplicationDbContext y migraciones futuras |

No edites bin, obj, dist, node_modules ni wwwroot para cambiar el comportamiento. No reemplaces package.json, package-lock.json, los perfiles de inicio o la solución con una exportación antigua sin comparar sus cambios.
