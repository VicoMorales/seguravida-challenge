using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeguraVida.Claims.Domain.Parties;

namespace SeguraVida.Claims.Infrastructure.Persistence.Configurations;

public sealed class InsuredPartyConfiguration : IEntityTypeConfiguration<InsuredParty>
{
    public void Configure(EntityTypeBuilder<InsuredParty> builder)
    {
        builder.ToTable("INSURED_PARTY");

        builder.HasKey(party => party.Id);
        builder.Property(party => party.Id).HasColumnName("party_id");
        builder.Property(party => party.DocumentId).HasColumnName("document_id").HasMaxLength(30).IsRequired();
        builder.Property(party => party.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        builder.Property(party => party.BirthDate).HasColumnName("birth_date").IsRequired();
        builder.Property(party => party.Email).HasColumnName("email").HasMaxLength(256).IsRequired();

        builder.HasIndex(party => party.DocumentId)
            .HasDatabaseName("IX_INSURED_PARTY_DOCUMENT_ID")
            .IsUnique();
    }
}
