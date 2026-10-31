-- Cambio aditivo: conserva cursos, matrículas e imágenes existentes.
ALTER TABLE "Courses" ADD COLUMN IF NOT EXISTS "InstructorImageUrl" text NOT NULL DEFAULT '';
