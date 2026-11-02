# Administración de eventos y asesorías

Disponibles en Administración para superadministrador y administrador académico. Se usan las API existentes; no hay migraciones de base de datos en esta entrega.

## Eventos en vivo

- Crear y editar título, tipo, descripción, precio en soles, fecha, horario, ponente, biografía, enlace HTTPS de sala, imagen HTTPS y estado.
- La fecha y hora se introducen y muestran en hora de Lima (UTC−5); se envían a la API en UTC.
- Buscar por título, tipo o ponente y filtrar por estado: Programado, En vivo, Finalizado o Cancelado.
- Cancelar con confirmación conserva el registro y sus órdenes y bloquea el acceso a la sala. No envía avisos ni procesa devoluciones automáticas.
- La sala puede configurarse después de crear el evento. El acceso del alumno sigue exigiendo pago aprobado y se habilita desde 15 minutos antes del inicio.

## Asesorías

- Crear y editar título, especialista, especialidad, descripción, tarifa en soles y WhatsApp con código de país.
- Buscar por título, especialista o especialidad. El formulario normaliza espacios y separadores del teléfono.
- Eliminar exige confirmación; la API impide eliminar asesorías que tengan órdenes, conservando su historial.
- Esta pantalla administra el servicio del catálogo. La asignación por compra, fechas, sesiones, estados e historial se desarrollaron en Atención de asesorías. La actualización de PostgreSQL fue autorizada, aplicada y verificada el 31/10/2026; ver GUIA_ATENCION_ASESORIAS.md.

## Código y verificación

- src/ForensicEdu.ClientApp/src/components/admin/ServiceManagers.tsx: listados y formularios.
- src/ForensicEdu.ClientApp/src/components/AdminPanel.tsx: navegación y permisos de las pantallas.
- API: GET/POST /api/admin/events y /api/admin/advisories; GET/PUT/DELETE de sus respectivos identificadores.
- Pruebas HTTP con datos en memoria: dotnet run --project tests/ForensicEdu.EmailHttpChecks -p:SkipInteractiveClient=true -p:OutputPath=bin/CatalogChecks/ -- --catalog
- Verificados en navegador: creación y edición de ambos productos, cancelación, búsqueda sin coincidencias y formularios a 390 px sin desbordamiento horizontal. Los datos de esas pruebas no se guardaron en PostgreSQL.

## Vista del alumno: Mis eventos y congresos en vivo

Las inscripciones con orden aprobada aparecen en tarjetas de dos columnas en escritorio y una en móvil. Cada tarjeta muestra tipo, fecha de Lima, horario, descripción, expositor, código real de orden y estado del acceso. No se genera un ticket nuevo ni se afirma que la asistencia esté verificada.

El enlace de Meet o Zoom se configura en Administración → Eventos en vivo → Editar evento → Enlace de la sala virtual (HTTPS). Al pulsar Ingresar al evento se consulta /api/student/events/{id}/access y se valida que haya un enlace HTTPS válido. Solo después se abre la pestaña nueva. Si el enlace falta o no es válido, no se abre ninguna ventana y se muestra una alerta. Si el navegador bloquea una pestaña autorizada, se ofrece un enlace manual para abrirla. Se conservan el pago aprobado y la apertura 15 minutos antes del inicio; cancelados y finalizados no admiten acceso.

Código: StudentEvents.tsx y StudentDashboard.tsx. Prueba de acceso: dotnet run --project tests/ForensicEdu.EmailHttpChecks -p:SkipInteractiveClient=true -p:OutputPath=bin/EventChecks/ -- --event-access
