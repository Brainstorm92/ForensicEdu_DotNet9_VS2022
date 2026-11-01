# Nueva matrícula por recompra

Implementado el 1 de noviembre de 2026. Cada pago de recompra aprobado habilita una matrícula independiente del mismo curso. Los intentos, las calificaciones y el progreso del ciclo anterior se conservan.

## Flujo del alumno

1. Termina los dos intentos sin aprobar.
2. En Mis Cursos y Evaluaciones aparece Solicitar nueva matrícula. También puede usar Nueva matrícula desde el catálogo o desde el aula agotada.
3. Registra una operación y comprobante nuevos, con el precio vigente del curso. Registrar el pago todavía no habilita el nuevo ciclo.
4. Administración revisa y aprueba el pago desde Pagos y comprobantes.
5. Se crea una matrícula ACTIVA con otro ID, ligada a la nueva orden: progreso 0 %, módulos sin completar, intentos 0/2, sin notas, certificado ni espera heredada.
6. El ciclo anterior queda DESAPROBADA. En Mis cursos se diferencia la matrícula actual de las anteriores y se puede abrir Historial de intentos de cada una.
7. El alumno vuelve a completar los materiales y puede rendir el intento 1 y, si corresponde, el intento 2. Se mantienen la duración y las reglas de reevaluación configuradas. Un certificado nuevo queda vinculado únicamente a la matrícula que aprueba.

La aprobación crea el aviso y correo de nueva matrícula a través del centro de notificaciones y el interruptor central ya existentes. No se envía correo dentro de la transacción financiera.

## Validaciones

- La recompra exige dos intentos realmente finalizados y desaprobados. Cambiar solamente el contador no la habilita.
- No se permite recomprar con un examen abierto, intentos disponibles, curso aprobado, certificado existente o bloqueo administrativo.
- Se vuelve a validar al aprobar el pago: una orden pendiente antigua no permite abrir un ciclo adicional si ya existe una matrícula vigente.
- La misma operación bancaria pendiente conserva su orden al repetirse. Una segunda operación para el mismo curso queda bloqueada mientras se revisa la primera.
- Dos aprobaciones simultáneas para el mismo alumno y curso se serializan con un bloqueo transaccional PostgreSQL. Solo una crea el nuevo ciclo; la otra conserva su estado y devuelve el motivo de conflicto.
- Una aprobación repetida no duplica matrículas, avisos ni intentos.
- La matrícula anterior cerrada no permite iniciar exámenes ni alterar el progreso de la actual. Su historial se consulta por ID y se limita al propietario.
- El aula utiliza el ID de la matrícula seleccionada, no la primera coincidencia por curso.

## Datos y despliegue

Se reutilizan Enrollments, ExamAttempts, Orders, PaymentEvidences, AuditLogs y StudentNotifications. No se necesitan columnas, tablas ni migraciones adicionales. Se debe recompilar y reiniciar API y frontend.

No se reescriben matrículas, intentos ni pagos históricos reales. Los pagos ya aprobados antes de esta corrección no se reprocesan automáticamente; deben revisarse individualmente si se desea regularizar una compra anterior.

## Archivos principales

- Domain/Policies/EnrollmentPolicy.cs: regla de agotamiento basada en los intentos reales.
- Application/Services/CourseEnrollmentRules.cs: elegibilidad de compra por alumno y curso.
- Infrastructure/Persistence/CoursePurchaseGate.cs: bloqueo y transacción de checkout/aprobación.
- Application/Handlers/Admin/Commands/ApprovePayment/AdminApprovePaymentCommandHandler.cs: nueva matrícula, cierre de ciclos anteriores, auditoría y notificación.
- Application/Handlers/Orders/Commands/CreateOrderAndSubmitPayment/OrdersCreateOrderAndSubmitPaymentCommandHandler.cs: compra e idempotencia de la operación pendiente.
- ClientApp/src/components/EnrollmentCycles.tsx: tarjetas e historial del alumno.
- ClientApp/src/components/VirtualClassroom.tsx: aula por matrícula.

## Pruebas

    dotnet run --project tests/ForensicEdu.EmailHttpChecks -- --reenrollment

31 comprobaciones con base en memoria y correo simulado: compra, aprobación, conservación de notas, ciclo limpio, progreso, dos intentos nuevos, certificado correcto, bloqueo de recompra prematura, aislamiento de alumnos, examen abierto, bloqueo administrativo y aprobaciones simultáneas.

    dotnet run --project tests/ForensicEdu.EmailHttpChecks -- --reenrollment-lock

Comprueba dos conexiones reales a PostgreSQL usando la configuración de desarrollo y un identificador aleatorio de diagnóstico. Solo adquiere/libera bloqueos transaccionales; no crea, elimina ni escribe datos de alumnos.

También se verifican las regresiones de arquitectura, notificaciones y asesorías, la compilación y la interfaz en escritorio y móvil.
