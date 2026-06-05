using SeguraVida.Claims.Domain.Common;
using SeguraVida.Claims.Domain.Policies;

namespace SeguraVida.Claims.Domain.Claims;

public sealed class Claim
{
    private readonly List<ClaimStatusHistory> _statusHistory = [];

    private Claim()
    {
        ClaimNumber = string.Empty;
        PolicyNumber = string.Empty;
        LineOfBusiness = string.Empty;
        Description = string.Empty;
        NormalizedDescription = string.Empty;
        CreatedBy = string.Empty;
    }

    private Claim(
        Guid id,
        InsurancePolicy policy,
        string claimNumber,
        ClaimType type,
        DateOnly incidentDate,
        DateOnly reportedAt,
        string description,
        decimal claimedAmount,
        string reportedBy,
        DateTimeOffset createdAt)
    {
        Id = id;
        ClaimNumber = claimNumber;
        PolicyId = policy.Id;
        PolicyNumber = policy.PolicyNumber;
        LineOfBusiness = policy.LineOfBusiness;
        Type = type;
        IncidentDate = incidentDate;
        ReportedAt = reportedAt;
        Description = description.Trim();
        NormalizedDescription = ClaimTextNormalizer.Normalize(description);
        ClaimedAmount = claimedAmount;
        Status = ClaimStatus.Reported;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        CreatedBy = reportedBy.Trim();

        AddHistory(ClaimStatus.Reported, ClaimStatus.Reported, CreatedBy, createdAt, "Claim reported.");
    }

    public Guid Id { get; private set; }
    public string ClaimNumber { get; private set; }
    public Guid PolicyId { get; private set; }
    public string PolicyNumber { get; private set; }
    public string LineOfBusiness { get; private set; }
    public ClaimType Type { get; private set; }
    public DateOnly IncidentDate { get; private set; }
    public DateOnly ReportedAt { get; private set; }
    public string Description { get; private set; }
    public string NormalizedDescription { get; private set; }
    public decimal ClaimedAmount { get; private set; }
    public decimal? ApprovedAmount { get; private set; }
    public string? AdjustmentNotes { get; private set; }
    public ClaimStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; }
    public IReadOnlyCollection<ClaimStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    public static Claim Report(
        InsurancePolicy policy,
        DateOnly incidentDate,
        DateOnly reportedAt,
        string description,
        decimal claimedAmount,
        string reportedBy,
        DateTimeOffset createdAt)
    {
        return Report(
            policy,
            GenerateClaimNumber(createdAt),
            ClaimType.Accident,
            incidentDate,
            reportedAt,
            description,
            claimedAmount,
            reportedBy,
            createdAt);
    }

    public static Claim Report(
        InsurancePolicy policy,
        string claimNumber,
        ClaimType type,
        DateOnly incidentDate,
        DateOnly reportedAt,
        string description,
        decimal claimedAmount,
        string reportedBy,
        DateTimeOffset createdAt)
    {
        if (!policy.IsActiveOn(incidentDate))
        {
            throw new DomainException("The policy must be active on the incident date.");
        }

        if (incidentDate > reportedAt)
        {
            throw new DomainException("Incident date cannot be after report date.");
        }

        if (claimedAmount <= 0)
        {
            throw new DomainException("Claimed amount must be greater than zero.");
        }

        if (claimedAmount > policy.InsuredAmount)
        {
            throw new DomainException("Claimed amount cannot exceed policy insured amount.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Claim description is required.");
        }

        if (string.IsNullOrWhiteSpace(reportedBy))
        {
            throw new DomainException("Reporter is required.");
        }

        if (string.IsNullOrWhiteSpace(claimNumber))
        {
            throw new DomainException("Claim number is required.");
        }

        return new Claim(Guid.NewGuid(), policy, claimNumber.Trim(), type, incidentDate, reportedAt, description, claimedAmount, reportedBy, createdAt);
    }

    public void StartReview(string changedBy, DateTimeOffset changedAt, string? notes = null)
    {
        TransitionTo(ClaimStatus.UnderReview, changedBy, changedAt, notes);
    }

    public void Approve(decimal approvedAmount, string adjustmentNotes, string changedBy, DateTimeOffset changedAt)
    {
        if (approvedAmount <= 0)
        {
            throw new DomainException("Approved amount is required to approve a claim.");
        }

        if (approvedAmount > ClaimedAmount)
        {
            throw new DomainException("Approved amount cannot exceed claimed amount.");
        }

        if (string.IsNullOrWhiteSpace(adjustmentNotes))
        {
            throw new DomainException("Adjustment notes are required to approve a claim.");
        }

        ApprovedAmount = approvedAmount;
        AdjustmentNotes = adjustmentNotes.Trim();
        TransitionTo(ClaimStatus.Approved, changedBy, changedAt, "Claim approved after adjustment.");
    }

    public void Reject(string adjustmentNotes, string changedBy, DateTimeOffset changedAt)
    {
        if (string.IsNullOrWhiteSpace(adjustmentNotes))
        {
            throw new DomainException("Adjustment notes are required to reject a claim.");
        }

        AdjustmentNotes = adjustmentNotes.Trim();
        TransitionTo(ClaimStatus.Rejected, changedBy, changedAt, "Claim rejected after adjustment.");
    }

    public void MarkAsPaid(string changedBy, DateTimeOffset changedAt, string? notes = null)
    {
        TransitionTo(ClaimStatus.Paid, changedBy, changedAt, notes);
    }

    private void TransitionTo(ClaimStatus nextStatus, string changedBy, DateTimeOffset changedAt, string? notes)
    {
        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new DomainException("Status change user is required.");
        }

        if (!IsValidTransition(Status, nextStatus))
        {
            throw new DomainException($"Invalid claim status transition from {Status} to {nextStatus}.");
        }

        var previousStatus = Status;
        Status = nextStatus;
        UpdatedAt = changedAt;
        AddHistory(previousStatus, nextStatus, changedBy.Trim(), changedAt, notes);
    }

    private void AddHistory(
        ClaimStatus fromStatus,
        ClaimStatus toStatus,
        string changedBy,
        DateTimeOffset changedAt,
        string? notes)
    {
        _statusHistory.Add(new ClaimStatusHistory(Guid.NewGuid(), Id, fromStatus, toStatus, changedBy, changedAt, notes));
    }

    private static bool IsValidTransition(ClaimStatus current, ClaimStatus next)
    {
        return current switch
        {
            ClaimStatus.Reported => next == ClaimStatus.UnderReview,
            ClaimStatus.UnderReview => next is ClaimStatus.Approved or ClaimStatus.Rejected,
            ClaimStatus.Approved => next == ClaimStatus.Paid,
            ClaimStatus.Rejected or ClaimStatus.Paid => false,
            _ => false
        };
    }

    private static string GenerateClaimNumber(DateTimeOffset createdAt)
    {
        return $"CLM-{createdAt:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..30].ToUpperInvariant();
    }
}
