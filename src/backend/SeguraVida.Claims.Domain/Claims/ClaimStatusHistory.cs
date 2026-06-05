namespace SeguraVida.Claims.Domain.Claims;

public sealed class ClaimStatusHistory
{
    private ClaimStatusHistory()
    {
        ChangedBy = string.Empty;
    }

    internal ClaimStatusHistory(
        Guid id,
        Guid claimId,
        ClaimStatus fromStatus,
        ClaimStatus toStatus,
        string changedBy,
        DateTimeOffset changedAt,
        string? notes)
    {
        Id = id;
        ClaimId = claimId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedBy = changedBy;
        ChangedAt = changedAt;
        Notes = notes;
    }

    public Guid Id { get; private set; }
    public Guid ClaimId { get; private set; }
    public ClaimStatus FromStatus { get; private set; }
    public ClaimStatus ToStatus { get; private set; }
    public string ChangedBy { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public string? Notes { get; private set; }
}
