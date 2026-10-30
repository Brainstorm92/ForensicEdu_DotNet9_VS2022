import React, { createContext, useContext, useState, useEffect } from 'react';
import QRCode from 'qrcode';
import confetti from 'canvas-confetti';
import { useAuth } from './AuthContext';
import {
  UserProfile,
  UserRole,
  Course,
  EventItem,
  AdvisoryItem,
  Order,
  Payment,
  Enrollment,
  Certificate,
  SupportTicket,
  AuditLogItem,
  SocialLinksConfig,
  ProductType,
  PaymentStatus,
  CertificateStatus,
} from '../types';
import {
  MOCK_USERS,
  MOCK_COURSES,
  MOCK_EVENTS,
  MOCK_ADVISORY,
  SEEDED_ORDERS,
  SEEDED_PAYMENTS,
  SEEDED_ENROLLMENTS,
  SEEDED_CERTIFICATES,
  INITIAL_SOCIAL_LINKS,
  MOCK_QUESTIONS,
} from '../data/mockData';

interface AppContextType {
  currentUser: UserProfile;
  allUsers: UserProfile[];
  switchUserRole: (role: UserRole) => void;
  switchUserById: (userId: string) => void;
  updateCurrentUserProfile: (data: Partial<UserProfile>) => void;
  
  // Navigation
  activeTab: string;
  setActiveTab: (tab: string) => void;
  selectedCourseId: string | null;
  setSelectedCourseId: (id: string | null) => void;
  publicVerificationCode: string | null;
  setPublicVerificationCode: (code: string | null) => void;

  // Catalog
  courses: Course[];
  events: EventItem[];
  advisory: AdvisoryItem;

  // Orders & Payments
  orders: Order[];
  payments: Payment[];
  createOrder: (productType: ProductType, productId: string) => Order;
  registerPayment: (orderId: string, paymentData: {
    banco: 'BCP' | 'BBVA' | 'Interbank' | 'Scotiabank' | 'Banco de la Nación' | 'Yape / Plin';
    numeroOperacion: string;
    fechaPago: string;
    monto: number;
    titularTransferencia: string;
    voucherFileName: string;
    voucherUrl?: string;
  }) => { success: boolean; isDuplicate: boolean; payment: Payment; message: string };
  reviewPayment: (paymentId: string, status: PaymentStatus, reason?: string) => void;

  // Academic & Evaluations
  enrollments: Enrollment[];
  certificates: Certificate[];
  updateModuleProgress: (enrollmentId: string, moduleId: string) => void;
  submitExamAttempt: (enrollmentId: string, answers: Record<string, number>) => {
    attemptNumber: number;
    score: number;
    passed: boolean;
    isFinalFail: boolean;
    certificateIssued: boolean;
  };
  bypassSecondAttemptWait: (enrollmentId: string) => void;
  resetExamForDemo: (enrollmentId: string) => void;

  // Certificates
  verifyCertificateByCode: (code: string) => Certificate | undefined;
  updateCertificateStatus: (certificateId: string, newStatus: CertificateStatus, reason?: string) => void;

  // Support & Social Links
  tickets: SupportTicket[];
  createTicket: (subject: string, category: SupportTicket['category'], message: string) => SupportTicket;
  replyTicket: (ticketId: string, message: string) => void;
  updateTicketStatus: (ticketId: string, status: SupportTicket['status']) => void;
  socialLinks: SocialLinksConfig;
  updateSocialLinks: (links: SocialLinksConfig) => void;

  // Audit Logs
  auditLogs: AuditLogItem[];
  addAuditLog: (action: AuditLogItem['action'], details: string, entityId?: string) => void;

  // Account validity simulation
  simulateValidityChange: (daysToAddOrSub: number) => void;
  resetAllDataToDefault: () => void;
}

const AppContext = createContext<AppContextType | undefined>(undefined);

