# Pruebas de la API con PostgreSQL

La API dispone de 83 casos de uso de negocio y una ruta adicional de configuración de Google. Los nuevos casos de uso pasan por MediatR, FluentValidation y EF Core con PostgreSQL. Se probaron contra una instancia PostgreSQL 17 local aislada, usando el esquema existente. Las pruebas HTTP y de navegador usan bases locales aisladas. Los productos de prueba y el superadministrador solicitados se aprovisionan por comandos locales contra la conexión configurada.

## Iniciar y autenticar

1. Ejecuta WebApi con el perfil http de Visual Studio. Abre http://localhost:5000/scalar/v1. La interfaz React permanece en /app/.
2. Configura ConnectionStrings:DefaultConnection mediante los secretos de usuario o appsettings.Local.json ya existente. No pegues credenciales en el repositorio.
3. Si aún no existe un superadministrador, créalo desde una terminal local con el comando siguiente. Se niega a ejecutarse si ya existe uno o si el correo pertenece a una cuenta; nunca promueve registros existentes.

```powershell
$env:BootstrapAdmin__Email = Read-Host 'Correo del administrador'
$adminSecret = Read-Host 'Contraseña del administrador (mínimo 12 caracteres)' -AsSecureString
$env:BootstrapAdmin__Password = [System.Net.NetworkCredential]::new('', $adminSecret).Password
try {
  dotnet run --project src/ForensicEdu.WebApi --launch-profile http -- --bootstrap-admin
} finally {
  Remove-Item Env:BootstrapAdmin__Password, Env:BootstrapAdmin__Email
}
```

4. Inicia la API normalmente. Ejecuta POST /api/auth/login con ese correo y contraseña. Copia token y pégalo como Bearer en la autenticación de Scalar. Los endpoints públicos no requieren token.
5. Registra un alumno con POST /api/auth/register e inicia sesión para obtener su token. No uses el token del administrador para simular la identidad del alumno.

## Recorrido de curso

1. Con el administrador, crea un curso en POST /api/admin/courses. Define isPublished=true e isActive=true para publicarlo y conserva su id.
2. Crea un módulo y sus documentos mediante las rutas de contenido. Guarda el banco de preguntas con PUT /api/admin/courses/{id}/questions.
3. Con el alumno, llama POST /api/orders/checkout con userId propio, productType=1, productId del curso, el precio exacto y datos bancarios. El precio procede de la base.
4. Adjunta un archivo JPG, PNG o PDF real con POST /api/orders/{orderId}/voucher (multipart, campo file, máximo 5 MB).
5. Con el administrador financiero o superadministrador, consulta GET /api/admin/payments, abre el voucher y aprueba con POST /api/admin/payments/{paymentId}/approve. No uses review para aprobar.
6. Con el alumno, consulta matrículas en GET /api/student/enrollments y el aula en GET /api/student/enrollments/{id}/classroom. Completa los módulos en orden.
7. Inicia el examen en POST /api/exam-sessions/{enrollmentId}/start, guarda respuestas con PUT /api/exam-sessions/{attemptId}/answers y entrega mediante POST /api/exams/submit.
8. Al aprobar, consulta GET /api/student/certificates. Verifica públicamente el código y descarga el PDF con el token del propietario o del administrador académico.

## Ejemplo de curso

```json
{
  "title": "Balística forense",
  "slug": "balistica-forense",
  "category": "Forense",
  "price": 100,
  "academicHours": 40,
  "instructor": "Docente",
  "shortDescription": "Curso de balística",
  "isPublished": true,
  "isActive": true
}
```

## Ejemplo del banco de preguntas

```json
{
  "questions": [
    {
      "id": "q1",
      "question": "Pregunta de ejemplo",
      "options": [
        "Opción A",
        "Opción B"
      ],
      "correctOptionIndex": 1,
      "explanation": "La segunda opción es correcta."
    }
  ]
}
```

Los índices empiezan en cero. Las claves y explicaciones solo se entregan al administrador académico; la sesión del examen devuelve únicamente las preguntas públicas.

## Roles y respuestas

Roles: 1 superadministrador, 2 académico, 3 financiero, 4 docente, 5 soporte, 6 estudiante, 7 consultor. Los endpoints de gestión del catálogo requieren 1 o 2; pagos 1 o 3; soporte 1 o 5; roles y estados de usuarios solo 1. Los docentes y consultores no reciben acceso global a información académica sin un modelo de asignaciones.

Las cuentas suspendidas o vencidas no pueden usar tokens previos. Un cambio de rol invalida el token anterior y requiere un nuevo login. Cambiar contraseña impide nuevos logins con la clave anterior; los JWT ya emitidos conservan su vigencia hasta expirar (8 horas), salvo suspensión o cambio de rol.

