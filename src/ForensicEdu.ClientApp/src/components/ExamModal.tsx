import React, { useState, useEffect } from 'react';
import { useApp } from '../context/AppContext';
import {
  Clock,
  AlertCircle,
  CheckCircle2,
  Award,
  ArrowRight,
  X,
  HelpCircle,
  Sparkles,
  RotateCcw
} from 'lucide-react';
import { Enrollment } from '../types';
import { MOCK_QUESTIONS } from '../data/mockData';

interface ExamModalProps {
  enrollment: Enrollment;
  courseTitle: string;
  onClose: () => void;
}

export const ExamModal: React.FC<ExamModalProps> = ({ enrollment, courseTitle, onClose }) => {
  const { submitExamAttempt, bypassSecondAttemptWait, setActiveTab } = useApp();

  const questions = MOCK_QUESTIONS[enrollment.courseId] || MOCK_QUESTIONS['crs-balistica'];
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [answers, setAnswers] = useState<Record<string, number>>({});

  // 10 minutes timer: 600 seconds (Section 12.1)
  const [timeLeft, setTimeLeft] = useState(600);
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [examResult, setExamResult] = useState<{
    attemptNumber: number;
    score: number;
    passed: boolean;
    isFinalFail: boolean;
    certificateIssued: boolean;
  } | null>(null);

  // Timer countdown
  useEffect(() => {
    if (isSubmitted || timeLeft <= 0) return;

    const timer = setInterval(() => {
      setTimeLeft(prev => {
        if (prev <= 1) {
          clearInterval(timer);
          // Auto submit when time expires (Section 12.1 Fin del tiempo)
          handleForceSubmit();
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [isSubmitted, timeLeft]);

  const handleSelectOption = (questionId: string, optionIndex: number) => {
    if (isSubmitted) return;
    setAnswers(prev => ({ ...prev, [questionId]: optionIndex }));
  };

  const handleForceSubmit = () => {
    if (isSubmitted) return;
    setIsSubmitted(true);
    const res = submitExamAttempt(enrollment.id, answers);
    setExamResult(res);
  };

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const answeredCount = Object.keys(answers).length;
  const currentQ = questions[currentQuestionIndex];

  // Check if second attempt is blocked due to 24h wait
  const waitDateStr = enrollment.secondAttemptAvailableAt;
  const isWaitActive = waitDateStr ? new Date(waitDateStr).getTime() > Date.now() : false;
  const isSecondAttempt = enrollment.attemptsUsed === 1;

  return (
    <div className="fixed inset-0 z-50 bg-canvas/90 backdrop-blur-md flex items-center justify-center p-4">
      <div className="bg-panel border border-line rounded-3xl max-w-3xl w-full max-h-[92vh] overflow-y-auto shadow-xl flex flex-col">
        {/* Exam Header */}
        <div className="p-5 border-b border-line flex items-center justify-between bg-canvas sticky top-0 z-10">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-brand/20 text-brand flex items-center justify-center font-bold">
              <Clock className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h3 className="font-bold text-sm sm:text-base text-ink">
                  Evaluación Cronometrada: {courseTitle}
                </h3>
                <span className="text-[10px] px-2 py-0.5 rounded-full font-bold bg-indigo-50 text-indigo-700 border border-indigo-200">
                  {isSecondAttempt ? '2º Intento (Definitivo)' : '1º Intento'}
                </span>
              </div>
              <p className="text-xs text-muted">
                Regla Sección 12: Duración 10 min • Nota mínima aprobatoria 13/20
              </p>
            </div>
          </div>

          {!isSubmitted && (
            <div className={`flex items-center gap-2 px-3 py-1.5 rounded-xl border font-mono font-bold text-sm ${
              timeLeft < 120
                ? 'bg-rose-50/80 text-rose-700 border-rose-200 animate-pulse'
                : 'bg-surface-low text-brand border-line'
            }`}>
              <Clock className="w-4 h-4" />
              <span>{formatTime(timeLeft)}</span>
            </div>
          )}
        </div>

        {/* Exam Body */}
        <div className="p-6 space-y-6">
          {/* Waiting Period Alert (If 24h wait is active) */}
          {isWaitActive && !isSubmitted && (
            <div className="p-5 rounded-2xl bg-brand/60 border border-brand text-brand space-y-3">
              <div className="flex items-center gap-2 font-bold text-sm">
                <AlertCircle className="w-5 h-5 text-brand" />
                <span>Periodo de Espera Configurable de 24 Horas Activo (Sección 12.3)</span>
              </div>
              <p className="text-xs leading-relaxed text-brand">
                Tras desaprobar el primer intento, el sistema habilita la re-evaluación tras un periodo de espera de 24 horas (habilitada en: <strong className="text-ink">{new Date(waitDateStr!).toLocaleString()}</strong>).
              </p>
              <div className="pt-2 flex gap-3">
                <button
                  onClick={() => bypassSecondAttemptWait(enrollment.id)}
                  className="px-3.5 py-1.5 rounded-lg bg-brand hover:bg-brand text-ink font-bold text-xs flex items-center gap-1.5"
                >
                  <Sparkles className="w-3.5 h-3.5" />
                  <span>Bypass Administrativo (Saltar espera para pruebas)</span>
                </button>
                <button
                  onClick={onClose}
                  className="px-3.5 py-1.5 rounded-lg bg-surface-low text-muted text-xs"
                >
                  Regresar al Aula
                </button>
              </div>
            </div>
          )}

          {/* ACTIVE TEST VIEW */}
          {!isSubmitted && (!isWaitActive || enrollment.attemptsUsed === 0) && (
            <div className="space-y-6">
              {/* Progress Tracker */}
              <div className="space-y-2">
                <div className="flex items-center justify-between text-xs text-muted">
                  <span>Pregunta {currentQuestionIndex + 1} de {questions.length}</span>
                  <span className="font-semibold text-brand">Respondidas: {answeredCount} / {questions.length}</span>
                </div>
                <div className="w-full h-2 rounded-full bg-surface-low overflow-hidden">
                  <div
                    className="h-full bg-gradient-to-r from-navy to-brand transition-all duration-300"
                    style={{ width: `${(answeredCount / questions.length) * 100}%` }}
                  ></div>
                </div>
              </div>

              {/* Question Box */}
              <div className="p-6 rounded-2xl bg-canvas border border-line space-y-4">
                <div className="flex items-start gap-3">
                  <span className="w-7 h-7 rounded-lg bg-brand/10 text-brand border border-brand/20 font-bold text-xs flex items-center justify-center shrink-0 mt-0.5">
                    {currentQuestionIndex + 1}
                  </span>
                  <h4 className="text-sm sm:text-base font-bold text-ink leading-snug">
                    {currentQ.question}
                  </h4>
                </div>

                {/* Options */}
                <div className="space-y-2.5 pt-2">
                  {currentQ.options.map((option, optIdx) => {
                    const isSelected = answers[currentQ.id] === optIdx;
                    return (
                      <button
                        key={optIdx}
                        type="button"
                        onClick={() => handleSelectOption(currentQ.id, optIdx)}
                        className={`w-full text-left p-3.5 rounded-xl border text-xs sm:text-sm transition-all flex items-start gap-3 ${
                          isSelected
                            ? 'bg-brand/15 border-brand text-brand font-medium shadow-md shadow-brand/10'
                            : 'bg-panel border-line text-muted hover:bg-surface-high hover:border-line'
                        }`}
                      >
                        <span className={`w-5 h-5 rounded-full border flex items-center justify-center text-[10px] shrink-0 font-bold mt-0.5 ${
                          isSelected ? 'border-brand bg-brand text-ink' : 'border-line text-muted'
                        }`}>
                          {String.fromCharCode(65 + optIdx)}
                        </span>
                        <span className="flex-1">{option}</span>
                      </button>
                    );
                  })}
                </div>
              </div>

              {/* Navigation Controls */}
              <div className="flex items-center justify-between pt-2">
                <button
                  type="button"
                  disabled={currentQuestionIndex === 0}
                  onClick={() => setCurrentQuestionIndex(prev => Math.max(0, prev - 1))}
                  className="px-4 py-2 rounded-xl bg-surface-low hover:bg-surface-high disabled:opacity-40 text-muted text-xs font-semibold"
                >
                  Anterior
                </button>

                <div className="flex items-center gap-1.5 overflow-x-auto max-w-xs">
                  {questions.map((q, idx) => (
                    <button
                      key={q.id}
                      onClick={() => setCurrentQuestionIndex(idx)}
                      className={`w-7 h-7 rounded-lg text-xs font-bold transition-all ${
                        idx === currentQuestionIndex
                          ? 'bg-brand text-ink'
                          : answers[q.id] !== undefined
                          ? 'bg-indigo-50 text-indigo-700 border border-indigo-200'
                          : 'bg-surface-low text-muted hover:bg-surface-high'
                      }`}
                    >
                      {idx + 1}
                    </button>
                  ))}
                </div>

                {currentQuestionIndex < questions.length - 1 ? (
                  <button
                    type="button"
                    onClick={() => setCurrentQuestionIndex(prev => Math.min(questions.length - 1, prev + 1))}
                    className="px-4 py-2 rounded-xl bg-surface-low hover:bg-surface-high text-muted text-xs font-semibold"
                  >
                    Siguiente
                  </button>
                ) : (
                  <button
                    type="button"
                    onClick={handleForceSubmit}
                    className="px-5 py-2.5 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-ink text-xs font-bold shadow-lg shadow-emerald-950"
                  >
                    Finalizar y Enviar Examen
                  </button>
                )}
              </div>
            </div>
          )}

          {/* RESULTS DISPLAY ACCORDING TO SECTION 12 */}
          {isSubmitted && examResult && (
            <div className="py-4 space-y-6 text-center animate-in zoom-in-95 duration-200">
              {examResult.passed ? (
                /* APROBADO: >= 13 */
                <div className="p-8 rounded-3xl bg-canvas border border-emerald-200/80 space-y-4 shadow-xl">
                  <div className="w-16 h-16 rounded-full bg-emerald-500/20 border border-emerald-500/40 text-emerald-700 flex items-center justify-center mx-auto">
                    <Award className="w-10 h-10" />
                  </div>
                  <h3 className="text-2xl font-extrabold text-ink">
                    ¡FELICITACIONES! HAS APROBADO EL CURSO
                  </h3>
                  <div className="text-4xl font-extrabold text-emerald-700 font-mono">
                    {examResult.score} <span className="text-base text-muted font-normal">/ 20</span>
                  </div>
                  <p className="text-xs sm:text-sm text-muted max-w-md mx-auto leading-relaxed">
                    Has superado la nota mínima aprobatoria de <strong className="text-ink">13/20</strong> en el {examResult.attemptNumber}º intento. Conforme a la <strong className="text-brand">Sección 12.2</strong>, pasas directamente a la emisión de tu certificado digital oficial con código QR verificable.
                  </p>

                  <div className="p-3.5 rounded-xl bg-emerald-50/50 border border-emerald-200/60 text-xs text-emerald-700 max-w-md mx-auto">
                    ✓ Certificado emitido y registrado en la base de datos de verificación pública.
                  </div>

                  <div className="pt-4 flex justify-center gap-3">
                    <button
                      onClick={() => {
                        onClose();
                        setActiveTab('mis-certificados');
                      }}
                      className="px-6 py-3 rounded-xl bg-brand hover:bg-brand text-ink font-bold text-xs sm:text-sm flex items-center gap-2 shadow-xl shadow-brand/20"
                    >
                      <Award className="w-4 h-4" />
                      <span>Ver y Descargar Mi Certificado con QR</span>
                    </button>
                  </div>
                </div>
              ) : (
                /* DESAPROBADO: < 13 */
                <div className="p-8 rounded-3xl bg-canvas border border-rose-200/80 space-y-4 shadow-xl">
                  <div className="w-16 h-16 rounded-full bg-rose-500/20 border border-rose-500/40 text-rose-700 flex items-center justify-center mx-auto">
                    <AlertCircle className="w-10 h-10" />
                  </div>
                  <h3 className="text-xl sm:text-2xl font-extrabold text-ink">
                    {examResult.isFinalFail ? 'DESAPROBADO DEFINITIVO' : 'DESAPROBADO EN PRIMER INTENTO'}
                  </h3>
                  <div className="text-4xl font-extrabold text-rose-700 font-mono">
                    {examResult.score} <span className="text-base text-muted font-normal">/ 20</span>
                  </div>

                  {examResult.isFinalFail ? (
                    <div className="space-y-3 max-w-md mx-auto">
                      <p className="text-xs text-muted leading-relaxed">
                        Conforme a la <strong className="text-rose-700">Sección 12.3</strong>, la nota del segundo intento ({examResult.score}/20) sustituye a la primera. Al ser inferior a 13/20, has quedado en <strong className="text-ink">Desaprobado Definitivo</strong>.
                      </p>
                      <div className="p-3 rounded-xl bg-rose-50/60 border border-rose-200 text-xs text-rose-700 font-semibold">
                        🚫 No se permiten más intentos ni se emitirá certificado sin una nueva matrícula autorizada.
                      </div>
                    </div>
                  ) : (
                    <div className="space-y-3 max-w-md mx-auto">
                      <p className="text-xs text-muted leading-relaxed">
                        Conforme a la <strong className="text-brand">Sección 12.2</strong>, al obtener una nota menor a 13/20 pasas al flujo de re-evaluación. Tienes derecho a exactamente <strong className="text-ink">un segundo intento</strong>.
                      </p>
                      <div className="p-3 rounded-xl bg-brand/60 border border-brand text-xs text-brand">
                        ⏳ La re-evaluación se habilitará tras el periodo de espera de 24 horas para estudio y repaso de materiales.
                      </div>
                    </div>
                  )}

                  <div className="pt-4 flex justify-center gap-3">
                    <button
                      onClick={onClose}
                      className="px-6 py-2.5 rounded-xl bg-surface-low hover:bg-surface-high text-ink font-bold text-xs"
                    >
                      Cerrar y Regresar al Aula
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
