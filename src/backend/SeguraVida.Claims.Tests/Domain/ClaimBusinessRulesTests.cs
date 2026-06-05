using SeguraVida.Claims.Domain.Claims;
using SeguraVida.Claims.Domain.Common;
using SeguraVida.Claims.Domain.Policies;

namespace SeguraVida.Claims.Tests.Domain;

public sealed class ClaimBusinessRulesTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Report_rejects_incident_date_outside_policy_validity()
    {
        var policy = CreatePolicy(validFrom: new DateOnly(2026, 1, 1), validTo: new DateOnly(2026, 1, 31));

        var exception = Assert.Throws<DomainException>(() => Claim.Report(
            policy,
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 2),
            "Vehicle collision",
            1000m,
            "operator",
            Now));

        Assert.Contains("active", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Report_rejects_incident_date_after_report_date()
    {
        var policy = CreatePolicy();

        var exception = Assert.Throws<DomainException>(() => Claim.Report(
            policy,
            new DateOnly(2026, 6, 6),
            new DateOnly(2026, 6, 5),
            "Vehicle collision",
            1000m,
            "operator",
            Now));

        Assert.Contains("Incident date", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Report_rejects_claimed_amount_greater_than_policy_insured_amount()
    {
        var policy = CreatePolicy(insuredAmount: 5000m);

        var exception = Assert.Throws<DomainException>(() => Claim.Report(
            policy,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 5),
            "Vehicle collision",
            5000.01m,
            "operator",
            Now));

        Assert.Contains("insured amount", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Transition_rejects_invalid_status_flow()
    {
        var claim = CreateReportedClaim();

        var exception = Assert.Throws<DomainException>(() => claim.MarkAsPaid("adjuster", Now));

        Assert.Contains("Invalid claim status transition", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Approve_requires_approved_amount()
    {
        var claim = CreateUnderReviewClaim();

        var exception = Assert.Throws<DomainException>(() => claim.Approve(0m, "Valid adjustment notes", "adjuster", Now));

        Assert.Contains("Approved amount", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Approve_requires_adjustment_notes()
    {
        var claim = CreateUnderReviewClaim();

        var exception = Assert.Throws<DomainException>(() => claim.Approve(1000m, " ", "adjuster", Now));

        Assert.Contains("Adjustment notes", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Every_status_change_registers_history()
    {
        var claim = CreateReportedClaim();

        claim.StartReview("adjuster", Now.AddMinutes(5));
        claim.Approve(800m, "Damage verified", "adjuster", Now.AddMinutes(10));
        claim.MarkAsPaid("operator", Now.AddMinutes(15));

        Assert.Equal(ClaimStatus.Paid, claim.Status);
        Assert.Equal(4, claim.StatusHistory.Count);
        Assert.Contains(claim.StatusHistory, history => history.FromStatus == ClaimStatus.Approved && history.ToStatus == ClaimStatus.Paid);
    }

    [Fact]
    public void Status_flow_allows_rejection_from_under_review()
    {
        var claim = CreateUnderReviewClaim();

        claim.Reject("Coverage exclusion applies", "adjuster", Now);

        Assert.Equal(ClaimStatus.Rejected, claim.Status);
        Assert.Equal(3, claim.StatusHistory.Count);
    }

    [Fact]
    public void Duplicate_policy_detects_same_policy_date_and_similar_description()
    {
        var policy = CreatePolicy();
        var existing = Claim.Report(
            policy,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 5),
            "Choque lateral del vehiculo asegurado",
            1000m,
            "operator",
            Now);

        var isDuplicate = DuplicateClaimPolicy.IsDuplicate(
            policy.Id,
            new DateOnly(2026, 6, 1),
            "choque lateral del vehículo asegurado",
            [existing]);

        Assert.True(isDuplicate);
    }

    private static Claim CreateReportedClaim()
    {
        return Claim.Report(
            CreatePolicy(),
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 5),
            "Vehicle collision",
            1000m,
            "operator",
            Now);
    }

    private static Claim CreateUnderReviewClaim()
    {
        var claim = CreateReportedClaim();
        claim.StartReview("adjuster", Now);

        return claim;
    }

    private static InsurancePolicy CreatePolicy(
        DateOnly? validFrom = null,
        DateOnly? validTo = null,
        decimal insuredAmount = 10000m)
    {
        return new InsurancePolicy(
            Guid.NewGuid(),
            "POL-001",
            "AUTO",
            validFrom ?? new DateOnly(2026, 1, 1),
            validTo ?? new DateOnly(2026, 12, 31),
            insuredAmount);
    }
}
