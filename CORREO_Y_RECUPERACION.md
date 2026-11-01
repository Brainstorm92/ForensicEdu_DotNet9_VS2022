# Correo Electronico: confirmación y recuperación

## Uso

En el login, **¿Olvidaste tu contraseña?** solicita el correo. Si corresponde a una cuenta con contraseña, Gmail envía un enlace al sitio web configurado. El enlace dura 30 minutos y queda invalidado al cambiar la contraseña. Las cuentas que solo usan Google deben seguir entrando con Google.

El superadministrador dispone de **Administración → Correo Electronico**. Puede activar/desactivar los envíos automáticos, cambiar cuenta, nombre, clave de aplicación y URL del frontend. La clave se deja vacía para conservarla. Al cambiar la cuenta se exige una nueva clave.

**Probar conexión y enviar correo** utiliza los valores guardados y manda un mensaje a la misma cuenta de Gmail, incluso si el envío automático está desactivado. Guardar por sí solo no envía mensajes. La aceptación SMTP no garantiza entrega en bandeja principal: revisar también spam.

## Configuración inicial privada

En desarrollo se carga desde los secretos de usuario de WebApi o desde appsettings.Local.json, archivo privado excluido de Git y de la publicación. En producción se usan variables privadas de Easypanel/OpenBao. Tras cambiar la configuración local, detener la depuración y volver a ejecutar con F5:

- Email__Enabled: true o false.
- Email__Address: cuenta Gmail completa.
- Email__Password: clave de aplicación de Google.
- Email__SenderName: nombre del remitente.
- Email__FrontendUrl: URL HTTPS pública del frontend, sin /api, parámetros ni fragmentos. En desarrollo se admite http://localhost:3000; si se sirve la SPA desde WebApi, usar http://localhost:5000/app.

Las actualizaciones guardadas en el panel prevalecen sobre estos valores iniciales. Se almacenan cifradas en SystemSocialSettings (clave PlatformEmail.Protected.v1), excluidas del endpoint público de redes sociales. La configuración SMTP conserva la tabla existente; la confirmación de cuentas requiere aplicar --upgrade-registration, descrito en GUIA_REGISTRO_Y_CORREO.md. Solo el rol 1 puede consultar o modificar correo; nunca se devuelve la clave, solo hasPassword.

Gmail utiliza smtp.gmail.com:587 y STARTTLS con validación de certificados. El servidor necesita salida TCP 587. No introducir estas credenciales en React, archivos públicos ni Git.

## Persistencia en Contabo / Easypanel

Montar un volumen persistente en **/app/App_Data**, conservando las claves de **DataProtectionKeys**. Sin estas claves se pierden los enlaces activos y la capacidad de descifrar la configuración guardada. Respaldar este volumen junto con PostgreSQL y limitar su acceso al usuario de la aplicación. Las claves del volumen requieren protección del disco y de las copias; guardar ambos elementos juntos con acceso público anularía esa protección.

El sitio ya desplegado necesita publicar estos cambios tanto en backend como en frontend y configurar su URL pública real. Los secretos locales no se transfieren a Easypanel ni a GitHub.

## Flujo y seguridad

- React: AccountAccess → PasswordRecoveryDialog → /api/auth/forgot-password y /api/auth/reset-password.
- Application: PasswordRecoveryService valida usuario y contraseña; IPlatformEmail e IPasswordRecoveryTokens abstraen los adaptadores.
- Infrastructure: GmailPlatformEmail protege configuración y envía SMTP; PasswordRecoveryTokens protege enlaces de 30 minutos.
- WebApi: EmailSettingsController restringe configuración al superadministrador; PasswordRecoveryController recibe solicitudes públicas con límite por IP.
- El token va en el fragmento URL y el frontend lo quita de la barra al abrirlo; no se envía como parte de la petición inicial al servidor web.
- El hash anterior actúa como versión de concurrencia: al guardar una nueva contraseña se invalidan todos sus enlaces anteriores, incluso ante solicitudes simultáneas. No cambia el estado ni la vigencia académica de la cuenta.
- No se revocan JWT ya emitidos; conservan la política de expiración existente del proyecto.
- La respuesta de solicitud no revela si existe una cuenta. Los fallos SMTP se registran por tipo sin incluir credenciales ni tokens. Ante indisponibilidad no se garantiza entrega ni se reintenta en segundo plano: solicitar otro enlace cuando vuelva el servicio.
- La confirmación de correo del registro y el reenvío ya están implementados en el mismo servicio. Ver GUIA_REGISTRO_Y_CORREO.md para las reglas y su activación.

## Verificación

dotnet run --project tests/ForensicEdu.EmailChecks

dotnet run --project tests/ForensicEdu.EmailHttpChecks -p:SkipInteractiveClient=true -p:OutputPath=bin/EmailChecks/

Prueba SMTP real opcional, enviada a la propia cuenta configurada en secretos locales:

dotnet run --project tests/ForensicEdu.EmailChecks -- --smtp-check
