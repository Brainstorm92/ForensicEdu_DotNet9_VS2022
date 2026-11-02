# Revisión funcional del proyecto frente al Word v5 y al PDF

> Actualización 01/11/2026: implementada la nueva matrícula por recompra después de dos intentos agotados, con ciclos independientes e historial conservado. Ver GUIA_NUEVA_MATRICULA.md. El diagnóstico original de ese pendiente se conserva como referencia.

> Registro y correo: nacimiento obligatorio para registros nuevos, confirmación/reenvío y recuperación centralizados en el interruptor SMTP. Código probado y actualización --upgrade-registration autorizada, aplicada y verificada en PostgreSQL el 01/11/2026. Las seis cuentas existentes conservaron acceso, sin nacimientos ni confirmaciones inventados. Ver GUIA_REGISTRO_Y_CORREO.md.

> Seguimiento de asesorías implementado con asignación por compra, fechas, estados, sesiones e historial. Actualización de PostgreSQL autorizada, aplicada y verificada el 31/10/2026: 2 compras aprobadas, 2 expedientes con historial, sin duplicados. Ver GUIA_ATENCION_ASESORIAS.md.


> Actualización: pantallas de eventos y asesorías implementadas el 31/10/2026. Incluyen listados, búsqueda, creación y edición, cancelación de eventos y eliminación de asesorías sin órdenes. Ver GUIA_EVENTOS_ASESORIAS.md. Las referencias posteriores a estas pantallas como pendientes corresponden al diagnóstico original. El seguimiento de atenciones, asignaciones, notificaciones y reportes sigue pendiente.

Fecha: 31 de octubre de 2026.
Repositorio revisado: C:/Users/Usuario/source/repos/ForensicEdu_DotNet9_VS2022.
Rama: RamaEdu3010. Commit base: a8315e6, actualizado desde develop. Se revisó también el contenido local disponible; los ajustes locales existentes no se modificaron.

## Conclusión

El proyecto ya tiene una integración real entre React, API y persistencia; no corresponde seguir describiéndolo como una demostración completa en localStorage. Existen implementaciones de autenticación, catálogo, órdenes, vouchers privados, aprobación financiera, matrículas, exámenes con tiempo de servidor, certificados PDF, consulta pública, tickets y administración de cursos. Sin embargo, todavía no cumple íntegramente los recorridos del Word y del PDF.

Los pendientes de mayor impacto son cerrar el QR hacia una página pública real, proteger la distribución de video, garantizar la exclusión de pagos duplicados bajo concurrencia, completar recuperación y confirmación de correo, implementar seguimiento de asesorías y completar los procesos administrativos que hoy solo tienen API.

Este informe sustituye, para el estado actual, las afirmaciones de REVISION_REQUISITOS.md que aún describen pantallas demo o funciones que ya se implementaron. No certifica el funcionamiento de todo lo desplegado en Contabo.

## Documentos leídos

- Flujo_Procesos_Plataforma_Educativa_Francisco_v5 (1).docx: 457 párrafos extraídos, incluidas tablas. Secciones 1 a 25.
- Flujo_Procesos_Plataforma_Educativa_EDITABLE_Francisco (1).pdf: cuatro páginas renderizadas e inspeccionadas visualmente. Se contrastaron el recorrido general, las reglas académicas, los eventos, las asesorías y la vigencia.
- No se recibió PPT. En la conversación se aclaró que el segundo documento era PDF.
- Archivos originales: D:/Documentos/MODULO CURSOS/Archivos.

La fecha de nacimiento aparece en el flujo maestro del Word, aunque no se repite en su lista de registro 4.1. La confirmación del correo aparece explícitamente en las páginas 1 y 2 del PDF. Se registran ambas diferencias para que no desaparezcan de la comparación.

## Estados utilizados

- Implementado en código: existe un recorrido o una implementación identificable; no equivale a validación integral de producción.
- Parcial: existe una parte, pero falta una función exigida, una pantalla o una garantía de negocio.
- Desviación: el diseño actual hace algo distinto de lo solicitado originalmente.
- No verificado en despliegue: requiere acceso a infraestructura, datos o pruebas que no se realizaron en esta revisión.

