IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE TABLE [INSURED_PARTY] (
        [party_id] uniqueidentifier NOT NULL,
        [document_id] nvarchar(30) NOT NULL,
        [full_name] nvarchar(200) NOT NULL,
        [birth_date] date NOT NULL,
        [email] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_INSURED_PARTY] PRIMARY KEY ([party_id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE TABLE [POLICY] (
        [policy_id] uniqueidentifier NOT NULL,
        [policy_number] nvarchar(30) NOT NULL,
        [holder_id] uniqueidentifier NOT NULL,
        [branch] nvarchar(20) NOT NULL,
        [premium] decimal(18,2) NOT NULL,
        [start_date] date NOT NULL,
        [end_date] date NOT NULL,
        [insured_amount] decimal(18,2) NOT NULL,
        [status] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_POLICY] PRIMARY KEY ([policy_id]),
        CONSTRAINT [FK_POLICY_INSURED_PARTY_holder_id] FOREIGN KEY ([holder_id]) REFERENCES [INSURED_PARTY] ([party_id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE TABLE [CLAIM] (
        [claim_id] uniqueidentifier NOT NULL,
        [claim_number] nvarchar(30) NOT NULL,
        [policy_id] uniqueidentifier NOT NULL,
        [type] nvarchar(30) NOT NULL,
        [incident_date] date NOT NULL,
        [reported_date] date NOT NULL,
        [description] nvarchar(1000) NOT NULL,
        [normalized_description] nvarchar(1000) NOT NULL,
        [claimed_amount] decimal(18,2) NOT NULL,
        [approved_amount] decimal(18,2) NULL,
        [peritaje_notes] nvarchar(1000) NULL,
        [status] nvarchar(30) NOT NULL,
        [created_at] datetimeoffset NOT NULL,
        [updated_at] datetimeoffset NOT NULL,
        [created_by] nvarchar(120) NOT NULL,
        CONSTRAINT [PK_CLAIM] PRIMARY KEY ([claim_id]),
        CONSTRAINT [FK_CLAIM_POLICY_policy_id] FOREIGN KEY ([policy_id]) REFERENCES [POLICY] ([policy_id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE TABLE [CLAIM_STATUS_HISTORY] (
        [history_id] uniqueidentifier NOT NULL,
        [claim_id] uniqueidentifier NOT NULL,
        [previous_status] nvarchar(30) NOT NULL,
        [new_status] nvarchar(30) NOT NULL,
        [changed_by] nvarchar(120) NOT NULL,
        [changed_at] datetimeoffset NOT NULL,
        [reason] nvarchar(1000) NULL,
        CONSTRAINT [PK_CLAIM_STATUS_HISTORY] PRIMARY KEY ([history_id]),
        CONSTRAINT [FK_CLAIM_STATUS_HISTORY_CLAIM_claim_id] FOREIGN KEY ([claim_id]) REFERENCES [CLAIM] ([claim_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CLAIM_CLAIM_NUMBER] ON [CLAIM] ([claim_number]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CLAIM_INCIDENT_DATE] ON [CLAIM] ([incident_date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CLAIM_POLICY_ID] ON [CLAIM] ([policy_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CLAIM_REPORTED_DATE] ON [CLAIM] ([reported_date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CLAIM_STATUS] ON [CLAIM] ([status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CLAIM_STATUS_HISTORY_CLAIM_ID] ON [CLAIM_STATUS_HISTORY] ([claim_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_INSURED_PARTY_DOCUMENT_ID] ON [INSURED_PARTY] ([document_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_POLICY_holder_id] ON [POLICY] ([holder_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_POLICY_POLICY_NUMBER] ON [POLICY] ([policy_number]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    CREATE TABLE MOCK_USER (
        user_id uniqueidentifier NOT NULL CONSTRAINT PK_MOCK_USER PRIMARY KEY,
        email nvarchar(256) NOT NULL,
        role nvarchar(30) NOT NULL,
        display_name nvarchar(120) NOT NULL
    );

    CREATE UNIQUE INDEX IX_MOCK_USER_EMAIL ON MOCK_USER(email);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    INSERT INTO INSURED_PARTY (party_id, document_id, full_name, birth_date, email) VALUES
    ('11111111-1111-1111-1111-111111111111', 'DNI100001', 'Carlos Mendoza', '1985-03-14', 'carlos.mendoza@example.com'),
    ('11111111-1111-1111-1111-111111111112', 'DNI100002', 'Lucia Fernandez', '1990-07-22', 'lucia.fernandez@example.com'),
    ('11111111-1111-1111-1111-111111111113', 'DNI100003', 'Mario Salazar', '1978-11-05', 'mario.salazar@example.com'),
    ('11111111-1111-1111-1111-111111111114', 'DNI100004', 'Ana Torres', '1994-01-30', 'ana.torres@example.com'),
    ('11111111-1111-1111-1111-111111111115', 'DNI100005', 'Rosa Castillo', '1982-09-18', 'rosa.castillo@example.com');

    INSERT INTO POLICY (policy_id, policy_number, holder_id, branch, premium, insured_amount, start_date, end_date, status) VALUES
    ('22222222-2222-2222-2222-222222222221', 'POL-AUTO-001', '11111111-1111-1111-1111-111111111111', 'AUTO', 1200.00, 30000.00, '2026-01-01', '2026-12-31', 'ACTIVE'),
    ('22222222-2222-2222-2222-222222222222', 'POL-LIFE-001', '11111111-1111-1111-1111-111111111112', 'LIFE', 2100.00, 100000.00, '2026-01-01', '2026-12-31', 'ACTIVE'),
    ('22222222-2222-2222-2222-222222222223', 'POL-HEALTH-001', '11111111-1111-1111-1111-111111111113', 'HEALTH', 1800.00, 50000.00, '2026-01-01', '2026-12-31', 'ACTIVE'),
    ('22222222-2222-2222-2222-222222222224', 'POL-HOME-001', '11111111-1111-1111-1111-111111111114', 'HOME', 900.00, 80000.00, '2026-01-01', '2026-12-31', 'ACTIVE'),
    ('22222222-2222-2222-2222-222222222225', 'POL-AUTO-002', '11111111-1111-1111-1111-111111111115', 'AUTO', 1350.00, 35000.00, '2026-01-01', '2026-12-31', 'ACTIVE'),
    ('22222222-2222-2222-2222-222222222226', 'POL-LIFE-002', '11111111-1111-1111-1111-111111111111', 'LIFE', 2500.00, 150000.00, '2025-01-01', '2025-12-31', 'EXPIRED'),
    ('22222222-2222-2222-2222-222222222227', 'POL-HEALTH-002', '11111111-1111-1111-1111-111111111112', 'HEALTH', 1750.00, 45000.00, '2026-02-01', '2027-01-31', 'ACTIVE'),
    ('22222222-2222-2222-2222-222222222228', 'POL-HOME-002', '11111111-1111-1111-1111-111111111113', 'HOME', 980.00, 90000.00, '2026-01-01', '2026-10-31', 'CANCELLED');

    INSERT INTO CLAIM (claim_id, claim_number, policy_id, type, incident_date, reported_date, description, normalized_description, claimed_amount, approved_amount, peritaje_notes, status, created_at, updated_at, created_by) VALUES
    ('33333333-3333-3333-3333-333333333331', 'CLM-20260601-001', '22222222-2222-2222-2222-222222222221', 'ACCIDENT', '2026-05-28', '2026-06-01', 'Rear-end vehicle collision', 'REAR-END VEHICLE COLLISION', 4200.00, 3900.00, 'Damage verified and payable.', 'PAID', '2026-06-01T10:00:00+00:00', '2026-06-04T10:00:00+00:00', 'operator@seguravida.com'),
    ('33333333-3333-3333-3333-333333333332', 'CLM-20260601-002', '22222222-2222-2222-2222-222222222222', 'DEATH', '2026-05-15', '2026-06-01', 'Life claim documentation submitted', 'LIFE CLAIM DOCUMENTATION SUBMITTED', 75000.00, NULL, 'Beneficiary documentation incomplete.', 'REJECTED', '2026-06-01T11:00:00+00:00', '2026-06-03T11:00:00+00:00', 'operator@seguravida.com'),
    ('33333333-3333-3333-3333-333333333333', 'CLM-20260602-001', '22222222-2222-2222-2222-222222222223', 'MEDICAL', '2026-05-30', '2026-06-02', 'Emergency medical expenses', 'EMERGENCY MEDICAL EXPENSES', 8200.00, 8000.00, 'Medical invoices accepted.', 'APPROVED', '2026-06-02T09:30:00+00:00', '2026-06-04T09:30:00+00:00', 'operator@seguravida.com'),
    ('33333333-3333-3333-3333-333333333334', 'CLM-20260602-002', '22222222-2222-2222-2222-222222222224', 'PROPERTY_DAMAGE', '2026-05-29', '2026-06-02', 'Kitchen fire damage', 'KITCHEN FIRE DAMAGE', 15000.00, NULL, NULL, 'UNDER_REVIEW', '2026-06-02T12:00:00+00:00', '2026-06-03T12:00:00+00:00', 'operator@seguravida.com'),
    ('33333333-3333-3333-3333-333333333335', 'CLM-20260603-001', '22222222-2222-2222-2222-222222222225', 'THEFT', '2026-06-01', '2026-06-03', 'Vehicle accessories theft', 'VEHICLE ACCESSORIES THEFT', 2500.00, NULL, NULL, 'REPORTED', '2026-06-03T08:00:00+00:00', '2026-06-03T08:00:00+00:00', 'operator@seguravida.com'),
    ('33333333-3333-3333-3333-333333333336', 'CLM-20260603-002', '22222222-2222-2222-2222-222222222221', 'ACCIDENT', '2026-06-02', '2026-06-03', 'Windshield damage', 'WINDSHIELD DAMAGE', 900.00, 850.00, 'Approved minor repair.', 'PAID', '2026-06-03T09:00:00+00:00', '2026-06-05T09:00:00+00:00', 'operator@seguravida.com'),
    ('33333333-3333-3333-3333-333333333337', 'CLM-20260604-001', '22222222-2222-2222-2222-222222222227', 'MEDICAL', '2026-06-01', '2026-06-04', 'Outpatient procedure reimbursement', 'OUTPATIENT PROCEDURE REIMBURSEMENT', 3200.00, 3000.00, 'Procedure covered by plan.', 'APPROVED', '2026-06-04T10:30:00+00:00', '2026-06-05T10:30:00+00:00', 'operator@seguravida.com'),
    ('33333333-3333-3333-3333-333333333338', 'CLM-20260604-002', '22222222-2222-2222-2222-222222222224', 'PROPERTY_DAMAGE', '2026-06-02', '2026-06-04', 'Water leak in apartment', 'WATER LEAK IN APARTMENT', 6000.00, NULL, 'Excluded maintenance issue.', 'REJECTED', '2026-06-04T11:30:00+00:00', '2026-06-05T11:30:00+00:00', 'operator@seguravida.com'),
    ('33333333-3333-3333-3333-333333333339', 'CLM-20260605-001', '22222222-2222-2222-2222-222222222222', 'DEATH', '2026-06-01', '2026-06-05', 'Life claim initial notice', 'LIFE CLAIM INITIAL NOTICE', 90000.00, NULL, NULL, 'UNDER_REVIEW', '2026-06-05T08:45:00+00:00', '2026-06-05T09:45:00+00:00', 'operator@seguravida.com'),
    ('33333333-3333-3333-3333-33333333333a', 'CLM-20260605-002', '22222222-2222-2222-2222-222222222225', 'ACCIDENT', '2026-06-04', '2026-06-05', 'Parking lot collision', 'PARKING LOT COLLISION', 1800.00, NULL, NULL, 'REPORTED', '2026-06-05T10:00:00+00:00', '2026-06-05T10:00:00+00:00', 'operator@seguravida.com');

    INSERT INTO CLAIM_STATUS_HISTORY (history_id, claim_id, previous_status, new_status, changed_by, changed_at, reason) VALUES
    ('44444444-4444-4444-4444-444444444401', '33333333-3333-3333-3333-333333333331', 'REPORTED', 'REPORTED', 'operator@seguravida.com', '2026-06-01T10:00:00+00:00', 'Claim reported.'),
    ('44444444-4444-4444-4444-444444444402', '33333333-3333-3333-3333-333333333331', 'REPORTED', 'UNDER_REVIEW', 'adjuster@seguravida.com', '2026-06-02T10:00:00+00:00', 'Review started.'),
    ('44444444-4444-4444-4444-444444444403', '33333333-3333-3333-3333-333333333331', 'UNDER_REVIEW', 'APPROVED', 'adjuster@seguravida.com', '2026-06-03T10:00:00+00:00', 'Claim approved.'),
    ('44444444-4444-4444-4444-444444444404', '33333333-3333-3333-3333-333333333331', 'APPROVED', 'PAID', 'adjuster@seguravida.com', '2026-06-04T10:00:00+00:00', 'Payment completed.'),
    ('44444444-4444-4444-4444-444444444405', '33333333-3333-3333-3333-333333333332', 'REPORTED', 'REPORTED', 'operator@seguravida.com', '2026-06-01T11:00:00+00:00', 'Claim reported.'),
    ('44444444-4444-4444-4444-444444444406', '33333333-3333-3333-3333-333333333332', 'REPORTED', 'UNDER_REVIEW', 'adjuster@seguravida.com', '2026-06-02T11:00:00+00:00', 'Review started.'),
    ('44444444-4444-4444-4444-444444444407', '33333333-3333-3333-3333-333333333332', 'UNDER_REVIEW', 'REJECTED', 'adjuster@seguravida.com', '2026-06-03T11:00:00+00:00', 'Rejected after review.'),
    ('44444444-4444-4444-4444-444444444408', '33333333-3333-3333-3333-333333333333', 'REPORTED', 'REPORTED', 'operator@seguravida.com', '2026-06-02T09:30:00+00:00', 'Claim reported.'),
    ('44444444-4444-4444-4444-444444444409', '33333333-3333-3333-3333-333333333333', 'REPORTED', 'UNDER_REVIEW', 'adjuster@seguravida.com', '2026-06-03T09:30:00+00:00', 'Review started.'),
    ('44444444-4444-4444-4444-44444444440a', '33333333-3333-3333-3333-333333333333', 'UNDER_REVIEW', 'APPROVED', 'adjuster@seguravida.com', '2026-06-04T09:30:00+00:00', 'Claim approved.'),
    ('44444444-4444-4444-4444-44444444440b', '33333333-3333-3333-3333-333333333334', 'REPORTED', 'REPORTED', 'operator@seguravida.com', '2026-06-02T12:00:00+00:00', 'Claim reported.'),
    ('44444444-4444-4444-4444-44444444440c', '33333333-3333-3333-3333-333333333334', 'REPORTED', 'UNDER_REVIEW', 'adjuster@seguravida.com', '2026-06-03T12:00:00+00:00', 'Review started.'),
    ('44444444-4444-4444-4444-44444444440d', '33333333-3333-3333-3333-333333333335', 'REPORTED', 'REPORTED', 'operator@seguravida.com', '2026-06-03T08:00:00+00:00', 'Claim reported.'),
    ('44444444-4444-4444-4444-44444444440e', '33333333-3333-3333-3333-333333333336', 'REPORTED', 'REPORTED', 'operator@seguravida.com', '2026-06-03T09:00:00+00:00', 'Claim reported.'),
    ('44444444-4444-4444-4444-44444444440f', '33333333-3333-3333-3333-333333333336', 'REPORTED', 'UNDER_REVIEW', 'adjuster@seguravida.com', '2026-06-04T09:00:00+00:00', 'Review started.'),
    ('44444444-4444-4444-4444-444444444410', '33333333-3333-3333-3333-333333333336', 'UNDER_REVIEW', 'APPROVED', 'adjuster@seguravida.com', '2026-06-05T08:00:00+00:00', 'Claim approved.'),
    ('44444444-4444-4444-4444-444444444411', '33333333-3333-3333-3333-333333333336', 'APPROVED', 'PAID', 'adjuster@seguravida.com', '2026-06-05T09:00:00+00:00', 'Payment completed.'),
    ('44444444-4444-4444-4444-444444444412', '33333333-3333-3333-3333-333333333337', 'REPORTED', 'REPORTED', 'operator@seguravida.com', '2026-06-04T10:30:00+00:00', 'Claim reported.'),
    ('44444444-4444-4444-4444-444444444413', '33333333-3333-3333-3333-333333333337', 'REPORTED', 'UNDER_REVIEW', 'adjuster@seguravida.com', '2026-06-05T09:30:00+00:00', 'Review started.'),
    ('44444444-4444-4444-4444-444444444414', '33333333-3333-3333-3333-333333333337', 'UNDER_REVIEW', 'APPROVED', 'adjuster@seguravida.com', '2026-06-05T10:30:00+00:00', 'Claim approved.'),
    ('44444444-4444-4444-4444-444444444415', '33333333-3333-3333-3333-333333333338', 'REPORTED', 'REPORTED', 'operator@seguravida.com', '2026-06-04T11:30:00+00:00', 'Claim reported.'),
    ('44444444-4444-4444-4444-444444444416', '33333333-3333-3333-3333-333333333338', 'REPORTED', 'UNDER_REVIEW', 'adjuster@seguravida.com', '2026-06-05T10:30:00+00:00', 'Review started.'),
    ('44444444-4444-4444-4444-444444444417', '33333333-3333-3333-3333-333333333338', 'UNDER_REVIEW', 'REJECTED', 'adjuster@seguravida.com', '2026-06-05T11:30:00+00:00', 'Rejected after review.'),
    ('44444444-4444-4444-4444-444444444418', '33333333-3333-3333-3333-333333333339', 'REPORTED', 'REPORTED', 'operator@seguravida.com', '2026-06-05T08:45:00+00:00', 'Claim reported.'),
    ('44444444-4444-4444-4444-444444444419', '33333333-3333-3333-3333-333333333339', 'REPORTED', 'UNDER_REVIEW', 'adjuster@seguravida.com', '2026-06-05T09:45:00+00:00', 'Review started.'),
    ('44444444-4444-4444-4444-44444444441a', '33333333-3333-3333-3333-33333333333a', 'REPORTED', 'REPORTED', 'operator@seguravida.com', '2026-06-05T10:00:00+00:00', 'Claim reported.');

    INSERT INTO MOCK_USER (user_id, email, role, display_name) VALUES
    ('55555555-5555-5555-5555-555555555551', 'operator@seguravida.com', 'OPERATOR', 'Mock Operator'),
    ('55555555-5555-5555-5555-555555555552', 'adjuster@seguravida.com', 'ADJUSTER', 'Mock Adjuster'),
    ('55555555-5555-5555-5555-555555555553', 'auditor@seguravida.com', 'AUDITOR', 'Mock Auditor');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    EXEC('
    CREATE PROCEDURE dbo.GetClaimsSummary
        @FromDate date = NULL,
        @ToDate date = NULL
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT
            p.branch AS Branch,
            c.status AS Status,
            COUNT(1) AS TotalClaims,
            CAST(SUM(CASE WHEN c.status = ''PAID'' THEN ISNULL(c.approved_amount, 0) ELSE 0 END) AS decimal(18,2)) AS PaidAmount
        FROM CLAIM c
        INNER JOIN POLICY p ON p.policy_id = c.policy_id
        WHERE (@FromDate IS NULL OR c.reported_date >= @FromDate)
          AND (@ToDate IS NULL OR c.reported_date <= @ToDate)
        GROUP BY p.branch, c.status
        ORDER BY p.branch, c.status;
    END
    ');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605204614_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260605204614_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

