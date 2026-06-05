using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeguraVida.Claims.Domain.Claims;
using SeguraVida.Claims.Domain.Policies;

namespace SeguraVida.Claims.Infrastructure.Persistence.Configurations;

public sealed class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("CLAIM");

        builder.HasKey(claim => claim.Id);
        builder.Property(claim => claim.Id).HasColumnName("claim_id");
        builder.Property(claim => claim.ClaimNumber).HasColumnName("claim_number").HasMaxLength(30).IsRequired();
        builder.Property(claim => claim.PolicyId).HasColumnName("policy_id").IsRequired();
        builder.Property(claim => claim.Type)
            .HasColumnName("type")
            .HasMaxLength(30)
            .HasConversion(
                type => DbEnumConversion.ToDatabase(type),
                value => DbEnumConversion.ToClaimType(value))
            .IsRequired();
        builder.Property(claim => claim.Description).HasColumnName("description").HasMaxLength(1000).IsRequired();
        builder.Property(claim => claim.NormalizedDescription).HasColumnName("normalized_description").HasMaxLength(1000).IsRequired();
        builder.Property(claim => claim.IncidentDate).HasColumnName("incident_date").IsRequired();
        builder.Property(claim => claim.ReportedAt).HasColumnName("reported_date").IsRequired();
        builder.Property(claim => claim.ClaimedAmount).HasColumnName("claimed_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(claim => claim.ApprovedAmount).HasColumnName("approved_amount").HasPrecision(18, 2);
        builder.Property(claim => claim.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .HasConversion(
                status => DbEnumConversion.ToDatabase(status),
                value => DbEnumConversion.ToClaimStatus(value))
            .IsRequired();
        builder.Property(claim => claim.AdjustmentNotes).HasColumnName("peritaje_notes").HasMaxLength(1000);
        builder.Property(claim => claim.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(claim => claim.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(claim => claim.CreatedBy).HasColumnName("created_by").HasMaxLength(120).IsRequired();

        builder.Ignore(claim => claim.PolicyNumber);
        builder.Ignore(claim => claim.LineOfBusiness);

        builder.HasIndex(claim => claim.ClaimNumber)
            .HasDatabaseName("IX_CLAIM_CLAIM_NUMBER")
            .IsUnique();
        builder.HasIndex(claim => claim.Status).HasDatabaseName("IX_CLAIM_STATUS");
        builder.HasIndex(claim => claim.IncidentDate).HasDatabaseName("IX_CLAIM_INCIDENT_DATE");
        builder.HasIndex(claim => claim.ReportedAt).HasDatabaseName("IX_CLAIM_REPORTED_DATE");
        builder.HasIndex(claim => claim.PolicyId).HasDatabaseName("IX_CLAIM_POLICY_ID");

        builder.HasOne<InsurancePolicy>()
            .WithMany()
            .HasForeignKey(claim => claim.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(claim => claim.StatusHistory)
            .WithOne()
            .HasForeignKey(history => history.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(claim => claim.StatusHistory)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
