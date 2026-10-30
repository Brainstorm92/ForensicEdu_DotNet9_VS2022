# Revisión funcional frente al PDF y Word v5

Fecha: 30 de octubre de 2026. Referencias: `Flujo_Procesos_Plataforma_Educativa_Francisco_v5 (1).docx` y `Flujo_Procesos_Plataforma_Educativa_EDITABLE_Francisco (1).pdf`, carpeta `D:/Documentos/MODULO CURSOS/Archivos`. No se recibió PowerPoint. PostgreSQL es el proveedor elegido.

## Conclusión de la revisión

El proyecto no cumple todavía integralmente los documentos. La interfaz es una demostración con datos locales y el backend tenía varias reglas incompletas. Las correcciones de esta entrega son cambios de código, no una certificación de funcionamiento en producción. Compilar no equivale a probar un proceso con base real. Se verificó la conexión PostgreSQL remota proporcionada por el propietario y se inicializaron 16 tablas sobre la base vacía, dentro de una transacción. La conexión se conserva exclusivamente en secretos locales de desarrollo.

En la matriz, «backend» significa que existe implementación en C#, «demo» que solo está representado localmente en React y «pendiente» que falta implementación, integración o validación. Las secciones corresponden al Word; el PDF resume los mismos recorridos.

## Matriz de trazabilidad

