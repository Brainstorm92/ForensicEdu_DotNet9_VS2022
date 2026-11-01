SELECT pg_advisory_xact_lock(2026103107);
CREATE TABLE IF NOT EXISTS "AdvisoryCases" (
 "Id" text PRIMARY KEY,"OrderId" text NOT NULL REFERENCES "Orders"("Id") ON DELETE RESTRICT,
 "AdvisoryServiceId" text NOT NULL REFERENCES "AdvisoryServices"("Id") ON DELETE RESTRICT,
 "StudentId" text NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
 "ConsultantId" text NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
 "Status" text NOT NULL,"ScheduledAt" timestamp with time zone NULL,"Summary" text NOT NULL,"InternalNotes" text NOT NULL,
 "CreatedAt" timestamp with time zone NOT NULL,"UpdatedAt" timestamp with time zone NOT NULL,"Version" text NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_AdvisoryCases_OrderId" ON "AdvisoryCases"("OrderId");
CREATE INDEX IF NOT EXISTS "IX_AdvisoryCases_StudentId" ON "AdvisoryCases"("StudentId");
CREATE INDEX IF NOT EXISTS "IX_AdvisoryCases_ConsultantId" ON "AdvisoryCases"("ConsultantId");
CREATE INDEX IF NOT EXISTS "IX_AdvisoryCases_AdvisoryServiceId" ON "AdvisoryCases"("AdvisoryServiceId");
CREATE TABLE IF NOT EXISTS "AdvisorySessions" (
 "Id" text PRIMARY KEY,"AdvisoryCaseId" text NOT NULL REFERENCES "AdvisoryCases"("Id") ON DELETE CASCADE,
 "ScheduledAt" timestamp with time zone NOT NULL,"DurationMinutes" integer NOT NULL,"Status" text NOT NULL,
 "Summary" text NOT NULL,"InternalNotes" text NOT NULL,"UpdatedAt" timestamp with time zone NOT NULL
);
CREATE INDEX IF NOT EXISTS "IX_AdvisorySessions_AdvisoryCaseId" ON "AdvisorySessions"("AdvisoryCaseId");
CREATE TABLE IF NOT EXISTS "AdvisoryCareHistory" (
 "Id" text PRIMARY KEY,"AdvisoryCaseId" text NOT NULL REFERENCES "AdvisoryCases"("Id") ON DELETE CASCADE,
 "CreatedAt" timestamp with time zone NOT NULL,"ActorId" text NOT NULL,"Action" text NOT NULL,"Details" text NOT NULL
);
CREATE INDEX IF NOT EXISTS "IX_AdvisoryCareHistory_AdvisoryCaseId" ON "AdvisoryCareHistory"("AdvisoryCaseId");
-- Each historical approved order gets its own case. Running again creates no duplicates.
INSERT INTO "AdvisoryCases" ("Id","OrderId","AdvisoryServiceId","StudentId","Status","Summary","InternalNotes","CreatedAt","UpdatedAt","Version")
 SELECT 'adv-'||o."Id",o."Id",o."ProductId",o."UserId",'PENDIENTE_ASIGNACION','','',o."CreatedAt",now(),'initial-'||o."Id"
 FROM "Orders" o JOIN "AdvisoryServices" a ON a."Id"=o."ProductId" JOIN "Users" u ON u."Id"=o."UserId"
 WHERE o."ProductType"=3 AND o."Status"=2 ON CONFLICT ("OrderId") DO NOTHING;
INSERT INTO "AdvisoryCareHistory" ("Id","AdvisoryCaseId","CreatedAt","ActorId","Action","Details")
 SELECT 'init-'||c."Id",c."Id",now(),'system','ADVISORY_CARE_CREATED','Compra aprobada. Pendiente de asignación de consultor.'
 FROM "AdvisoryCases" c WHERE NOT EXISTS (SELECT 1 FROM "AdvisoryCareHistory" h WHERE h."AdvisoryCaseId"=c."Id") ON CONFLICT ("Id") DO NOTHING;
