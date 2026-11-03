# Evaluaciones de cursos

## Administración
En Cursos y contenidos → Editar curso → Banco de preguntas se elige respuesta única o varias respuestas correctas. Se pueden configurar entre 2 y 8 opciones y debe existir al menos una correcta. Una pregunta múltiple solo suma cuando el alumno marca exactamente todas las correctas, sin opciones incorrectas; no hay puntaje parcial. Cada pregunta tiene el mismo peso y la nota se escala de 0 a 20.

Los bancos anteriores de respuesta única siguen funcionando. Al iniciar se conserva una copia del banco: las ediciones posteriores no alteran un examen ya iniciado. Las soluciones y explicaciones nunca se incluyen en la sesión del alumno. No se requiere migración: el banco, las respuestas y la copia del intento usan las columnas JSON existentes.

## Alumno
Antes de iniciar se muestra una confirmación. Solo confirmar crea o retoma el intento; volver al repaso no lo consume. La evaluación ocupa toda la ventana, permite navegar por preguntas y muestra el reloj. No bloquea las funciones del sistema operativo ni promete impedir capturas de pantalla.

El servidor fija diez minutos desde el inicio; cambiar el reloj del equipo, recargar, salir o abrir otra pestaña no amplía ese plazo. Se conserva el acceso al recargar en la misma pestaña mediante una sesión de autenticación en sessionStorage, eliminada al cerrar sesión o vencer el token. Al cerrar la pestaña puede ser necesario iniciar sesión otra vez: el intento sigue disponible en el servidor mientras no termine.

Las respuestas se guardan automáticamente. Se mantiene un borrador en sessionStorage por alumno e intento para recuperar selecciones pendientes al recargar la misma pestaña. Si otra pestaña guarda una versión distinta, se avisa y se exige recuperar la versión del servidor antes de continuar para evitar sobrescrituras silenciosas.

## Tiempo, conexión y envío
Al vencer el plazo se califican únicamente las respuestas recibidas por el servidor a tiempo. Los cambios hechos sin conexión después del vencimiento no cuentan. El reloj sigue corriendo durante la desconexión. Se avisa de respuestas pendientes y se reintenta al recuperar la conexión; el proceso de servidor también cierra intentos vencidos aunque el alumno abandone la web. La API debe estar ejecutándose para procesarlos.

Antes de enviar manualmente se pide confirmación y se muestra la cantidad sin responder. Una vez enviado no se puede modificar. El identificador del intento hace idempotentes los envíos repetidos y la recuperación de una respuesta de red perdida: no crea otro intento ni otro certificado. El bloqueo en PostgreSQL coordina inicio, guardado y envío entre peticiones. Se mantiene el máximo de dos intentos, la espera configurada para el segundo, aprobación desde 13/20 y bloqueo después de aprobar.

Se añadieron comprobaciones HTTP aisladas (--exam-scenarios) para permisos, selección múltiple, errores de opciones, recarga, tiempo, copia del banco, concurrencia, resultados y certificados. No se usan datos académicos reales para las pruebas.
