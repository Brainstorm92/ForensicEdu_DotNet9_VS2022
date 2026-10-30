# Imágenes en RustFS

En Panel Admin → Cursos y contenidos → Información, seleccionar la portada y la foto del docente. Cada imagen se sube al seleccionar el archivo y se muestra su vista previa; Guardar curso persiste la asociación. Quitar imagen retira esa asociación al guardar. La foto del docente pertenece a ese curso; no se ha creado una entidad de profesores independiente.

Se aceptan JPG, PNG y WebP de hasta 5 MB. El servidor verifica el contenido real, rechaza archivos corruptos y limita la entrada a 24 megapíxeles y 12000 píxeles por lado. SkiaSharp decodifica la imagen, aplica orientación y vuelve a codificarla a WebP con máximo 1920 píxeles por lado, sin copiar los metadatos originales. No se aceptan SVG, PDF ni videos en este endpoint.

## Acceso y persistencia

- POST `/api/admin/media/images`: multipart con campo `file`, exclusivo de superadministrador y administrador académico. Respuesta con id, url, contentType y size únicamente después de completar la escritura en RustFS.
- GET `/api/media/images/{id}`: lectura pública de imágenes del catálogo mediante la aplicación, con caché de un día y nosniff. Solo admite IDs UUID sin guiones y accede al prefijo `images/`. No permite leer comprobantes ni rutas arbitrarias.
- RustFS conserva el bucket privado. El backend usa S3 path-style y los objetos `images/{id}.webp`; no convierte el bucket en público ni entrega credenciales al navegador.
- PostgreSQL guarda referencias estables en Courses.ImageUrl y Courses.InstructorImageUrl. Estas referencias no contienen firmas temporales. Las claves S3 quedan en configuración privada del backend.
- La lectura de imágenes públicas del catálogo no sirve para documentos restringidos de alumnos. Los materiales requieren autorización de matrícula y un endpoint propio de descarga o enlaces firmados de corta duración.
- No se borran objetos al reemplazar o quitar imágenes: podrían tener otras referencias. Las subidas de formularios abandonados también se conservan. La limpieza de objetos sin referencias es una tarea pendiente; no hay un recolector automático en este MVP.

## Configuración

En la aplicación backend de Easypanel (no en el frontend), definir:

```env
ObjectStorage__Endpoint=https://forense9-rustfs-s3.wv6a0u.easypanel.host
ObjectStorage__Bucket=forense-media
ObjectStorage__Region=us-east-1
ObjectStorage__AccessKey=<clave de acceso>
ObjectStorage__SecretKey=<clave secreta>
```

En desarrollo se usa appsettings.Local.json, excluido de Git y de la publicación. appsettings.json solo contiene valores vacíos y nombres de opciones. No copiar claves al código React ni a variables VITE_. El endpoint es el dominio que apunta a 9000; 9001 es únicamente la consola. La cuenta de aplicación debe limitarse al bucket y prefijos necesarios.

Para una base existente, aplicar `database-upgrade-course-images.sql` antes de arrancar esta versión, o ejecutar la API con `--upgrade-media`. Este comando agrega solamente la columna InstructorImageUrl si falta; no modifica cursos existentes. El esquema inicial ya incluye el cambio.

Los fallos de almacenamiento devuelven un error y no confirman la subida. No se reintenta una escritura S3 automáticamente. Se deben conservar un volumen persistente en RustFS y copias de seguridad.

## Videos y documentos

Los documentos PDF y Word (.docx) ya tienen subida privada y visor autorizado por matrícula, separados de los endpoints públicos de imágenes. Ver DOCUMENTOS_PRIVADOS.md. Los videos siguen usando enlaces HTTPS/YouTube.

Los módulos admiten enlaces HTTPS y YouTube. El aula convierte enlaces watch/youtu.be/shorts/embed de YouTube en reproductor integrado; los archivos directos usan el reproductor HTML5. El acceso a la URL del módulo sigue validando la matrícula. YouTube no listado permite ver y compartir el video a cualquiera que conozca su enlace: no es protección por alumno. Guardar videos en RustFS no añade por sí solo transcodificación, HLS, calidades adaptativas ni CDN.

## Verificación

`tests/ForensicEdu.ArchitectureChecks` verifica validación, normalización WebP, dimensiones, IDs, referencias estables y fallos de almacenamiento con un adaptador en memoria.

El recorrido `tests/Test-ConnectedUi.mjs` incluye la prueba real al definir `MEDIA_TEST_USE_REAL_STORAGE=1`: lee la configuración privada solo para el proceso backend, sube portada y foto desde Chrome, comprueba las referencias mediante SQL y recupera las imágenes por HTTP. Verifica además que el bucket rechace lectura anónima directa y que se rechacen archivos falsos, tamaños excesivos y usuarios no autorizados. La BD es PostgreSQL local aislada; las imágenes de prueba sí se conservan en el RustFS configurado. Sin esa opción, el recorrido general no necesita almacenamiento externo.
