# Materiales privados de los cursos

## Uso

Panel Admin → Cursos y contenidos → Editar curso → Módulos y materiales: escribe un título, selecciona PDF o Word (.docx), y pulsa **Subir material privado**. Máximo 20 MB. Requiere superadministrador o administrador académico (roles 1 y 2). No hay que hacer migraciones: se usa la tabla existente CourseDocument.

En Mis Cursos y Evaluaciones → aula del curso → **Ver material**, el alumno lee el documento dentro de un modal responsive, con nombre, documento de identidad y fecha como marca de agua. El visor admite modo claro y nocturno, paginación y zoom de PDF; no tiene acciones de descarga, impresión o apertura externa.

PDF se dibuja en canvas con PDF.js; la marca se incorpora a los píxeles de cada página mostrada. Word se transforma en HTML seguro en el servidor y se muestra en un iframe sandbox sin scripts ni acceso al origen de la app. Se conservan párrafos, títulos básicos, negrita, cursiva, subrayado, tablas e imágenes PNG/JPEG incrustadas. Es una vista de lectura semántica, no una reproducción exacta de Word: para maquetación precisa exporta a PDF. Los .doc antiguos deben guardarse como .docx o PDF. No se admiten macros, objetos Office incrustados ni HTML alternativo.

## Acceso y almacenamiento

- RustFS utiliza el bucket privado ya configurado por ObjectStorage y claves `documents/<UUID>.pdf` o `.docx`. No se genera URL pública ni enlace firmado para materiales.
- En la base solo se guarda `rustfs://documents/<UUID>.<ext>`, tipo, tamaño, título y marca obligatoria. La API pública de imágenes solo puede leer `images/<UUID>.webp`; no sirve documentos.
- Cada lectura verifica usuario vigente, matrícula propia ACTIVA o COMPLETADA, orden APROBADA del mismo usuario y mismo curso. La asociación del documento al módulo del curso se verifica en servidor. Roles administrativos no dan acceso automático a la matrícula de otro usuario.
- Tanto metadatos como contenido requieren JWT. Una lectura con otra matrícula/documento devuelve 404, sin entregar bytes. Cuenta suspendida o vencida también queda bloqueada por el guard del token.
- Contenido lleva `Cache-Control: no-store, private`, `nosniff`, sin descarga attachment y CSP restrictiva. Los originales Word no se entregan al navegador. PDF necesita entregar bytes al visor autorizado.
- La lectura del archivo se audita como PRIVATE_DOCUMENT_VIEWED y la subida como PRIVATE_DOCUMENT_UPLOADED.
- El visor revalida acceso cada 60 segundos y limpia el contenido si la sesión o matrícula dejaron de autorizarlo. No persiste materiales en localStorage ni crea URLs blob públicas.
- Quitar un material revoca futuras lecturas por la API. Los objetos conservados en RustFS no se eliminan automáticamente al quitar el material o módulo; quedan privados, pendientes de política de retención/limpieza. Una pantalla ya cargada puede conservar el contenido hasta su siguiente validación; nada puede retirar una copia ya obtenida.

## Materiales antiguos

Los registros de enlaces HTTPS existentes se conservan, pero el endpoint ya no devuelve sus URLs: solicita volver a subir el archivo privado. No se descargan URLs antiguas desde el servidor para evitar peticiones arbitrarias a terceros. Cambiar un enlace en la API de metadatos no lo convierte en privado. Para reemplazar un material privado, quítalo y sube el nuevo archivo.

## Límites de protección

Esto es control de acceso y disuasión, no DRM. Un usuario autorizado recibe contenido para verlo; puede extraer el PDF mediante herramientas del navegador, quitar protecciones del cliente, realizar capturas, grabar pantalla o fotografiarla. La aplicación no puede garantizar bloquear capturas. No hay botones de descarga o impresión y la marca identifica al lector. Los administradores deben mantener privado el bucket y aplicar el proceso institucional que corresponda para distribuir materiales.

Los archivos subidos se validan por extensión y contenido, con límites de tamaño y expansión ZIP/XML, rechazo de macros/objetos y sin ejecución de vínculos externos. Esto no sustituye análisis antivirus de los documentos antes de subirlos.

## Endpoints

- POST `/api/admin/courses/{courseId}/modules/{moduleId}/documents/upload`, multipart: `title`, `file`.
- GET `/api/student/enrollments/{id}/documents/{documentId}`, devuelve título, tipo y marca personal, sin URL de almacenamiento.
- GET `/api/student/enrollments/{enrollmentId}/documents/{documentId}/content`, devuelve PDF o vista HTML segura de Word después de autorizar otra vez.

## Pruebas

`dotnet run --project tests/ForensicEdu.DocumentChecks`: pruebas de archivos falsos, macros, límites, HTML/XML seguro, pertenencia del módulo, pago, vigencia, IDOR, legado sin enlaces y fallos del almacenamiento.

`dotnet run --project tests/ForensicEdu.ArchitectureChecks`: registro de los 83 handlers y pipeline.

`tests/Test-ConnectedUi.mjs` con `PRIVATE_DOCUMENT_TEST=1`, `DOCUMENT_PDF_FILE` y `DOCUMENT_DOCX_FILE` añade subida real por UI y lectura PDF/Word con capturas, permisos HTTP y verificación del bucket privado a la base PostgreSQL de prueba aislada. Usa las variables ObjectStorage__* del proceso de pruebas; nunca apuntes estos tests a la base de producción. Los archivos de prueba se eliminan de RustFS al finalizar.

Validación realizada: compilación sin errores/advertencias, TypeScript sin errores, 31 comprobaciones específicas y ArchitectureChecks correctos. El navegador completó subida PDF/DOCX a RustFS real y lectura desde PostgreSQL aislado, incluyendo móvil, marca personal, cabeceras sin caché, impresión oculta, acceso anónimo rechazado, IDOR y matrícula cancelada. Se conservaron los flujos de pago, examen y certificado. Capturas de prueba en .artifacts/document-browser (no incluidas en Git).
