SELECT pg_advisory_xact_lock(2026103108);
ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "BirthDate" date NULL;
ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "EmailConfirmationRequired" boolean NOT NULL DEFAULT false;
ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "EmailConfirmedAt" timestamp with time zone NULL;
ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "EmailConfirmationStamp" text NOT NULL DEFAULT '';
ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "EmailConfirmationSentAt" timestamp with time zone NULL;
-- Existing users keep access. No birth dates or confirmations are invented.
