/**
 * Cliente HTTP para comunicación con el backend .NET 9 Web API
 * (ForensicEdu.WebApi / ForensicEdu.sln)
 */

import { authHeaders } from './apiTransport';
export { API_BASE_URL } from './apiTransport';
import { API_BASE_URL } from './apiTransport';

export interface CheckoutPayload {
  userId: string;
  productId: string;
  productType: 'CURSO' | 'EVENTO' | 'ASESORIA';
  amount: number;
  bankName: string;
  operationNumber: string;
  operationDate: string;
  accountHolderName: string;
}

export interface ExamSubmissionPayload {
  enrollmentId: string;
  userId: string;
  selectedAnswers: Record<string, number>;
  durationSeconds: number;
}

export const DotnetApiClient = {
  // 1. Auth & Usuarios
  login: async (email: string, password: string) => {
    const res = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: { ...authHeaders(), 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });
    if (!res.ok) throw new Error((await res.json()).message || 'Error de inicio de sesión');
    return res.json();
  },

  // 2. Catálogo de Cursos
  getCourses: async () => {
    const res = await fetch(`${API_BASE_URL}/courses`);
    if (!res.ok) throw new Error('Error al cargar cursos desde .NET 9');
    return res.json();
  },

  // 3. Órdenes y Validación Antifraude en Tiempo Real
  submitCheckoutOrder: async (payload: CheckoutPayload) => {
    const res = await fetch(`${API_BASE_URL}/orders/checkout`, {
      method: 'POST',
      headers: { ...authHeaders(), 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });
    const data = await res.json();
    if (!res.ok) {
      throw new Error(data.message || 'Error al procesar la orden y pago');
    }
    return data;
  },

  // 4. Rendición de Evaluación (10 min / 13 a 20)
  submitExam: async (payload: ExamSubmissionPayload) => {
    const res = await fetch(`${API_BASE_URL}/exams/submit`, {
      method: 'POST',
      headers: { ...authHeaders(), 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });
    const data = await res.json();
    if (!res.ok) throw new Error(data.message || 'Error al registrar evaluación');
    return data;
  },

  // 5. Validación Pública de Certificados QR
  verifyCertificate: async (code: string) => {
    const res = await fetch(`${API_BASE_URL}/certificates/verify/${encodeURIComponent(code)}`);
    const data = await res.json();
    return data;
  },

  // 6. Descarga de Diploma PDF Oficial
  downloadCertificatePdfUrl: (code: string) => {
    return `${API_BASE_URL}/certificates/download/${encodeURIComponent(code)}/pdf`;
  },

  // 7. Aprobación Financiera y Renovación de Vigencia (2 Años)
  approvePayment: async (paymentId: string, adminUserId: string) => {
    const res = await fetch(`${API_BASE_URL}/admin/payments/${paymentId}/approve?adminUserId=${adminUserId}`, {
      method: 'POST', headers: authHeaders(),
    });
    if (!res.ok) throw new Error('Error al aprobar pago');
    return res.json();
  },

  // 8. Revocación de Certificado
  revokeCertificate: async (code: string, reason: string, adminUserId: string) => {
    const res = await fetch(`${API_BASE_URL}/admin/certificates/${encodeURIComponent(code)}/revoke?adminUserId=${adminUserId}`, {
      method: 'POST',
      headers: { ...authHeaders(), 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason }),
    });
    if (!res.ok) throw new Error('Error al revocar certificado');
    return res.json();
  }
};
