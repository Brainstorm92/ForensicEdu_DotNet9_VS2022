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
    <div className="fixed inset-0 z-50 bg-slate-950/90 backdrop-blur-md flex items-center justify-center p-4">
      <div className="bg-slate-900 border border-slate-700 rounded-3xl max-w-3xl w-full max-h-[92vh] overflow-y-auto shadow-2xl flex flex-col">
        {/* Exam Header */}
        <div className="p-5 border-b border-slate-800 flex items-center justify-between bg-slate-950 sticky top-0 z-10">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-amber-500/20 text-amber-400 flex items-center justify-center font-bold">
              <Clock className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h3 className="font-bold text-sm sm:text-base text-white">
                  Evaluación Cronometrada: {courseTitle}
                </h3>
                <span className="text-[10px] px-2 py-0.5 rounded-full font-bold bg-indigo-900 text-indigo-200 border border-indigo-700">
                  {isSecondAttempt ? '2º Intento (Definitivo)' : '1º Intento'}
                </span>
              </div>
              <p className="text-xs text-slate-400">
                Regla Sección 12: Duración 10 min • Nota mínima aprobatoria 13/20
              </p>
            </div>
          </div>

          {!isSubmitted && (
            <div className={`flex items-center gap-2 px-3 py-1.5 rounded-xl border font-mono font-bold text-sm ${
              timeLeft < 120 
                ? 'bg-rose-950/80 text-rose-300 border-rose-700 animate-pulse' 
                : 'bg-slate-800 text-amber-300 border-slate-700'
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
            <div className="p-5 rounded-2xl bg-amber-950/60 border border-amber-600 text-amber-200 space-y-3">
              <div className="flex items-center gap-2 font-bold text-sm">
                <AlertCircle className="w-5 h-5 text-amber-400" />
                <span>Periodo de Espera Configurable de 24 Horas Activo (Sección 12.3)</span>
              </div>
              <p className="text-xs leading-relaxed text-amber-100">
                Tras desaprobar el primer intento, el sistema habilita la re-evaluación tras un periodo de espera de 24 horas (habilitada en: <strong className="text-white">{new Date(waitDateStr!).toLocaleString()}</strong>).
              </p>
              <div className="pt-2 flex gap-3">
                <button
                  onClick={() => bypassSecondAttemptWait(enrollment.id)}
                  className="px-3.5 py-1.5 rounded-lg bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs flex items-center gap-1.5"
                >
                  <Sparkles className="w-3.5 h-3.5" />
                  <span>Bypass Administrativo (Saltar espera para pruebas)</span>
                </button>
                <button
                  onClick={onClose}
                  className="px-3.5 py-1.5 rounded-lg bg-slate-800 text-slate-300 text-xs"
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
                <div className="flex items-center justify-between text-xs text-slate-400">
                  <span>Pregunta {currentQuestionIndex + 1} de {questions.length}</span>
                  <span className="font-semibold text-amber-400">Respondidas: {answeredCount} / {questions.length}</span>
                </div>
                <div className="w-full h-2 rounded-full bg-slate-800 overflow-hidden">
                  <div 
                    className="h-full bg-gradient-to-r from-amber-500 to-amber-400 transition-all duration-300"
                    style={{ width: `${(answeredCount / questions.length) * 100}%` }}
                  ></div>
                </div>
              </div>

              {/* Question Box */}
              <div className="p-6 rounded-2xl bg-slate-950 border border-slate-800 space-y-4">
                <div className="flex items-start gap-3">
                  <span className="w-7 h-7 rounded-lg bg-amber-500/10 text-amber-400 border border-amber-500/20 font-bold text-xs flex items-center justify-center shrink-0 mt-0.5">
                    {currentQuestionIndex + 1}
                  </span>
                  <h4 className="text-sm sm:text-base font-bold text-white leading-snug">
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
                            ? 'bg-amber-500/15 border-amber-400 text-amber-200 font-medium shadow-md shadow-amber-500/10'
                            : 'bg-slate-900 border-slate-800 text-slate-300 hover:bg-slate-850 hover:border-slate-700'
                        }`}
                      >
                        <span className={`w-5 h-5 rounded-full border flex items-center justify-center text-[10px] shrink-0 font-bold mt-0.5 ${
                          isSelected ? 'border-amber-400 bg-amber-400 text-slate-950' : 'border-slate-600 text-slate-400'
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
                  className="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 disabled:opacity-40 text-slate-300 text-xs font-semibold"
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
                          ? 'bg-amber-500 text-slate-950'
                          : answers[q.id] !== undefined
                          ? 'bg-indigo-900 text-indigo-200 border border-indigo-700'
                          : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
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
                    className="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 text-xs font-semibold"
                  >
                    Siguiente
                  </button>
                ) : (
                  <button
                    type="button"
                    onClick={handleForceSubmit}
                    className="px-5 py-2.5 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-bold shadow-lg shadow-emerald-950"
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
                <div className="p-8 rounded-3xl bg-slate-950 border border-emerald-600/80 space-y-4 shadow-xl">
                  <div className="w-16 h-16 rounded-full bg-emerald-500/20 border border-emerald-500/40 text-emerald-400 flex items-center justify-center mx-auto">
                    <Award className="w-10 h-10" />
                  </div>
                  <h3 className="text-2xl font-extrabold text-white">
                    ¡FELICITACIONES! HAS APROBADO EL CURSO
                  </h3>
                  <div className="text-4xl font-extrabold text-emerald-400 font-mono">
                    {examResult.score} <span className="text-base text-slate-400 font-normal">/ 20</span>
                  </div>
                  <p className="text-xs sm:text-sm text-slate-300 max-w-md mx-auto leading-relaxed">
                    Has superado la nota mínima aprobatoria de <strong className="text-white">13/20</strong> en el {examResult.attemptNumber}º intento. Conforme a la <strong className="text-amber-300">Sección 12.2</strong>, pasas directamente a la emisión de tu certificado digital oficial con código QR verificable.
                  </p>

                  <div className="p-3.5 rounded-xl bg-emerald-950/50 border border-emerald-700/60 text-xs text-emerald-300 max-w-md mx-auto">
                    ✓ Certificado emitido y registrado en la base de datos de verificación pública.
                  </div>

                  <div className="pt-4 flex justify-center gap-3">
                    <button
                      onClick={() => {
                        onClose();
                        setActiveTab('mis-certificados');
                      }}
                      className="px-6 py-3 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs sm:text-sm flex items-center gap-2 shadow-xl shadow-amber-500/20"
                    >
                      <Award className="w-4 h-4" />
                      <span>Ver y Descargar Mi Certificado con QR</span>
                    </button>
                  </div>
                </div>
              ) : (
                /* DESAPROBADO: < 13 */
                <div className="p-8 rounded-3xl bg-slate-950 border border-rose-600/80 space-y-4 shadow-xl">
                  <div className="w-16 h-16 rounded-full bg-rose-500/20 border border-rose-500/40 text-rose-400 flex items-center justify-center mx-auto">
                    <AlertCircle className="w-10 h-10" />
                  </div>
                  <h3 className="text-xl sm:text-2xl font-extrabold text-white">
                    {examResult.isFinalFail ? 'DESAPROBADO DEFINITIVO' : 'DESAPROBADO EN PRIMER INTENTO'}
                  </h3>
                  <div className="text-4xl font-extrabold text-rose-400 font-mono">
                    {examResult.score} <span className="text-base text-slate-400 font-normal">/ 20</span>
                  </div>

                  {examResult.isFinalFail ? (
                    <div className="space-y-3 max-w-md mx-auto">
                      <p className="text-xs text-slate-300 leading-relaxed">
                        Conforme a la <strong className="text-rose-300">Sección 12.3</strong>, la nota del segundo intento ({examResult.score}/20) sustituye a la primera. Al ser inferior a 13/20, has quedado en <strong className="text-white">Desaprobado Definitivo</strong>.
                      </p>
                      <div className="p-3 rounded-xl bg-rose-950/60 border border-rose-800 text-xs text-rose-300 font-semibold">
                        🚫 No se permiten más intentos ni se emitirá certificado sin una nueva matrícula autorizada.
                      </div>
                    </div>
                  ) : (
                    <div className="space-y-3 max-w-md mx-auto">
                      <p className="text-xs text-slate-300 leading-relaxed">
                        Conforme a la <strong className="text-amber-300">Sección 12.2</strong>, al obtener una nota menor a 13/20 pasas al flujo de re-evaluación. Tienes derecho a exactamente <strong className="text-white">un segundo intento</strong>.
                      </p>
                      <div className="p-3 rounded-xl bg-amber-950/60 border border-amber-800 text-xs text-amber-200">
                        ⏳ La re-evaluación se habilitará tras el periodo de espera de 24 horas para estudio y repaso de materiales.
                      </div>
                    </div>
                  )}

                  <div className="pt-4 flex justify-center gap-3">
                    <button
                      onClick={onClose}
                      className="px-6 py-2.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 font-bold text-xs"
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
