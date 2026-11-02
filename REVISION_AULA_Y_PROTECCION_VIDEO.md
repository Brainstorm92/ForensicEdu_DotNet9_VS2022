# Aula y protección de videos

El aula usa la portada configurada en el curso, módulos y materiales reales, progreso de la matrícula y las reglas vigentes del examen (10 minutos, 13/20 y dos intentos). Conserva la espera académica configurada y la compra de una matrícula nueva al agotar intentos. Es adaptable a móvil y usa los temas claro y oscuro.

## Revisión del Word, sección 11.2

- Acceso autenticado: la API comprueba alumno, matrícula, pago y orden de módulos antes de emitir el enlace.
- RustFS privado: los objetos no se hacen públicos; reproducción mediante URL firmada de 15 minutos y renovación autorizada.
- Reproductor propio sin botón de descarga, sin menú contextual, con marca de agua visible que contiene el nombre y documento enmascarado del alumno. La pantalla completa conserva la marca.
- Materiales: continúa el visor privado con marca de agua existente.

## Límite de la protección

Ocultar controles y usar enlaces firmados no impide copiar el MP4 mediante herramientas del navegador mientras el enlace sea válido, ni grabar la pantalla. La marca de agua es una superposición, no está grabada dentro del archivo. Los videos externos conservan las condiciones y controles de su proveedor.

Actualmente RustFS entrega MP4 privado; no se ha implementado conversión a HLS ni DRM. Por tanto, se cumplen los controles de acceso y la ausencia de descarga directa en la interfaz; no se garantiza una imposibilidad absoluta de descarga. Para una protección mayor habrá que evaluar HLS cifrado y un proveedor con DRM.

No se subieron videos ni se modificó contenido real de cursos para esta revisión.
