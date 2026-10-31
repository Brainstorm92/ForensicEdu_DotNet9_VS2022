-- Cambio aditivo. No vincula ni modifica identidades existentes.
ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "GoogleSubject" character varying(255) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_GoogleSubject" ON "Users" ("GoogleSubject");