## Matriz de requisitos y pendientes

| Área y referencia original | Estado actual | Qué queda pendiente |
|---|---|---|
| Cuenta única y perfil, Word 3 y 4 | Parcial. Registro, BCrypt, JWT, Google y edición del perfil existen. | Fecha de nacimiento; confirmación de correo para registro con contraseña; recuperación de contraseña por correo. Reforzar unicidad del documento frente a registros simultáneos: DNI tiene índice, pero no único. |
| Vigencia, Word 4.3 y PDF 4C | Parcial. Primer pago y compras posteriores renuevan dos años desde aprobación; se marca pendiente de eliminación tras 30 días sin compra. | Política ejecutable de eliminación/anonimización de la cuenta de acceso conservando historial. Avisos previos y estados de vencimiento coherentes; actualmente el vencimiento de dos años se convierte en SUSPENDIDO. Los avisos 7/3/1 días son una recomendación del Word, no un plazo obligatorio cerrado. |
| Catálogo, Word 5 | Implementado para cursos; parcial para toda la oferta. Hay CRUD, publicación, requisitos, imágenes, módulos y precios. | Comprobar que la oferta real cargada incluya las ocho especialidades y las fichas requeridas. Completar la gestión visual de eventos y asesorías; no confundir catálogo vacío con falta de endpoint. |
| Asesorías, introducción y Word 6, 18; PDF 4B | Parcial. Se venden y se habilita WhatsApp con pago aprobado. | Registrar atención por compra: consultor asignado, fecha, estado, observaciones, sesiones e historial y cierre del servicio. El especialista actual es texto del producto, no una asignación de usuario consultor. Agenda y documentos de apoyo son opcionales en el Word. |
| Orden previa al pago, Word 7 y 22 | Desviación. El checkout crea orden y evidencia bancaria juntos. | Permitir generar orden antes de pagar, conservar el precio y después registrar operación/voucher. Distinguir PENDIENTE_DE_PAGO de PAGO_REGISTRADO: actualmente los alias tienen el mismo valor 1. |
| Pago y corrección, Word 8 y 23 | Parcial. JPG/PNG/PDF, hash, subida privada, revisión, observación/rechazo y reemplazo de voucher existen. | Corrección de banco, operación, fecha y titular de una evidencia observada; el flujo actual permite reemplazar el archivo, pero no editar esos datos. Definir reenvío y transición de un pago rechazado/observado a una nueva revisión. |
| Duplicidad, Word 9 y aceptación 24 | Parcial. Se verifican banco+operación, huella y hash contra aprobados; aprobación, matrícula y renovación se guardan juntas. | Protección de concurrencia entre aprobaciones/checkout simultáneos mediante restricción adecuada o bloqueo transaccional y pruebas PostgreSQL. Los índices de huella y hash no son únicos. La transacción de SaveChanges no hace indivisible la comprobación previa. Falta una revisión explícita de coincidencias dudosas/parciales. |
| Matrícula, Word 10 y 12.3 | Parcial. La aprobación habilita matrícula y aula. | Recorrido de nueva matrícula o autorización administrativa tras dos desaprobaciones. La aprobación busca cualquier matrícula del mismo usuario y curso y no crea otra si ya existe; pagar de nuevo por sí solo no restablece intentos. Correo de matrícula pendiente. |
| Documentos, Word 2, 11.1; PDF 3 | Desviación respecto del original. Visor privado PDF/DOCX, marca personal y auditoría de lectura implementados. | El original permite documentos descargables y pide registro de descarga. La UI actual solo ofrece lectura, bloquea imprimir/guardar y audita PRIVATE_DOCUMENT_VIEWED. Resolver el cambio de alcance o implementar descarga autorizada. PPTX/XLSX y otros formatos mencionados no tienen soporte actual; debe definirse cuáles se autorizarán. |
| Video, Word 11.2 y aceptación 24; PDF 3 | Parcial. El endpoint exige matrícula/pago y el reproductor oculta el botón de descarga. | Proveedor de streaming protegido, HLS o equivalente, enlaces firmados/temporales y control por sesión. Se entrega VideoStreamingUrl sin firmarla ni transformarla; también se aceptan embeds de YouTube o video src. Ocultar un botón no cumple la garantía de no exponer una URL reutilizable. La protección absoluta contra capturas no se exige ni puede garantizarse desde una página. |
| Evaluación, Word 12 y PDF 3 | Implementado en sus reglas centrales. 600 segundos en servidor, persistencia/resume, auto cierre, umbral 13, máximo dos intentos y reemplazo de nota. | Se baraja todo el banco: no hay selección configurable de un subconjunto. El segundo intento puede repetir las mismas preguntas en otro orden, permitido por el texto de re-evaluación. La espera configurable existe en backend; falta control en panel web. Acreditar concurrencia y recuperación en PostgreSQL/navegador, además de las pruebas en memoria. |
| Certificado y QR, Word 13 y PDF 3 | Parcial. PDF y QR generados, verificación por API y revocación existentes. | QrValidationUrl sigue fija en https://forensic-edu.org/verificar/{codigo}. React no interpreta esa ruta al abrirla ni extrae el código de la URL. Conectar dominio real, ruta pública y búsqueda automática del certificado. La verificación manual dentro de la aplicación sí existe. La app móvil genera un QR hacia el endpoint JSON de la API: conviene unificarlo con la página pública. |
| Eventos, Word 14 y 23; PDF 4A | Parcial. Compra, pago, listado y acceso con apertura 15 minutos antes existen. | Cierre por hora final/periodo configurable: hoy depende de cambiar FINALIZADO manualmente. Integración privada con proveedor de sala; se devuelve RoomUrl estática. Cancelación solo cambia estado: faltan aviso a inscritos y devolución o reprogramación. Gestión visual de eventos pendiente. |
| Redes sociales, Word 15.1 | Implementado en código. Configuración por API/panel, enlaces dinámicos, vacío desactiva. | Verificar en la configuración real las URLs institucionales y su presencia en las superficies solicitadas. No es un pendiente general de integración HTTP. |
| Tickets y soporte, Word 15.2 | Parcial. Crear, responder, cambiar estado y consultar desde alumno/soporte existen. | Notificaciones automáticas de correo y su trazabilidad. Los adjuntos no se marcan obligatorios: el documento pide formulario y tickets, pero no los exige explícitamente. |
| Panel del alumno, Word 16 | Parcial. Cursos, eventos, pagos, certificados, perfil y consultas consultan API. | Centro de notificaciones; historial de asesorías completo; correcciones de pagos y flujo de recuperación de cuenta. |
| Administración, Word 17 | Parcial. Usuarios/roles, pagos, cursos/módulos, materiales, preguntas, notas, revocación, tickets, redes y auditoría tienen pantallas. | Pantallas de eventos y asesorías, reportes/estadísticas y detalle de intentos, más ajuste de espera de re-evaluación. Varias operaciones ya existen como endpoints; no deben contarse como backend inexistente. El reporte actual de API es un resumen de contadores, no una pantalla ni un conjunto de informes. |
| Docente y consultor, Word 18 | Parcial. Existen los siete valores de rol y controles para administradores/alumno/soporte. | Asignaciones de cursos al docente, edición y seguimiento autorizado de sus contenidos/preguntas; bandeja de asesorías asignadas al consultor y registro de atención. roles.ts da permisos vacíos a docente y consultor; gestión general de catálogo solo admite roles 1 y 2. |
| Datos, Word 19 | Parcial. Entidades y relaciones cubren una parte importante; se usan también JSON y órdenes para derivar listados. | Persistencia de atenciones de asesoría y notificaciones; migraciones/evolución versionada. La lista de tablas del documento es sugerida: usar JSON u órdenes no es incumplimiento por sí mismo si conserva reglas e historial. |
| Auditoría, Word 21 | Parcial. Pagos, matrículas, documentos, exámenes, certificados y cambios administrativos registran acciones. | Login y logout auditados. AuthLogin devuelve token sin añadir auditoría; logout de React solo limpia el estado en memoria. Cambiar contraseña no revoca los JWT emitidos, que debe tratarse como decisión técnica adicional de sesiones. |
| Seguridad y backups, Word 21 | Implementación parcial y operación no verificada. Hay hash de contraseña, JWT, permisos, HTTPS en dominios y almacenamiento privado previsto. | No hay evidencia de trabajos de backup/restauración en el repositorio; verificar el servidor antes de afirmar que existen o faltan. Comprobar recuperación de PostgreSQL y documentos y configuración real de buckets/proveedor. Rate limiting es una mejora recomendada, no un requisito literal del documento. |
| Excepciones y aceptación, Word 23 y 24 | Parcial. Varios controles tienen pruebas automatizadas. | Pruebas de aprobaciones simultáneas, corrección/rechazo, nueva matrícula tras desaprobar, QR escaneado desde otro navegador, cancelación/devolución, streaming, correos y ciclo de eliminación. No se ejecutaron recorridos con alumnos ni escrituras en producción durante esta revisión. |
| Diagrama, Word 25 | Revisado el PDF de referencia. | Mantener el diagrama final alineado con las decisiones sobre descarga, orden previa al pago, asesorías y canales de notificación. |

