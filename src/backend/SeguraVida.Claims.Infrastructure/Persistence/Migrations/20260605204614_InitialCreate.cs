using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeguraVida.Claims.Infrastructure.Persistence.Migrations;

    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "INSURED_PARTY",
                columns: table => new
                {
                    party_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    document_id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INSURED_PARTY", x => x.party_id);
                });

            migrationBuilder.CreateTable(
                name: "POLICY",
                columns: table => new
                {
                    policy_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    policy_number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    holder_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    branch = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    premium = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    insured_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POLICY", x => x.policy_id);
                    table.ForeignKey(
                        name: "FK_POLICY_INSURED_PARTY_holder_id",
                        column: x => x.holder_id,
                        principalTable: "INSURED_PARTY",
                        principalColumn: "party_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CLAIM",
                columns: table => new
                {
                    claim_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    claim_number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    policy_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    incident_date = table.Column<DateOnly>(type: "date", nullable: false),
                    reported_date = table.Column<DateOnly>(type: "date", nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    normalized_description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    claimed_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    approved_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    peritaje_notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLAIM", x => x.claim_id);
                    table.ForeignKey(
                        name: "FK_CLAIM_POLICY_policy_id",
                        column: x => x.policy_id,
                        principalTable: "POLICY",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CLAIM_STATUS_HISTORY",
                columns: table => new
                {
                    history_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    claim_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    previous_status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    new_status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLAIM_STATUS_HISTORY", x => x.history_id);
                    table.ForeignKey(
                        name: "FK_CLAIM_STATUS_HISTORY_CLAIM_claim_id",
                        column: x => x.claim_id,
                        principalTable: "CLAIM",
                        principalColumn: "claim_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CLAIM_CLAIM_NUMBER",
                table: "CLAIM",
                column: "claim_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CLAIM_INCIDENT_DATE",
                table: "CLAIM",
                column: "incident_date");

            migrationBuilder.CreateIndex(
                name: "IX_CLAIM_POLICY_ID",
                table: "CLAIM",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_CLAIM_REPORTED_DATE",
                table: "CLAIM",
                column: "reported_date");

            migrationBuilder.CreateIndex(
                name: "IX_CLAIM_STATUS",
                table: "CLAIM",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_CLAIM_STATUS_HISTORY_CLAIM_ID",
                table: "CLAIM_STATUS_HISTORY",
                column: "claim_id");

            migrationBuilder.CreateIndex(
                name: "IX_INSURED_PARTY_DOCUMENT_ID",
                table: "INSURED_PARTY",
                column: "document_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_POLICY_holder_id",
                table: "POLICY",
                column: "holder_id");

            migrationBuilder.CreateIndex(
                name: "IX_POLICY_POLICY_NUMBER",
                table: "POLICY",
                column: "policy_number",
                unique: true);

            migrationBuilder.Sql(
                """
                CREATE TABLE MOCK_USER (
                    user_id uniqueidentifier NOT NULL CONSTRAINT PK_MOCK_USER PRIMARY KEY,
                    email nvarchar(256) NOT NULL,
                    role nvarchar(30) NOT NULL,
                    display_name nvarchar(120) NOT NULL
                );

                CREATE UNIQUE INDEX IX_MOCK_USER_EMAIL ON MOCK_USER(email);
                """);

            migrationBuilder.Sql(
                """
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
                """);

            migrationBuilder.Sql(
                """
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
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.GetClaimsSummary;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS MOCK_USER;");

            migrationBuilder.DropTable(
                name: "CLAIM_STATUS_HISTORY");

            migrationBuilder.DropTable(
                name: "CLAIM");

            migrationBuilder.DropTable(
                name: "POLICY");

            migrationBuilder.DropTable(
                name: "INSURED_PARTY");
        }
}
