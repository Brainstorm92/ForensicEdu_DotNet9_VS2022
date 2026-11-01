# Atención de asesorías por compra

## Estado de activación

Actualización aplicada con autorización del usuario el 31/10/2026 en forense9. Se crearon las tres tablas y se prepararon 2 expedientes para las 2 compras de asesoría aprobadas, sin duplicados ni historiales faltantes. La lectura del listado y detalle con Entity Framework se verificó contra PostgreSQL real. Reiniciar el backend y el frontend con la versión actual para utilizar las pantallas.

## Flujo

1. El alumno compra una asesoría y administración aprueba el pago. En la misma transacción se crea un expediente pendiente de asignación. Cada compra tiene su propio expediente, aunque sea del mismo alumno y servicio.
2. Superadministrador o administrador académico entra en Administración → Atención de asesorías, abre la compra y selecciona un consultor con cuenta activa y vigencia disponible.
3. El consultor entra en Panel de atención → Atención de asesorías. Solo recibe los expedientes asignados a su cuenta.
4. Administración o consultor asignado registra fecha de atención, estado, resumen compartido y observaciones internas.
5. Se agregan sesiones con fecha, duración, estado, resumen compartido y observaciones internas. Pueden editarse mientras la atención siga abierta; cada cambio agrega un evento al historial.
6. El alumno revisa Mis asesorías: consultor, fecha, resumen, sesiones e historial propios. Las observaciones internas no se incluyen en sus respuestas de API. Si el consultor tiene teléfono configurado, puede contactarlo por WhatsApp.

## Reglas

- Estados: Pendiente de asignación, Consultor asignado, Programada, En atención, Completada y Cancelada.
- Programar exige consultor y fecha. Marcar una sesión como realizada exige fecha pasada y resumen.
- Completar exige al menos una sesión realizada y ninguna sesión programada pendiente. Cancelar exige motivo compartido y resolver las sesiones pendientes.
- Completar o cancelar deja el expediente en lectura. No se ofrece eliminación de expedientes, sesiones ni historial.
- La reasignación se registra en historial y retira el acceso del consultor anterior.
- Los cambios llevan versión de concurrencia: si otro usuario guarda primero, se responde 409 y debe actualizarse la vista antes de intentar otra vez.
- Las fechas se introducen y muestran en hora de Lima; se guardan en UTC.
- El historial conserva fecha, responsable y acción. Los resúmenes y observaciones se consultan en el expediente y sus sesiones; no hay versionado completo del texto de cada observación anterior.
- Este flujo no envía avisos automáticos por correo y no genera devoluciones de dinero al cancelar.

## Base de datos y actualización

Tablas nuevas: AdvisoryCases, AdvisorySessions y AdvisoryCareHistory. AdvisoryCases tiene una restricción única por OrderId y claves foráneas a la compra, el servicio, el alumno y el consultor.

El script versionado está en src/ForensicEdu.Infrastructure/Persistence/Schema/20261031_AdvisoryCare.sql. La actualización usa transacción y bloqueo para ejecutar una vez a la vez. Es idempotente: permite repetirla y prepara también las compras de asesoría ya aprobadas, sin cambiar ni eliminar usuarios, órdenes ni pagos existentes. Las compras históricas deben seguir apuntando a un servicio y alumno existentes para generar su expediente.

Después de obtener autorización, ejecutar desde la raíz del repositorio en desarrollo:

    dotnet run --project src/ForensicEdu.WebApi -p:SkipInteractiveClient=true --launch-profile http -- --upgrade-advisories

En producción, con las variables privadas ya configuradas y la versión nueva publicada:

    dotnet ForensicEdu.WebApi.dll --upgrade-advisories

Después, reiniciar el backend y el frontend. Los consultores se crean desde Gestión de usuarios con rol Consultor; el texto del especialista del catálogo no se convierte automáticamente en una cuenta.

## Estructura

- Domain/Entities/AdvisoryCase.cs: expediente, sesiones e historial.
- Application/Services/AdvisoryCareService.cs: permisos, asignación, reglas de atención y concurrencia.
- Application/Contracts/AdvisoryCare: solicitudes y respuestas; notas privadas omitidas para alumnos.
- Infrastructure/Persistence/AdvisoryCareSchema.cs: actualización de PostgreSQL.
- WebApi/Controllers/AdvisoryCareController.cs: API /api/advisory-care.
- ClientApp/src/components/advisories/AdvisoryCareBoard.tsx: bandejas y formularios de administración, consultor y alumno.

## Verificación

    dotnet run --project tests/ForensicEdu.EmailHttpChecks -p:SkipInteractiveClient=true -p:OutputPath=bin/AdvisoryCareChecks/ -- --advisory-care

31 comprobaciones con datos en memoria: creación por aprobación, una atención por compra, permisos, privacidad, fechas, sesiones, cierre, reasignación e historial. Comprobación visual de las tres vistas y adaptación a móvil. La estructura y los 2 expedientes generados también se verificaron en la base real mediante consultas de solo lectura; no se agregaron sesiones ni datos ficticios en PostgreSQL.
