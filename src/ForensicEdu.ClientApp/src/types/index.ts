export type UserRole =
  | 'superadmin'
  | 'admin_academico'
  | 'admin_financiero'
  | 'docente'
  | 'soporte'
  | 'estudiante'
  | 'consultor';

export type AccountStatus =
  | 'REGISTRADA'
  | 'ACTIVA'
  | 'PRÓXIMA_A_VENCER'
  | 'VENCIDA'
  | 'SUSPENDIDA'
  | 'PENDIENTE_DE_ELIMINACIÓN'
  | 'ELIMINADA';

export interface UserProfile {
  id: string;
  nombres: string;
  apellidos: string;
  tipoDocumento: 'DNI' | 'CE' | 'Pasaporte';
  numeroDocumento: string;
  paisResidencia: string;
  birthDate?: string;
  email: string;
  telefono: string;
  nivelEducativo: 'Pregrado' | 'Bachiller' | 'Licenciatura' | 'Maestría' | 'Doctorado';
  institucionEducativa: string;
  profesion: string;
  role: UserRole;
  createdAt: string; // ISO date
  accountStatus: AccountStatus;
  validUntil: string; // ISO date (30 days if no approved purchase; 2 years from last approved purchase)
  hasApprovedPurchase: boolean;
  lastPurchaseDate?: string;
}

export type ProductType = 'curso' | 'evento' | 'asesoria';

export interface Course {
  isPublished?: boolean;
  isActive?: boolean;
  id: string;
  code: string;
  title: string;
  category: 'Forense';
  shortDescription: string;
  fullDescription: string;
  price: number;
  durationHours: number;
  instructor: string;
  instructorTitle: string;
  instructorImageUrl?: string;
  modalidad: 'Virtual Asincrónico' | 'Virtual Híbrido';
  requisitos: string;
  materialesIncluidos: string[];
  bannerUrl: string;
  modules: CourseModule[];
}

export interface CourseModule {
  hasVideo?: boolean;
  hasPrivateVideo?: boolean;
  id: string;
  title: string;
  duration: string;
  videoUrl: string; // secure stream url
  streamingId: string;
  documents: CourseDocument[];
}

export interface CourseDocument {
  id: string;
  title: string;
  type: 'PDF' | 'DOCX' | 'PPTX' | 'XLSX';
  size: string;
  downloadUrl: string;
}

export interface EventItem {
  id: string;
  code: string;
  title: string;
  type: 'Congreso' | 'Seminario' | 'Taller' | 'Conferencia en vivo';
  description: string;
  price: number;
  date: string;
  time: string;
  speaker: string;
  speakerBio: string;
  accessStatus: 'PROGRAMADO' | 'EN_VIVO' | 'FINALIZADO' | 'CANCELADO';
  roomUrl?: string;
  bannerUrl: string;
}

export interface AdvisoryItem {
  id: string;
  title: string;
  specialist: string;
  specialty: string;
  description: string;
  price: number;
  whatsappContact: string;
  bannerUrl: string;
}

export type OrderStatus = 'PENDIENTE_DE_PAGO' | 'PAGO_REGISTRADO' | 'APROBADA' | 'CANCELADA';

export interface Order {
  id: string;
  orderCode: string; // e.g. ORD-2026-000125
  userId: string;
  userEmail: string;
  userName: string;
  productType: ProductType;
  productId: string;
  productTitle: string;
  price: number;
  createdAt: string;
  status: OrderStatus;
  paymentId?: string;
}

export type PaymentStatus =
  | 'PENDIENTE'
  | 'EN_REVISIÓN'
  | 'APROBADO'
  | 'RECHAZADO'
  | 'DUPLICADO'
  | 'OBSERVADO';

