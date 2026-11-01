# Videos privados en RustFS

## Subir un video

1. Panel Admin → Cursos y contenidos → Editar curso → Módulos y materiales.
2. Guarda el módulo si es nuevo.
3. En Video privado en RustFS, selecciona tu MP4 y pulsa Subir video privado.
4. Espera a que termine la carga y validación; después pulsa Previsualizar video.
5. El alumno lo abre desde el aula y debe tener cuenta vigente, matrícula propia, pago aprobado y los módulos anteriores completados.

Formato: MP4 preparado para web, video H.264 y audio AAC, máximo 2 GB. RustFS almacena y entrega el archivo; esta implementación no transcodifica ni genera distintas calidades. El servidor valida tamaño, tipo y cabecera MP4; la compatibilidad de los códecs se comprueba al reproducir. No se cargaron archivos reales durante la implementación. El usuario realizará la carga de su propio video.

## Configuración

Se reutiliza ObjectStorage (Endpoint, Bucket, AccessKey, SecretKey, Region) mediante la configuración privada existente. El bucket forense-media sigue privado. PublicEndpoint es opcional: permite firmar con el dominio HTTPS público S3 si Endpoint apunta a una dirección interna. Debe ser el endpoint S3, no la consola de RustFS. Las credenciales permanecen en el backend.

Ya se añadió y comprobó CORS en el bucket para https://forense9-webforense.wv6a0u.easypanel.host y desarrollo local en http://localhost:3000, http://localhost:5000 y http://localhost:5173. Regla forensicedu-private-video: GET, HEAD y PUT; cabeceras content-type y range; se conservan las demás reglas. No es necesario cambiar el bucket a público ni crear otro.

La configuración real respondió correctamente al preflight de subida desde el frontend de producción. Si se cambia de dominio, añadir su origen a la regla CORS. Como alternativa para instalaciones que no aplican CORS de bucket, RustFS documenta RUSTFS_CORS_ALLOWED_ORIGINS; en esta instalación no fue necesario modificar esa variable ni reiniciar RustFS.

## Funcionamiento y conservación

El navegador envía el archivo directamente a RustFS con una autorización de subida de 30 minutos; los bytes no pasan por la API. Se muestra progreso y se puede cancelar. La API comprueba el archivo y lo copia a una referencia privada independiente antes de vincularlo al módulo. Reutilizar la URL de subida no permite sobrescribir el video publicado. Se detectan cambios concurrentes y las finalizaciones repetidas son idempotentes.

La reproducción usa enlaces de 15 minutos, renovados por la API mientras el reproductor está abierto; conserva la posición y el estado de reproducción. La respuesta de la API y el recurso firmado indican no-store. Un enlace sigue siendo utilizable por quien lo posea hasta su vencimiento; no es DRM ni protección contra captura de pantalla.

Quitar un video lo desvincula; los objetos publicados anteriores se conservan para recuperación administrativa. Los temporales del prefijo video-uploads/ con identificadores de la aplicación y más de 24 horas se limpian cada hora mientras la API esté ejecutándose. No se borran documentos, imágenes ni videos publicados mediante ese trabajo.

Los módulos anteriores con enlaces externos continúan funcionando. Subir un video privado reemplaza el enlace externo del módulo al terminar correctamente. Para volver a un enlace externo, quitar primero la vinculación privada.

## Puesta en marcha y pruebas

Recompilar y reiniciar API y frontend; publicar ambos para usarlo en producción. No se requieren migraciones: se utiliza CourseModule.StreamingId para la referencia privada y se conserva VideoStreamingUrl para enlaces externos.

Se verificaron 29 casos automatizados con almacenamiento en memoria: permisos, propietario, MP4 y tamaño, tokens, finalización, cancelación, enlace temporal, matrícula/pago/vigencia, bloqueo de módulos, cambios concurrentes y auditoría. Compilación, TypeScript y comprobaciones de arquitectura correctas. La interfaz se revisó en móvil. No se hizo una prueba de reproducción con un archivo real ni una prueba de concurrencia de usuarios.

    dotnet run --project tests/ForensicEdu.EmailHttpChecks -- --videos
    dotnet run --project tests/ForensicEdu.EmailHttpChecks -- --video-storage-check

El segundo comando usa los secretos/configuración de desarrollo y solo comprueba bucket y CORS; no carga videos. Para aplicar de nuevo la regla del dominio configurado: --video-storage-cors.

Documentación de CORS: https://docs.rustfs.com/en/administration/cors
