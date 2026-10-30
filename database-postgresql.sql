CREATE TABLE "AdvisoryServices" (
    "Id" text NOT NULL,
    "Title" text NOT NULL,
    "Specialist" text NOT NULL,
    "Specialty" text NOT NULL,
    "Description" text NOT NULL,
    "Price" numeric NOT NULL,
    "WhatsappContact" text NOT NULL,
    CONSTRAINT "PK_AdvisoryServices" PRIMARY KEY ("Id")
);


CREATE TABLE "AuditLogs" (
    "Id" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "UserId" text NOT NULL,
    "UserName" text NOT NULL,
    "UserRole" text NOT NULL,
    "Action" text NOT NULL,
    "EntityType" text NOT NULL,
    "EntityId" text,
    "Details" text NOT NULL,
    "IpAddress" text,
    CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id")
);


CREATE TABLE "CertificateVerificationLogs" (
    "Id" text NOT NULL,
    "CertificateCode" text NOT NULL,
    "IpAddress" text NOT NULL,
    "UserAgent" text NOT NULL,
    "VerifiedAt" timestamp with time zone NOT NULL,
    "IsValid" boolean NOT NULL,
    CONSTRAINT "PK_CertificateVerificationLogs" PRIMARY KEY ("Id")
);


CREATE TABLE "Courses" (
    "Id" text NOT NULL,
    "Code" text NOT NULL,
    "ShortCode" text NOT NULL,
    "Slug" text NOT NULL,
    "Title" character varying(300) NOT NULL,
    "Category" text NOT NULL,
    "ShortDescription" text NOT NULL,
    "FullDescription" text NOT NULL,
    "Price" numeric(18,2) NOT NULL,
    "DiscountPrice" numeric(18,2) NOT NULL,
    "DurationHours" integer NOT NULL,
    "AcademicHours" integer NOT NULL,
    "OrderIndex" integer NOT NULL,
    "Instructor" text NOT NULL,
    "InstructorTitle" text NOT NULL,
    "Modalidad" text NOT NULL,
    "Requisitos" text NOT NULL,
    "BannerUrl" text NOT NULL,
    "ImageUrl" text NOT NULL,
    "VideoStreamingUrl" text NOT NULL,
    "QuestionsJson" text NOT NULL,
    "IsActive" boolean NOT NULL,
    "IsPublished" boolean NOT NULL,
    CONSTRAINT "PK_Courses" PRIMARY KEY ("Id")
);


CREATE TABLE "LiveEvents" (
    "Id" text NOT NULL,
    "Code" text NOT NULL,
    "Title" text NOT NULL,
    "Type" text NOT NULL,
    "Description" text NOT NULL,
    "Price" numeric NOT NULL,
    "EventDate" timestamp with time zone NOT NULL,
    "TimeSlot" text NOT NULL,
    "Speaker" text NOT NULL,
    "SpeakerBio" text NOT NULL,
    "AccessStatus" text NOT NULL,
    "RoomUrl" text NOT NULL,
    "BannerUrl" text NOT NULL,
    CONSTRAINT "PK_LiveEvents" PRIMARY KEY ("Id")
);


CREATE TABLE "SystemSocialSettings" (
    "Id" text NOT NULL,
    "Key" text NOT NULL,
    "Value" text NOT NULL,
    "Description" text NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_SystemSocialSettings" PRIMARY KEY ("Id")
);


CREATE TABLE "Users" (
    "Id" text NOT NULL,
    "Nombres" text NOT NULL,
    "Apellidos" text NOT NULL,
    "FullName" character varying(200) NOT NULL,
    "TipoDocumento" text NOT NULL,
    "NumeroDocumento" text NOT NULL,
    "Dni" character varying(20) NOT NULL,
    "PaisResidencia" text NOT NULL,
    "Email" character varying(200) NOT NULL,
    "PasswordHash" text NOT NULL,
    "Telefono" text NOT NULL,
    "Phone" text NOT NULL,
    "NivelEducativo" text NOT NULL,
    "InstitucionEducativa" text NOT NULL,
    "Profesion" text NOT NULL,
    "Role" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "RegisteredAt" timestamp with time zone NOT NULL,
    "Status" integer NOT NULL,
    "AccountStatus" integer NOT NULL,
    "ValidUntil" timestamp with time zone,
    "HasApprovedPurchase" boolean NOT NULL,
    "LastPurchaseDate" timestamp with time zone,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);


