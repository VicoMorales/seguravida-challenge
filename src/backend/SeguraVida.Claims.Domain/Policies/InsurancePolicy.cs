using SeguraVida.Claims.Domain.Common;

namespace SeguraVida.Claims.Domain.Policies;

public sealed class InsurancePolicy
{
    private InsurancePolicy()
    {
        PolicyNumber = string.Empty;
    }

    public InsurancePolicy(
        Guid id,
        string policyNumber,
        string lineOfBusiness,
        DateOnly validFrom,
        DateOnly validTo,
        decimal insuredAmount)
        : this(
            id,
            policyNumber,
            Guid.Empty,
            ParseBranch(lineOfBusiness),
            premium: 0m,
            insuredAmount,
            validFrom,
            validTo,
            PolicyStatus.Active)
    {
    }

    public InsurancePolicy(
        Guid id,
        string policyNumber,
        Guid holderId,
        PolicyBranch branch,
        decimal premium,
        decimal insuredAmount,
        DateOnly startDate,
        DateOnly endDate,
        PolicyStatus status)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Policy id is required.");
        }

        if (string.IsNullOrWhiteSpace(policyNumber))
        {
            throw new DomainException("Policy number is required.");
        }

        if (endDate < startDate)
        {
            throw new DomainException("Policy validity range is invalid.");
        }

        if (premium < 0)
        {
            throw new DomainException("Policy premium cannot be negative.");
        }

        if (insuredAmount <= 0)
        {
            throw new DomainException("Insured amount must be greater than zero.");
        }

        Id = id;
        PolicyNumber = policyNumber.Trim();
        HolderId = holderId;
        Branch = branch;
        Premium = premium;
        ValidFrom = startDate;
        ValidTo = endDate;
        InsuredAmount = insuredAmount;
        Status = status;
    }

    public Guid Id { get; private set; }
    public string PolicyNumber { get; private set; }
    public Guid HolderId { get; private set; }
    public PolicyBranch Branch { get; private set; }
    public decimal Premium { get; private set; }
    public DateOnly ValidFrom { get; private set; }
    public DateOnly ValidTo { get; private set; }
    public decimal InsuredAmount { get; private set; }
    public PolicyStatus Status { get; private set; }
    public string LineOfBusiness => Branch.ToString().ToUpperInvariant();

    public bool IsActiveOn(DateOnly date)
    {
        return Status == PolicyStatus.Active && date >= ValidFrom && date <= ValidTo;
    }

    private static PolicyBranch ParseBranch(string lineOfBusiness)
    {
        if (Enum.TryParse<PolicyBranch>(lineOfBusiness, ignoreCase: true, out var branch))
        {
            return branch;
        }

        throw new DomainException("Line of business is invalid.");
    }
}
