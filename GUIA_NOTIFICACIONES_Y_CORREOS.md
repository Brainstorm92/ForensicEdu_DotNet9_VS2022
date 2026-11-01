# Correos y centro de notificaciones

Implementación del 1 de noviembre de 2026. Conserva el modelo visual aprobado: cabecera azul oscuro, acción verde y contenido claro. No incluye el bloque de enlace alternativo ni el pie descartado. El HTML escapa los datos de usuario; se incluye una alternativa de texto para lectores y clientes sin HTML.

## Cuándo se genera un aviso

| Evento | Correo y centro de notificaciones | Destino |
| --- | --- | --- |
| Pago de curso aprobado | Matrícula confirmada y fecha de vigencia | Mis Cursos y Evaluaciones |
| Pago de evento o asesoría aprobado | Compra confirmada y fecha de vigencia | Mis eventos / Mis asesorías |
| Respuesta de un administrador o soporte al alumno | Código, asunto y respuesta de la consulta | Mis consultas |
| Cuenta próxima a vencer | Aviso en las ventanas de 7, 3 y 1 día | Catálogo |
| Fin de 30 días sin compra aprobada | Cuenta pendiente de eliminación, conforme a la regla existente | Mis consultas |
| Fin de vigencia de una cuenta con compras | Suspensión temporal, conforme a la regla existente | Mis consultas |
| Renovación desde el servicio de vigencia | Nueva fecha de vigencia | Mi perfil |

La aprobación financiera guarda pago, matrícula, renovación y notificación en una misma transacción de SaveChanges. SMTP ocurre después, en otro trabajo, y sus fallos no revierten la compra ni una respuesta de soporte. No se reenvían matrículas o respuestas históricas de manera retroactiva.

Confirmación de correo y recuperación usan el mismo diseño y configuración. Sus enlaces continúan siendo de un solo uso: 24 horas para confirmación, 30 minutos para recuperación. Por contener enlaces de seguridad, estos correos se solicitan y envían en su flujo actual; no se almacenan en la bandeja ni en la cola educativa.

## Control de envío

Panel Admin → Correo Electronico controla todos los correos automáticos. Cuenta Gmail, clave de aplicación, nombre de remitente y URL del frontend continúan centralizados. La prueba manual conserva su comportamiento explícito aunque el envío automático esté apagado.

- Desactivado: los avisos se guardan y se muestran en la web; SMTP no se ejecuta.
- Activado: el trabajo procesa los pendientes cada dos minutos, mientras la API esté en ejecución.
- Los avisos educativos para cuentas nuevas esperan a que el correo esté confirmado. Las cuentas antiguas mantienen la compatibilidad del flujo anterior.
- Avisos de vencimiento antiguos no se envían después de renovar. Los que ya vencieron se descartan de la cola, pero se conservan en el historial web.
- Un fallo SMTP se reintenta con espera creciente, hasta cinco intentos. Guardar la configuración con el envío habilitado libera los envíos fallidos para volver a intentarlos después de corregir credenciales o conexión.
- La configuración, los tokens y el estado interno de entrega no se exponen en la API de notificaciones del alumno.

La auditoría de vigencia mantiene su horario diario existente, 00:00 UTC (19:00 de Lima). La ventana 7/3/1 usa tiempo de servidor: si la API estuvo detenida, genera el aviso correspondiente al plazo actual, sin disparar todos los recordatorios atrasados. Las reglas de alumnos no se aplican a cuentas administrativas.

## Uso por el alumno

La campana de la cabecera abre Mis notificaciones. El contador muestra las pendientes de lectura y se actualiza cada 30 segundos y al volver a la ventana. La pantalla permite filtrar sin leer, marcar un aviso o todos como leídos, actualizar y paginar. Abrir el destino marca el aviso como leído. La lectura se guarda en PostgreSQL y se conserva entre sesiones.

Cada consulta y modificación se limita al UserId autenticado, incluso si se proporciona el ID de un aviso ajeno. Una cuenta suspendida o pendiente de eliminación conserva los avisos almacenados, pero mantiene el bloqueo de acceso previo de la plataforma; recibe el correo de estado cuando corresponde.

Los botones de correo abren la URL del frontend configurado con #section= y una sección permitida. Al iniciar sesión se conserva el destino. No configures aquí la dirección de la API.

## Persistencia y despliegue

Nueva tabla: StudentNotifications. Incluye contenido del aviso, usuario, evento único, lectura y cola de entrega con control de versión. La actualización no elimina ni modifica cuentas existentes.

En una base existente ejecutar UNA VEZ, con las variables de conexión del entorno correspondiente:

    dotnet ForensicEdu.WebApi.dll --upgrade-notifications

En desarrollo, desde src/ForensicEdu.WebApi:

    dotnet run -- --upgrade-notifications

El comando es transaccional e idempotente y verifica lectura de la tabla al terminar. Para desarrollo debe usarse el entorno Development que carga los secretos y configuración local. Al desplegar, actualizar el esquema ANTES de iniciar la nueva API. Se debe reiniciar la API para cargar el código y los trabajos nuevos.

Hangfire agenda los trabajos, y la cola de correo permanece en PostgreSQL aunque se reinicie la API. El arrendamiento y la versión evitan envíos concurrentes de una misma fila. Como SMTP no ofrece una transacción compartida con PostgreSQL, una interrupción exactamente después de aceptar el correo y antes de confirmar su estado puede ocasionar un reenvío al recuperarse; no se promete entrega exactamente una vez.

## Archivos principales

- Infrastructure/Services/PlatformEmailTemplate.cs: diseño común.
- Application/Services/PasswordRecoveryService.cs y EmailConfirmationService.cs: correos de acceso.
- Application/Services/StudentNotifications.cs: generación de avisos dentro de la operación de negocio.
- Application/Services/StudentNotificationCenter.cs: bandeja y lectura por usuario.
- Infrastructure/Services/NotificationEmailDispatcher.cs: cola, interruptor, vigencia y reintentos.
- Infrastructure/BackgroundJobs/NotificationEmailJob.cs: ejecución periódica.
- ClientApp/src/components/StudentNotifications.tsx: campana y pantalla del alumno.
- Infrastructure/Persistence/Schema/20261101_StudentNotifications.sql: actualización PostgreSQL.

## Validación

Pruebas automatizadas con base en memoria y correo simulado: matrícula, respuesta de soporte, aislamiento de alumnos, estados de lectura, interruptor central, reactivación, no duplicación, avisos 7/3/1, vencimiento, renovación, reintentos y escape de HTML. También se verifican regresiones de registro, confirmación y recuperación. No se envían correos reales al ejecutar estas pruebas.

    dotnet run --project tests/ForensicEdu.EmailHttpChecks -- --notifications
    dotnet run --project tests/ForensicEdu.EmailHttpChecks -- --registration-email