Listados nuevos: page=1, pageSize=20 (máximo 100) y search opcional. Filtros de pagos y tickets: status numérico. Ejemplos de pagos: 1 pendiente, 2 en revisión, 3 aprobado, 4 rechazado, 5 duplicado, 6 observado. Estados de tickets: 1 abierto, 2 en atención, 3 respondido, 4 cerrado.

Las respuestas usan 200 para datos, 204 para modificaciones sin cuerpo, 400 para validación, 401 para sesión no válida, 403 para falta de permisos, 404 para recursos inexistentes o ajenos, 409 para conflictos y 503 para PostgreSQL o recursos no disponibles.

## Endpoints añadidos

| Método | Ruta | Acceso |
|---|---|---|
| GET | /api/events | Público |
| GET | /api/events/{id} | Público |
| GET | /api/advisories | Público |
| GET | /api/advisories/{id} | Público |
| GET | /api/admin/courses | 1,2 |
| GET | /api/admin/courses/{id} | 1,2 |
| POST | /api/admin/courses | 1,2 |
| PUT | /api/admin/courses/{id} | 1,2 |
| DELETE | /api/admin/courses/{id} | 1,2 |
| POST | /api/admin/courses/{id}/modules | 1,2 |
| PUT | /api/admin/courses/{id}/modules/{moduleId} | 1,2 |
| DELETE | /api/admin/courses/{id}/modules/{moduleId} | 1,2 |
| POST | /api/admin/courses/{id}/modules/{moduleId}/documents | 1,2 |
| PUT | /api/admin/courses/{id}/modules/{moduleId}/documents/{documentId} | 1,2 |
| DELETE | /api/admin/courses/{id}/modules/{moduleId}/documents/{documentId} | 1,2 |
| GET | /api/admin/courses/{id}/questions | 1,2 |
| PUT | /api/admin/courses/{id}/questions | 1,2 |
| GET | /api/admin/events | 1,2 |
| GET | /api/admin/events/{id} | 1,2 |
| POST | /api/admin/events | 1,2 |
| PUT | /api/admin/events/{id} | 1,2 |
| DELETE | /api/admin/events/{id} | 1,2 |
| GET | /api/admin/advisories | 1,2 |
| GET | /api/admin/advisories/{id} | 1,2 |
| POST | /api/admin/advisories | 1,2 |
| PUT | /api/admin/advisories/{id} | 1,2 |
| DELETE | /api/admin/advisories/{id} | 1,2 |
| PUT | /api/auth/me | Cuenta autenticada |
| PUT | /api/auth/password | Cuenta autenticada |
| GET | /api/admin/users | 1,5 |
| GET | /api/admin/users/{id} | 1,5 |
| PUT | /api/admin/users/{id}/status | 1 |
| PUT | /api/admin/users/{id}/role | 1 |
| GET | /api/student/certificates | Cuenta autenticada |
| GET | /api/student/payments | Cuenta autenticada |
| GET | /api/student/enrollments/{id}/classroom | Cuenta autenticada |
| GET | /api/student/enrollments/{id}/attempts | Cuenta autenticada |
| GET | /api/student/enrollments/{id}/modules/{moduleId}/video | Cuenta autenticada |
| GET | /api/student/enrollments/{id}/documents/{documentId} | Cuenta autenticada |
| GET | /api/admin/payments | 1,3 |
| GET | /api/admin/payments/{id} | 1,3 |
| PUT | /api/admin/payments/{id}/review | 1,3 |
| GET | /api/admin/payments/{id}/voucher | 1,3 |
| GET | /api/admin/tickets | 1,5 |
| GET | /api/admin/tickets/{id} | 1,5 |
| PUT | /api/admin/tickets/{id}/status | 1,5 |
| GET | /api/admin/reports/summary | 1 |
| GET | /api/admin/enrollments | 1,2 |
| GET | /api/admin/exam-attempts | 1,2 |
| GET | /api/admin/certificates | 1,2 |

## Límites concretos

Las pantallas de catálogo, registro, pagos, aprobación, aula, examen y certificados utilizan la API. AppContext carga registros de PostgreSQL y no persiste datos académicos en localStorage. Los datos de demostración anteriores no se migran. Después de recargar, inicia sesión de nuevo: el token solo se mantiene en memoria.

Los videos siguen usando enlaces HTTPS. Los materiales PDF y Word (.docx) se suben a RustFS privado y se leen en el visor con autorización por matrícula y marca de agua personal. Los enlaces antiguos de materiales se bloquean hasta que se vuelva a subir el archivo. No se ha integrado Zoom, correo ni HLS. Ver DOCUMENTOS_PRIVADOS.md para rutas, restricciones y límites de protección.

DELETE de un curso lo archiva; DELETE de un evento lo cancela. No se eliminan módulos de cursos con matrículas ni asesorías con órdenes. Las actualizaciones no alteran el precio de órdenes ya creadas. La observación de pagos queda registrada en auditoría.

