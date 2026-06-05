using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeguraVida.Claims.Domain.Policies;

namespace SeguraVida.Claims.Infrastructure.Persistence.Configurations;

public sealed class InsurancePolicyConfiguration : IEntityTypeConfiguration<InsurancePolicy>
{
    public void Configure(EntityTypeBuilder<InsurancePolicy> builder)
    {
        builder.ToTable("POLICY");

        builder.HasKey(policy => policy.Id);
        builder.Property(policy => policy.Id).HasColumnName("policy_id");
        builder.Property(policy => policy.PolicyNumber).HasColumnName("policy_number").HasMaxLength(30).IsRequired();
        builder.Property(policy => policy.HolderId).HasColumnName("holder_id").IsRequired();
        builder.Property(policy => policy.Branch)
            .HasColumnName("branch")
            .HasMaxLength(20)
            .HasConversion(
                branch => DbEnumConversion.ToDatabase(branch),
                value => DbEnumConversion.ToPolicyBranch(value))
            .IsRequired();
        builder.Property(policy => policy.Premium).HasColumnName("premium").HasPrecision(18, 2).IsRequired();
        builder.Property(policy => policy.InsuredAmount).HasColumnName("insured_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(policy => policy.ValidFrom).HasColumnName("start_date").IsRequired();
        builder.Property(policy => policy.ValidTo).HasColumnName("end_date").IsRequired();
        builder.Property(policy => policy.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(
                status => DbEnumConversion.ToDatabase(status),
                value => DbEnumConversion.ToPolicyStatus(value))
            .IsRequired();

        builder.Ignore(policy => policy.LineOfBusiness);

        builder.HasIndex(policy => policy.PolicyNumber)
            .HasDatabaseName("IX_POLICY_POLICY_NUMBER")
            .IsUnique();

        builder.HasOne<Domain.Parties.InsuredParty>()
            .WithMany()
            .HasForeignKey(policy => policy.HolderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
