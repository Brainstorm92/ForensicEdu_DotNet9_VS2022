# Antifraude y pagos simultáneos

Actualizado: 2 de noviembre de 2026. Implementación local; requiere desplegar la WebAPI y el frontend para aplicarse en producción.

## Qué se protege

Checkout, aprobación, cambio administrativo de estado y reemplazo del voucher adquieren el mismo bloqueo transaccional de PostgreSQL. El bloqueo funciona entre usuarios, productos y réplicas de la API. Cada solicitud consulta el estado posterior al commit anterior antes de decidir. La aprobación y sus efectos (orden, matrícula, vigencia, aviso y auditoría) se guardan dentro de esa transacción.

Se mantiene el bloqueo por alumno/curso dentro de la misma transacción, siempre después del bloqueo de pagos. Commit, rollback o cancelación de la conexión liberan los bloqueos de PostgreSQL. Las pruebas en memoria usan semáforos y no sustituyen al bloqueo distribuido de producción.

La operación se compara ignorando espacios ASCII, guiones y barras, conservando ceros iniciales. Se compara el banco sin espacios exteriores y sin distinguir mayúsculas. El hash del voucher tampoco distingue mayúsculas. No se interpretan equivalencias entre nombres distintos de bancos.

## Duplicados exactos

Banco y operación ya aprobados, huella bancaria aprobada o archivo idéntico ya aprobado bloquean otra aprobación. El nuevo pago se marca DUPLICADO al detectar el conflicto durante la aprobación. Una revisión manual no permite omitir ese bloqueo. Reemplazar un voucher aprobado o duplicado tampoco está permitido.

## Coincidencias que requieren revisión

- Misma operación o comprobante en otro pago pendiente.
- Mismo número de operación (al menos cuatro caracteres) registrado en otro banco.
- Mismo banco, día, monto y titular, con una operación distinta.

Estas señales no prueban fraude: pueden ser pagos legítimos. Se asigna EN_REVISION sin habilitar el producto. Las consultas consideran pagos aprobados, pendientes, observados y en revisión; excluyen rechazados y duplicados.

## Revisión administrativa

En Panel Admin → Pagos y comprobantes, pulsa Revisar coincidencias. Se muestran las órdenes coincidentes y sus motivos, con acceso autorizado a sus comprobantes. Verifica la transferencia, escribe una justificación de al menos diez caracteres y marca la confirmación. Aprobar tras revisión envía una huella de la evidencia examinada.

Si cambia el pago, sus comprobantes o las coincidencias, la revisión anterior deja de servir y debe actualizarse. El servidor vuelve a comprobar todo bajo bloqueo. Auditoría registra PAYMENT_PARTIAL_MATCH, PAYMENT_PARTIAL_MATCH_ACCEPTED o PAYMENT_DUPLICATE, con responsable y motivo. El alumno no puede consultar las coincidencias de otros usuarios.

## Despliegue y límites

No hay cambios de esquema ni migración de datos. Despliega todas las instancias de la WebAPI con esta versión; una instancia anterior no utiliza este bloqueo. El control cubre las rutas de escritura de la aplicación: importaciones o escrituras SQL externas deben respetar la misma exclusión. Los datos duplicados que ya existan no se eliminan ni se aprueban automáticamente.

Se serializan las mutaciones financieras para priorizar integridad en el volumen inicial del proyecto. No se serializan las lecturas ni la reproducción de contenido. Si crece el volumen, podrá sustituirse por reservas e índices exclusivos por identificador, manteniendo las mismas reglas.

## Verificación realizada

- 19 comprobaciones HTTP aisladas: carreras de checkout/aprobación, usuarios y productos distintos, comprobantes repetidos, normalización, revisión manual, justificación, permisos y revisiones obsoletas.
- 31 comprobaciones de nueva matrícula, 32 de notificaciones y 32 de evaluaciones sin regresiones.
- PostgreSQL real: dos conexiones independientes, bloqueo global anidado con bloqueo de curso, liberación tras commit y rollback, y ejecución de la consulta SQL de coincidencias. Estas pruebas no escriben tablas ni alteran esquemas.
- Los casos HTTP financieros usan base en memoria y correo simulado; no se realizaron compras reales ni se aprobaron pagos de producción.

Comandos: EmailHttpChecks --antifraud y --antifraud-lock. El segundo usa la configuración local/secretos del backend sin mostrarlos.