export interface Payment {
  hasVoucher?: boolean;
  id: string;
  orderId: string;
  orderCode: string;
  userId: string;
  userEmail: string;
  userName: string;
  productType: ProductType;
  productId: string;
  productTitle: string;
  banco: 'BCP' | 'BBVA' | 'Interbank' | 'Scotiabank' | 'Banco de la Nación' | 'Yape / Plin';
  numeroOperacion: string;
  fechaPago: string;
  monto: number;
  titularTransferencia: string;
  voucherFileName: string;
  voucherHash: string;
  voucherUrl?: string;
  status: PaymentStatus;
  registeredAt: string;
  reviewedAt?: string;
  reviewedBy?: string;
  rejectionReason?: string;
  paymentFingerprint: string; // banco + numeroOperacion + fechaPago + monto + titular
}

export interface Enrollment {
  canReenroll?: boolean;
  id: string;
  userId: string;
  courseId: string;
  orderId: string;
  paymentId: string;
  enrollmentDate: string;
  progressPercent: number;
  completedModules: string[];
  status: 'ACTIVA' | 'COMPLETADA' | 'DESAPROBADA' | 'BLOQUEADA';
  examStatus: 'NO_INICIADA' | 'EN_CURSO' | 'APROBADA' | 'DESAPROBADA' | 'EXPIRADA';
  firstAttemptScore?: number;
  secondAttemptScore?: number;
  finalScore?: number;
  attemptsUsed: number; // 0, 1, 2
  secondAttemptAvailableAt?: string; // ISO date for 24h wait
  certificateId?: string;
}

export interface Question {
  id: string;
  question: string;
  options: string[];
  correctAnswerIndex: number;
  explanation: string;
}

export interface ExamAttempt {
  id: string;
  enrollmentId: string;
  userId: string;
  courseId: string;
  attemptNumber: 1 | 2;
  startTime: string;
  endTime?: string;
  durationSeconds: number; // 600s = 10 min
  answers: Record<string, number>;
  score: number; // 0 to 20
  passed: boolean; // >= 13
  submittedAt?: string;
}

export type CertificateStatus = 'VÁLIDO' | 'REVOCADO' | 'ANULADO' | 'VENCIDO';

export interface Certificate {
  id: string;
  certificateCode: string; // e.g. CF-2026-BAL-000027
  userId: string;
  studentName: string;
  studentDocument: string;
  courseId: string;
  courseTitle: string;
  durationHours: number;
  issueDate: string;
  finalScore: number;
  status: CertificateStatus;
  verificationUrl: string;
  qrDataUrl?: string;
  revocationReason?: string;
  revokedAt?: string;
  revokedBy?: string;
}

export interface SupportTicket {
  id: string;
  ticketCode: string;
  userId: string;
  userName: string;
  userEmail: string;
  subject: string;
  category: 'Pagos' | 'Acceso a Curso' | 'Evaluación' | 'Certificado' | 'Asesoría' | 'General';
  message: string;
  status: 'ABIERTO' | 'EN_ATENCIÓN' | 'RESPONDIDO' | 'CERRADO';
  createdAt: string;
  responses: {
    author: string;
    role: string;
    message: string;
    timestamp: string;
  }[];
}

export interface AuditLogItem {
  id: string;
  timestamp: string;
  userId: string;
  userName: string;
  userRole: UserRole;
  action:
    | 'LOGIN'
    | 'CREAR_ORDEN'
    | 'REGISTRAR_PAGO'
    | 'APROBAR_PAGO'
    | 'RECHAZAR_PAGO'
    | 'DETECTAR_DUPLICADO'
    | 'OBSERVAR_PAGO'
    | 'INICIAR_EXAMEN'
    | 'FINALIZAR_EXAMEN'
    | 'EMITIR_CERTIFICADO'
    | 'REVOCAR_CERTIFICADO'
    | 'MODIFICAR_REDES'
    | 'RESPONDER_TICKET';
  details: string;
  entityId?: string;
  ipSimulated?: string;
}

export interface SocialLinksConfig {
  facebook: { url: string; enabled: boolean };
  instagram: { url: string; enabled: boolean };
  youtube: { url: string; enabled: boolean };
  tiktok: { url: string; enabled: boolean };
  linkedin: { url: string; enabled: boolean };
  x: { url: string; enabled: boolean };
  whatsapp: { url: string; enabled: boolean; number: string };
}