## Evidencias de código para los pendientes principales

Las rutas siguientes son relativas a la raíz del repositorio auditado.

1. Cuenta: src/ForensicEdu.Application/Contracts/Auth/RegisterRequest.cs y src/ForensicEdu.Domain/Entities/User.cs no contienen fecha de nacimiento. AccountManagementController.cs ofrece editar perfil y cambiar contraseña autenticado; no ofrece recuperación por correo. AuthRegisterCommandHandler.cs registra al usuario directamente sin confirmación por correo.
2. Vigencia: src/ForensicEdu.Application/Services/AccountLifecycleService.cs, ProcessDailyAccountExpirationsAsync, marca pendientes y suspende vencidos, pero no elimina/anonimiza ni envía avisos. UserLifecycle.cs sí renueva desde la aprobación.
3. Video: src/ForensicEdu.Application/Handlers/Management/StudentWorkspace/Queries/GetModuleVideo/GetModuleVideoQueryHandler.cs devuelve module.VideoStreamingUrl e indica que los enlaces temporales dependen del proveedor. src/ForensicEdu.ClientApp/src/components/ModuleVideo.tsx reproduce iframe o video src.
4. QR: src/ForensicEdu.Application/Services/EvaluationService.cs:150 establece el dominio fijo. src/ForensicEdu.ClientApp/src/components/PublicQRVerifier.tsx:6-8 usa estado React, sin leer ruta/código de la URL. src/ForensicEdu.ClientApp/src/App.tsx selecciona pantallas por activeTab, no por ruta de verificación.
5. Duplicidad: src/ForensicEdu.Application/Handlers/Admin/Commands/ApprovePayment/AdminApprovePaymentCommandHandler.cs:29 realiza la consulta antes del guardado; ApplicationDbContext.cs:78-79 crea índices no únicos. Existe un guardado atómico conjunto, pero falta exclusión entre transacciones concurrentes. ApplicationDbContext.cs:46 tampoco marca DNI como único.
6. Recompra: AdminApprovePaymentCommandHandler.cs:53 busca matrícula por UserId+CourseId sin distinguir una nueva compra ni un ciclo de dos intentos agotado.
7. Orden: OrdersCreateOrderAndSubmitPaymentCommandHandler.cs exige operación y titular antes de crear la orden; src/ForensicEdu.Domain/Enums/OrderStatus.cs asigna 1 tanto a PendienteDePago como a PagoRegistrado.
8. Corrección de pago: OrdersUploadVoucherCommandHandler.cs reemplaza fichero/hash pero no cambia datos bancarios ni reinicia estado; no se encontró endpoint de edición de esos datos por alumno. ReviewPaymentCommandHandler.cs permite revisión administrativa.
9. Materiales: ProtectedDocumentViewer.tsx ofrece lectura y bloquea Ctrl+P/Ctrl+S; PrivateDocuments.cs limita claves a PDF/DOCX. GetProtectedDocumentQueryHandler.cs audita PRIVATE_DOCUMENT_VIEWED. DOCUMENTOS_PRIVADOS.md confirma la decisión deliberada de no ofrecer descarga.
10. Asesorías: AdvisoryService.cs solo guarda datos del servicio y el especialista como texto. StudentServicesAdvisoriesQueryHandler.cs devuelve servicios adquiridos derivados de órdenes aprobadas; no hay una entidad de atención por compra.
11. Eventos: StudentServicesEventAccessQueryHandler.cs bloquea 15 minutos antes y estados CANCELADO/FINALIZADO, pero no evalúa hora de término. CancelEventCommandHandler.cs solo actualiza estado y auditoría.
12. Panel y roles: AdminPanel.tsx:13 no tiene pestañas para eventos, asesorías, informes ni configuración del examen. components/admin/roles.ts declara docente y consultor sin permisos administrativos. CatalogAdminController.cs:34 restringe gestión a roles 1 y 2.
13. Reportes: GetSummaryReportQueryHandler.cs devuelve contadores. Hay endpoints de intentos y configuración, pero no sus controles equivalentes en el panel actual.
14. Auditoría de sesión: AuthLoginCommandHandler.cs devuelve token sin AuditLogs.Add; AuthContext.tsx:58 realiza logout únicamente en cliente.

