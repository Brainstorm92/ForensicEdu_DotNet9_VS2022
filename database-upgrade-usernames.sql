-- Actualización aditiva. Ejecutar antes de iniciar la versión con usuarios de acceso.
ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "Username" character varying(50) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Username" ON "Users" ("Username");