La detección de duplicados se prueba secuencialmente. Antes de producción debe completarse la protección PostgreSQL contra aprobaciones concurrentes, junto con pruebas de carga y migraciones versionadas. No se afirma que ese control concurrente esté terminado.

## Pruebas

```powershell
dotnet run --project tests/ForensicEdu.BusinessChecks
dotnet run --project tests/ForensicEdu.ArchitectureChecks
# Integración real: requiere una instancia PostgreSQL local aislada con trust solo para pruebas.
# Definir API_TEST_DLL y FORENSIC_TEST_PG_PORT; el script crea su propia base forensicedu_test_*.
node tests/Test-PlatformEndpoints.mjs
```


La arquitectura mantiene los endpoints y contratos anteriores, con request, handler y validador separados. Los fallos transitorios repetidos de PostgreSQL pueden abrir el circuit breaker y devolver 503 con Retry-After. El servidor no reintenta automáticamente una escritura: ante pérdida de confirmación, consulta primero si la operación se registró. Consulta ARQUITECTURA_MEDIATR.md para configuración y pruebas.


## Administración y datos de prueba

- Panel Admin → Cursos y contenidos: crear borradores, editar, publicar/archivar; agregar/editar módulos, enlaces de video y materiales PDF; editar el banco de preguntas y marcar respuestas correctas.
- Panel Admin → Gestión de usuarios: superadministrador crea cuentas por identidad, rol y vigencia; asigna roles y cambia estados. Soporte solo consulta usuarios.
- Roles y permisos presenta los siete perfiles y los permisos implementados. Docente y consultor no reciben edición global de cursos. No se muestran garantías ficticias de 2FA, DRM, firmas criptográficas ni roles personalizados.
- POST /api/admin/users: exclusivo SUPERADMIN. Contrato: nombres, apellidos, email, password (12 a 72 bytes), role (1–7), validDays (0 permanente para personal; estudiante requiere días positivos), dni y phone opcionales. No devuelve hashes.
- Iniciar sesión acepta correo o Username. La contraseña nunca se almacena en texto claro.

Para bases existentes, ejecutar `database-upgrade-usernames.sql` antes de iniciar la nueva versión. Es una actualización aditiva; no elimina registros. `database-postgresql.sql` ya la incluye para instalaciones nuevas.

Alta local explícita de un superadministrador adicional: definir temporalmente `AdminProvision__Username` y `AdminProvision__Password`, ejecutar la API con `--provision-admin` y eliminar las variables después. El comando aplica la columna Username, crea la cuenta y audita el alta; no reemplaza ni asciende cuentas existentes. Si se repite con la misma identidad y contraseña, no duplica la cuenta. No existe endpoint público de aprovisionamiento. No guardar contraseñas en el repositorio.

En Development, `--seed-demo` usa Entity Framework para insertar dos cursos identificados con `[PRUEBA]` (Balística básica y Escena del crimen), sus módulos y preguntas, un seminario y una asesoría. Es idempotente, conserva los productos existentes y no crea órdenes, pagos, matrículas, intentos ni certificados ficticios. Estos aparecen al efectuar las acciones desde una cuenta real. Los productos de prueba no incluyen videos ni contactos externos.

Recorrido para probar: registrarse → comprar → subir comprobante → aprobar como financiero/superadmin → entrar al aula → completar módulos → rendir examen → consultar certificado. Verificar en `Orders`, `PaymentEvidences`, `Enrollments`, `ExamAttempts`, `DigitalCertificates` filtrando por usuario/orden.

`tests/Test-ConnectedUi.mjs` automatiza en Chrome el recorrido hasta SQL con una base PostgreSQL local aislada: seed idempotente, ingreso por Username, alta administrativa de curso/módulo/preguntas/usuario, pago, aprobación, examen, certificado, recuperación tras recarga, error de red sin éxito simulado y panel móvil. Requiere API_TEST_DLL y FORENSIC_TEST_PG_PORT; no utiliza la base configurada del usuario.


El acceso con Google y el registro renovado están documentados en ACCESO_GOOGLE.md. La ruta de configuración no consulta datos de negocio; emite el nonce de sesión. El Client ID local se reutiliza de PSYCO_APP, con firma, audiencia y nonce validados por la API. No se vinculan usuarios automáticamente por correo.

## Materiales privados

POST /api/admin/courses/{courseId}/modules/{moduleId}/documents/upload (multipart title/file, roles 1 y 2, 20 MB). GET /api/student/enrollments/{enrollmentId}/documents/{documentId}/content requiere cuenta vigente, matrícula propia y pago aprobado del mismo curso. La ruta de metadatos ya no devuelve enlaces públicos. Los endpoints JSON anteriores se conservan por compatibilidad de metadatos, pero sus enlaces no autorizan lectura. Ver DOCUMENTOS_PRIVADOS.md.
