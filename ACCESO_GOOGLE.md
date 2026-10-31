# Inicio de sesión, registro y Google

La interfaz de acceso incluye panel de identidad, registro en tres pasos, confirmación de contraseña, mostrar/ocultar contraseña, validación junto al campo y modos claro/nocturno. El token de sesión continúa solo en memoria; después de recargar se debe iniciar sesión nuevamente.

Se reutilizó el Client ID de PSYCO_APP en appsettings.Local.json, excluido de Git. Google Identity Services usa el botón oficial con popup y callback JavaScript. No necesita Client Secret, API key, acceso al buzón ni un endpoint /signin-google.

## Configurar Google Cloud

En el mismo cliente OAuth web de PSYCO_APP, conservar sus orígenes y añadir en Orígenes autorizados de JavaScript:

```text
http://localhost
http://localhost:3000
http://localhost:5000
```

Si se usa https://localhost:7001, añadirlo también. En producción añadir el dominio HTTPS real desde el que se abre la aplicación, sin rutas. Estas opciones pertenecen a Google Cloud y no se modifican al copiar el Client ID. Si la audiencia exige usuarios de prueba, agregar las cuentas autorizadas. El Client ID compartido conserva la marca/pantalla de consentimiento de ese proyecto.

En Easypanel, definir solo en el backend:

```env
Authentication__Google__Enabled=true
Authentication__Google__ClientId=<mismo Client ID web de PSYCO_APP>
```

El frontend consulta el Client ID público mediante GET /api/auth/google/config. No se copia ningún secreto al código React. El despliegue recomendado mantiene /api y la SPA bajo el mismo origen; en desarrollo Vite utiliza su proxy. El challenge requiere una cookie HttpOnly, SameSite=Lax y Secure en producción; configurar correctamente HTTPS. Si se configura COOP, permitir los popups de autenticación.

## Flujo

- Nuevo alumno: Google verifica identidad; la app solicita documento, teléfono, datos académicos y aceptación de términos/privacidad antes de crear el usuario. No se inventan datos ni contraseñas. Siempre se crea un estudiante con la misma vigencia inicial de 30 días y sin compras.
- Cuenta local existente con ese correo: se exige confirmar su contraseña para vincular Google. La coincidencia de correo no vincula una cuenta automáticamente. Se conserva ID, rol, matrículas, historial y contraseña.
- Cuenta vinculada: se busca por el sub inmutable de Google, no por el correo. Se verifican estado y vigencia; no se reactiva ni renueva por iniciar sesión con Google.
- No se implementó recuperación por correo ni desvinculación de Google. Las cuentas creadas solo con Google no tienen contraseña local.

La validación criptográfica usa metadatos y claves HTTPS oficiales de Google. Comprueba firma RS256, emisor, audiencia, expiración, correo verificado, sub y nonce de la sesión. El nonce evita reutilizar una identidad obtenida desde otra sesión o aplicación que comparta el Client ID. Las claves públicas se actualizan ante rotación. No se aceptan tokens simulados en producción.

El nonce está vinculado a una cookie temporal de 10 minutos, no se almacena la credencial Google en localStorage y se elimina al emitir la sesión local. El token Google se mantiene en memoria únicamente mientras se completa el perfil o la vinculación.

## Esquema y pruebas

Aplicar database-upgrade-google.sql antes de iniciar esta versión o ejecutar --upgrade-google: agrega Users.GoogleSubject y un índice único sin vincular ni modificar cuentas existentes. El esquema inicial incluye este cambio.

- tests/ForensicEdu.GoogleAuthChecks: claves RSA sintéticas, firma, emisor, audiencia, expiración, nonce, correo, alta incompleta, perfil, consentimiento, vinculación explícita, rol, suspensión y vigencia. No autentica una cuenta Google real.
- tests/Test-PlatformEndpoints.mjs: configuración pública, nonce HttpOnly/no-store y rechazo HTTP de sesión o token Google inválidos, además de la regresión de endpoints existentes.
- tests/Test-ConnectedUi.mjs: registro por pasos, acceso, creación de cursos, pagos, aula, examen, certificado y vista móvil. GOOGLE_TEST_USE_REAL_CLIENT=1 utiliza el Client ID local para comprobar que carga el botón oficial; no automatiza una cuenta personal ni el consentimiento.

La selección y autorización de una cuenta Google real requiere la interacción del titular y que Google Cloud tenga los orígenes anteriores. No se considera esa prueba completada por verificar solo el botón o las claves sintéticas.

Fuentes: https://developers.google.com/identity/gsi/web/guides/get-google-api-clientid y https://developers.google.com/identity/gsi/web/guides/verify-google-id-token.