CREATE TABLE "CourseModule" (
    "Id" text NOT NULL,
    "CourseId" text NOT NULL,
    "OrderIndex" integer NOT NULL,
    "Title" text NOT NULL,
    "Duration" text NOT NULL,
    "VideoStreamingUrl" text NOT NULL,
    "StreamingId" text NOT NULL,
    CONSTRAINT "PK_CourseModule" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CourseModule_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE
);


CREATE TABLE "Question" (
    "Id" text NOT NULL,
    "CourseId" text NOT NULL,
    "QuestionText" text NOT NULL,
    "OptionsJson" text NOT NULL,
    "CorrectAnswerIndex" integer NOT NULL,
    "Explanation" text NOT NULL,
    CONSTRAINT "PK_Question" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Question_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE
);


CREATE TABLE "Enrollments" (
    "Id" text NOT NULL,
    "UserId" text NOT NULL,
    "CourseId" text NOT NULL,
    "OrderId" text NOT NULL,
    "EnrollmentDate" timestamp with time zone NOT NULL,
    "EnrolledAt" timestamp with time zone NOT NULL,
    "CompletedAt" timestamp with time zone,
    "ProgressPercent" integer NOT NULL,
    "CompletedModulesJson" text NOT NULL,
    "Status" text NOT NULL,
    "ExamStatus" integer NOT NULL,
    "FinalGrade" numeric,
    "FirstAttemptScore" numeric,
    "SecondAttemptScore" numeric,
    "AttemptsUsed" integer NOT NULL,
    "SecondAttemptAvailableAt" timestamp with time zone,
    "CertificateId" text,
    CONSTRAINT "PK_Enrollments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Enrollments_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Enrollments_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "Orders" (
    "Id" text NOT NULL,
    "OrderCode" text NOT NULL,
    "UserId" text NOT NULL,
    "ProductType" integer NOT NULL,
    "ProductId" text NOT NULL,
    "ProductTitle" text NOT NULL,
    "Price" numeric NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "Status" integer NOT NULL,
    CONSTRAINT "PK_Orders" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Orders_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "SupportTickets" (
    "Id" text NOT NULL,
    "TicketCode" text NOT NULL,
    "UserId" text NOT NULL,
    "UserName" text NOT NULL,
    "UserEmail" text NOT NULL,
    "Subject" text NOT NULL,
    "Category" text NOT NULL,
    "Message" text NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "ResponsesJson" text NOT NULL,
    CONSTRAINT "PK_SupportTickets" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SupportTickets_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "CourseDocument" (
    "Id" text NOT NULL,
    "ModuleId" text NOT NULL,
    "Title" text NOT NULL,
    "FileType" text NOT NULL,
    "FileSize" text NOT NULL,
    "StorageUrl" text NOT NULL,
    "RequireWatermark" boolean NOT NULL,
    CONSTRAINT "PK_CourseDocument" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CourseDocument_CourseModule_ModuleId" FOREIGN KEY ("ModuleId") REFERENCES "CourseModule" ("Id") ON DELETE CASCADE
);


CREATE TABLE "DigitalCertificates" (
    "Id" text NOT NULL,
    "EnrollmentId" text NOT NULL,
    "UserId" text NOT NULL,
    "CourseId" text NOT NULL,
    "CertificateCode" text NOT NULL,
    "QrValidationUrl" text NOT NULL,
    "IssueDate" timestamp with time zone NOT NULL,
    "StudentFullName" text NOT NULL,
    "StudentDni" text NOT NULL,
    "CourseTitle" text NOT NULL,
    "AcademicHours" integer NOT NULL,
    "FinalGrade" numeric(4,2) NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "RevokedAt" timestamp with time zone,
    "RevocationReason" text,
    CONSTRAINT "PK_DigitalCertificates" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DigitalCertificates_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_DigitalCertificates_Enrollments_EnrollmentId" FOREIGN KEY ("EnrollmentId") REFERENCES "Enrollments" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DigitalCertificates_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "ExamAttempts" (
    "Id" text NOT NULL,
    "QuestionsSnapshotJson" text NOT NULL,
    "EnrollmentId" text NOT NULL,
    "UserId" text NOT NULL,
    "CourseId" text NOT NULL,
    "AttemptNumber" integer NOT NULL,
    "Score" numeric(4,2) NOT NULL,
    "MaxScore" numeric(4,2) NOT NULL,
    "PassingScore" numeric(4,2) NOT NULL,
    "Approved" boolean NOT NULL,
    "Status" integer NOT NULL,
    "StartedAt" timestamp with time zone NOT NULL,
    "CompletedAt" timestamp with time zone,
    "NextAttemptAllowedAt" timestamp with time zone,
    "DurationSeconds" integer NOT NULL,
    "AnswersJson" text NOT NULL,
    CONSTRAINT "PK_ExamAttempts" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ExamAttempts_Enrollments_EnrollmentId" FOREIGN KEY ("EnrollmentId") REFERENCES "Enrollments" ("Id") ON DELETE CASCADE
);


CREATE TABLE "PaymentEvidences" (
    "Id" text NOT NULL,
    "OrderId" text NOT NULL,
    "BankName" text NOT NULL,
    "OperationNumber" text NOT NULL,
    "OperationDate" timestamp with time zone NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "AccountHolderName" text NOT NULL,
    "PaymentFingerprint" text NOT NULL,
    "VoucherFileHash" text NOT NULL,
    "VoucherSha256Hash" text NOT NULL,
    "VoucherFileUrl" text NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "RegisteredAt" timestamp with time zone NOT NULL,
    "ValidatedAt" timestamp with time zone,
    "ValidatedByUserId" text,
    "ReviewedAt" timestamp with time zone,
    "ReviewedByUserId" text,
    CONSTRAINT "PK_PaymentEvidences" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PaymentEvidences_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE
);


CREATE INDEX "IX_CourseDocument_ModuleId" ON "CourseDocument" ("ModuleId");


CREATE INDEX "IX_CourseModule_CourseId" ON "CourseModule" ("CourseId");


CREATE UNIQUE INDEX "IX_DigitalCertificates_CertificateCode" ON "DigitalCertificates" ("CertificateCode");


CREATE INDEX "IX_DigitalCertificates_CourseId" ON "DigitalCertificates" ("CourseId");


CREATE UNIQUE INDEX "IX_DigitalCertificates_EnrollmentId" ON "DigitalCertificates" ("EnrollmentId");


CREATE INDEX "IX_DigitalCertificates_UserId" ON "DigitalCertificates" ("UserId");


CREATE INDEX "IX_Enrollments_CourseId" ON "Enrollments" ("CourseId");


CREATE INDEX "IX_Enrollments_UserId" ON "Enrollments" ("UserId");


CREATE UNIQUE INDEX "IX_ExamAttempts_EnrollmentId_AttemptNumber" ON "ExamAttempts" ("EnrollmentId", "AttemptNumber");


CREATE UNIQUE INDEX "IX_Orders_OrderCode" ON "Orders" ("OrderCode");


CREATE INDEX "IX_Orders_UserId" ON "Orders" ("UserId");


CREATE UNIQUE INDEX "IX_PaymentEvidences_OrderId" ON "PaymentEvidences" ("OrderId");


CREATE INDEX "IX_PaymentEvidences_PaymentFingerprint" ON "PaymentEvidences" ("PaymentFingerprint");


CREATE INDEX "IX_PaymentEvidences_VoucherFileHash" ON "PaymentEvidences" ("VoucherFileHash");


CREATE INDEX "IX_Question_CourseId" ON "Question" ("CourseId");


CREATE INDEX "IX_SupportTickets_UserId" ON "SupportTickets" ("UserId");


CREATE INDEX "IX_Users_Dni" ON "Users" ("Dni");


CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");