La búsqueda de funcionalidades ausentes se realizó sobre Application, Infrastructure, Controllers y los componentes/contextos activos de React. No se usaron los fragmentos explicativos de DotnetArchitectureExplorer.tsx ni los manuales visuales como evidencia de implementación.

## Validación ejecutada en esta revisión

- BusinessChecks: 10 comprobaciones correctas con EF InMemory: vigencia, 30 días, duplicidad secuencial, límites 12.99/13.00, bloqueo de reintento y respuestas tardías.
- DocumentChecks: 31 comprobaciones correctas con EF InMemory y almacenamiento simulado: formato, acceso, aislamiento de matrículas, vigencia, marca, fallos de almacenamiento y auditoría.
- MobileChecks: 17 comprobaciones correctas con transporte HTTP simulado: autenticación, reloj de servidor, respuestas guardadas, envío único y Google.
- ArchitectureChecks: correctos; se descubrieron/registraron 83 comandos y consultas y pasaron las validaciones y casos de uso incluidos. Eso no significa 83 procesos funcionales completos validados en producción.
- No se conectaron estas pruebas a PostgreSQL de producción ni se crearon usuarios/pagos reales.
- No se compiló la aplicación Android ni se probó en dispositivo. La ampliación MAUI es trabajo adicional sobre el alcance web original y no resuelve automáticamente los pendientes del backend.
- No se inspeccionó la interfaz completa en navegador en esta revisión ni las configuraciones internas de Contabo/OpenBao/RustFS. Una API pública que responde 200 no acredita por sí sola todos los requisitos funcionales.

## Orden de cierre propuesto

1. Corregir QR público y demostrar escaneo externo; garantizar duplicidad y cuenta única frente a concurrencia; contratar/configurar streaming protegido.
2. Completar correo, confirmación/recuperación, notificaciones y fecha de nacimiento; cerrar vigencia y conservación de cuenta.
3. Resolver descarga frente a visor; separar orden/pago y correcciones; habilitar nueva matrícula tras agotamiento; implementar atención de asesorías y funciones del docente/consultor.
4. Completar paneles de eventos/asesorías/reportes/intentos/configuración; cancelación y reprogramación/devolución; ejecutar una matriz de aceptación con PostgreSQL aislado y navegador; verificar backups/restauración en infraestructura.

No se asigna un porcentaje global de cumplimiento: daría una falsa precisión sin ponderar el alcance de cada proceso y sin probar los recorridos pendientes.
