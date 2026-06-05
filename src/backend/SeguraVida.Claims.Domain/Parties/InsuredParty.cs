using SeguraVida.Claims.Domain.Common;

namespace SeguraVida.Claims.Domain.Parties;

public sealed class InsuredParty
{
    private InsuredParty()
    {
        DocumentId = string.Empty;
        FullName = string.Empty;
        Email = string.Empty;
    }

    public InsuredParty(Guid id, string documentId, string fullName, DateOnly birthDate, string email)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Party id is required.");
        }

        if (string.IsNullOrWhiteSpace(documentId))
        {
            throw new DomainException("Document id is required.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Email is required.");
        }

        Id = id;
        DocumentId = documentId.Trim();
        FullName = fullName.Trim();
        BirthDate = birthDate;
        Email = email.Trim();
    }

    public Guid Id { get; private set; }
    public string DocumentId { get; private set; }
    public string FullName { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public string Email { get; private set; }
}