| Sección | Requisito | Hallazgo inicial | Cambio o estado verificable | Pendiente para cumplimiento completo |
|---|---|---|---|---|
| 1 a 3 | Cuenta única y recorrido integral | Pantallas conectadas mediante estado local | Se conserva el diseño y se explican las dos capas | Integrar todos los recorridos React con API y PostgreSQL |
| 4.1 | Identidad y perfil académico, términos y consentimiento | Entidad tiene campos duplicados; registro HTTP solo recibe datos básicos | Se mantienen los datos existentes sin migración destructiva | Completar DTO, validación de fecha de nacimiento, consentimiento versionado y perfil real |
| 4.2 | Login y contraseña segura | Login ignoraba contraseña y entregaba token ficticio | Ahora verifica BCrypt, entrega JWT firmado, revisa vigencia y restringe endpoints | Integrar formulario de login, correo de verificación, recuperación y cierre de sesión/revocación |
| 4.3 | 30 días sin pago, 2 años desde aprobación | Renovación desde fecha bancaria, con acumulación indebida | Corregida a fecha de aprobación; cuenta registrada incluida en caducidad; sincroniza campos de estado | Avisos 7/3/1 días, política y ejecución de eliminación respetando conservación |
| 5 | Catálogo completo y editable | Catálogo demo y consultas de cursos | Consulta pública ya no expone claves de preguntas ni URL de video | CRUD completo, precios, imágenes y datos reales |
| 6 | Asesoría independiente, consultor e historial | Servicio de catálogo sin atención por usuario | Endpoint de asesorías adquiridas exige orden aprobada | Entidad de atención, asignación, sesiones, agenda opcional e historial |
| 7 | Una orden por producto y precio vigente | API aceptaba precio del navegador y producto arbitrario | Valida existencia y precio del producto; código único con UUID | Separar crear orden de registrar pago; estados pendientes de pago/registrado distintos |
| 8 | Datos bancarios y voucher JPG/PNG/PDF | No recibía archivo real | Endpoint multipart privado de hasta 5 MB, firma de archivo, SHA-256; aprobación exige voucher | Visor financiero autenticado, rechazo/observación, edición y nueva carga desde React |
| 9 | Duplicidad y antifraude | Solo huella completa antes de registrar | Comprueba banco+operación, huella y hash; revalida antes de aprobar; monto normalizado invariable | Índices parciales y transacción PostgreSQL contra aprobaciones simultáneas; revisión de coincidencias dudosas |
| 10 | Matrícula automática y progreso | Backend creaba matrícula sin OrderId | Guarda vínculo a orden; endpoint de progreso secuencial y listado propio | Verificación efectiva de materiales obligatorios, correo y conexión del aula |
| 11.1 | Materiales descargables y auditoría | Entidades/documentos de ejemplo | Evita exponer ubicaciones de video en catálogo | Descarga privada por matrícula, auditoría de descarga y marca de agua opcional |
| 11.2 | Streaming autenticado sin URL pública | Videos de ejemplo | No se publica URL de video en catálogo público | Elegir proveedor y emitir enlaces temporales, HLS y validación de sesiones; no se garantiza impedir capturas |
| 12.1 | Diez minutos controlados por servidor | Tiempo declarado por navegador; no existía sesión | Inicio persistido, respuestas guardadas, límite de 600 segundos, recuperación y cierre periódico con últimas respuestas | Integrar modal, pruebas de concurrencia y recuperación de red; orden nuevo de preguntas implementado, selección de banco pendiente |
| 12.2 | Aprobar desde 13, sin segundo intento | Redondeo a un decimal; estimación de respuestas correctas si JSON fallaba | Eliminada estimación; dos decimales; prueba exacta 12.99/13.00 y bloqueo tras aprobar | Contrato único de preguntas y validación exhaustiva de opciones |
| 12.3 | Segundo intento, espera configurable, nota reemplaza | Espera fija y controles demo para saltarla | Espera configurable por administrador mediante ExamRetakeWaitHours; segundo reemplaza, máximo dos | Conectar administración y modal; retirar utilidades demo del entorno real |
| 13 | PDF, QR, código único y revocación | Un revocado devolvía isValid=true; DNI público | isValid refleja estado; oculta DNI; bloquea PDF no válido; código con UUID evita contador concurrente | Dominio real del QR, página pública conectada y protección de descarga para propietario |
| 14 | Evento pagado, apertura 15 minutos antes y cierre | Acceso principalmente demo | Lista eventos adquiridos y acceso por orden aprobada; bloquea antes de apertura, cancelado o finalizado | Integración Zoom/streaming, cierre automático por horario configurable e inscripción explícita |
| 15.1 | Redes editables/activables | Solo localStorage | API pública de enlaces y edición académica/superadmin, HTTPS y vacío para desactivar | Conectar pie de página y panel |
| 15.2 | Tickets, estados y correo | Datos locales | Crear, listar propios y responder; soporte/superadmin puede responder; auditoría | Bandeja de soporte, cambio de estados, adjuntos y correo real |
| 16 | Panel del alumno | Secciones visuales con datos demo | Endpoints propios para matrículas, eventos, asesorías y tickets | Conectar pagos, perfil, certificados, consultas y notificaciones a API |
| 17 | Panel administrativo completo | Presentación local, endpoints parciales | Permisos de aprobación financiera y revocación académica | CRUD de usuarios, contenido, cuestionarios, eventos, reportes y gestión integral |
| 18 | Siete roles con permisos | Selector local y API sin autorización | JWT con rol; protege endpoints existentes; identidad de auditoría proviene del token | Matriz completa por acción, permisos del docente/consultor y primer administrador seguro |
| 19 | Modelo de datos funcional | Entidades parciales y atributos duplicados | Nueva instantánea de preguntas e índice único de número de intento | Migraciones, eventos inscritos, asesorías atendidas, notificaciones, sesiones de recuperación y normalización |
| 20 | Reglas aplicadas en backend | Parte solo en React | Correcciones descritas y pruebas ejecutables | Pruebas integrales de cada criterio con PostgreSQL y frontend |
| 21 | Seguridad, auditoría, HTTPS, backups | Clave JWT fija, CORS abierto, acciones sin identidad | Clave local temporal/producción externa; elimina origen arbitrario; restringe dashboard Hangfire | HTTPS de producción, auditoría completa/inmutable, backups con restauración probada y rate limiting |
| 22 | Estados coherentes | Enums con alias y estados de orden indistinguibles | Conservados por compatibilidad | Unificar contrato JSON y separar estados sin perder datos |
| 23 | Excepciones y recuperación | Dependía del navegador | Respuestas tardías ignoradas, sesión recuperable y rechazo de duplicados | Cancelación/devolución de eventos, voucher ilegible, políticas de reintento y fallos externos |
| 24 | Criterios de aceptación | Sin pruebas automatizadas de reglas | Ejecutable BusinessChecks con diez comprobaciones | Pruebas HTTP/roles, PostgreSQL, concurrencia, PDFs, streaming y recorridos del alumno |
| 25 | Diagrama | PDF funcional de referencia | Guía del flujo entre carpetas y capas | Actualizar diagrama cuando termine la integración real |