export const AppProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user: authenticatedUser } = useAuth();
  // Persistence in LocalStorage
  const [allUsers, setAllUsers] = useState<UserProfile[]>(() => {
    const saved = localStorage.getItem('forense_users');
    return saved ? JSON.parse(saved) : MOCK_USERS;
  });

  const [currentUserId, setCurrentUserId] = useState<string>(() => {
    return localStorage.getItem('forense_current_user_id') || 'usr-student-1';
  });

  const currentUser = authenticatedUser || allUsers.find(u => u.id === currentUserId) || allUsers[0];

  const [activeTab, setActiveTab] = useState<string>('catalogo');
  const [selectedCourseId, setSelectedCourseId] = useState<string | null>(null);
  const [publicVerificationCode, setPublicVerificationCode] = useState<string | null>(null);

  const [courses] = useState<Course[]>(MOCK_COURSES);
  const [events] = useState<EventItem[]>(MOCK_EVENTS);
  const [advisory] = useState<AdvisoryItem>(MOCK_ADVISORY);

  const [orders, setOrders] = useState<Order[]>(() => {
    const saved = localStorage.getItem('forense_orders');
    const list: Order[] = saved ? JSON.parse(saved) : SEEDED_ORDERS;
    if (!list.some(o => o.productId === 'crs-escena' && o.userId === 'usr-student-1')) {
      const escenaOrder = SEEDED_ORDERS.find(o => o.id === 'ord-carlos-escena-003');
      if (escenaOrder) list.push(escenaOrder);
    }
    return list;
  });

  const [payments, setPayments] = useState<Payment[]>(() => {
    const saved = localStorage.getItem('forense_payments');
    const list: Payment[] = saved ? JSON.parse(saved) : SEEDED_PAYMENTS;
    if (!list.some(p => p.productId === 'crs-escena' && p.userId === 'usr-student-1')) {
      const escenaPay = SEEDED_PAYMENTS.find(p => p.id === 'pay-carlos-escena-003');
      if (escenaPay) list.push(escenaPay);
    }
    return list;
  });

  const [enrollments, setEnrollments] = useState<Enrollment[]>(() => {
    const saved = localStorage.getItem('forense_enrollments');
    const list: Enrollment[] = saved ? JSON.parse(saved) : SEEDED_ENROLLMENTS;
    if (!list.some(e => e.courseId === 'crs-escena' && e.userId === 'usr-student-1')) {
      const escenaEnr = SEEDED_ENROLLMENTS.find(e => e.id === 'enr-carlos-escena');
      if (escenaEnr) list.push(escenaEnr);
    }
    return list;
  });

  const [certificates, setCertificates] = useState<Certificate[]>(() => {
    const saved = localStorage.getItem('forense_certificates');
    return saved ? JSON.parse(saved) : SEEDED_CERTIFICATES;
  });

  const [tickets, setTickets] = useState<SupportTicket[]>(() => {
    const saved = localStorage.getItem('forense_tickets');
    return saved ? JSON.parse(saved) : [
      {
        id: 'tkt-001',
        ticketCode: 'TK-2026-0041',
        userId: 'usr-student-1',
        userName: 'Carlos Alberto Mendoza Vargas',
        userEmail: 'estudiante@forense.edu.pe',
        subject: 'Consulta sobre el Módulo 3 de Balística',
        category: 'Acceso a Curso',
        message: 'Buenas tardes, quisiera consultar si la descarga de los archivos de Excel para cálculo de parábola requiere contraseña adicional.',
        status: 'RESPONDIDO',
        createdAt: '2026-09-05T16:00:00.000Z',
        responses: [
          {
            author: 'Jorge Peralta (Soporte Técnico)',
            role: 'soporte',
            message: 'Estimado Carlos, los archivos están completamente habilitados sin contraseña para todos los estudiantes matriculados.',
            timestamp: '2026-09-05T17:15:00.000Z',
          }
        ]
      }
    ];
  });

  const [socialLinks, setSocialLinks] = useState<SocialLinksConfig>(() => {
    const saved = localStorage.getItem('forense_social_links');
    return saved ? JSON.parse(saved) : INITIAL_SOCIAL_LINKS;
  });

  const [auditLogs, setAuditLogs] = useState<AuditLogItem[]>(() => {
    const saved = localStorage.getItem('forense_audit_logs');
    return saved ? JSON.parse(saved) : [
      {
        id: 'aud-001',
        timestamp: '2026-09-01T15:30:00.000Z',
        userId: 'usr-finanzas',
        userName: 'Mariana Gómez Centeno',
        userRole: 'admin_financiero',
        action: 'APROBAR_PAGO',
        details: 'Pago pay-seeded-approved-1 de S/. 350.00 aprobado para Orden ORD-2026-000101 (Balística Forense). Se activó vigencia de cuenta de Carlos Alberto Mendoza por 2 años.',
        entityId: 'pay-seeded-approved-1',
      },
      {
        id: 'aud-002',
        timestamp: '2026-09-15T18:00:00.000Z',
        userId: 'usr-academico',
        userName: 'Mg. Roberto Quispe Navarro',
        userRole: 'admin_academico',
        action: 'EMITIR_CERTIFICADO',
        details: 'Certificado CF-2026-BAL-000027 emitido para Carlos Alberto Mendoza Vargas tras aprobar examen con nota 18/20 en primer intento.',
        entityId: 'cert-carlos-balistica-001',
      }
    ];
  });

  // Save changes to localStorage
  useEffect(() => {
    localStorage.setItem('forense_users', JSON.stringify(allUsers));
  }, [allUsers]);

  useEffect(() => {
    localStorage.setItem('forense_current_user_id', currentUserId);
  }, [currentUserId]);

  useEffect(() => {
    localStorage.setItem('forense_orders', JSON.stringify(orders));
  }, [orders]);

  useEffect(() => {
    localStorage.setItem('forense_payments', JSON.stringify(payments));
  }, [payments]);

  useEffect(() => {
    localStorage.setItem('forense_enrollments', JSON.stringify(enrollments));
  }, [enrollments]);

  useEffect(() => {
    localStorage.setItem('forense_certificates', JSON.stringify(certificates));
  }, [certificates]);

  useEffect(() => {
    localStorage.setItem('forense_tickets', JSON.stringify(tickets));
  }, [tickets]);

  useEffect(() => {
    localStorage.setItem('forense_social_links', JSON.stringify(socialLinks));
  }, [socialLinks]);

  useEffect(() => {
    localStorage.setItem('forense_audit_logs', JSON.stringify(auditLogs));
  }, [auditLogs]);

  // Ensure certificates have QR code generated
  useEffect(() => {
    certificates.forEach(async (cert) => {
      if (!cert.qrDataUrl) {
        try {
          const qrUrl = await QRCode.toDataURL(cert.verificationUrl, {
            errorCorrectionLevel: 'H',
            margin: 2,
            width: 256,
            color: { dark: '#0f172a', light: '#ffffff' }
          });
          setCertificates(prev => prev.map(c => c.id === cert.id ? { ...c, qrDataUrl: qrUrl } : c));
        } catch (err) {
          console.error('Error generating QR', err);
        }
      }
    });
  }, [certificates]);

  const addAuditLog = (action: AuditLogItem['action'], details: string, entityId?: string) => {
    const log: AuditLogItem = {
      id: `aud-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
      timestamp: new Date().toISOString(),
      userId: currentUser.id,
      userName: `${currentUser.nombres} ${currentUser.apellidos}`,
      userRole: currentUser.role,
      action,
      details,
      entityId,
      ipSimulated: '190.237.45.18',
    };
    setAuditLogs(prev => [log, ...prev]);
  };

  const switchUserRole = (role: UserRole) => {
    const userWithRole = allUsers.find(u => u.role === role);
    if (userWithRole) {
      setCurrentUserId(userWithRole.id);
      addAuditLog('LOGIN', `Cambio de sesión a usuario con rol ${role}: ${userWithRole.nombres} ${userWithRole.apellidos}`);
    }
  };

  const switchUserById = (userId: string) => {
    const target = allUsers.find(u => u.id === userId);
    if (target) {
      setCurrentUserId(target.id);
      addAuditLog('LOGIN', `Inicio de sesión para ${target.nombres} ${target.apellidos} (${target.role})`);
    }
  };

  const updateCurrentUserProfile = (data: Partial<UserProfile>) => {
    setAllUsers(prev => prev.map(u => u.id === currentUser.id ? { ...u, ...data } : u));
    addAuditLog('LOGIN', `Actualización de perfil personal del usuario ${currentUser.nombres} ${currentUser.apellidos}`);
  };

  // Section 7: Orden de compra y regla de pago por producto
  const createOrder = (productType: ProductType, productId: string): Order => {
    let title = '';
    let price = 0;

    if (productType === 'curso') {
      const c = courses.find(x => x.id === productId);
      title = c ? c.title : 'Curso Forense';
      price = c ? c.price : 350;
    } else if (productType === 'evento') {
      const e = events.find(x => x.id === productId);
      title = e ? e.title : 'Evento Forense';
      price = e ? e.price : 150;
    } else {
      title = advisory.title;
      price = advisory.price;
    }

    const orderSeq = (orders.length + 101).toString().padStart(6, '0');
    const newOrder: Order = {
      id: `ord-${Date.now()}`,
      orderCode: `ORD-2026-${orderSeq}`,
      userId: currentUser.id,
      userEmail: currentUser.email,
      userName: `${currentUser.nombres} ${currentUser.apellidos}`,
      productType,
      productId,
      productTitle: title,
      price,
      createdAt: new Date().toISOString(),
      status: 'PENDIENTE_DE_PAGO',
    };

    setOrders(prev => [newOrder, ...prev]);
    addAuditLog('CREAR_ORDEN', `Generación de orden ${newOrder.orderCode} para producto "${title}" (S/. ${price})`, newOrder.id);
    return newOrder;
  };

  // Section 8 & 9: Registro y validación del pago bancario con CONTROL ANTIFRAUDE Y DUPLICADOS
  const registerPayment = (orderId: string, paymentData: {
    banco: 'BCP' | 'BBVA' | 'Interbank' | 'Scotiabank' | 'Banco de la Nación' | 'Yape / Plin';
    numeroOperacion: string;
    fechaPago: string;
    monto: number;
    titularTransferencia: string;
    voucherFileName: string;
    voucherUrl?: string;
  }) => {
    const order = orders.find(o => o.id === orderId);
    if (!order) {
      return { success: false, isDuplicate: false, payment: {} as Payment, message: 'Orden no encontrada.' };
    }

    // Clean inputs for normalized fingerprint
    const normBanco = paymentData.banco.trim().toUpperCase();
    const normNumOp = paymentData.numeroOperacion.trim().replace(/\s+/g, '');
    const normFecha = paymentData.fechaPago.trim();
    const normMonto = Number(paymentData.monto);
    const normTitular = paymentData.titularTransferencia.trim().toUpperCase();

    // Fingerprint: BANCO-NUMERO_OPERACION-FECHA-MONTO-TITULAR
    const fingerprint = `${normBanco}-${normNumOp}-${normFecha}-${normMonto}-${normTitular}`;

    // Simple hash simulation of voucher
    const voucherHash = `hash_${normNumOp}_${paymentData.voucherFileName.replace(/\s+/g, '')}`;

    // ANTIFRAUDE: Buscar si ya existe un pago APROBADO con la misma huella o número de operación en el mismo banco
    const duplicateApprovedPayment = payments.find(p => 
      p.status === 'APROBADO' && (
        p.paymentFingerprint === fingerprint || 
        (p.banco.toUpperCase() === normBanco && p.numeroOperacion.trim() === normNumOp)
      )
    );

    const isDuplicate = !!duplicateApprovedPayment;
    const initialStatus: PaymentStatus = isDuplicate ? 'DUPLICADO' : 'PENDIENTE';

    const newPayment: Payment = {
      id: `pay-${Date.now()}`,
      orderId: order.id,
      orderCode: order.orderCode,
      userId: currentUser.id,
      userEmail: currentUser.email,
      userName: `${currentUser.nombres} ${currentUser.apellidos}`,
      productType: order.productType,
      productId: order.productId,
      productTitle: order.productTitle,
      banco: paymentData.banco,
      numeroOperacion: normNumOp,
      fechaPago: normFecha,
      monto: normMonto,
      titularTransferencia: paymentData.titularTransferencia,
      voucherFileName: paymentData.voucherFileName,
      voucherHash,
      voucherUrl: paymentData.voucherUrl || 'https://images.unsplash.com/photo-1554224155-6726b3ff858f?auto=format&fit=crop&w=600&q=80',
      status: initialStatus,
      registeredAt: new Date().toISOString(),
      paymentFingerprint: fingerprint,
      rejectionReason: isDuplicate ? `Operación bancaria ${normNumOp} ya fue utilizada en la compra aprobada anterior (${duplicateApprovedPayment?.orderCode}). Regla antifraude: un pago no puede reutilizarse para dos productos.` : undefined,
    };

    setPayments(prev => [newPayment, ...prev]);

    // Update order status
    setOrders(prev => prev.map(o => o.id === orderId ? {
      ...o,
      status: isDuplicate ? 'CANCELADA' : 'PAGO_REGISTRADO',
      paymentId: newPayment.id,
    } : o));

    if (isDuplicate) {
      addAuditLog(
        'DETECTAR_DUPLICADO',
        `ALERTA ANTIFRAUDE: Se intentó registrar pago duplicado #${normNumOp} (${normBanco}) en la orden ${order.orderCode}. Coincide con el pago aprobado previo ${duplicateApprovedPayment?.orderCode}.`,
        newPayment.id
      );
      return {
        success: false,
        isDuplicate: true,
        payment: newPayment,
        message: `ALERTA ANTIFRAUDE: La operación ${normNumOp} del banco ${normBanco} ya fue registrada y aprobada previamente para la orden ${duplicateApprovedPayment?.orderCode}. No se permite reutilizar un mismo pago para múltiples productos.`,
      };
    }

    addAuditLog(
      'REGISTRAR_PAGO',
      `Pago registrado para orden ${order.orderCode}: ${normBanco} Op# ${normNumOp} por S/. ${normMonto} (Comprobante: ${paymentData.voucherFileName})`,
      newPayment.id
    );

    return {
      success: true,
      isDuplicate: false,
      payment: newPayment,
      message: 'Pago registrado exitosamente. Ha sido enviado a revisión administrativa.',
    };
  };

  // Section 8.2 & 10: Validación de pago administrativo
  const reviewPayment = (paymentId: string, newStatus: PaymentStatus, reason?: string) => {
    const payment = payments.find(p => p.id === paymentId);
    if (!payment) return;

    const reviewer = `${currentUser.nombres} ${currentUser.apellidos} (${currentUser.role})`;
    const nowIso = new Date().toISOString();

    setPayments(prev => prev.map(p => p.id === paymentId ? {
      ...p,
      status: newStatus,
      reviewedAt: nowIso,
      reviewedBy: reviewer,
      rejectionReason: reason || p.rejectionReason,
    } : p));

    // Update order
    setOrders(prev => prev.map(o => o.id === payment.orderId ? {
      ...o,
      status: newStatus === 'APROBADO' ? 'APROBADA' : newStatus === 'RECHAZADO' || newStatus === 'DUPLICADO' ? 'CANCELADA' : o.status,
    } : o));

    // If APROBADO: Section 10 (Matrícula) + Section 4.3 (Vigencia 2 años)
    if (newStatus === 'APROBADO') {
      // 1. Matrícula en curso si es curso
      if (payment.productType === 'curso') {
        const existingEnrollment = enrollments.find(e => e.userId === payment.userId && e.courseId === payment.productId);
        if (!existingEnrollment) {
          const newEnrollment: Enrollment = {
            id: `enr-${Date.now()}`,
            userId: payment.userId,
            courseId: payment.productId,
            orderId: payment.orderId,
            paymentId: payment.id,
            enrollmentDate: nowIso,
            progressPercent: 0,
            completedModules: [],
            status: 'ACTIVA',
            examStatus: 'NO_INICIADA',
            attemptsUsed: 0,
          };
          setEnrollments(prev => [newEnrollment, ...prev]);
        }
      }

      // 2. Section 4.3: Vigencia de cuenta -> 2 años a partir de la fecha de ESTE pago aprobado
      const twoYearsFromNow = new Date();
      twoYearsFromNow.setFullYear(twoYearsFromNow.getFullYear() + 2);

      setAllUsers(prev => prev.map(u => u.id === payment.userId ? {
        ...u,
        hasApprovedPurchase: true,
        accountStatus: 'ACTIVA',
        lastPurchaseDate: nowIso,
        validUntil: twoYearsFromNow.toISOString(),
      } : u));

      addAuditLog(
        'APROBAR_PAGO',
        `Pago ${payment.id} de S/. ${payment.monto} aprobado por ${reviewer}. Se habilitó acceso a "${payment.productTitle}" y se extendió la vigencia de la cuenta del usuario por 2 años hasta ${twoYearsFromNow.toLocaleDateString()}.`,
        payment.id
      );
    } else if (newStatus === 'RECHAZADO') {
      addAuditLog('RECHAZAR_PAGO', `Pago ${payment.id} rechazado por ${reviewer}. Motivo: ${reason || 'Comprobante no válido'}`, payment.id);
    } else if (newStatus === 'OBSERVADO') {
      addAuditLog('OBSERVAR_PAGO', `Pago ${payment.id} marcado como OBSERVADO por ${reviewer}. Se solicita subsanación al usuario: ${reason || 'Información incompleta'}`, payment.id);
    } else if (newStatus === 'DUPLICADO') {
      addAuditLog('DETECTAR_DUPLICADO', `Pago ${payment.id} marcado como DUPLICADO por ${reviewer}. Motivo: ${reason || 'Operación repetida'}`, payment.id);
    }
  };

  const updateModuleProgress = (enrollmentId: string, moduleId: string) => {
    setEnrollments(prev => prev.map(enr => {
      if (enr.id !== enrollmentId) return enr;
      const completed = enr.completedModules.includes(moduleId)
        ? enr.completedModules
        : [...enr.completedModules, moduleId];
      
      const course = courses.find(c => c.id === enr.courseId);
      const totalModules = course?.modules.length || 4;
      const progress = Math.min(100, Math.round((completed.length / totalModules) * 100));

      return {
        ...enr,
        completedModules: completed,
        progressPercent: progress,
      };
    }));
  };

  // Section 12: Evaluación y re-evaluación (Regla clave del cliente: 10 min, 13/20, 2do intento reemplaza, <13 = desaprobado definitivo)
  const submitExamAttempt = (enrollmentId: string, answers: Record<string, number>) => {
    const enrollment = enrollments.find(e => e.id === enrollmentId);
    if (!enrollment) {
      return { attemptNumber: 1, score: 0, passed: false, isFinalFail: false, certificateIssued: false };
    }

    const currentAttempts = enrollment.attemptsUsed || 0;
    const attemptNumber = (currentAttempts + 1) as 1 | 2;

    // Grade questions (Balística questions bank has 10 questions, each worth 2 pts)
    const questions = MOCK_QUESTIONS[enrollment.courseId] || MOCK_QUESTIONS['crs-balistica'];
    let correctCount = 0;
    questions.forEach(q => {
      if (answers[q.id] === q.correctAnswerIndex) {
        correctCount++;
      }
    });

    const score = Math.round((correctCount / questions.length) * 20); // 0 to 20
    const passed = score >= 13; // Nota mínima aprobatoria 13/20

    let isFinalFail = false;
    let certificateIssued = false;

    const course = courses.find(c => c.id === enrollment.courseId);
    const courseTitle = course?.title || 'Curso Forense';

    if (attemptNumber === 1) {
      if (passed) {
        // Regla obligatoria: si la nota es 13/20 o superior, el estudiante queda APROBADO y pasa directamente a certificación; NO debe ir a re-evaluación.
        confetti({ particleCount: 100, spread: 70, origin: { y: 0.6 } });
        certificateIssued = true;

        // Generate Certificate
        const certCode = `CF-2026-${course?.code.split('-')[1] || 'BAL'}-${(certificates.length + 27).toString().padStart(6, '0')}`;
        const newCert: Certificate = {
          id: `cert-${Date.now()}`,
          certificateCode: certCode,
          userId: enrollment.userId,
          studentName: `${currentUser.nombres} ${currentUser.apellidos}`,
          studentDocument: currentUser.numeroDocumento,
          courseId: enrollment.courseId,
          courseTitle,
          durationHours: course?.durationHours || 60,
          issueDate: new Date().toISOString().split('T')[0],
          finalScore: score,
          status: 'VÁLIDO',
          verificationUrl: `${window.location.origin}/verificar/${certCode}`,
        };

        setCertificates(prev => [newCert, ...prev]);

        setEnrollments(prev => prev.map(e => e.id === enrollmentId ? {
          ...e,
          examStatus: 'APROBADA',
          firstAttemptScore: score,
          finalScore: score,
          attemptsUsed: 1,
          certificateId: newCert.id,
        } : e));

        addAuditLog(
          'EMITIR_CERTIFICADO',
          `Evaluación aprobada en 1º intento con nota ${score}/20. Se emitió certificado digital ${certCode} para ${currentUser.nombres} ${currentUser.apellidos}.`,
          newCert.id
        );
      } else {
        // Desaprobado en 1er intento -> pasa a re-evaluación con espera de 24 horas
        const waitDate = new Date(Date.now() + 24 * 60 * 60 * 1000); // 24 hours wait
        setEnrollments(prev => prev.map(e => e.id === enrollmentId ? {
          ...e,
          examStatus: 'DESAPROBADA',
          firstAttemptScore: score,
          finalScore: score,
          attemptsUsed: 1,
          secondAttemptAvailableAt: waitDate.toISOString(),
        } : e));

        addAuditLog(
          'FINALIZAR_EXAMEN',
          `1º Intento de evaluación desaprobado con nota ${score}/20 (mínimo 13). Re-evaluación habilitada a partir de ${waitDate.toLocaleString()}.`,
          enrollmentId
        );
      }
    } else {
      // Segundo intento: Reemplaza a la primera nota
      if (passed) {
        confetti({ particleCount: 120, spread: 80, origin: { y: 0.6 } });
        certificateIssued = true;

        const certCode = `CF-2026-${course?.code.split('-')[1] || 'BAL'}-${(certificates.length + 27).toString().padStart(6, '0')}`;
        const newCert: Certificate = {
          id: `cert-${Date.now()}`,
          certificateCode: certCode,
          userId: enrollment.userId,
          studentName: `${currentUser.nombres} ${currentUser.apellidos}`,
          studentDocument: currentUser.numeroDocumento,
          courseId: enrollment.courseId,
          courseTitle,
          durationHours: course?.durationHours || 60,
          issueDate: new Date().toISOString().split('T')[0],
          finalScore: score,
          status: 'VÁLIDO',
          verificationUrl: `${window.location.origin}/verificar/${certCode}`,
        };

        setCertificates(prev => [newCert, ...prev]);

        setEnrollments(prev => prev.map(e => e.id === enrollmentId ? {
          ...e,
          examStatus: 'APROBADA',
          secondAttemptScore: score,
          finalScore: score, // nota del segundo intento sustituye a la primera
          attemptsUsed: 2,
          certificateId: newCert.id,
        } : e));

        addAuditLog(
          'EMITIR_CERTIFICADO',
          `Re-evaluación (2º intento) aprobada con nota definitiva ${score}/20. Se emitió certificado digital ${certCode}.`,
          newCert.id
        );
      } else {
        // Segundo intento desaprobado: Desaprobado definitivo
        isFinalFail = true;
        setEnrollments(prev => prev.map(e => e.id === enrollmentId ? {
          ...e,
          examStatus: 'DESAPROBADA',
          secondAttemptScore: score,
          finalScore: score,
          attemptsUsed: 2,
          status: 'DESAPROBADA',
        } : e));

        addAuditLog(
          'FINALIZAR_EXAMEN',
          `2º Intento desaprobado con nota ${score}/20. El estudiante ha quedado en DESAPROBADO DEFINITIVO. No se emite certificado.`,
          enrollmentId
        );
      }
    }

    return {
      attemptNumber,
      score,
      passed,
      isFinalFail,
      certificateIssued,
    };
  };

  const bypassSecondAttemptWait = (enrollmentId: string) => {
    // Utility for demo/testing: admin can bypass the 24h wait period
    setEnrollments(prev => prev.map(e => e.id === enrollmentId ? {
      ...e,
      secondAttemptAvailableAt: new Date(Date.now() - 1000).toISOString(),
    } : e));
    addAuditLog('FINALIZAR_EXAMEN', `Bypass administrativo de periodo de espera de 24h para re-evaluación en matrícula ${enrollmentId}`);
  };

  const resetExamForDemo = (enrollmentId: string) => {
    setEnrollments(prev => prev.map(e => e.id === enrollmentId ? {
      ...e,
      examStatus: 'NO_INICIADA',
      attemptsUsed: 0,
      firstAttemptScore: undefined,
      secondAttemptScore: undefined,
      finalScore: undefined,
      secondAttemptAvailableAt: undefined,
      status: 'ACTIVA',
    } : e));
    addAuditLog('FINALIZAR_EXAMEN', `Examen reiniciado para fines de prueba/demostración en matrícula ${enrollmentId}`);
  };

  // Section 13: Certificación digital con QR y Verificación Pública
  const verifyCertificateByCode = (code: string): Certificate | undefined => {
    const cleanCode = code.trim().toUpperCase();
    return certificates.find(c => c.certificateCode.toUpperCase() === cleanCode);
  };

  const updateCertificateStatus = (certificateId: string, newStatus: CertificateStatus, reason?: string) => {
    setCertificates(prev => prev.map(c => c.id === certificateId ? {
      ...c,
      status: newStatus,
      revocationReason: reason || c.revocationReason,
      revokedAt: newStatus === 'REVOCADO' || newStatus === 'ANULADO' ? new Date().toISOString() : undefined,
      revokedBy: newStatus === 'REVOCADO' || newStatus === 'ANULADO' ? `${currentUser.nombres} ${currentUser.apellidos}` : undefined,
    } : c));

    addAuditLog(
      'REVOCAR_CERTIFICADO',
      `Estado del certificado ${certificateId} actualizado a ${newStatus}. Motivo: ${reason || 'Decisión administrativa pericial'}`
    );
  };

  // Section 15.2: Soporte y tickets
  const createTicket = (subject: string, category: SupportTicket['category'], message: string): SupportTicket => {
    const code = `TK-2026-${(tickets.length + 101).toString().padStart(4, '0')}`;
    const newTicket: SupportTicket = {
      id: `tkt-${Date.now()}`,
      ticketCode: code,
      userId: currentUser.id,
      userName: `${currentUser.nombres} ${currentUser.apellidos}`,
      userEmail: currentUser.email,
      subject,
      category,
      message,
      status: 'ABIERTO',
      createdAt: new Date().toISOString(),
      responses: [],
    };

    setTickets(prev => [newTicket, ...prev]);
    addAuditLog('RESPONDER_TICKET', `Nuevo ticket de soporte ${code} creado por ${currentUser.nombres} ${currentUser.apellidos}: "${subject}"`, newTicket.id);
    return newTicket;
  };

  const replyTicket = (ticketId: string, message: string) => {
    setTickets(prev => prev.map(t => {
      if (t.id !== ticketId) return t;
      return {
        ...t,
        status: currentUser.role === 'soporte' || currentUser.role === 'superadmin' ? 'RESPONDIDO' : 'EN_ATENCIÓN',
        responses: [
          ...t.responses,
          {
            author: `${currentUser.nombres} ${currentUser.apellidos} (${currentUser.role})`,
            role: currentUser.role,
            message,
            timestamp: new Date().toISOString(),
          }
        ]
      };
    }));
    addAuditLog('RESPONDER_TICKET', `Respuesta añadida al ticket ${ticketId} por ${currentUser.nombres} ${currentUser.apellidos}`, ticketId);
  };

  const updateTicketStatus = (ticketId: string, status: SupportTicket['status']) => {
    setTickets(prev => prev.map(t => t.id === ticketId ? { ...t, status } : t));
  };

  const updateSocialLinks = (links: SocialLinksConfig) => {
    setSocialLinks(links);
    addAuditLog('MODIFICAR_REDES', `Actualización de enlaces y visibilidad de redes sociales por ${currentUser.nombres}`);
  };

  const simulateValidityChange = (daysToAddOrSub: number) => {
    setAllUsers(prev => prev.map(u => {
      if (u.id !== currentUser.id) return u;
      const currentValid = new Date(u.validUntil);
      currentValid.setDate(currentValid.getDate() + daysToAddOrSub);
      
      const isPast = currentValid.getTime() < Date.now();
      return {
        ...u,
        validUntil: currentValid.toISOString(),
        accountStatus: isPast ? 'PENDIENTE_DE_ELIMINACIÓN' : u.hasApprovedPurchase ? 'ACTIVA' : 'REGISTRADA',
      };
    }));
  };

  const resetAllDataToDefault = () => {
    localStorage.clear();
    setAllUsers(MOCK_USERS);
    setCurrentUserId('usr-student-1');
    setOrders(SEEDED_ORDERS);
    setPayments(SEEDED_PAYMENTS);
    setEnrollments(SEEDED_ENROLLMENTS);
    setCertificates(SEEDED_CERTIFICATES);
    setSocialLinks(INITIAL_SOCIAL_LINKS);
    setActiveTab('catalogo');
  };

  return (
    <AppContext.Provider
      value={{
        currentUser,
        allUsers,
        switchUserRole,
        switchUserById,
        updateCurrentUserProfile,
        activeTab,
        setActiveTab,
        selectedCourseId,
        setSelectedCourseId,
        publicVerificationCode,
        setPublicVerificationCode,
        courses,
        events,
        advisory,
        orders,
        payments,
        createOrder,
        registerPayment,
        reviewPayment,
        enrollments,
        certificates,
        updateModuleProgress,
        submitExamAttempt,
        bypassSecondAttemptWait,
        resetExamForDemo,
        verifyCertificateByCode,
        updateCertificateStatus,
        tickets,
        createTicket,
        replyTicket,
        updateTicketStatus,
        socialLinks,
        updateSocialLinks,
        auditLogs,
        addAuditLog,
        simulateValidityChange,
        resetAllDataToDefault,
      }}
    >
      {children}
    </AppContext.Provider>
  );
};

export const useApp = () => {
  const context = useContext(AppContext);
  if (!context) {
    throw new Error('useApp must be used within an AppProvider');
  }
  return context;
};
