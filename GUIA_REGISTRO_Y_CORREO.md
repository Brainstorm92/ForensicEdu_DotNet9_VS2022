# Registro, confirmación de correo y recuperación

## Funcionamiento

- Registro por contraseña: nombres, apellidos y fecha de nacimiento, junto con los datos personales y académicos existentes. La fecha es obligatoria para cuentas nuevas y no puede ser futura; no se impuso una edad mínima que no esté definida en los requisitos.
- La cuenta nueva se guarda pendiente de confirmar y no puede iniciar sesión hasta completar la confirmación.
- Con envío automático activado y configurado, se envía un enlace de confirmación al frontend. Dura 24 horas, es de un solo uso y pertenece exclusivamente a ese correo y cuenta.
- Desde el login se puede pulsar Reenviar confirmación. Se aplica espera mínima de un minuto, límite por IP y respuesta genérica para no revelar cuentas existentes.
- Con envíos desactivados, el registro continúa y queda pendiente: no se manda correo ni se permite saltar la confirmación. Al reactivar, el usuario pulsa Reenviar confirmación; no se despacha automáticamente un envío masivo de cuentas pendientes.
- Un fallo SMTP no borra la cuenta ni la confirma. Puede solicitarse otro enlace después de corregir la configuración y esperar un minuto.
- Las cuentas existentes conservan acceso; no se inventaron fechas de nacimiento ni se marcaron correos antiguos como verificados. Las cuentas creadas manualmente por administración siguen el acceso institucional existente.
- El acceso por Google continúa validando email_verified y solicita nacimiento al completar un perfil nuevo. Un correo acreditado por Google no necesita un segundo mensaje de confirmación. La vinculación de una cuenta existente mantiene la comprobación de contraseña local.
- La fecha de nacimiento se devuelve al titular en /api/auth/me y se muestra en su perfil. No se agregó a los listados públicos o de consultores.

## Configuración central

Administración → Correo Electronico sigue utilizando GmailPlatformEmail y la configuración privada existente. No se añadieron otra cuenta, otra clave ni otro proveedor.

El interruptor Activar envío automático de correos controla tanto confirmaciones como recuperación. Apagarlo bloquea los nuevos envíos de ambos tipos; los enlaces válidos que ya se enviaron siguen funcionando. La prueba manual del panel sigue permitiendo enviar un mensaje a la propia cuenta configurada aunque el envío automático esté apagado.

La URL del sitio web debe ser la del frontend. En local: http://localhost:3000. El sistema añade /#confirm-email=TOKEN o /#reset-password=TOKEN. No usar Google como URL del sitio.

La recuperación existente conserva su enlace de 30 minutos, contraseña de 12 caracteres y máximo 72 bytes, e invalidación tras el cambio. Recuperar una contraseña no confirma por sí solo un correo pendiente.

## Persistencia y activación

Actualización autorizada y aplicada el 01/11/2026 en forense9. Se verificaron los cinco campos, sus valores iniciales y la lectura desde Entity Framework. Las seis cuentas existentes conservaron acceso: ninguna pasó a requerir confirmación retroactiva y no se inventaron nacimientos ni confirmaciones. Reiniciar el backend y el frontend con la versión actual para utilizar el nuevo registro.

El script src/ForensicEdu.Infrastructure/Persistence/Schema/20261031_RegistrationEmail.sql agrega cinco campos a Users: BirthDate, EmailConfirmationRequired, EmailConfirmedAt, EmailConfirmationStamp y EmailConfirmationSentAt. Es transaccional e idempotente. Los usuarios existentes quedan con BirthDate y EmailConfirmedAt nulos y EmailConfirmationRequired=false; no se modifican sus contraseñas, roles ni vigencias.

En desarrollo:

    dotnet run --project src/ForensicEdu.WebApi -p:SkipInteractiveClient=true --launch-profile http -- --upgrade-registration

En producción, con las variables privadas y la versión nueva ya disponibles:

    dotnet ForensicEdu.WebApi.dll --upgrade-registration

Conservar el volumen App_Data/DataProtectionKeys: protege tanto enlaces de recuperación como de confirmación y la configuración SMTP guardada. No publicar claves ni credenciales en Git.

## Código

- Domain/Entities/User.cs: nacimiento y estado de confirmación.
- Application/Services/EmailConfirmationService.cs: envío, reenvío, espera entre mensajes y confirmación.
- Infrastructure/Services/EmailConfirmationTokens.cs: enlaces de 24 horas con propósito separado de recuperación.
- WebApi/Controllers/EmailConfirmationController.cs: POST /api/auth/resend-confirmation y POST /api/auth/confirm-email.
- ClientApp/src/components/AccountAccess.tsx: nacimiento, registro pendiente y detección del enlace.
- ClientApp/src/components/auth/EmailConfirmationDialog.tsx: confirmación y reenvío.
- ClientApp/src/components/admin/EmailSettings.tsx: configuración central.

## Verificación

    dotnet run --project tests/ForensicEdu.EmailHttpChecks -p:SkipInteractiveClient=true -p:OutputPath=bin/RegistrationEmailChecks/ -- --registration-email

29 comprobaciones con datos en memoria y sin correos reales: nacimiento, pendientes, confirmación, caducidad, reutilización, espera entre reenvíos, apagado/reactivación, fallo SMTP, cuentas anteriores y recuperación. También se verificaron las pruebas de Google, arquitectura y los formularios en navegador.