## Cambios en el contrato de API

Actualización de integración de cuenta: se añadieron formularios React de registro e inicio de sesión, consulta autenticada `/api/auth/me` y verificación del perfil persistido. El registro guarda nombres, apellidos, tipo/número de documento, país, teléfono y datos académicos existentes; registra la aceptación de términos y privacidad en auditoría. Aún faltan fecha de nacimiento, textos legales definitivos/versionados, confirmación de correo, recuperación de contraseña y edición real del perfil. Las otras pantallas permanecen demo. La prueba HTTP de cuenta pasó contra PostgreSQL remoto y comprobó la fila directamente en la base; retiró su usuario temporal al finalizar. No se ha realizado una prueba visual automatizada del formulario en navegador.

Las operaciones de órdenes, exámenes y administración ahora requieren `Authorization: Bearer TOKEN`. El parámetro adminUserId deja de decidir quién valida. La entrega de examen requiere una sesión iniciada previamente. El campo DurationSeconds se conserva por compatibilidad pero ya no controla el reloj. Deben guardarse respuestas periódicamente para que el cierre automático pueda recuperar las últimas respuestas recibidas.

El tiempo máximo es 600 segundos; el worker cierra intentos vencidos aproximadamente cada 15 segundos mientras WebApi está activo. Si el proceso está detenido, los cerrará al reiniciar. No se aceptan respuestas nuevas después del límite, aunque todavía no haya ejecutado el worker. Se necesita un despliegue continuo para cierre oportuno.

La base inicial debe regenerarse desde el modelo actualizado con:

```powershell
dotnet run --project tests/ForensicEdu.BusinessChecks -- --schema database-postgresql.sql
```

No aplicar ese script encima de una base con tablas/datos. Para bases existentes faltan migraciones versionadas. No se ha eliminado ni sustituido ninguna base.

## Validación y límites

Diez comprobaciones de reglas pasan con EF InMemory. La compilación aislada permite evitar interferencias con Visual Studio abierto. Esto no valida transacciones, índices ni compatibilidad de un esquema ya instalado. La compilación de Vite tiene avisos de tamaño de bundle y de uso de __dirname; no son fallos de compilación.

El frontend aún no se debe usar con alumnos reales: conserva roles y acciones demo en localStorage. El manual técnico anterior y su representación dentro de React son material heredado; no deben tomarse como evidencia de cumplimiento. Esta matriz documenta los pendientes sin marcarlos como finalizados.

## Orden de trabajo restante

1. Disponer de PostgreSQL, crear esquema mediante migraciones y aprovisionar administrador con credenciales privadas.
2. Conectar autenticación y sustituir progresivamente AppContext por datos HTTP, manteniendo el diseño.
3. Cerrar el recorrido de curso: catálogo, orden, voucher, validación financiera, matrícula, contenidos, sesión de examen y certificado.
4. Completar gestión de eventos, historial de asesorías, soporte, comunicaciones y administración.
5. Contratar/configurar streaming y correo; configurar dominio, HTTPS, backups y conservación de datos.
6. Ejecutar todos los criterios de aceptación sobre una base de prueba persistente y navegador antes de publicar.
