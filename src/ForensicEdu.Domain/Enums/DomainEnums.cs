namespace ForensicEdu.Domain.Enums;

public enum UserRole
{
    SUPERADMIN = 1,
    ADMIN_ACADEMICO = 2,
    ADMIN_FINANCIERO = 3,
    DOCENTE = 4,
    SOPORTE = 5,
    ESTUDIANTE = 6,
    CONSULTOR = 7,
    Superadmin = 1,
    AdminAcademico = 2,
    AdminFinanciero = 3,
    Docente = 4,
    Soporte = 5,
    Estudiante = 6,
    Consultor = 7
}

public enum AccountStatus
{
    REGISTRADA = 1,
    ACTIVO = 2,
    PROXIMA_A_VENCER = 3,
    VENCIDA = 4,
    SUSPENDIDO = 5,
    PENDIENTE_DE_ELIMINACION = 6,
    ELIMINADO = 7,
    Registrada = 1,
    Activa = 2,
    ProximaAVencer = 3,
    Vencida = 4,
    Suspendido = 5,
    PendienteDeEliminacion = 6,
    Eliminada = 7
}

public enum ProductType
{
    CURSO = 1,
    EVENTO = 2,
    ASESORIA = 3,
    Curso = 1,
    Evento = 2,
    Asesoria = 3
}

public enum OrderStatus
{
    PENDIENTE_VALIDACION = 1,
    APROBADO = 2,
    RECHAZADO = 3,
    CANCELADA = 4,
    PendienteDePago = 1,
    PagoRegistrado = 1,
    Aprobada = 2,
    Cancelada = 4
}

public enum PaymentStatus
{
    PENDIENTE = 1,
    EN_REVISION = 2,
    APROBADO = 3,
    RECHAZADO = 4,
    DUPLICADO = 5,
    OBSERVADO = 6,
    Pendiente = 1,
    EnRevision = 2,
    Aprobado = 3,
    Rechazado = 4,
    Duplicado = 5,
    Observado = 6
}

public enum ExamStatus
{
    NO_INICIADA = 1,
    NO_INICIADO = 1,
    EN_CURSO = 2,
    APROBADO = 3,
    DESAPROBADO_TEMPORAL = 4,
    DESAPROBADO_DEFINITIVO = 5,
    EXPIRADO = 6,
    NoIniciada = 1,
    EnCurso = 2,
    Aprobada = 3,
    Desaprobada = 4,
    Expirada = 6
}

public enum CertificateStatus
{
    VALIDO = 1,
    REVOCADO = 2,
    ANULADO = 3,
    VENCIDO = 4,
    Valido = 1,
    Revocado = 2,
    Anulado = 3,
    Vencido = 4
}

public enum TicketStatus
{
    ABIERTO = 1,
    EN_ATENCION = 2,
    RESPONDIDO = 3,
    CERRADO = 4,
    Abierto = 1,
    EnAtencion = 2,
    Respondido = 3,
    Cerrado = 4
}
