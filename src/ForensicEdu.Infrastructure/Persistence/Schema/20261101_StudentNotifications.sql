SELECT pg_advisory_xact_lock(720261101);
CREATE TABLE IF NOT EXISTS "StudentNotifications" (
 "Id" text PRIMARY KEY, "UserId" text NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
 "EventKey" varchar(300) NOT NULL, "Kind" text NOT NULL, "Title" text NOT NULL, "Body" text NOT NULL,
 "ActionTab" text NOT NULL, "CreatedAt" timestamptz NOT NULL, "ReadAt" timestamptz NULL,
 "EmailExpiresAt" timestamptz NULL, "EmailSentAt" timestamptz NULL, "NextEmailAttemptAt" timestamptz NULL,
 "EmailAttempts" integer NOT NULL, "EmailStatus" text NOT NULL, "Version" text NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_StudentNotifications_EventKey" ON "StudentNotifications"("EventKey");
CREATE INDEX IF NOT EXISTS "IX_StudentNotifications_UserId_CreatedAt" ON "StudentNotifications"("UserId","CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_StudentNotifications_EmailStatus_NextEmailAttemptAt" ON "StudentNotifications"("EmailStatus","NextEmailAttemptAt");
