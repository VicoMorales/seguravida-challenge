using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeguraVida.Claims.Domain.Claims;

namespace SeguraVida.Claims.Infrastructure.Persistence.Configurations;

public sealed class ClaimStatusHistoryConfiguration : IEntityTypeConfiguration<ClaimStatusHistory>
{
    public void Configure(EntityTypeBuilder<ClaimStatusHistory> builder)
    {
        builder.ToTable("CLAIM_STATUS_HISTORY");

        builder.HasKey(history => history.Id);
        builder.Property(history => history.Id).HasColumnName("history_id");
        builder.Property(history => history.ClaimId).HasColumnName("claim_id").IsRequired();
        builder.Property(history => history.FromStatus)
            .HasColumnName("previous_status")
            .HasMaxLength(30)
            .HasConversion(
                status => DbEnumConversion.ToDatabase(status),
                value => DbEnumConversion.ToClaimStatus(value))
            .IsRequired();
        builder.Property(history => history.ToStatus)
            .HasColumnName("new_status")
            .HasMaxLength(30)
            .HasConversion(
                status => DbEnumConversion.ToDatabase(status),
                value => DbEnumConversion.ToClaimStatus(value))
            .IsRequired();
        builder.Property(history => history.ChangedBy).HasColumnName("changed_by").HasMaxLength(120).IsRequired();
        builder.Property(history => history.ChangedAt).HasColumnName("changed_at").IsRequired();
        builder.Property(history => history.Notes).HasColumnName("reason").HasMaxLength(1000);

        builder.HasIndex(history => history.ClaimId).HasDatabaseName("IX_CLAIM_STATUS_HISTORY_CLAIM_ID");

        builder.HasOne<Claim>()
            .WithMany(claim => claim.StatusHistory)
            .HasForeignKey(history => history.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
